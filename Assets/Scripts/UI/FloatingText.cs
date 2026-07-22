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
        if(mainCamera == null)
        {
            return;
        }
        Vector3 direction = transform.position - mainCamera.position;

        // 2. CRITICAL: Flatten the direction so there is no vertical tilt
        direction.y = 0;

        // 3. Create the rotation based on the flattened direction
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
        transform.localPosition = unit.position + offset;
        transform.localScale = Vector3.one;
    }
}
