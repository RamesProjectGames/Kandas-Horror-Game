using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class PinSystem : MonoBehaviour
{
    public int maxPins = 4;
    
    public GameObject SafePinPanel;
    public List<string> pinTexts = new List<string>();
    public List<string> correctPins = new List<string>();
    public List<TextMeshProUGUI> textOnPins = new List<TextMeshProUGUI>();
    public UnityEvent OnCorrectPins;

    void Start()
    {
        for (int i = 0; i < maxPins; i++)
        {
            pinTexts.Add("");
        }
        SetCorrectPins();
    }
    public void AddPin(string pinText)
    {
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
        for (int i = 0; i < pinTexts.Count; i++)
        {
            string pin = pinTexts[i];
            textOnPins[i].text = pin;
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
}
