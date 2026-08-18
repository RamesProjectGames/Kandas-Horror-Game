using Dialogue;
using FMOD.Studio;
using FMODUnity;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static UnityEngine.Rendering.GPUSort;

public class PlayerController : MovableObjects
{
    [Header("Main Components")]
    [SerializeField] private AudioSource audioSrc;
    [SerializeField] private GameObject face;
    [SerializeField] private Animator anim;
    [SerializeField] public GameObject rig;
    [HideInInspector] public int lunchProgress = 0;

    [Header("Ground Detection")]
    public Transform foot;
    public LayerMask groundMask;
    public AudioClip[] audioClip;
    public GroundSurface currentSurface;

    [Header("Input Action")]
    private Vector3 input;
    [SerializeField] private InputActionReference moveAction, lookAction, sprintAction, crouchAction, unlockAction;

    [Header("Player State")]
    [SerializeField] private bool isActivePlayer = true;

    [Header("Numeric Values")]
    [SerializeField] private float jumpPow;
    [SerializeField] private float groundDist = 1f, speed = 150f, sprintMulti = 2.0f, jumpCd, maxStamina, staminaDecayRate;
    [SerializeField] private bool isExhausted;
    private float stamina, moveSpd;
    private bool isGrounded;
    public bool isSprinting;
    public bool isCrouching;
    public bool isBeingGrab;

    [Header("Player Camera Settings")]
    [SerializeField] private CinemachineCamera playerCam;
    [SerializeField] private Transform cam;
    public float lookSensitivity = 1f;
    public float smoothTime = 0.1f;
    public float minVerticalAngle = -20f, maxVerticalAngle = 20f;
    public float interactionAngle = 20f, interactionDist = 5f;
    //private CinemachineBasicMultiChannelPerlin _noise;
    [SerializeField] private CinemachineInputAxisController inputController;
    private Transform originalFollowTarget;
    private Transform originalLookTarget;

    [Header("Bob Settings")]
    public float headBobAmplitude = 0.5f;
    public float headBobFrequency = 1.0f;
    public float walkBobMultiplier = 1.5f;
    public float sprintBobMultiplier = 2.0f;
    public float crouchBobMultiplier = 0.5f;

    [Header("Crouch Settings")]
    [SerializeField] private float standingHeight = 2f;
    [SerializeField] private float crouchHeight = 1.2f;
    [SerializeField] private float crouchSpeedMultiplier = 0.45f;
    [SerializeField] private float crouchTransitionSpeed = 10f;

    [SerializeField] private Transform cameraHeightTarget; // assign your face or camera pivot
    [SerializeField] private float standingCameraY = 1.6f;
    [SerializeField] private float crouchCameraY = 1.0f;

    [SerializeField] private LayerMask ceilingMask;
    [SerializeField] private float ceilingCheckRadius = 0.25f;
    [SerializeField] private float ceilingCheckOffset = 0.1f;
    private float targetControllerHeight;
    private float targetCameraY;

    [Header("Audio")]
    public EventReference pantingSound;
    private EventInstance pantingSoundEvent;
    private bool wasExhausted = false;

    [Header("Footsteps")]
    // reference to the centralized sound manager – typically on the same GameObject
    public FootstepsSoundManager footstepManager;
    // base step distance at normal walking speed; adjusted dynamically based on moveSpd
    public float baseStepDistance = 1.25f;
    private Vector3 _lastFootstepPosition;
    private float _footstepDistanceAccum;

    private float GetStrideLength()
    {
        float baseStride = baseStepDistance;

        if (isSprinting)
            baseStride *= 0.8f;
        else if (isCrouching)
            baseStride *= 1.3f;

        float speedRatio = speed / Mathf.Max(moveSpd, 0.1f);
        float stride = baseStride * Mathf.Clamp(speedRatio, 0.55f, 1.7f);
        return Mathf.Clamp(stride, 0.45f, 1.8f);
    }

    [Header("Hiding")]
    public PlayerHiding Hiding;

    [Header("Stamina")]
    public Slider staminaFillImage;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Hiding = GetComponent<PlayerHiding>();
        audioSrc = GetComponent<AudioSource>();
        RegisterWithSwitchManager();
        originalFollowTarget = playerCam.Follow;
        originalLookTarget = playerCam.LookAt;

        pantingSoundEvent = AudioManager.Instance.CreateInstance(pantingSound);
        RuntimeManager.AttachInstanceToGameObject(pantingSoundEvent, gameObject, false);

