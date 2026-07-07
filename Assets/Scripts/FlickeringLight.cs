using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Light))]
public class FlickeringLight : MonoBehaviour
{
    [Header("Light")]
    public Light targetLight;

    [Range(0f, 1f)]
    public float flickerChance = 0.4f;

    public float minInterval = 0.05f;
    public float maxInterval = 0.2f;

    [Header("Material Swap")]
    public MeshRenderer targetRenderer;
    public Material lightOnMaterial;
    public Material lightOffMaterial;
    public int materialIndex = 1;

    private void Awake()
    {
        if (targetLight == null)
            targetLight = GetComponent<Light>();
    }

    private void Start()
    {
        StartCoroutine(FlickerRoutine());
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