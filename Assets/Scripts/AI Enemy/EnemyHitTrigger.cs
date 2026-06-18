using UnityEngine;
using UnityEngine.Events;

public class EnemyHitTrigger : MonoBehaviour
{
    public LayerMask registeredHitLayer;
    public LayerMask groundLayer;
    public UnityEvent registeredHitEvent;
    public UnityEvent HitEvent;
    public UnityEvent LeaveEvent;
    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.layer == registeredHitLayer)
        {
            registeredHitEvent?.Invoke();
            var settingUI = FindAnyObjectByType<SettingsUI>();
            if(settingUI !=null)
            {
                settingUI.ShowGameover(true);
            }
        }
        else if(collision.gameObject.layer == groundLayer)
        {
            // TO DO : play hit ground sound
        }
        if (collision.articulationBody.CompareTag("EnemyStop"))
        {
        }
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("EnemyStop"))
        {
            HitEvent?.Invoke();
        }
    }
    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("EnemyStop"))
        {
            LeaveEvent?.Invoke();
        }
    }
}
