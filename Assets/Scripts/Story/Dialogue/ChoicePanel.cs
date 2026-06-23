using Dialogue;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChoicePanel : MonoBehaviour
{
    public static ChoicePanel Instance { get; private set; }
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI title;

    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private VerticalLayoutGroup buttonLayoutGroup;

    public ChoicePanelDecision lastChoiceData { get; private set; } = null;
    private List<ChoiceButton> choiceList = new List<ChoiceButton>();

    public bool isWaitingChoice { get; private set; }

    [Header("Canvas Group Controller")]
    private const float fadeSpeed = 5f;

    private Coroutine showingCo = null;
    private Coroutine hidingCo = null;

    public bool active = false;
    public bool isShowing => showingCo != null;
    public bool isHiding => hidingCo != null;
    public bool isFading => isShowing || isHiding;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        canvasGroup.alpha = 0;
        SetPanelState(false);
    }

    public void AcceptAnswer(int index)
    {
        if (index < 0 || index >= lastChoiceData.choices.Length)
            return;
        lastChoiceData.answerIdx = index;
        isWaitingChoice = false;
        HideChoices();
    }

    #region CanvasGroupController
    public void ShowChoices(string[] choices, string question = "")
    {
        if (isShowing)
            return;
        else if (isHiding)
        {
            StopCoroutine(hidingCo);
            hidingCo = null;
        }
        lastChoiceData = new ChoicePanelDecision(question, choices);
        title.text = question;
        GenerateChoices(choices);

        showingCo = StartCoroutine(Fading(1));
        active = true;
        isWaitingChoice = true;
    }

    private void GenerateChoices(string[] choices)
    {
        float maxWidth = 0;

        for (int i = 0; i < choices.Length; i++)
        {
            ChoiceButton choiceButton;
            if(i < choiceList.Count)
            {
                choiceButton = choiceList[i];
            }
            else
            {
                GameObject newChoiceButton = Instantiate(buttonPrefab, buttonLayoutGroup.transform);
                newChoiceButton.SetActive(true);
            }
        }
    }

    public void HideChoices()
    {
        if (isHiding)
            return;
        else if (isShowing)
        {
            StopCoroutine(showingCo);
            showingCo = null;
        }

        active = false;
        isWaitingChoice = false;
        hidingCo = StartCoroutine(Fading(0));
    }

    private IEnumerator Fading(float targetOpac)
    {
        CanvasGroup cg = canvasGroup;
        while (cg.alpha != targetOpac)
        {
            cg.alpha = Mathf.MoveTowards(cg.alpha, targetOpac, Time.deltaTime * fadeSpeed);
            yield return null;
        }

        showingCo = null;
        hidingCo = null;
    }

    public void SetPanelState(bool active)
    {
        canvasGroup.interactable = active;
        canvasGroup.blocksRaycasts = active;
    }
#endregion

    public class ChoicePanelDecision
    {
        public string question = string.Empty;
        public int answerIdx = -1;
        public string[] choices = new string[0];

        public ChoicePanelDecision(string question, string[] choices)
        {
            this.question = question;
            this.choices = choices;
        }
    }

    private struct ChoiceButton
    {
        public Button button;
        public TextMeshProUGUI title;
        public LayoutElement layout;
    }
}
