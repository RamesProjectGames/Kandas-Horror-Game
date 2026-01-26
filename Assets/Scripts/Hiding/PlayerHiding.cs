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

    private bool isHiding = false;
    private HidingSpot currentHidingSpot;
    private Vector3 hidingPosition;
    private Vector3 originalPosition;
    private Rigidbody playerRigidbody;
    private Collider playerCollider;
    private InputAction hideAction, unhideAction;

    private void Start()
    {
        playerRigidbody = GetComponent<Rigidbody>();
        playerCollider = GetComponent<Collider>();
        
        var inputActions = GetComponent<PlayerInput>();
        if (inputActions != null)
        {
            hideAction = inputActions.actions.FindAction("Hide");
            unhideAction = inputActions.actions.FindAction("Unhide");
        }
    }

    private void Update()
    {
        // Check for nearby hiding spots when not hiding
        if (!isHiding)
        {
            DetectNearbyHidingSpots();
        }

        // Handle hiding input
        if (hideAction != null && hideAction.WasPerformedThisFrame())
        {
            TryHide();
        }

        // Handle unhiding input
        if (unhideAction != null && unhideAction.WasPerformedThisFrame())
        {
            Unhide();
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
        if (isHiding)
            return;

        isHiding = true;
        currentHidingSpot = hidingSpot;
        originalPosition = transform.position;
        hidingPosition = hidingSpot.GetHidingPosition();

        // Notify the hiding spot
        hidingSpot.HidePlayer(gameObject);

        // Move player to hiding position
        transform.position = hidingPosition;

        // Optionally disable physics/collider
        if (playerRigidbody != null)
        {
            playerRigidbody.isKinematic = true;
        }
        if (playerCollider != null)
        {
            playerCollider.enabled = false;
        }

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

        // Notify the hiding spot
        currentHidingSpot.UnhidePlayer();

        // Restore player position and physics
        transform.position = originalPosition;

        if (playerRigidbody != null)
        {
            playerRigidbody.isKinematic = false;
        }
        if (playerCollider != null)
        {
            playerCollider.enabled = true;
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

        if (currentHidingSpot != null)
        {
            currentHidingSpot.UnhidePlayer();
            currentHidingSpot = null;
        }

        // Restore player position and physics
        transform.position = originalPosition;

        if (playerRigidbody != null)
        {
            playerRigidbody.isKinematic = false;
        }
        if (playerCollider != null)
        {
            playerCollider.enabled = true;
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
