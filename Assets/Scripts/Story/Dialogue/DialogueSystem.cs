using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static Dialogue.TextArchitect;

namespace Dialogue
{
    public class DialogueSystem : MonoBehaviour
    {
        public DialogueContainer dialogueContainer = new DialogueContainer();
        private ConvoManager convoManager;
        public BuildMethod buildMethod = BuildMethod.typewriter;
        public bool isRunningConvo => convoManager.isRunning;
        public TextArchitect architect { get; private set; }

        [SerializeField] private PlayerInput input;

        //Dialogue System Trigger Events for Player Input (and others)
        public delegate void DialogueSystemEvent();
        public event DialogueSystemEvent onUserPrompt;
        public DialoguePrompt dialoguePrompt;

        public static DialogueSystem Instance { get; private set; }

        //Initialize System
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                Init();
            }
            else
                Destroy(gameObject);
        }

        bool initialized = false;

        //Initialize Text Architect and Convo Manager
        private void Init()
        {
            if(initialized) return;

            architect = new TextArchitect(dialogueContainer.dialogueText);
            architect.buildMethod = buildMethod;
            architect.speed = .5f;

            convoManager = new ConvoManager(architect);

            input.actions["Next"].performed += OnUserPrompt;
        }

        private void Update()
        {

            if (!isRunningConvo)
                return;
            if (buildMethod != architect.buildMethod)
            {
                architect.buildMethod = buildMethod;
                architect.StopBuildingText();
            }
            //if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Z))
            //{
            //    OnUserPrompt();
            //}
        }

        #region Conversation

        // Wrapper for Show/Hide Speaker Name
        public void ShowSpeakerName(string speakerName = "") => dialogueContainer.ShowName(speakerName);

        public void HideSpeakerName() => dialogueContainer.HideName();

        //Say for one-liner (not rlly used)
        public void Say(string speaker, string dialogue)
        {
            List<string> convo = new List<string> { $"{speaker} \"{dialogue}\""};
            Say(convo);
        }

        //Say for conversations
        public void Say(List<string> dialogue)
        {
            convoManager.StartConvo(dialogue);
        }
        #endregion

        #region Triggers
        public void OpenDialogue(string assetName)
        {
            if (isRunningConvo)
                return;
            List<string> lines = FileReader.ReadAsset(assetName);
            Say(lines);
        }
        //public void OnUserPrompt()
        //{
        //    onUserPrompt?.Invoke();
        //}
        public void OnUserPrompt(InputAction.CallbackContext ctx)
        {
            if(isRunningConvo && !SettingManager.Instance.isPaused)
                onUserPrompt?.Invoke();
        }
        #endregion
    }
}