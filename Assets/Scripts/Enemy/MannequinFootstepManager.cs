using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class MannequinFootstepManager : MonoBehaviour
{
    [SerializeField] private EventReference footstepAudio;
    private EventInstance footstepEvent;   

    private bool HasValidFootstepEvent()
    {
        return footstepEvent.hasHandle() && footstepEvent.isValid();
    }
    private void EnsureFootstepEventCreated()
    {
        if (AudioManager.Instance == null || footstepAudio.IsNull)
            return;

        if (!footstepEvent.hasHandle() || !footstepEvent.isValid())
        {
            footstepEvent = AudioManager.Instance.CreateInstance(footstepAudio);
            RuntimeManager.AttachInstanceToGameObject(footstepEvent, gameObject, false);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        EnsureFootstepEventCreated();        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void PlayFootstep()
    {
        EnsureFootstepEventCreated();

        if (HasValidFootstepEvent())
        {
            AudioManager.Instance.PlayOneShot3D(footstepAudio,false,SettingManager.Instance.settings.SoundEffectVolume,1, transform.position);
        }
    }
    public void StopFootstep()
    {
        if (HasValidFootstepEvent())
        {
            footstepEvent.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        }
    }
}
