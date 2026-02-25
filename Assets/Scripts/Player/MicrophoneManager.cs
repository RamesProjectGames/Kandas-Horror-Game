using UnityEngine;

public class MicrophoneManager : MonoBehaviour
{
    public static MicrophoneManager Instance { get; private set; }
    [Header("Microphone Settings")]
    [SerializeField] private bool enableMicrophone = true;
    
    [Header("Loudness Calculation (PC-Quality)")]
    [SerializeField] private int frequencyBands = 64;
    [SerializeField] private float loudnessSmoothing = 0.1f;
    [SerializeField] private float peakFrequencyWeight = 1.5f;
    
    [Header("Audio Source")]
    [SerializeField] private AudioSource microphoneSource;
    
    private AudioClip microphoneClip;
    
    private bool isInitialized = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Auto-initialize AudioSource if not assigned
        if (microphoneSource == null)
        {
            microphoneSource = GetComponent<AudioSource>();
            if (microphoneSource == null)
            {
                microphoneSource = gameObject.AddComponent<AudioSource>();
            }
        }

        if (enableMicrophone)
        {
            InitializeMicrophone();
        }
    }

    void Update()
    {
        
    }
    public void GetMicrophoneDevices()
    {
        foreach (var device in Microphone.devices)
        {
            Debug.Log($"Available microphone: {device}");
        }
    }
    private void InitializeMicrophone()
    {
        GetMicrophoneDevices();

        SettingManager.Instance.settings.AudioInputDeviceName = Microphone.devices.Length > 0 ? Microphone.devices[0] : null;

        if (string.IsNullOrEmpty(SettingManager.Instance.settings.AudioInputDeviceName))
        {
            Debug.LogWarning("No microphone device found!");
            enableMicrophone = false;
            return;
        }

        // Start recording from microphone
        microphoneClip = Microphone.Start(SettingManager.Instance.settings.AudioInputDeviceName, true, 20, AudioSettings.outputSampleRate);

        microphoneSource.clip = microphoneClip;
        microphoneSource.loop = true;

        if (microphoneClip == null)
        {
            Debug.LogError("Failed to start microphone recording!");
            enableMicrophone = false;
        }
        else
        {
            isInitialized = true;
            Debug.Log($"Microphone initialized: {SettingManager.Instance.settings.AudioInputDeviceName}");
        }
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
        return GetLoudnessFromAudioClip(Microphone.GetPosition(SettingManager.Instance.settings.AudioInputDeviceName), microphoneClip);
    }

    /// <summary>
    /// Checks if microphone is initialized and recording
    /// </summary>
    public bool IsMicrophoneActive()
    {
        return isInitialized && Microphone.IsRecording(SettingManager.Instance.settings.AudioInputDeviceName);
    }

    void OnDestroy()
    {
        // Stop microphone recording when object is destroyed
        if (isInitialized && Microphone.IsRecording(SettingManager.Instance.settings.AudioInputDeviceName))
        {
            Microphone.End(SettingManager.Instance.settings.AudioInputDeviceName);
        }
    }
}
