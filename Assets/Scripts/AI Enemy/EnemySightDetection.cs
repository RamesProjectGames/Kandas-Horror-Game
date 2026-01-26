using UnityEngine;
using UnityEngine.SceneManagement;

[ExecuteInEditMode]
public class EnemySightDetection : MonoBehaviour
{
    public float viewRadius;
    [Range(0, 360)]
    public float viewAngle;

    public LayerMask targetMask;
    public LayerMask obstacleMask;

    public GameObject player;

    public bool canSeePlayer;
    
    [Header("Hiding Spot Tracking")]
    [SerializeField] private bool playerWasSpottedWhileHiding = false;
    public bool PlayerWasSpottedWhileHiding => playerWasSpottedWhileHiding;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 playerTarget = (player.transform.position - transform.position).normalized;

        if(Vector3.Angle(transform.forward, playerTarget) < viewAngle / 2)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

            if(distanceToPlayer < viewRadius)
            {
                if(!Physics.Raycast(transform.position, playerTarget, distanceToPlayer, obstacleMask))
                {
                    ChangePlayerMaterial(Color.green);
                    canSeePlayer = true;
                    
                    // Check if player is hiding
                    PlayerHiding playerHiding = player.GetComponent<PlayerHiding>();
                    if (playerHiding != null && playerHiding.IsHiding())
                    {
                        playerWasSpottedWhileHiding = true;
                    }
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
        player.GetComponent<Renderer>().material.color = newColor;
    }

    /// <summary>
    /// Reset the spotted while hiding flag. Call this when the player successfully hides
    /// in a new location or when the hunt is abandoned.
    /// </summary>
    public void ResetSpottedFlag()
    {
        playerWasSpottedWhileHiding = false;
    }
}
