using UnityEngine;

public class FloatingText : MonoBehaviour
{
    public Transform mainCamera;
    Transform unit;
    Transform worldSpaceCanvas;

    public Vector3 offset;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mainCamera = Camera.main.transform;
        unit = transform.parent;
        worldSpaceCanvas = GameObject.Find("WorldCanvas").transform;

        transform.SetParent(worldSpaceCanvas);
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.LookRotation(transform.position - mainCamera.position);
        transform.position = unit.position + offset;
    }
}
