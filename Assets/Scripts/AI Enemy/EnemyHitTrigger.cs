using UnityEngine;
using UnityEngine.Events;

public class EnemyHitTrigger : MonoBehaviour
{
    public string registeredHitLayerName = "Player";
    public UnityEvent registeredHitEvent;
    public UnityEvent registeredLeaveEvent;
    public UnityEvent HitEvent;
    public UnityEvent LeaveEvent;

    private EnemyMovement enemyMovement;

    private void Awake()
    {
        enemyMovement = GetComponentInParent<EnemyMovement>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (IsEnemyStopZone(other))
        {
            Debug.Log("EnemyStop Triggered");
            enemyMovement?.OnEnterEnemyStopZone(true);
            HitEvent?.Invoke();
        }
        if (other.gameObject.CompareTag(registeredHitLayerName))
        {
            registeredHitEvent?.Invoke();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (IsEnemyStopZone(other))
        {
            enemyMovement?.OnEnterEnemyStopZone(false);
            LeaveEvent?.Invoke();
        }
        else if (other.gameObject.CompareTag(registeredHitLayerName))
        {
            registeredLeaveEvent?.Invoke();
        }
    }

    private static bool IsEnemyStopZone(Collider other)
    {
        return other.CompareTag("EnemyStop") || other.GetComponentInParent<Transform>().CompareTag("EnemyStop");
    }
}
