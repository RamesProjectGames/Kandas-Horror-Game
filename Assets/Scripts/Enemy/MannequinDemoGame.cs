using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Type 1 Mannequin Enemy: Moves toward player when not visible
/// Plays catch animation when reaching player, then resets to original position
/// Event-based system for interaction tracking
/// </summary>
public class MannequinDemoGame : MonoBehaviour
{
    // ===== EVENTS =====
    public delegate void MannequinEvent();
    public delegate void PlayerDetectionEvent(Transform player);
    
    public event MannequinEvent OnPlayerDetected;
    public event MannequinEvent OnStartMoving;
    public event MannequinEvent OnStoppedMoving;
    public event MannequinEvent OnCatchAnimationStart;
    public event MannequinEvent OnCatchAnimationComplete;
    [Header("Detection")]
    [SerializeField] private float detectionRange = 20f;
    private PlayerSightInteraction playerSight;
    private Transform playerTransform;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float stoppingDistance = 0.5f;
    private NavMeshAgent navMeshAgent;

    [Header("Catch Animation")]
    [SerializeField] private string catchAnimationName = "Catch";
    private Animator animator;

    [Header("Reset")]
    [SerializeField] private float contactThreshold = 1f;
    private PlayerResetManager resetManager;
    private Vector3 originalPosition;

    private bool isMovingTowardPlayer = false;
    private bool isAnimatingCatch = false;
    private bool wasMovingLastFrame = false;

    void Start()
    {
        originalPosition = transform.position;
        playerSight = FindAnyObjectByType<PlayerSightInteraction>();
        playerTransform = playerSight?.transform;
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        resetManager = FindAnyObjectByType<PlayerResetManager>();

        if (navMeshAgent == null)
        {
            navMeshAgent = gameObject.AddComponent<NavMeshAgent>();
        }

        // Configure NavMeshAgent
        navMeshAgent.speed = moveSpeed;
        navMeshAgent.stoppingDistance = stoppingDistance;
    }

    void Update()
    {
        if (playerSight == null || playerTransform == null)
            return;

        // Check if player can see this enemy
        bool isPlayerLooking = IsPlayerLooking();

        if (!isPlayerLooking)
        {
            // Check if player is within detection range
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            if (distanceToPlayer <= detectionRange)
            {
                // Trigger player detected event
                if (!isMovingTowardPlayer)
                {
                    OnPlayerDetected?.Invoke();
                }

                isMovingTowardPlayer = true;
                
                // Trigger start moving event on state change
                if (!wasMovingLastFrame)
                {
                    OnStartMoving?.Invoke();
                    wasMovingLastFrame = true;
                }
                
                MoveTowardPlayer();
            }
            else
            {
                // Player out of range
                if (isMovingTowardPlayer && !isAnimatingCatch)
                {
                    isMovingTowardPlayer = false;
                    StopMovement();
                }
            }
        }
        else
        {
            // Stop moving when player sees this enemy
            if (isMovingTowardPlayer)
            {
                OnStoppedMoving?.Invoke();
            }
            isMovingTowardPlayer = false;
            wasMovingLastFrame = false;
            StopMovement();
        }
    }

    private bool IsPlayerLooking()
    {
        // Check if this enemy is in the player's visible enemies list
        foreach (Transform visibleEnemy in playerSight.GetVisibleEnemies())
        {
            if (visibleEnemy == transform)
            {
                return true;
            }
        }
        return false;
    }

    private void MoveTowardPlayer()
    {
        if (playerTransform == null || isAnimatingCatch)
            return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // Only move if not at stopping distance
        if (distanceToPlayer > stoppingDistance)
        {
            // Use NavMeshAgent to set destination toward the player
            navMeshAgent.SetDestination(playerTransform.position);
        }
        else
        {
            // Reached player - start catch animation
            PlayCatchAnimation();
        }
    }

    private void StopMovement()
    {
        if (navMeshAgent != null)
        {
            navMeshAgent.velocity = Vector3.zero;
            navMeshAgent.ResetPath();
        }
    }

    private void PlayCatchAnimation()
    {
        isAnimatingCatch = true;
        StopMovement();

        // Trigger catch animation start event
        OnCatchAnimationStart?.Invoke();

        if (animator != null)
        {
            animator.SetTrigger(catchAnimationName);
        }

        // Start coroutine to wait for animation to complete, then reset position
        StartCoroutine(WaitForCatchAnimationAndReset());
    }

    private System.Collections.IEnumerator WaitForCatchAnimationAndReset()
    {
        // Wait for animation to start
        yield return new WaitForSeconds(0.1f);

        if (animator != null)
        {
            // Get the current animation state
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            
            // Wait for the animation to complete
            yield return new WaitForSeconds(stateInfo.length);
        }
        else
        {
            // Fallback delay if no animator
            yield return new WaitForSeconds(2f);
        }

        // Reset mannequin to original position
        transform.position = originalPosition;
        
        // Reset NavMeshAgent
        if (navMeshAgent != null)
        {
            navMeshAgent.velocity = Vector3.zero;
            navMeshAgent.ResetPath();
        }
        
        isAnimatingCatch = false;
        wasMovingLastFrame = false;
        
        // Trigger catch animation complete event
        OnCatchAnimationComplete?.Invoke();
        
        // Trigger player reset
        TriggerPlayerReset();
    }

    private void TriggerPlayerReset()
    {
        if (resetManager != null)
        {
            resetManager.ResetPlayer("Mannequin Type 1 caught player!");
        }
    }

    private void OnDrawGizmos()
    {
        if (isMovingTowardPlayer)
        {
            Gizmos.color = Color.red;
        }
        else
        {
            Gizmos.color = Color.yellow;
        }
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Draw original position
        if (Application.isPlaying)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(originalPosition, 0.5f);
        }
    }
}
