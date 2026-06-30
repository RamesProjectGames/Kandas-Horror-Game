using Dialogue.Functions;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
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

        private DialogicManager dialogicManager;

        public Convo convo => convoQueue.IsEmpty() ? null : convoQueue.top;
        public int convoProgress => convoQueue.IsEmpty() ? -1 : convoQueue.top.GetProgress();
        public ConvoQueue convoQueue;

        public ConvoManager(TextArchitect architect)
        {
            this.architect = architect;
            ds.onUserPrompt += OnUserPrompt;

            dialogicManager = new DialogicManager();
            convoQueue = new ConvoQueue();
        }

        public void Enqueue(Convo convo) => convoQueue.Enqueue(convo);
        public void EnqueuePrio(Convo convo) => convoQueue.EnqueuePrio(convo);

        private void OnUserPrompt()
        {
            userPrompt = true;
        }

        //Starting a new Dialogue
        public void StartConvo(Convo convo)
        {
            if (convo == null)
                return;
            StopConvo();
            Enqueue(convo);

            process = ds.StartCoroutine(RunningConvo());
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
        IEnumerator RunningConvo()
        {
            while(!convoQueue.IsEmpty())
            {
                Convo currConvo = convo;
                string text = string.Empty;
                if(currConvo.ConvoDone())
                {
                    TryAdvanceConvo(currConvo);
                    continue;
                }
                string rawline = currConvo.CurrLine();
                if (string.IsNullOrWhiteSpace(rawline))
                {
                    TryAdvanceConvo(currConvo); 
                    continue;
                }
                DialogueStructure line = DialogueParser.Parse(rawline);

                if (dialogicManager.TryGetLogic(line, out Coroutine logic))
                {
                    yield return logic;
                }
                else
                {
                    if (line.hasDialogue)
                        yield return RunDialogue(line);
                    if (line.hasFunctions)
                        yield return RunFunctions(line);

                    if (line.hasDialogue)
                        yield return WaitForUserInput();
                }
                TryAdvanceConvo(currConvo);
            }
            StopConvo();
        }

        private void TryAdvanceConvo(Convo currConvo)
        {
            currConvo.IncrementProgress();
            if (convo.ConvoDone())
            {
                convoQueue.Dequeue();
            }
        }

        #region Handling Dialogues
        IEnumerator RunDialogue(DialogueStructure line)
        {
            ds.dialogueContainer.ShowDialogue();
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
            for (int i = 0; i < line.dialogue.Count; i++)
            {
                if (i != 0)
                {
                    yield return HandleSegmentSignal(line.dialogue[i]);
                }

                yield return BuildDialogue(line.dialogue[i].dialogue, line.dialogue[i].append);
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