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
        SettingManager.Instance.isPaused = false;
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
            if (button.gameObject.scene.name == null) continue;

            //Add Hover Sound
            if(button.GetComponentInParent<Canvas>() != null && button.GetComponentInParent<Canvas>().renderMode != RenderMode.WorldSpace)
            {
                EventTrigger trigger = button.GetComponent<EventTrigger>();
                if (trigger == null)
                {
                    trigger = button.AddComponent<EventTrigger>();
                }

                trigger.triggers.RemoveAll(entry => entry.eventID == EventTriggerType.PointerEnter);

                EventTrigger.Entry entry = new EventTrigger.Entry
                {
                    eventID = EventTriggerType.PointerEnter
                };
                entry.callback.AddListener((data) => { PlayHoverSound(); });

                trigger.triggers.Add(entry);

                //Add click for normal buttons and cancel for exit buttons
                if (!cancelButtons.Contains(button.gameObject.name.Trim()))
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
            //Add Click Sound for World Canvas instead
            else
            {
                EventTrigger trigger = button.GetComponent<EventTrigger>();
                if (trigger == null)
                {
                    trigger = button.AddComponent<EventTrigger>();
                }

                trigger.triggers.RemoveAll(entry => entry.eventID == EventTriggerType.PointerClick);

                EventTrigger.Entry entry = new EventTrigger.Entry
                {
                    eventID = EventTriggerType.PointerClick
                };
                entry.callback.AddListener((data) => { PlayClickSound(); });

                trigger.triggers.Add(entry);
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
