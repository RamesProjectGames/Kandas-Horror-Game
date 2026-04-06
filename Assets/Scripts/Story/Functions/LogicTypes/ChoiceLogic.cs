using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dialogue.LogicLines
{
    public class ChoiceLogic : IDialogic
    {
        public string keyword => "Choice";
        private const char ENC_START = '{';
        private const char ENC_END = '}';
        private const char CHOICE = '}';

        public IEnumerator Execute(DialogueStructure line)
        {
            yield return null;
        }

        public bool Matches(DialogueStructure line)
        {
            return (line.hasSpeaker && line.speaker.ToLower() == keyword);
        }

        //private DialogueChoiceData RipDialogueChoiceData()
        //{
        //    return ;
        //}

        private struct DialogueChoiceData
        {
            public List<string> lines;
            public int endingIdx;
        }
    }
}
