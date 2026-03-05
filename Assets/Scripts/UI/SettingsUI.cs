using TMPro;
using UnityEngine;
using UnityEngine.UI;
using FMOD;
using FMODUnity;
using System.Collections.Generic;
using System;
using UnityEngine.InputSystem.Samples.RebindUI;
using Unity.Mathematics;

public class SettingsUI : MonoBehaviour
{
    public SettingManager settingManager;
    void Start()
    {
        settingManager = SettingManager.Instance;
        UpdateUI();
        PopulateAudioInputDevices();
        PopulateAudioOutputDevices();
        PopulateLanguageOptions();
        ScrollToSection(0);
        HighlightSectionButton(0);
        CloseSettings();
    }
    void Update()
    {
        
    }
    #region Graphics Settings UI Elements
    [Header("Graphics Settings UI Elements")]
    // UI Texts for displaying current values
    public TextMeshProUGUI resolutionText;
    public TextMeshProUGUI frameRateText;
    public TextMeshProUGUI vSyncText;
    public TextMeshProUGUI ditheringText;
    public TextMeshProUGUI bloomText;
    public TextMeshProUGUI grainText;
    public Slider fogText;
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
    public void SetFogDensity(float value) { settingManager.SetFogValue(value); UpdateUI(); }
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
        fogText.value = Mathf.InverseLerp(settingManager.minimumFogDistance, settingManager.maximumFogDistance, s.Fog);
        motionBlurText.text = s.MotionBlur ? "On" : "Off";
        vertexJitterText.text = s.VertexJitter ? "On" : "Off";
        textureQualityText.text = TextureQualityToString(s.TextureQuality);

        // Control Settings
        microphoneSensitivitySlider.value = Mathf.InverseLerp(settingManager.minimumMicrophoneVolume, settingManager.maximumMicrophoneVolume, s.MicrophoneSensitivity);
        mouseSensitivitySlider.value = Mathf.InverseLerp(settingManager.minimumMouseSensitivity, settingManager.maximumMouseSensitivity, s.MouseSensitivity);
        sprintToggleText.text = s.SprintToggle ? "Toggle" : "Hold";
        // Set selected audio input device in dropdown
        int micIndex = System.Array.IndexOf(Microphone.devices, s.AudioInputDeviceName);
        if (micIndex >= 0)
        {
            microphoneDropdown.value = micIndex;
        }
        else
        {
            microphoneDropdown.value = 0; // Default to first device if not found
        }

        // Audio Settings
        masterVolumeSlider.value = s.MasterVolume;
        musicVolumeSlider.value = s.MusicVolume;
        soundEffectVolumeSlider.value = s.SoundEffectVolume;
        mobVolumeSlider.value = s.MobVolume;

