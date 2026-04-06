using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

/// <summary>
/// Type 2 Mannequin Enemy: Takes poses when not visible
/// Can be struck - triggers reset if hit incorrectly
/// Event-based system for pose and strike tracking
/// </summary>
public class MannequinFullGame : MonoBehaviour
{
    // ===== EVENTS =====
    public delegate void MannequinEvent();
    public delegate void PoseEvent(int poseIndex, string poseName);
    public delegate void StrikeEvent(string limbName);
    
    public event MannequinEvent OnLightOn;
    public event MannequinEvent OnLightOff;
    public event PoseEvent OnPoseStarted;
    public event PoseEvent OnPoseChanged;
    public event StrikeEvent OnCorrectStrike;
    public event StrikeEvent OnWrongStrike;
    public event MannequinEvent OnAllPosesReset;
    [Header("Detection")]
    [SerializeField] private float detectionRange = 20f;
    private PlayerSightInteraction playerSight;
    private Transform playerTransform;


    [Header("Animator Poses")]
    [SerializeField] private string[] poseAnimations = new string[] { "Pose1", "Pose2", "Pose3" };
    private Animator animator;
    private int currentPoseIndex = 0;

    [Header("Pose Behavior")]
    [SerializeField] private float timeBetweenPoses = 3f;
    private float poseTimer = 0f;

    [Header("Strike Detection")]
    [SerializeField] private float strikeRadius = 2f;
    [SerializeField] private bool isDestroyable = false; // Can this mannequin be destroyed? (5 out of 20)
    [SerializeField] private bool correctStrike = false; // Is this the correct one to destroy? (1 out of 5 destroyable)
    private Collider strikeCollider;
    private Dictionary<string, Collider> limbColliders = new();

    [Header("Audio")]
    [SerializeField] private EventReference PoseSound;
    [SerializeField] private EventReference StrikeSound;
    private EventInstance poseSoundEvent;
    private EventInstance strikeSoundEvent;

    [Header("Reset")]
    private PlayerResetManager resetManager;

    private bool isInPose = false;
    private bool wasLightOnLastFrame = true;
    private int correctStrikesNeeded = 1; // Number of correct strikes needed to defeat all limbs
    private int correctStrikesReceived = 0;

    void Start()
    {
        playerSight = FindAnyObjectByType<PlayerSightInteraction>();
        playerTransform = playerSight?.transform;
        animator = GetComponent<Animator>();
        strikeCollider = GetComponent<Collider>();
        resetManager = FindAnyObjectByType<PlayerResetManager>();
        poseSoundEvent = AudioManager.Instance.CreateInstance(PoseSound);
        strikeSoundEvent = AudioManager.Instance.CreateInstance(StrikeSound);
        
        RuntimeManager.AttachInstanceToGameObject(poseSoundEvent, gameObject, false);
        RuntimeManager.AttachInstanceToGameObject(strikeSoundEvent, gameObject, false);

        if (animator == null)
        {
            Debug.LogError("Mannequin JOJO POSE: No Animator component found!");
        }

        // Initialize limbColliders dictionary with child colliders
        Collider[] allColliders = GetComponentsInChildren<Collider>();
        foreach (Collider col in allColliders)
        {
            limbColliders[col.gameObject.name] = col;
        }

        poseTimer = timeBetweenPoses;
        correctStrikesReceived = 0;
    }

    void Update()
    {
        if (playerSight == null || playerTransform == null)
            return;

        // Check if the light is on or off
        bool isLightOn = LightSwitch.IsLightOn();

        // Detect light state change
        if (isLightOn != wasLightOnLastFrame)
        {
            if (isLightOn)
            {
                OnLightOn?.Invoke();
            }
            else
            {
                OnLightOff?.Invoke();
            }
            wasLightOnLastFrame = isLightOn;
        }

        if (!isLightOn)
        {
            // Light is OFF: only destroyable mannequins can pose
            if (isDestroyable)
            {
                UpdatePoseSequence();
                isInPose = true;
            }
            else
            {
                // Non-destroyable mannequins stay in idle
                if (!isInPose)
                {
                    ResetToIdle();
                }
            }
        }
        else
        {
            // Light is ON: all mannequins return to idle
            if (isInPose)
            {
                ResetToIdle();
                isInPose = false;
            }
        }
    }

