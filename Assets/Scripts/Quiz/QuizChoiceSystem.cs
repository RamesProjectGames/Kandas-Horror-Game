using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
[System.Serializable]
public class QuizButton
{
    public TMP_Text quizAnswerText;
    public Button quizButton;
}
public class QuizChoiceSystem : MonoBehaviour
{
    public static QuizChoiceSystem Instance { get; private set; }
    [Header("Quiz Data")]
    public List<QuizChoiceData> quizQuestions = new List<QuizChoiceData>();
    public float quizTimer = 10f;
    [Header("UI Elements")]
    public GameObject quizPanel;
    public GameObject quizUIBlocker;
    public TMP_Text questionText;
    public TMP_Text timerText;
    public List<QuizButton> choiceTexts = new List<QuizButton>();

    [Header("Quiz Events")]
    public UnityEvent onQuizCompleted;
    public UnityEvent onQuestionChanged;

    [SerializeField]List<string> answers = new List<string>();
    List<string> correctAnswers = new List<string>();
    List<string> wrongAnswers = new List<string>();
    [SerializeField] int currentQuestionIndex = 0;
    float currentTimer = 0f;
    bool isQuizActive = false;
    void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        isQuizActive = false;
    }
    void Update()
    {
        if(isQuizActive)
        {
            if (currentTimer > 0 )
            {
                currentTimer -= Time.deltaTime;
                if (timerText != null) timerText.text = "Time: " + currentTimer.ToString("F0");
            }
            else
            {
                SkipQuestion();
            }
        }
    }
    public void OpenQuiz(bool open)
    {
        if (quizPanel != null)
            quizPanel.SetActive(open);
        isQuizActive = open;
        SettingManager.Instance.isPaused = open;
        if(open)
            PopulateQuizUI();
    }
    void PopulateQuizUI()
    {
        if (quizQuestions.Count == 0)
        {
            Debug.LogError("No quiz questions assigned!");
            return;
        }

        quizQuestions = quizQuestions.OrderBy(x => Random.value).ToList();
        currentQuestionIndex = 0;
        currentTimer = quizTimer;
        isQuizActive = true;

        correctAnswers.Clear();
        wrongAnswers.Clear();

        ChangeQuestion();
    }

    private void ChangeQuestion()
    {
        if(currentQuestionIndex != 0)
        {
            onQuestionChanged?.Invoke();
        }
        currentTimer = quizTimer;
        QuizChoiceData currentQuestion = quizQuestions[currentQuestionIndex];
        questionText.text = currentQuestion.questionText;
        answers = currentQuestion.GetAnswers();
        choiceTexts = choiceTexts.OrderBy(x => Random.value).ToList();
        for (int i = 0; i < answers.Count; i++)
        {
            if (i < answers.Count && i < choiceTexts.Count)
            {
                QuizButton quizButton = choiceTexts[i];
                quizButton.quizButton.image.color = Color.white;
                string answer = answers[i]; // Capture the current answer in a local variable for the lambda
                bool isCorrect = currentQuestion.IsCorrect(answer);
                quizButton.quizAnswerText.text = answers[i];
                quizButton.quizButton.onClick.RemoveAllListeners();
                quizButton.quizButton.onClick.AddListener(() =>
                {
                    quizUIBlocker.SetActive(true);
                    if (isCorrect)
                    {    
                        quizButton.quizButton.image.color = Color.green;
                        correctAnswers.Add(currentQuestion.questionText);
                    }
                    else
                    {    
                        quizButton.quizButton.image.color = Color.red;
                        wrongAnswers.Add(currentQuestion.questionText);
                    }
                    StartCoroutine(WaitForQuizCompletion(0.5f));
                });
                choiceTexts[i].quizButton.gameObject.SetActive(true);
            }
            else
            {
                choiceTexts[i].quizButton.gameObject.SetActive(false);
            }
        }
        quizUIBlocker.SetActive(false);
    }

    void NextQuestion()
    {
        currentQuestionIndex++;
        if(currentQuestionIndex > quizQuestions.Count - 1)
        {
            onQuizCompleted?.Invoke();
            return;
        }
        ChangeQuestion();
    }
    void SkipQuestion()
    {
        quizUIBlocker.SetActive(true);
        currentQuestionIndex++;
        QuizChoiceData currentQuestion = quizQuestions[currentQuestionIndex];
        wrongAnswers.Add(currentQuestion.questionText);
        if (currentQuestionIndex > quizQuestions.Count -1 )
        {
            onQuizCompleted?.Invoke();
            return;
        }
        ChangeQuestion();
    }
    public IEnumerator WaitForQuizCompletion(float delay)
    {
        yield return new WaitForSeconds(delay);
        NextQuestion();
    }
}
