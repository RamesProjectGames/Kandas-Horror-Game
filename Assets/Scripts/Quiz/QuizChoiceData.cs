using System;
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
    public bool IsCorrect(string userInput, bool ignoreCase = true)
    {
        if (string.IsNullOrWhiteSpace(userInput))
            return false;

        var validAnswers = GetAnswers();

        foreach (var valid in validAnswers)
        {
            if (ignoreCase)
            {
                if (string.Equals(userInput.Trim(), valid, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            else
            {
                if (userInput.Trim() == valid)
                    return true;
            }
        }

        return false;
    }
}
