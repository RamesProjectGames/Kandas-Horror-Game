using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;
public class InspectObject : MonoBehaviour, IDragHandler
{
    [SerializeField] private float rotationSpeed = 0.5f;

    public void OnDrag(PointerEventData eventData)
    {
        Transform target = InspectManagerUI.Instance.currentInpectObject;

        if (target != null)
        {
            // Use Rotate with Space.World to keep the controls intuitive 
            // regardless of the object's current orientation.
            Vector3 camRight = InspectManagerUI.Instance.lookAtCamera.transform.right;
            Vector3 camUp = InspectManagerUI.Instance.lookAtCamera.transform.up;

            // Rotate around the camera's vertical axis (Horizontal drag)
            target.Rotate(camUp, -eventData.delta.x * rotationSpeed, Space.World);

            // Rotate around the camera's horizontal axis (Vertical drag)
            target.Rotate(camRight, eventData.delta.y * rotationSpeed, Space.World);
        }
    }
}
