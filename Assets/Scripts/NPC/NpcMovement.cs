using Dialogue;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class NpcMovement : MovableObjects
{
    public Waypoint[] point;
    public int idxPoint = 0;
    [SerializeField] Animator animator;
    [SerializeField] bool loopMovement;
    [SerializeField] Transform head;

    [Header("Footsteps")]
    [SerializeField] FootstepsSoundManager footstepManager;
    public Transform foot;
    public LayerMask groundMask;
    public GroundSurface currentSurface;
    [SerializeField] private NPCAnimationState animState;
    private static string headGOName = "DEF-spine.005";
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
    [SerializeField] float speed = 1f;
    float idleTime = 5f, currIdleTime;
    bool wasPausedLastFrame = false;
    float lastFootstep;
    [HideInInspector] public bool moveMyself = false;

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
        Vector3 validPos = new Vector3();
        if (agent != null)
        {
            moveMyself = true;
            agent.enabled = true;
            agent.speed = speed;
            validPos = GetValidNavMeshPosition(pos);
            agent.SetDestination(validPos);
        }
        animator.SetFloat("Blend", 1);
        yield return new WaitForEndOfFrame();
    }

    public override IEnumerator Teleport(Vector3 pos)
    {
        if (agent != null)
        {
            agent.enabled = true;
            agent.Warp(pos);
            agent.ResetPath();
            yield return new WaitForEndOfFrame();
            agent.enabled = (movementAllowed || moveMyself) && point.Length > 1;
        }
        transform.position = pos;
        float state = 0;
        if (animState == NPCAnimationState.Sit)
        {
            if (DialogueSystem.Instance.isRunningConvo)
            {
                state = UnityEngine.Random.Range(4, 7);
            }
            else
                state = 4;
        }
        else if (animState == NPCAnimationState.Stand)
        {
            if (DialogueSystem.Instance.isRunningConvo)
            {
                state = UnityEngine.Random.Range(0, 4);
            }
            else
                state = UnityEngine.Random.Range(0, 2);
        }
        animator.SetFloat("Blend", state / 7f);
        yield return new WaitForEndOfFrame();
    }
    public override IEnumerator Rotate(float yrot, float rotSpd = 5f)
    {
        rotSpd = Mathf.Min(rotSpd, 1f);
        Quaternion targetRotation = Quaternion.Euler(0, yrot, 0);

        while (Quaternion.Angle(transform.rotation, targetRotation) >= 10f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotSpd * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }
        transform.rotation = targetRotation;
    }
    public IEnumerator RotateHead(float yrot, float rotSpd = 5f)
    {
        yrot = Mathf.Clamp(yrot, -45f, 45f);
        Quaternion targetRotation = Quaternion.Euler(head.rotation.x, yrot, head.rotation.z);

        while (Quaternion.Angle(head.rotation, targetRotation) >= 10f)
        {
            head.rotation = Quaternion.Slerp(head.rotation, targetRotation, rotSpd * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }
        head.rotation = targetRotation;
    }

    public void ToggleNPCMovement()
    {
        if ((point.Length == 0 && !agent.hasPath) || !moveMyself) return;
        if (agent != null) agent.enabled = movementAllowed || moveMyself;
        if (movementAllowed && point.Length > 1)
            agent.SetDestination(GetValidNavMeshPosition(point[idxPoint].transform.position));
    }

    private void TeleportToFirstWaypoint()
    {
        if (point.Length > 0 && point[0] != null)
        {
            animState = NPCAnimationState.Stand;
            StartCoroutine(Teleport(point[0].transform.position));
            if (point[0].faceTowards != null)
            {
                Vector3 targetPos = point[0].faceTowards.position;
                targetPos.y = transform.position.y;
                Quaternion targetRotation = Quaternion.LookRotation(targetPos - transform.position);
                transform.rotation = targetRotation;
            }
            else
            {
                Quaternion targetRotation = transform.rotation;
                targetRotation.y = point[0].transform.rotation.y;
                transform.rotation = targetRotation;
            }
            HandleAnimationEndState();
            transform.position = point[0].position;
        }
        if (point.Length > 1)
            idxPoint = ++idxPoint % point.Length;
    }

    public void TeleportToWaypoint(int index = 0)
    {
        if (point.Length > 0 && point[index] != null)
        {
            animState = NPCAnimationState.Stand;
            StartCoroutine(Teleport(point[index].transform.position));
            if (point[index].faceTowards != null)
            {
                Vector3 targetPos = point[index].faceTowards.position;
                targetPos.y = transform.position.y;
                Quaternion targetRotation = Quaternion.LookRotation(targetPos - transform.position);
                transform.rotation = targetRotation;
            }
            else
            {
                Quaternion targetRotation = transform.rotation;
                targetRotation.y = point[index].transform.rotation.y;
                transform.rotation = targetRotation;
            }
            transform.position = point[index].position;
        }
        if (moveMyself && point.Length > 1)
        {
            if (++index >= point.Length)
            {
                if (loopMovement)
                {
                    idxPoint = index % point.Length;
                    agent.SetDestination(GetValidNavMeshPosition(point[idxPoint].transform.position));
                    animState = NPCAnimationState.Walk;
                    animator.SetFloat("Blend", 1f);
                }
                else
                {
                    moveMyself = false;
                    HandleAnimationEndState();
                    return;
                }
            }
            else
            {
                idxPoint = index % point.Length;
                agent.SetDestination(GetValidNavMeshPosition(point[idxPoint].transform.position));
                animState = NPCAnimationState.Walk;
                animator.SetFloat("Blend", 1f);
            }
        }
        else
        {
            HandleAnimationEndState();
        }
    }
    #endregion

    #region Start-up
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            agent = gameObject.AddComponent<NavMeshAgent>();
        }

        // NavMeshAgent must drive the transform because the animation has no root motion
        agent.updatePosition = true;
        agent.updateRotation = true;
        agent.enabled = true;
        if (point.Length > 0)
            agent.SetDestination(point[idxPoint].position);
        agent.enabled = false;


        ToggleNPCMovement();
        currIdleTime = idleTime;
        // footsteps helper
        if (footstepManager == null)
            footstepManager = GetComponent<FootstepsSoundManager>();
        if (animator != null)
        {
            animator.applyRootMotion = true;
        }

        if(head == null)
            head = GetComponentsInChildren<Transform>().First(x => x.gameObject.name == headGOName);

        float state = 0;
        if (animState == NPCAnimationState.Sit)
        {
            if (DialogueSystem.Instance.isRunningConvo)
            {
                state = UnityEngine.Random.Range(4, 7);
            }
            else
                state = 4;
        }
        else if (animState == NPCAnimationState.Stand)
        {
            if (DialogueSystem.Instance.isRunningConvo)
            {
                state = UnityEngine.Random.Range(0, 4);
            }
            else
                state = UnityEngine.Random.Range(0, 2);
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
        if (moveMyself)
        {
            if (agent != null && !agent.enabled)
            {
                agent.enabled = true;
            }
        }
        else
        {
            return;
        }
        if (HandlePauseState()) return;

        if (animState == NPCAnimationState.Sit)
            return;

        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            if (point.Length == 0)
            {
                HandleAnimationEndState();
                moveMyself = false;
                return;
            }
            if (animState != NPCAnimationState.Walk)
            {
                currIdleTime -= Time.deltaTime;
                if (currIdleTime <= 0)
                {
                    StartCoroutine(RotateHead(0f));
                    if (++idxPoint >= point.Length)
                    {
                        if (loopMovement)
                        {
                            idxPoint %= point.Length;
                        }
                        else
                        {
                            moveMyself = false;
                            return;
                        }
                    }
                    else
                        idxPoint %= point.Length;
                    if (moveMyself)
                    {
                        // Transition from Idle to Moving
                        animator.SetFloat("Blend", 1);
                        animState = NPCAnimationState.Walk;
                        agent.SetDestination(GetValidNavMeshPosition(point[idxPoint].transform.position));
                        agent.speed = speed;
                    }
                }
                return; // Exit early while idling
            }
            currIdleTime = point[idxPoint].endPosition ? idleTime : 0.5f;
            HandleAnimationEndState();
            if (point[idxPoint].faceTowards != null && animState == NPCAnimationState.Sit)
            {
                //Rotate
                Vector3 targetPos = point[idxPoint].faceTowards.position;
                targetPos.y = transform.position.y;
                Quaternion targetRotation = Quaternion.LookRotation(targetPos - transform.position);
                if (targetRotation.y >= -45f && targetRotation.y <= 45f)
                    StartCoroutine(RotateHead(targetRotation.y));
                else
                    StartCoroutine(Rotate(targetRotation.y));
            }
            else
            {
                StartCoroutine(Rotate(point[idxPoint].transform.rotation.y));
            }
        }
        else
        {
            //Debug.Log($"Character {name} is {agent.remainingDistance}m away from it's target, point {idxPoint}, it's currently {(agent.remainingDistance <= agent.stoppingDistance ? "indeed" : "not")} stopped");
            animator.SetFloat("Blend", 1f);
            agent.speed = speed;
        }
    }

    void HandleAnimationEndState()
    {
        float state = 0;
        if (point.Length > 0 && point[idxPoint] != null)
            animState = point[idxPoint].endState;
        else
            animState = NPCAnimationState.Stand;
        if (animState == NPCAnimationState.Sit)
        {
            if (DialogueSystem.Instance.isRunningConvo)
            {
                state = UnityEngine.Random.Range(4, 7);
            }
            else
                state = 4;
        }
        else if (animState == NPCAnimationState.Stand)
        {
            if (DialogueSystem.Instance.isRunningConvo)
            {
                state = UnityEngine.Random.Range(0, 4);
            }
            else
                state = UnityEngine.Random.Range(0, 2);
        }
        animator.SetFloat("Blend", state / 7f);
    }

    void OnAnimatorMove()
    {
        if (animator == null) return;
        // NavMeshAgent drives position. OnAnimatorMove just syncs
        // agent.nextPosition to the transform so the NavMesh stays consistent.
        agent.nextPosition = transform.position;
    }

    public IEnumerator ToggleRig(bool active, float delay)
    {
        float elapsedTime = 0;
        while (elapsedTime < delay)
        {
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        Debug.Log($"{(active? "Showing": "Hiding")} NPC Rig furreal");

        foreach (SkinnedMeshRenderer rig in GetComponentsInChildren<SkinnedMeshRenderer>(true))
        {
            rig.gameObject.SetActive(active);
        }
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
        bool isPaused = SettingManager.Instance.isPaused;
        if (isPaused)
        {
            if (!wasPausedLastFrame && agent != null) agent.enabled = false;
            wasPausedLastFrame = true;
            return true;
        }
        else if (movementAllowed && agent != null) agent.enabled = true;
        if (wasPausedLastFrame)
        {
            wasPausedLastFrame = false;

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
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, speed * 5 * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }
        transform.rotation = targetRotation;
    }
    public void TriggerDialogue()
    {
        if (animState == NPCAnimationState.Walk)
            animState = NPCAnimationState.Stand;
        float state = 0;
        if (animState == NPCAnimationState.Sit)
        {
            if (DialogueSystem.Instance.isRunningConvo)
            {
                state = UnityEngine.Random.Range(4, 7);
            }
            else
                state = 4;
        }
        else if (animState == NPCAnimationState.Stand)
        {
            if (DialogueSystem.Instance.isRunningConvo)
            {
                state = UnityEngine.Random.Range(0, 4);
            }
            else
                state = UnityEngine.Random.Range(0, 2);
        }
        animator.SetFloat("Blend", state / 7f);
        animator.SetFloat("Blend", state / 7f);
    }
    #endregion
}

public enum NPCAnimationState
{
    Sit,
    Stand,
    Walk
}