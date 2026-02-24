using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using static UnityEngine.Rendering.HableCurve;

namespace Dialogue
{
    public class DialogueStructure
    {
        public string speaker, functions;
        public List<DialogueSegment> dialogue;
        private const string segmentIdPattern = @"\{[ca]\}|\{w[ca]\s\d*\.?\d*\}";

        public DialogueStructure(string speaker, string dialogue, string functions)
        {
            this.speaker = speaker;
            this.dialogue = RipDialogue(dialogue);
            this.functions = functions;
        }
        public bool hasSpeaker => speaker != string.Empty;
        public bool hasDialogue => dialogue.Count >= 0;
        public bool hasFunctions => functions != string.Empty;

        public List<DialogueSegment> RipDialogue(string rawDialogue)
        {
            List<DialogueSegment> segments = new List<DialogueSegment>();
            MatchCollection matches = Regex.Matches(rawDialogue, segmentIdPattern);

            int lastIdx = 0;
            DialogueSegment segment = new DialogueSegment();
            segment.dialogue = matches.Count == 0 ? rawDialogue : rawDialogue.Substring(0, matches[0].Index);
            segment.segmentSignal = DialogueSegment.SegmentSignal.None;
            segment.signalDelay = 0;
            segments.Add(segment);

            if(matches.Count == 0)
                return segments;
            else
                lastIdx = matches[0].Index;

            for (int i = 0; i < matches.Count; i++)
            {
                Match match = matches[i];
                segment = new DialogueSegment();

                //Signal
                string signalMatch = match.Value;
                signalMatch = signalMatch.Substring(1, match.Length - 2);
                string[] signalSplit = signalMatch.Split(' ');

                segment.segmentSignal = (DialogueSegment.SegmentSignal) Enum.Parse(typeof(DialogueSegment.SegmentSignal), signalSplit[0].ToUpper());

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
    }

    public struct DialogueSegment
    {
        public string dialogue;
        public SegmentSignal segmentSignal;
        public float signalDelay;
        public enum SegmentSignal { None, C, A, WC, WA }

        public bool append => (segmentSignal == DialogueSegment.SegmentSignal.A || segmentSignal == DialogueSegment.SegmentSignal.WA);
    }
}