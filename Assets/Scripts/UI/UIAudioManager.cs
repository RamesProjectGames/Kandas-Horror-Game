using FMODUnity;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIAudioManager : MonoBehaviour
{
    public static UIAudioManager Instance;
    public EventReference clickSound, hoverSound, cancelSound;
    public List<string> cancelButtons;

    private void Awake()
    {
        if (Instance == null)
        {
            transform.SetParent(null);
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if(Instance != this)
        {
            Destroy(gameObject);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        BindAllButtonsInScene();
    }
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        BindAllButtonsInScene();
    }

    /// <summary>
    /// Finds all UI buttons in the scene and registers the central audio playback method.
    /// </summary>
    public void BindAllButtonsInScene()
    {
        // Find every active and inactive button component in the current scene
        Button[] allButtons = Resources.FindObjectsOfTypeAll<Button>();

        foreach (Button button in allButtons)
        {
            // Filter out scene assets or prefabs that are not part of the active scene hierarchy
            if (button.gameObject.scene.name == null) continue;

            // Get or add the EventTrigger component
            EventTrigger trigger = button.GetComponent<EventTrigger>();
            if (trigger == null)
            {
                trigger = button.AddComponent<EventTrigger>();
            }

            // Clean up existing PointerEnter entries to avoid duplication
            trigger.triggers.RemoveAll(entry => entry.eventID == EventTriggerType.PointerEnter);

            // Create the PointerEnter (Hover) entry
            EventTrigger.Entry entry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerEnter
            };
            entry.callback.AddListener((data) => { PlayHoverSound(); });

            // Add entry to the EventTrigger
            trigger.triggers.Add(entry);

            if(!cancelButtons.Contains(button.gameObject.name.Trim()))
            {
                // Remove the listener first to prevent duplicating listeners if called multiple times
                button.onClick.RemoveListener(PlayClickSound);
                button.onClick.AddListener(PlayClickSound);
            }
            else
            {
                // Remove the listener first to prevent duplicating listeners if called multiple times
                button.onClick.RemoveListener(PlayCancelSound);
                button.onClick.AddListener(PlayCancelSound);
            }
        }
    }

    public void PlayClickSound()
    {
        AudioManager.Instance.PlayOneShot2D(clickSound, 1, 1);
    }

    public void PlayCancelSound()
    {
        AudioManager.Instance.PlayOneShot2D(cancelSound, 1, 1);
    }

    public void PlayHoverSound()
    {
        AudioManager.Instance.PlayOneShot2D(hoverSound, 1, 1);
    }
}
