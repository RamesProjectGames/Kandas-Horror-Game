using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager Instance;
    public List<ObjectiveData> objectiveDatas = new List<ObjectiveData>();
    public Objective ObjectivePrefab;
    public Transform ObjectivesParent;
    List<Objective> Objectives = new List<Objective>();
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
        Objective newObjective = Instantiate(ObjectivePrefab, ObjectivesParent);
        if (objData.fragmentData != null)
        {
            objData.isHidden = true;
        }
        newObjective.objectiveData = objData;
        newObjective.UpdateObjectiveText();
        Objectives.Add(newObjective);
    }

    public void CompleteObjective(string objectiveName)
    {
        Objective obj = Objectives.Find(o => o.objectiveData.Name == objectiveName);
        if (obj != null && (string.IsNullOrEmpty(obj.objectiveData.LimitedAfterObjective) || !Objectives.Find(x => x.objectiveData.Name == obj.objectiveData.LimitedAfterObjective).objectiveData.IsCompleted))
        {
            obj.objectiveData.IsCompleted = true;
            obj.UpdateObjectiveText();
        }
        UpdateCurrentObjectives();
    }

    public void UpdateCurrentObjectives()
    {
        List<Objective> openObjectives = Objectives.FindAll(x => string.IsNullOrEmpty(x.objectiveData.LimitedAfterObjective) || !Objectives.Find(y => y.objectiveData.Name == x.objectiveData.LimitedAfterObjective).objectiveData.IsCompleted);

        foreach (Objective obj in openObjectives)
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
                if(obj.objectiveData.fragmentData != null && !FragmentManager.Instance.allFragments.Contains(obj.objectiveData.fragmentData))
                {
                    FragmentManager.Instance.SpawnFragmentInScene(obj.objectiveData.fragmentData);
                }
            }
        }

        //Remove all locked objectives
        List<Objective> closedObjectives = Objectives.FindAll(x => !string.IsNullOrEmpty(x.objectiveData.LimitedAfterObjective) && Objectives.Find(y => y.objectiveData.Name == x.objectiveData.LimitedAfterObjective).objectiveData.IsCompleted);

        foreach (Objective obj in closedObjectives)
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
            if (obj.objectiveData.fragmentData != null)
            {
                FragmentManager.Instance.UpdateFragmentState(obj.objectiveData.fragmentData);
            }
        }
    }

    public bool isCurrentAndNotCompleted(string objName)
    {
        return currentObjectives.Contains(objName) && !Objectives.Find(x => x.objectiveData.Name == objName).objectiveData.IsCompleted;
    }

    public bool CheckIfFragmentValid(FragmentData fragData)
    {
        return currentObjectives.Contains(objectiveDatas.Find(x => x.fragmentData == fragData).Name) && !objectiveDatas.Find(x => x.fragmentData == fragData).IsCompleted;
    }
}
