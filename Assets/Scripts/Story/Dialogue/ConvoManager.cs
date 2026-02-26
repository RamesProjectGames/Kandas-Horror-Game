using Dialogue.Functions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dialogue
{
    public class ConvoManager
    {
        private DialogueSystem ds = DialogueSystem.Instance;

        private Coroutine process = null;
        public bool isRunning => process != null;

        private TextArchitect architect;
        private bool userPrompt = false;

        public ConvoManager(TextArchitect architect)
        {
            this.architect = architect;
            ds.onUserPrompt += OnUserPrompt;
        }

        private void OnUserPrompt()
        {
            userPrompt = true;
        }

        //Starting a new Dialogue
        public void StartConvo(List<string> convo)
        {
            StopConvo();

            process = ds.StartCoroutine(RunningConvo(convo));
        }

        //Stopping a Conversation
        public void StopConvo()
        {
            if(!isRunning)
                return;

            ds.StopCoroutine(process);
            process = null;
        }

        //Convo Parse and Run
        IEnumerator RunningConvo(List<string> convo)
        {
            for (int i = 0; i < convo.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace(convo[i]))
                {
                    DialogueStructure line = DialogueParser.Parse(convo[i]);

                    if (line.hasDialogue)
                        yield return RunDialogue(line);
                    if (line.hasFunctions)
                        yield return RunFunctions(line);

                    if (line.hasDialogue)
                        yield return WaitForUserInput();
                }
            }
        }

        #region Handling Dialogues
        IEnumerator RunDialogue(DialogueStructure line)
        {
            if (line.hasSpeaker)
            {
                if(line.speaker.ToLower() != "narration")
                {
                    ds.ShowSpeakerName(line.speaker);
                }
                else
                {
                    ds.HideSpeakerName();
                }
            }

            Debug.Log(line.dialogue.Count);

            foreach (DialogueData segment in line.dialogue)
            {
                yield return HandleSegmentSignal(segment);
                yield return BuildDialogue(segment.dialogue, segment.append);
            }
        }

        IEnumerator HandleSegmentSignal(DialogueData segment)
        {
            Debug.Log(segment.segmentSignal);
            switch (segment.segmentSignal)
            {
                case DialogueData.SegmentSignal.C:
                case DialogueData.SegmentSignal.A:
                    Debug.Log($"Press to continue");
                    yield return WaitForUserInput();
                    break;
                case DialogueData.SegmentSignal.WC:
                case DialogueData.SegmentSignal.WA:
                    Debug.Log($"Waiting for {segment.signalDelay} seconds");
                    yield return new WaitForSeconds(segment.signalDelay);
                    break;
                default:
                    break;
            }
            yield return null;
        }

        IEnumerator BuildDialogue(string dialogue, bool append = false)
        {
            if(append)
                architect.Append(dialogue);
            else
                architect.Build(dialogue);
            while (architect.isBuilding)
            {
                if (userPrompt)
                {
                    if (architect.speedUp)
                    {
                        architect.ForceComplete();
                    }
                    else
                    {
                        architect.speedUp = true;
                    }
                    userPrompt = false;
                }
                yield return null;
            }
        }
        #endregion

        IEnumerator WaitForUserInput()
        {
            while (!userPrompt)
            {
                yield return null;
            }
            userPrompt = false;
        }

        #region Handling Functions
        IEnumerator RunFunctions(DialogueStructure line)
        {
            List<FunctionsData> functions = line.functions;

            foreach (FunctionsData function in functions)
            {
                if(function.waitForCompletion)
                    yield return DialogueFunctionManager.Instance.Execute(function.name, function.args);
                else
                    DialogueFunctionManager.Instance.Execute(function.name, function.args);
            }

            yield return null;
        }
        #endregion
    }
}