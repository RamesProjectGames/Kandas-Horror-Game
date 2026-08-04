using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.ProBuilder.Shapes;
using static Dialogue.TextArchitect;

namespace Dialogue
{
    public class DialogueSystem : MonoBehaviour
    {
        public DialogueContainer dialogueContainer = new DialogueContainer();
        public ConvoManager convoManager { get; private set; }
        public BuildMethod buildMethod = BuildMethod.typewriter;
        public bool isRunningConvo => convoManager.isRunning;
        public bool cameraControl;
        public TextArchitect architect { get; private set; }

        [SerializeField] private InputActionReference nextInput, enqDebugInput;
        public Coroutine screenCo;

        //Dialogue System Trigger Events for Player Input (and others)
        public delegate void DialogueSystemEvent();
        public event DialogueSystemEvent onUserPrompt;
        public DialoguePrompt dialoguePrompt;

        public static DialogueSystem Instance { get; private set; }

        //Initialize System
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(Instance.gameObject);
            }
            Instance = this;
            Init();
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

            nextInput.action.performed += OnUserPrompt;
            enqDebugInput.action.performed += EnqueueDebug;
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
        }

        #region Conversation

        // Wrapper for Show/Hide Speaker Name
        public void ShowSpeakerName(string speakerName = "") => dialogueContainer.ShowName(speakerName);

        public void HideSpeakerName() => dialogueContainer.HideName();

        //Say for one-liner (not rlly used)
        public void Say(string speaker, string dialogue)
        {
            List<string> convoString = new List<string> { $"{speaker} \"{dialogue}\""};
            Convo convo = new Convo(convoString);
            convoManager.StartConvo(convo);
        }
        #endregion

        #region Triggers

        public IEnumerator FadeToBlack(float duration)
        {
            while (screenCo != null)
            {
                yield return null;
            }
            screenCo = StartCoroutine(dialogueContainer.FadeToBlack(duration));
        }

        public IEnumerator FadeFromBlack(float duration)
        {
            while (screenCo != null)
            {
                yield return null;
            }
            screenCo = StartCoroutine(dialogueContainer.FadeFromBlack(duration));
        }

        public void OpenDialogue(string assetName, bool allowCam = false)
        {
            cameraControl = allowCam;

            if (string.IsNullOrWhiteSpace(assetName))
            {
                Debug.LogWarning("DialogueSystem.OpenDialogue received an empty asset name.");
                return;
            }

            if (isRunningConvo)
            {
                Debug.LogWarning($"DialogueSystem.OpenDialogue skipped because a conversation is already running: {assetName}");
                return;
            }

            Convo convo = FileReader.ReadAsset(assetName);
            if (convo == null)
            {
                Debug.LogError($"DialogueSystem.OpenDialogue could not load dialogue asset: {assetName}");
                return;
            }

            Debug.Log($"DialogueSystem.OpenDialogue starting: {assetName}");
            convoManager.StartConvo(convo);
        }
        public void StopDialogue()
        {
            if (!isRunningConvo)
                return;
            convoManager.convoQueue.Dequeue();
            convoManager.StopConvo();
        }

        public void EnqueueDebug(InputAction.CallbackContext ctx)
        {
            convoManager.EnqueuePrio(FileReader.ReadAsset("PostLunch"));
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