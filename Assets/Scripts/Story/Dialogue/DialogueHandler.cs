using Dialogue;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Dialogue
{
    public class DialogueHandler : ItemInteraction
    {
        [SerializeField] private List<ObjectiveDialoguePair> objectiveDialoguePair;

        public void TriggerDialogue()
        {
            List<string> currObjectives = ObjectiveManager.Instance.CurrentObjectives();

            DialogueSystem.Instance.OpenDialogue(objectiveDialoguePair.First(x => currObjectives.Contains(x.objective)).dialogueAsset);
        }
    }

    [Serializable]
    public class ObjectiveDialoguePair
    {
        [SerializeField] public string objective;
        [SerializeField] public string dialogueAsset;
    }
}