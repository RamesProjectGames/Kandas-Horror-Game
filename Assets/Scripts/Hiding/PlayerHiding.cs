using Dialogue;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.PostProcessing;

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

    public CinemachineCamera originCamera;
    private bool isHiding = false;
    public bool Hiding => isHiding;
    private bool isAnimatingHide = false;
    private HidingSpot currentHidingSpot;
    private Vector3 hidingPosition;
    private Rigidbody playerRigidbody;
    private Collider playerCollider;
    private NavMeshAgent agent;
    private Animator animator;
    [SerializeField] private InputActionReference interactAction;
    private float hidingAnimationTimer = 0f;

    private void Start()
    {
        playerRigidbody = GetComponent<Rigidbody>();
        playerCollider = GetComponent<Collider>();
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (SettingManager.Instance.isPaused || DialogueSystem.Instance.isRunningConvo)
            return;
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
        if (interactAction != null && interactAction.action.WasPerformedThisFrame())
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
        Vector3 bodyCenter = new Vector3(transform.position.x, transform.position.y + +(GetComponent<CapsuleCollider>().height / 2), transform.position.z);
        Collider[] colliders = Physics.OverlapSphere(bodyCenter, detectionRadius, hidingSpotLayer);

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

        Vector3 bodyCenter = new Vector3(transform.position.x, transform.position.y + +(GetComponent<CapsuleCollider>().height / 2), transform.position.z);
        Collider[] colliders = Physics.OverlapSphere(bodyCenter, detectionRadius, hidingSpotLayer);
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
        CameraManager.SwitchCamera(hidingSpot.GetHidingCamera());
        var postProcessVolume = FindAnyObjectByType<PostProcessVolume>();
        if (postProcessVolume != null)
        {
            postProcessVolume.profile.GetSetting<Vignette>().enabled.value = true;
        }
        // originalPosition = transform.position;
        // originalRotation = transform.rotation;
        // hidingPosition = hidingSpot.GetHidingPosition();

        // // Notify the hiding spot
        // hidingSpot.HidePlayer(gameObject);

        // // Rotate player to look away from the cupboard (player faces opposite direction)
        // Vector3 directionToCupboard = (hidingPosition - transform.position).normalized;
        // Quaternion targetRotation = Quaternion.LookRotation(-directionToCupboard);
        // transform.rotation = targetRotation;

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
        if (agent != null)
        {
            agent.enabled = false;
        }

        // Move player to hiding position (can be done instantly or smoothly depending on animation)
        // transform.position = hidingPosition; // use world position to avoid parent-relative offsets
        isHiding = true;

        // inform any enemies that currently can see the player that the player
        // has just slipped into a hiding spot; they will become alerted to the
        // hiding attempt.  Clear all previous flags first so that only this event
        // matters.
        var allSight = FindObjectsByType<EnemySightDetection>(FindObjectsSortMode.None);
        foreach (var sight in allSight)
        {
            sight.ResetSpottedFlag();
        }
        foreach (var sight in allSight)
        {
            sight.NotifyPlayerHidWhileVisible();
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

        // Trigger exiting animation
        if (animator != null)
        {
            animator.SetBool("IsEnteringCupboard", false);
            animator.SetTrigger("ExitCupboard");
        }

        // Notify the hiding spot
        currentHidingSpot.UnhidePlayer();

        CameraManager.SwitchCamera(originCamera);

        var postProcessVolume = FindAnyObjectByType<PostProcessVolume>();
        if (postProcessVolume != null)
        {
            postProcessVolume.profile.GetSetting<Vignette>().enabled.value = false;
        }

        if (playerRigidbody != null)
        {
            playerRigidbody.isKinematic = false;
        }
        if (playerCollider != null)
        {
            playerCollider.enabled = true;
        }
        if (agent != null)
        {
            agent.enabled = true;
        }

        currentHidingSpot = null;

        // when the player leaves a hiding spot, enemies should forget that they
        // once saw them concealed so they will resume normal vision behaviour
        foreach (var sight in FindObjectsByType<EnemySightDetection>(FindObjectsSortMode.None))
        {
            sight.ResetSpottedFlag();
        }

        Debug.Log("Player is no longer hiding!");
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
        
        CameraManager.SwitchCamera(originCamera);

        if (playerRigidbody != null)
        {
            playerRigidbody.isKinematic = false;
        }
        if (playerCollider != null)
        {
            playerCollider.enabled = true;
        }
        if (agent != null)
        {
            agent.enabled = true;
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
        Vector3 bodyCenter = new Vector3(transform.position.x, transform.position.y + (GetComponent<CapsuleCollider>().height/2), transform.position.z);
        Gizmos.DrawWireSphere(bodyCenter, detectionRadius);
    }
}
