using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    public SettingManager settingManager;
    void Start()
    {
        settingManager = SettingManager.Instance;
        UpdateUI();
    }
    #region Graphics Settings UI Elements
    // UI Texts for displaying current values
    public TextMeshProUGUI resolutionText;
    public TextMeshProUGUI frameRateText;
    public TextMeshProUGUI vSyncText;
    public TextMeshProUGUI ditheringText;
    public TextMeshProUGUI bloomText;
    public TextMeshProUGUI grainText;
    public TextMeshProUGUI fogText;
    public TextMeshProUGUI motionBlurText;
    public TextMeshProUGUI vertexJitterText;
    public TextMeshProUGUI textureQualityText;
    


    public void NextResolution() { settingManager.NextResolution(); UpdateUI(); }
    public void PrevResolution() { settingManager.PrevResolution(); UpdateUI(); }
    public void NextFrameRate() { settingManager.NextFrameRate(); UpdateUI(); }
    public void PrevFrameRate() { settingManager.PrevFrameRate(); UpdateUI(); }
    public void ToggleVSync() { settingManager.ToggleVSync(); UpdateUI(); }
    public void ToggleDithering() { settingManager.ToggleDithering(); UpdateUI(); }
    public void ToggleBloom() { settingManager.ToggleBloom(); UpdateUI(); }
    public void ToggleGrain() { settingManager.ToggleGrain(); UpdateUI(); }
    public void ToggleFog() { settingManager.ToggleFog(); UpdateUI(); }
    public void ToggleMotionBlur() { settingManager.ToggleMotionBlur(); UpdateUI(); }
    public void ToggleVertexJitter() { settingManager.ToggleVertexJitter(); UpdateUI(); }
    public void NextTextureQuality() { settingManager.NextTextureQuality(); UpdateUI(); }
    public void PrevTextureQuality() { settingManager.PrevTextureQuality(); UpdateUI(); }

    public void UpdateUI()
    {
        var s = settingManager.settings;
        resolutionText.text = ResolutionToString(s.GameResolution);
        frameRateText.text = s.FrameRate + " FPS";
        vSyncText.text = s.VSync ? "On" : "Off";
        ditheringText.text = s.Dithering ? "On" : "Off";
        bloomText.text = s.Bloom ? "On" : "Off";
        grainText.text = s.Grain ? "On" : "Off";
        fogText.text = s.Fog ? "On" : "Off";
        motionBlurText.text = s.MotionBlur ? "On" : "Off";
        vertexJitterText.text = s.VertexJitter ? "On" : "Off";
        textureQualityText.text = TextureQualityToString(s.TextureQuality);
    }

    private string TextureQualityToString(SettingData.TextureQualityLevel quality)
    {
        switch (quality)
        {
            case SettingData.TextureQualityLevel.Low: return "240p";
            case SettingData.TextureQualityLevel.Medium: return "360p";
            case SettingData.TextureQualityLevel.High: return "480p";
            default: return quality.ToString();
        }
    }

    private string ResolutionToString(SettingData.Resolution res)
    {
        switch (res)
        {
            case SettingData.Resolution.R1920x1080: return "1920 x 1080";
            case SettingData.Resolution.R1600x900: return "1600 x 900";
            case SettingData.Resolution.R1280x720: return "1280 x 720";
            case SettingData.Resolution.R1366x768: return "1366 x 768";
            case SettingData.Resolution.R1920x1200: return "1920 x 1200";
            case SettingData.Resolution.R1680x1050: return "1680 x 1050";
            case SettingData.Resolution.R1440x900: return "1440 x 900";
            case SettingData.Resolution.R1280x800: return "1280 x 800";
            case SettingData.Resolution.R1024x768: return "1024 x 768";
            case SettingData.Resolution.R800x600: return "800 x 600";
            case SettingData.Resolution.R640x480: return "640 x 480";
            default: return res.ToString();
        }
    }
    public void ResetToDefaults() { settingManager.ResetToDefaults(); UpdateUI(); }
    #endregion

    #region Control Settings UI Elements

    #endregion
}
