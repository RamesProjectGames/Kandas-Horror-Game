using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour, IAudioRadiusListener
{
    [SerializeField] Waypoint[] point;
    [SerializeField] int idxPoint = 0;
    [SerializeField] EnemySightDetection fov;
    NavMeshAgent agent;
    bool detectedSound;
    public Vector3 soundSource;
    public float speed = 3f;
    public float pursueSpeed = 6f;
    public float idleTime = 5f, currIdleTime;
    bool comeback = false;
    bool reachPoint = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null) agent = gameObject.AddComponent<NavMeshAgent>();

        // Configure NavMeshAgent for patrol/search
        agent.angularSpeed = 300f;
        agent.acceleration = 8f;
        agent.stoppingDistance = 0.1f;
        agent.isStopped = true;
        currIdleTime = idleTime;
    }

    // Update is called once per frame
    void Update()
    {
        if (!fov.canSeePlayer)
        {
            if(agent.isStopped && !comeback)
            {
                if (!reachPoint)
                {
                    Vector3 posTarget = new Vector3(point[idxPoint].position.x, transform.position.y, point[idxPoint].position.z);

                    if (Vector3.Distance(transform.position, posTarget) > 0.1f)
                    {
                        transform.position = Vector3.MoveTowards(transform.position, posTarget, speed * Time.deltaTime);
                        Vector3 posPoint = posTarget - transform.position;
                        transform.rotation = Quaternion.LookRotation(posPoint);
                    }
                    else
                    {
                        if (point[idxPoint].endPosition)
                        {
                            reachPoint = point[idxPoint].endPosition;
                            currIdleTime = idleTime;
                        }
                        Debug.Log($"Player arrived at waypoint {idxPoint}, updating next waypoint {idxPoint++}");
                        idxPoint = idxPoint % point.Length;
                    }
                }
                else
                {
                    currIdleTime -= Time.deltaTime;
                    if (currIdleTime <= 0)
                    {
                        reachPoint = false;
                        Debug.Log($"mob continuing journey");
                    }
                    else
                    {
                        Debug.Log($"mob idle after moving");
                    }
                }
            }
            else
            {
                if (agent.remainingDistance <= agent.stoppingDistance)
                {
                    if (detectedSound)
                    {
                        Vector3 posPoint = soundSource - transform.position;
                        transform.rotation = Quaternion.LookRotation(posPoint);
                        reachPoint = true;
                        ObserveSurroundings();
                        agent.isStopped = true;
                    }
                    else if (comeback)
                    {
                        reachPoint = point[idxPoint].endPosition;
                        comeback = false;
                        agent.isStopped = true;
                    }
                    Debug.Log($"mob idle after chasing audio");
                }
                else if(!agent.isStopped)
                {
                    Debug.Log($"Mob chasing audio");
                }
            }
        }
        else
        {
            Vector3 posPlayer = new Vector3(fov.player.transform.position.x, transform.position.y, fov.player.transform.position.z);

            transform.position = Vector3.MoveTowards(transform.position, posPlayer, pursueSpeed * Time.deltaTime);
            Vector3 posPoint = posPlayer - transform.position;
            transform.rotation = Quaternion.LookRotation(posPoint);
            Debug.Log($"Mob Chasing player");
        }
    }

    void ObserveSurroundings()
    {
        //Observe

        Vector3 posTarget = new Vector3(point[idxPoint].position.x, transform.position.y, point[idxPoint].position.z);
        agent.SetDestination(posTarget);
    }

    void StartAgentMovement()
    {
        agent.isStopped = false;
        agent.SetDestination(soundSource);
        reachPoint = false;
    }
    public void OnEnterAudioRadius(GameObject audioSource)
    {
        agent.speed = 6f;
        detectedSound = true;
        soundSource = new Vector3(audioSource.gameObject.transform.position.x, transform.position.y, audioSource.gameObject.transform.position.z-5f);
        StartAgentMovement();
    }

    public void OnExitAudioRadius(GameObject audioSource)
    {
        agent.speed = 3f;
        detectedSound = false;
        agent.isStopped = false;
        comeback = true;
    }
}
