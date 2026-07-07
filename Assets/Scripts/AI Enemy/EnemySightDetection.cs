using Dialogue;
using UnityEngine;
using UnityEngine.SceneManagement;

// [ExecuteInEditMode]
public class EnemySightDetection : MonoBehaviour
{
    public float viewRadius;
    [SerializeField] private float baseViewRadius = 8f;
    [SerializeField] private float maxViewRadius = 14f;
    [SerializeField] private float radiusChangeSpeed = 4f;

    [Range(0, 360)]
    public float viewAngle;

    public Transform face;

    public LayerMask targetMask;
    public LayerMask obstacleMask;

    public GameObject player;
    public GameObject playerMesh;

    public bool canSeePlayer;
    
    [Header("Hiding Spot Tracking")]
    [SerializeField] private bool playerWasSpottedWhileHiding = false;
    public bool PlayerWasSpottedWhileHiding => playerWasSpottedWhileHiding;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        playerMesh = player.transform.Find("Capsule Mesh").gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        if (Application.isPlaying && (SettingManager.Instance.isPaused || DialogueSystem.Instance.isRunningConvo))
            return;

        float targetRadius = canSeePlayer ? maxViewRadius : baseViewRadius;
        viewRadius = Mathf.MoveTowards(viewRadius, targetRadius, radiusChangeSpeed * Time.deltaTime);

        Vector3 enemyPos = transform.position;
        Vector3 playerPos = player.transform.position;

        Vector3 playerTarget = (playerPos - enemyPos).normalized;
        playerTarget.y = 0;

        // don't try to see a player who is currently tucked away in a hiding spot;
        // normal visibility checks are suspended until the 'spotted while hiding'
        // flag is manually set (see NotifyPlayerHidWhileVisible).
        PlayerHiding playerHiding = player.GetComponent<PlayerHiding>();
        bool hiding = playerHiding != null && playerHiding.IsHiding();
        if (hiding)
        {
            ChangePlayerMaterial(Color.white);
            canSeePlayer = false;
            return; // bail out early, actual alert happens elsewhere
        }

        Vector3 currentForward = transform.forward;
        currentForward.y = 0;

        float angle = Vector3.Angle(currentForward, playerTarget);

        if (angle < viewAngle / 2)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

            if(distanceToPlayer < viewRadius)
            {
                if(!Physics.Raycast(transform.position, playerTarget, distanceToPlayer, obstacleMask))
                {
                    ChangePlayerMaterial(Color.green);
                    canSeePlayer = true;
                }
                else
                {
                    ChangePlayerMaterial(Color.white);
                    canSeePlayer = false;
                }
            }
            else
            {
                ChangePlayerMaterial(Color.white);
                canSeePlayer = false;
            }
        }
        else if(canSeePlayer)
        {
            ChangePlayerMaterial(Color.white);
            canSeePlayer = false;
        }
    }
    public void ChangePlayerMaterial(Color newColor)
    {
        playerMesh.GetComponent<Renderer>().material.color = newColor;
    }

    /// <summary>
    /// Reset the spotted while hiding flag. Call this when the player successfully hides
    /// in a new location or when the hunt is abandoned.
    /// </summary>
    public void ResetSpottedFlag()
    {
        playerWasSpottedWhileHiding = false;
    }

    /// <summary>
    /// Notify the detector that the player has just slipped into a hiding spot while
    /// they were still within this enemy's view.  This is the moment we actually alert
    /// the AI to a hiding attempt; the flag will be consumed by <see cref="EnemyMovement"/>.
    /// </summary>
    public void NotifyPlayerHidWhileVisible()
    {
        if (canSeePlayer)
            playerWasSpottedWhileHiding = true;
    }
}
