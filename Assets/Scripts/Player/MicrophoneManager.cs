using Dialogue;
using FMOD;
using FMOD.Studio;
using FMODUnity;
using System;
using UnityEngine;

public class MicrophoneManager : MonoBehaviour
{
    public static MicrophoneManager Instance { get; private set; }
    [Header("Microphone Settings")]
    [SerializeField] private bool enableMicrophone = true;
    [SerializeField] private string recordingDeviceName = "";
    
    [Header("Loudness Calculation (PC-Quality)")]
    [SerializeField] private int frequencyBands = 64;
    [SerializeField] private float loudnessSmoothing = 0.1f;
    [SerializeField] private float peakFrequencyWeight = 1.5f;
    
    private FMOD.System coreSystem;
    private FMOD.Sound recordingSound;
    private bool isRecording = false;
    private float[] audioBuffer;
    private int activeRecordingDevice = -1;
    
    private bool isInitialized = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Get FMOD core system
        coreSystem = RuntimeManager.CoreSystem;
        
        if (coreSystem.handle == System.IntPtr.Zero)
        {
            UnityEngine.Debug.LogError("FMOD Core System not initialized!");
            enableMicrophone = false;
            return;
        }

        // Initialize audio buffer
        audioBuffer = new float[frequencyBands];

        if (enableMicrophone)
        {
            InitializeMicrophone();
        }
    }

    void Update()
    {
        if (Application.isPlaying && (SettingManager.Instance.isPaused || DialogueSystem.Instance.isRunningConvo))
            return;
        if (isRecording && coreSystem.handle != System.IntPtr.Zero)
        {
            coreSystem.update();
        }
    }

    public void GetMicrophoneDevices()
    {
        int numDrivers = 0;
        int numConnected = 0;
        coreSystem.getRecordNumDrivers(out numDrivers, out numConnected);
        
        UnityEngine.Debug.Log($"Available microphones: {numDrivers}");
        
        for (int i = 0; i < numDrivers; i++)
        {
            string name;
            Guid guid;
            int rate;
            SPEAKERMODE mode;
            int channels;
            DRIVER_STATE state;

            coreSystem.getRecordDriverInfo(
                i,
                out name,
                256,
                out guid,
                out rate,
                out mode,
                out channels,
                out state
            );
            UnityEngine.Debug.Log($"  [{i}] {name}");
        }
    }
    public int GetCurrentMicrophoneIndex()
    {
        int numDrivers = 0;
        int numConnected = 0;
        int microphoneIndex = -1;
        coreSystem.getRecordNumDrivers(out numDrivers, out numConnected);

        for (int i = 0; i < numDrivers; i++)
        {
            string name;
            Guid guid;
            int rate;
            SPEAKERMODE mode;
            int channels;
            DRIVER_STATE state;

            coreSystem.getRecordDriverInfo(
                i,
                out name,
                256,
                out guid,
                out rate,
                out mode,
                out channels,
                out state
            );
            if(name == recordingDeviceName) microphoneIndex = i;
        }
        return microphoneIndex;
    }

    private void InitializeMicrophone()
    {        
        recordingDeviceName = SettingManager.Instance.settings.AudioInputDeviceName;

        // Get default recording device (-1)
        int recordingDevice = GetCurrentMicrophoneIndex();
        int numDrivers = 0;
        int numConnected = 0;
        coreSystem.getRecordNumDrivers(out numDrivers, out numConnected);
        
        if (numDrivers <= 0 || recordingDevice == -1)
        {
            UnityEngine.Debug.LogWarning("No microphone device found!");
            enableMicrophone = false;
            return;
        }

        activeRecordingDevice = recordingDevice;

        // Create sound object for recording
        FMOD.CREATESOUNDEXINFO exInfo = new FMOD.CREATESOUNDEXINFO();
        exInfo.cbsize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(FMOD.CREATESOUNDEXINFO));
        exInfo.numchannels = 1;
        exInfo.format = FMOD.SOUND_FORMAT.PCM16;
        exInfo.defaultfrequency = 48000;
        exInfo.length = (uint)(exInfo.defaultfrequency * sizeof(short) * exInfo.numchannels * 20); // 20 seconds buffer

        FMOD.RESULT result = coreSystem.createSound((string)null, FMOD.MODE.OPENUSER, ref exInfo, out recordingSound);
        
        if (result != FMOD.RESULT.OK)
        {
            UnityEngine.Debug.LogError($"Failed to create recording sound: {result}");
            enableMicrophone = false;
            return;
        }

        // Start recording on the resolved device index
        result = coreSystem.recordStart(activeRecordingDevice, recordingSound, true);
        
        if (result != FMOD.RESULT.OK)
        {
            UnityEngine.Debug.LogError($"Failed to start recording: {result}");
            enableMicrophone = false;
            return;
        }

        isRecording = true;
        isInitialized = true;
        UnityEngine.Debug.Log($"Microphone initialized with FMOD on device [{activeRecordingDevice}]: {recordingDeviceName}");
        
        // if (SettingManager.Instance != null)
        // {
        //     SettingManager.Instance.settings.AudioInputDeviceName = "FMOD_Default_Microphone";
        // }
    }
    public float GetLoudnessFromAudioClip(int clipPosition, AudioClip audioClip)
    {
        int startPosition = clipPosition - frequencyBands;
        if (startPosition < 0) startPosition = 0;
        float[] samples = new float[frequencyBands];
        audioClip.GetData(samples, startPosition);
        float totalLoudness = 0f;
        for (int i = 0; i < samples.Length; i++)
        {
            totalLoudness += Mathf.Abs(samples[i]);
        }
        return totalLoudness / samples.Length;
    }

    public float GetMicrophoneLoudness()
    {
        if (!isRecording || recordingSound.handle == System.IntPtr.Zero)
        {
            return 0f;
        }

        try
        {
            // Get the current write position in the recording buffer
            uint recordPos = 0;
            coreSystem.getRecordPosition(activeRecordingDevice, out recordPos);

            // Calculate the read start: step back by frequencyBands samples from the write head
            uint soundLength = 0;
            recordingSound.getLength(out soundLength, FMOD.TIMEUNIT.PCMBYTES);
            uint readBytesNeeded = (uint)(frequencyBands * sizeof(short));
            uint writeBytePos = recordPos * sizeof(short);

            uint lockOffset;
            if (writeBytePos >= readBytesNeeded)
                lockOffset = writeBytePos - readBytesNeeded;
            else
                lockOffset = soundLength - (readBytesNeeded - writeBytePos);

            // Read audio data from the recording sound
            System.IntPtr ptr1, ptr2;
            uint len1, len2;
            recordingSound.@lock(lockOffset, readBytesNeeded, out ptr1, out ptr2, out len1, out len2);

            // Copy data to managed array
            if (len1 > 0)
            {
                short[] shortSamples = new short[frequencyBands];
                System.Runtime.InteropServices.Marshal.Copy(ptr1, shortSamples, 0, (int)len1 / sizeof(short));

                // Convert to float and calculate loudness
                float sum = 0f;

                for (int i = 0; i < shortSamples.Length; i++)
                {
                    float sample = shortSamples[i] / 32768f;
                    sum += sample * sample;
                }

                float rms = Mathf.Sqrt(sum / shortSamples.Length);

                // Convert to dB
                float db = 20f * Mathf.Log10(rms);

                // Clamp human voice range
                db = Mathf.Clamp(db, -60f, 0f);

                // Normalize
                float normalized = Mathf.InverseLerp(-50f, -20f, db) * SettingManager.Instance.settings.MicrophoneSensitivity;

                return normalized;
            }

            recordingSound.unlock(ptr1, ptr2, len1, len2);
            return 0f;
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError($"Error getting microphone loudness: {e}");
            return 0f;
        }
    }

    /// <summary>
    /// Stops the current recording session and restarts it with the device stored in settings.
    /// Call this whenever the user changes the audio input device.
    /// </summary>
    public void RestartRecording()
    {
        if (isRecording && coreSystem.handle != System.IntPtr.Zero && activeRecordingDevice >= 0)
        {
            coreSystem.recordStop(activeRecordingDevice);
            isRecording = false;
        }

        if (recordingSound.handle != System.IntPtr.Zero)
        {
            recordingSound.release();
            recordingSound = default;
        }

        isInitialized = false;
        activeRecordingDevice = -1;
        enableMicrophone = true;
        InitializeMicrophone();
    }

    /// <summary>
    /// Checks if microphone is initialized and recording
    /// </summary>
    public bool IsMicrophoneActive()
    {
        return isInitialized && isRecording;
    }

    void OnDestroy()
    {
        // Stop microphone recording when object is destroyed
        if (isRecording && coreSystem.handle != System.IntPtr.Zero && activeRecordingDevice >= 0)
        {
            coreSystem.recordStop(activeRecordingDevice);
            isRecording = false;
        }

        // Release the recording sound
        if (recordingSound.handle != System.IntPtr.Zero)
        {
            recordingSound.release();
        }
    }
}
