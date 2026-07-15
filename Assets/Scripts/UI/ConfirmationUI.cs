using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ConfirmationUI : MonoBehaviour
{
    public static ConfirmationUI Instance;
    public TMP_Text TitleText;
    public Button confirmButton;
    void Awake()
    {
        if (Instance != null)
        {
            Destroy(Instance.gameObject);
            Instance = null;
        }
        Instance = this;
    }
    void Start()
    {
        Cancel();
    }
    public void SetConfirmationUI(string text, Action confirmed)
    {
        TitleText.text = text;
        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(() =>
        {
            confirmed.Invoke();
            Cancel();
        });
        IsShowed(true);
    }
    public void Cancel()
    {
        confirmButton.onClick.RemoveAllListeners();
        IsShowed(false);
    }
    public void IsShowed(bool show)
    {
        transform.LeanScale(show ? Vector3.one : Vector3.zero, 0.2f);
    }
}
