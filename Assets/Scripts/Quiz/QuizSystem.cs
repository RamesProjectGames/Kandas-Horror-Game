using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class QuizSystem : MonoBehaviour
{
    [Header("Questions")]
    public List<string> questions = new List<string>();
    public TMP_Text questionText;
    private int currentQuestion = 0;
    private List<bool> questionResults = new List<bool>();
    public GameObject QuizPanel;

    [Header("Drawing Area")]
    public RectTransform drawArea;
    public RectTransform strokeContainer;
    public Camera uiCamera; // null if Screen Space Overlay

    [Header("Brush")]
    public Image dotPrefab;
    public float dotSpacing = 20f;   // better for big area
    public float dotSize = 40f;      // good for your 1600x750 area

    [Header("Quiz Validation")]
    public float totalLengthRequired = 300f; // good for 1600x750
    public UnityEvent onQuizFinished;
    public UnityEvent onAllCorrect;

    private bool isDrawing = false;
    private Vector2 lastLocalPoint;
    private float accumulatedLength = 0f;

    public bool HasValidDrawing => accumulatedLength >= totalLengthRequired;

    void Start()
    {
        if (questions.Count <= 0)
        {
            Debug.LogWarning("No questions assigned.");
            return;
        }

        // Prepare results list
        questionResults.Clear();
        for (int i = 0; i < questions.Count; i++)
        {
            questionResults.Add(false);
        }

        currentQuestion = 0;
        ShowQuestion();
    }
    void Update()
    {
        HandleMouse();
        HandleTouch();
    }
    
    public void OpenQuiz(bool open)
    {
        QuizPanel.SetActive(open);
        SettingManager.Instance.isPaused = open;
    } 
    void ShowQuestion()
    {
        if (currentQuestion >= questions.Count)
        {
            FinishQuiz();
            return;
        }

        questionText.text = questions[currentQuestion];
        ClearDrawing();
    }

    public void NextQuestion()
    {
        if (questions.Count <= 0) return;
        if (currentQuestion >= questions.Count) return;

        // Save current question result
        questionResults[currentQuestion] = HasValidDrawing;

        currentQuestion++;

        if (currentQuestion >= questions.Count)
        {
            FinishQuiz();
        }
        else
        {
            ShowQuestion();
        }
    }

    void FinishQuiz()
    {
        int correctCount = 0;

        for (int i = 0; i < questionResults.Count; i++)
        {
            if (questionResults[i])
                correctCount++;
        }

        Debug.Log($"Quiz Finished! Correct: {correctCount}/{questions.Count}");

        if (correctCount == questions.Count)
        {
            onAllCorrect?.Invoke();
        }

        onQuizFinished?.Invoke();
    }

    void HandleMouse()
    {
        if (Input.touchCount > 0) return;

        if (Input.GetMouseButtonDown(0))
        {
            TryBeginDraw(Input.mousePosition);
        }

        if (Input.GetMouseButton(0) && isDrawing)
        {
            ContinueDraw(Input.mousePosition);
        }

        if (Input.GetMouseButtonUp(0))
        {
            EndDraw();
        }
    }

    void HandleTouch()
    {
        if (Input.touchCount == 0) return;

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

    void TryBeginDraw(Vector2 screenPos)
    {
        if (IsPointerOverUIButNotDrawArea(screenPos))
            return;

        if (ScreenPointToLocalPointInDrawArea(screenPos, out Vector2 localPoint))
        {
            isDrawing = true;
            lastLocalPoint = localPoint;

            SpawnDot(localPoint);
        }
    }

    void ContinueDraw(Vector2 screenPos)
    {
        if (!ScreenPointToLocalPointInDrawArea(screenPos, out Vector2 localPoint))
        {
            EndDraw();
            return;
        }

        float distance = Vector2.Distance(lastLocalPoint, localPoint);

        if (distance < dotSpacing)
            return;

        int steps = Mathf.FloorToInt(distance / dotSpacing);

        for (int i = 1; i <= steps; i++)
        {
            Vector2 point = Vector2.Lerp(lastLocalPoint, localPoint, (float)i / steps);
            SpawnDot(point);
        }

        accumulatedLength += distance;
        lastLocalPoint = localPoint;
    }

    void EndDraw()
    {
        isDrawing = false;
    }

    bool ScreenPointToLocalPointInDrawArea(Vector2 screenPos, out Vector2 localPoint)
    {
        localPoint = Vector2.zero;

        if (!RectTransformUtility.RectangleContainsScreenPoint(drawArea, screenPos, uiCamera))
            return false;

        return RectTransformUtility.ScreenPointToLocalPointInRectangle(drawArea, screenPos, uiCamera, out localPoint);
    }

    void SpawnDot(Vector2 localPoint)
    {
        Image dot = Instantiate(dotPrefab, strokeContainer);
        RectTransform rt = dot.rectTransform;
        rt.anchoredPosition = localPoint;
        rt.sizeDelta = new Vector2(dotSize, dotSize);
    }

    bool IsPointerOverUIButNotDrawArea(Vector2 screenPos)
    {
        return false;
    }

    public void ClearDrawing()
    {
        for (int i = strokeContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(strokeContainer.GetChild(i).gameObject);
        }

        accumulatedLength = 0f;
        isDrawing = false;
    }

    public int GetCorrectCount()
    {
        int correctCount = 0;

        for (int i = 0; i < questionResults.Count; i++)
        {
            if (questionResults[i])
                correctCount++;
        }

        return correctCount;
    }
}
