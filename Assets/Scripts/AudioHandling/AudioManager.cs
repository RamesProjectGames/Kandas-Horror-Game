using FMOD.Studio;
using FMODUnity;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private List<EventInstance> eventInstances;
    private Dictionary<string, EventInstance> eventInstancesBySound;
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
        else
        {
            Destroy(gameObject);
        }

        eventInstances = new List<EventInstance>();
        eventInstancesBySound = new Dictionary<string, EventInstance>();

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
        sfxBus.setVolume(SettingManager.Instance.settings.SoundEffectVolume);
        voiceBus.setVolume(SettingManager.Instance.settings.MobVolume);
    }

    public void PlayOneShot3D(EventReference sound, float volume, float pitch, Vector3 position = default)
    {
        var instance = CreateInstance(sound, true);
        instance.setVolume(volume);
        instance.setPitch(pitch);
        instance.set3DAttributes(RuntimeUtils.To3DAttributes(position));
        instance.start();
        instance.release();
    }

    public void PlayOneShot2D(EventReference sound, float volume, float pitch, Vector3 position = default)
    {
        var instance = CreateInstance(sound, true);
        instance.setVolume(volume);
        instance.setPitch(pitch);
        instance.start();
        instance.release();
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

        if (eventInstancesBySound.TryGetValue(soundKey, out EventInstance existingInstance) && IsInstanceActive(existingInstance))
        {
            if (!overrideDuplicate)
                return existingInstance;

            existingInstance.stop(stopMode);
            existingInstance.release();
            eventInstances.Remove(existingInstance);
            eventInstancesBySound.Remove(soundKey);
        }

        EventInstance eventInstance = RuntimeManager.CreateInstance(sound);
        eventInstances.Add(eventInstance);
        eventInstancesBySound[soundKey] = eventInstance;
        return eventInstance;
    }

    public void StopSoundInstance(EventReference sound, FMOD.Studio.STOP_MODE stopMode = FMOD.Studio.STOP_MODE.ALLOWFADEOUT)
    {
        string soundKey = GetSoundKey(sound);

        if (!eventInstancesBySound.TryGetValue(soundKey, out EventInstance eventInstance))
            return;

        eventInstance.stop(stopMode);
        eventInstance.release();
        eventInstances.Remove(eventInstance);
        eventInstancesBySound.Remove(soundKey);
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
        string keyToRemove = null;

        foreach (KeyValuePair<string, EventInstance> pair in eventInstancesBySound)
        {
            if (!pair.Value.Equals(instance))
                continue;

            keyToRemove = pair.Key;
            break;
        }

        if (keyToRemove != null)
            eventInstancesBySound.Remove(keyToRemove);
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
