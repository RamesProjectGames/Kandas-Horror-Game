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
		R800x600,
		R640x480
	}
	public Resolution GameResolution = Resolution.R1920x1080;

	public int FrameRate = 60; // e.g., 30, 60, 120
	public bool VSync = false;

	public bool Dithering = true;
	public bool Bloom = false;
	public bool Grain = true;
	public bool Fog = true;
	public bool MotionBlur = false;
	public bool VertexJitter = true;

	public enum TextureQualityLevel { Low = 240, Medium = 360, High = 480 }
	public TextureQualityLevel TextureQuality = TextureQualityLevel.Low;

	// Control Settings
	public string AudioInputDeviceName = ""; // Name of the selected input device
	public int AudioOutputDeviceIndex = 0; // Index in available outputs (if supported)
	public float MicrophoneSensitivity = 100f; // 1.0 = default
	public float MouseSensitivity = .5f; // 1.0 = default
	public bool SprintToggle = false; // false = hold to sprint, true = toggle

	// Audio Settings
	public float MusicVolume = 1.0f;
	public float SoundEffectVolume = 1.0f;
	public float MobVolume = 1.0f;

	//Language Settings
	public enum Language { English, Indonesia }
	public Language GameLanguage = Language.English;
}