        inputController = playerCam.GetComponent<CinemachineInputAxisController>();
        ApplyLookSensitivity();

        stamina = maxStamina;
        staminaFillImage.gameObject.SetActive(false);

        agent = GetComponent<NavMeshAgent>();

        // footsteps helper
        if (footstepManager == null)
            footstepManager = GetComponent<FootstepsSoundManager>();
        _lastFootstepPosition = transform.position;
        if (agent == null) agent = gameObject.AddComponent<NavMeshAgent>();

        // Configure NavMeshAgent for auto movement
        agent.angularSpeed = 300f;
        agent.acceleration = 8f;
        agent.stoppingDistance = 0.1f;
        if (CanUseAgent())
        {
            agent.isStopped = true;
            agent.enabled = true;
        }
        agent.updateRotation = false;
        agent.updatePosition = false;

        targetControllerHeight = standingHeight;
        targetCameraY = standingCameraY;

        // Force initial values
        agent.height = standingHeight;

        if (cameraHeightTarget != null)
        {
            Vector3 camLocal = cameraHeightTarget.localPosition;
            camLocal.y = standingCameraY;
            cameraHeightTarget.localPosition = camLocal;
        }
    }
    [ContextMenu("Show Input Axis")]
    public void ShowInputAxis()
    {
        if (inputController != null)
        {
            foreach (var controller in inputController.Controllers)
            {
                Debug.Log($"Controller Name: {controller.Name}, Gain: {controller.Input.Gain}");
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!isActivePlayer)
        {
            SetInputActionsEnabled(false);
            ResetMovementState();
            return;
        }

        SetInputActionsEnabled(true);

        //Mouse Lock - Unlock when holding Alt
        {
            if (CameraManager.currentActiveCamera != playerCam)
            {
                agent.enabled = false;
            }
            else
            {
                agent.enabled = true;
            }
            if (unlockAction != null && unlockAction.action.IsPressed())
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                if (SettingManager.Instance.isPaused || SettingManager.Instance.gameOver || (DialogueSystem.Instance.isRunningConvo && !DialogueSystem.Instance.cameraControl) || CameraManager.currentActiveCamera != playerCam)
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
                else
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
            }
        }
        if (SettingManager.Instance.isPaused || CameraManager.currentActiveCamera != playerCam || isBeingGrab || SettingManager.Instance.gameOver)
        {
            ResetMovementState();
            lookAction.action.Disable();
        }
        else if (!lookAction.action.enabled)
        {
            lookAction.action.Enable();
        }
        //Mouse Look Control
        if (inputController != null)
        {
            ApplyLookSensitivity();
        }
        // inputController.enabled = Cursor.lockState == CursorLockMode.Locked;
        if (SettingManager.Instance.isPaused) return;

        if (!CanUseAgent()) return;
        //Movement - skip input if player is hiding
        if (Hiding != null && Hiding.IsHiding())
        {
            // when hidden we don't process movement input or physics
        }
        else if (agent.isStopped)
        {
            if (!SettingManager.Instance.settings.SprintToggle)
            {
                if (sprintAction != null && sprintAction.action.IsPressed() && !isExhausted)
                {
                    isSprinting = true;
                }
                else
                {
                    isSprinting = false;
                }
            }
            else
            {
                if (sprintAction != null && sprintAction.action.WasPressedThisFrame() && !isExhausted)
                {
                    isSprinting = true;
                }
                else
                {
                    isSprinting = false;
                }
            }
            if (!SettingManager.Instance.settings.CrouchToggle)
            {
                // Hold crouch
                isCrouching = crouchAction != null && crouchAction.action.IsPressed();
            }
            else
            {
                // Toggle crouch
                if (crouchAction != null && crouchAction.action.WasPressedThisFrame())
                {
                    if (isCrouching)
                    {
                        // Try to stand up only if there is room
                        if (CanStandUp())
                            isCrouching = false;
                    }
                    else
                    {
                        isCrouching = true;
                    }
                }
            }
            if (isExhausted)
            {
                moveSpd = speed / (sprintMulti * sprintMulti);
                if (stamina >= maxStamina)
                {
                    stamina = maxStamina;
                    isExhausted = false;
                }
            }
            else
            {
                if (isSprinting && !isCrouching)
                {
                    moveSpd = speed * sprintMulti;
                }
                else if (isCrouching)
                {
                    moveSpd = speed * crouchSpeedMultiplier;
                }
                else
                {
                    moveSpd = speed;
                }
            }

            // Handle panting sound when exhausted
            if (isExhausted && !wasExhausted)
            {
                PlayPantingSound();
            }
            else if (!isExhausted && wasExhausted)
            {
                StopPantingSound();
            }
            wasExhausted = isExhausted;


            if (!Hiding.IsHiding())
            {
                Vector2 moveInput = moveAction != null ? moveAction.action.ReadValue<Vector2>() : Vector2.zero;

                // Use camera forward and right for movement relative to camera view
                Vector3 camForward = playerCam.transform.forward;
                Vector3 camRight = playerCam.transform.right;

                // Flatten camera forward to prevent upward/downward movement bias
                camForward.y = 0;
                camForward.Normalize();

                Vector3 hor = camRight * moveInput.x;
                Vector3 ver = camForward * moveInput.y;

                input = (hor + ver).normalized;
            }
            if (!isSprinting)
            {
                stamina += staminaDecayRate * Time.deltaTime * (isCrouching ? sprintMulti : 1f);
            }
            if (stamina < maxStamina)
            {
                staminaFillImage.gameObject.SetActive(true);
                staminaFillImage.value = stamina / maxStamina;
            }
            else
            {
                stamina = maxStamina;
                staminaFillImage.gameObject.SetActive(false);
            }
        }
        else
        {
            if (!CanUseAgent()) return;
            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                Vector3 posPoint = agent.destination - transform.position;
                playerCam.ForceCameraPosition(posPoint, Quaternion.identity);
                agent.updatePosition = false;
                agent.updateRotation = false;
                agent.isStopped = true;
            }
        }
    }

    private void RegisterWithSwitchManager()
    {
        if (PlayerSwitchManager.Instance != null)
        {
            PlayerSwitchManager.Instance.RegisterPlayer(this);
        }
    }

    public void SetActivePlayer(bool active)
    {
        isActivePlayer = active;

        if (active)
        {
            if (playerCam != null)
            {
                CameraManager.SwitchCamera(playerCam);
            }
        }

        SetInputActionsEnabled(active);
    }

    private void SetInputActionsEnabled(bool enabled)
    {
        if (moveAction != null && moveAction.action != null)
        {
            if (enabled) moveAction.action.Enable(); else moveAction.action.Disable();
        }

        if (lookAction != null && lookAction.action != null)
        {
            if (enabled) lookAction.action.Enable(); else lookAction.action.Disable();
        }

        if (sprintAction != null && sprintAction.action != null)
        {
            if (enabled) sprintAction.action.Enable(); else sprintAction.action.Disable();
        }

        if (crouchAction != null && crouchAction.action != null)
        {
            if (enabled) crouchAction.action.Enable(); else crouchAction.action.Disable();
        }

        if (unlockAction != null && unlockAction.action != null)
        {
            if (enabled) unlockAction.action.Enable(); else unlockAction.action.Disable();
        }

    }

    private void ApplyLookSensitivity()
    {
        if (inputController == null || SettingManager.Instance == null || (DialogueSystem.Instance.isRunningConvo && !DialogueSystem.Instance.cameraControl) || SettingManager.Instance.settings == null)
        {
            inputController.enabled = false;
            return;
        }
        else if (!inputController.enabled)
        {
            inputController.enabled = true;
        }

        float sliderValue = SettingManager.Instance.settings.MouseSensitivity * lookSensitivity;

        foreach (var controller in inputController.Controllers)
        {
            if (controller == null)
                continue;

            if (controller.Name == "Look Y (Tilt)")
            {
                controller.Input.Gain = -sliderValue;
            }
            else if (controller.Name == "Look X (Pan)")
            {
               controller.Input.Gain = sliderValue;
            }

            controller.Driver.AccelTime = 0f;
            controller.Driver.DecelTime = 0f;
        }
    }

    private void FixedUpdate()
    {
        bool isMoving = input.magnitude > 0.01f
            && moveSpd > 0.1f
            && !SettingManager.Instance.isPaused
            && !DialogueSystem.Instance.isRunningConvo
            && (Hiding == null || !Hiding.IsHiding());

        if (footstepManager != null)
        {
            if (isMoving)
            {
                float dist = Vector3.Distance(transform.position, _lastFootstepPosition);
                if (dist > 0.01f)
                {
                    _footstepDistanceAccum += dist;
                    float strideLength = GetStrideLength();

                    if (_footstepDistanceAccum >= strideLength)
                    {
                        _footstepDistanceAccum = Mathf.Repeat(_footstepDistanceAccum, strideLength);
                        footstepManager.PlayFootstep();
                    }
                }
            }
            else
            {
                _footstepDistanceAccum = 0f;
                footstepManager.StopFootstep();
            }
        }

        _lastFootstepPosition = transform.position;
    }

    private void LateUpdate()
    {
        if (!isActivePlayer)
        {
            return;
        }

        if (agent.enabled && !SettingManager.Instance.isPaused && !DialogueSystem.Instance.isRunningConvo)
        {
            //// Rotate body left/right using Look X input
            transform.Rotate(Vector3.up * lookAction.action.ReadValue<Vector2>().x * SettingManager.Instance.settings.MouseSensitivity * lookSensitivity * Time.deltaTime);
            HandleCrouch();
            if (input != Vector3.zero)
            {
                if(isSprinting)
                {
                    stamina -= staminaDecayRate * Time.deltaTime;
                    if (stamina <= 0f)
                    {
                        stamina = 0;
                        isExhausted = true;
                    }
                }
                agent.Move(moveSpd * Time.deltaTime * input);
                transform.position = agent.nextPosition;
            }
            anim.SetFloat("MoveBlend", Mathf.CeilToInt(input.magnitude));
        }
    }

    private void OnFootstep(AnimationEvent animEvent)
    {
        audioSrc.clip = audioClip[animEvent.intParameter];
        audioSrc.Play();
    }
    private void OnLand(AnimationEvent animEvent)
    {
        audioSrc.clip = audioClip[Random.Range(0, audioClip.Length)];
        audioSrc.Play();
    }

    #region Animation
    public void ToggleRig(bool active)
    {
        rig.SetActive(active);
    }

    public IEnumerator PrepLunch()
    {
        ToggleRig(true);
        var bowlParent = GameObject.Find("BangkuKantin (10)");
        var bowl = bowlParent?.transform.Find("Bowl")?.gameObject;
        for (int i = 0; i < 3; i++)
        {
            if (bowl != null)
            {
                bowl.transform.GetChild(i).gameObject.SetActive(i==lunchProgress);
            }
        }
        bowl?.SetActive(true);
        var spoonParent = GameObject.Find("SpoonPos");
        spoonParent.transform.Find("Spoon")?.gameObject.SetActive(true);
        anim.SetBool("Lunch", true);
        CameraManager.SwitchCamera(GameObject.Find("LunchCam").GetComponent<CinemachineCamera>());
        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(Teleport(new Vector3(293.5f, transform.position.y, 218.75f)));
        yield return StartCoroutine(Rotate(180f));
    }

    public void ToggleLunchSequence(bool active)
    {
        rig.SetActive(active);
        anim.SetBool("Lunch", active);
    }

    public void EatFood()
    {
        anim.SetTrigger("EatLunch");
        lunchProgress++;
        var bowlParent = GameObject.Find("BangkuKantin (10)");
        var bowl = bowlParent?.transform.Find("Bowl")?.gameObject;
        for (int i = 0; i < 3; i++)
        {
            if (bowl != null)
            {
                bowl.transform.GetChild(i).gameObject.SetActive(i==lunchProgress);
            }
        }
        if (lunchProgress == 2)
        {
            DialogueSystem.Instance.convoManager.Enqueue(FileReader.ReadAsset("PostLunch"));

            // For Full Game with Take Meds
            //var spoonParent = GameObject.Find("SpoonPos");
            //spoonParent?.transform.Find("Spoon")?.gameObject.SetActive(true);
            //var pills = GameObject.Find("Pills");
            //pills?.transform.Find("Pill")?.gameObject.SetActive(true);
            //anim.SetBool("Meds", true);
            //anim.SetBool("Lunch", false);
            //GetComponent<PlayerGrabInteraction>().currentItem.ChangeInteractionText("Take Meds");
        }
    }

    public void EatMeds()
    {
        anim.SetTrigger("EatMeds");
        var bowlParent = GameObject.Find("BangkuKantin (10)");
        var bowl = bowlParent?.transform.Find("Bowl")?.gameObject;
        bowl?.SetActive(false);
        DialogueSystem.Instance.convoManager.Enqueue(FileReader.ReadAsset("PostLunch"));
    }
    #endregion

    #region Camera Handling
    private void HandleCrouch()
    {
        // Prevent standing up if blocked by ceiling
        if (!SettingManager.Instance.settings.CrouchToggle && !isCrouching)
        {
            if (!CanStandUp())
                isCrouching = true;
        }

        float desiredCameraY = isCrouching ? crouchCameraY : standingCameraY;

        // ONLY move Follow (camera target)
        if (cameraHeightTarget != null)
        {
            Vector3 localPos = cameraHeightTarget.localPosition;
            localPos.y = Mathf.MoveTowards(localPos.y, desiredCameraY, crouchTransitionSpeed * Time.deltaTime);
            cameraHeightTarget.localPosition = localPos;
        }
    }
    private bool CanStandUp()
    {
        if (cameraHeightTarget == null) return true;

        float extraHeightNeeded = standingCameraY - crouchCameraY;
        if (extraHeightNeeded <= 0.01f) return true;

        Vector3 origin = cameraHeightTarget.position;
        Vector3 target = origin + Vector3.up * extraHeightNeeded;

        return !Physics.CheckSphere(target, ceilingCheckRadius, ceilingMask, QueryTriggerInteraction.Ignore);
    }
    private bool IsCrouchTransitioning()
    {
        if (cameraHeightTarget == null) return false;

        float desiredY = isCrouching ? crouchCameraY : standingCameraY;
        return Mathf.Abs(cameraHeightTarget.localPosition.y - desiredY) > 0.02f;
    }

    public void FaceFront()
    {
        transform.rotation = Quaternion.Euler(0, 0, 0);
    }
    public void ChangeCameraFollow(Transform newFollow = null)
    {
        playerCam.Follow = newFollow ?? originalFollowTarget;
    }
    public void ChangeCameraLookAt(Transform newLook = null)
    {
        playerCam.LookAt = newLook ?? originalLookTarget;
    }
    #endregion

    #region Agent (auto) Movement
    public override IEnumerator Teleport(Vector3 pos)
    {
        yield return new WaitForEndOfFrame();
        agent.enabled = false;
        transform.position = pos;
        agent.Warp(pos);
        agent.nextPosition = transform.position;
        yield return new WaitForSeconds(.1f);
        agent.enabled = true;
        agent.ResetPath();
    }
    public override IEnumerator Rotate(float yrot, float rotSpd = 5f)
    {
        rotSpd = Mathf.Max(rotSpd, 1f);
        Quaternion targetRotation = Quaternion.Euler(0, yrot, 0);
        while (Quaternion.Angle(transform.rotation, targetRotation) > 10f)
        {
            // Putar secara bertahap dari rotasi saat ini ke rotasi target
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotSpd);
            yield return null;
        }
        // Snap to exact target
        transform.rotation = targetRotation;
    }

    public override IEnumerator Move(Vector3 pos, float speed = 150f)
    {
        agent.updatePosition = true;
        agent.updateRotation = true;
        agent.SetDestination(pos);
        agent.isStopped = false;
        yield return new WaitForEndOfFrame();
    }

    public bool CanUseAgent()
    {
        return agent != null && agent.enabled && agent.isOnNavMesh;
    }
    public void ResetMovementState()
    {
        isSprinting = false;
        isCrouching = false;
        stamina = maxStamina;
        staminaFillImage.gameObject.SetActive(false);

        agent.height = standingHeight;

        if (cameraHeightTarget != null)
        {
            Vector3 localPos = cameraHeightTarget.localPosition;
            localPos.y = standingCameraY;
            cameraHeightTarget.localPosition = localPos;
        }
    }

    private Vector3 GetValidNavMeshPosition(Vector3 target)
    {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(target, out hit, 2.0f, NavMesh.AllAreas)) return hit.position;
        if (NavMesh.SamplePosition(target, out hit, 10.0f, NavMesh.AllAreas)) return hit.position;
        return transform.position;
    }
    #endregion

    #region Audio
    private void PlayPantingSound()
    {
        PLAYBACK_STATE playbackState;
        pantingSoundEvent.getPlaybackState(out playbackState);
        if (playbackState != PLAYBACK_STATE.PLAYING)
        {
            // Recreate the instance to ensure it can be played
            pantingSoundEvent.release();
            pantingSoundEvent = AudioManager.Instance.CreateInstance(pantingSound);
            RuntimeManager.AttachInstanceToGameObject(pantingSoundEvent, gameObject, false);
            pantingSoundEvent.start();
        }
    }

    private void StopPantingSound()
    {
        PLAYBACK_STATE playbackState;
        pantingSoundEvent.getPlaybackState(out playbackState);
        if (playbackState == PLAYBACK_STATE.PLAYING)
        {
            pantingSoundEvent.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            pantingSoundEvent.release();
        }
    }
    #endregion
}