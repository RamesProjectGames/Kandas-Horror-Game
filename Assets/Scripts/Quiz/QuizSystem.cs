using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class QuizSystem : MonoBehaviour
{
    [Header("Quiz Panel")]
    public GameObject quizPanel;
    [Header("Draw Area")]
    public RectTransform drawArea;           // Your 1600 x 750 panel
    public RectTransform dotsContainer;      // Usually same as drawArea
    public Image dotPrefab;                  // UI Image prefab (round soft brush)

    [Header("Brush Settings")]
    public float dotSize = 40f;              // Good for your 50x50 style
    public float dotSpacing = 8f;            // Smooth drawing
    public float totalLengthRequired = 300f; // How much they must draw to count as correct

    [Header("Questions")]
    public List<string> questions = new List<string>();
    public TMP_Text questionText;
    public TMP_Text progressText;
    private int currentQuestion = 0;

    [Header("Result UI")]
    public GameObject resultPanel;
    public TMP_Text resultText;

    [Header("Optional Feedback")]
    public TMP_Text feedbackText;
    public float nextQuestionDelay = 0.5f;
    [Header("Optional Events")]
    public UnityEvent onDrawComplete;
    public UnityEvent onFinalQuestion;

    private bool isDrawing = false;
    private bool isTransitioning = false;
    private bool hasTriggeredCompletion = false;

    private Vector2 lastLocalPoint;
    private float accumulatedLength = 0f;

    private List<RectTransform> spawnedDots = new List<RectTransform>();
    private List<bool> questionResults = new List<bool>();

    public bool HasValidDrawing => accumulatedLength >= totalLengthRequired;

    void Start()
    {
        if (resultPanel != null)
            resultPanel.SetActive(false);

        if (feedbackText != null)
            feedbackText.gameObject.SetActive(false);

        questionResults.Clear();
        for (int i = 0; i < questions.Count; i++)
        {
            questionResults.Add(false);
        }

        currentQuestion = 0;
    }

    void Update()
    {
        if (isTransitioning) return;

        // Mouse
        if (Input.GetMouseButtonDown(0))
        {
            // Prevent drawing if clicking UI outside draw area? Optional
            // If you have buttons, this can help:
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                // Still allow drawing IF inside draw area
                TryBeginDraw(Input.mousePosition);
            }
            else
            {
                TryBeginDraw(Input.mousePosition);
            }
        }

        if (Input.GetMouseButton(0) && isDrawing)
        {
            ContinueDraw(Input.mousePosition);
        }

        if (Input.GetMouseButtonUp(0))
        {
            EndDraw();
        }

        // Touch support (optional)
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    TryBeginDraw(touch.position);
                    break;

                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    if (isDrawing)
                        ContinueDraw(touch.position);
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    EndDraw();
                    break;
            }
        }
    }

    // =========================
    // Question Flow
    // =========================
    public void OpenQuiz(bool open)
    {
        if (quizPanel != null)
            quizPanel.SetActive(open);
        SettingManager.Instance.isPaused = open;
        ShowQuestion();
    }
    void ShowQuestion()
    {
        if (currentQuestion >= questions.Count)
        {
            FinishQuiz();
            return;
        }

        if (questionText != null)
            questionText.text = questions[currentQuestion];

        if (progressText != null)
            progressText.text = $"Question {currentQuestion + 1}/{questions.Count}";

        ClearDrawing();

        if (feedbackText != null)
            feedbackText.gameObject.SetActive(false);

        
    }

    IEnumerator NextQuestionRoutine()
    {
        if (isTransitioning) yield break;
        isTransitioning = true;

        // Mark current as correct because enough drawing was completed
        if (currentQuestion < questionResults.Count)
            questionResults[currentQuestion] = true;

        if (feedbackText != null)
        {
            feedbackText.gameObject.SetActive(true);
            feedbackText.text = "Correct!";
        }

        yield return new WaitForSeconds(nextQuestionDelay);

        currentQuestion++;

        if (currentQuestion >= questions.Count)
        {
            FinishQuiz();
        }
        else
        {
            ShowQuestion();
        }

        isTransitioning = false;
    }

    void FinishQuiz()
    {
        isDrawing = false;

        int correctCount = 0;
        for (int i = 0; i < questionResults.Count; i++)
        {
            if (questionResults[i])
                correctCount++;
        }

        if (questionText != null)
            questionText.gameObject.SetActive(false);

        if (progressText != null)
            progressText.gameObject.SetActive(false);

        if (feedbackText != null)
            feedbackText.gameObject.SetActive(false);

        if (resultPanel != null)
            resultPanel.SetActive(true);

        if (resultText != null)
            resultText.text = $"Quiz Finished!\nCorrect: {correctCount}/{questions.Count}";

        ClearDrawing();

        onFinalQuestion?.Invoke();
    }

    public void RestartQuiz()
    {
        currentQuestion = 0;

        if (questionText != null)
            questionText.gameObject.SetActive(true);

        if (progressText != null)
            progressText.gameObject.SetActive(true);

        if (resultPanel != null)
            resultPanel.SetActive(false);

        if (feedbackText != null)
            feedbackText.gameObject.SetActive(false);

        questionResults.Clear();
        for (int i = 0; i < questions.Count; i++)
        {
            questionResults.Add(false);
        }

        ShowQuestion();
    }

    // =========================
    // Drawing Logic
    // =========================

    void TryBeginDraw(Vector2 screenPos)
    {
        if (isTransitioning) return;

        if (!ScreenPointToLocalPointInDrawArea(screenPos, out Vector2 localPoint))
            return;

        isDrawing = true;
        hasTriggeredCompletion = false;
        lastLocalPoint = localPoint;

        SpawnDot(localPoint);
    }

    void ContinueDraw(Vector2 screenPos)
    {
        if (!ScreenPointToLocalPointInDrawArea(screenPos, out Vector2 localPoint))
        {
            // EndDraw();
            return;
        }

        float distance = Vector2.Distance(lastLocalPoint, localPoint);

        if (distance <= 0.01f)
            return;

        int steps = Mathf.CeilToInt(distance / dotSpacing);

        for (int i = 1; i <= steps; i++)
        {
            float t = (float)i / steps;
            Vector2 point = Vector2.Lerp(lastLocalPoint, localPoint, t);
            SpawnDot(point);
        }

        accumulatedLength += distance;
        lastLocalPoint = localPoint;

        
    }

    void EndDraw()
    {
        if (!isDrawing) return;

        isDrawing = false;

        // Player finished drawing and released mouse
        if (accumulatedLength >= totalLengthRequired)
        {
            onDrawComplete?.Invoke();
            StartCoroutine(NextQuestionRoutine());
        }
        else
        {
            // Optional feedback if not enough drawing
            if (feedbackText != null)
            {
                feedbackText.gameObject.SetActive(true);
                feedbackText.text = "Draw more!";
            }
        }
    }

    void SpawnDot(Vector2 localPoint)
    {
        if (dotPrefab == null || dotsContainer == null)
            return;

        Image dot = Instantiate(dotPrefab, dotsContainer);
        RectTransform rt = dot.rectTransform;

        rt.anchoredPosition = localPoint;
        rt.sizeDelta = new Vector2(dotSize, dotSize);

        spawnedDots.Add(rt);
    }

    public void ClearDrawing()
    {
        for (int i = 0; i < spawnedDots.Count; i++)
        {
            if (spawnedDots[i] != null)
                Destroy(spawnedDots[i].gameObject);
        }

        spawnedDots.Clear();
        accumulatedLength = 0f;
        hasTriggeredCompletion = false;
        isDrawing = false;
    }

    // =========================
    // Utility
    // =========================

    bool ScreenPointToLocalPointInDrawArea(Vector2 screenPos, out Vector2 localPoint)
    {
        Camera cam = null;

        Canvas canvas = drawArea.GetComponentInParent<Canvas>();
        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            cam = canvas.worldCamera;
        }

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(drawArea, screenPos, cam, out localPoint))
            return false;

        return drawArea.rect.Contains(localPoint);
    }
}
