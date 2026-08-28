using System.Collections;
using UnityEngine;
//[ExecuteAlways]

public class EnvironmentIntensityController : MonoBehaviour
{
    [Header("Environment Lighting")]
    public float intensityMultiplier = 1.5f;

    [Header("Skybox Settings")]
    public Material skyboxMaterial;

    [Header("Fog Settings")]
    public bool enableFog = true;
    public Color fogColor = Color.gray;
    public float fogDensity = 0.01f;

    // Fungsi ini dipanggil dari Timeline saat cutscene dimulai
    public void ApplyEnvironmentSettings()
    {
        RenderSettings.ambientIntensity = intensityMultiplier;

        if (skyboxMaterial != null)
        {
            RenderSettings.skybox = skyboxMaterial;
        }

        RenderSettings.fog = enableFog;
        if (enableFog)
        {
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = fogColor;
            RenderSettings.fogDensity = fogDensity;
        }
    }

    // Dipanggil untuk mengubah Intensity secara bertahap jika di-animasikan
    public void SetIntensity(float value)
    {
        RenderSettings.ambientIntensity = value;
    }
}
