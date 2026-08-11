using System.Collections.Generic;
using System.Linq;
using Dialogue;
using UnityEngine;

public class LocalDialogueManager : MonoBehaviour
{
     public List<ObjectiveDialoguePair> objectiveDialoguePair = new List<ObjectiveDialoguePair>();
    public void TriggerDialogue()
    {
        if (ObjectiveManager.Instance == null)
        {
            Debug.Log("OutOfBound.TriggerDialogue skipped because ObjectiveManager is missing.");
            return;
        }

        if (objectiveDialoguePair == null || objectiveDialoguePair.Count == 0)
        {
            Debug.Log("OutOfBound.TriggerDialogue skipped because no dialogue pairs were assigned.");
            return;
        }

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
            Debug.Log($"OutOfBound opening objective dialogue: {bestMatch.Pair.dialogueAsset}");
            DialogueSystem.Instance.OpenDialogue(bestMatch.Pair.dialogueAsset);
            return;
        }

        ObjectiveDialoguePair fallback = objectiveDialoguePair.Find(x => x?.objective != null && x.objective.Length > 0 && x.objective[0] == string.Empty);
        if (fallback == null)
        {
            fallback = objectiveDialoguePair.Find(x => x?.objective != null && x.objective.Length == 0);
        }

        if (fallback != null)
        {
            Debug.Log($"OutOfBound opening fallback dialogue: {fallback.dialogueAsset}");
            DialogueSystem.Instance.OpenDialogue(fallback.dialogueAsset);
        }
        else
        {
            Debug.LogWarning("OutOfBound could not find a matching or fallback dialogue entry.");
        }
    }
}
