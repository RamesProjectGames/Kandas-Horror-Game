using Dialogue;
using System.Collections;
using UnityEngine;
using static Dialogue.LogicLines.DialogicUtils.Encapsulation;
using static UnityEngine.Rendering.GPUSort;

namespace Dialogue.LogicLines
{
    public class ConditionLogic : IDialogic
    {
        public string keyword => "if";
        private const string ELSE = "else";
        private readonly string[] CONTAINERS = new string[] {"(", ")"};

        public IEnumerator Execute(DialogueStructure line)
        {
            if(line.GetRawLine() == null)
                yield return null;
            string rawCondition = ExtractCondition(line.GetRawLine().Trim());
            bool conditionCheck = ObjectiveCleared(rawCondition);

            Convo currConvo = DialogueSystem.Instance.convoManager.convo;
            int currProgress = DialogueSystem.Instance.convoManager.convoProgress;

            EncapsulatedData ifData = RipEncapsulatedData(currConvo, currProgress, false), elseData = new EncapsulatedData();

            if(ifData.endingIdx + 1 == currConvo.count)
            {
                string nextLine = currConvo.GetLines()[ifData.endingIdx+1].GetRawLine().Trim();
                if(nextLine.ToLower() == ELSE.ToLower())
                {
                    elseData = RipEncapsulatedData(currConvo, ifData.endingIdx + 1, false);
                    ifData.endingIdx = elseData.endingIdx;
                }
            }

            EncapsulatedData selectedData = conditionCheck ? ifData : elseData;
            currConvo.SetProgress(selectedData.endingIdx);
            if (!selectedData.isNull && selectedData.lines.Count > 0)
            {
                Convo newConvo = new Convo(selectedData.lines);
                DialogueSystem.Instance.convoManager.EnqueuePrio(newConvo);
            }

            yield return null;
        }

        public bool Matches(DialogueStructure line)
        {
            return !string.IsNullOrEmpty(line.GetRawLine()) && line.GetRawLine().Trim().StartsWith(keyword);
        }

        private string ExtractCondition(string line)
        {
            int startIdx = line.IndexOf(CONTAINERS[0])+1;
            int endIdx = line.IndexOf(CONTAINERS[1]);

            return line.Substring(startIdx, endIdx - startIdx);
        }

        private bool ObjectiveCleared(string condition)
        {
            bool negate = false;
            if (condition.StartsWith('!'))
            {
                negate = true;
                condition = condition.Substring(1).Trim();
            }
            ObjectiveData objData = ObjectiveManager.Instance.objectiveDatas.Find(x => x.Name.ToLower() == condition.ToLower());
            if(objData == null)
            {
                Debug.LogError("Objective not Found");
                return false;
            }

            return negate ? !objData.IsCompleted : objData.IsCompleted;
        }
    }
}
