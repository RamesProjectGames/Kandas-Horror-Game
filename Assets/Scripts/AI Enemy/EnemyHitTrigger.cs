using UnityEngine;
using UnityEngine.Events;

public class EnemyHitTrigger : MonoBehaviour
{
    public LayerMask registeredHitLayer;
    public LayerMask groundLayer;
    public UnityEvent HitEvent;
    public UnityEvent LeaveEvent;
    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.layer == registeredHitLayer)
        {
            // TO DO : one hit kill player
        }
        else if(collision.gameObject.layer == groundLayer)
        {
            // TO DO : play hit ground sound
        }
        if (collision.articulationBody.CompareTag("EnemyStop"))
        {
            HitEvent?.Invoke();
        }
    }
    void OnCollisionExit(Collision collision)
    {
        if (collision.articulationBody.CompareTag("EnemyStop"))
        {
            LeaveEvent?.Invoke();
        }
    }
}
