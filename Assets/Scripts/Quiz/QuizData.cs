using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "QuizData", order = 1)]
public class QuizData : ScriptableObject
{
    public string question;
    public List<QuizAnswer> answers = new List<QuizAnswer>();
}
[System.Serializable]
public class QuizAnswer
{
    public string answer;
    public bool isCorrect;
}
