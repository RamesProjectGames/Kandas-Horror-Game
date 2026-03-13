using Dialogue;
using UnityEngine;
using UnityEngine.UI;

public class MicrophoneDetectionUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MicrophoneManager micManager;
    [SerializeField] private EnemySoundDetection enemyDetection;
    [SerializeField] private PlayerHiding playerHiding;

    [Header("UI Components")]
    [SerializeField] private Slider loudnessSlider;
    [SerializeField] private Image fillImage;
    [SerializeField] private RectTransform thresholdLine;

    [Header("Stage Colors")]
    [SerializeField] private Color safeColor = Color.green;    // Low volume
    [SerializeField] private Color warningColor = Color.yellow; // Close to threshold
    [SerializeField] private Color dangerColor = Color.red;     // Over threshold

    [Header("Threshold Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float warningBuffer = 0.2f; // Trigger warning at 80% of threshold

    void Awake()
    {
        if(micManager == null) micManager = FindAnyObjectByType<MicrophoneManager>();
        if(playerHiding == null) playerHiding = FindAnyObjectByType<PlayerHiding>();
        if(enemyDetection == null) enemyDetection = FindAnyObjectByType<EnemySoundDetection>();
    }

    private void Update()
    {
        if (Application.isPlaying && (SettingManager.Instance.isPaused || DialogueSystem.Instance.isRunningConvo))
            return;
        if (playerHiding == null || !playerHiding.IsHiding())
        {
            // Optionally hide the UI when not hiding
            loudnessSlider.gameObject.SetActive(false);
            return;
        }

        float currentLoudness = micManager.GetMicrophoneLoudness();
        float threshold = enemyDetection.GetCurrentRequiredThreshold();

        // 1. Update Slider and Threshold Line Position
        loudnessSlider.value = currentLoudness;
        UpdateVerticalThreshold(threshold);

        // 2. Determine the 3-Stage State
        UpdateVisualState(currentLoudness, threshold);
    }

    private void UpdateVisualState(float loudness, float threshold)
    {
        loudnessSlider.gameObject.SetActive(true);
        if (loudness >= threshold)
        {
            // STAGE 3: DANGER (Detection Triggered)
            fillImage.color = dangerColor;
            // You could trigger a screen shake here
        }
        else if (loudness >= (threshold - warningBuffer))
        {
            // STAGE 2: WARNING (Almost detected)
            fillImage.color = warningColor;
        }
        else
        {
            // STAGE 1: SAFE
            fillImage.color = safeColor;
        }
    }

    private void UpdateVerticalThreshold(float threshold)
    {
        float sliderHeight = loudnessSlider.GetComponent<RectTransform>().rect.height;
        // Calculate Y position from bottom (-height/2) to top (height/2)
        float newY = (threshold * sliderHeight) - (sliderHeight / 2f);
        thresholdLine.anchoredPosition = new Vector2(thresholdLine.anchoredPosition.x, newY);
    }
}