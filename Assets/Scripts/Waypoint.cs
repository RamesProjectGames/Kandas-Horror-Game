using UnityEngine;

public class Waypoint : MonoBehaviour
{
    [HideInInspector] public Vector3 position;
    public bool endPosition;
    public Transform faceTowards;

    private void Start()
    {
        position = transform.position;
    }
}
