using Dialogue;
using Dialogue.Functions;
using FMODUnity;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using static UnityEngine.Rendering.GPUSort;

namespace Dialogue.Functions
{
    public abstract class FuncDBExtension
    {
        public static string sfxPath = "event:/SFX/";
        public static string bgmPath = "event:/BGM/";
        public static void Extend(FunctionsDatabase db)
        {

        }

        public static FunctionParams ConvertArgsToParams(string[] args) => new FunctionParams(args);
    }
}

namespace TestingPurposes
{
    public class TestFunction : FuncDBExtension
    {
        new public static void Extend(FunctionsDatabase db)
        {
            db.AddFunction("Teleport", new Action<string[]>(TeleportObject));
            db.AddFunction("Move", new Action<string[]>(MoveObject));
            db.AddFunction("Wait", new Func<string, IEnumerator>(Wait));
            db.AddFunction("Poultry", new Action(PrintPoultry));
            db.AddFunction("Objective", new Action<string>(CompleteObjective));
            db.AddFunction("PlaySFX", new Action<string[]>(PlaySFX));
            db.AddFunction("StopSFX", new Action(StopSFX));
            db.AddFunction("PlayBGM", new Action<string[]>(PlayBGM));
            db.AddFunction("ShowDialogue", new Func<string, IEnumerator>(ShowDialogue));
            db.AddFunction("HideDialogue", new Func<string, IEnumerator>(HideDialogue));
            db.AddFunction("PlayCutsceneVideo", new Action<string[]>(PlayCutscene));
            db.AddFunction("InspectFragment", new Action<string[]>(InspectFragment));
            db.AddFunction("StartQuiz", new Action(StartQuiz));
            db.AddFunction("EndQuiz", new Action(EndQuiz));
            db.AddFunction("StopDialogue", new Action(StopDialogue));
            db.AddFunction("NextDialogue", new Action(NextDialogue));
            db.AddFunction("PlayerFaceFront", new Action(PlayerFaceFront));
            db.AddFunction("AllowNPCMovement", new Action<string>(AllowNPCMovement));
            db.AddFunction("Fadein", new Func<string, IEnumerator>(FadeIn));
            db.AddFunction("fadeout", new Func<string, IEnumerator>(FadeOut));
            db.AddFunction("StopWithoutObj", new Action<string>(ConditionalStopDialogue));
            //db.AddFunction("SetUpEnding", new Action());

        }


        #region Move Objects
        private static void TeleportObject(string[] args)
        {
            float x, y, z;
            GameObject movableObject = GameObject.Find(args[0]);
            var funcParams = ConvertArgsToParams(args);
            funcParams.TryGetValue(new string[] { "^x" }, out x, defaultValue: movableObject.transform.position.x);
            funcParams.TryGetValue(new string[] { "^y" }, out y, defaultValue: movableObject.transform.position.y);
            funcParams.TryGetValue(new string[] { "^z" }, out z, defaultValue: movableObject.transform.position.z);
            movableObject.TryGetComponent(out MovableObjects moveScript);
            if (moveScript != null)
            {
                moveScript.StartCoroutine(moveScript.Teleport(new Vector3(x, y, z)));
            }
            else
            {
                movableObject.transform.position = new Vector3(x, y, z);
            }
        }

        private static void MoveObject(string[] args)
        {
            float x, y, z;
            GameObject movableObject = GameObject.Find(args[0]);
            FunctionParams funcParams = ConvertArgsToParams(args);
            funcParams.TryGetValue(new string[] { "^x" }, out x, defaultValue: movableObject.transform.position.x);
            funcParams.TryGetValue(new string[] { "^y" }, out y, defaultValue: movableObject.transform.position.y);
            funcParams.TryGetValue(new string[] { "^z" }, out z, defaultValue: movableObject.transform.position.z);
            movableObject.TryGetComponent(out MovableObjects moveScript);
            if (moveScript != null)
            {
                moveScript.StartCoroutine(moveScript.Move(new Vector3(x, y, z)));
            }
        }

