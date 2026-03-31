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
    public TMP_Text questionText;
    public TMP_Text timerText;
    public List<QuizButton> choiceTexts = new List<QuizButton>();

    [Header("Quiz Events")]
    public UnityEvent onQuizCompleted;
    public UnityEvent onQuestionChanged;

    List<string> correctAnswers = new List<string>();
    List<string> wrongAnswers = new List<string>();
    int currentQuestionIndex = 0;
    float currentTimer = 0f;
    void Awake()
    {
        Instance = this;
    }
    void Update()
    {
        if (currentTimer > 0 && currentQuestionIndex < quizQuestions.Count - 1)
        {
            currentTimer -= Time.deltaTime;
            if(timerText != null) timerText.text = "Time: " + currentTimer.ToString("F0");
        }
        else
        {
            NextQuestion();
        }
    }
    public void OpenQuiz(bool open)
    {
        if (quizPanel != null)
            quizPanel.SetActive(open);
        SettingManager.Instance.isPaused = open;
        if(open)
            PopulateQuizUI();
    }
    void PopulateQuizUI()
    {
        currentQuestionIndex = 0;
        if (quizQuestions.Count == 0)
        {
            Debug.LogError("No quiz questions assigned!");
            return;
        }
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
        List<string> answers = currentQuestion.GetAnswers();
        for (int i = 0; i < answers.Count; i++)
        {
            if (i < currentQuestion.answers.Count())
            {
                choiceTexts[i].quizButton.image.color = Color.white;
                choiceTexts[i].quizAnswerText.text = answers[i];
                choiceTexts[i].quizButton.onClick.RemoveAllListeners();
                choiceTexts[i].quizButton.onClick.AddListener(() =>
                {
                    if(currentQuestion.correctAnswer == answers[i])
                    {
                        choiceTexts[i].quizButton.image.color = Color.green;
                        correctAnswers.Add(answers[i]);
                    }
                    else
                    {
                        choiceTexts[i].quizButton.image.color = Color.red;
                        wrongAnswers.Add(answers[i]);
                    }
                    StartCoroutine(WaitForQuizCompletion(1.5f));
                });
                choiceTexts[i].quizButton.gameObject.SetActive(true);
            }
            else
            {
                choiceTexts[i].quizButton.gameObject.SetActive(false);
            }
        }
    }

    void NextQuestion()
    {
        if(currentQuestionIndex >= quizQuestions.Count - 1)
        {
            Debug.Log("Quiz Completed!");
            Debug.Log("Correct Answers: " + correctAnswers.Count);
            Debug.Log("Wrong Answers: " + wrongAnswers.Count);
            onQuizCompleted?.Invoke();
            return;
        }
        currentQuestionIndex++;
        ChangeQuestion();
    }
    public IEnumerator WaitForQuizCompletion(float delay)
    {
        yield return new WaitForSeconds(delay);
        NextQuestion();
    }
}
