using FMOD.Studio;
using FMODUnity;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private List<EventInstance> eventInstances;
    public static AudioManager Instance;
    private Bus masterVolumeBus, bgmBus, sfxBus, voiceBus;

    private void Awake()
    {
        if(Instance != null)
            Destroy(Instance.gameObject);
        
        Instance = this;

        eventInstances = new List<EventInstance>();

        masterVolumeBus = RuntimeManager.GetBus("bus:/");
        bgmBus = RuntimeManager.GetBus("bus:/BGM");
        sfxBus = RuntimeManager.GetBus("bus:/SFX");
        voiceBus = RuntimeManager.GetBus("bus:/Voice");
    }

    private void Update()
    {
        masterVolumeBus.setVolume(SettingManager.Instance.settings.MasterVolume);
        bgmBus.setVolume(SettingManager.Instance.settings.MusicVolume);
        sfxBus.setVolume(SettingManager.Instance.settings.SoundEffectVolume);
        voiceBus.setVolume(SettingManager.Instance.settings.MobVolume);
    }

    public void PlayOneShot(EventReference sound, float volume, float pitch, Vector3 position = default)
    {
        var instance = RuntimeManager.CreateInstance(sound);
        instance.setVolume(volume);
        instance.setPitch(pitch);
        instance.set3DAttributes(RuntimeUtils.To3DAttributes(position));
        instance.start();
        instance.release();
    }

    public void StopAllSfx()
    {
        sfxBus.stopAllEvents(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }

    public void StopAllVoice()
    {
        voiceBus.stopAllEvents(FMOD.Studio.STOP_MODE.IMMEDIATE);
    }

    public EventInstance CreateInstance(EventReference sound)
    {
        EventInstance eventInstance = RuntimeManager.CreateInstance(sound);
        eventInstances.Add(eventInstance);
        return eventInstance;
    }

    private void CleanupEventInstances()
    {
        foreach(EventInstance eventInstance in eventInstances)
        {
            eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            eventInstance.release();
        }
    }

    private void OnDestroy()
    {
        CleanupEventInstances();
    }
}
