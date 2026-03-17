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
            if (pickupText != null)
            {
                if (interactAction != null)
                {
                    string bindingDisplay = interactAction.action.GetBindingDisplayString(0);
                    ButtonInteractionText.text = $"{bindingDisplay}";
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
            //Check if Objective and Dialogue Pair Exists
            if (ObjectiveManager.Instance == null) return;
            if (objectiveDialoguePair == null || objectiveDialoguePair.Count == 0) return;

            //NPC faces player
            if(GetComponent<NpcMovement>() != null)
                GetComponent<NpcMovement>().StartCoroutine(GetComponent<NpcMovement>().FacePlayer());

            //Find the best matching dialogue pair
            var bestMatch = objectiveDialoguePair.Select(pair => new
            {
                Pair = pair,
                MatchCount = pair.objective.Count(obj => ObjectiveManager.Instance.isCurrentAndNotCompleted(obj)),
                //MatchCount and EarliestMatchIndex might be interchangeable if needed, currently brain fried to consider
                EarliestMatchIndex = pair.objective.Where(obj => ObjectiveManager.Instance.currentObjectives.Contains(obj))
                    .Select(obj => ObjectiveManager.Instance.currentObjectives.IndexOf(obj))
                    .DefaultIfEmpty(int.MaxValue).Min()
            })
            .Where(x => x.MatchCount > 0).OrderByDescending(x => x.MatchCount)
            .ThenBy(x => x.EarliestMatchIndex)
            .FirstOrDefault();

            if (bestMatch != null)
            {
                DialogueSystem.Instance.OpenDialogue(bestMatch.Pair.dialogueAsset);
            }
            else
            {
                DialogueSystem.Instance.OpenDialogue(objectiveDialoguePair.Find(x => x.objective.Length == 0).dialogueAsset);
            }
        }
    }

    [Serializable]
    public class ObjectiveDialoguePair
    {
        [Tooltip("If There are no Objectives, it'd be considered default convo")]
        [SerializeField] public string[] objective;
        [SerializeField] public string dialogueAsset;
    }
}