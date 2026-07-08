using UnityEngine;

public class LookAtPlayer : MonoBehaviour
{
    public bool canLookPlayer;
    public float maxYaw = 70f;
    public float maxPitch = 45f;
    public bool smoothMotion = true;
    public float smoothSpeed = 8f;
    public Transform headTransform;

    private Transform playerTransform;
    private Quaternion originalRotation;

    void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        originalRotation = headTransform.rotation;
    }

    void Update()
    {
        if (canLookPlayer && playerTransform != null)
        {
            Vector3 direction = playerTransform.position - headTransform.position;
            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = GetClampedHeadRotation(direction);
                headTransform.rotation = smoothMotion
                    ? Quaternion.Slerp(headTransform.rotation, targetRotation, Time.deltaTime * smoothSpeed)
                    : targetRotation;
            }
        }
        else
        {
            headTransform.rotation = smoothMotion
                ? Quaternion.Slerp(headTransform.rotation, originalRotation, Time.deltaTime * smoothSpeed)
                : originalRotation;
        }
    }

    private Quaternion GetClampedHeadRotation(Vector3 worldDirection)
    {
        Vector3 normalizedDirection = worldDirection.normalized;
        Quaternion lookRotation = Quaternion.LookRotation(normalizedDirection, Vector3.up);
        Quaternion localRotation = Quaternion.Inverse(originalRotation) * lookRotation;

        Vector3 localEuler = localRotation.eulerAngles;
        float yaw = NormalizeAngle(localEuler.y);
        float pitch = NormalizeAngle(localEuler.x);

        yaw = Mathf.Clamp(yaw, -maxYaw, maxYaw);
        pitch = Mathf.Clamp(pitch, -maxPitch, maxPitch);

        return originalRotation * Quaternion.Euler(pitch, yaw, 0f);
    }

    private static float NormalizeAngle(float angle)
    {
        while (angle > 180f) angle -= 360f;
        while (angle <= -180f) angle += 360f;
        return angle;
    }

    public void SetCanLookPlayer(bool value)
    {
        canLookPlayer = value;
    }
}
