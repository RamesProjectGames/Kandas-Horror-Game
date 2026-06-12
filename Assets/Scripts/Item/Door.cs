using UnityEngine;

public class Door : MonoBehaviour
{
    public Vector3 openRotation;
    public Vector3 closedRotation;
    bool canInteractWithDoor = true;

    [ContextMenu("Set Open Rotation")]
    public void OpenDoor()
    {
        if (!canInteractWithDoor) return;
        transform.LeanRotate(openRotation, 0.5f);
    }
    [ContextMenu("Set Closed Rotation")]
    public void CloseDoor()
    {
        if (!canInteractWithDoor) return;
        transform.LeanRotate(closedRotation, 0.5f);
    }
}
