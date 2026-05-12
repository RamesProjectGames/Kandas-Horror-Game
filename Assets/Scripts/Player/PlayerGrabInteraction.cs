using System.Collections.Generic;
using Dialogue;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerGrabInteraction : MonoBehaviour
{
    [SerializeField] float yOffset = .5f;
    public float pickupRadius = 2f;
    public float frontDotThreshold = 0.5f;
    [Tooltip("X = minimum force, Y = maximum force when fully charged.")]
    public Vector2 throwForce;

    [Tooltip("How long the player can hold the throw button to reach max force.")]
    public static float maxThrowChargeTime = 1f;

    public LayerMask pickupLayer, interactableLayer, fragmentLayer;
    public Transform holdPoint;
    [Tooltip("Optional camera whose forward vector will be used for throws. If not assigned the player transform is used.")]
    public Camera playerCamera;
    public List<string> playerInteractionTexts = new List<string>();
    public TextMeshProUGUI bottomInteractText;
    public Slider throwpowerSlider;

    private ItemInteraction currentItem;
    private ItemInteraction heldItem;
    [SerializeField] private InputActionReference throwAction, interAction;

    // runtime state for charging a throw
    private static float throwCharge;

    void Start()
    {
        throwpowerSlider.gameObject.SetActive(false);
        if(throwAction != null)
        {
            string bindingDisplay = throwAction.action.GetBindingDisplayString(0);
            AddPlayerInteractionTexts($"Press {bindingDisplay} to throw");
        }
    }

    void Update()
    {
        DetectItemInteraction();

        if (SettingManager.Instance.isPaused || DialogueSystem.Instance.isRunningConvo)
            return;
        if (interAction != null && interAction.action.WasPerformedThisFrame())
        {
            if (currentItem != null)
            {
                if ((interactableLayer & (1 << currentItem.gameObject.layer)) != 0 || (fragmentLayer & (1 << currentItem.gameObject.layer)) != 0)
                {
                    currentItem.onInteract.Invoke();
                    //GetComponent<PlayerController>().FaceObject(currentItem.transform);
                }
                else if ((pickupLayer & (1 << currentItem.gameObject.layer)) != 0)
                {
                    if (currentItem != null && heldItem == null)
                    {
                        heldItem = currentItem;
                        heldItem.Pickup(holdPoint);
                    }
                }
                else if (currentItem.CanInteractWhenHeld && heldItem != null)
                {
                    currentItem.onHoldInteract?.Invoke();
                }
            }
        }
        
        // handle charging and releasing a throw
        if (throwAction != null)
        {
            
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

            }
            throwpowerSlider.value = throwCharge / maxThrowChargeTime;
        }
        
        if (heldItem != null)
        {
            string interactionText = "";
            for (int i = 0; i < playerInteractionTexts.Count; i++)
            {
                interactionText += playerInteractionTexts[i] + (i < playerInteractionTexts.Count - 1 ? " or \n" : "");
            }
            bottomInteractText.text = interactionText;
        }
        else
        {
            bottomInteractText.text = "";
        }
    }
    public void AddPlayerInteractionTexts(string newText)
    {
        if(string.IsNullOrEmpty(newText) || playerInteractionTexts.Contains(newText))
        {
            return;
        }
        playerInteractionTexts.Add(newText);
    }
    public void RemovePlayerInteractionTexts(string textToRemove)
    {
        if (playerInteractionTexts.Contains(textToRemove))
        {
            playerInteractionTexts.Remove(textToRemove);
        }
    }
    public static float GetThrowCharge()
    {
        return throwCharge / maxThrowChargeTime;
    }
    public static void ResetThrowCharge()
    {
        throwCharge = 0f;
    }
    void DetectItemInteraction()
    {
        ItemInteraction bestItem = null;
        float bestDistance = float.MaxValue;
        Vector3 visionPos = new Vector3(transform.position.x, transform.position.y + yOffset, transform.position.z);

        Collider[] hits = Physics.OverlapSphere(visionPos, pickupRadius, pickupLayer | interactableLayer | fragmentLayer);

        foreach (Collider hit in hits)
        {
            if (!hit.TryGetComponent(out ItemInteraction item) || item.IsHeld)
                continue;

            Vector3 toItem = (hit.transform.position - visionPos).normalized;
            Vector3 detectionForward = (playerCamera != null) ? playerCamera.transform.forward : transform.forward;
            float dot = Vector3.Dot(detectionForward, toItem);

            // Only detect front cone
            if (dot >= frontDotThreshold)
            {

                float distance = Vector3.Distance(visionPos, hit.transform.position);
                if (distance < bestDistance)
                {
                    if ((interactableLayer & (1 << item.gameObject.layer)) != 0 || (fragmentLayer & (1 << item.gameObject.layer)) != 0)
                    {
                        if(item.IsDialogueRelevant())
                        {
                            bestDistance = distance;
                            bestItem = item;
                        }
                    }
                    else if ((pickupLayer & (1 << item.gameObject.layer)) != 0)
                    {
                        bestDistance = distance;
                        bestItem = item;
                    }
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
        Vector3 visionPos = new Vector3(transform.position.x, transform.position.y + yOffset, transform.position.z);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(visionPos, pickupRadius);

        Gizmos.color = Color.blue;
        Vector3 gizmoForward = (playerCamera != null) ? playerCamera.transform.forward : transform.forward;
        Gizmos.DrawRay(visionPos, gizmoForward * pickupRadius);
    }
}
