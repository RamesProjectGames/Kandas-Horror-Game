using FMODUnity;
using UnityEngine;
using UnityEngine.ProBuilder.Shapes;

public class Door : MonoBehaviour
{
    public Vector3 openRotation;
    public Vector3 closedRotation;
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
    public void OpenDoor(float duration = 0.5f, System.Action onComplete = null)
    {
        EventReference openSfx = RuntimeManager.PathToEventReference("event:/SFX/OpenDoor");
        EventReference closeSfx = RuntimeManager.PathToEventReference("event:/SFX/CloseDoor");
        AudioManager.Instance.StopSoundInstance(openSfx);
        AudioManager.Instance.StopSoundInstance(closeSfx);
        AudioManager.Instance.PlayOneShot3D(openSfx,true, 1, 1, transform.position);
        RotateTo(openRotation, duration, onComplete);
        ItemInteraction interactor = GetComponent<ItemInteraction>();
        if(interactor != null)
            interactor.ChangeInteractionText("Close Door");
        isOpen = true;
    }
    [ContextMenu("Set Closed Rotation")]
    public void CloseDoor(float duration = 0.5f, System.Action onComplete = null)
    {
        EventReference openSfx = RuntimeManager.PathToEventReference("event:/SFX/OpenDoor");
        EventReference closeSfx = RuntimeManager.PathToEventReference("event:/SFX/CloseDoor");
        AudioManager.Instance.StopSoundInstance(openSfx);
        AudioManager.Instance.StopSoundInstance(closeSfx);
        AudioManager.Instance.PlayOneShot3D(closeSfx,true, 1, 1, transform.position);
        RotateTo(closedRotation, duration, onComplete);
        ItemInteraction interactor = GetComponent<ItemInteraction>();
        if (interactor != null)
            interactor.ChangeInteractionText("Open Door");
        isOpen = false;
    }

    private void RotateTo(Vector3 targetEulerAngles, float duration, System.Action onComplete)
    {
        Quaternion startRotation = transform.localRotation;
        Quaternion targetRotation = Quaternion.Euler(targetEulerAngles);

        LeanTween.cancel(gameObject);
        LeanTween.value(gameObject, 0f, 1f, duration)
            .setOnUpdate(progress => transform.localRotation = Quaternion.Slerp(startRotation, targetRotation, progress))
            .setOnComplete(() =>
            {
                transform.localRotation = targetRotation;
                onComplete?.Invoke();
            });
    }
}
