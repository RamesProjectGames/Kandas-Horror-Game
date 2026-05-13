using Dialogue;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Windows;

public class EnemyMovement : MovableObjects, IAudioRadiusListener
{
    [SerializeField] Waypoint[] point;
    [SerializeField] int idxPoint = 0;
    [SerializeField] EnemySightDetection fov;
    [SerializeField] Animator animator;
    bool detectedSound;
    public Vector3 soundSource;
    public float speed = 3f;
    public float pursueSpeed = 6f;
    public float idleTime = 5f, currIdleTime;
    private bool isKilling = false;
    private bool isStunned = false;

    [Header("Hiding Spot Detection")]
    [SerializeField] private float hidingSpotDetectionRadius = 15f;
    [SerializeField] private LayerMask hidingSpotLayer;
    private HidingSpot targetHidingSpot;
    private bool isDiscoveringSpot = false;

    // Pause tracking: used to detect pause/unpause transitions
    private bool wasPausedLastFrame = false;

    [Header("Footsteps")]
    // reference to the centralized sound manager – typically on the same GameObject
    public FootstepsSoundManager footstepManager;
    // base step distance at normal walking speed; adjusted dynamically based on moveSpd
    public float baseStepDistance = 2f;
    private Vector3 _lastFootstepPosition;
    private float _footstepDistanceAccum;
    private Vector2 Velocity;
    private Vector2 smoothDeltaPosition;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null) agent = gameObject.AddComponent<NavMeshAgent>();

        // Configure NavMeshAgent for patrol/search
        agent.updateRotation = true;
        agent.angularSpeed = 600f;
        agent.acceleration = 8f;
        agent.stoppingDistance = 0.1f;
        agent.isStopped = false;

        currIdleTime = idleTime;

        if (point != null && point.Length > 0)
        {
            agent.SetDestination(point[idxPoint].position);
        }

        // footsteps helper
        if (footstepManager == null)
            footstepManager = GetComponent<FootstepsSoundManager>();
        _lastFootstepPosition = transform.position;

        if(animator != null)
        {
            animator.applyRootMotion = true;
            agent.updatePosition = false;
            agent.updateRotation = true;
        }
    }
    void OnAnimatorMove()
    {
        if(animator == null) return;
        Vector3 rootPosition = animator.rootPosition;
        rootPosition.y = agent.nextPosition.y;
        transform.position = rootPosition;
        agent.nextPosition = rootPosition;
    }

    // Update is called once per frame
    void Update()
    {
        if (HandlePauseState()) return;

        if(isStunned)
        {
            currIdleTime -= Time.deltaTime;
            if (currIdleTime <= 0)
            {
                // Transition from Idle to Moving
                agent.isStopped = false;
                agent.speed = speed;

                isStunned = false;
            }
            return; // Exit early while idling
        }

        // 1. Check if player is currently hiding
        PlayerHiding checkHiding = fov.player.GetComponent<PlayerHiding>();
        bool isPlayerHiding = checkHiding != null && checkHiding.IsHiding();

        // 2. State Logic
        if (isDiscoveringSpot && targetHidingSpot != null)
        {
            HandleHidingSpotDiscovery();
        }
        else if (fov.canSeePlayer && !isPlayerHiding)
        {
            // PURSUIT: Follow player directly without SetDestination
            isDiscoveringSpot = false; // Ensure we exit discovery if we see the player again
            Vector3 directionToPlayer = (GetValidNavMeshPosition(fov.player.transform.position) - transform.position).normalized;
            transform.LookAt(new Vector3(fov.player.transform.position.x, transform.position.y, fov.player.transform.position.z));
            detectedSound = false; 
            agent.isStopped = true;            
            // Move directly towards player
            transform.position += directionToPlayer * pursueSpeed * Time.deltaTime;
        }
        else if (isPlayerHiding && fov.PlayerWasSpottedWhileHiding && !isDiscoveringSpot)
        {
            // TRANSITION TO DISCOVERY: Player hid while in view
            HidingSpot spot = checkHiding.GetCurrentHidingSpot();
            if (spot != null) StartDiscoveringHidingSpot(spot);
        }
        else
        {
            // PATROL OR INVESTIGATION
            HandleNavigation();
        }
    }

    private void FixedUpdate()
    {
        if (Application.isPlaying && (SettingManager.Instance.isPaused || DialogueSystem.Instance.isRunningConvo))
            return;
        // accumulate distance travelled this frame and trigger a step when we've covered enough ground
        if (footstepManager != null)
        {
            // scale step distance inversely with speed: faster movement = more frequent steps
            // use speed (normal walk speed, ~150) as baseline
            float effectiveStepDistance = baseStepDistance;

            if(agent.enabled && !agent.isStopped)
            {
                effectiveStepDistance = baseStepDistance * speed;
            }
            else
            {
                effectiveStepDistance = baseStepDistance;
            }
            

            float dist = Vector3.Distance(transform.position, _lastFootstepPosition);
            _footstepDistanceAccum += dist;
            if (_footstepDistanceAccum >= effectiveStepDistance)
            {
                _footstepDistanceAccum -= effectiveStepDistance;
                footstepManager.PlayFootstep();
            }
        }
        else if (footstepManager != null)
        {
            footstepManager.StopFootstep();
        }
        _lastFootstepPosition = transform.position;
    }
    private bool HandlePauseState()
    {
        bool isPaused = SettingManager.Instance.isPaused || DialogueSystem.Instance.isRunningConvo;
        if (isPaused)
        {
            if (!wasPausedLastFrame && agent != null) agent.enabled = false;
            wasPausedLastFrame = true;
            return true;
        }
        if (wasPausedLastFrame)
        {
            if (agent != null) agent.enabled = true;
            wasPausedLastFrame = false;
            // Force recalculate destination on unpause
            if (detectedSound) agent.SetDestination(soundSource);
            else agent.SetDestination(point[idxPoint].position);
        }
        return false;
    }
    private void FinishSoundInvestigation()
    {
        detectedSound = false; // Important: Reset so the next throw can be detected
        isDiscoveringSpot = false; 
        isKilling = false;
        agent.isStopped = true;
        currIdleTime = idleTime; // Wait at the spot to "look around"
        
        // Queue up the next patrol point so it's ready when idle ends
        if (point.Length > 0)
        {
            agent.SetDestination(point[idxPoint].position);
        }
    }
    public void TriggerKillPlayer(PlayerHiding player)
    {
        if (isKilling) return;
        
        isKilling = true;
        isDiscoveringSpot = false;
        StopAllCoroutines();

        StartCoroutine(KillRoutine(player));
    }

    private IEnumerator KillRoutine(PlayerHiding player)
    {
        agent.isStopped = false;
        agent.speed = pursueSpeed;
        
        // Move to the player's exact position
        while (Vector3.Distance(transform.position, player.transform.position) > 1.2f)
        {
            agent.SetDestination(GetValidNavMeshPosition(player.transform.position));
            yield return null;
        }

        agent.isStopped = true;
        
        // Play Kill Animation
        // anim.SetTrigger("Attack"); 
        
        // Force the player out of the cupboard so they are visible during death
        player.ForceUnhide();

        // Rotate monster to face player for the kill
        transform.LookAt(new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z));

        yield return new WaitForSeconds(1.5f);
        
        // Call your Game Over / Scene Reload logic here
        Debug.Log("GAME OVER: Player Eaten");
    }
    public void InvestigatePlayerSpot(HidingSpot spot)
    {
        if (isDiscoveringSpot || isKilling || spot == null) return;

        isDiscoveringSpot = true;
        targetHidingSpot = spot;
        agent.isStopped = false;
        agent.speed = pursueSpeed;
        agent.SetDestination(GetValidNavMeshPosition(spot.transform.position));
        
        // Debug.Log("Enemy is suspicious of a hiding spot...");
    }
    private void HandleNavigation()
    {
        // If the agent is stopped, it means we are in the "Idle" phase
        if (agent.isStopped)
        {
            currIdleTime -= Time.deltaTime;
            if (currIdleTime <= 0)
            {
                // Transition from Idle to Moving
                agent.isStopped = false;
                agent.speed = speed;
            }
            return; // Exit early while idling
        }
        // Only proceed if agent has finished calculating and reached destination
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            if (detectedSound || isDiscoveringSpot || isKilling)
            {
                // Arrived at the noise source
                FinishSoundInvestigation();
            }
            else
            {
                if (point[idxPoint].faceTowards != null)
                {
                    Vector3 targetPos = point[idxPoint].faceTowards.position;
                    targetPos.y = transform.position.y;
                    Quaternion targetRotation = Quaternion.LookRotation(targetPos - transform.position);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, speed * Time.deltaTime);

                    if (Quaternion.Angle(transform.rotation, targetRotation) <= 5f)
                    {
                        currIdleTime = point[idxPoint].endPosition ? idleTime : 0.5f;
                        idxPoint = ++idxPoint % point.Length;
                        agent.SetDestination(point[idxPoint].position);
                        agent.isStopped = true;
                    }
                }
                else
                {
                    currIdleTime = point[idxPoint].endPosition ? idleTime : 0.5f;
                    idxPoint = ++idxPoint % point.Length;
                    agent.SetDestination(point[idxPoint].position);
                    agent.isStopped = true;
                }
                SynchronizeAnimatorAndAgent();
            }
        }
    }
    public void SynchronizeAnimatorAndAgent()
    {
        if(animator == null) return;

        Vector3 worldDeltaPosition = agent.nextPosition - transform.position;
        worldDeltaPosition.y = 0f;

        float dx = Vector3.Dot(transform.right, worldDeltaPosition);
        float dy = Vector3.Dot(transform.forward, worldDeltaPosition);
        Vector2 deltaPosition = new Vector2(dx, dy);

        float smooth = Mathf.Min(1, Time.deltaTime / .1f);
        smoothDeltaPosition = Vector2.Lerp(smoothDeltaPosition, deltaPosition, smooth);

        Velocity = smoothDeltaPosition / Time.deltaTime;
        if(agent.remainingDistance <= agent.stoppingDistance)
        {
            Velocity = Vector2.Lerp(Velocity, 
            Vector2.zero, 
            agent.remainingDistance/ agent.stoppingDistance
            );
        }

        bool shouldMove = Velocity.magnitude > 0.5f 
            && agent.remainingDistance > agent.stoppingDistance;


        animator.SetBool("move", shouldMove);
        animator.SetFloat("velocity", Velocity.magnitude);
    }

    #region Player Hiding
    /// <summary>
    /// Start the process of discovering and opening a hiding spot where the player is spotted.
    /// </summary>
    private void StartDiscoveringHidingSpot(HidingSpot hidingSpot)
    {
        if (isDiscoveringSpot) return;
        targetHidingSpot = hidingSpot;
        isDiscoveringSpot = true;
        hidingSpot.StartDiscovery(this);
        detectedSound = false;
    }
    
    /// <summary>
    /// Handle the discovery process of opening a hiding spot.
    /// </summary>
    private void HandleHidingSpotDiscovery()
    {
        if (targetHidingSpot == null) { isDiscoveringSpot = false; return; }

        agent.isStopped = false;
        agent.speed = pursueSpeed;
        agent.SetDestination(GetValidNavMeshPosition(targetHidingSpot.transform.position));

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            if (targetHidingSpot.AdvanceDiscovery(Time.deltaTime))
            {
                targetHidingSpot.DiscoverSpot();
                GameObject player = targetHidingSpot.GetHiddenPlayer();
                if (player != null) player.GetComponent<PlayerHiding>().ForceUnhide();
                
                isDiscoveringSpot = false;
                targetHidingSpot = null;
            }
        }
    }
    #endregion

    #region Agent Movement

    public IEnumerator FacePlayer()
    {
        Vector3 targetPos = GameObject.FindGameObjectWithTag("Player").transform.position;
        targetPos.y = transform.position.y;
        Quaternion targetRotation = Quaternion.LookRotation(targetPos - transform.position);

        while (Quaternion.Angle(transform.rotation, targetRotation) >= 1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, speed * 3 * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }
    }

    public override IEnumerator Teleport(Vector3 pos)
    {
        agent.enabled = false;
        transform.position = pos;
        //agent.Warp(pos);
        yield return new WaitForSeconds(.1f);
        agent.enabled = true;
        StartCoroutine(FacePlayer());
        agent.ResetPath();
        ReturnToPatrol();
        yield return new WaitForEndOfFrame();
    }

    public override IEnumerator Rotate(float yrot)
    {
        Quaternion targetRotation = Quaternion.Euler(0, yrot, 0);

        while (Quaternion.Angle(transform.rotation, targetRotation) >= 1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, speed * 3 * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }
    }

    public override IEnumerator Move(Vector3 pos, float speed = 3f)
    {
        // ensure we send the agent to a valid NavMesh position
        Vector3 validPos = GetValidNavMeshPosition(pos);
        agent.SetDestination(validPos);
        soundSource = validPos;
        StartAgentMovement();
        while (agent.remainingDistance >= agent.stoppingDistance)
        {
            yield return new WaitForEndOfFrame();
        }
        StartCoroutine(FacePlayer());
        agent.ResetPath();
        ReturnToPatrol();
    }

    /// <summary>
    /// Called when enemy gives up pursuit and returns to patrolling.
    /// </summary>
    public void ReturnToPatrol()
    {
        detectedSound = false;
        isDiscoveringSpot = false;
        targetHidingSpot = null;

        // 2. Reset Movement State
        agent.isStopped = false; // Essential: Unpause the NavMeshAgent
        agent.speed = speed;     // Return to normal walking speed

        // 3. Set Destination
        if (point != null && point.Length > 0)
        {
            // Ensure we are targeting the current waypoint index
            agent.SetDestination(point[idxPoint].position);
        }
    }

    void StartAgentMovement()
    {
        agent.isStopped = false;
        agent.SetDestination(soundSource);
    }
    public void OnEnterAudioRadius(GameObject audioSource)
    {
        // if the source belongs to the player and the player is currently hiding
        // we should ignore the event completely – hidden players don't make noise
        PlayerHiding ph = audioSource.GetComponentInParent<PlayerHiding>();
        if (ph != null && ph.IsHiding()) return;

        agent.isStopped = false;
        agent.speed = pursueSpeed;
        detectedSound = true;

        // raw position received from the audio source (flattened to our y)
        Vector3 rawPos = new Vector3(audioSource.transform.position.x, transform.position.y, audioSource.transform.position.z);

        // If the notifier sits inside a NavMeshObstacle (carving collider) we
        // should not try to navigate to its exact centre.  Prefer a nearby
        // sampled NavMesh position that is reachable from the agent.
        bool insideObstacle = false;
        Collider[] nearby = Physics.OverlapSphere(rawPos, 0.5f);
        foreach (var c in nearby)
        {
            if (c.GetComponent<NavMeshObstacle>() != null)
            {
                insideObstacle = true;
                break;
            }
        }

        // find a valid candidate on the NavMesh and ensure the agent can reach it
        Vector3 candidate = GetValidNavMeshPosition(rawPos);
        if (agent != null)
        {
            NavMeshPath path = new NavMeshPath();
            agent.CalculatePath(candidate, path);
            // if path is partial or invalid, try to broaden the search
            if (path.status != NavMeshPathStatus.PathComplete || insideObstacle)
            {
                // attempt a slightly broader search around the raw source
                candidate = GetValidNavMeshPosition(rawPos + (transform.position - rawPos).normalized * 1.5f);
                agent.CalculatePath(candidate, path);
                // if still not reachable, fall back to agent's current destination
                if (path.status != NavMeshPathStatus.PathComplete)
                {
                    if (agent.hasPath)
                        candidate = agent.destination;
                    else
                        candidate = transform.position;
                }
            }
        }

        soundSource = candidate;
        agent.SetDestination(soundSource);
    }

    public void OnExitAudioRadius(GameObject audioSource)
    {
        ReturnToPatrol();
    }

    /// <summary>
    /// Attempts to project a world position onto the NavMesh.  The 
    /// supplied <paramref name="target"/> is sampled first with a small radius
    /// and then with a broader radius before falling back to either the
    /// agent's current destination or the enemy's own position.  This keeps
    /// the AI from trying to walk to an unreachable point if the sound comes
    /// from off the mesh.
    /// </summary>
    private Vector3 GetValidNavMeshPosition(Vector3 target)
    {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(target, out hit, 2.0f, NavMesh.AllAreas)) return hit.position;
        if (NavMesh.SamplePosition(target, out hit, 10.0f, NavMesh.AllAreas)) return hit.position;
        return transform.position;
    }
    
    #endregion

    #region Gizmos

    private void OnDrawGizmos()
    {
        // Draw alert indicator when enemy detected sound
        if (detectedSound)
        {
            // Draw red sphere at enemy position when alerted
            Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
            Gizmos.DrawSphere(transform.position, 1f);

            // Draw line from enemy to sound source
            Gizmos.color = new Color(1f, 0f, 0f, 0.8f);
            Gizmos.DrawLine(transform.position, soundSource);

            // Draw sphere at sound source location
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.6f);
            Gizmos.DrawWireSphere(soundSource, 1.5f);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // When selected, always show hiding spot detection radius
        Gizmos.color = new Color(0.5f, 0.5f, 1f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, hidingSpotDetectionRadius);

        // Show alert state more prominently when selected
        if (detectedSound)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.8f);
            Gizmos.DrawSphere(transform.position, 1f);
            Gizmos.DrawLine(transform.position, soundSource);
            Gizmos.DrawWireSphere(soundSource, 1.5f);
        }
    }

    #endregion

    #region Stun
    public void GetStunned()
    {
        Debug.Log("Haha get stunned bozo");
        isStunned = true;
        agent.isStopped = true;
        currIdleTime = idleTime * PlayerGrabInteraction.GetThrowCharge();
    }
    #endregion
}
