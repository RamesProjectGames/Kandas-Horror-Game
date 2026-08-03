
using FMODUnity;
using System.Collections;
using UnityEngine;

public class LightningEffect : MonoBehaviour
{
    private Light lightningLight;
    private StudioEventEmitter thunderAudio; // Variabel untuk audio petir
    private float defaultIntensity;

    [Header("Pengaturan Durasi & Interval")]
    public float minTimeBetweenStrikes = 2f;
    public float maxTimeBetweenStrikes = 6f;

    void Start()
    {
        lightningLight = GetComponent<Light>();
        thunderAudio = GetComponent<StudioEventEmitter>(); // Mengambil komponen Audio Source di objek yang sama

        defaultIntensity = lightningLight.intensity;
        StartCoroutine(LightningRoutine());
    }

    IEnumerator LightningRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minTimeBetweenStrikes, maxTimeBetweenStrikes));

            // Efek kedip kilatan cahaya
            int flashes = Random.Range(2, 5);
            for (int i = 0; i < flashes; i++)
            {
                lightningLight.intensity = Random.Range(3f, 8f);
                yield return new WaitForSeconds(Random.Range(0.05f, 0.15f));
                lightningLight.intensity = defaultIntensity;
                yield return new WaitForSeconds(Random.Range(0.02f, 0.08f));
            }

            lightningLight.intensity = defaultIntensity;

            // Putar suara petir saat kilatan terjadi (bisa diberi sedikit delay jika ingin mensimulasikan jarak suara)
            if (thunderAudio != null && !thunderAudio.EventReference.IsNull)
            {
                thunderAudio.Play();
            }
        }
    }
}
