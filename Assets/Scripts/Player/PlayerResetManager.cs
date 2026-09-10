using UnityEngine;

/// <summary>
/// Manages player reset to starting position
/// Triggered when enemy reaches player or player strikes wrong enemy
/// </summary>
public class PlayerResetManager : MonoBehaviour
{
    private PlayerController playerController;
    [SerializeField] private float resetDelay = 1f;
    [SerializeField] private Waypoint checkpoint;
    private float resetTimer = 0f;
    private bool shouldReset = false;
    private string resetReason = "";

    void Start()
    {
        playerController = GetComponent<PlayerController>();
        if (playerController == null)
        {
            playerController = FindAnyObjectByType<PlayerController>();
        }
    }

    void Update()
    {
        if (shouldReset)
        {
            resetTimer -= Time.deltaTime;
            if (resetTimer <= 0f)
            {
                ExecuteReset();
                shouldReset = false;
            }
        }
    }

    /// <summary>
    /// Trigger player reset with a delay
    /// </summary>
    public void ResetPlayer(string reason = "Reset triggered")
    {
        if (!shouldReset) // Prevent multiple simultaneous resets
        {
            shouldReset = true;
            resetTimer = resetDelay;
            resetReason = reason;
            Debug.Log($"Player reset scheduled: {reason}");
        }
    }

    private void ExecuteReset()
    {
        if (playerController != null)
        {
            Debug.Log($"Executing reset: {resetReason}");
            StartCoroutine(playerController.ResetToStartingPosition(checkpoint != null ? checkpoint.position : default));
        }
        else
        {
            Debug.LogError("PlayerResetManager: PlayerSightInteraction not found!");
        }
    }

    /// <summary>
    /// Cancel any pending reset
    /// </summary>
    public void CancelReset()
    {
        shouldReset = false;
        resetTimer = 0f;
    }

    public void SetCheckpoint(Waypoint newCheckpoint)
    {
        checkpoint = newCheckpoint;
    }
}
