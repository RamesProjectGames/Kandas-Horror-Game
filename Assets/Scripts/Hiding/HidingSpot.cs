using Unity.Cinemachine;
using UnityEngine;

/// <summary>
/// Represents a hiding spot in the game world.
/// Players can hide in these spots to avoid being detected by enemies.
/// Enemies can discover and open hidden spots if they spot the player while hiding.
/// </summary>
public class HidingSpot : MonoBehaviour
{
    [Header("Hiding Spot Configuration")]
    [SerializeField] private float interactionRadius = 2f;
    [SerializeField] private float hidingHeight = 1f; // Height offset for hiding position
    [SerializeField] private bool visualizationEnabled = true;
    [SerializeField] private CinemachineCamera hidingCamera;
    private CinemachineInputAxisController inputController;
    
    [Header("Spot Discovery")]
    [SerializeField] private float discoveryTime = 2f; // Time for enemy to fully discover/open the spot
    private float currentDiscoveryProgress = 0f;
    private bool isBeingDiscovered = false;
    private EnemyMovement discoveringEnemy;

    private bool isOccupied = false;
    private GameObject hiddenPlayer;
    private bool isDiscovered = false;
    private Collider coll;
    private Rigidbody rb;
    private PlayerController playerController;

    void Start()
    {
        coll = GetComponent<Collider>();
        rb = GetComponent<Rigidbody>();
        inputController = hidingCamera.GetComponent<CinemachineInputAxisController>();
        playerController = FindAnyObjectByType<PlayerController>();
    }
    void Update()
    {
        if (inputController != null && SettingManager.Instance != null && playerController != null)
        {
            float sliderValue = SettingManager.Instance.settings.MouseSensitivity;
            float calculatedGain = Mathf.Clamp(sliderValue, SettingManager.Instance.minimumMouseSensitivity, SettingManager.Instance.maximumMouseSensitivity) * playerController.lookSensitivity * 2;

            // Controllers is a list. Usually: Index 0 = Pan, Index 1 = Tilt
            foreach (var controller in inputController.Controllers)
            {
                if (controller.Name.Contains("Tilt") )
                {
                    // Multiplying by -1 flips the direction
                    controller.Input.Gain = -calculatedGain;
                }
                else
                {
                    controller.Input.Gain = calculatedGain;
                }
                controller.Driver.AccelTime = 0f;
                controller.Driver.DecelTime = 0f;
            }
        }
    }
    private void OnDrawGizmos()
    {
        if (!visualizationEnabled) return;

        // Draw interaction radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * hidingHeight, interactionRadius);

        // Draw hiding height reference
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * hidingHeight);
    }

    // private void OnDrawGizmosSelected()
    // {
    //     if (!visualizationEnabled) return;

    //     // Draw filled sphere when selected
    //     Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
    //     Gizmos.DrawSphere(transform.position, interactionRadius);

    //     // Draw discovery state
    //     if (isDiscovered)
    //     {
    //         Gizmos.color = Color.red;
    //         Gizmos.DrawWireSphere(transform.position, interactionRadius);
    //     }
    // }
    public CinemachineCamera GetHidingCamera() => hidingCamera;

    /// <summary>
    /// Check if a player can hide in this spot.
    /// </summary>
    public bool CanHide()
    {
        return !isOccupied && !isDiscovered;
    }

    /// <summary>
    /// Called when player hides in this spot.
    /// </summary>
    public void HidePlayer(GameObject player)
    {
        coll.enabled = false;
        if(rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }
        isOccupied = true;
        hiddenPlayer = player;
        currentDiscoveryProgress = 0f;
        isBeingDiscovered = false;
    }

    /// <summary>
    /// Called when player leaves the hiding spot.
    /// </summary>
    public void UnhidePlayer()
    {
        coll.enabled = true;
        rb.useGravity = true;
        rb.isKinematic = false;
        isOccupied = false;
        hiddenPlayer = null;
        currentDiscoveryProgress = 0f;
        isBeingDiscovered = false;
    }

    /// <summary>
    /// Returns the hidden player if this spot is occupied.
    /// </summary>
    public GameObject GetHiddenPlayer()
    {
        return hiddenPlayer;
    }

    /// <summary>
    /// Check if this spot is currently hiding a player.
    /// </summary>
    public bool HasHiddenPlayer()
    {
        return isOccupied && hiddenPlayer != null;
    }

    /// <summary>
    /// Get the position where the player should be hidden.
    /// </summary>
    public Vector3 GetHidingPosition()
    {
        return transform.localPosition + Vector3.up * hidingHeight;
    }

    /// <summary>
    /// Get the interaction radius for this hiding spot.
    /// </summary>
    public float GetInteractionRadius()
    {
        return interactionRadius;
    }

    /// <summary>
    /// Called by enemy when it spots a hidden player and starts discovering the spot.
    /// </summary>
    public void StartDiscovery(EnemyMovement enemy)
    {
        if (!isBeingDiscovered)
        {
            isBeingDiscovered = true;
            discoveringEnemy = enemy;
            currentDiscoveryProgress = 0f;
        }
    }

    /// <summary>
    /// Called by enemy to advance the discovery progress.
    /// Returns true when discovery is complete.
    /// </summary>
    public bool AdvanceDiscovery(float deltaTime)
    {
        if (!isBeingDiscovered)
            return false;

        currentDiscoveryProgress += deltaTime;
        return currentDiscoveryProgress >= discoveryTime;
    }

    /// <summary>
    /// Fully discover/open the hiding spot, exposing the hidden player.
    /// </summary>
    public void DiscoverSpot()
    {
        isDiscovered = true;
        isBeingDiscovered = false;
        currentDiscoveryProgress = 0f;
    }

    /// <summary>
    /// Check if this spot is currently being discovered.
    /// </summary>
    public bool IsBeingDiscovered()
    {
        return isBeingDiscovered;
    }

    /// <summary>
    /// Check if this spot has been discovered and opened.
    /// </summary>
    public bool IsDiscovered()
    {
        return isDiscovered;
    }

    /// <summary>
    /// Reset the discovery state of this hiding spot.
    /// </summary>
    public void ResetDiscoveryState()
    {
        isDiscovered = false;
        isBeingDiscovered = false;
        currentDiscoveryProgress = 0f;
        discoveringEnemy = null;
    }

    /// <summary>
    /// Get the discovery progress (0-1).
    /// </summary>
    public float GetDiscoveryProgress()
    {
        return Mathf.Clamp01(currentDiscoveryProgress / discoveryTime);
    }

    /// <summary>
    /// Cancel ongoing discovery.
    /// </summary>
    public void CancelDiscovery()
    {
        isBeingDiscovered = false;
        currentDiscoveryProgress = 0f;
        discoveringEnemy = null;
    }
}
