using System.Collections.Generic;
using UnityEngine;

public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager Instance;
    public List<ObjectiveData> objectiveDatas = new List<ObjectiveData>();
    public Objectives ObjectivePrefab;
    public Transform ObjectivesParent;
    List<Objectives> Objectives = new List<Objectives>();
    public List<string> currentObjectives = new List<string>();

    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        foreach (var objData in objectiveDatas)
        {
            AddObjective(objData);
        }
        UpdateCurrentObjectives();
    }
    public void AddObjective(ObjectiveData objData)
    {
        Objectives newObjective = Instantiate(ObjectivePrefab, ObjectivesParent);
        newObjective.objectiveData = objData;
        newObjective.UpdateObjectiveText();
        Objectives.Add(newObjective);
    }

    public void CompleteObjective(string objectiveName)
    {
        Objectives obj = Objectives.Find(o => o.objectiveData.Name == objectiveName);
        if (obj != null && (string.IsNullOrEmpty(obj.objectiveData.LimitedAfterObjective) || !Objectives.Find(x => x.objectiveData.Name == obj.objectiveData.LimitedAfterObjective).objectiveData.IsCompleted))
        {
            obj.objectiveData.IsCompleted = true;
            obj.UpdateObjectiveText();
        }
        UpdateCurrentObjectives();
    }

    public void UpdateCurrentObjectives()
    {
        List<Objectives> openObjectives = Objectives.FindAll(x => string.IsNullOrEmpty(x.objectiveData.LimitedAfterObjective) || !Objectives.Find(y => y.objectiveData.Name == x.objectiveData.LimitedAfterObjective).objectiveData.IsCompleted);

        foreach (Objectives obj in openObjectives)
        {
            bool isCurrent = true;
            foreach(string req in obj.objectiveData.requirements)
            {
                //Remove open objectives where the requirements aren't completed
                if(!Objectives.Find(x => x.objectiveData.Name == req).objectiveData.IsCompleted)
                {
                    isCurrent = false;
                    if (currentObjectives.Contains(obj.objectiveData.Name))
                    {
                        currentObjectives.Remove(obj.objectiveData.Name);
                    }
                    break;
                }
            }

            //Add open objectives where requirements are completed
            if (isCurrent && !currentObjectives.Contains(obj.objectiveData.Name))
            {
                currentObjectives.Add(obj.objectiveData.Name);
            }
        }

        //Remove all locked objectives
        List<Objectives> closedObjectives = Objectives.FindAll(x => !string.IsNullOrEmpty(x.objectiveData.LimitedAfterObjective) && Objectives.Find(y => y.objectiveData.Name == x.objectiveData.LimitedAfterObjective).objectiveData.IsCompleted);

        foreach (Objectives obj in closedObjectives)
        {
            if (currentObjectives.Contains(obj.objectiveData.Name))
            {
                currentObjectives.Remove(obj.objectiveData.Name);
            }
        }

        UpdateObjectiveViewList();
    }

    public void UpdateObjectiveViewList()
    {
        foreach (var obj in Objectives)
        {
            if(currentObjectives.Contains(obj.objectiveData.Name) && !obj.objectiveData.isHidden)
            {
                obj.gameObject.SetActive(true);
            }
            else
            {
                obj.gameObject.SetActive(false);
            }
        }
    }

    public bool isCurrentAndNotCompleted(string objName)
    {
        return currentObjectives.Contains(objName) && !Objectives.Find(x => x.objectiveData.Name == objName).objectiveData.IsCompleted;
    }
}
