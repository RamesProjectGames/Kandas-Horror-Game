using System.Collections.Generic;
using System.Linq;
using Dialogue;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class OutOfBound : MonoBehaviour
{
    public enum TriggerTargetType
    {
        Player,
        Tag,
        GameObjectName
    }

    public List<ObjectiveDialoguePair> objectiveDialoguePair = new List<ObjectiveDialoguePair>(); // List of dialogue pairs for different objectives
    public List<string> relatedObjectives = new List<string>(); // List of objective IDs related to this out-of-bounds area

    [SerializeField] private TriggerTargetType triggerTargetType = TriggerTargetType.Player;
    [SerializeField] private string targetTag = "Player";
    [SerializeField] private string targetObjectName = "";

    private BoxCollider col;

    void Start()
    {
        col = GetComponent<BoxCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        if (ObjectiveManager.Instance == null)
            return;

        col.enabled = relatedObjectives.Any(x => ObjectiveManager.Instance.currentObjectives.Contains(x));
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"ShouldTrigger: {ShouldTrigger(other.gameObject)}");
        if (ShouldTrigger(other.gameObject))
        {
            TriggerDialogue();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (ShouldTrigger(collision.collider.gameObject))
        {
            TriggerDialogue();
        }
    }

    private bool ShouldTrigger(GameObject otherObject)
    {
        switch (triggerTargetType)
        {
            case TriggerTargetType.Player:
                return otherObject.CompareTag("Player");

            case TriggerTargetType.Tag:
                return !string.IsNullOrWhiteSpace(targetTag) && otherObject.CompareTag(targetTag);

            case TriggerTargetType.GameObjectName:
                return !string.IsNullOrWhiteSpace(targetObjectName) && otherObject.name == targetObjectName;

            default:
                return false;
        }
    }

    public void TriggerDialogue()
    {
        if (ObjectiveManager.Instance == null)
        {
            Debug.Log("OutOfBound.TriggerDialogue skipped because ObjectiveManager is missing.");
            return;
        }

        if (DialogueSystem.Instance == null)
        {
            Debug.Log("OutOfBound.TriggerDialogue skipped because DialogueSystem is missing.");
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
