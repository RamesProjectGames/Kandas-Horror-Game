using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Manages the player's ability to hide in hiding spots.
/// Handles hiding/unhiding mechanics and communication with hiding spots and enemies.
/// </summary>
public class PlayerHiding : MonoBehaviour
{
    [Header("Hiding Configuration")]
    [SerializeField] private float detectionRadius = 3f;
    [SerializeField] private LayerMask hidingSpotLayer;

    [Header("Animation Configuration")]
    [SerializeField] private float hidingAnimationDuration = 1.5f;
    [SerializeField] private float rotationSpeed = 5f;

    private bool isHiding = false;
    public bool Hiding => isHiding;
    private bool isAnimatingHide = false;
    private HidingSpot currentHidingSpot;
    private Vector3 hidingPosition;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Rigidbody playerRigidbody;
    private Collider playerCollider;
    private CharacterController characterController;
    private Animator animator;
    private InputAction interactAction;
    private float hidingAnimationTimer = 0f;

    private void Start()
    {
        playerRigidbody = GetComponent<Rigidbody>();
        playerCollider = GetComponent<Collider>();
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        
        var inputActions = GetComponent<PlayerInput>();
        if (inputActions != null)
        {
            interactAction = inputActions.actions.FindAction("Interact");
        }
    }

    private void Update()
    {
        // Handle hiding animation timing
        if (isAnimatingHide)
        {
            hidingAnimationTimer += Time.deltaTime;
            if (hidingAnimationTimer >= hidingAnimationDuration)
            {
                isAnimatingHide = false;
                hidingAnimationTimer = 0f;
                // Animation complete, player is now fully in cupboard
            }
        }

        // Check for nearby hiding spots when not hiding
        if (!isHiding && !isAnimatingHide)
        {
            DetectNearbyHidingSpots();
        }

        // Handle hiding input
        if (interactAction != null && interactAction.WasPerformedThisFrame())
        {
            if (isHiding)
            {
                Unhide();
            }
            else
            {
                TryHide();
            }
        }

    }

    /// <summary>
    /// Detect hiding spots in range.
    /// </summary>
    private void DetectNearbyHidingSpots()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius, hidingSpotLayer);

        foreach (Collider col in colliders)
        {
            HidingSpot spot = col.GetComponent<HidingSpot>();
            if (spot != null && spot.CanHide())
            {
                // You could add UI feedback here to show available hiding spots
            }
        }
    }

    /// <summary>
    /// Attempt to hide in the nearest hiding spot.
    /// </summary>
    public void TryHide()
    {
        if (isHiding)
            return;

        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius, hidingSpotLayer);
        HidingSpot nearestSpot = null;
        float nearestDistance = float.MaxValue;

        foreach (Collider col in colliders)
        {
            HidingSpot spot = col.GetComponent<HidingSpot>();
            if (spot != null && spot.CanHide())
            {
                float distance = Vector3.Distance(transform.position, spot.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestSpot = spot;
                }
            }
        }

        if (nearestSpot != null)
        {
            Hide(nearestSpot);
        }
    }

    /// <summary>
    /// Hide the player in the specified hiding spot.
    /// </summary>
    public void Hide(HidingSpot hidingSpot)
    {
        if (isHiding || isAnimatingHide)
            return;

        isAnimatingHide = true;
        hidingAnimationTimer = 0f;
        currentHidingSpot = hidingSpot;
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        hidingPosition = hidingSpot.GetHidingPosition();

        // Notify the hiding spot
        hidingSpot.HidePlayer(gameObject);

        // Rotate player to look away from the cupboard (player faces opposite direction)
        Vector3 directionToCupboard = (hidingPosition - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(-directionToCupboard);
        transform.rotation = targetRotation;

        // Trigger entering animation
        if (animator != null)
        {
            animator.SetBool("IsEnteringCupboard", true);
            animator.SetTrigger("EnterCupboard");
        }

        // Disable physics/collider during animation
        if (playerRigidbody != null)
        {
            playerRigidbody.isKinematic = true;
        }
        if (playerCollider != null)
        {
            playerCollider.enabled = false;
        }
        if (characterController != null)
        {
            characterController.enabled = false;
        }

        // Move player to hiding position (can be done instantly or smoothly depending on animation)
        transform.position = hidingPosition; // use world position to avoid parent-relative offsets
        isHiding = true;

        // Debug.Log("Player is now hiding!");
    }

    /// <summary>
    /// Unhide the player from current hiding spot.
    /// </summary>
    public void Unhide()
    {
        if (!isHiding || currentHidingSpot == null)
            return;

        isHiding = false;

        // Trigger exiting animation
        if (animator != null)
        {
            animator.SetBool("IsEnteringCupboard", false);
            animator.SetTrigger("ExitCupboard");
        }

        // Notify the hiding spot
        currentHidingSpot.UnhidePlayer();

        // Restore player position, rotation and physics
        transform.position = originalPosition;
        transform.rotation = originalRotation;

        if (playerRigidbody != null)
        {
            playerRigidbody.isKinematic = false;
        }
        if (playerCollider != null)
        {
            playerCollider.enabled = true;
        }
        if (characterController != null)
        {
            characterController.enabled = true;
        }

        currentHidingSpot = null;
        // Debug.Log("Player is no longer hiding!");
    }

    /// <summary>
    /// Force unhide the player when discovered (called by enemy/hiding spot).
    /// </summary>
    public void ForceUnhide()
    {
        if (!isHiding)
            return;

        isHiding = false;

        // Trigger exit animation
        if (animator != null)
        {
            animator.SetBool("IsEnteringCupboard", false);
            animator.SetTrigger("ExitCupboard");
        }

        if (currentHidingSpot != null)
        {
            currentHidingSpot.UnhidePlayer();
            currentHidingSpot = null;
        }

        // Restore player position, rotation and physics
        transform.position = originalPosition;
        transform.rotation = originalRotation;

        if (playerRigidbody != null)
        {
            playerRigidbody.isKinematic = false;
        }
        if (playerCollider != null)
        {
            playerCollider.enabled = true;
        }
        if (characterController != null)
        {
            characterController.enabled = true;
        }

        // Debug.Log("Player has been discovered and forced to unhide!");
    }

    /// <summary>
    /// Check if player is currently hiding.
    /// </summary>
    public bool IsHiding()
    {
        return isHiding;
    }

    /// <summary>
    /// Get the current hiding spot if player is hiding.
    /// </summary>
    public HidingSpot GetCurrentHidingSpot()
    {
        return currentHidingSpot;
    }

    private void OnDrawGizmos()
    {
        // Draw detection radius
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
