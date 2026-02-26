using TMPro;
using UnityEngine;

public class Objectives : MonoBehaviour
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
    public string nextObjective;
    public GameObject ObjectiveObject;
}
