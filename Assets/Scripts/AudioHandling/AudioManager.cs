using FMOD.Studio;
using FMODUnity;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    private Bus masterVolumeBus, bgmBus, sfxBus, voiceBus;

    private void Awake()
    {
        if(Instance != null)
            Destroy(Instance.gameObject);
        
        Instance = this;

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

    public void PlayOneShot(EventReference sound, Vector3 position = default)
    {
        RuntimeManager.PlayOneShot(sound);
    }
}
