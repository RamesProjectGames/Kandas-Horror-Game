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

//[RequireComponent (typeof(CharacterController), typeof(Animator))]
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MovableObjects
{
    [Header("Main Components")]
    [SerializeField] private CharacterController controller;
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
    [SerializeField] private float gravity = 9.81f, groundDist = 1f, speed = 150f, sprintMulti = 2.0f, jumpCd, maxStamina, staminaDecayRate;
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
    public float baseStepDistance = 2f;
    private Vector3 _lastFootstepPosition;
    private float _footstepDistanceAccum;

    [Header("Hiding")]
    public PlayerHiding Hiding;

    [Header("Stamina")]
    public Image staminaFillImage;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controller = GetComponent<CharacterController>();
        Hiding = GetComponent<PlayerHiding>();
        audioSrc = GetComponent<AudioSource>();
        RegisterWithSwitchManager();
        originalFollowTarget = playerCam.Follow;
        originalLookTarget = playerCam.LookAt;

        pantingSoundEvent = AudioManager.Instance.CreateInstance(pantingSound);
        RuntimeManager.AttachInstanceToGameObject(pantingSoundEvent, gameObject, false);

        inputController = playerCam.GetComponent<CinemachineInputAxisController>();
        ApplyLookSensitivity();

        Cursor.lockState = CursorLockMode.Locked;

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

        targetControllerHeight = standingHeight;
        targetCameraY = standingCameraY;

        // Force initial values
        controller.height = standingHeight;

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
                if (SettingManager.Instance.isPaused || (DialogueSystem.Instance.isRunningConvo && !DialogueSystem.Instance.cameraControl) || CameraManager.currentActiveCamera != playerCam)
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
                    stamina -= staminaDecayRate * Time.deltaTime;
                    if (stamina <= 0f)
                    {
                        stamina = 0;
                        isExhausted = true;
                    }
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
                    if (stamina <= 0f)
                    {
                        stamina = 0;
                        isExhausted = true;
                    }
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
                staminaFillImage.fillAmount = stamina / maxStamina;
            }
            else
            {
                stamina = maxStamina;
                staminaFillImage.gameObject.SetActive(false);
            }

            //float targetAmplitude = 0f;
            //float targetFrequency = 0f;

            //bool crouchTransitioning = IsCrouchTransitioning();

            //if (input != Vector3.zero && !crouchTransitioning)
            //{
            //    float headBobMultiplier = 1f;

            //    if (isCrouching)
            //        headBobMultiplier = crouchBobMultiplier;

            //    if (isSprinting && !isCrouching)
            //    {
            //        headBobMultiplier = sprintBobMultiplier;

            //        stamina -= staminaDecayRate * Time.deltaTime;
            //        if (stamina <= 0f)
            //            stamina = 0f;
            //    }

            //    targetAmplitude = headBobAmplitude * headBobMultiplier;
            //    targetFrequency = headBobFrequency * headBobMultiplier;
            //}

            // Smoothly apply noise instead of snapping
            //_noise.AmplitudeGain = Mathf.Lerp(_noise.AmplitudeGain, targetAmplitude, Time.deltaTime * 8f);
            //_noise.FrequencyGain = Mathf.Lerp(_noise.FrequencyGain, targetFrequency, Time.deltaTime * 8f);

        }
        else
        {
            if (!CanUseAgent()) return;
            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                Vector3 posPoint = agent.destination - transform.position;
                playerCam.ForceCameraPosition(posPoint, Quaternion.identity);
                agent.isStopped = true;
            }
        }


        // Player body rotation is handled by Cinemachine camera - do not force rotation here
        // Uncomment only if you need manual body rotation separate from camera
        // if (Cursor.lockState == CursorLockMode.Locked)
        // {
        //     transform.rotation = Quaternion.Euler(0, playerCam.transform.eulerAngles.y, 0);
        // }
        //Rotation
        // {
        //     Vector2 lookInput = lookAction != null ? lookAction.ReadValue<Vector2>() : Vector2.zero;
        //     yaw = transform.localEulerAngles.y + lookInput.x * SettingManager.Instance.settings.MouseSensitivity * lookSensitivity;

        //     pitch -= SettingManager.Instance.settings.MouseSensitivity * lookInput.y * lookSensitivity;

        //     // Clamp pitch between lookAngle
        //     pitch = Mathf.Clamp(pitch, minVerticalAngle, maxVerticalAngle);

        //     transform.localEulerAngles = new Vector3(0, yaw, 0);
        //     face.transform.localEulerAngles = new Vector3(pitch, 0, 0);
        // }
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
        isGrounded = Physics.CheckSphere(foot.position, groundDist, groundMask);

        // accumulate distance travelled this frame and trigger a step when we've covered enough ground
        if (footstepManager != null && isGrounded && input.magnitude > 0.01f)
        {
            // scale step distance inversely with speed: faster movement = more frequent steps
            // use speed (normal walk speed, ~150) as baseline
            float effectiveStepDistance = moveSpd > 0.1f
                ? baseStepDistance * (speed / moveSpd)
                : baseStepDistance;

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

    private void LateUpdate()
    {
        if (!isActivePlayer)
        {
            return;
        }

        if (controller.enabled && !SettingManager.Instance.isPaused && !DialogueSystem.Instance.isRunningConvo)
        {
            // Rotate body left/right using Look X input
            transform.Rotate(Vector3.up * lookAction.action.ReadValue<Vector2>().x * SettingManager.Instance.settings.MouseSensitivity * lookSensitivity * Time.deltaTime);
            HandleCrouch();
            controller.SimpleMove(moveSpd * input);
            anim.SetFloat("MoveBlend", Mathf.CeilToInt(input.magnitude));
            if(input != Vector3.zero)
            {
                agent.nextPosition = transform.position;
            }
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
        if (lunchProgress++ == 2)
        {
            anim.SetBool("Meds", true);
            anim.SetBool("Lunch", false);
            GetComponent<PlayerGrabInteraction>().currentItem.ChangeInteractionText("EatMeds");
        }
    }

    public void EatMeds()
    {
        anim.SetTrigger("EatMeds");
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
        controller.enabled = false;
        yield return new WaitForEndOfFrame();
        agent.enabled = false;
        transform.position = pos;
        //agent.Warp(pos);
        yield return new WaitForSeconds(.1f);
        agent.enabled = true;
        agent.ResetPath();
        yield return new WaitForEndOfFrame();
        controller.enabled = true;
    }
    public override IEnumerator Rotate(float yrot)
    {
        Quaternion targetRotation = Quaternion.Euler(0, yrot, 0);
        while (Quaternion.Angle(transform.rotation, targetRotation) > 10f)
        {
            // Putar secara bertahap dari rotasi saat ini ke rotasi target
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5.0f);
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
    public void ResetMovementState()
    {
        isSprinting = false;
        isCrouching = false;
        stamina = maxStamina;
        staminaFillImage.gameObject.SetActive(false);

        controller.height = standingHeight;

        if (cameraHeightTarget != null)
        {
            Vector3 localPos = cameraHeightTarget.localPosition;
            localPos.y = standingCameraY;
            cameraHeightTarget.localPosition = localPos;
        }
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