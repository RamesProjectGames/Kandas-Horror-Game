using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Light))]
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

    private float targetIntensity;

    private void Awake()
    {
        if (targetLight == null)
            targetLight = GetComponent<Light>();

        if (targetLight != null)
        {
            targetIntensity = targetLight.intensity;
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
                    }
                    else
                    {
                        targetIntensity = 0f;
                    }
                }

                // Swap material at index 1
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
}