using UnityEngine;
using System.Collections.Generic;

public class EnemyMovement : MonoBehaviour, IAudioRadiusListener
{
    [SerializeField] Waypoint[] point;
    [SerializeField] int idxPoint = 0;
    [SerializeField] EnemySightDetection fov;
    bool detectedSound;
    public Vector3 soundSource;
    public float speed = 3f;
    public float pursueSpeed = 6f;
    public float idleTime = 5f, currIdleTime;
    bool comeback = false;
    bool reachPoint = false;

    [Header("Hiding Spot Detection")]
    [SerializeField] private float hidingSpotDetectionRadius = 15f;
    [SerializeField] private LayerMask hidingSpotLayer;
    private HidingSpot targetHidingSpot;
    private bool isDiscoveringSpot = false;
    private int initialPatrolIndex = 0; // Store initial patrol point to return to

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        initialPatrolIndex = idxPoint;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 posTarget = new Vector3(point[idxPoint].position.x, this.transform.position.y, point[idxPoint].position.z);

        Vector3 posPlayer = new Vector3(fov.player.transform.position.x, this.transform.position.y, fov.player.transform.position.z);
        
        // If player was spotted while hiding, prioritize discovering the hiding spot
        if (isDiscoveringSpot && targetHidingSpot != null)
        {
            HandleHidingSpotDiscovery();
        }
        else if (!fov.canSeePlayer)
        {
            if(!reachPoint)
            {
                if (detectedSound)
                {
                    if (Vector3.Distance(this.transform.position, soundSource) > 2f)
                    {
                        this.transform.position = Vector3.MoveTowards(this.transform.position, soundSource, pursueSpeed * Time.deltaTime);
                        Vector3 posPoint = soundSource - this.transform.position;
                        this.transform.rotation = Quaternion.LookRotation(posPoint);
                    }
                    else
                    {
                        reachPoint = true;
                        currIdleTime = idleTime;
                    }
                }
                else
                {
                    // Check for hidden players in nearby hiding spots
                    CheckForHiddenPlayers();

                    if (Vector3.Distance(this.transform.position, posTarget) > 0.1f)
                    {
                        this.transform.position = Vector3.MoveTowards(this.transform.position, posTarget, speed * Time.deltaTime);
                        Vector3 posPoint = posTarget - this.transform.position;
                        this.transform.rotation = Quaternion.LookRotation(posPoint);
                    }
                    else
                    {
                        // Debug.Log("Player arrived at waypoint, updating next waypoint");
                        if (point[idxPoint].endPosition)
                        {
                            reachPoint = true;
                            currIdleTime = idleTime;
                        }
                        idxPoint++;
                        idxPoint = idxPoint % point.Length;
                    }
                }
            }
            else
            {
                if (detectedSound)
                {
                    //ObserveSurroundings();
                }
                else
                {
                    currIdleTime -= Time.deltaTime;
                    if (currIdleTime <= 0)
                        reachPoint = false;

                }
            }
        }
        else
        {
            // Player is visible - check if they're hiding in a spot
            PlayerHiding playerHiding = fov.player.GetComponent<PlayerHiding>();
            if (playerHiding != null && playerHiding.IsHiding())
            {
                // Player was spotted while hiding - start discovering their hiding spot
                HidingSpot hidingSpot = playerHiding.GetCurrentHidingSpot();
                if (hidingSpot != null)
                {
                    StartDiscoveringHidingSpot(hidingSpot);
                }
            }
            else
            {
                // Normal pursuit behavior
                this.transform.position = Vector3.MoveTowards(this.transform.position, posPlayer, pursueSpeed * Time.deltaTime);
                Vector3 posPoint = posPlayer - this.transform.position;
                this.transform.rotation = Quaternion.LookRotation(posPoint);
            }
        }
    }

    /// <summary>
    /// Check for players hidden in nearby hiding spots during patrol.
    /// If no player is spotted yet, enemy passes by the spot.
    /// </summary>
    private void CheckForHiddenPlayers()
    {
        Collider[] hidingSpots = Physics.OverlapSphere(transform.position, hidingSpotDetectionRadius, hidingSpotLayer);

        foreach (Collider col in hidingSpots)
        {
            HidingSpot spot = col.GetComponent<HidingSpot>();
            if (spot != null && spot.HasHiddenPlayer())
            {
                // There's a hidden player here, but we haven't spotted them yet
                // Just pass by naturally during patrol
                // The player remains hidden if they stay still
            }
        }
    }

    /// <summary>
    /// Start the process of discovering and opening a hiding spot where the player is spotted.
    /// </summary>
    private void StartDiscoveringHidingSpot(HidingSpot hidingSpot)
    {
        if (isDiscoveringSpot)
            return; // Already discovering another spot

        targetHidingSpot = hidingSpot;
        isDiscoveringSpot = true;
        hidingSpot.StartDiscovery(this);
        detectedSound = false;
        reachPoint = false;
    }

    /// <summary>
    /// Handle the discovery process of opening a hiding spot.
    /// </summary>
    private void HandleHidingSpotDiscovery()
    {
        if (targetHidingSpot == null)
        {
            isDiscoveringSpot = false;
            return;
        }

        Vector3 hidingSpotPos = new Vector3(targetHidingSpot.transform.position.x, this.transform.position.y, targetHidingSpot.transform.position.z);
        float distanceToSpot = Vector3.Distance(this.transform.position, hidingSpotPos);

        // Move to the hiding spot
        if (distanceToSpot > 0.5f)
        {
            this.transform.position = Vector3.MoveTowards(this.transform.position, hidingSpotPos, pursueSpeed * Time.deltaTime);
            Vector3 posPoint = hidingSpotPos - this.transform.position;
            this.transform.rotation = Quaternion.LookRotation(posPoint);
        }

        // Advance discovery progress
        if (targetHidingSpot.AdvanceDiscovery(Time.deltaTime))
        {
            // Discovery complete - open the hiding spot
            targetHidingSpot.DiscoverSpot();
            
            // Force the hidden player to unhide
            GameObject hiddenPlayer = targetHidingSpot.GetHiddenPlayer();
            if (hiddenPlayer != null)
            {
                PlayerHiding playerHiding = hiddenPlayer.GetComponent<PlayerHiding>();
                if (playerHiding != null)
                {
                    playerHiding.ForceUnhide();
                    Debug.Log("Enemy discovered the hiding spot and exposed the player!");
                }
            }

            // Now proceed to attack the exposed player
            isDiscoveringSpot = false;
            targetHidingSpot = null;
        }
    }

    /// <summary>
    /// Called when enemy gives up pursuit and returns to patrolling.
    /// </summary>
    public void ReturnToPatrol()
    {
        isDiscoveringSpot = false;
        targetHidingSpot = null;
        idxPoint = initialPatrolIndex;
        reachPoint = false;
        detectedSound = false;
    }

    public void OnEnterAudioRadius(GameObject audioSource)
    {
        detectedSound = true;
        soundSource = audioSource.gameObject.transform.position;
    }

    public void OnExitAudioRadius(GameObject audioSource)
    {
        //throw new System.NotImplementedException();
    }
}
