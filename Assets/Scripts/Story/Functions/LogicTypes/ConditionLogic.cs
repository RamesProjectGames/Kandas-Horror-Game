using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TestingPurposes;
using UnityEngine;
using static Dialogue.LogicLines.DialogicUtils.Encapsulation;

namespace Dialogue.LogicLines
{
    public class ConditionLogic : IDialogic
    {
        public string keyword => "if";
        private const string ELSE = "else";
        private readonly string[] CONTAINERS = new string[] {"(", ")"};
        private static readonly string OPERATOR_REGEX = @"(==|!=|<|>|<=|>=|&&|\|\|)";

        public IEnumerator Execute(DialogueStructure line)
        {
            if(line.GetRawLine() == null)
                yield return null;
            string rawCondition = ExtractCondition(line.GetRawLine().Trim());
            Debug.Log($"rawCondition: {rawCondition}");
            bool conditionCheck = ConditionCheck(rawCondition);

            Convo currConvo = DialogueSystem.Instance.convoManager.convo;
            int currProgress = DialogueSystem.Instance.convoManager.convoProgress;

            EncapsulatedData ifData = RipEncapsulatedData(currConvo, currProgress, false);
            EncapsulatedData elseData = new EncapsulatedData();

            if(ifData.endingIdx + 1 < currConvo.count)
            {
                string nextLine = currConvo.GetLines()[ifData.endingIdx+1].Trim();
                if(nextLine.ToLower() == ELSE.ToLower())
                {
                    elseData = RipEncapsulatedData(currConvo, ifData.endingIdx + 1, false);
                    ifData.endingIdx = elseData.endingIdx;
                }
            }

            EncapsulatedData selectedData = conditionCheck ? ifData : elseData;
            currConvo.SetProgress(ifData.endingIdx);
            if (!selectedData.isNull && selectedData.lines.Count > 0)
            {
                Convo newConvo = new Convo(selectedData.lines);

                foreach (string selectedLines in newConvo.GetLines())
                {
                    Debug.Log($"if: {selectedLines}");
                }
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
            int endIdx = line.LastIndexOf(CONTAINERS[1]);

            return line.Substring(startIdx, endIdx - startIdx);
        }

        private delegate string ConditionFunc<T>(T arg);
        private Dictionary<string, ConditionFunc<string>> condFunc = new Dictionary<string, ConditionFunc<string>>
        {
            {"Objective", obj =>
                {
                    ObjectiveData objData = ObjectiveManager.Instance.objectiveDatas.Find(x => x.Name.ToLower() == obj.ToLower());
                    if(objData != null)
                    {
                        return objData.IsCompleted.ToString();
                    }
                    return false.ToString();
                }
            },
            {"DoorAttempts", x =>
                {
                    Debug.Log($"{DialogueEvents.DoorAttempts} attempts");
                    return DialogueEvents.DoorAttempts.ToString();
                }
            }
        };

        private bool ConditionCheck(string condition)
        {
            string[] condParts = Regex.Split(condition, OPERATOR_REGEX).Select(p => p.Trim()).ToArray();
            bool[] negate = new bool[condParts.Length];
            for (int i = 0; i < condParts.Length; i++)
            {
                Debug.Log(condParts[i]);
                if (condParts[i].StartsWith("\"") && condParts[i].EndsWith("\""))
                {
                    condParts[i] = condParts[i].Substring(1, condParts[i].Length - 2);
                }
                if (condParts[i].Contains('(') && condParts[i].Contains(')'))
                {
                    negate[i] = false;
                    if (condParts[i].StartsWith('!'))
                    {
                        negate[i] = true;
                        condParts[i] = condParts[i].Substring(1).Trim();
                    }
                    int idx = condParts[i].IndexOf('(');
                    string func = condParts[i].Substring(0, idx).Trim();
                    string arg = condParts[i].Substring(idx + 1, condParts[i].Length - idx - 2);
                    if (condFunc.ContainsKey(func))
                    {
                        condParts[i] = condFunc[func](arg);
                    }
                }
            }
            if(condParts.Length == 1)
            {
                if (bool.TryParse(condParts[0], out bool result))
                {
                    return negate[0] ^ result;
                }
                else
                {
                    Debug.LogError("Objective not Found");
                    return false;
                }
            }
            else if(condParts.Length == 3)
            {
                return EvaluateOperations(condParts[0], negate[0], condParts[1], condParts[2], negate[2]);
            }
            else
            {
                Debug.LogError($"Unsupported Conditions format: {condition}");
                return false;
            }
        }

        #region Condition Logic
        private delegate bool OperatorFunc<T>(T left, T right);

        private static Dictionary<string, OperatorFunc<bool>> boolOps = new Dictionary<string, OperatorFunc<bool>>()
        {
            { "&&", (left, right) => left && right },
            { "||", (left, right) => left || right },
            { "==", (left, right) => left == right },
            { "!=", (left, right) => left != right },
        };

        private static Dictionary<string, OperatorFunc<float>> floatOps = new Dictionary<string, OperatorFunc<float>>()
        {
            { "==", (left, right) => left == right },
            { "!=", (left, right) => left != right },
            { ">=", (left, right) => left >= right },
            { "<=", (left, right) => left <= right },
            { ">", (left, right) => left > right },
            { "<", (left, right) => left < right },
        };

        private static Dictionary<string, OperatorFunc<int>> intOps = new Dictionary<string, OperatorFunc<int>>()
        {
            { "==", (left, right) => left == right },
            { "!=", (left, right) => left != right },
            { ">=", (left, right) => left >= right },
            { "<=", (left, right) => left <= right },
            { ">", (left, right) => left > right },
            { "<", (left, right) => left < right },
        };

        private static bool EvaluateOperations(string left, bool leftNegate, string op, string right, bool rightNegate)
        {
            if (bool.TryParse(left, out bool leftBool) && bool.TryParse(right, out bool rightBool))
            {
                if(boolOps.ContainsKey(op))
                    return boolOps[op](leftBool^leftNegate, rightBool^rightNegate);
            }
            else if(float.TryParse(left, out float leftFloat) && float.TryParse(right, out float rightFloat))
            {
                if (floatOps.ContainsKey(op))
                    return floatOps[op](leftFloat, rightFloat);
            }
            else if (int.TryParse(left, out int leftInt) && int.TryParse(right, out int rightInt))
            {
                if (intOps.ContainsKey(op))
                    return intOps[op](leftInt, rightInt);
            }

            switch (op)
            {
                case "==": return left == right;
                case "!=": return left != right;
                default: throw new InvalidOperationException($"Unsupported Operation: {op}");
            }
        }
        #endregion
    }
}
