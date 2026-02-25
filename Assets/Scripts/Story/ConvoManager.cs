using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.HableCurve;

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
                    {
                        yield return RunDialogue(line);
                    }
                    if (line.hasFunctions)
                    {
                        yield return RunFunctions(line);
                    }
                }
            }
        }

        //Playing Convo
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

            foreach (DialogueSegment segment in line.dialogue)
            {
                yield return HandleSegmentSignal(segment);
                yield return BuildDialogue(segment.dialogue, segment.append);
            }

            yield return WaitForUserInput();
        }

        IEnumerator HandleSegmentSignal(DialogueSegment segment)
        {
            Debug.Log(segment.segmentSignal);
            switch (segment.segmentSignal)
            {
                case DialogueSegment.SegmentSignal.C:
                case DialogueSegment.SegmentSignal.A:
                    Debug.Log($"Press to continue");
                    yield return WaitForUserInput();
                    break;
                case DialogueSegment.SegmentSignal.WC:
                case DialogueSegment.SegmentSignal.WA:
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


        IEnumerator WaitForUserInput()
        {
            while (!userPrompt)
            {
                yield return null;
            }
            userPrompt = false;
        }

        //Running Convo Functions
        IEnumerator RunFunctions(DialogueStructure line)
        {
            yield return null;
        }
    }
}