using Dialogue;
using System.Collections;
using System.Drawing;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class NpcMovement : MovableObjects
{
    [SerializeField] Waypoint[] point;
    [SerializeField] int idxPoint = 0;
    float speed = 1f;
    float idleTime = 5f, currIdleTime;
    bool wasPausedLastFrame = false;
    private Vector3 GetValidNavMeshPosition(Vector3 target)
    {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(target, out hit, 2.0f, NavMesh.AllAreas)) return hit.position;
        if (NavMesh.SamplePosition(target, out hit, 10.0f, NavMesh.AllAreas)) return hit.position;
        return transform.position;
    }

    public override IEnumerator Move(Vector3 pos, float speed = 3f)
    {
        // ensure we send the agent to a valid NavMesh position
        Vector3 validPos = GetValidNavMeshPosition(pos);
        agent.SetDestination(validPos);
        yield return new WaitForEndOfFrame();
    }

    public override IEnumerator Teleport(Vector3 pos)
    {
        agent.Warp(pos);
        transform.LookAt(GameObject.Find("Player").transform.position);
        agent.ResetPath();
        yield return new WaitForEndOfFrame();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null) agent = gameObject.AddComponent<NavMeshAgent>();
        if(point.Length > 0)
            agent.SetDestination(point[idxPoint].position);
        currIdleTime = idleTime;
    }

    // Update is called once per frame
    void Update()
    {
        if (HandlePauseState()) return;

        if (point.Length == 0)
            return;
        else if (point.Length == 1)
        {
            if (Vector3.Distance(transform.position, point[0].position) < agent.stoppingDistance)
            {
                if (point[idxPoint].faceTowards != null)
                {
                    Vector3 targetPos = point[idxPoint].faceTowards.position;
                    targetPos.y = transform.position.y; // Maintain same Y level
                    transform.LookAt(targetPos);
                }
            }
            return;
        }

        if (agent.isStopped)
        {
            currIdleTime -= Time.deltaTime;
            if (currIdleTime <= 0)
            {
                // Transition from Idle to Moving
                agent.isStopped = false;
                agent.speed = speed;

                agent.SetDestination(point[idxPoint].position);
            }
            return; // Exit early while idling
        }
        else if(agent.remainingDistance <= agent.stoppingDistance)
        {
            if (point[idxPoint].faceTowards != null)
            {
                Vector3 targetPos = point[idxPoint].faceTowards.position;
                targetPos.y = transform.position.y;
                Quaternion targetRotation = Quaternion.LookRotation(targetPos - transform.position);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, speed * Time.deltaTime);

                if (transform.rotation == targetRotation)
                {
                    idxPoint = idxPoint++ % point.Length;
                    agent.isStopped = true;
                    currIdleTime = point[idxPoint].endPosition ? idleTime : 0.5f;
                }
            }
            else
            {
                idxPoint = idxPoint++ % point.Length;
                agent.isStopped = true;
                currIdleTime = point[idxPoint].endPosition ? idleTime : 0.5f;
            }
        }
    }

    private bool HandlePauseState()
    {
        bool isPaused = SettingManager.Instance.isPaused || DialogueSystem.Instance.isRunningConvo;
        if (isPaused)
        {
            if (!wasPausedLastFrame && agent != null) agent.enabled = false;
            wasPausedLastFrame = true;
            return true;
        }
        if (wasPausedLastFrame)
        {
            if (agent != null) agent.enabled = true;
            wasPausedLastFrame = false;
            agent.SetDestination(point[idxPoint].position);
        }
        return false;
    }
}