    /// <summary>
    /// Called when this mannequin is struck/hit
    /// limb: Name of the hit collider/limb
    /// </summary>
    public void OnStruck(string limb = "")
    {
        // Only destroyable mannequins can be struck
        if (!isDestroyable)
        {
            // Trying to strike a non-destroyable mannequin - trigger wrong strike
            OnWrongStrike?.Invoke(limb);
            
            // Reset all mannequins poses
            ResetAllMannequinPoses();
            
            // Trigger player reset
            TriggerPlayerResetWrongStrike(limb);
            
            Debug.Log($"Cannot strike non-destroyable mannequin: {gameObject.name}");
            return;
        }

        // Check if correct strike based on correctStrike flag
        if (!correctStrike)
        {
            // Wrong strike - trigger event
            OnWrongStrike?.Invoke(limb);
            
            // Reset all mannequins poses
            ResetAllMannequinPoses();
            
            // Trigger player reset
            TriggerPlayerResetWrongStrike(limb);
            
            Debug.Log($"Wrong mannequin! This one is NOT the correct one!");
        }
        else
        {
            // Correct strike - trigger event
            OnCorrectStrike?.Invoke(limb);
            
            // Disable the struck limb collider
            RemoveLimb(limb);
            
            correctStrikesReceived++;
            Debug.Log($"Correct mannequin destroyed! This was the RIGHT one!");
            
            // Check if all limbs have been removed
            if (correctStrikesReceived >= correctStrikesNeeded)
            {
                DefeatMannequin();
            }
            else
            {
                // Play defeated pose/animation
                ResetToIdle();
            }
        }
    }

    /// <summary>
    /// Removes/disables a limb collider when struck correctly
    /// </summary>
    private void RemoveLimb(string limbName)
    {
        if (limbColliders.ContainsKey(limbName))
        {
            Collider limbCollider = limbColliders[limbName];
            limbCollider.enabled = false;
            Debug.Log($"Limb '{limbName}' removed from mannequin");
        }
        else
        {
            Debug.LogWarning($"Limb '{limbName}' not found in limbColliders dictionary");
        }
    }

    /// <summary>
    /// Called when all limbs have been defeated
    /// </summary>
    private void DefeatMannequin()
    {
        Debug.Log("Mannequin defeated! All limbs removed!");
        // Disable the entire mannequin
        gameObject.SetActive(false);
    }

    private void ResetAllMannequinPoses()
    {
        // Find all mannequins in the scene
        MannequinFullGame[] allMannequins = FindObjectsByType<MannequinFullGame>(FindObjectsSortMode.None);
        
        foreach (MannequinFullGame mannequin in allMannequins)
        {
            mannequin.ForceResetPose();            // Trigger all poses reset event on each mannequin
            mannequin.OnAllPosesReset?.Invoke();        }
        
        Debug.Log("All mannequin poses reset!");
    }

    /// <summary>
    /// Force this mannequin to reset its pose (called from reset all function)
    /// </summary>
    public void ForceResetPose()
    {
        ResetToIdle();
        isInPose = false;
    }

    private void UpdatePoseSequence()
    {
        poseTimer -= Time.deltaTime;

        if (poseTimer <= 0f)
        {
            // Switch to next pose
            currentPoseIndex = (currentPoseIndex + 1) % poseAnimations.Length;
            string poseName = poseAnimations[currentPoseIndex];

            PlayPoseSound();
            
            // Trigger pose changed event
            OnPoseChanged?.Invoke(currentPoseIndex, poseName);
            
            TriggerPose(poseName);
            poseTimer = timeBetweenPoses;
        }
    }

    private void TriggerPose(string poseName)
    {
        if (animator != null)
        {
            animator.SetTrigger(poseName);
            // Trigger pose started event
            OnPoseStarted?.Invoke(currentPoseIndex, poseName);
        }
    }

    private void ResetToIdle()
    {
        if (animator != null)
        {
            animator.SetTrigger("Idle");
        }
        poseTimer = timeBetweenPoses;
    }

    private void TriggerPlayerResetWrongStrike(string hitLimb)
    {
        if (resetManager != null)
        {
            resetManager.ResetPlayer($"Wrong strike on {hitLimb}!");
        }
    }

    public void PlayPoseSound()
    {
        
        PLAYBACK_STATE playbackState;
        poseSoundEvent.getPlaybackState(out playbackState);
        if (playbackState == PLAYBACK_STATE.STOPPED)
        {
            RuntimeManager.AttachInstanceToGameObject(poseSoundEvent, gameObject, false);
            poseSoundEvent.start();
        }
    }
    public void StopPoseSound()
    {
        poseSoundEvent.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }
    public void PlayStrikeSound()
    {
        PLAYBACK_STATE playbackState;
        strikeSoundEvent.getPlaybackState(out playbackState);
        if (playbackState == PLAYBACK_STATE.STOPPED)
        {
            RuntimeManager.AttachInstanceToGameObject(strikeSoundEvent, gameObject, false);
            strikeSoundEvent.start();
        }
    }
    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.name == "Attack Item Hitbox")
        {
            PlayStrikeSound();
        }
    }
    public void StopStrikeSound()
    {
        strikeSoundEvent.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }

    private void OnDrawGizmos()
    {
        if (isInPose)
        {
            Gizmos.color = Color.cyan;
        }
        else
        {
            Gizmos.color = Color.yellow;
        }
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Draw strike radius
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, strikeRadius);
    }
}

