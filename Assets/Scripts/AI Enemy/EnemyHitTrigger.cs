using UnityEngine;
using UnityEngine.Events;

public class EnemyHitTrigger : MonoBehaviour
{
    public string registeredHitLayerName = "Player";
    public UnityEvent registeredHitEvent;
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
            Debug.Log("EnemyHitTrigger: Player hit by enemy");
            registeredHitEvent?.Invoke();
            var settingUI = FindAnyObjectByType<SettingsUI>();
            if (settingUI != null)
            {
                settingUI.ShowGameover(true);
            }
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
