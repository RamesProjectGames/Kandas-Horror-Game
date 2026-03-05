using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EnemySightDetection))]
public class EnemySightDetectionEditor : Editor
{
    void OnSceneGUI()
    {
        EnemySightDetection sight = (EnemySightDetection)target;
        
        // 1. Draw the outer radius circle
        Handles.color = Color.white;
        Handles.DrawWireArc(sight.transform.position, Vector3.up, Vector3.forward, 360, sight.viewRadius);

        // 2. Calculate FOV boundaries based on CURRENT transform rotation
        // We use the transform's actual forward to ensure it moves when the enemy turns
        Vector3 viewAngle01 = DirectionFromAngle(sight.transform.eulerAngles.y, -sight.viewAngle / 2);
        Vector3 viewAngle02 = DirectionFromAngle(sight.transform.eulerAngles.y, sight.viewAngle / 2);

        // 3. Draw the FOV cone lines
        Handles.color = Color.yellow;
        Handles.DrawLine(sight.transform.position, sight.transform.position + viewAngle01 * sight.viewRadius);
        Handles.DrawLine(sight.transform.position, sight.transform.position + viewAngle02 * sight.viewRadius);

        // 4. Draw line to player if spotted
        if (sight.canSeePlayer && sight.player != null)
        {
            Handles.color = Color.green;
            Handles.DrawLine(sight.transform.position, sight.player.transform.position);
        }
    }

    /// <summary>
    /// Converts an angle to a direction vector, accounting for Unity's Y-axis rotation.
    /// </summary>
    private Vector3 DirectionFromAngle(float eulerY, float angleInDegrees)
    {
        // Add the current Y rotation to the relative FOV angle
        angleInDegrees += eulerY;

        // Convert to radians and calculate the vector
        // Unity's 0 degrees is Z-forward, so we swap Sin and Cos logic
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
}
