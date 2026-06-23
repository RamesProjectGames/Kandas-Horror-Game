using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class Objective : MonoBehaviour
{
    public ObjectiveData objectiveData;
    public TextMeshProUGUI objectiveText;
    const string bulletpoint = "<space=5px>•<space=10px> ";
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void UpdateObjectiveText()
    {
        if(objectiveData.IsCompleted)
        {
            objectiveText.text = $"{bulletpoint}<color=red><s>{objectiveData.Description}</s></color>";
        }
        else
        {
            objectiveText.text = $"{bulletpoint}{objectiveData.Description}";
        }
    }
}
[System.Serializable]
public class ObjectiveData
{
    public string Name;
    public string Description;
    public bool IsCompleted;
    public bool isHidden;
    public bool isDemo;
    public FragmentData fragmentData;
    [Tooltip("These objectives must be cleared to access this objective")]
    public List<string> requirements = new List<string>();
    [Tooltip("This objective no longer doable if the following objective is completed, regardless of completion status")]
    public string LimitedAfterObjective;
    public GameObject ObjectiveObject;
    public int Chapter;
}
