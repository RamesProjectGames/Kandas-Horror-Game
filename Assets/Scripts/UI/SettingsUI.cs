using Dialogue;
using FMOD;
using FMODUnity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.Samples.RebindUI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    void Start()
    {
        PopulateAudioInputDevices();
        PopulateAudioOutputDevices();
        PopulateLanguageOptions();
        ScrollToSection(0);
        HighlightSectionButton(0);
        UpdateUI();
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
    public Slider gammaIntensity;
    public TextMeshProUGUI motionBlurText;
    public TextMeshProUGUI vertexJitterText;
    public TextMeshProUGUI textureQualityText;
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private List<LoadingBarPercentages> loadingPercentages = new List<LoadingBarPercentages>();
    [SerializeField] private Image loadingBar;
    float totalProgress;


    public void NextResolution() { SettingManager.Instance.NextResolution(); UpdateUI(); }
    public void PrevResolution() { SettingManager.Instance.PrevResolution(); UpdateUI(); }
    public void NextFrameRate() { SettingManager.Instance.NextFrameRate(); UpdateUI(); }
    public void PrevFrameRate() { SettingManager.Instance.PrevFrameRate(); UpdateUI(); }
    public void ToggleVSync() { SettingManager.Instance.ToggleVSync(); UpdateUI(); }
    public void ToggleDithering() { SettingManager.Instance.ToggleDithering(); UpdateUI(); }
    public void ToggleBloom() { SettingManager.Instance.ToggleBloom(); UpdateUI(); }
    public void ToggleGrain() { SettingManager.Instance.ToggleGrain(); UpdateUI(); }
    public void SetGammaIntensity(float value) { SettingManager.Instance.SetGammaValue(value); UpdateUI(); }
    public void ToggleMotionBlur() { SettingManager.Instance.ToggleMotionBlur(); UpdateUI(); }
    public void ToggleVertexJitter() { SettingManager.Instance.ToggleVertexJitter(); UpdateUI(); }

    public void UpdateUI()
    {
        var s = SettingManager.Instance.settings;
        resolutionText.text = ResolutionToString(s.GameResolution);
        frameRateText.text = s.FrameRate + " FPS";
        vSyncText.text = s.VSync ? "On" : "Off";
        ditheringText.text = s.Dithering ? "On" : "Off";
        bloomText.text = s.Bloom ? "On" : "Off";
        grainText.text = s.Grain ? "On" : "Off";
        gammaIntensity.value = Mathf.Clamp(s.gamma, SettingManager.Instance.minimumGammaIntensity, SettingManager.Instance.maximumGammaIntensity);
        motionBlurText.text = s.MotionBlur ? "On" : "Off";
        vertexJitterText.text = s.VertexJitter ? "On" : "Off";

        // Control Settings
        mouseSensitivitySlider.minValue = SettingManager.Instance.minimumMouseSensitivity;
        mouseSensitivitySlider.maxValue = SettingManager.Instance.maximumMouseSensitivity;
        mouseSensitivitySlider.value = s.MouseSensitivity;
        sprintToggleText.text = s.SprintToggle ? "Toggle" : "Hold";
        crouchToggleText.text = s.CrouchToggle ? "Toggle" : "Hold";

        // Audio Settings
        microphoneSensitivitySlider.minValue = SettingManager.Instance.minimumMicrophoneVolume;
        microphoneSensitivitySlider.maxValue = SettingManager.Instance.maximumMicrophoneVolume;
        microphoneSensitivitySlider.value = s.MicrophoneSensitivity;
        microphoneDropdown.value = microphoneDropdown.options.IndexOf(microphoneDropdown.options.Find(x => x.text == s.AudioInputDeviceName));
        outputDropdown.value = s.AudioOutputDeviceIndex;
        int micIndex = System.Array.IndexOf(Microphone.devices, s.AudioInputDeviceName);
        if (micIndex >= 0)
        {
            microphoneDropdown.value = micIndex;
        }
        else
        {
            microphoneDropdown.value = 0; // Default to first device if not found
        }
        masterVolumeSlider.value = s.MasterVolume;
        musicVolumeSlider.value = s.MusicVolume;
        soundEffectVolumeSlider.value = s.SoundEffectVolume;
        mobVolumeSlider.value = s.MobVolume;

        // Language Settings
        languageDropdown.value = (int)s.GameLanguage;
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
    public void ResetToDefaults() { SettingManager.Instance.ResetGraphicsToDefaults(); UpdateUI(); }
    public void ConfirmResetSettings() {ConfirmationUI.Instance.SetConfirmationUI("Reset Setting to defaults?", () => ResetToDefaults());}
    #endregion

    #region Control Settings UI Elements

    [Header("Control Settings UI Elements")]
    public TMP_Dropdown microphoneDropdown;
    public TMP_Dropdown outputDropdown;
    public Slider mouseSensitivitySlider;
    public TextMeshProUGUI sprintToggleText;  
    public TextMeshProUGUI crouchToggleText;  
    [SerializeField] private List<RebindActionUI> rebindActions = new List<RebindActionUI>();
    public void SetRebinding(bool isRebinding) => StartCoroutine(RebindCoroutine(isRebinding));

    public IEnumerator RebindCoroutine(bool isRebinding)
    {
        while(SettingManager.Instance == null)
        {
            yield return null;
        }
        SettingManager.Instance.SetRebind(isRebinding);
    }
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

        if(string.IsNullOrEmpty(SettingManager.Instance.settings.AudioInputDeviceName))
        {
            if(options.Count > 0) SettingManager.Instance.settings.AudioInputDeviceName = options[0];
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
        SettingManager.Instance.SelectAudioInputDevice(index);
    }
    public void SetAudioOutput(int index)
    {
        SettingManager.Instance.SelectAudioOutputDevice(index);
    }
    public void SetMicrophoneSensitivity(float value)
    {
        SettingManager.Instance.SetMicrophoneSensitivity(value);
    }
    public void SetMouseSensitivity(float value)
    {
        SettingManager.Instance.SetMouseSensitivity(value);
    }
    public void ToggleSprintMode() {
        SettingManager.Instance.ToggleSprintToggle();
        sprintToggleText.text = SettingManager.Instance.settings.SprintToggle ? "Toggle" : "Hold";
    }
    public void ToggleCrouchMode()
    {
        SettingManager.Instance.ToggleCrouch();
        crouchToggleText.text = SettingManager.Instance.settings.CrouchToggle ? "Toggle" : "Hold";
    }
    public void ResetControlSettingsToDefaults() 
    { 
        SettingManager.Instance.ResetControlsToDefaults();
        foreach (var rebindUI in rebindActions)
        {
            rebindUI.ResetToDefault();
        }
        UpdateUI(); 
    }
    public void ConfirmResetControls() { ConfirmationUI.Instance.SetConfirmationUI("Are you sure you want to reset your controls?", () => ResetControlSettingsToDefaults()); }
    #endregion

    #region Audio Settings UI Elements

    public Slider microphoneSensitivitySlider;
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider soundEffectVolumeSlider;
    public Slider mobVolumeSlider;

    public void SetMasterVolume(float value)
    {
        SettingManager.Instance.SetMasterVolume(value);
    }
    public void SetMusicVolume(float value)
    {
        SettingManager.Instance.SetMusicVolume(value);
    }
    public void SetSoundEffectVolume(float value)
    {
        SettingManager.Instance.SetSoundEffectVolume(value);
    }
    public void SetMobVolume(float value)
    {
        SettingManager.Instance.SetMobVolume(value);
    }
    public void ResetAudioSettingsToDefaults() { SettingManager.Instance.ResetAudioToDefaults(); UpdateUI(); }
    public void ConfirmResetAudio() { ConfirmationUI.Instance.SetConfirmationUI("Reset audio to defaults?", () => ResetAudioSettingsToDefaults()); }

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
    public void SetLanguage(int index) { SettingManager.Instance.SetLanguage(index); }
    public void ResetLanguageToDefault() { SettingManager.Instance.ResetLanguageToDefault(); UpdateUI(); }
    public void ConfirmResetLanguage() { ConfirmationUI.Instance.SetConfirmationUI("Reset language to default?", () => ResetLanguageToDefault()); }
    #endregion

    #region UI Navigation

    [Header("UI Navigation")]
    [SerializeField] private GameObject PausePanel;
    [SerializeField] private GameObject SettingPanel;
    [SerializeField] private GameObject GameoverPanel;
    [SerializeField] private GameObject demoEndPanel;
    [SerializeField] private List<GameObject> scrollings = new List<GameObject>();
    [SerializeField] private List<Button> sectionButtons = new List<Button>();

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
    public void OpenSettings() { SettingPanel.SetActive(true); if(PausePanel != null) PausePanel.SetActive(false); }
    public void CloseSettings() { SettingPanel.SetActive(false); if(PausePanel != null) PausePanel.SetActive(true);}
    public void PausePanelToggle() 
    {
        if(FindAnyObjectByType<ConfirmationUI>() != null && FindAnyObjectByType<ConfirmationUI>().transform.localScale.x > 0)
        {
            UIAudioManager.Instance.PlayCancelSound();
            FindAnyObjectByType<ConfirmationUI>().Cancel();
        }
        else if(FindAnyObjectByType<ChapterManager>() != null && FindAnyObjectByType<ChapterManager>().chapterPanel.activeInHierarchy)
        {
            UIAudioManager.Instance.PlayCancelSound();
            FindAnyObjectByType<ChapterManager>().ChangePanelState(false);
        }
        else if(SettingPanel.activeInHierarchy)
        {
            UIAudioManager.Instance.PlayCancelSound();
            CloseSettings();
        }
        else
        {
            if (PausePanel != null)
            {
                UIAudioManager.Instance.PlayCancelSound();
                SettingManager.Instance.isPaused = !SettingManager.Instance.isPaused;
                PausePanel.SetActive(SettingManager.Instance.isPaused);
            }
        }
    }

    public void ConfirmMainMenu() { ConfirmationUI.Instance.SetConfirmationUI("Return to Main Menu?", () => BackToMainMenu()); }

    public void BackToMainMenu()
    {
        DialogueSystem.Instance.StopDialogue();
        GameoverPanel.SetActive(false);
        demoEndPanel.SetActive(false);
        SceneManager.LoadScene("Main Menu");
    }
    public void ShowGameover(bool open)
    { 
        GameoverPanel.SetActive(open);
        SettingManager.Instance.gameOver = open;
    }

    public void ShowDemoEnd()
    {
        demoEndPanel.SetActive(true);
        SettingManager.Instance.gameOver = true;
    }

    public void VisitSteamPage()
    {
        Application.OpenURL($"https://store.steampowered.com/");
    }


    public void TryAgain()
    {
        var playerReset = FindAnyObjectByType<PlayerResetManager>();
        if(playerReset == null) return;
        UnityEngine.Debug.Log("Player Reset Triggered");
        playerReset.ResetPlayer($"Getting Hit by Monster Bat!");
        if(SettingManager.Instance.gameOver)
        {
            ShowGameover(false);
        }        
    }

    public void RestartChapter()
    {
        StartCoroutine(RestartChapterCO());
    }

    public IEnumerator RestartChapterCO()
    {
        ObjectiveManager.Instance.objectiveDatas.FindAll(x => x.Chapter == ObjectiveManager.Instance.currentChapter).ForEach(x => x.IsCompleted = false);
        yield return StartCoroutine(DialogueSystem.Instance.FadeToBlack(0));
        GameObject.Find("Player").GetComponent<PlayerController>().ToggleRig(true);

        loadingPanel.SetActive(true);

        SceneField currentChapterScene = ChapterDataManager.Instance.GetChapterScene(ChapterDataManager.Instance.currentChapterIndex);
        if (currentChapterScene != null)
        {
            List<SceneField> scenesToLoad = new List<SceneField> { currentChapterScene };
            List<SceneField> scenesToUnload = new List<SceneField> { currentChapterScene };
            AsyncSceneLoader.Instance.LoadScenes(scenesToLoad, scenesToUnload, AsyncSceneLoader.Instance.persistentScene, () =>
            {
                ObjectiveManager.Instance.UpdateCurrentObjectives();
                if (DialogueSystem.Instance.isRunningConvo)
                    DialogueSystem.Instance.StopDialogue();
                loadingPanel.SetActive(false);
                DialogueSystem.Instance.OpenDialogue($"Chapter{ChapterDataManager.Instance.currentChapterIndex + 1}");
            }, async progress =>
            {
                totalProgress = progress;
                UpdateLoadingSprite();
            });
        }
        else
        {
            if (DialogueSystem.Instance.isRunningConvo)
                DialogueSystem.Instance.StopDialogue();
            loadingPanel.SetActive(false);
            DialogueSystem.Instance.OpenDialogue($"Chapter{ChapterDataManager.Instance.currentChapterIndex + 1}");
        }
        if (SettingManager.Instance.isPaused)
        {
            PausePanelToggle();
        }
    }
    private void UpdateLoadingSprite()
    {
        if (loadingBar == null || loadingPercentages == null || loadingPercentages.Count == 0)
            return;

        foreach (var item in loadingPercentages.OrderBy(x => x.progresThreshold))
        {
            if (totalProgress >= item.progresThreshold)
            {
                if (item.loadingBarThreshold != null)
                    loadingBar.sprite = item.loadingBarThreshold;
            }
        }
    }
    #endregion

}
