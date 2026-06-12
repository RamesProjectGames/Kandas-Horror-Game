using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EnemyAttack))]
public class EnemyAttackEditor : Editor
{
    void OnSceneGUI()
    {
        EnemyAttack attack = (EnemyAttack)target;
        
        // 1. Draw the outer radius circle
        Handles.color = Color.white;
        Handles.DrawWireArc(attack.transform.position, Vector3.up, Vector3.forward, 360, attack.attackRange);

        // 2. Calculate FOV boundaries based on CURRENT transform rotation
        // We use the transform's actual forward to ensure it moves when the enemy turns
        // Vector3 viewAngle01 = DirectionFromAngle(attack.transform.eulerAngles.y, -attack.attackRange / 2);
        // Vector3 viewAngle02 = DirectionFromAngle(attack.transform.eulerAngles.y, attack.attackRange / 2);

        // 3. Draw the FOV cone lines
        // Handles.color = Color.yellow;
        // Handles.DrawLine(attack.transform.position, attack.transform.position * attack.attackRange);
        // Handles.DrawLine(attack.transform.position, attack.transform.position * attack.attackRange);

        // 4. Draw line to player if spotted
        if (attack.canAttackPlayer && attack.player != null)
        {
            Handles.color = Color.green;
            Handles.DrawLine(attack.transform.position, attack.player.transform.position);
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
