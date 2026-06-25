using System.Collections.Generic;
using UnityEngine;

namespace Dialogue
{
    public class Convo
    {
        public List<string> lines = new List<string>();
        private int progress = 0;

        public Convo(List<string> lines, int progress = 0)
        {
            this.lines = lines;
            this.progress = progress;
        }

        public int GetProgress() => progress;
        public void SetProgress(int val) => progress = val;
        public void IncrementProgress() => progress++;
        public int count => lines.Count;
        public List<string> GetLines() => lines;
        public string CurrLine() => lines[progress];
        public bool ConvoDone() => progress >= lines.Count;
    }
}
