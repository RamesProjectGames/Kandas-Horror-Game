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
    [SerializeField] private string correctStrikeLimb = "Head"; // Which limb is OK to strike
    private Collider strikeCollider;

    [Header("Reset")]
    private PlayerResetManager resetManager;

    private bool isInPose = false;
    private bool wasLightOnLastFrame = true;

    void Start()
    {
        playerSight = FindAnyObjectByType<PlayerSightInteraction>();
        playerTransform = playerSight?.transform;
        animator = GetComponent<Animator>();
        strikeCollider = GetComponent<Collider>();
        resetManager = FindAnyObjectByType<PlayerResetManager>();

        if (animator == null)
        {
            Debug.LogError("MannequinEnemyType2: No Animator component found!");
        }

        poseTimer = timeBetweenPoses;
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
            // Light is OFF: mannequin can pose
            UpdatePoseSequence();
            isInPose = true;
        }
        else
        {
            // Light is ON: mannequin returns to idle
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
        // Check if correct limb was struck
        if (limb != correctStrikeLimb && !string.IsNullOrEmpty(limb))
        {
            // Wrong limb - trigger event
            OnWrongStrike?.Invoke(limb);
            
            // Reset all mannequins poses
            ResetAllMannequinPoses();
            
            // Trigger player reset
            TriggerPlayerResetWrongStrike(limb);
        }
        else
        {
            // Correct strike - trigger event
            OnCorrectStrike?.Invoke(limb);
            
            // Play defeated pose/animation
            ResetToIdle();
        }
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
            resetManager.ResetPlayer($"Struck wrong limb: {hitLimb} (should hit {correctStrikeLimb})");
        }
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

