using UnityEngine;

public class EnemySightDetection : MonoBehaviour
{
    public float viewRadius;
    [Range(0, 360)]
    public float viewAngle;

    public LayerMask targetMask;
    public LayerMask obstacleMask;

    public GameObject player;

    public bool canSeePlayer;
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
                    canSeePlayer = true;
                }
                else
                {
                    canSeePlayer = false;
                }
            }
        }
        else if(canSeePlayer)
        {
            canSeePlayer = false;
        }
    }
}
