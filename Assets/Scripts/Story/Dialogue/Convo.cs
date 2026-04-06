using System.Collections.Generic;
using UnityEngine;

namespace Dialogue
{
    public class Convo
    {
        private List<DialogueStructure> lines = new List<DialogueStructure>();
        private int progress = 0;

        public Convo(List<string> stringLines, int progress = 0)
        {
            foreach (string line in stringLines)
            {
                lines.Add(DialogueParser.Parse(line));
            }
            this.progress = progress;
        }
        public Convo(List<DialogueStructure> lines, int progress = 0)
        {
            this.lines = lines;
            this.progress = progress;
        }

        public int GetProgress() => progress;
        public void SetProgress(int val) => this.progress = val;
        public void IncrementProgress() => progress++;
        public int count => lines.Count;

        public List<DialogueStructure> GetLines() => lines;
        public DialogueStructure CurrLine() => lines[progress];

        public bool ConvoDone() => progress == lines.Count - 1;
    }
}
