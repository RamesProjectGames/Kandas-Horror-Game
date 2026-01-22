using UnityEngine;

public class MicrophoneManager : MonoBehaviour
{
    [Header("Microphone Settings")]
    [SerializeField] private bool enableMicrophone = true;
    [SerializeField] private int microphoneSampleRate = 44100;
    [SerializeField] private float microphoneVolumeSensitivity = 1.0f;
    
    [Header("Loudness Calculation (PC-Quality)")]
    [SerializeField] private int frequencyBands = 256;
    [SerializeField] private float loudnessSmoothing = 0.1f;
    [SerializeField] private float peakFrequencyWeight = 1.5f;
    [SerializeField] private AnimationCurve loudnessResponseCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Audio Source")]
    [SerializeField] private AudioSource microphoneSource;
    
    private AudioClip microphoneClip;
    private float[] frequencySpectrumData;
    private float[] smoothedFrequencyData;
    private float previousLoudness = 0f;
    private float currentMicrophoneLoudness = 0f;
    
    private bool isInitialized = false;

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
        if (isInitialized && Microphone.IsRecording(null))
        {
            currentMicrophoneLoudness = CalculateMicrophoneLoudness();
        }
    }

    /// <summary>
    /// Initializes microphone input for sound detection
    /// </summary>
    private void InitializeMicrophone()
    {
        string microphoneName = Microphone.devices.Length > 0 ? Microphone.devices[0] : null;

        if (string.IsNullOrEmpty(microphoneName))
        {
            Debug.LogWarning("No microphone device found!");
            enableMicrophone = false;
            return;
        }

        // Start recording from microphone
        microphoneClip = Microphone.Start(microphoneName, true, 1, microphoneSampleRate);

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
            Debug.Log($"Microphone initialized: {microphoneName}");
        }
    }

    /// <summary>
    /// Calculates sound level from microphone input with PC-quality spectrum analysis
    /// </summary>
    private float CalculateMicrophoneLoudness()
    {
        if (microphoneClip == null)
        {
            return 0f;
        }

        // Initialize spectrum data if needed
        if (frequencySpectrumData == null || frequencySpectrumData.Length != frequencyBands)
        {
            frequencySpectrumData = new float[frequencyBands];
            smoothedFrequencyData = new float[frequencyBands];
        }

        // Get spectrum data from audio listener with high-quality FFT
        AudioListener.GetSpectrumData(frequencySpectrumData, 0, FFTWindow.Blackman);

        // Calculate weighted loudness from microphone input
        float totalLoudness = 0f;
        float peakMagnitude = 0f;

        for (int i = 0; i < frequencySpectrumData.Length; i++)
        {
            float magnitude = frequencySpectrumData[i];
            
            // Smooth frequency data over time
            smoothedFrequencyData[i] = Mathf.Lerp(smoothedFrequencyData[i], magnitude, 0.3f);
            
            // Apply frequency weighting (human hearing perception)
            float frequencyWeight = GetFrequencyWeight(i);
            totalLoudness += smoothedFrequencyData[i] * frequencyWeight;

            // Track peak magnitude
            if (smoothedFrequencyData[i] > peakMagnitude)
            {
                peakMagnitude = smoothedFrequencyData[i];
            }
        }

        // Normalize loudness
        float averageLoudness = totalLoudness / frequencyBands;
        float peakLoudness = peakMagnitude * peakFrequencyWeight;
        float microphoneLoudness = Mathf.Max(averageLoudness, peakLoudness * 0.5f);

        // Apply sensitivity and response curve
        float soundLevel = microphoneLoudness * microphoneVolumeSensitivity;
        soundLevel = loudnessResponseCurve.Evaluate(soundLevel);
        
        // Smooth loudness over time
        previousLoudness = Mathf.Lerp(previousLoudness, soundLevel, loudnessSmoothing);

        return Mathf.Clamp01(previousLoudness);
    }

    /// <summary>
    /// Gets frequency weighting based on human hearing perception (A-weighting inspired)
    /// </summary>
    private float GetFrequencyWeight(int frequencyBandIndex)
    {
        float normalizedIndex = (float)frequencyBandIndex / frequencyBands;
        
        // A-weighting inspired curve: emphasis on speech/footstep frequencies (500Hz-4kHz)
        if (normalizedIndex < 0.1f) // Sub-bass
            return 0.3f;
        else if (normalizedIndex < 0.3f) // Bass (typical footstep range)
            return 1.3f;
        else if (normalizedIndex < 0.6f) // Mid-range (speech range)
            return 1.5f;
        else if (normalizedIndex < 0.8f) // Upper-mid
            return 1.2f;
        else // Treble
            return 0.9f;
    }

    /// <summary>
    /// Gets the current microphone loudness (0-1)
    /// </summary>
    public float GetMicrophoneLoudness()
    {
        return currentMicrophoneLoudness;
    }

    /// <summary>
    /// Checks if microphone is initialized and recording
    /// </summary>
    public bool IsMicrophoneActive()
    {
        return isInitialized && Microphone.IsRecording(null);
    }

    void OnDestroy()
    {
        // Stop microphone recording when object is destroyed
        if (isInitialized && Microphone.IsRecording(null))
        {
            Microphone.End(null);
        }
    }
}
