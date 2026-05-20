using Dialogue;
using FMOD.Studio;
using FMODUnity;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.UI;

//[RequireComponent (typeof(CharacterController), typeof(Animator))]
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MovableObjects
{
    [Header("Main Components")]
    [SerializeField] private CharacterController controller;
    [SerializeField] private AudioSource audioSrc;
    [SerializeField] private GameObject face;
    //[SerializeField] private Animator anim;

    [Header("Ground Detection")]
    public Transform foot;
    public LayerMask groundMask;
    public AudioClip[] audioClip;
    public GroundSurface currentSurface;

    [Header("Input Action")]
    private Vector3 input;
    private Vector3 up;
    [SerializeField] private InputActionReference moveAction, lookAction, sprintAction, crouchAction, unlockAction;

    [Header("Numeric Values")]
    [SerializeField] private float jumpPow;
    [SerializeField] private float gravity = 9.81f, groundDist = 1f, speed = 150f, sprintMulti = 2.0f, jumpCd, maxStamina, staminaDecayRate;
    [SerializeField] private bool isExhausted;
    private float stamina, moveSpd;
    private bool isGrounded;
    public bool isSprinting;
    public bool isCrouching;

    [Header("Player Camera Settings")]
    [SerializeField] private CinemachineCamera playerCam;
    [SerializeField] private Transform cam;
    public float lookSensitivity = 1f;
    public float smoothTime = 0.1f;
    public float minVerticalAngle = -20f, maxVerticalAngle = 20f;
    public float interactionAngle = 20f, interactionDist = 5f;
    //private CinemachineBasicMultiChannelPerlin _noise;
    private CinemachineInputAxisController inputController;
    private CinemachinePanTilt panTilt;

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
        //anim = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        Hiding = GetComponent<PlayerHiding>();
        audioSrc = GetComponent<AudioSource>();
        CameraManager.SwitchCamera(playerCam);
        //_noise = playerCam.GetComponent<CinemachineBasicMultiChannelPerlin>();
        panTilt = playerCam.GetComponent<CinemachinePanTilt>();

        pantingSoundEvent = AudioManager.Instance.CreateInstance(pantingSound);
        RuntimeManager.AttachInstanceToGameObject(pantingSoundEvent, gameObject, false);

        inputController = playerCam.GetComponent<CinemachineInputAxisController>();
        
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
        if(CanUseAgent()) agent.isStopped = true;
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

    // Update is called once per frame
    void Update()
    {
        //Mouse Lock - Unlock when holding Alt
        {
            if (unlockAction != null && unlockAction.action.IsPressed())
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                if(SettingManager.Instance.isPaused || DialogueSystem.Instance.isRunningConvo
                    )
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
        if (SettingManager.Instance.isPaused)
        {
            ResetMovementState();
            lookAction.action.Disable();
            return;
        }
        else if(!lookAction.action.enabled)
        {
            lookAction.action.Enable();
        }
        if (inputController != null)
        {
            float sliderValue = SettingManager.Instance.settings.MouseSensitivity ;
            float calculatedGain = Mathf.Lerp(SettingManager.Instance.minimumMouseSensitivity, SettingManager.Instance.maximumMouseSensitivity, sliderValue) * lookSensitivity;

            // Controllers is a list. Usually: Index 0 = Pan, Index 1 = Tilt
            foreach (var controller in inputController.Controllers)
            {
                if (controller.Name.Contains("Tilt") )
                {
                    // Multiplying by -1 flips the direction
                    controller.Input.Gain = -calculatedGain;
                }
                else
                {
                    controller.Input.Gain = calculatedGain;
                }
                controller.Driver.AccelTime = 0f;
                controller.Driver.DecelTime = 0f;
            }
        }
        inputController.enabled = Cursor.lockState == CursorLockMode.Locked;
        if(SettingManager.Instance.isPaused) return;

        if(!CanUseAgent()) return;
        //Movement - skip input if player is hiding
        if (Hiding != null && Hiding.IsHiding())
        {
            // when hidden we don't process movement input or physics
        }
        else if(agent.isStopped)
        {
            if(!SettingManager.Instance.settings.SprintToggle)
            {
                if (sprintAction != null && sprintAction.action.IsPressed() && !isExhausted)
                {
                    isSprinting = true;
                    if (stamina <= 0f)
                    {
                        stamina= 0;
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
                        stamina= 0;
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
                else if(isCrouching)
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
            

            if(!Hiding.IsHiding())
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
            if(!isSprinting)
            {
                stamina += staminaDecayRate * Time.deltaTime * (isCrouching ? sprintMulti : 1f);
            }
            if(stamina < maxStamina)
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
            if(!CanUseAgent()) return;
            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                agent.updateRotation = false;
                Vector3 posPoint = agent.destination - transform.position;
                playerCam.ForceCameraPosition(posPoint, Quaternion.identity);
                agent.isStopped = true;
            }
            else
            {
                agent.updateRotation = true;
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

    private void FixedUpdate()
    {
        isGrounded = Physics.CheckSphere(foot.position, groundDist, groundMask);
        //Debug.Log($"Player is Grounded: {isGrounded}");

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
        else if(footstepManager != null)
        {
            footstepManager.StopFootstep();
        }
        _lastFootstepPosition = transform.position;
    }

    private void LateUpdate()
    {
        // cam.localRotation = Quaternion.Euler(pitch, 0, 0);
        // transform.Rotate(Vector3.up * xMove);
        if (controller.enabled && !SettingManager.Instance.isPaused && !DialogueSystem.Instance.isRunningConvo)
        {
            HandleCrouch();
            controller.SimpleMove(moveSpd * input);
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

    public void FaceObject(Transform targetObject)
    {
        Vector3 direction = targetObject.position - playerCam.transform.position;

        // Yaw (horizontal rotation)
        float yaw = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;

        // Pitch (vertical rotation)
        float pitch = -Mathf.Asin(direction.normalized.y) * Mathf.Rad2Deg;

        panTilt.PanAxis.Value = yaw;
        panTilt.TiltAxis.Value = pitch;
        playerCam.LookAt = targetObject;
    }

    public void FaceFront()
    {
        transform.rotation = Quaternion.Euler(0, 0, 0);

        //panTilt.PanAxis.Value = 0;
        //panTilt.TiltAxis.Value = 0;
        //playerCam.ForceCameraPosition(transform.forward, Quaternion.identity);
    }

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
        transform.rotation = Quaternion.Euler(0, 0, 0);
        agent.ResetPath();
        yield return new WaitForEndOfFrame();
        controller.enabled = true;
    }
    public override IEnumerator Rotate(float yrot)
    {
        if (agent != null)
            agent.updateRotation = false;
        transform.rotation = Quaternion.Euler(0, yrot, 0);
        if (agent != null)
            agent.updateRotation = true;
        if (panTilt == null) yield break;

        //For incremental rotation
        //float startPan = panTilt.PanAxis.Value;
        //float elapsed = 0f;
        //float duration = 1f / speed; // adjust duration based on your speed definition

        //// Optionally clamp target angle to [-180,180] range
        //yrot = ((yrot % 360) + 360) % 360;
        //if (yrot > 180) yrot -= 360;

        //while (elapsed < duration)
        //{
        //    elapsed += Time.deltaTime;
        //    float t = elapsed / duration;
        //    float newPan = Mathf.LerpAngle(startPan, yrot, t);
        //    panTilt.PanAxis.Value = newPan;
        //    yield return null; // or WaitForEndOfFrame
        //}

        // Snap to exact target
        panTilt.PanAxis.Value = 0;
        panTilt.PanAxis.TrackValueChange();
    }

    public override IEnumerator Move(Vector3 pos, float speed = 150f)
    {
        agent.SetDestination(pos);
        agent.isStopped = false;
        yield return new WaitForEndOfFrame();
    }

    public bool CanUseAgent()
    {
        return agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh;
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
