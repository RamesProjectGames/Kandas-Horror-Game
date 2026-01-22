using UnityEngine;

public class EnemyMovement : MonoBehaviour, IAudioRadiusListener
{
    [SerializeField] Waypoint[] point;
    [SerializeField] int idxPoint = 0;
    [SerializeField] EnemySightDetection fov;
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
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 posTarget = new Vector3(point[idxPoint].position.x, this.transform.position.y, point[idxPoint].position.z);

        Vector3 posPlayer = new Vector3(fov.player.transform.position.x, this.transform.position.y, fov.player.transform.position.z);
        if (!fov.canSeePlayer)
        {
            if(!reachPoint)
            {
                if (detectedSound)
                {
                    if (Vector3.Distance(this.transform.position, soundSource) > 2f)
                    {
                        this.transform.position = Vector3.MoveTowards(this.transform.position, soundSource, pursueSpeed * Time.deltaTime);
                        Vector3 posPoint = soundSource - this.transform.position;
                        this.transform.rotation = Quaternion.LookRotation(posPoint);
                    }
                    else
                    {
                        reachPoint = true;
                        currIdleTime = idleTime;
                    }
                }
                else
                {
                    if (Vector3.Distance(this.transform.position, posTarget) > 0.1f)
                    {
                        this.transform.position = Vector3.MoveTowards(this.transform.position, posTarget, speed * Time.deltaTime);
                        Vector3 posPoint = posTarget - this.transform.position;
                        this.transform.rotation = Quaternion.LookRotation(posPoint);
                    }
                    else
                    {
                        Debug.Log("Player arrived at waypoint, updating next waypoint");
                        if (point[idxPoint].endPosition)
                        {
                            reachPoint = true;
                            currIdleTime = idleTime;
                        }
                        idxPoint++;
                        idxPoint = idxPoint % point.Length;
                    }
                }
            }
            else
            {
                if (detectedSound)
                {
                    //ObserveSurroundings();
                }
                else
                {
                    currIdleTime -= Time.deltaTime;
                    if (currIdleTime <= 0)
                        reachPoint = false;

                }
            }
        }
        else
        {
            this.transform.position = Vector3.MoveTowards(this.transform.position, posPlayer, pursueSpeed * Time.deltaTime);
            Vector3 posPoint = posPlayer - this.transform.position;
            this.transform.rotation = Quaternion.LookRotation(posPoint);
        }
    }

    public void OnEnterAudioRadius(GameObject audioSource)
    {
        detectedSound = true;
        soundSource = audioSource.gameObject.transform.position;
    }

    public void OnExitAudioRadius(GameObject audioSource)
    {
        //throw new System.NotImplementedException();
    }
}
