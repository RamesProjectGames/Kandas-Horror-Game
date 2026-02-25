using UnityEngine;
using System.Text.RegularExpressions;

namespace Dialogue
{
    public class DialogueParser
    {
        private const string regexString = @"\w*[^\s]\(";

        //Creating Dialogue Structure from separated lines
        public static DialogueStructure Parse(string line)
        {
            var (speaker, dialogue, functions) = SegmentLines(line);

            Debug.Log($"Speaker: {speaker}\nDialogue: {dialogue}\nFunctions: {functions}");

            return new DialogueStructure(speaker, dialogue, functions);
        }

        //Separate Lines into speaker, dialogue, and functions
        private static (string, string, string) SegmentLines(string line)
        {
            string speaker = "", dialogue = "", functions = "";

            // Dialogue Segment
            int dialogueStart = -1;
            int dialogueEnd = -1;
            bool escapedChar = false;

            for (int i = 0; i < line.Length; i++)
            {
                char curr = line[i];
                if(curr == '\\')
                {
                    escapedChar = true;
                }
                else if(curr == '"' && !escapedChar)
                {
                    if(dialogueStart == -1)
                        dialogueStart = i;
                    else if(dialogueEnd == -1)
                        dialogueEnd = i;
                }
                else
                {
                    escapedChar = false;
                }
            }

            //Functions Segment
            Regex funcRegex = new Regex(regexString);
            Match match = funcRegex.Match(line);
            int functionsStart = -1;
            if (match.Success)
            {
                functionsStart = match.Index;
                if(dialogueStart == -1 && dialogueEnd == -1)
                    return ("", "", line.Trim());
            }

            if (dialogueStart != -1 && dialogueEnd != -1 && (functionsStart == -1 || functionsStart > dialogueEnd))
            {
                speaker = line.Substring(0, dialogueStart).Trim();
                dialogue = line.Substring(dialogueStart+1, dialogueEnd-dialogueStart-1).Replace("\\\"", "\"");
                if(functionsStart != -1)
                    functions = line.Substring(functionsStart).Trim();
            }
            else if (functionsStart != -1 && dialogueStart > functionsStart)
            {
                functions = line.Trim();
            }
            else
            {
                speaker = line.Trim();
            }
            return (speaker, dialogue, functions);
        }
    }
}