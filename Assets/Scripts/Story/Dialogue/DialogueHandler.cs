using Dialogue;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Dialogue
{
    public class DialogueHandler : ItemInteraction
    {
        [SerializeField] private List<ObjectiveDialoguePair> objectiveDialoguePair;

        void Update()
        {
            if (pickupText != null && inputActions != null)
            {
                var interactAction = inputActions.FindAction("Interact");
                if (interactAction != null && CheckCurrentObjectives())
                {
                    string bindingDisplay = interactAction.GetBindingDisplayString(0);
                    pickupText.text = $"{ItemInteractionText}";
                }
                else
                {
                    pickupText.gameObject.SetActive(false);
                }
            }
        }

        public void TriggerDialogue()
        {
            if (ObjectiveManager.Instance == null) return;
            if (objectiveDialoguePair == null || objectiveDialoguePair.Count == 0) return;

            if(GetComponent<NpcMovement>() != null)
                GetComponent<NpcMovement>().StartCoroutine(GetComponent<NpcMovement>().FacePlayer());
            List<string> currObjectives = ObjectiveManager.Instance.CurrentObjectives();
            var bestMatch = objectiveDialoguePair.Select(pair => new {
                Pair = pair,
                MatchCount = pair.objective.Count(obj => currObjectives.Contains(obj)),
                //MatchCount and EarliestMatchIndex might be interchangeable if needed, currently brain fried to consider
                EarliestMatchIndex = pair.objective.Where(obj => currObjectives.Contains(obj)).Select(obj => currObjectives.IndexOf(obj))
                .DefaultIfEmpty(int.MaxValue).Min()
            }).Where(x => x.MatchCount > 0).OrderByDescending(x => x.MatchCount)
            .ThenBy(x => x.EarliestMatchIndex)
            .FirstOrDefault();

            if (bestMatch != null)
            {
                DialogueSystem.Instance.OpenDialogue(bestMatch.Pair.dialogueAsset);
            }
        }

        bool CheckCurrentObjectives()
        {
            if (ObjectiveManager.Instance == null) return false;
            if (objectiveDialoguePair == null || objectiveDialoguePair.Count == 0) return false;

            List<string> currObjectives = ObjectiveManager.Instance.CurrentObjectives();
            // Use Any() - stops at first match
            return currObjectives.Any(obj =>
                objectiveDialoguePair.Any(pair => pair.objective.Contains(obj))
            );
        }
    }

    [Serializable]
    public class ObjectiveDialoguePair
    {
        [SerializeField] public string[] objective;
        [SerializeField] public string dialogueAsset;
    }
}