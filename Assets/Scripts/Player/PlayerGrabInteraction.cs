using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerGrabInteraction : MonoBehaviour
{
    public float pickupRadius = 2f;
    public float frontDotThreshold = 0.5f;
    [Tooltip("X = minimum force, Y = maximum force when fully charged.")]
    public Vector2 throwForce;

    [Tooltip("How long the player can hold the throw button to reach max force.")]
    public float maxThrowChargeTime = 1f;

    public LayerMask pickupLayer, interactableLayer;
    public Transform holdPoint;

    private ItemInteraction currentItem;
    private ItemInteraction heldItem;
    private InputAction grabAction, throwAction, interAction;

    // runtime state for charging a throw
    private float throwCharge;

    void Start()
    {
        var inputActions = GetComponent<PlayerInput>();
        if (inputActions != null)
        {
            grabAction = inputActions.actions.FindAction("Grab");
            throwAction = inputActions.actions.FindAction("Throw");
            interAction = inputActions.actions.FindAction("Interact");
        }
    }

    void Update()
    {
        DetectItemInteraction();

        if (interAction != null && interAction.WasPerformedThisFrame())
        {
            if (currentItem != null)
            {
                if (1 << currentItem.gameObject.layer == interactableLayer)
                {
                    currentItem.onInteract.Invoke();
                }
                else if (1 << currentItem.gameObject.layer == pickupLayer)
                {
                    if (currentItem != null && heldItem == null)
                    {
                        heldItem = currentItem;
                        heldItem.Pickup(holdPoint);
                    }
                }
            }
        }
        // handle charging and releasing a throw
        if (throwAction != null)
        {
            // accumulate charge while the button is held and we have an item
            if (throwAction.IsPressed() && heldItem != null)
            {
                throwCharge += Time.deltaTime;
                if (throwCharge > maxThrowChargeTime)
                    throwCharge = maxThrowChargeTime;
            }

            // when the button is released, actually perform the throw
            if (throwAction.WasReleasedThisFrame())
            {
                if (heldItem != null)
                {
                    float t = Mathf.Clamp01(throwCharge / maxThrowChargeTime);
                    float forceMag = Mathf.Lerp(throwForce.x, throwForce.y, t);
                    heldItem.Throw(transform.forward * forceMag);
                    heldItem = null;
                }

                // reset charge no matter what
                throwCharge = 0f;
            }
        }
    }

    void DetectItemInteraction()
    {
        ItemInteraction bestItem = null;
        float bestDistance = float.MaxValue;

        Collider[] hits = Physics.OverlapSphere(transform.position, pickupRadius, pickupLayer | interactableLayer);

        foreach (Collider hit in hits)
        {
            if (!hit.TryGetComponent(out ItemInteraction item) || item.IsHeld)
                continue;

            Vector3 toItem = (hit.transform.position - transform.position).normalized;
            float dot = Vector3.Dot(transform.forward, toItem);

            // Only detect front cone
            if (dot >= frontDotThreshold)
            {

                float distance = Vector3.Distance(transform.position, hit.transform.position);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestItem = item;
                }
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
