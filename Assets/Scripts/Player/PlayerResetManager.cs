using UnityEngine;

/// <summary>
/// Manages player reset to starting position
/// Triggered when enemy reaches player or player strikes wrong enemy
/// </summary>
public class PlayerResetManager : MonoBehaviour
{
    private PlayerSightInteraction playerSight;
    [SerializeField] private float resetDelay = 1f;
    [SerializeField] private Transform startingPositionTransform;
    private float resetTimer = 0f;
    private bool shouldReset = false;
    private string resetReason = "";

    void Start()
    {
        playerSight = GetComponent<PlayerSightInteraction>();
        if (playerSight == null)
        {
            playerSight = FindAnyObjectByType<PlayerSightInteraction>();
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
        if (playerSight != null)
        {
            Debug.Log($"Executing reset: {resetReason}");
            playerSight.ResetToStartingPosition(startingPositionTransform != null ? startingPositionTransform.position : default);
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
}
