using FMOD;
using FMODUnity;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.PostProcessing;
public class SettingManager : MonoBehaviour
{
    public static SettingManager Instance { get; private set; }
    private InputAction pauseAction;
    private SettingsUI settingsUI;
    public bool isPaused = false;
    public bool gameOver;
    public bool isDemo;
    void Awake()
    {
        transform.SetParent(null);
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        // rebindActions = new List<RebindActionUI>(FindObjectsByType<RebindActionUI>(sortMode: FindObjectsSortMode.None));
        savePath = Application.persistentDataPath + "/settings.json";
        LoadSettings();
    }
    void Start()
    {
        settingsUI = FindAnyObjectByType<SettingsUI>();
        settingsUI.PausePanelToggle();
        var inputActions = GetComponent<PlayerInput>();
        if (inputActions != null)
        {            
            pauseAction = inputActions.actions.FindAction("Pause");
        }
    }
    void Update()
    {
        if (pauseAction != null && pauseAction.WasCompletedThisFrame() && !isRebinding)
        {
            isPaused = true;
            settingsUI.PausePanelToggle(isPaused);
        }
    }
    void OnDestroy()
    {
        Instance = null;
    }
    #region Graphics Settings Methods

    public SettingData settings = new SettingData();
    private string savePath;
    int[] rates = new int[] { 24, 30, 60, 120 };
    public float minimumGammaIntensity = 0f;
    public float maximumGammaIntensity = 1f;
    public Material DefaultMat;
    public Material VertexJitterMat;

    public void NextResolution() {
        int max = System.Enum.GetValues(typeof(SettingData.Resolution)).Length;
        int idx = ((int)settings.GameResolution + 1) % max;
        settings.GameResolution = (SettingData.Resolution)idx;
        ApplyGraphicsSettings();
        SaveSettings();
    }
    public void PrevResolution() {
        int max = System.Enum.GetValues(typeof(SettingData.Resolution)).Length;
        int idx = ((int)settings.GameResolution - 1 + max) % max;
        settings.GameResolution = (SettingData.Resolution)idx;
        ApplyGraphicsSettings();
        SaveSettings();
    }

    public void NextFrameRate() {
        int idx = System.Array.IndexOf(rates, settings.FrameRate);
        idx = (idx + 1) % rates.Length;
        settings.FrameRate = rates[idx];
        ApplyGraphicsSettings();
        SaveSettings();
    }
    public void PrevFrameRate() {
        int idx = System.Array.IndexOf(rates, settings.FrameRate);
        idx = (idx - 1 + rates.Length) % rates.Length;
        settings.FrameRate = rates[idx];
        ApplyGraphicsSettings();
        SaveSettings();
    }

    public void ToggleVSync() {
        settings.VSync = !settings.VSync;
        ApplyGraphicsSettings();
        SaveSettings();
    }
    public void ToggleDithering() {
        settings.Dithering = !settings.Dithering;
        ApplyGraphicsSettings();
        SaveSettings();
    }
    public void ToggleBloom() {
        settings.Bloom = !settings.Bloom;
        ApplyGraphicsSettings();
        SaveSettings();
    }
    public void ToggleGrain() {
        settings.Grain = !settings.Grain;
        ApplyGraphicsSettings();
        SaveSettings();
    }
    public void SetGammaValue(float val)
    {
        settings.gamma = Mathf.Clamp(val, minimumGammaIntensity, maximumGammaIntensity);
        ApplyGraphicsSettings();
        SaveSettings();
    }
    public void ToggleMotionBlur() {
        settings.MotionBlur = !settings.MotionBlur;
        ApplyGraphicsSettings();
        SaveSettings();
    }
    public void ToggleVertexJitter() {
        settings.VertexJitter = !settings.VertexJitter;
        ApplyGraphicsSettings();
        SaveSettings();
    }

    public void SaveSettings()
    {
        string json = JsonUtility.ToJson(settings, true);
        System.IO.File.WriteAllText(savePath, json);
    }

    public void LoadSettings()
    {
        if (System.IO.File.Exists(savePath))
        {
            string json = System.IO.File.ReadAllText(savePath);
            settings = JsonUtility.FromJson<SettingData>(json);
        }
        else
        {
            settings = new SettingData();
            AutoAdjustGraphicsSettings();
        }
        ApplyGraphicsSettings();

    }

