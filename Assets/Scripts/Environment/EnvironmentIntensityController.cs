using System.Collections;
using UnityEngine;
[ExecuteAlways]
public class EnvironmentIntensityController : MonoBehaviour
{
    [Header("Environment Lighting")]
    [Range(0f, 8f)]
    public float intensityMultiplier = 1.5f;

    [Header("Skybox Settings")]
    public Material skyboxMaterial;

    [Header("Fog Settings")]
    public bool enableFog = true;
    public Color fogColor = Color.gray;
    [Range(0f, 1f)]
    public float fogDensity = 0.01f;

    void Update()
    {
        // Mengubah Intensity Multiplier
        RenderSettings.ambientIntensity = intensityMultiplier;

        // Mengubah Skybox Material (jika diset)
        if (skyboxMaterial != null && RenderSettings.skybox != skyboxMaterial)
        {
            RenderSettings.skybox = skyboxMaterial;
        }

        // Mengatur Fog
        RenderSettings.fog = enableFog;
        if (enableFog)
        {
            RenderSettings.fogMode = FogMode.ExponentialSquared; // Mode standar untuk density
            RenderSettings.fogColor = fogColor;
            RenderSettings.fogDensity = fogDensity;
        }
    }
}
