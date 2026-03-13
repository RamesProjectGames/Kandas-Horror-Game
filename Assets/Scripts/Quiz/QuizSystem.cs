using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QuizSystem : MonoBehaviour
{
    public List<QuizData> quizzes = new List<QuizData>();
    public TMP_Text QuestionText;
    public List<TMP_Text> AnswerTexts = new List<TMP_Text>();

    public void PopulateQuiz()
    {
        foreach (QuizData quiz in quizzes)
        {
            QuestionText.text = quiz.question;
            for (int i = 0; i < quiz.answers.Count; i++)
            {
                AnswerTexts[i].text = quiz.answers[i].answer;
            }
        }
    }
}
