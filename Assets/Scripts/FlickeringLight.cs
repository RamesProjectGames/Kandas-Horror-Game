using System.Collections;
using UnityEngine;


public class FlickeringLight : MonoBehaviour
{
    [Header("Light")]
    public Light targetLight;

    [Range(0f, 1f)]
    public float flickerChance = 0.4f;

    [Range(0f, 10f)]
    public float minIntensity = 0.2f;

    [Range(0f, 10f)]
    public float maxIntensity = 1.2f;

    [Range(0.1f, 20f)]
    public float intensityChangeSpeed = 6f;

    public float minInterval = 0.05f;
    public float maxInterval = 0.2f;

    [Header("Material Swap")]
    public MeshRenderer targetRenderer;
    public Material lightOnMaterial;
    public Material lightOffMaterial;
    public int materialIndex = 1;

    [Header("Audio Settings")]
    [Tooltip("Komponen AudioSource (bisa kosong jika dipasang di GameObject ini)")]
    public AudioSource audioSource;

    [Tooltip("Suara klik/pukulan listrik saat lampu flicker (bisa beberapa variasi audio)")]
    public AudioClip[] flickerSounds;

    [Tooltip("Suara hum/dengung listrik loop saat lampu menyala (opsional)")]
    public AudioClip electricHumSound;

    [Range(0f, 1f)]
    public float sfxVolume = 0.7f;

    [Tooltip("Acak sedikit pitch agar suara flicker tidak monoton")]
    public bool randomizePitch = true;

    private float targetIntensity;
    private AudioSource humAudioSource;

    private void Awake()
    {
        if (targetLight == null)
            targetLight = GetComponent<Light>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (targetLight != null)
        {
            targetIntensity = targetLight.intensity;
        }

        // Buat AudioSource khusus untuk Loop Suara Hum jika disediakan file-nya
        if (electricHumSound != null)
        {
            humAudioSource = gameObject.AddComponent<AudioSource>();
            humAudioSource.clip = electricHumSound;
            humAudioSource.loop = true;
            humAudioSource.playOnAwake = false;
            humAudioSource.spatialBlend = 1f; // 3D Sound
            humAudioSource.volume = sfxVolume * 0.5f;
        }
    }

    private void Start()
    {
        StartCoroutine(FlickerRoutine());
    }

    private void Update()
    {
        if (targetLight == null)
            return;

        if (targetLight.enabled)
        {
            targetLight.intensity = Mathf.MoveTowards(targetLight.intensity, targetIntensity, intensityChangeSpeed * Time.deltaTime);
        }
    }

    IEnumerator FlickerRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minInterval, maxInterval));

            if (Random.value <= flickerChance)
            {
                bool isOn = !targetLight.enabled;

                // Toggle light
                targetLight.enabled = isOn;

                if (targetLight != null)
                {
                    if (isOn)
                    {
                        targetIntensity = Random.Range(minIntensity, maxIntensity);

                        // Play Hum Loop jika ada
                        if (humAudioSource != null && !humAudioSource.isPlaying)
                            humAudioSource.Play();
                    }
                    else
                    {
                        targetIntensity = 0f;

                        // Stop Hum Loop saat lampu mati
                        if (humAudioSource != null && humAudioSource.isPlaying)
                            humAudioSource.Stop();
                    }
                }

                // Play SFX Click/Flicker
                PlayFlickerSound();

                // Swap material at specified index
                if (targetRenderer != null &&
                    materialIndex >= 0 &&
                    materialIndex < targetRenderer.materials.Length)
                {
                    Material[] mats = targetRenderer.materials;
                    mats[materialIndex] = isOn ? lightOnMaterial : lightOffMaterial;
                    targetRenderer.materials = mats;
                }
            }
        }
    }

    private void PlayFlickerSound()
    {
        if (audioSource == null || flickerSounds == null || flickerSounds.Length == 0)
            return;

        // Ambil SFX acak dari array
        AudioClip clip = flickerSounds[Random.Range(0, flickerSounds.Length)];

        if (clip != null)
        {
            if (randomizePitch)
            {
                audioSource.pitch = Random.Range(0.85f, 1.15f);
            }

            audioSource.PlayOneShot(clip, sfxVolume);
        }
    }
}