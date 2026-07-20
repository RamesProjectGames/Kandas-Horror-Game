using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Type 1 Mannequin Enemy: Moves toward player when not visible
/// Plays catch animation when reaching player, then resets to original position
/// Event-based system for interaction tracking
/// </summary>

public class MannequinDemoGame : MovableObjects
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
    [SerializeField] private List<string> idleAnimations = new List<string>();
    [SerializeField] private Animator animator;
    [SerializeField] private CinemachineCamera chokeCamera;
    private CinemachineCamera playerCamera;
    private float previousSpeed;

    [Header("Reset")]
    [SerializeField] private float contactThreshold = 1f;
    private PlayerResetManager resetManager;
    private Vector3 originalPosition;
    public Vector3 playerResetPosition;

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
        resetManager = FindAnyObjectByType<PlayerResetManager>();

        if (navMeshAgent == null)
        {
            navMeshAgent = gameObject.AddComponent<NavMeshAgent>();
        }

        // Configure NavMeshAgent
        navMeshAgent.speed = moveSpeed;
        navMeshAgent.stoppingDistance = stoppingDistance;
        ReturnIdleAnimation();

    }

    void Update()
    {
        if (playerSight == null || playerTransform == null || !ObjectiveManager.Instance.isCompleted("NurseReport") || SettingManager.Instance.isPaused || SettingManager.Instance.gameOver)
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
        return playerSight.GetVisibleEnemies().Contains(transform);
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

        if (Physics.Raycast(transform.position, directionToPlayer, out RaycastHit hit, distanceToPlayer, obstacleMask, QueryTriggerInteraction.Ignore))
        {
            // Ignore the player itself and the mannequin's own colliders
            if (hit.transform == playerTransform || hit.transform == transform)
            {
                return false;
            }
            return true;
        }

        return false; // Path is clear
    }
    private void PauseAnimator()
    {
        if (animator != null)
        {
            previousSpeed = animator.speed > 0f ? animator.speed : 1f;
            animator.speed = 0f;
        }
    }
    private void ResumeAnimator()
    {
        if (animator != null)
        {
            animator.SetFloat("MoveBlend", 1);
            animator.speed = previousSpeed > 0f ? previousSpeed : 1f;
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

        if (navMeshAgent != null)
        {
            navMeshAgent.isStopped = false;
            navMeshAgent.SetDestination(playerTransform.position);
        }

        if (animator != null)
        {
            animator.SetFloat("MoveBlend", 1);
        }
    }

    private void StopMovement()
    {
        if (navMeshAgent != null)
        {
            navMeshAgent.isStopped = true;
            navMeshAgent.velocity = Vector3.zero;
            navMeshAgent.ResetPath();
        }

        if (animator != null && !isAnimatingCatch)
        {
            animator.SetFloat("MoveBlend", 0);
        }
    }
    private void ReturnIdleAnimation()
    {
        if (animator != null)
        {
            animator.SetFloat("MoveBlend", 0);

            if (idleAnimations.Count > 0)
            {
                int randomIdle = Random.Range(0, idleAnimations.Count);
                animator.SetFloat("SelectedPose", randomIdle);
            }

            animator.SetBool("Capture", false);
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
            Teleport(originalPosition);
            if (animator != null)
            {
                animator.SetFloat("MoveBlend", 0);
            }
        }
        else
        {
            // Reached original position
            isReturningToOrigin = false;
            ReturnIdleAnimation();
            StopMovement();
        }
    }

    public void PlayCatchAnimation()
    {
        isAnimatingCatch = true;
        StopMovement();

        // Trigger catch animation start event
        OnCatchAnimationStart?.Invoke();

        if (animator != null)
        {
            animator.SetFloat("MoveBlend", 0);
            animator.SetBool("Capture", true);
        }

        // Start coroutine to wait for animation to complete, then reset position
        // StartCoroutine(WaitForCatchAnimationAndReset());
    }
    public void SwitchPlayerPerspective()
    {
        playerCamera = CameraManager.currentActiveCamera;
        CameraManager.SwitchCamera(chokeCamera);
    }
    public void TriggerResetDoll()
    {        
        // Reset mannequin to original position
        transform.position = originalPosition;
        
        // Reset NavMeshAgent
        if (navMeshAgent != null)
        {
            navMeshAgent.isStopped = true;
            navMeshAgent.velocity = Vector3.zero;
            navMeshAgent.ResetPath();
        }
        
        isAnimatingCatch = false;
        wasMovingLastFrame = false;

        if (animator != null)
        {
            animator.SetBool("Capture", false);
            animator.SetFloat("MoveBlend", 0);
        }
        
        // Trigger catch animation complete event
        OnCatchAnimationComplete?.Invoke();
        
        // Trigger player reset
        TriggerPlayerReset();
    }
    private void TriggerPlayerReset()
    {
        CameraManager.SwitchCamera(playerCamera);
        if(playerResetPosition == Vector3.zero)
        {
            resetManager.ResetPlayer("Mannequin caught the player");
            return;
        }
        var playerController = FindAnyObjectByType<PlayerController>();
        if (playerController != null)
        {
            StartCoroutine(playerController.Teleport(playerResetPosition));
        }
    }
    #region Agent (auto) Movement
    public override IEnumerator Teleport(Vector3 pos)
    {
        agent.enabled = false;
        transform.position = pos;
        //agent.Warp(pos);
        yield return new WaitForSeconds(.1f);
        agent.enabled = true;
        agent.ResetPath();
    }
    public override IEnumerator Rotate(float yrot)
    {
        Quaternion targetRotation = Quaternion.Euler(0, yrot, 0);
        while (Quaternion.Angle(transform.rotation, targetRotation) > 5f)
        {
            // Putar secara bertahap dari rotasi saat ini ke rotasi target
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 2.0f);
            yield return null;
        }
        // Snap to exact target
        transform.rotation = targetRotation;
    }

    public override IEnumerator Move(Vector3 pos, float speed = 150f)
    {
        agent.SetDestination(pos);
        agent.isStopped = false;
        yield return new WaitForEndOfFrame();
    }

    public bool CanUseAgent()
    {
        return agent != null && agent.enabled && agent.isOnNavMesh;
    }
    #endregion
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

        if(!canRoamAround)
        {
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
        }

        // Draw original position
        if (Application.isPlaying)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(originalPosition, 0.5f);
        }

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, contactThreshold);
    }
}
