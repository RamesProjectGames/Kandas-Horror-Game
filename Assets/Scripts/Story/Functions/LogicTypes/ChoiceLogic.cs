using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Dialogue.LogicLines.DialogicUtils.Encapsulation;

namespace Dialogue.LogicLines
{
    public class ChoiceLogic : IDialogic
    {
        public string keyword => "Choice";
        private const char CHOICE = '>';

        public IEnumerator Execute(DialogueStructure line)
        {
            if (line.GetRawLine() == null)
                yield return null;
            var currConvo = DialogueSystem.Instance.convoManager.convo;
            var progress = DialogueSystem.Instance.convoManager.convoProgress;
            EncapsulatedData choiceData = RipEncapsulatedData(currConvo, progress, true);
            yield return null;
        }

        private bool IsChoiceStart(string line) => line.Trim().StartsWith(CHOICE);

        public bool Matches(DialogueStructure line)
        {
            return (!string.IsNullOrEmpty(line.GetRawLine()) && line.GetRawLine().ToLower() == keyword);
        }
    }
}
