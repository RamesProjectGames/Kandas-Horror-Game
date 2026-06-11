using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName ="Lighting Preset", menuName ="Scriptables/Lighting Preset",order =1)]
public class LightingPreset : ScriptableObject
{
    public Gradient AmbientColor;
    public Gradient DirectionalColor;
    public Gradient FogColor;


    [Header("Intensity")]
    public AnimationCurve DirectionalIntensity;
    public AnimationCurve AmbientIntensity;
    public AnimationCurve FogDensity;

    [Header("Point Lights")]
    public AnimationCurve PointLightMultiplier;

    [Header("Skybox")]
public Gradient SkyboxTint;
public AnimationCurve SkyboxExposure;
public AnimationCurve SkyboxRotation;
    public AnimationCurve ReflectionIntensity;
}