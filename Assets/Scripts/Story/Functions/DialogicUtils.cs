using Dialogue.Functions;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Dialogue.LogicLines
{
    public class DialogicUtils
    {
        public static class Encapsulation
        {
            public struct EncapsulatedData
            {
                public bool isNull => lines == null;
                public List<string> lines;
                public int startingIdx;
                public int endingIdx;
            }

            private const char ENC_START = '{';
            private const char ENC_END = '}';

            public static EncapsulatedData RipEncapsulatedData(Convo currConvo, int startIdx, bool RipHeaderAndEncapsulators = false)
            {
                int encDepth = 0;

                EncapsulatedData data = new EncapsulatedData { lines = new List<string>(), startingIdx = startIdx, endingIdx = 0};
                for (int i = startIdx; i < currConvo.count; i++)
                {
                    DialogueStructure line = currConvo.GetLines()[i];

                    if(!string.IsNullOrWhiteSpace(line.GetRawLine()) && (RipHeaderAndEncapsulators || (encDepth > 0 && !IsEncapsulationEnd(line.GetRawLine()))))
                        data.lines.Add(line.GetRawLine());

                    if(IsEncapsulationStart(line.GetRawLine()))
                    {
                        encDepth++;
                        continue;
                    }

                    if (IsEncapsulationEnd(line.GetRawLine()))
                    {
                        encDepth--;
                        if(encDepth == 0)
                        {
                            data.endingIdx = i;
                            break;
                        }
                    }
                }
                return data;
            }

            private static bool IsEncapsulationStart(string line) => line.Trim().StartsWith(ENC_START);
            private static bool IsEncapsulationEnd(string line) => line.Trim().StartsWith(ENC_END);
        }
    }
}
