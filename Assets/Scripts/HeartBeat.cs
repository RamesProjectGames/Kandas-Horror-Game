using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class HeartBeat : MonoBehaviour
{
    private const float MinBpm = 40f;
    private const float MaxBpm = 200f;
    private const float DefaultBpm = 70f;
    private const float MinPitch = 0.75f;
    private const float MaxPitch = 1.5f;

    [Header("FMOD Event")]
    [SerializeField] private EventReference bpmSound;
    [SerializeField] private string bpmParameterName = "BPM";

    [Header("Playback")]
    [SerializeField] [Range(MinBpm, MaxBpm)] private float bpm = DefaultBpm;
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private bool loop = true;
    [SerializeField] [Range(0f, 1f)] private float volume = 1f;

    [Header("Enemy Response")]
    [SerializeField] private string enemyTag = "Enemy";
    [SerializeField] [Range(1f, 100f)] private float enemyDetectionRange = 25f;
    [SerializeField] [Range(0.1f, 100f)] private float startThreshold = 5f;
    [SerializeField] [Range(1f, 100f)] private float bpmChangeSpeed = 20f;
    [SerializeField] [Range(0f, 1f)] private float maxVolume = 1f;
    [SerializeField] [Range(0.1f, 10f)] private float volumeChangeSpeed = 1f;

    private EventInstance bpmEvent;
    private bool isPlaying;

    private void Start()
    {
        if (playOnStart && GetNearestEnemyDistance() <= startThreshold)
        {
            StartBpmSound();
        }
    }

    private void Update()
    {
        float nearestDistance = GetNearestEnemyDistance();
        bool shouldPlay = nearestDistance != float.MaxValue && nearestDistance <= startThreshold;

        if (!shouldPlay)
        {
            if (isPlaying)
            {
                StopBpmSound();
            }

            return;
        }

        if (!isPlaying)
        {
            StartBpmSound();
            return;
        }

        if (!bpmEvent.isValid())
            return;

        float targetBpm = GetBpmFromNearestEnemy();
        float targetVolume = GetVolumeFromNearestEnemy();

        bpm = Mathf.MoveTowards(bpm, targetBpm, bpmChangeSpeed * Time.deltaTime);
        bpm = Mathf.Clamp(bpm, MinBpm, MaxBpm);

        volume = Mathf.MoveTowards(volume, targetVolume, volumeChangeSpeed * Time.deltaTime);
        volume = Mathf.Clamp01(volume);

        float pitch = GetPitchFromBpm(bpm);

        bpmEvent.setParameterByName(bpmParameterName, bpm);
        bpmEvent.setVolume(volume);
        bpmEvent.setPitch(pitch);
    }

    public void StartBpmSound()
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogError("AudioManager is missing from the scene.");
            return;
        }

        if (bpmSound.IsNull)
        {
            Debug.LogWarning("No FMOD event assigned to TestRPMSound.");
            return;
        }

        if (bpmEvent.isValid())
        {
            bpmEvent.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            bpmEvent.release();
        }

        bpm = Mathf.Clamp(bpm, MinBpm, MaxBpm);
        float pitch = GetPitchFromBpm(bpm);

        bpmEvent = AudioManager.Instance.CreateInstance(bpmSound);
        bpmEvent.set3DAttributes(RuntimeUtils.To3DAttributes(transform.position));
        bpmEvent.setParameterByName(bpmParameterName, bpm);
        bpmEvent.setVolume(volume);
        bpmEvent.setPitch(pitch);
        bpmEvent.start();
        isPlaying = true;
    }

    public void StopBpmSound()
    {
        if (!bpmEvent.isValid())
        {
            isPlaying = false;
            volume = 0f;
            return;
        }

        bpmEvent.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        bpmEvent.release();
        bpmEvent.clearHandle();
        isPlaying = false;
        volume = 0f;
    }

    private float GetNearestEnemyDistance()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
        if (enemies.Length == 0)
        {
            return float.MaxValue;
        }

        float nearestDistance = float.MaxValue;
        foreach (GameObject enemy in enemies)
        {
            if (enemy == null)
                continue;

            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
            }
        }

        return nearestDistance;
    }

    private float GetBpmFromNearestEnemy()
    {
        float nearestDistance = GetNearestEnemyDistance();
        if (nearestDistance == float.MaxValue || nearestDistance > enemyDetectionRange)
        {
            return DefaultBpm;
        }

        float normalizedDistance = Mathf.InverseLerp(enemyDetectionRange, 0f, nearestDistance);
        return Mathf.Lerp(DefaultBpm, MaxBpm, normalizedDistance);
    }

    private float GetVolumeFromNearestEnemy()
    {
        float nearestDistance = GetNearestEnemyDistance();
        if (nearestDistance == float.MaxValue || nearestDistance > enemyDetectionRange)
        {
            return 0f;
        }

        float normalizedDistance = Mathf.InverseLerp(enemyDetectionRange, 0f, nearestDistance);
        return Mathf.Lerp(0f, maxVolume, normalizedDistance);
    }

    private float GetPitchFromBpm(float currentBpm)
    {
        float normalized = Mathf.InverseLerp(MinBpm, MaxBpm, currentBpm);
        return Mathf.Lerp(MinPitch, MaxPitch, normalized);
    }

    private void OnDisable()
    {
        StopBpmSound();
    }
}
