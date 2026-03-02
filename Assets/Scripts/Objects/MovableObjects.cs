using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public abstract class MovableObjects : MonoBehaviour
{
    public NavMeshAgent agent;

    public abstract IEnumerator Teleport(Vector3 pos);

    public abstract IEnumerator Move(Vector3 pos);
}