    public void ApplyGraphicsSettings()
    {
        // Resolution
        switch (settings.GameResolution)
        {
            case SettingData.Resolution.R1920x1080:
                Screen.SetResolution(1920, 1080, Screen.fullScreen);
                break;
            case SettingData.Resolution.R1600x900:
                Screen.SetResolution(1600, 900, Screen.fullScreen);
                break;
            case SettingData.Resolution.R1280x720:
                Screen.SetResolution(1280, 720, Screen.fullScreen);
                break;
            case SettingData.Resolution.R1366x768:
                Screen.SetResolution(1366, 768, Screen.fullScreen);
                break;
            case SettingData.Resolution.R1920x1200:
                Screen.SetResolution(1920, 1200, Screen.fullScreen);
                break;
            case SettingData.Resolution.R1680x1050:
                Screen.SetResolution(1680, 1050, Screen.fullScreen);
                break;
            case SettingData.Resolution.R1440x900:
                Screen.SetResolution(1440, 900, Screen.fullScreen);
                break;
            case SettingData.Resolution.R1280x800:
                Screen.SetResolution(1280, 800, Screen.fullScreen);
                break;
            case SettingData.Resolution.R1024x768:
                Screen.SetResolution(1024, 768, Screen.fullScreen);
                break;
            case SettingData.Resolution.R800x600:
                Screen.SetResolution(800, 600, Screen.fullScreen);
                break;
        }

        // Frame Rate
        Application.targetFrameRate = settings.FrameRate;

        // V-Sync
        QualitySettings.vSyncCount = settings.VSync ? 1 : 0;

        var postProcessVolumes = FindObjectsByType<PostProcessVolume>(FindObjectsSortMode.None);
        
        foreach (PostProcessVolume volume in postProcessVolumes)
        {
            UnityEngine.Debug.Log($"Modifying Volume {volume.gameObject.name}");
            if (volume == null || volume.profile == null)
            {
                UnityEngine.Debug.Log($"Invalid Volume {volume.gameObject.name}");
                continue;
            }

            if (volume.profile.TryGetSettings<ColorGrading>(out var colorGrading))
            {
                Vector4 tempGamma = colorGrading.gamma;
                tempGamma.w = settings.gamma;
                colorGrading.gamma.Override(tempGamma);
            }
            if (volume.profile.TryGetSettings<RetroPostProcessEffect>(out var retro))
            {
                retro.active = true;
                retro.DitherThreshold.value = settings.Dithering ? 0.5f : 0f;
            }
            if (volume.profile.TryGetSettings<Bloom>(out var bloom))
            {
                bloom.intensity.value = settings.Bloom ? 5f : 0f;
            }
            if (volume.profile.TryGetSettings<Grain>(out var grain))
            {
                grain.intensity.value = settings.Grain ? .5f : 0f;
            }
            if (volume.profile.TryGetSettings<MotionBlur>(out var motBlur))
            {
                motBlur.sampleCount.value = settings.MotionBlur ? 10 : 0;
            }


            MeshRenderer[] all = FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None);

            foreach (var mr in all)
            {
                if (mr.CompareTag("Environment") || mr.CompareTag("Enemy"))
                {
                    mr.material = settings.VertexJitter ? VertexJitterMat : DefaultMat;
                }
            }
        }
    }

    public void ChangeResolution(int index)
    {
        SettingData.Resolution resolution =(int)settings.GameResolution + (SettingData.Resolution)index;
        settings.GameResolution = resolution;
        ApplyGraphicsSettings();
    }
    public void ResetGrapichsToDefaults()
    {
        settings = new SettingData();
        AutoAdjustGraphicsSettings();
        ApplyGraphicsSettings();
        SaveSettings();
    }
    public void AutoAdjustGraphicsSettings()
    {
        // Example: Use SystemInfo to set reasonable defaults
        int ram = SystemInfo.systemMemorySize; // in MB
        int vram = SystemInfo.graphicsMemorySize; // in MB
        string gpu = SystemInfo.graphicsDeviceName.ToLower();
        int cpuCores = SystemInfo.processorCount;
        int width = Display.main.systemWidth;
        int height = Display.main.systemHeight;

        // Resolution: pick closest match to native
        int bestDist = int.MaxValue;
        SettingData.Resolution bestRes = SettingData.Resolution.R1920x1080;
        int[,] resList = new int[,] {
            {1920,1080}, {1600,900}, {1280,720}, {1366,768}, {1920,1200}, {1680,1050}, {1440,900}, {1280,800}, {1024,768}, {800,600}, {640,480}
        };
        for (int i = 0; i < resList.GetLength(0); i++) {
            int dw = width - resList[i,0];
            int dh = height - resList[i,1];
            int dist = dw*dw + dh*dh;
            if (dist < bestDist) {
                bestDist = dist;
                bestRes = (SettingData.Resolution)i;
            }
        }
        settings.GameResolution = bestRes;

        // Frame Rate: lower for low-end, higher for high-end
        if (cpuCores <= 2 || ram < 4000) settings.FrameRate = 30;
        else if (cpuCores <= 4 || ram < 8000) settings.FrameRate = 60;
        else settings.FrameRate = 120;

        // VSync: on for most
        settings.VSync = true;

        // Effects: enable/disable based on RAM/VRAM
        settings.Dithering = true;
        settings.Bloom = vram >= 2000;
        settings.Grain = true;
        settings.gamma = minimumGammaIntensity;
        settings.MotionBlur = vram >= 2000;
        settings.VertexJitter = true;

        ApplyGraphicsSettings();
        SaveSettings();
    }
    public void OnApplicationQuit()
    {
        SaveSettings();
    }
    #endregion

    #region Control Settings Methods

    public float minimumMicrophoneVolume = .25f;
    public float maximumMicrophoneVolume = 1f;
    public float minimumMouseSensitivity = 0.1f;
    public float maximumMouseSensitivity = 1f;
    bool isRebinding = false;
    public void SetRebind(bool isRebinding) => this.isRebinding = isRebinding;
    
    public void SelectAudioInputDevice(int index)
    {
        string micName = GetMicName(index);
        UnityEngine.Debug.Log($"Mic Name : {micName}");
        if (!string.IsNullOrEmpty(micName))
        {
            settings.AudioInputDeviceName = micName;
        }
        else
        {
            settings.AudioInputDeviceName = "";
        }
        SaveSettings();

        MicrophoneManager micManager = MicrophoneManager.Instance;
        if (micManager != null)
        {
            micManager.RestartRecording();
        }
    }
    public string GetMicName(int index)
    {
        var core = RuntimeManager.CoreSystem;

        string name;
        Guid guid;
        int rate;
        SPEAKERMODE mode;
        int channels;
        DRIVER_STATE state;

        core.getRecordDriverInfo(
            index,
            out name,
            256,
            out guid,
            out rate,
            out mode,
            out channels,
            out state
        );

        return name;
    }
    public void SelectAudioOutputDevice(int index)
    {
        var coreSystem = RuntimeManager.CoreSystem;
        settings.AudioOutputDeviceIndex = index;

        coreSystem.setDriver(settings.AudioOutputDeviceIndex);
        SaveSettings();
    }
    public void SetMicrophoneSensitivity(float sensitivity)
    {
        settings.MicrophoneSensitivity = Mathf.Lerp(minimumMicrophoneVolume, maximumMicrophoneVolume, sensitivity);
        SaveSettings();
    }
    public void SetMouseSensitivity(float sensitivity)
    {
        settings.MouseSensitivity = Mathf.Lerp(minimumMouseSensitivity, maximumMouseSensitivity, sensitivity);
        SaveSettings();
    }
    public void ToggleSprintToggle() {
        settings.SprintToggle = !settings.SprintToggle;
        SaveSettings();
    }
    public void ToggleCrouch()
    {
        settings.CrouchToggle = !settings.CrouchToggle;
        SaveSettings();    
    }
    public void ResetControlsToDefaults()
    {
        settings.AudioInputDeviceName = "";
        settings.AudioOutputDeviceIndex = 0;
        settings.MicrophoneSensitivity = 100f;
        settings.MouseSensitivity = 1.0f;
        settings.SprintToggle = false;
        settings.CrouchToggle = false;
        SaveSettings();
    }
    #endregion 

    #region Audio Settings Methods

    public void SetMasterVolume(float volume)
    {
        settings.MasterVolume = volume;
        AudioManager.Instance.UpdateVolumeSettings();
        SaveSettings();
    }
    public void SetMusicVolume(float volume)
    {
        settings.MusicVolume = volume;
        SaveSettings();
    }
    public void SetSoundEffectVolume(float volume)
    {
        settings.SoundEffectVolume = volume;
        SaveSettings();
    }
    public void SetMobVolume(float volume)
    {
        settings.MobVolume = volume;
        SaveSettings();
    }
    public void ResetAudioToDefaults()
    {
        settings.MasterVolume = 1.0f;
        settings.MusicVolume = 1.0f;
        settings.SoundEffectVolume = 1.0f;
        settings.MobVolume = 1.0f;
        SaveSettings();
    }
    #endregion

    #region Language Settings Methods
    public void SetLanguage(int index)
    {
        settings.GameLanguage = (SettingData.Language)index;
        SaveSettings();
    }
    public void ResetLanguageToDefault()
    {
        settings.GameLanguage = SettingData.Language.English;
        SaveSettings();
    }
    #endregion
}
