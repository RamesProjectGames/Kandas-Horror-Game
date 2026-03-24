using Dialogue;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerGrabInteraction : MonoBehaviour
{
    public float pickupRadius = 2f;
    public float frontDotThreshold = 0.5f;
    [Tooltip("X = minimum force, Y = maximum force when fully charged.")]
    public Vector2 throwForce;

    [Tooltip("How long the player can hold the throw button to reach max force.")]
    public float maxThrowChargeTime = 1f;

    public LayerMask pickupLayer, interactableLayer, fragmentLayer;
    public Transform holdPoint;
    [Tooltip("Optional camera whose forward vector will be used for throws. If not assigned the player transform is used.")]
    public Camera playerCamera;
    public TextMeshProUGUI throwInteractionText;
    public Slider throwpowerSlider;

    private ItemInteraction currentItem;
    private ItemInteraction heldItem;
    [SerializeField] private InputActionReference throwAction, interAction;

    // runtime state for charging a throw
    private float throwCharge;

    void Start()
    {
        throwpowerSlider.gameObject.SetActive(false);
    }

    void Update()
    {
        DetectItemInteraction();
        //DetectFragmentItem();

        if (SettingManager.Instance.isPaused || DialogueSystem.Instance.isRunningConvo)
            return;
        if (interAction != null && interAction.action.WasPerformedThisFrame())
        {
            if (currentItem != null)
            {
                if ((interactableLayer & (1 << currentItem.gameObject.layer)) != 0 || (fragmentLayer & (1 << currentItem.gameObject.layer)) != 0)
                {
                    currentItem.onInteract.Invoke();
                    if (currentItem is DialogueHandler)
                        transform.LookAt(currentItem.transform);
                }
                else if ((pickupLayer & (1 << currentItem.gameObject.layer)) != 0)
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
            if (heldItem != null)
            {
                string bindingDisplay = throwAction.action.GetBindingDisplayString(0);
                throwInteractionText.text = $"Press {bindingDisplay} to throw";
            }
            else
            {
                throwInteractionText.text = "";
            }
            // accumulate charge while the button is held and we have an item
            if (throwAction.action.IsPressed() && heldItem != null)
            {
                throwpowerSlider.gameObject.SetActive(true);
                throwCharge += Time.deltaTime;
                if (throwCharge > maxThrowChargeTime)
                    throwCharge = maxThrowChargeTime;

            }

            // when the button is released, actually perform the throw
            if (throwAction.action.WasReleasedThisFrame())
            {
                if (heldItem != null)
                {
                    throwpowerSlider.gameObject.SetActive(false);
                    float t = Mathf.Clamp01(throwCharge / maxThrowChargeTime);
                    float forceMag = Mathf.Lerp(throwForce.x, throwForce.y, t);
                    // use camera forward direction if available, otherwise fall back to player forward
                    Vector3 direction = (playerCamera != null) ? playerCamera.transform.forward : transform.forward;
                    heldItem.Throw(direction * forceMag);
                    heldItem = null;
                }

                // reset charge no matter what
                throwCharge = 0f;
            }
            throwpowerSlider.value = throwCharge / maxThrowChargeTime;
        }
    }

    void DetectItemInteraction()
    {
        ItemInteraction bestItem = null;
        float bestDistance = float.MaxValue;

        Collider[] hits = Physics.OverlapSphere(transform.position, pickupRadius, pickupLayer | interactableLayer | fragmentLayer);

        foreach (Collider hit in hits)
        {
            if (!hit.TryGetComponent(out ItemInteraction item) || item.IsHeld)
                continue;

            Vector3 toItem = (hit.transform.position - transform.position).normalized;
            Vector3 detectionForward = (playerCamera != null) ? playerCamera.transform.forward : transform.forward;
            float dot = Vector3.Dot(detectionForward, toItem);

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
    //void DetectFragmentItem()
    //{
    //    Fragment bestItem = null;
    //    float bestDistance = float.MaxValue;

    //    Collider[] hits = Physics.OverlapSphere(transform.position, pickupRadius, fragmentLayer);
    //    foreach (Collider hit in hits)
    //    {
    //        if (!hit.TryGetComponent(out Fragment item))
    //            continue;

    //        Vector3 toItem = (hit.transform.position - transform.position).normalized;
    //        Vector3 detectionForward = (playerCamera != null) ? playerCamera.transform.forward : transform.forward;
    //        float dot = Vector3.Dot(detectionForward, toItem);

    //        // Only detect front cone
    //        if (dot >= frontDotThreshold)
    //        {

    //            float distance = Vector3.Distance(transform.position, hit.transform.position);
    //            if (distance < bestDistance)
    //            {
    //                bestDistance = distance;
    //                bestItem = item;
    //            }
    //        }
    //    }

    //    if (bestItem != fragmentItem)
    //    {
    //        fragmentItem = bestItem;
    //        if (fragmentItem != null)
    //            fragmentItem.HideUI();

    //        if (fragmentItem != null)
    //            fragmentItem.ShowUI();
    //    }
    //}
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);

        Gizmos.color = Color.blue;
        Vector3 gizmoForward = (playerCamera != null) ? playerCamera.transform.forward : transform.forward;
        Gizmos.DrawRay(transform.position, gizmoForward * pickupRadius);
    }
}
