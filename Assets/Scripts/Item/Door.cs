using UnityEngine;

public class Door : MonoBehaviour
{
    public Vector3 openRotation;
    public Vector3 closedRotation;
    bool canInteractWithDoor = true;
    bool isOpen = false;

    public void ToggleDoor()
    {
        Debug.Log("Toggling Door");
        if (isOpen)
            CloseDoor();
        else
            OpenDoor();
    }
    [ContextMenu("Set Open Rotation")]
    public void OpenDoor()
    {
        if (!canInteractWithDoor) return;
        transform.LeanRotate(openRotation, 0.5f);
        isOpen = true;
    }
    [ContextMenu("Set Closed Rotation")]
    public void CloseDoor()
    {
        if (!canInteractWithDoor) return;
        transform.LeanRotate(closedRotation, 0.5f);
        isOpen = false;
    }
}
