using System.Collections.Generic;
using System.Linq;
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

        [SerializeField] private InputActionReference nextInput;
        public List<MiniConvo> allMiniConvos;

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

            allMiniConvos = Resources.LoadAll<MiniConvo>("").ToList();

            architect = new TextArchitect(dialogueContainer.dialogueText);
            architect.buildMethod = buildMethod;
            architect.speed = .5f;

            convoManager = new ConvoManager(architect);

            nextInput.action.performed += OnUserPrompt;
        }

        private void Update()
        {
            if (Application.isPlaying && (SettingManager.Instance.isPaused || !isRunningConvo) && !dialogueContainer.active)
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
            convoManager.StartConvo(convo);
        }
        #endregion

        #region Triggers
        public void OpenDialogue(string assetName)
        {
            if (isRunningConvo)
                return;
            if(assetName.StartsWith("Mini_"))
                convoManager.StartConvo(allMiniConvos.Find(x => x.convoName == assetName).dialogues);
            else
                convoManager.StartConvo(FileReader.ReadAsset(assetName));
        }
        public void StopDialogue()
        {
            if (!isRunningConvo)
                return;
            convoManager.StopConvo();
        }

        public void OnUserPrompt(InputAction.CallbackContext ctx)
        {
            if (!isRunningConvo || !dialogueContainer.active || SettingManager.Instance.isPaused)
                return;
            onUserPrompt?.Invoke();
        }

        public void OnUserPrompt()
        {
            if (!isRunningConvo || !dialogueContainer.active || SettingManager.Instance.isPaused)
                return;
            onUserPrompt?.Invoke();
        }
        #endregion
    }
}