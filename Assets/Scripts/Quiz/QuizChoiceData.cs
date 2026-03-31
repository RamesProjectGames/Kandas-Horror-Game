using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "QuizChoiceData", menuName = "QuizChoiceData", order = 0)]
public class QuizChoiceData : ScriptableObject {
    [Multiline(3)]
    public string questionText;
    public string answers;
    public string correctAnswer;

    public List<string> GetAnswers()
    {
        List<string> results = new List<string>();

        if (string.IsNullOrWhiteSpace(answers))
            return results;

        string[] split = answers.Split('|');

        foreach (string item in split)
        {
            string trimmed = item.Trim();
            if (!string.IsNullOrEmpty(trimmed))
                results.Add(trimmed);
        }

        return results;
    }
}
