using UnityEngine;

public class Waypoint : MonoBehaviour
{
    [HideInInspector] public Vector3 position => transform.position;
    public bool endPosition;
    public NPCAnimationState endState;
    public Transform faceTowards;
}
