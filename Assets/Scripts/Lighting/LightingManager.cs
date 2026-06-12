using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class LightingManager : MonoBehaviour
{
    //Scene References
    [SerializeField] private Light DirectionalLight;
    [SerializeField] private LightingPreset Preset;
    [SerializeField] private Material SkyboxMaterial;
    //Variables
    [SerializeField, Range(0, 24)] private float TimeOfDay;
    [SerializeField] private List<Light> PointLights = new List<Light>();
    [SerializeField] private List<ReflectionProbe> ReflectionProbes = new List<ReflectionProbe>();
    private void Start()
    {
        if (Preset == null)
        {
            Debug.LogError("No lighting preset assigned to " + name);
        }
        
    }
    private void Update()
    {
        if (Preset == null)
            return;

        if (Application.isPlaying)
        {
            //(Replace with a reference to the game time)
            // TimeOfDay += Time.deltaTime;
        }
        TimeOfDay %= 24; //Modulus to ensure always between 0-24
        UpdateLighting(TimeOfDay / 24f);
    }


    private void UpdateLighting(float timePercent)
    {
        //Set ambient and fog
        RenderSettings.ambientLight = Preset.AmbientColor.Evaluate(timePercent);
        RenderSettings.fogDensity =Preset.FogDensity.Evaluate(timePercent);
        RenderSettings.fogColor = Preset.FogColor.Evaluate(timePercent);
        RenderSettings.reflectionIntensity = Preset.ReflectionIntensity.Evaluate(timePercent);
        float multiplier = Preset.PointLightMultiplier.Evaluate(timePercent);
        float intensity = Mathf.Lerp(0f, 1f, Preset.AmbientIntensity.Evaluate(timePercent));

        //If the directional light is set then rotate and set it's color, I actually rarely use the rotation because it casts tall shadows unless you clamp the value
        if (DirectionalLight != null)
        {
            DirectionalLight.color = Preset.DirectionalColor.Evaluate(timePercent);

            DirectionalLight.intensity = Preset.DirectionalIntensity.Evaluate(timePercent);

            DirectionalLight.transform.localRotation = Quaternion.Euler(new Vector3((timePercent * 360f) - 90f, 170f, 0));
        }

        UpdateSkybox(timePercent);

        foreach (Light light in PointLights)
        {
            if (light == null)
                continue;

            light.intensity =
                light.GetComponent<OriginalIntensity>().BaseIntensity
                * multiplier;
        }
        foreach (ReflectionProbe probe in ReflectionProbes)
        {
            if (probe == null)
                continue;

            probe.intensity = intensity;
        }
    }
    private void UpdateSkybox(float timePercent)
    {
        if (SkyboxMaterial == null)
            return;

        SkyboxMaterial.SetColor(
        "_Tint",
        Preset.SkyboxTint.Evaluate(timePercent));

        SkyboxMaterial.SetFloat(
            "_Exposure",
            Preset.SkyboxExposure.Evaluate(timePercent));

        SkyboxMaterial.SetFloat(
            "_Rotation",
            Preset.SkyboxRotation.Evaluate(timePercent));

        DynamicGI.UpdateEnvironment();
        RefreshReflections();
    }
    //Try to find a directional light to use if we haven't set one
    private void OnValidate()
    {
        SkyboxMaterial = RenderSettings.skybox;
        
        if (DirectionalLight != null)
            return;
        
        PopulateLights();
        
        //Search for lighting tab sun
        if (RenderSettings.sun != null)
        {
            DirectionalLight = RenderSettings.sun;
        }
        //Search scene for light that fits criteria (directional)
        else
        {
            Light[] lights = FindObjectsByType<Light>(FindObjectsInactive.Include,FindObjectsSortMode.None);
            foreach (Light light in lights)
            {
                if (light.type == LightType.Directional)
                {
                    DirectionalLight = light;
                    return;
                }
            }
        }        
    }
    [ContextMenu("Populate Point Lights")]
    public void PopulateLights()
    {
        PointLights.Clear();
        Light[] pointLights = FindObjectsByType<Light>(FindObjectsInactive.Include,FindObjectsSortMode.None);
        foreach (Light light in pointLights)
        {
            if (light.type == LightType.Point)
            {
                if (light.transform.GetComponent<OriginalIntensity>() == null)
                    light.gameObject.AddComponent<OriginalIntensity>();
                PointLights.Add(light);
            }
        }
        ReflectionProbes.Clear();
        ReflectionProbes = new List<ReflectionProbe>(FindObjectsByType<ReflectionProbe>(FindObjectsInactive.Include,FindObjectsSortMode.None));
    }
    private void RefreshReflections()
    {
        foreach (var probe in ReflectionProbes)
        {
            if (probe != null)
                probe.RenderProbe();
        }
    }
}
public class OriginalIntensity : MonoBehaviour
{
    public float BaseIntensity = 1f;

    private void Awake()
    {
        BaseIntensity = GetComponent<Light>().intensity;
    }
}