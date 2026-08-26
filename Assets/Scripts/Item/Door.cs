using FMODUnity;
using UnityEngine;

public class Door : MonoBehaviour
{
    public Vector3 openRotation;
    public Vector3 closedRotation;
    bool canInteractWithDoor = true;
    bool isOpen = false;
    OcclusionPortal portal;

    void Start()
    {
        portal = GetComponent<OcclusionPortal>();
    }

    void Update()
    {
        if(portal != null)
        {
            portal.open = isOpen;            
        }
    }

    public void ToggleDoor()
    {
        Debug.Log("Toggling Door");
        if (isOpen)
            CloseDoor();
        else
            OpenDoor();
    }
    [ContextMenu("Set Open Rotation")]
    public void OpenDoor(float duration = 0.5f)
    {
        EventReference openSfx = RuntimeManager.PathToEventReference("event:/SFX/OpenDoor");
        EventReference closeSfx = RuntimeManager.PathToEventReference("event:/SFX/CloseDoor");
        AudioManager.Instance.StopSoundInstance(openSfx);
        AudioManager.Instance.StopSoundInstance(closeSfx);
        AudioManager.Instance.PlayOneShot3D(openSfx,true, 1, 1, transform.position);
        if (!canInteractWithDoor) return;
        transform.LeanRotate(openRotation, duration);
        isOpen = true;
    }
    [ContextMenu("Set Closed Rotation")]
    public void CloseDoor(float duration = 0.5f)
    {
        EventReference openSfx = RuntimeManager.PathToEventReference("event:/SFX/OpenDoor");
        EventReference closeSfx = RuntimeManager.PathToEventReference("event:/SFX/CloseDoor");
        AudioManager.Instance.StopSoundInstance(openSfx);
        AudioManager.Instance.StopSoundInstance(closeSfx);
        AudioManager.Instance.PlayOneShot3D(closeSfx,true, 1, 1, transform.position);
        if (!canInteractWithDoor) return;
        transform.LeanRotate(closedRotation, duration);
        isOpen = false;
    }
}
