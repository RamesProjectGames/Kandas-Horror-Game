using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Dialogue
{
    public class DialogueStructure
    {
        public string speaker;

        //Dialogue
        public List<DialogueData> dialogue;
        public List<FunctionsData> functions;
        private const string segmentIdPattern = @"\{[ca]\}|\{w[ca]\s\d*\.?\d*\}";

        //Functions
        private const char functionSplit = ',';
        private const char argumentContainer = '(';
        private const string waitSignal = "[w]";

        public DialogueStructure(string speaker, string dialogue, string functions)
        {
            this.speaker = speaker;
            this.dialogue = string.IsNullOrWhiteSpace(dialogue) ? null : RipDialogue(dialogue);
            this.functions = string.IsNullOrWhiteSpace(functions) ? null : RipFunctions(functions);
        }
        public bool hasSpeaker => speaker != string.Empty;
        public bool hasDialogue => dialogue != null;
        public bool hasFunctions => functions != null;

        public List<DialogueData> RipDialogue(string rawDialogue)
        {
            List<DialogueData> segments = new List<DialogueData>();
            MatchCollection matches = Regex.Matches(rawDialogue, segmentIdPattern);

            int lastIdx = 0;
            DialogueData segment = new DialogueData();
            segment.dialogue = matches.Count == 0 ? rawDialogue : rawDialogue.Substring(0, matches[0].Index);
            segment.segmentSignal = DialogueData.SegmentSignal.None;
            segment.signalDelay = 0;
            segments.Add(segment);

            if(matches.Count == 0)
                return segments;
            else
                lastIdx = matches[0].Index;

            for (int i = 0; i < matches.Count; i++)
            {
                Match match = matches[i];
                segment = new DialogueData();

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
                FunctionsData function = new FunctionsData();
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

        public string[] ParseArgs(string args)
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

    public struct DialogueData
    {
        public string dialogue;
        public SegmentSignal segmentSignal;
        public float signalDelay;
        public enum SegmentSignal { None, C, A, WC, WA }

        public bool append => (segmentSignal == SegmentSignal.A || segmentSignal == SegmentSignal.WA);
    }

    public struct FunctionsData
    {
        public string name;
        public string[] args;
        public bool waitForCompletion;
    }
}