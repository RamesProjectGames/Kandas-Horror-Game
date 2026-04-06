using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class PinSystem : MonoBehaviour
{
    public int maxPins = 4;
    
    public GameObject SafePinPanel;
    public List<string> pinTexts = new List<string>();
    public List<string> correctPins = new List<string>();
    public TMP_Text textOnPins;
    public UnityEvent OnCorrectPins;
    public UnityEvent OnIncorrectPins;

    [Header("Audio")]
    [SerializeField] private EventReference inputPinSound;
    private EventInstance inputPinSoundEvent;

    void Start()
    {
        inputPinSoundEvent = AudioManager.Instance.CreateInstance(inputPinSound);
        RuntimeManager.AttachInstanceToGameObject(inputPinSoundEvent, gameObject, false);
        for (int i = 0; i < maxPins; i++)
        {
            pinTexts.Add("");
        }
        SetCorrectPins();
    }
    public void AddPin(string pinText)
    {
        PlayInputPinSound();
        for (int i = 0; i < maxPins; i++)
        {
            if(string.IsNullOrEmpty(pinTexts[i]))
            {
                pinTexts[i] = pinText;
                break;
            }
        }
        UpdatePinsUI();
        CheckPins();
    }
    public void ClearPins()
    {
        for (int i = 0; i < maxPins; i++)
        {
            pinTexts[i] = "";
        }
        UpdatePinsUI();
    }
    public void UpdatePinsUI()
    {
       textOnPins.text = "";
        for (int i = 0; i < maxPins; i++)
        {
            textOnPins.text += string.IsNullOrEmpty(pinTexts[i]) ? "" : pinTexts[i];
            if (i < maxPins - 1)
                textOnPins.text += "";
        }
    }
    public void SetCorrectPins()
    {
        for (int i = 0; i < maxPins; i++)
        {
            correctPins[i] = Random.Range(0, 9).ToString();
        }
    }
    public void CheckPins()
    {
        if(pinTexts.Count == maxPins)
        {
            for (int i = 0; i < maxPins; i++)
            {
                if (pinTexts[i] != correctPins[i])
                {
                    OnIncorrectPins?.Invoke();

                    EnemyMovement[] enemies = FindObjectsByType<EnemyMovement>(FindObjectsSortMode.None);
                    for (int j = 0; j < enemies.Length; j++)
                    {
                        if (enemies[j] != null)
                            enemies[j].OnEnterAudioRadius(gameObject);
                    }
                    
                    ClosePanel();
                    return;
                }
            }
            OnCorrectPins?.Invoke();
        }
    }
    public void OpenPanel()
    {
        SafePinPanel.SetActive(true);
    }
    public void ClosePanel()
    {
        SafePinPanel.SetActive(false);
        ClearPins();
    }
    public void PlayInputPinSound()
    {
        PLAYBACK_STATE playbackState;
        inputPinSoundEvent.getPlaybackState(out playbackState);
        if (playbackState != PLAYBACK_STATE.PLAYING)
        {
            RuntimeManager.AttachInstanceToGameObject(inputPinSoundEvent, gameObject, false);
            inputPinSoundEvent.start();
        }
    }
}
