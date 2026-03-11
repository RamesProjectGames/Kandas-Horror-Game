using System.Collections;
using System.Drawing;
using UnityEngine;
using UnityEngine.AI;

public class NpcMovement : MovableObjects
{
    [SerializeField] Waypoint[] point;
    [SerializeField] int idxPoint = 0;
    public float speed = 3f;
    public float idleTime = 5f, currIdleTime;
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
        currIdleTime = idleTime;
    }

    // Update is called once per frame
    //void Update()
    //{
    //    if (agent.isStopped)
    //    {
    //        currIdleTime = point[idxPoint].endPosition ? idleTime : 0.5f;
    //        currIdleTime -= Time.deltaTime;
    //        if (currIdleTime <= 0)
    //        {
    //            // Transition from Idle to Moving
    //            agent.isStopped = false;
    //            agent.speed = speed;

    //            idxPoint = (idxPoint + 1) % point.Length;
    //            agent.SetDestination(point[idxPoint].position);
    //        }
    //        return; // Exit early while idling
    //    }
    //}
}
