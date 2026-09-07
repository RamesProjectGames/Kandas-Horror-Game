using UnityEngine;

public class CowTrigger : MonoBehaviour
{
    [Header("Referensi Manager & Sapi")]
    public RandomCowDestroy cowManager; // Drag GameObject CowManager ke sini
    public GameObject myCowIntact;      // Drag Sapi Utuh milik trigger ini

    private bool isDestroyed = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isDestroyed)
        {
            isDestroyed = true;

            // Panggil fungsi hancur di Manager dan kirim referensi sapi ini
            if (cowManager != null && myCowIntact != null)
            {
                cowManager.HancurkanSapiSpesifik(myCowIntact, transform.position);
            }

            // Matikan trigger box agar tidak terpicu 2 kali
            gameObject.SetActive(false);
        }
    }
}