        private static void PlayerFaceFront()
        {
            GameObject.Find("Player").GetComponent<PlayerController>().FaceFront();
        }

        private static void AllowNPCMovement(string arg)
        {
            bool allow = false;
            bool.TryParse(arg, out allow);
            NpcMovement.allowMovement = allow;
        }
        #endregion

        #region Dialogue Progression
        private static IEnumerator Wait(string arg)
        {
            if (float.TryParse(arg, out float duration))
            {
                yield return new WaitForSeconds(duration);
            }
        }

        private static IEnumerator FadeIn(string arg)
        {
            if (float.TryParse(arg, out float duration))
            {
                yield return DialogueSystem.Instance.StartCoroutine(DialogueSystem.Instance.FadeFromBlack(duration));
            }
        }

        private static IEnumerator FadeOut(string arg)
        {
            if (float.TryParse(arg, out float duration))
            {
                yield return DialogueSystem.Instance.StartCoroutine(DialogueSystem.Instance.FadeToBlack(duration));
            }
        }

        private static void CompleteObjective(string arg)
        {
            ObjectiveManager.Instance.CompleteObjective(arg);
        }

        private static void ConditionalStopDialogue(string arg)
        {
            if(!ObjectiveManager.Instance.objectiveDatas.Find(x => x.Name == arg).IsCompleted)
            {
                StopDialogue();
            }
        }

        private static IEnumerator ShowDialogue(string arg)
        {
            yield return DialogueSystem.Instance.dialogueContainer.ShowDialogue();
        }

        private static IEnumerator HideDialogue(string arg)
        {
            yield return DialogueSystem.Instance.dialogueContainer.HideDialogue();
        }

        private static void StopDialogue()
        {
            DialogueSystem.Instance.StopDialogue();
        }

        private static void NextDialogue()
        {
            DialogueSystem.Instance.OnUserPrompt();
        }
        #endregion

        #region Audio
        private static void PlaySFX(string[] args)
        {
            var funcParams = ConvertArgsToParams(args);
            EventReference sfx = RuntimeManager.PathToEventReference(sfxPath + args[0]);
            funcParams.TryGetValue(new string[] { "^v" }, out float volume, defaultValue: 1);
            funcParams.TryGetValue(new string[] { "^p" }, out float pitch, defaultValue: 1);

            Vector3 pos = GameObject.Find("Player").transform.position;
            AudioManager.Instance.PlayOneShot(sfx, volume, pitch, pos);
        }

        private static void StopSFX()
        {
            AudioManager.Instance.StopAllSfx();
        }

        private static void PlayBGM(string[] args)
        {
            var funcParams = ConvertArgsToParams(args);
            EventReference sfx = RuntimeManager.PathToEventReference(bgmPath + args[0]);
            funcParams.TryGetValue(new string[] { "^v" }, out float volume, defaultValue: 1);
            funcParams.TryGetValue(new string[] { "^p" }, out float pitch, defaultValue: 1);

            Vector3 pos = GameObject.Find("Player").transform.position;
        }
        #endregion

        #region Dialogue Events
        private static void PlayCutscene(string[] args)
        {

        }

        private static void PrintPoultry()
        {
            Debug.Log("Poultry printed from functions");
        }

        private static void SpawnMannequins()
        {

        }

        private static void StartQuiz()
        {
            UnityEngine.Object.FindAnyObjectByType<QuizChoiceSystem>(FindObjectsInactive.Include).OpenQuiz(true);
        }
        private static void EndQuiz()
        {
            UnityEngine.Object.FindAnyObjectByType<QuizChoiceSystem>(FindObjectsInactive.Include).OpenQuiz(false);
        }

        private static void InspectFragment(string[] args)
        {
            int fragment;
            var funcParams = ConvertArgsToParams(args);
            funcParams.TryGetValue(new string[] { "^f" }, out fragment);
            DialogueSystem.Instance.dialogueContainer.HideDialogue();
            InspectManagerUI.Instance.OnItemSelected(FragmentManager.Instance.GetFragmentGO(fragment));
        }
        #endregion
    }
}
