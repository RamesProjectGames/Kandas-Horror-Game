using UnityEngine;
using UnityEngine.Events;

public class EnemyHitTrigger : MonoBehaviour
{
    public string registeredHitLayerName = "Player";
    public UnityEvent registeredHitEvent;
    public UnityEvent registeredLeaveEvent;
    public UnityEvent HitEvent;
    public UnityEvent LeaveEvent;
    
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("EnemyStop"))
        {
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
            LeaveEvent?.Invoke();
        }
        else if (other.gameObject.CompareTag(registeredHitLayerName))
        {
            registeredLeaveEvent?.Invoke();
        }
    }
}
