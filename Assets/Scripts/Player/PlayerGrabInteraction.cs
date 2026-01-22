using UnityEngine;

public class PlayerGrabInteraction : MonoBehaviour
{
    public float pickupRadius = 2f;
    public float frontDotThreshold = 0.5f;
    public LayerMask pickupLayer;
    public Transform holdPoint;

    private ItemInteraction currentItem;
    private ItemInteraction heldItem;

    void Update()
    {
        DetectItemInteraction();

        if (Input.GetKeyDown(KeyCode.E) && currentItem != null && heldItem == null)
        {
            heldItem = currentItem;
            heldItem.Pickup(holdPoint);
        }
        else if (Input.GetKeyDown(KeyCode.Q) && heldItem != null)
        {
            heldItem.Throw(transform.forward);
            heldItem = null;
            currentItem = null;
        }
    }

    void DetectItemInteraction()
    {
        ItemInteraction bestItem = null;
        float bestDistance = float.MaxValue;

        Collider[] hits = Physics.OverlapSphere(transform.position, pickupRadius, pickupLayer);

        foreach (Collider hit in hits)
        {
            if (!hit.TryGetComponent(out ItemInteraction item) || item.IsHeld)
                continue;

            Vector3 toItem = (hit.transform.position - transform.position).normalized;
            float dot = Vector3.Dot(transform.forward, toItem);

            // Only detect front cone
            if (dot < frontDotThreshold)
                continue;

            float distance = Vector3.Distance(transform.position, hit.transform.position);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestItem = item;
            }
        }

        if (bestItem != currentItem)
        {
            if (currentItem != null)
                currentItem.HideUI();

            currentItem = bestItem;

            if (currentItem != null)
                currentItem.ShowUI();
        }
    }
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, transform.forward * pickupRadius);
    }
}
