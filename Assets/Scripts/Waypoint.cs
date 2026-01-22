using UnityEngine;

public class Waypoint : MonoBehaviour
{
    [HideInInspector] public Vector3 position;
    public bool endPosition;
    private void Start()
    {
        position = transform.position;
    }
}
