using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class EnemySoundDetection : MonoBehaviour
{
    [Header("Sound Detection Settings")]
    [SerializeField] private float minSoundThreshold = 0.1f;
    [SerializeField] private float maxSoundThreshold = 1.0f;
    [SerializeField] private float minDetectionRadius = 1f;
    [SerializeField, Min(0f)] private float maxDetectionRadius = 50f;
    [SerializeField] private float soundFalloffDistance = 30f;
    [SerializeField, Min(0)] private float soundSensitivityMultiplier = 1.0f; // Global sensitivity multiplier for all sound sources
    [SerializeField] private MicrophoneManager microphoneManager; // Reference to microphone manager
    
    [Header("Audio Source Detection")]
    [SerializeField] private float audioSourceCheckInterval = 0.1f;
    [SerializeField] private float maxSoundDetectionDistance = 100f; // Max distance for all sound sources
    [SerializeField] private LayerMask audioSourceLayerMask = -1; // Default to all layers
    
    [Header("Loudness Calculation (PC-Quality)")]
    [SerializeField] private int frequencyBands = 256; // Higher = more accurate spectrum (PC-quality)
    [SerializeField] private float loudnessSmoothing = 0.1f; // Smoothing factor for loudness calculation
    [SerializeField] private float peakFrequencyWeight = 1.5f; // Weight for peak frequencies
    [SerializeField] private AnimationCurve loudnessResponseCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Visual Feedback")]
    [SerializeField] private bool showDetectionSphere = true;
    [SerializeField] private Color sphereColor = new Color(1f, 0.5f, 0f, 0.3f);
    
    private float currentSoundLevel = 0f;
    private float smoothedSoundLevel = 0f;
    private float detectionSphereRadius = 0f;
    private float audioSourceCheckTimer = 0f;
    private float closestAudioSourceDistance = float.MaxValue;
    
    private float[] frequencySpectrumData;
    private float[] smoothedFrequencyData;
    
    private Collider[] detectedColliders = new Collider[100];
    private HashSet<AudioSource> activeAudioSources = new HashSet<AudioSource>();

    void Start()
    {
        // Initialize microphone manager reference if not assigned
        if (microphoneManager == null)
        {
            microphoneManager = FindAnyObjectByType<MicrophoneManager>(FindObjectsInactive.Include);
            if (microphoneManager == null)
            {
                Debug.LogWarning("MicrophoneManager not found in scene!");
            }
        }

        // Create a sphere collider for visualization if it doesn't exist
        if (GetComponent<SphereCollider>() == null)
        {
            SphereCollider sphereCollider = gameObject.AddComponent<SphereCollider>();
            sphereCollider.isTrigger = true;
        }
    }

    void Update()
    {
        // Update sound detection with smoothing
        currentSoundLevel = CalculateCurrentSoundLevel();
        smoothedSoundLevel = Mathf.Lerp(smoothedSoundLevel, currentSoundLevel, loudnessSmoothing);
        
        // Calculate proximity factor (closer sounds = bigger sphere)
        float proximityFactor = CalculateProximityFactor();
        
        // Update detection sphere radius based on sound level and proximity
        // Higher sound + closer distance = bigger sphere
        float combinedSoundFactor = Mathf.Clamp01(smoothedSoundLevel * (1f + proximityFactor));
        detectionSphereRadius = Mathf.Lerp(minDetectionRadius, maxDetectionRadius, combinedSoundFactor);
        
        // Update sphere collider
        SphereCollider sphereCollider = GetComponent<SphereCollider>();
        if (sphereCollider != null)
        {
            sphereCollider.radius = detectionSphereRadius;
        }

        // Check for audio sources at intervals
        audioSourceCheckTimer -= Time.deltaTime;
        if (audioSourceCheckTimer <= 0f)
        {
            DetectAudioSources();
            audioSourceCheckTimer = audioSourceCheckInterval;
        }

        // Detect enemies in the sound detection sphere
        DetectEnemiesInSphere(smoothedSoundLevel);
    }

    void OnDrawGizmos()
    {
        if (!showDetectionSphere) return;

        // Draw detection sphere (always visible, even when not selected)
        Gizmos.color = sphereColor;
        Gizmos.DrawSphere(transform.position, detectionSphereRadius);

        // Draw audio source detection range (blue wireframe)
        Gizmos.color = new Color(0f, 0.5f, 1f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, maxSoundDetectionDistance);

        // Draw microphone detection range (cyan wireframe)
        Gizmos.color = new Color(0f, 1f, 1f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, maxSoundDetectionDistance);
    }

    void OnDrawGizmosSelected()
    {
        if (!showDetectionSphere) return;

        // Draw detection sphere in editor with full opacity when selected
        Gizmos.color = sphereColor;
        Gizmos.DrawSphere(transform.position, detectionSphereRadius);

        // Draw min and max radius reference spheres
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        Gizmos.DrawSphere(transform.position, minDetectionRadius);
        
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawSphere(transform.position, maxDetectionRadius);

        // Draw audio source detection range (blue wireframe)
        Gizmos.color = new Color(0f, 0.5f, 1f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, maxSoundDetectionDistance);

        // Draw microphone detection range (cyan wireframe)
        Gizmos.color = new Color(0f, 1f, 1f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, maxSoundDetectionDistance);

        // Draw radius indicator line with color matching sphere
        Gizmos.color = new Color(sphereColor.r, sphereColor.g, sphereColor.b, 1f);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.right * detectionSphereRadius);

        // Draw text info (requires using handles in editor context)
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * detectionSphereRadius * 1.1f, 
            $"Sound Radius: {detectionSphereRadius:F2}");
        UnityEditor.Handles.Label(transform.position + Vector3.up * (detectionSphereRadius + 5f),
            $"Max Sound Detection: {maxSoundDetectionDistance:F2}");
        #endif
    }

    /// <summary>
    /// Calculates the current sound level from both in-game audio sources and microphone input
    /// </summary>
    private float CalculateCurrentSoundLevel()
    {
        float soundLevel = 0f;

        // Get sound from in-game audio sources
        soundLevel += GetAudioSourceSoundLevel();

        // Get sound from microphone via MicrophoneManager
        if (microphoneManager != null && microphoneManager.IsMicrophoneActive())
        {
            soundLevel += microphoneManager.GetMicrophoneLoudness();
        }

        // Clamp and normalize the sound level
        return Mathf.Clamp01(soundLevel);
    }

    /// <summary>
    /// Detects and sums up sound levels from nearby audio sources with PC-quality loudness calculation
    /// </summary>
    private float GetAudioSourceSoundLevel()
    {
        float totalSoundLevel = 0f;

        foreach (AudioSource audioSource in activeAudioSources)
        {
            if (audioSource == null || !audioSource.isPlaying)
            {
                continue;
            }

            float distance = Vector3.Distance(transform.position, audioSource.transform.position);
            // Debug.Log($"AudioSource '{audioSource.gameObject.name}' at distance {distance:F2}");

            // Only consider audio sources within max distance
            if (distance > maxSoundDetectionDistance)
            {
                continue;
            }

            // Check if audio source is on the allowed layer mask
            if (!IsLayerInMask(audioSource.gameObject.layer, audioSourceLayerMask))
            {
                continue;
            }

            // Check if audio source is within detection sphere
            // if (distance > detectionSphereRadius)
            // {
            //     continue;
            // }

            // Calculate loudness with PC-quality spectrum analysis
            float loudness = CalculateAudioSourceLoudness(audioSource);

            // Apply global sound sensitivity multiplier
            loudness *= soundSensitivityMultiplier;

            // Calculate falloff based on distance
            float falloff = Mathf.Pow(1f - Mathf.Clamp01(distance / soundFalloffDistance), 1.5f);

            // Add to total sound level
            totalSoundLevel += loudness * falloff;
        }

        return Mathf.Clamp01(loudnessResponseCurve.Evaluate(totalSoundLevel));
    }

    /// <summary>
    /// Calculates loudness from an audio source by analyzing the actual audio clip samples
    /// </summary>
    private float CalculateAudioSourceLoudness(AudioSource audioSource)
    {
        if (audioSource == null || !audioSource.isPlaying || audioSource.clip == null)
        {
            return 0f;
        }

        AudioClip clip = audioSource.clip;
        
        // Get RMS loudness from the actual audio clip data
        float rmsLoudness = GetAudioClipRMSLoudness(clip, audioSource.timeSamples);

        // Get spectrum data for frequency weighting as secondary analysis
        if (frequencySpectrumData == null || frequencySpectrumData.Length != frequencyBands)
        {
            frequencySpectrumData = new float[frequencyBands];
            smoothedFrequencyData = new float[frequencyBands];
        }

        AudioListener.GetSpectrumData(frequencySpectrumData, 0, FFTWindow.Blackman);

        // Calculate frequency-weighted emphasis
        float frequencyEmphasis = 0f;
        for (int i = 0; i < frequencySpectrumData.Length; i++)
        {
            float magnitude = frequencySpectrumData[i];
            smoothedFrequencyData[i] = Mathf.Lerp(smoothedFrequencyData[i], magnitude, 0.3f);
            
            // Weight frequencies (bass emphasis for footsteps)
            float frequencyWeight = 1f;
            if (i < frequencyBands * 0.3f) // Bass range (0-30%)
                frequencyWeight = 1.3f;
            else if (i < frequencyBands * 0.6f) // Mid range (30-60%)
                frequencyWeight = 1.0f;
            else // High range (60-100%)
                frequencyWeight = 0.8f;

            frequencyEmphasis += smoothedFrequencyData[i] * frequencyWeight;
        }

        frequencyEmphasis /= frequencyBands;

        // Combine RMS loudness with frequency emphasis
        float combinedLoudness = Mathf.Lerp(rmsLoudness, frequencyEmphasis, 0.3f);

        // Apply audio source volume and spatial blend
        float spatialBlend = audioSource.spatialBlend;
        float finalLoudness = combinedLoudness * audioSource.volume * (1f - spatialBlend + 0.5f);

        return Mathf.Clamp01(finalLoudness);
    }

    /// <summary>
    /// Calculates RMS (Root Mean Square) loudness from an audio clip
    /// </summary>
    private float GetAudioClipRMSLoudness(AudioClip clip, int currentSamplePosition)
    {
        if (clip == null)
            return 0f;

        int sampleRate = clip.frequency;
        int channels = clip.channels;
        
        // Sample window size (0.05 seconds for responsive audio)
        int sampleWindowSize = Mathf.Max(256, sampleRate / 20);
        
        // Get samples around current playback position
        float[] samples = new float[sampleWindowSize * channels];
        int startSample = Mathf.Max(0, currentSamplePosition - sampleWindowSize / 2);
        
        clip.GetData(samples, startSample);

        // Calculate RMS loudness
        float sumSquares = 0f;
        for (int i = 0; i < samples.Length; i++)
        {
            sumSquares += samples[i] * samples[i];
        }

        float rms = Mathf.Sqrt(sumSquares / samples.Length);
        
        // Convert to normalized 0-1 scale using a response curve
        // Peak audio is typically around 0.9 amplitude
        float normalizedRMS = Mathf.Clamp01(rms / 0.5f);

        return normalizedRMS;
    }

    /// <summary>
    /// Gets sound level from microphone via MicrophoneManager
    /// </summary>
    private float GetMicrophoneSoundLevel()
    {
        if (microphoneManager != null && microphoneManager.IsMicrophoneActive())
        {
            float micLoudness = microphoneManager.GetMicrophoneLoudness();
            // Apply global sound sensitivity multiplier
            return micLoudness * soundSensitivityMultiplier;
        }
        return 0f;
    }

    /// <summary>
    /// Detects all audio sources nearby and tracks them (filtered by layer mask)
    /// </summary>
    private void DetectAudioSources()
    {
        activeAudioSources.Clear();
        closestAudioSourceDistance = float.MaxValue;

        // Find all audio sources in the scene
        List<AudioSource> allAudioSources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None).ToList();

        foreach (AudioSource audioSource in allAudioSources)
        {
            if (audioSource == null || !audioSource.isPlaying)
            {
                continue;
            }

            // Check layer mask
            if (!IsLayerInMask(audioSource.gameObject.layer, audioSourceLayerMask))
            {
                continue;
            }

            float distance = Vector3.Distance(transform.position, audioSource.transform.position);
            
            // Track closest audio source distance
            if (distance < closestAudioSourceDistance)
            {
                closestAudioSourceDistance = distance;
            }
            
            if (distance <= maxSoundDetectionDistance)
            {
                activeAudioSources.Add(audioSource);
            }
        }

        // Reset if no audio sources found
        if (activeAudioSources.Count == 0)
        {
            closestAudioSourceDistance = float.MaxValue;
        }
    }

    /// <summary>
    /// Calculates proximity factor based on closest audio source distance (closer = higher factor)
    /// </summary>
    private float CalculateProximityFactor()
    {
        if (closestAudioSourceDistance == float.MaxValue || closestAudioSourceDistance >= maxSoundDetectionDistance)
        {
            return 0f;
        }

        // Normalize distance: 0 at closest point, 1 at max distance
        float normalizedDistance = closestAudioSourceDistance / maxSoundDetectionDistance;
        
        // Invert so closer = higher factor (0.5 to 1.0 range for gentle boost)
        float proximityFactor = (1f - normalizedDistance) * 0.5f;

        return proximityFactor;
    }

    /// <summary>
    /// Helper method to check if a layer is in a layer mask
    /// </summary>
    private bool IsLayerInMask(int layer, LayerMask mask)
    {
        return ((mask.value & (1 << layer)) > 0);
    }

    /// <summary>
    /// Detects enemies within the detection sphere and triggers events
    /// </summary>
    private void DetectEnemiesInSphere(float soundLevel)
    {
        // Use sphere overlap to find colliders in range
        int detectedCount = Physics.OverlapSphereNonAlloc(
            transform.position,
            detectionSphereRadius,
            detectedColliders,
            LayerMask.GetMask("Enemy") // Make sure enemies are on "Enemy" layer
        );

        // Process detected enemies
        for (int i = 0; i < detectedCount; i++)
        {
            Collider collider = detectedColliders[i];
            IEnemySoundReactive enemy = collider.GetComponent<IEnemySoundReactive>();

            if (enemy != null)
            {
                // Notify enemy of detected sound with current and smoothed sound level
                enemy.OnSoundDetected(transform.position, soundLevel, detectionSphereRadius);
            }
        }
    }

    /// <summary>
    /// Simple gizmo drawing helper for filled spheres (no longer needed with Gizmos.DrawSphere)
    /// </summary>
    private void DrawFilledSphere(Vector3 position, float radius, Color color)
    {
        // This method is kept for reference but Gizmos.DrawSphere is now used instead
    }

    /// <summary>
    /// Gets the current detection sphere radius
    /// </summary>
    public float GetDetectionRadius()
    {
        return detectionSphereRadius;
    }

    /// <summary>
    /// Gets the current sound level (0-1)
    /// </summary>
    public float GetCurrentSoundLevel()
    {
        return currentSoundLevel;
    }

    /// <summary>
    /// Gets the smoothed sound level (0-1)
    /// </summary>
    public float GetSmoothedSoundLevel()
    {
        return smoothedSoundLevel;
    }

    void OnDestroy()
    {
        // Stop microphone recording via MicrophoneManager when object is destroyed
        // MicrophoneManager handles microphone cleanup automatically
    }
}

/// <summary>
/// Interface for enemies to react to detected sound
/// </summary>
public interface IEnemySoundReactive
{
    void OnSoundDetected(Vector3 soundSource, float soundLevel, float detectionRadius);
}
