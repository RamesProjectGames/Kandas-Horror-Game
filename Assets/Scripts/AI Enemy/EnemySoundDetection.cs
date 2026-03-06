using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class EnemySoundDetection : MonoBehaviour
{
    [Header("Detection Range")]
    [SerializeField] private float maxHearingRange = 25f; // The fixed "Ear" size
    [SerializeField] private LayerMask playerLayer;
    
    [Header("Sensitivity Settings")]
    [SerializeField] private float minMicThreshold = 0.02f; // Silent (Enemy is touching the cupboard)
    [SerializeField] private float maxMicThreshold = 0.80f; // Loud (Enemy is at the edge of hearing)
    
    [Header("References")]
    [SerializeField] private MicrophoneManager micManager;
    [SerializeField] private PlayerHiding playerHiding;
    [SerializeField] private EnemyMovement enemyMovement;
    [SerializeField] private IEnemySoundReactive enemy; // Your interface for alerting the AI

    private void Update()
    {
        // Only process if the player is actually hiding
        if (playerHiding != null && playerHiding.IsHiding())
        {
            CheckMicrophoneDetection();
        }
    }

    private void CheckMicrophoneDetection()
    {
        if (playerHiding == null || !playerHiding.IsHiding()) return;

        Collider[] hits = Physics.OverlapSphere(transform.position, maxHearingRange, playerLayer);
        if (hits.Length > 0)
        {
            float distance = Vector3.Distance(transform.position, hits[0].transform.position);
            float currentThreshold = Mathf.Lerp(minMicThreshold, maxMicThreshold, distance / maxHearingRange);
            float loudness = micManager.GetMicrophoneLoudness();

            // Use the same buffer logic as your UI
            float warningBuffer = 0.15f;

            if (loudness >= currentThreshold)
            {
                // DANGER: Kill the player
                enemyMovement.TriggerKillPlayer(playerHiding);
            }
            else if (loudness >= (currentThreshold - warningBuffer))
            {
                // WARNING: Go inspect the hiding spot
                enemyMovement.InvestigatePlayerSpot(playerHiding.GetCurrentHidingSpot());
            }
        }
    }

    // This is for your UI to pull the current "Danger Line"
    public float GetCurrentRequiredThreshold()
    {
        if (playerHiding == null || !playerHiding.IsHiding()) return maxMicThreshold;
        
        float dist = Vector3.Distance(transform.position, playerHiding.transform.position);
        return Mathf.Clamp(Mathf.Lerp(minMicThreshold, maxMicThreshold, dist / maxHearingRange), 0, 1);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, maxHearingRange);
    }
}

/// <summary>
/// Interface for enemies to react to detected sound
/// </summary>
public interface IEnemySoundReactive
{
    void OnSoundDetected(Vector3 soundSource, float soundLevel, float detectionRadius);
}
