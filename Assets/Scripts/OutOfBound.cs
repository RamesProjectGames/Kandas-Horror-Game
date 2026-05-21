using System.Collections.Generic;
using System.Linq;
using Dialogue;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class OutOfBound : MonoBehaviour
{
    public List<ObjectiveDialoguePair> objectiveDialoguePair = new List<ObjectiveDialoguePair>(); // List of dialogue pairs for different objectives
    public List<string> relatedObjectives = new List<string>(); // List of objective IDs related to this out-of-bounds area
    BoxCollider col;
    void Start()
    {
        col = GetComponent<BoxCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        if(ObjectiveManager.Instance == null)
            return;
        foreach (var obj in ObjectiveManager.Instance.currentObjectives)
        {
            col.enabled = relatedObjectives.Contains(obj);
        }
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Handle player entering the out-of-bounds area
            
                TriggerDialogue();
        }
    }
    public void TriggerDialogue()
    {
        if (ObjectiveManager.Instance == null) return;
        if (objectiveDialoguePair == null) return;

        var bestMatch = objectiveDialoguePair
            .Select(pair => new
            {
                Pair = pair,
                MatchCount = pair.objective.Count(obj => ObjectiveManager.Instance.isCurrentAndNotCompleted(obj)),
                EarliestMatchIndex = pair.objective
                    .Where(obj => ObjectiveManager.Instance.currentObjectives.Contains(obj))
                    .Select(obj => ObjectiveManager.Instance.currentObjectives.IndexOf(obj))
                    .DefaultIfEmpty(int.MaxValue)
                    .Min()
            })
            .Where(x => x.MatchCount > 0)
            .OrderByDescending(x => x.MatchCount)
            .ThenBy(x => x.EarliestMatchIndex)
            .FirstOrDefault();

        if (bestMatch != null)
        {
            DialogueSystem.Instance.OpenDialogue(bestMatch.Pair.dialogueAsset);
        }
        else
        {
            ObjectiveDialoguePair fallback = objectiveDialoguePair.Find(x => x.objective.Length == 0);
            if (fallback != null)
            {
                DialogueSystem.Instance.OpenDialogue(fallback.dialogueAsset);
            }
        }
    }
}
