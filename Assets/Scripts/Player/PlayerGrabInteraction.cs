using System.Collections.Generic;
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
    public static float maxThrowChargeTime = 1f;

    public LayerMask pickupLayer, interactableLayer, fragmentLayer;
    public Transform holdPoint;
    [Tooltip("Optional camera whose forward vector will be used for throws. If not assigned the player transform is used.")]
    public Camera playerCamera;
    public PlayerHiding playerHiding;
    public List<string> playerInteractionTexts = new List<string>();
    public TextMeshProUGUI bottomInteractText;
    public Slider throwpowerSlider;

    public ItemInteraction currentItem;
    private ItemInteraction heldItem;
    public ItemInteraction HeldItem => heldItem;
    [SerializeField] private InputActionReference throwAction, interAction;

    private List<ItemInteraction> detectedItems = new List<ItemInteraction>();
    private List<ItemInteraction> prevDetectedItems = new List<ItemInteraction>();
    private int currentIndex = 0;

    [System.Serializable]
    private enum InteractionLockType { None, Hiding, HoldingItem, Inspecting }
    [SerializeField]
    private InteractionLockType interactionLock = InteractionLockType.None;

    public bool IsInteractionLocked => interactionLock != InteractionLockType.None;

    // runtime state for charging a throw
    private static float throwCharge;

    void Start()
    {
        throwpowerSlider.gameObject.SetActive(false);
        SubscribeToInteractions();
    }

    void Update()
    {
        DetectItemInteraction();

        if (SettingManager.Instance.isPaused || SettingManager.Instance.gameOver || DialogueSystem.IsConversationRunning)
            return;

        if (Mouse.current != null)
        {
            Vector2 scroll = Mouse.current.scroll.ReadValue();
            if (Mathf.Abs(scroll.y) > 0.01f)
            {
                NavigateItems(scroll.y > 0 ? +1 : -1);
            }
        }

        if (interAction != null && interAction.action.WasPerformedThisFrame() && !IsInteractionLocked)
        {
            if (currentItem != null)
            {
                if ((interactableLayer & (1 << currentItem.gameObject.layer)) != 0 || (fragmentLayer & (1 << currentItem.gameObject.layer)) != 0)
                {
                    currentItem.onInteract.Invoke();
                    currentItem.TryGetComponent(out NpcMovement npcInteract);
                    if (npcInteract != null)
                    {
                        if (!npcInteract.facePlayer && npcInteract.animState != NPCAnimationState.Sit)
                            npcInteract.HandleAnimationEndState();
                        npcInteract.facePlayer = true;
                    }
                    currentItem.TryGetComponent(out HidingSpot hidingSpot);
                    if (playerHiding != null && hidingSpot != null)
                    {
                        playerHiding.Hide(hidingSpot);
                        if (playerHiding.IsHiding())
                            interactionLock = InteractionLockType.Hiding;
                    }
                    else if (IsInspecting())
                    {
                        interactionLock = InteractionLockType.Inspecting;
                    }
                    //GetComponent<PlayerController>().FaceObject(currentItem.transform);
                }
                else if ((pickupLayer & (1 << currentItem.gameObject.layer)) != 0)
                {
                    if (currentItem != null && heldItem == null)
                    {
                        heldItem = currentItem;
                        heldItem.Pickup(holdPoint);
                        ThrowItemInstruction(true);
                        interactionLock = IsInspecting() ? InteractionLockType.Inspecting : InteractionLockType.HoldingItem;
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
                    Vector3 direction = (CameraManager.currentActiveCamera != null) ? CameraManager.currentActiveCamera.transform.forward : transform.forward;
                    heldItem.Throw(direction * forceMag);
                    heldItem = null;
                    ThrowItemInstruction(false);
                    interactionLock = InteractionLockType.None;
                }

            }
            throwpowerSlider.value = throwCharge / maxThrowChargeTime;
        }

        if (heldItem != null)
        {
            // string interactionText = "";
            // for (int i = 0; i < playerInteractionTexts.Count; i++)
            // {
            //     interactionText += playerInteractionTexts[i] + (i < playerInteractionTexts.Count - 1 ? " or \n" : "");
            // }
            // bottomInteractText.text = interactionText;
        }
        else
        {
            bottomInteractText.text = "";
        }
    }
    public void AddPlayerInteractionTexts(string newText)
    {
        if (string.IsNullOrEmpty(newText) || playerInteractionTexts.Contains(newText))
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

    private static bool IsInspecting()
    {
        var inspectUI = InspectManagerUI.Instance;
        return inspectUI != null && inspectUI.InspectObjectUI != null && inspectUI.InspectObjectUI.activeSelf;
    }

    private void SubscribeToInteractions()
    {
        if (playerHiding != null)
            playerHiding.OnUnhideFinished += HandleUnhideFinished;

        var inspectUI = InspectManagerUI.Instance;
        if (inspectUI != null)
            inspectUI.OnInspectionClosed += HandleUnhideFinished;
    }

    private void OnDisable()
    {
        interactionLock = InteractionLockType.None;

        if (playerHiding != null)
            playerHiding.OnUnhideFinished -= HandleUnhideFinished;

        var inspectUI = InspectManagerUI.Instance;
        if (inspectUI != null)
            inspectUI.OnInspectionClosed -= HandleUnhideFinished;
    }

    private void HandleUnhideFinished()
    {
        interactionLock = InteractionLockType.None;
    }

    public bool TryGrabItem(ItemInteraction item)
    {
        if (item == null || heldItem != null || item.IsInActions)
            return false;

        heldItem = item;
        item.Pickup(holdPoint);
        ThrowItemInstruction(true);
        interactionLock = IsInspecting() ? InteractionLockType.Inspecting : InteractionLockType.HoldingItem;
        return true;
    }

    public bool TryTransferHeldItemTo(PlayerGrabInteraction target)
    {
        if (heldItem == null || target == null || target == this)
            return false;

        if (target.heldItem != null)
            return false;

        ItemInteraction item = heldItem;
        heldItem = null;
        target.heldItem = item;
        item.Pickup(target.holdPoint);
        ThrowItemInstruction(false);
        target.ThrowItemInstruction(true);
        return true;
    }

    public bool TryThrowHeldItem(float force, Vector3 direction)
    {
        if (heldItem == null)
            return false;

        Vector3 throwDirection = direction.sqrMagnitude > 0.001f ? direction.normalized : transform.forward;
        heldItem.Throw(throwDirection * Mathf.Max(0.1f, force));
        heldItem = null;
        ThrowItemInstruction(false);
        interactionLock = InteractionLockType.None;
        return true;
    }

    public bool TryThrowItem(ItemInteraction item, float force, Vector3 direction)
    {
        if (item == null)
            return false;

        if (heldItem == item)
        {
            return TryThrowHeldItem(force, direction);
        }

        Vector3 throwDirection = direction.sqrMagnitude > 0.001f ? direction.normalized : transform.forward;
        item.Throw(throwDirection * Mathf.Max(0.1f, force));
        return true;
    }
    public void ReleaseHeldItem()
    {
        if (heldItem != null)
        {
            heldItem.Drop();
            heldItem = null;
            ThrowItemInstruction(false);
            interactionLock = InteractionLockType.None;
        }
    }
    public void ThrowItemInstruction(bool isHolding)
    {
        if (throwAction == null || throwAction.action == null)
            return;

        if (InstructionManager.Instance == null)
            return;

        string bindingDisplay = throwAction.action.GetBindingDisplayString(0);
        if (isHolding)
        {
            InstructionManager.Instance.AddInstruction($"Press {bindingDisplay} to throw");
        }
        else
        {
            InstructionManager.Instance.RemoveInstruction($"Press {bindingDisplay} to throw");
        }
    }
    void DetectItemInteraction()
    {
        if (SettingManager.Instance.isPaused || SettingManager.Instance.gameOver || DialogueSystem.IsConversationRunning)
        {
            HideAllDetectedUI();
            return;
        }
        Vector3 visionPos = (CameraManager.currentActiveCamera != null) ? CameraManager.currentActiveCamera.transform.position : transform.position;

        Collider[] hits = Physics.OverlapSphere(visionPos, pickupRadius, pickupLayer | interactableLayer | fragmentLayer, QueryTriggerInteraction.Collide);

        detectedItems.Clear();
        foreach (Collider hit in hits)
        {
            if (!hit.TryGetComponent(out ItemInteraction item) || item.IsInActions)
                continue;

            Vector3 toItem = (hit.transform.position - visionPos).normalized;
            Vector3 detectionForward = (CameraManager.currentActiveCamera != null) ? CameraManager.currentActiveCamera.transform.forward : transform.forward;
            float dot = Vector3.Dot(detectionForward, toItem);

            // Only detect front cone
            if (dot >= frontDotThreshold)
            {
                if ((interactableLayer & (1 << item.gameObject.layer)) != 0 || (fragmentLayer & (1 << item.gameObject.layer)) != 0)
                {
                    if (item.IsDialogueRelevant())
                        detectedItems.Add(item);
                }
                else if ((pickupLayer & (1 << item.gameObject.layer)) != 0)
                {
                    detectedItems.Add(item);
                }
            }
        }

        // Sort detected items by distance so navigation order is deterministic
        detectedItems.Sort((a, b) =>
        {
            float da = Vector3.Distance(visionPos, a.transform.position);
            float db = Vector3.Distance(visionPos, b.transform.position);
            return da.CompareTo(db);
        });

        if (detectedItems.Count > 0)
        {
            if (currentIndex < 0 || currentIndex >= detectedItems.Count)
                currentIndex = 0;
        }
        else
        {
            currentIndex = 0;
        }

        ItemInteraction newCurrent = (detectedItems.Count > 0) ? detectedItems[currentIndex] : null;

        if (newCurrent != currentItem)
        {
            if (currentItem != null)
            {
                currentItem.TryGetComponent(out NpcMovement npcInteract);
                if (npcInteract != null)
                {
                    if (npcInteract.facePlayer && npcInteract.animState != NPCAnimationState.Sit)
                        npcInteract.HandleAnimationEndState();
                    npcInteract.facePlayer = false;
                }
            }

            currentItem = newCurrent;
        }

        // Show the prompt UI for every available interaction, but only highlight the selected one.
        for (int i = 0; i < detectedItems.Count; i++)
        {
            ItemInteraction item = detectedItems[i];
            item.ShowUI();
            item.SetHighlight(item == currentItem);
        }

        // Hide the UI of items that are no longer available.
        for (int i = 0; i < prevDetectedItems.Count; i++)
        {
            ItemInteraction item = prevDetectedItems[i];
            if (item != null && !detectedItems.Contains(item))
            {
                item.SetHighlight(false);
                item.HideUI();
            }
        }

        prevDetectedItems.Clear();
        prevDetectedItems.AddRange(detectedItems);
    }

    private void HideAllDetectedUI()
    {
        for (int i = 0; i < prevDetectedItems.Count; i++)
        {
            if (prevDetectedItems[i] != null)
            {
                prevDetectedItems[i].SetHighlight(false);
                prevDetectedItems[i].HideUI();
            }
        }
        prevDetectedItems.Clear();

        if (currentItem != null)
        {
            currentItem.SetHighlight(false);
            currentItem.HideUI();
        }
    }

    void NavigateItems(int direction)
    {
        if (detectedItems == null || detectedItems.Count == 0)
            return;

        if (currentItem != null)
        {
            currentItem.SetHighlight(false);

            currentItem.TryGetComponent(out NpcMovement npcInteract);
            if (npcInteract != null)
            {
                if (npcInteract.facePlayer && npcInteract.animState != NPCAnimationState.Sit)
                    npcInteract.HandleAnimationEndState();
                npcInteract.facePlayer = false;
            }
        }

        currentIndex = (currentIndex + direction) % detectedItems.Count;
        if (currentIndex < 0)
            currentIndex += detectedItems.Count;

        currentItem = detectedItems[currentIndex];
        if (currentItem != null)
            currentItem.SetHighlight(true);
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
        Vector3 visionPos = (CameraManager.currentActiveCamera != null) ? CameraManager.currentActiveCamera.transform.position : transform.position;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(visionPos, pickupRadius);

        Gizmos.color = Color.blue;
        Vector3 gizmoForward = (CameraManager.currentActiveCamera != null) ? CameraManager.currentActiveCamera.transform.forward : transform.forward;
        Gizmos.DrawRay(visionPos, gizmoForward * pickupRadius);
    }
}
