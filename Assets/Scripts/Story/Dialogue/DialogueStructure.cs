using Dialogue;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Dialogue
{
    public class DialogueStructure
    {
        public string rawLine = string.Empty;
        public string GetRawLine() => string.IsNullOrWhiteSpace(rawLine) ? string.Empty : rawLine;
        public string speaker;

        //Dialogue
        public List<DialogueData> dialogue;
        public List<FunctionsData> functions;
        private const string segmentIdPattern = @"\{[ca]\}|\{w[ca]\s\d*\.?\d*\}";

        //Functions
        private const char functionSplit = ',';
        private const char argumentContainer = '(';
        private const string waitSignal = "[w]";

        public DialogueStructure(string rawLine, string speaker, string dialogue, string functions)
        {
            this.rawLine = rawLine;
            this.speaker = speaker;
            this.dialogue = string.IsNullOrWhiteSpace(dialogue) ? new List<DialogueData>() : RipDialogue(dialogue);
            this.functions = string.IsNullOrWhiteSpace(functions) ? new List<FunctionsData>() : RipFunctions(functions);
        }
        public bool hasSpeaker => speaker != string.Empty;
        public bool hasDialogue => dialogue.Count > 0;
        public bool hasFunctions => functions.Count > 0;

        public List<DialogueData> RipDialogue(string rawDialogue)
        {
            List<DialogueData> segments = new List<DialogueData>();
            MatchCollection matches = Regex.Matches(rawDialogue, segmentIdPattern);

            int lastIdx = 0;
            string firstSegment = matches.Count == 0 ? rawDialogue : rawDialogue.Substring(0, matches[0].Index);
            if (!string.IsNullOrWhiteSpace(firstSegment))
            {
                DialogueData segment = new DialogueData
                {
                    rawData = rawDialogue,
                    dialogue = firstSegment,
                    segmentSignal = DialogueData.SegmentSignal.C,
                    signalDelay = 0
                };
                segments.Add(segment);
            }

            if(matches.Count == 0)
                return segments;
            else
                lastIdx = matches[0].Index;

            for (int i = 0; i < matches.Count; i++)
            {
                Match match = matches[i];
                DialogueData segment = new DialogueData();

                //Signal
                string signalMatch = match.Value;
                signalMatch = signalMatch.Substring(1, match.Length - 2);
                string[] signalSplit = signalMatch.Split(' ');

                segment.segmentSignal = (DialogueData.SegmentSignal) Enum.Parse(typeof(DialogueData.SegmentSignal), signalSplit[0].ToUpper());

                //Wait delay
                if(signalSplit.Length > 1)
                    float.TryParse(signalSplit[1], out segment.signalDelay);

                //Dialogue
                int nextIdx = i + 1 < matches.Count ? matches[i+1].Index : rawDialogue.Length;
                segment.dialogue = rawDialogue.Substring(lastIdx + match.Length, nextIdx - (lastIdx + match.Length));
                lastIdx = nextIdx;

                segments.Add(segment);
            }

            return segments;
        }

        public List<FunctionsData> RipFunctions(string rawFunctions)
        {
            string[] data = rawFunctions.Split(functionSplit, StringSplitOptions.RemoveEmptyEntries);
            List<FunctionsData> functions = new List<FunctionsData>();
            foreach (string func in data)
            {
                FunctionsData function = new FunctionsData
                {
                    rawData = func
                };
                int idx = func.IndexOf(argumentContainer);
                function.name = func.Substring(0, idx).Trim();
                if(function.name.ToLower().StartsWith(waitSignal))
                {
                    function.name = function.name.Substring(waitSignal.Length);
                    function.waitForCompletion = true;
                }
                else
                    function.waitForCompletion = false;

                function.args = ParseArgs(func.Substring(idx + 1, func.Length - idx - 2));

                functions.Add(function);
            }
            return functions;
        }

        public static string[] ParseArgs(string args)
        {
            List<string> argList = new List<string>();
            StringBuilder currArg = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == '"')
                {
                    inQuotes = !inQuotes;
                    continue;
                }

                if (!inQuotes && args[i] == ' ')
                {
                    if(!string.IsNullOrWhiteSpace(currArg.ToString()))
                        argList.Add(currArg.ToString());
                    currArg.Clear();
                    continue;
                }

                currArg.Append(args[i]);
            }
            if (currArg.Length > 0)
            {
                argList.Add(currArg.ToString());
            }
            currArg.Clear();

            return argList.ToArray();
        }
    }

    [Serializable]
    public struct DialogueData
    {
        public string rawData { get; set; }
        public string dialogue;
        public SegmentSignal segmentSignal;
        public float signalDelay;
        [Serializable]
        public enum SegmentSignal { C, A, WC, WA }

        public bool append => (segmentSignal == SegmentSignal.A || segmentSignal == SegmentSignal.WA);
    }

    [Serializable]
    public struct FunctionsData
    {
        public string rawData { get; set; }
        public string name;
        public string[] args;
        public bool waitForCompletion;
    }
}