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
        if (other.gameObject.CompareTag("EnemyStop"))
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
        if (other.gameObject.CompareTag("EnemyStop"))
        {
            // enemyMovement?.OnEnterEnemyStopZone(false);
            LeaveEvent?.Invoke();
        }
        else if (other.gameObject.CompareTag(registeredHitLayerName))
        {
            registeredLeaveEvent?.Invoke();
        }
    }
}
