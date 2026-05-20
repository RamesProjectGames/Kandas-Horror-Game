using Dialogue;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class NpcMovement : MovableObjects
{
    [SerializeField] Waypoint[] point;
    [SerializeField] int idxPoint = 0;
    [SerializeField] Animator animator;
    [SerializeField] bool loopMovement; //WIP

    [Header("Footsteps")]
    [SerializeField] FootstepsSoundManager footstepManager;
    public Transform foot;
    public LayerMask groundMask;
    public GroundSurface currentSurface;
    [SerializeField] private NPCAnimationState animState;

    private static bool allowMovement = false;
    public static Action NPCMovementTrigger, MovePrep;
    public static bool movementAllowed
    {
        get => allowMovement;
        set
        {
            // Only trigger if the value is actually changed
            if (allowMovement != value)
            {
                NPCMovementTrigger.Invoke();
            }
            allowMovement = value;
        }
    }
    float speed = 1f;
    float idleTime = 5f, currIdleTime;
    bool wasPausedLastFrame = false;
    float lastFootstep;
    
    private Vector2 Velocity;
    private Vector2 smoothDeltaPosition;
    private Vector3 GetValidNavMeshPosition(Vector3 target)
    {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(target, out hit, 2.0f, NavMesh.AllAreas)) return hit.position;
        if (NavMesh.SamplePosition(target, out hit, 10.0f, NavMesh.AllAreas)) return hit.position;
        return transform.position;
    }

    #region Dialogue Player Movement
    public override IEnumerator Move(Vector3 pos, float speed = 3f)
    {
        // ensure we send the agent to a valid NavMesh position
        Vector3 validPos = GetValidNavMeshPosition(pos);
        agent.SetDestination(validPos);
        agent.isStopped = false;
        animator.SetFloat("Blend", 1);
        agent.enabled = true;
        while (agent.remainingDistance >= agent.stoppingDistance)
        {
            yield return new WaitForEndOfFrame();
        }
        float state = 0;
        if (DialogueSystem.Instance.isRunningConvo)
            animState = NPCAnimationState.Talk;
        else
            animState = NPCAnimationState.Stand;
        if (animState == NPCAnimationState.Stand)
        {
            state = UnityEngine.Random.Range(0, 2);
        }
        else if (animState == NPCAnimationState.Talk)
        {
            state = UnityEngine.Random.Range(2, 4);
        }
        animator.SetFloat("Blend", state / 7f);
        agent.ResetPath();
    }

    public override IEnumerator Teleport(Vector3 pos)
    {
        if (agent != null) agent.enabled = true;
        transform.position = pos;
        agent.Warp(pos);
        agent.ResetPath();
        yield return new WaitForSeconds(.1f);
        if (agent != null)
        {
            agent.enabled = movementAllowed && point.Length > 1;
        }
        //else
        //{
        //    agent.isStopped = true;
        //    agent.enabled = false;
        //}
        float state = 0;
        if (DialogueSystem.Instance.isRunningConvo)
            animState = NPCAnimationState.Talk;
        else
            animState = NPCAnimationState.Stand;
        if (animState == NPCAnimationState.Stand)
        {
            state = UnityEngine.Random.Range(0, 2);
        }
        else if (animState == NPCAnimationState.Talk)
        {
            state = UnityEngine.Random.Range(2, 4);
        }
        animator.SetFloat("Blend", state / 7f);
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

    public void ToggleNPCMovement()
    {
        if (point.Length == 0) return;
        if (agent != null) agent.enabled = movementAllowed;
        if (movementAllowed && point.Length > 1)
            agent.SetDestination(GetValidNavMeshPosition(point[idxPoint].transform.position));
    }

    private void TeleportToFirstWaypoint()
    {
        if (point.Length > 0 && point[0] != null)
        {
            StartCoroutine(Teleport(point[0].transform.position));
            if (point[idxPoint].faceTowards != null)
            {
                Vector3 targetPos = point[idxPoint].faceTowards.position;
                targetPos.y = transform.position.y;
                Quaternion targetRotation = Quaternion.LookRotation(targetPos - transform.position);
                transform.rotation = targetRotation;
                if(point.Length > 1)
                    idxPoint++;
            }
        }
    }
    #endregion

    #region Start-up
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null) agent = gameObject.AddComponent<NavMeshAgent>();

        // NavMeshAgent must drive the transform because the animation has no root motion
        agent.updatePosition = true;
        agent.updateRotation = true;

        ToggleNPCMovement();
        currIdleTime = idleTime;
        // footsteps helper
        if (footstepManager == null)
            footstepManager = GetComponent<FootstepsSoundManager>();
        if (animator != null)
        {
            animator.applyRootMotion = true;
        }

        float state = 0;
        if (animState == NPCAnimationState.Sit)
        {
            state = UnityEngine.Random.Range(4, 7);
        }
        else if (animState == NPCAnimationState.Stand)
        {
            state = UnityEngine.Random.Range(0, 2);
        }
        else if (animState == NPCAnimationState.Talk)
        {
            state = UnityEngine.Random.Range(2, 4);
        }
        else if (animState == NPCAnimationState.Walk)
        {
            state = 7;
        }
        animator.SetFloat("Blend", state / 7f);
    }
    private void OnEnable()
    {
        NPCMovementTrigger += ToggleNPCMovement;
        MovePrep += TeleportToFirstWaypoint;
    }
    private void OnDisable()
    {
        NPCMovementTrigger -= ToggleNPCMovement;
        MovePrep -= TeleportToFirstWaypoint;
    }
    #endregion

    #region Update
    // Update is called once per frame
    void Update()
    {
        if (HandlePauseState() || !movementAllowed) return;

        if (point.Length < 2)
            return;

        if (!loopMovement && idxPoint == 0)
            return;
        //else if (point.Length == 1 && !agent.isStopped)
        //{
        //    if (Vector3.Distance(transform.position, point[0].transform.position) < agent.stoppingDistance)
        //    {
        //        agent.isStopped = true;
        //        if (point[idxPoint].faceTowards != null)
        //        {
        //            Vector3 targetPos = point[idxPoint].faceTowards.position;
        //            targetPos.y = transform.position.y;
        //            Quaternion targetRotation = Quaternion.LookRotation(targetPos - transform.position);
        //            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, speed * Time.deltaTime);

        //            if (Quaternion.Angle(transform.rotation, targetRotation) <= 5f)
        //            {
        //                transform.rotation = targetRotation;
        //            }
        //        }
        //        float state = 0;
        //        //if (animState == NPCAnimationState.Sit)
        //        //{
        //        //    state = UnityEngine.Random.Range(4, 7);
        //        //}
        //        if (animState == NPCAnimationState.Stand)
        //        {
        //            state = UnityEngine.Random.Range(0, 2);
        //        }
        //        else if (animState == NPCAnimationState.Talk)
        //        {
        //            state = UnityEngine.Random.Range(2, 4);
        //        }
        //        //else if (animState == NPCAnimationState.Walk)
        //        //{
        //        //    state = 7;
        //        //}
        //        animator.SetFloat("Blend", state / 7f);
        //    }
        //    else
        //    {
        //        agent.speed = speed;
        //        animator.SetFloat("Blend", 1f);
        //    }
        //    return;
        //}

        if (agent.isStopped)
        {
            currIdleTime -= Time.deltaTime;
            if (currIdleTime <= 0)
            {
                // Transition from Idle to Moving
                animator.SetFloat("Blend", 1);
                agent.isStopped = false;
                agent.speed = speed;
            }
            return; // Exit early while idling
        }
        else if (agent.remainingDistance <= agent.stoppingDistance)
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
                    agent.SetDestination(GetValidNavMeshPosition(point[idxPoint].transform.position));
                    transform.rotation = targetRotation;
                    agent.isStopped = true;
                }
            }
            else
            {
                currIdleTime = point[idxPoint].endPosition ? idleTime : 0.5f;
                idxPoint = ++idxPoint % point.Length;
                if (!loopMovement && idxPoint == 0)
                    return;
                agent.SetDestination(GetValidNavMeshPosition(point[idxPoint].transform.position));
                agent.isStopped = true;
            }
            //SynchronizeAnimatorAndAgent();
        }
        else
        {
            Debug.Log($"Agent {name} is {agent.remainingDistance}m away from it's target, point {idxPoint}, it's currently {(agent.isStopped ? "indeed" : "not")} stopped");
            agent.isStopped = false;
            animator.SetFloat("Blend", 1f);
            agent.speed = speed;
        }
    }
    void OnAnimatorMove()
    {
        if (animator == null) return;
        // NavMeshAgent drives position. OnAnimatorMove just syncs
        // agent.nextPosition to the transform so the NavMesh stays consistent.
        agent.nextPosition = transform.position;
    }
    public void SynchronizeAnimatorAndAgent()
    {
        if (animator == null) return;

        Vector3 worldDeltaPosition = agent.nextPosition - transform.position;
        worldDeltaPosition.y = 0f;

        float dx = Vector3.Dot(transform.right, worldDeltaPosition);
        float dy = Vector3.Dot(transform.forward, worldDeltaPosition);
        Vector2 deltaPosition = new Vector2(dx, dy);

        float smooth = Mathf.Min(1, Time.deltaTime / .1f);
        smoothDeltaPosition = Vector2.Lerp(smoothDeltaPosition, deltaPosition, smooth);

        Velocity = smoothDeltaPosition / Time.deltaTime;
        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            Velocity = Vector2.Lerp(Velocity,
            Vector2.zero,
            agent.remainingDistance / agent.stoppingDistance
            );
        }

        bool shouldMove = Velocity.magnitude > 0.5f
            && agent.remainingDistance > agent.stoppingDistance;


        animator.SetBool("move", shouldMove);
        animator.SetFloat("velocity", Velocity.magnitude);

        var footstep = animator.GetFloat("Footstep");
        if (Math.Abs(footstep) < .00001f)
        {
            footstep = 0f;
        }

        if ((footstep > 0f && lastFootstep < 0f) || (footstep < 0f && lastFootstep > 0f))
        {
            footstepManager.PlayFootstep();
        }
        lastFootstep = footstep;
        float deltaMagnitude = worldDeltaPosition.magnitude;
        if (deltaMagnitude > agent.radius / 2f)
        {
            transform.position = Vector3.Lerp(
                animator.rootPosition,
                agent.nextPosition,
                smooth);
        }
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
            if (movementAllowed && point.Length > 0 && agent != null) agent.enabled = true;
            wasPausedLastFrame = false;
            if (agent.enabled && point.Length > 0)
            {
                agent.SetDestination(GetValidNavMeshPosition(point[idxPoint].transform.position));
            }
        }
        return false;
    }
    #endregion

    #region Events
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
        transform.rotation = targetRotation;
    }
    public void TriggerDialogue()
    {
        animState = NPCAnimationState.Talk;
        float state = 0;
        state = UnityEngine.Random.Range(2, 5);
        animator.SetFloat("Blend", state / 7f);
    }
    #endregion
}

public enum NPCAnimationState
{
    Sit,
    Stand,
    Talk,
    Walk
}