using UnityEngine;
using System.Collections.Generic;

public class PlayerSightInteraction : MonoBehaviour
{
    [Header("Sight Detection")]
    [SerializeField] private float sightRange = 30f;
    [SerializeField] private float fieldOfViewAngle = 60f;
    [SerializeField] private float eyeOffset = 1.5f;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private string enemyTag = "Enemy";

    [SerializeField]private List<Transform> visibleEnemies = new List<Transform>();
    [SerializeField]private bool canSeeAnyEnemy = false;
    [SerializeField]private Camera playerCamera;
    private Vector3 startingPosition;

    // Events
    public delegate void EnemySightEvent(Transform enemy, bool isVisible);
    public event EnemySightEvent OnEnemyStateChanged;

    void Start()
    {
        startingPosition = transform.position;
        if (obstacleLayer == 0)
        {
            obstacleLayer = LayerMask.GetMask("Default");
        }
    }

    void LateUpdate()
    {
        DetectEnemies();
    }

    /// <summary>
    /// Main detection method that checks for enemies in the field of view
    /// </summary>
    private void DetectEnemies()
    {
        List<Transform> previouslyVisible = new List<Transform>(visibleEnemies);
        visibleEnemies.Clear();
        canSeeAnyEnemy = false;

        // Find all enemies in scene
        GameObject[] allEnemies = GameObject.FindGameObjectsWithTag(enemyTag);

        foreach (GameObject enemy in allEnemies)
        {
            if (CanSeeEnemy(enemy.transform))
            {
                visibleEnemies.Add(enemy.transform);
                canSeeAnyEnemy = true;

                // Trigger event if this enemy just became visible
                if (!previouslyVisible.Contains(enemy.transform))
                {
                    OnEnemyStateChanged?.Invoke(enemy.transform, true);
                }
            }
            else
            {
                canSeeAnyEnemy = false;
                // Trigger event if this enemy just became invisible
                if (previouslyVisible.Contains(enemy.transform))
                {
                    OnEnemyStateChanged?.Invoke(enemy.transform, false);
                }
            }
        }
    }

    /// <summary>
    /// Checks if a specific enemy is visible to the player
    /// </summary>
    private bool CanSeeEnemy(Transform enemy)
    {
        // Check distance
        Vector3 directionToEnemy = enemy.position - transform.position;
        float distanceToEnemy = directionToEnemy.magnitude;

        if (distanceToEnemy > sightRange)
        {
            return false;
        }

        // Check field of view
        if (!IsInFieldOfView(directionToEnemy))
        {
            return false;
        }

        // Check line of sight
        if (!HasLineOfSight(enemy, distanceToEnemy))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Checks if target is within the player's field of view cone
    /// </summary>
    private bool IsInFieldOfView(Vector3 directionToTarget)
    {
        // 1. Convert enemy 3D world position to 2D screen coordinates (0 to 1)
        Vector3 viewportPoint = Camera.main.WorldToViewportPoint(directionToTarget);
        // 2. Check if enemy is in front of the camera and inside screen edges
        bool isAhead = viewportPoint.z > 0;
        bool isInsideX = viewportPoint.x > 0 && viewportPoint.x < 1;
        bool isInsideY = viewportPoint.y > 0 && viewportPoint.y < 1;
        return isAhead && isInsideX && isInsideY;
        //Vector3 viewDirection = playerCamera != null ? playerCamera.transform.forward : transform.forward;
        //float angleToTarget = Vector3.Angle(viewDirection, directionToTarget);
        //return angleToTarget <= fieldOfViewAngle / 2f;
    }

    /// <summary>
    /// Checks if there are obstacles between player and enemy
    /// </summary>
    private bool HasLineOfSight(Transform enemy, float distanceToEnemy)
    {
        Vector3 directionToEnemy = (enemy.position - transform.position).normalized;
        
        if (Physics.Raycast(transform.position, directionToEnemy, out RaycastHit hit, distanceToEnemy, obstacleLayer))
        {
            // Check if the raycast hit the enemy or something behind it
            if (hit.transform != enemy && !hit.transform.IsChildOf(enemy))
            {
                return false; // Hit an obstacle before reaching enemy
            }
        }

        return true;
    }

    /// <summary>
    /// Returns the starting position of the player
    /// </summary>
    public Vector3 GetStartingPosition()
    {
        return startingPosition;
    }

    /// <summary>
    /// Resets player to starting position
    /// </summary>
    public void ResetToStartingPosition()
    {
        transform.position = startingPosition;
    }

    /// <summary>
    /// Returns whether the player can see any enemy
    /// </summary>
    public bool CanSeeAnyEnemy()
    {
        return canSeeAnyEnemy;
    }

    /// <summary>
    /// Returns list of currently visible enemies
    /// </summary>
    public List<Transform> GetVisibleEnemies()
    {
        return new List<Transform>(visibleEnemies);
    }

    /// <summary>
    /// Returns the closest visible enemy, or null if none visible
    /// </summary>
    public Transform GetClosestVisibleEnemy()
    {
        if (visibleEnemies.Count == 0)
            return null;

        Transform closest = visibleEnemies[0];
        float closestDistance = Vector3.Distance(transform.position, closest.position);

        foreach (Transform enemy in visibleEnemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.position);
            if (distance < closestDistance)
            {
                closest = enemy;
                closestDistance = distance;
            }
        }

        return closest;
    }

    /// <summary>
    /// Visualize the field of view in the editor
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!enabled) return;

        // Use camera forward if available, otherwise use transform forward
        Camera cam = Camera.main;
        Vector3 viewDirection = cam != null ? cam.transform.forward : transform.forward;

        Vector3 visionPos = new Vector3(transform.position.x, cam != null ? cam.transform.position.y : transform.position.y, transform.position.z);
        // Draw sight range circle
        Gizmos.color = Color.yellow;
        DrawCircle(visionPos, sightRange, 32);

        // Draw field of view cone based on camera direction
        Gizmos.color = Color.cyan;
        Vector3 leftBound = Quaternion.Euler(0, -fieldOfViewAngle / 2f, 0) * viewDirection * sightRange;
        Vector3 rightBound = Quaternion.Euler(0, fieldOfViewAngle / 2f, 0) * viewDirection * sightRange;
        
        Gizmos.DrawLine(visionPos, visionPos + leftBound);
        Gizmos.DrawLine(visionPos, visionPos + rightBound);

        // Draw visible enemies
        Gizmos.color = Color.green;
        foreach (Transform enemy in visibleEnemies)
        {
            Gizmos.DrawLine(visionPos, enemy.position);
        }
    }

    private void DrawCircle(Vector3 center, float radius, int segments)
    {
        float angle = 0f;
        float angleStep = 360f / segments;
        Vector3 lastPoint = center + new Vector3(radius, 0, 0);

        for (int i = 0; i < segments; i++)
        {
            angle += angleStep;
            float radians = angle * Mathf.Deg2Rad;
            Vector3 newPoint = center + new Vector3(Mathf.Cos(radians) * radius, 0, Mathf.Sin(radians) * radius);
            Gizmos.DrawLine(lastPoint, newPoint);
            lastPoint = newPoint;
        }
    }
}
