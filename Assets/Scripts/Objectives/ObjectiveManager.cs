using System.Collections.Generic;
using UnityEngine;

public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager Instance;
    public List<ObjectiveData> objectiveDatas = new List<ObjectiveData>();
    public Objectives ObjectivePrefab;
    public Transform ObjectivesParent;
    List<Objectives> Objectives = new List<Objectives>();

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
            AddObjective(objData.Name, objData.Description);
        }
    }
    public void AddObjective(string name, string description, bool isHidden = false)
    {
        ObjectiveData newObjectiveData = new ObjectiveData { Name = name, Description = description, IsCompleted = false, isHidden = isHidden };
        Objectives newObjective = Instantiate(ObjectivePrefab, ObjectivesParent);
        newObjective.gameObject.SetActive(!isHidden);
        newObjective.objectiveData = newObjectiveData;
        newObjective.UpdateObjectiveText();
        Objectives.Add(newObjective);
    }
    public void CompleteObjective(string objectiveName)
    {
        Objectives obj = Objectives.Find(o => o.objectiveData.Name == objectiveName);
        if (obj != null)
        {
            obj.objectiveData.IsCompleted = true;
            obj.UpdateObjectiveText();
        }
    }
}
