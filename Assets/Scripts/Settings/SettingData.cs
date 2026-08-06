using UnityEngine;
[System.Serializable]
public class SettingData 
{
	// Graphics Settings
	public enum Resolution {
		R1920x1080,
		R1600x900,
		R1280x720,
		R1366x768,
		R1920x1200,
		R1680x1050,
		R1440x900,
		R1280x800,
		R1024x768,
		R800x600
	}
	public Resolution GameResolution = Resolution.R1920x1080;

	public int FrameRate = 60; // e.g., 30, 60, 120
	public bool VSync = false;

	public bool Dithering = false;
	public bool Bloom = false;
	public bool Grain = true;
	public float Fog = .01f;
	public bool MotionBlur = false;
	public bool VertexJitter = true;

	// Control Settings
	public string AudioInputDeviceName = ""; // Name of the selected input device
	public int AudioOutputDeviceIndex = 0; // Index in available outputs (if supported)
    [Range(100, 500)]
    public float MicrophoneSensitivity = 500f;
    [Range(0.1f, 100)]
    public float MouseSensitivity = 25.0f;
	public bool SprintToggle = false; // false = hold to sprint, true = toggle
	public bool CrouchToggle = false;
    [Range(-1, 1)]
    public float gamma = 0f;

    // Audio Settings
    [Range(0, 1)]
    public float MasterVolume = 1.0f;
    [Range(0, 1)]
    public float MusicVolume = 1.0f;
    [Range(0, 1)]
    public float SoundEffectVolume = 1.0f;
    [Range(0, 1)]
    public float MobVolume = 1.0f;

	//Language Settings
	public enum Language { English, Indonesia }
	public Language GameLanguage = Language.English;
}
