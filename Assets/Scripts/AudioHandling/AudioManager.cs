using FMOD.Studio;
using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private List<EventInstance> eventInstances;
    private Dictionary<string, List<EventInstance>> eventInstancesBySound;
    public static AudioManager Instance;
    private Bus masterVolumeBus, bgmBus, sfxBus, voiceBus, ambienceBus;

    private void Awake()
    {
        if (Instance == null)
        {
            transform.SetParent(null);
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }

        eventInstances = new List<EventInstance>();
        eventInstancesBySound = new Dictionary<string, List<EventInstance>>();

        masterVolumeBus = RuntimeManager.GetBus("bus:/");
        bgmBus = RuntimeManager.GetBus("bus:/BGM");
        sfxBus = RuntimeManager.GetBus("bus:/SFX");
        voiceBus = RuntimeManager.GetBus("bus:/Voice");
        ambienceBus = RuntimeManager.GetBus("bus:/Ambience");
    }

    void Update()
    {
        RemoveStoppedInstances();
    }

    public void UpdateVolumeSettings()
    {
        masterVolumeBus.setVolume(SettingManager.Instance.settings.MasterVolume);
        bgmBus.setVolume(SettingManager.Instance.settings.MusicVolume);
        ambienceBus.setVolume(SettingManager.Instance.settings.MusicVolume);
        sfxBus.setVolume(SettingManager.Instance.settings.SoundEffectVolume);
        voiceBus.setVolume(SettingManager.Instance.settings.MobVolume);
    }

    public void PlayOneShot3D(EventReference sound, bool dup, float volume, float pitch, Vector3 position = default, float volumeIncreaseAmount = 0f, float pitchIncreaseAmount = 0f, float increaseDuration = 0f)
    {
        var instance = CreateInstance(sound, dup);
        instance.setVolume(volume);
        instance.setPitch(pitch);
        instance.set3DAttributes(RuntimeUtils.To3DAttributes(position));
        instance.start();
        instance.release();

        if (volumeIncreaseAmount != 0f)
        {
            StartCoroutine(AnimateVolumeChange(instance, volume, volumeIncreaseAmount, increaseDuration));
        }

        if (pitchIncreaseAmount != 0f)
        {
            StartCoroutine(AnimatePitchChange(instance, pitch, pitchIncreaseAmount, increaseDuration));
        }
    }

    public void PlayOneShot2D(EventReference sound, float volume, float pitch, Vector3 position = default, float volumeIncreaseAmount = 0f, float pitchIncreaseAmount = 0f, float increaseDuration = 0f)
    {
        var instance = CreateInstance(sound, true);
        instance.setVolume(volume);
        instance.setPitch(pitch);
        instance.start();
        instance.release();

        if (volumeIncreaseAmount != 0f)
        {
            StartCoroutine(AnimateVolumeChange(instance, volume, volumeIncreaseAmount, increaseDuration));
        }

        if (pitchIncreaseAmount != 0f)
        {
            StartCoroutine(AnimatePitchChange(instance, pitch, pitchIncreaseAmount, increaseDuration));
        }
    }

    private IEnumerator AnimateVolumeChange(EventInstance instance, float startVolume, float increaseAmount, float duration)
    {
        if (!instance.isValid())
        {
            yield break;
        }

        float targetVolume = Mathf.Clamp01(startVolume + increaseAmount);

        if (duration <= 0f)
        {
            instance.setVolume(targetVolume);
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (!instance.isValid())
            {
                yield break;
            }

            float t = Mathf.Clamp01(elapsed / duration);
            instance.setVolume(Mathf.Lerp(startVolume, targetVolume, t));
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (instance.isValid())
        {
            instance.setVolume(targetVolume);
        }
    }

    private IEnumerator AnimatePitchChange(EventInstance instance, float startPitch, float increaseAmount, float duration)
    {
        if (!instance.isValid())
        {
            yield break;
        }

        float targetPitch = startPitch + increaseAmount;

        if (duration <= 0f)
        {
            instance.setPitch(targetPitch);
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (!instance.isValid())
            {
                yield break;
            }

            float t = Mathf.Clamp01(elapsed / duration);
            instance.setPitch(Mathf.Lerp(startPitch, targetPitch, t));
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (instance.isValid())
        {
            instance.setPitch(targetPitch);
        }
    }

    public bool TryGetEventInstance(EventReference sound, out EventInstance eventInstance)
    {
        eventInstance = default;
        string soundKey = GetSoundKey(sound);

        if (!eventInstancesBySound.TryGetValue(soundKey, out List<EventInstance> instancesForSound))
            return false;

        for (int i = instancesForSound.Count - 1; i >= 0; i--)
        {
            EventInstance candidate = instancesForSound[i];
            if (!candidate.isValid() || !IsInstanceActive(candidate))
                continue;

            eventInstance = candidate;
            return true;
        }

        return false;
    }

    public void ChangeVolumeProgression(EventReference sound, float increaseAmount, float duration)
    {
        if (!TryGetEventInstance(sound, out EventInstance eventInstance))
            return;

        eventInstance.getVolume(out float currentVolume);
        StartCoroutine(AnimateVolumeChange(eventInstance, currentVolume, increaseAmount, duration));
    }

    public void ChangePitchProgression(EventReference sound, float increaseAmount, float duration)
    {
        if (!TryGetEventInstance(sound, out EventInstance eventInstance))
            return;

        eventInstance.getPitch(out float currentPitch);
        StartCoroutine(AnimatePitchChange(eventInstance, currentPitch, increaseAmount, duration));
    }

    public void StopAllSfx()
    {
        sfxBus.stopAllEvents(FMOD.Studio.STOP_MODE.IMMEDIATE);
    }

    public void StopAllVoice()
    {
        voiceBus.stopAllEvents(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }

    public void StopAllAmbience()
    {
        ambienceBus.stopAllEvents(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }

    public EventInstance CreateInstance(EventReference sound)
    {
        return CreateInstance(sound, false);
    }

    public EventInstance CreateInstance(EventReference sound, bool overrideDuplicate, FMOD.Studio.STOP_MODE stopMode = FMOD.Studio.STOP_MODE.ALLOWFADEOUT)
    {
        string soundKey = GetSoundKey(sound);

        if (!eventInstancesBySound.TryGetValue(soundKey, out List<EventInstance> instancesForSound))
        {
            instancesForSound = new List<EventInstance>();
            eventInstancesBySound[soundKey] = instancesForSound;
        }

        EventInstance existingInstance = default;
        bool hasExistingInstance = false;
        for (int i = instancesForSound.Count - 1; i >= 0; i--)
        {
            EventInstance candidate = instancesForSound[i];
            if (!candidate.isValid())
            {
                instancesForSound.RemoveAt(i);
                continue;
            }

            if (!IsInstanceActive(candidate))
                continue;

            existingInstance = candidate;
            hasExistingInstance = true;
            break;
        }

        if (hasExistingInstance)
        {
            if (!overrideDuplicate)
                return existingInstance;

            existingInstance.stop(stopMode);
            existingInstance.release();
            eventInstances.Remove(existingInstance);
            RemoveInstanceFromMap(existingInstance);
        }

        EventInstance eventInstance = RuntimeManager.CreateInstance(sound);
        eventInstances.Add(eventInstance);
        instancesForSound.Add(eventInstance);
        return eventInstance;
    }

    public void StopSoundInstance(EventReference sound, FMOD.Studio.STOP_MODE stopMode = FMOD.Studio.STOP_MODE.ALLOWFADEOUT)
    {
        string soundKey = GetSoundKey(sound);

        if (!eventInstancesBySound.TryGetValue(soundKey, out List<EventInstance> instancesForSound))
            return;

        List<EventInstance> instancesToStop = new List<EventInstance>(instancesForSound);
        foreach (EventInstance eventInstance in instancesToStop)
        {
            if (!eventInstance.isValid())
                continue;

            eventInstance.stop(stopMode);
            eventInstance.release();
        }

        foreach (EventInstance eventInstance in instancesToStop)
        {
            eventInstances.Remove(eventInstance);
            RemoveInstanceFromMap(eventInstance);
        }
    }

    private string GetSoundKey(EventReference sound)
    {
#if UNITY_EDITOR && !FMOD_SERIALIZE_GUID_ONLY
        if (!string.IsNullOrEmpty(sound.Path))
            return sound.Path;
#endif

        return sound.Guid.ToString();
    }

    private bool IsInstanceActive(EventInstance instance)
    {
        if (!instance.isValid())
            return false;

        FMOD.RESULT playbackResult = instance.getPlaybackState(out PLAYBACK_STATE state);
        if (playbackResult != FMOD.RESULT.OK)
            return false;

        return state != PLAYBACK_STATE.STOPPED;
    }

    private void RemoveStoppedInstances()
    {
        for (int i = eventInstances.Count - 1; i >= 0; i--)
        {
            EventInstance eventInstance = eventInstances[i];

            if (IsInstanceActive(eventInstance))
                continue;

            if (eventInstance.isValid())
                eventInstance.release();

            eventInstances.RemoveAt(i);
            RemoveInstanceFromMap(eventInstance);
        }
    }

    private void RemoveInstanceFromMap(EventInstance instance)
    {
        List<string> keysToRemove = new List<string>();

        foreach (KeyValuePair<string, List<EventInstance>> pair in eventInstancesBySound)
        {
            List<EventInstance> instancesForSound = pair.Value;
            for (int i = instancesForSound.Count - 1; i >= 0; i--)
            {
                if (!instancesForSound[i].Equals(instance))
                    continue;

                instancesForSound.RemoveAt(i);
                break;
            }

            if (instancesForSound.Count == 0)
                keysToRemove.Add(pair.Key);
        }

        foreach (string key in keysToRemove)
            eventInstancesBySound.Remove(key);
    }

    private void CleanupEventInstances()
    {
        foreach(EventInstance eventInstance in eventInstances)
        {
            if (!eventInstance.isValid())
                continue;

            eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            eventInstance.release();
        }

        eventInstances.Clear();
        eventInstancesBySound.Clear();
    }

    private void OnDestroy()
    {
        CleanupEventInstances();
    }
}
