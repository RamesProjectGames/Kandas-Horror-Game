using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EnemySightDetection))]
public class EnemySightDetectionEditor : Editor
{
    void OnSceneGUI()
    {
        EnemySightDetection sight = (EnemySightDetection)target;
        Handles.color = Color.white;
        Handles.DrawWireArc(sight.transform.position, Vector3.up, Vector3.forward, 360, sight.viewRadius);

        Vector3 viewAngle01 = DirectionFromAngle(-sight.transform.eulerAngles.y, -sight.viewAngle / 2);
        Vector3 viewAngle02 = DirectionFromAngle(-sight.transform.eulerAngles.y, sight.viewAngle / 2);

        Handles.color = Color.yellow;
        Handles.DrawLine(sight.transform.position, sight.transform.position + viewAngle01 * sight.viewRadius);
        Handles.DrawLine(sight.transform.position, sight.transform.position + viewAngle02 * sight.viewRadius);

        if(sight.canSeePlayer)
        {
            sight.ChangePlayerMaterial(Color.green);
            Handles.color = Color.green;
            Handles.DrawLine(sight.transform.position, sight.player.transform.position);
        }
        else
        {
            sight.ChangePlayerMaterial(Color.white);
        }
    }

    private Vector3 DirectionFromAngle(float eualerY, float angleInDegrees)
    {
        float angle = eualerY + angleInDegrees;
        return new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), 0, Mathf.Cos(angle * Mathf.Deg2Rad));
    }
}
