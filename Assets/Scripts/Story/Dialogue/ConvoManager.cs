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
            if(convo == null)
                return;
            StopConvo();

            ds.dialogueContainer.ShowDialogue();
            process = ds.StartCoroutine(RunningConvo(convo));
        }
        public void StartConvo(List<DialogueStructure> convo)
        {
            StopConvo();

            ds.dialogueContainer.ShowDialogue();
            process = ds.StartCoroutine(RunningConvo(convo));
        }

        //Stopping a Conversation
        public void StopConvo()
        {
            if(!isRunning)
                return;

            ds.StopCoroutine(process);
            process = null;
            ds.dialogueContainer.HideDialogue();
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
                AudioManager.Instance.StopAllSfx();
            }

            StopConvo();
        }

        //Convo Parse and Run
        IEnumerator RunningConvo(List<DialogueStructure> convo)
        {
            for (int i = 0; i < convo.Count; i++)
            {
                if (convo[i].hasDialogue)
                    yield return RunDialogue(convo[i]);
                if (convo[i].hasFunctions)
                    yield return RunFunctions(convo[i]);

                if (convo[i].hasDialogue)
                    yield return WaitForUserInput();
                AudioManager.Instance.StopAllSfx();
            }

            StopConvo();
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

            foreach (DialogueData segment in line.dialogue)
            {
                yield return HandleSegmentSignal(segment);
                yield return BuildDialogue(segment.dialogue, segment.append);
            }
        }

        IEnumerator HandleSegmentSignal(DialogueData segment)
        {
            switch (segment.segmentSignal)
            {
                case DialogueData.SegmentSignal.C:
                case DialogueData.SegmentSignal.A:
                    yield return WaitForUserInput();
                    break;
                case DialogueData.SegmentSignal.WC:
                case DialogueData.SegmentSignal.WA:
                    yield return WaitForDelayOrInput(segment.signalDelay);
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
            ds.dialoguePrompt.Show();

            while (!userPrompt)
            {
                yield return null;
            }
            userPrompt = false;
            ds.dialoguePrompt.Hide();
        }

        IEnumerator WaitForDelayOrInput(float duration)
        {
            float start = Time.time;
            while (!userPrompt && Time.time<start+duration)
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
                if(function.waitForCompletion || function.name == "wait")
                    yield return DialogueFunctionManager.Instance.Execute(function.name, function.args);
                else
                    DialogueFunctionManager.Instance.Execute(function.name, function.args);
            }

            yield return null;
        }
        #endregion
    }
}