        // Language Settings
        languageDropdown.value = (int)s.GameLanguage;
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
            default: return res.ToString();
        }
    }
    public void ResetToDefaults() { settingManager.ResetGrapichsToDefaults(); UpdateUI(); }
    #endregion

    #region Control Settings UI Elements

    [Header("Control Settings UI Elements")]
    public TMP_Dropdown microphoneDropdown;
    public TMP_Dropdown outputDropdown;
    public Slider microphoneSensitivitySlider;
    public Slider mouseSensitivitySlider;
    public TextMeshProUGUI sprintToggleText;    
    [SerializeField] private List<RebindActionUI> rebindActions = new List<RebindActionUI>();
    public void SetRebinding(bool isRebinding) => settingManager.SetRebind(isRebinding);
    public void PopulateAudioInputDevices()
    {
        var core = RuntimeManager.CoreSystem;

        int numDrivers = 0;
        int numConnected = 0;
        core.getRecordNumDrivers(out numDrivers, out numConnected);

        if(numDrivers == 0)
        {
            microphoneDropdown.ClearOptions();
            microphoneDropdown.options.Add(new TMP_Dropdown.OptionData("No Output Devices Detected"));
            microphoneDropdown.interactable = false;
            return;
        }

        List<string> options = new List<string>();

        for (int i = 0; i < numDrivers; i++)
        {
            string name;
            Guid guid;
            int rate;
            SPEAKERMODE mode;
            int channels;
            DRIVER_STATE state;

            core.getRecordDriverInfo(
                i,
                out name,
                256,
                out guid,
                out rate,
                out mode,
                out channels,
                out state
            );

            options.Add(name);
        }

        microphoneDropdown.ClearOptions();
        microphoneDropdown.AddOptions(options);

        if(string.IsNullOrEmpty(settingManager.settings.AudioInputDeviceName))
        {
            if(options.Count > 0) settingManager.settings.AudioInputDeviceName = options[0];
        }
        
        // if(Microphone.devices.Length == 0)
        // {
        //     microphoneDropdown.options.Add(new TMP_Dropdown.OptionData("No Microphone Detected"));
        //     microphoneDropdown.interactable = false;
        // }
        // else
        // {
        //     foreach (var device in Microphone.devices)
        //     {
        //         microphoneDropdown.options.Add(new TMP_Dropdown.OptionData(device));
        //     }
        // }
           
    }
    public void PopulateAudioOutputDevices()
    {

        var coreSystem = RuntimeManager.CoreSystem;

        int numDrivers = 0;
        coreSystem.getNumDrivers(out numDrivers);

        if(numDrivers == 0)
        {
            outputDropdown.ClearOptions();
            outputDropdown.options.Add(new TMP_Dropdown.OptionData("No Output Devices Detected"));
            outputDropdown.interactable = false;
            return;
        }
        
        List<string> options = new List<string>();

        for (int i = 0; i < numDrivers; i++)
        {
            string name;
            Guid guid;
            int rate;
            SPEAKERMODE mode;
            int channels;

            coreSystem.getDriverInfo(i, out name, 256, out guid, out rate, out mode, out channels);

            options.Add(name);
        }

        outputDropdown.ClearOptions();
        outputDropdown.AddOptions(options);
    }
    public void SetAudioInput(int index)
    {
        settingManager.SelectAudioInputDevice(index);
    }
    public void SetAudioOutput(int index)
    {
        settingManager.SelectAudioOutputDevice(index);
    }
    public void SetMicrophoneSensitivity(float value)
    {
        settingManager.SetMicrophoneSensitivity(value);
    }
    public void SetMouseSensitivity(float value)
    {
        settingManager.SetMouseSensitivity(value);
    }
    public void ToggleSprintMode() {
        settingManager.ToggleSprintToggle();
        sprintToggleText.text = settingManager.settings.SprintToggle ? "Toggle" : "Hold";
    }
    public void ResetControlSettingsToDefaults() 
    { 
        settingManager.ResetControlsToDefaults();
        foreach (var rebindUI in rebindActions)
        {
            rebindUI.ResetToDefault();
        }
        UpdateUI(); 
    }
    #endregion

    #region Audio Settings UI Elements

    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider soundEffectVolumeSlider;
    public Slider mobVolumeSlider;

    public void SetMasterVolume(float value)
    {
        settingManager.SetMasterVolume(value);
    }
    public void SetMusicVolume(float value)
    {
        settingManager.SetMusicVolume(value);
    }
    public void SetSoundEffectVolume(float value)
    {
        settingManager.SetSoundEffectVolume(value);
    }
    public void SetMobVolume(float value)
    {
        settingManager.SetMobVolume(value);
    }
    public void ResetAudioSettingsToDefaults() { settingManager.ResetAudioToDefaults(); UpdateUI(); }

    #endregion

    #region Language Settings UI Elements

    public TMP_Dropdown languageDropdown;
    public void PopulateLanguageOptions()
    {
        languageDropdown.ClearOptions();
        foreach (var lang in System.Enum.GetNames(typeof(SettingData.Language)))
        {
            languageDropdown.options.Add(new TMP_Dropdown.OptionData(lang));
        }
    }
    public void SetLanguage(int index) { settingManager.SetLanguage(index); }
    public void ResetLanguageToDefault() { settingManager.ResetLanguageToDefault(); UpdateUI(); }
    #endregion

    #region UI Navigation

    [Header("UI Navigation")]
    public GameObject PausePanel;
    public GameObject SettingPanel;
    public List<GameObject> scrollings = new List<GameObject>();
    public List<Button> sectionButtons = new List<Button>();

    public void ScrollToSection(int index)
    {
        for (int i = 0; i < scrollings.Count; i++)
        {
            scrollings[i].SetActive(i == index);
        }
    }
    public void HighlightSectionButton(int index)
    {
        for (int i = 0; i < sectionButtons.Count; i++)
        {
            sectionButtons[i].interactable = i != index;
            sectionButtons[i].gameObject.GetComponentInChildren<TextMeshProUGUI>().fontStyle = i == index ? FontStyles.Underline : FontStyles.Normal;
        }
    }
    public void OpenSettings() { SettingPanel.SetActive(true); PausePanel.SetActive(false); }
    public void CloseSettings() { SettingPanel.SetActive(false); PausePanel.SetActive(true);}
    public void PausePanelToggle(bool state = false) 
    { 
        settingManager.isPaused = state;
        if(!SettingPanel.activeInHierarchy)
        {
            PausePanel.SetActive(state);            
        }
        else
        {
            PausePanel.SetActive(false);
        }
    }
    #endregion
}
