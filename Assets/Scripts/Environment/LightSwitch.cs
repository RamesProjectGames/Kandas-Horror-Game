using UnityEngine;

/// <summary>
/// Manages a light switch state in the room
/// Can be toggled on/off to control mannequin behavior
/// </summary>
public class LightSwitch : MonoBehaviour
{
    [SerializeField] private Light roomLight;
    [SerializeField] private bool isLightOn = true;
    
    // Static reference so all mannequins can access the state
    private static LightSwitch instance;

    void Awake()
    {
        instance = this;
        
        if (roomLight == null)
        {
            roomLight = GetComponent<Light>();
            if (roomLight == null)
            {
                roomLight = FindAnyObjectByType<Light>();
            }
        }
        
        UpdateLightState();
    }

    /// <summary>
    /// Toggle the light on/off
    /// </summary>
    public void ToggleLight()
    {
        isLightOn = !isLightOn;
        UpdateLightState();
    }

    private void UpdateLightState()
    {
        if (roomLight != null)
        {
            roomLight.enabled = isLightOn;
        }
        Debug.Log($"Light switch: {(isLightOn ? "ON" : "OFF")}");
    }

    /// <summary>
    /// Check if the light is currently on
    /// </summary>
    public static bool IsLightOn()
    {
        return instance != null && instance.isLightOn;
    }

    /// <summary>
    /// Get the singleton instance
    /// </summary>
    public static LightSwitch GetInstance()
    {
        return instance;
    }
}
