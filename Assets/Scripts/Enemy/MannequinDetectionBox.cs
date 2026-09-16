using UnityEngine;

public class MannequinDetectionBox : MonoBehaviour
{
    public bool isInDetectionRange;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isInDetectionRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isInDetectionRange = false;
        }
    }
    
}
