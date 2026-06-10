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
    public bool canRoamAround = false;
    [SerializeField] private Vector3 detectionBoxSize = new Vector3(20f, 5f, 20f);
    [SerializeField] private Vector3 detectionBoxOffset = Vector3.zero;
    [SerializeField] private LayerMask obstacleMask;
    public Vector3 DetectionBoxSize { get => detectionBoxSize; set => detectionBoxSize = value; }
    public Vector3 DetectionBoxOffset { get => detectionBoxOffset; set => detectionBoxOffset = value; }
    private PlayerSightInteraction playerSight;
    private EnemySightDetection sightDetection;
    private Transform playerTransform;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float stoppingDistance = 0.5f;
    private NavMeshAgent navMeshAgent;

    [Header("Catch Animation")]
    [SerializeField] private string catchAnimationName = "Catch";
    private Animator animator;
    private float previousSpeed;

    [Header("Reset")]
    [SerializeField] private float contactThreshold = 1f;
    private PlayerResetManager resetManager;
    private Vector3 originalPosition;

    private bool isMovingTowardPlayer = false;
    private bool isAnimatingCatch = false;
    private bool wasMovingLastFrame = false;
    private bool isReturningToOrigin = false;

    void Start()
    {
        originalPosition = transform.position;
        playerSight = FindAnyObjectByType<PlayerSightInteraction>();
        sightDetection = GetComponent<EnemySightDetection>();
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

        // Check if player can see this enemy (Weeping Angel behavior: moves when NOT observed)
        bool isPlayerLooking = IsPlayerLooking();

        if (!isPlayerLooking)
        {
            if(canRoamAround)
            {
                if(sightDetection == null)
                {
                    Debug.LogWarning("EnemySightDetection component not found on mannequin. Roaming behavior will not function properly.");
                    return;
                }
                // If roaming is enabled, ignore detection box and always move toward player
                if(sightDetection.canSeePlayer)
                {
                    isReturningToOrigin = false;

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
                    // Player out of range - return to original position
                    if (isMovingTowardPlayer && !isAnimatingCatch)
                    {
                        isMovingTowardPlayer = false;
                        OnStoppedMoving?.Invoke();
                    }
                    wasMovingLastFrame = false;
                    isReturningToOrigin = true;
                    ReturnToOriginalPosition();
                }
                return;
            }
            // Player is NOT looking - move toward player like a Weeping Angel
            if (IsPlayerInDetectionBox())
            {
                // In range - moving toward player
                isReturningToOrigin = false;
                
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
                // Player out of range - return to original position
                if (isMovingTowardPlayer && !isAnimatingCatch)
                {
                    isMovingTowardPlayer = false;
                    OnStoppedMoving?.Invoke();
                }
                wasMovingLastFrame = false;
                isReturningToOrigin = true;
                ReturnToOriginalPosition();
            }
        }
        else
        {
            // Player IS looking - freeze in place (Weeping Angel style)
            if (isMovingTowardPlayer)
            {
                OnStoppedMoving?.Invoke();
            }
            isMovingTowardPlayer = false;
            isReturningToOrigin = false;
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

    private bool IsPlayerInDetectionBox()
    {
        if (playerTransform == null)
            return false;

        if(canRoamAround)
        {
            return true; // Ignore detection box and always move toward player
        }

        // Check if player position is within detection box bounds (with offset)
        Vector3 boxCenter = transform.position + detectionBoxOffset;
        Vector3 relativePlayerPos = playerTransform.position - boxCenter;
        
        return Mathf.Abs(relativePlayerPos.x) <= detectionBoxSize.x / 2f &&
               Mathf.Abs(relativePlayerPos.y) <= detectionBoxSize.y / 2f &&
               Mathf.Abs(relativePlayerPos.z) <= detectionBoxSize.z / 2f;
    }

    private bool IsPathObstructed()
    {
        if (playerTransform == null)
            return false;

        // Cast a ray from enemy to player to check for obstructions
        Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // Use obstacleMask to detect obstructions
        if (Physics.Raycast(transform.position, directionToPlayer, distanceToPlayer, obstacleMask))
        {
            return true; // Path is obstructed
        }

        return false; // Path is clear
    }
    private void PauseAnimator()
    {
        if (animator != null)
        {
            previousSpeed = animator.speed;
            animator.speed = 0f;
        }
    }
    private void ResumeAnimator()
    {
        if (animator != null)
        {
            animator.speed = previousSpeed;
        }
    }
    private void MoveTowardPlayer()
    {
        if (playerTransform == null || isAnimatingCatch)
            return;

        // Check if path to player is obstructed
        if (IsPathObstructed())
        {
            StopMovement();
            return; // Cannot move - path is blocked
        }

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // Only move if not at stopping distance
        if (distanceToPlayer > stoppingDistance)
        {
            // Use NavMeshAgent to set destination toward the player
            ResumeAnimator();
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
        PauseAnimator();
        if (navMeshAgent != null)
        {
            navMeshAgent.velocity = Vector3.zero;
            navMeshAgent.ResetPath();
        }
    }

    private void ReturnToOriginalPosition()
    {
        if (isAnimatingCatch)
            return;

        float distanceToOrigin = Vector3.Distance(transform.position, originalPosition);

        // If not at original position, navigate back
        if (distanceToOrigin > stoppingDistance)
        {
            navMeshAgent.SetDestination(originalPosition);
        }
        else
        {
            // Reached original position
            isReturningToOrigin = false;
            StopMovement();
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
        // Draw detection box with offset
        if (isMovingTowardPlayer)
        {
            Gizmos.color = Color.red;
        }
        else
        {
            Gizmos.color = Color.yellow;
        }

        if(canRoamAround)
        {
            
        }
        Vector3 boxCenter = transform.position + detectionBoxOffset;
        Gizmos.DrawWireCube(boxCenter, detectionBoxSize);
        
        // Draw offset indicator line
        if (detectionBoxOffset != Vector3.zero)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, boxCenter);
        }

        // Draw line to player if in range (for debugging obstruction)
        if (Application.isPlaying && playerTransform != null)
        {
            if (IsPathObstructed())
            {
                Gizmos.color = Color.red; // Path blocked
            }
            else
            {
                Gizmos.color = Color.green; // Path clear
            }
            Gizmos.DrawLine(transform.position, playerTransform.position);
        }

        // Draw original position
        if (Application.isPlaying)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(originalPosition, 0.5f);
        }
    }
}
