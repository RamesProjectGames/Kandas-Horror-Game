using Dialogue;
using Dialogue.Functions;
using FMODUnity;
using System;
using System.Collections;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;
using static UnityEngine.Rendering.GPUSort;

namespace Dialogue.Functions
{
    public abstract class FuncDBExtension
    {
        public static string sfxPath = "event:/SFX/";
        public static string bgmPath = "event:/BGM/";
        public static string ambiencePath = "event:/Ambience/";
        public static string voicePath = "event:/Voice/";
        public static void Extend(FunctionsDatabase db)
        {

        }

        public static FunctionParams ConvertArgsToParams(string[] args) => new FunctionParams(args);
    }
}

namespace TestingPurposes
{
    public class DialogueEvents : FuncDBExtension
    {
    #region Variables
        public static int DoorAttempts = 0;
    #endregion
        new public static void Extend(FunctionsDatabase db)
        {
            #region Movement
            db.AddFunction("Teleport", new Func<string[], IEnumerator>(TeleportObject));
            db.AddFunction("TeleportToWaypoint", new Action<string[]>(TeleportToWaypoint));
            db.AddFunction("Rotate", new Func<string[], IEnumerator>(RotateObject));
            db.AddFunction("Move", new Func<string[], IEnumerator>(MoveObject));
            db.AddFunction("PlayerFaceFront", new Action(PlayerFaceFront));
            db.AddFunction("AllowNPCMovement", new Action<string>(AllowNPCMovement));
            db.AddFunction("Fadein", new Func<string, IEnumerator>(FadeIn));
            db.AddFunction("fadeout", new Func<string, IEnumerator>(FadeOut));
            db.AddFunction("SwitchCam", new Action<string>(SwitchCamera));
            db.AddFunction("Despawn", new Action<string>(Despawn));
            db.AddFunction("MovePrep", new Action(MovePrep));
            #endregion
            #region Audio
            db.AddFunction("PlaySFX", new Action<string[]>(PlaySFX));
            db.AddFunction("StopSFX", new Action(StopSFX));
            db.AddFunction("PlayBGM", new Action<string[]>(PlayBGM));
            db.AddFunction("PlayAmbience", new Action<string[]>(PlayAmbience));
            db.AddFunction("StopAmbience", new Action(StopAmbience));
            db.AddFunction("PlayVoice", new Action<string[]>(PlayVoice));
            db.AddFunction("StopVoice", new Action(StopVoice));
            #endregion
            #region Dialogue Progression
            db.AddFunction("ShowDialogue", new Func<string, IEnumerator>(ShowDialogue));
            db.AddFunction("HideDialogue", new Func<string, IEnumerator>(HideDialogue));
            db.AddFunction("NextDialogue", new Action(NextDialogue));
            db.AddFunction("StopDialogue", new Action(StopDialogue));
            db.AddFunction("Wait", new Func<string, IEnumerator>(Wait));
            db.AddFunction("Objective", new Action<string>(CompleteObjective));
            db.AddFunction("StopWithoutObj", new Action<string>(ConditionalStopDialogue));
            #endregion
            #region Misc Events
            db.AddFunction("PlayCutsceneVideo", new Action<string[]>(PlayCutscene));
            db.AddFunction("InspectFragment", new Action<string[]>(InspectFragment));
            db.AddFunction("StartQuiz", new Action(StartQuiz));
            db.AddFunction("EndQuiz", new Action(EndQuiz));
            db.AddFunction("PrepLunch", new Func<IEnumerator>(PrepLunch));
            db.AddFunction("EatLunch", new Func<IEnumerator>(EatLunch));
            db.AddFunction("HidePlayerRig", new Action(HidePlayerRig));
            db.AddFunction("ShowPlayerRig", new Action(ShowPlayerRig));
            db.AddFunction("ToggleDoor", new Action(ToggleDoor));
            db.AddFunction("CloseDoor", new Action(CloseDoor));
            db.AddFunction("OpenDoor", new Action(OpenDoor));
            db.AddFunction("ToggleSpecificDoor", new Action<string>(ToggleSpecificDoor));
            db.AddFunction("CloseSpecificDoor", new Action<string>(CloseSpecificDoor));
            db.AddFunction("OpenSpecificDoor", new Action<string>(OpenSpecificDoor));
            db.AddFunction("TryOpenDoor", new Func<IEnumerator>(TryOpenDoor));
            db.AddFunction("PrepChase", new Action(SurvivalHorrorPrep));
            db.AddFunction("SpawnNurseMannequins", new Action(SpawnNurseOfficeMannequin));
            #endregion
        }


        #region Move Objects
        private static IEnumerator TeleportObject(string[] args)
        {
            float x, y, z, rot;
            GameObject movableObject = GameObject.Find(args[0]);
            var funcParams = ConvertArgsToParams(args);
            funcParams.TryGetValue(new string[] { "^x" }, out x, defaultValue: movableObject.transform.position.x);
            funcParams.TryGetValue(new string[] { "^y" }, out y, defaultValue: movableObject.transform.position.y);
            funcParams.TryGetValue(new string[] { "^z" }, out z, defaultValue: movableObject.transform.position.z);
            funcParams.TryGetValue(new string[] { "^r" }, out rot, defaultValue: movableObject.transform.rotation.y);
            movableObject.TryGetComponent(out MovableObjects moveScript);
            if (moveScript != null)
            {
                yield return moveScript.StartCoroutine(moveScript.Teleport(new Vector3(x, y, z)));
            }
            else
            {
                yield return movableObject.transform.position = new Vector3(x, y, z);
            }
        }

        private static void TeleportToWaypoint(string[] args)
        {
            int index;
            GameObject movableObject = GameObject.Find(args[0]);
            var funcParams = ConvertArgsToParams(args);
            funcParams.TryGetValue(new string[] { "^idx" }, out index, defaultValue: 0);
            movableObject.TryGetComponent(out NpcMovement moveScript);
            if (moveScript != null)
            {
                moveScript.TeleportToWaypoint(index);
            }
        }

        private static IEnumerator RotateObject(string[] args)
        {
            float rot;
            GameObject movableObject = GameObject.Find(args[0]);
            var funcParams = ConvertArgsToParams(args);
            funcParams.TryGetValue(new string[] { "^r" }, out rot, defaultValue: movableObject.transform.rotation.y);
            movableObject.TryGetComponent(out MovableObjects moveScript);
            if (moveScript != null)
            {
                yield return moveScript.StartCoroutine(moveScript.Rotate(rot));
            }
            else
            {
                yield return movableObject.transform.rotation = Quaternion.Euler(0, rot, 0);
            }
        }

        private static IEnumerator MoveObject(string[] args)
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
                yield return moveScript.StartCoroutine(moveScript.Move(new Vector3(x, y, z)));
            }
        }

        private static void Despawn(string arg)
        {
            GameObject obj = GameObject.Find(arg);
            if (obj != null)
            {
                UnityEngine.Object.Destroy(obj);
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
            NpcMovement.movementAllowed = allow;
        }

        private static void MovePrep()
        {
            NpcMovement.MovePrep.Invoke();
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

        private static void StopVoice()
        {
            AudioManager.Instance.StopAllVoice();
        }

        private static void StopAmbience()
        {
            AudioManager.Instance.StopAllAmbience();
        }

        private static void PlayBGM(string[] args)
        {
            var funcParams = ConvertArgsToParams(args);
            EventReference bgm = RuntimeManager.PathToEventReference(bgmPath + args[0]);
            funcParams.TryGetValue(new string[] { "^v" }, out float volume, defaultValue: 1);
            funcParams.TryGetValue(new string[] { "^p" }, out float pitch, defaultValue: 1);

            Vector3 pos = GameObject.Find("Player").transform.position;
            AudioManager.Instance.PlayOneShot(bgm, volume, pitch, pos);
        }

        private static void PlayAmbience(string[] args)
        {
            Debug.Log("Playing Ambience");
            var funcParams = ConvertArgsToParams(args);
            EventReference ambience = RuntimeManager.PathToEventReference(ambiencePath + args[0]);
            funcParams.TryGetValue(new string[] { "^v" }, out float volume, defaultValue: 1);
            funcParams.TryGetValue(new string[] { "^p" }, out float pitch, defaultValue: 1);

            Vector3 pos = GameObject.Find("Player").transform.position;
            AudioManager.Instance.PlayOneShot(ambience, volume, pitch, pos);
        }

        private static void PlayVoice(string[] args)
        {
            var funcParams = ConvertArgsToParams(args);
            EventReference voice = RuntimeManager.PathToEventReference(voicePath + args[0]);
            funcParams.TryGetValue(new string[] { "^v" }, out float volume, defaultValue: 1);
            funcParams.TryGetValue(new string[] { "^p" }, out float pitch, defaultValue: 1);

            Vector3 pos = GameObject.Find("Player").transform.position;
            AudioManager.Instance.PlayOneShot(voice, volume, pitch, pos);
        }
        #endregion

        #region Misc Events
        private static void PlayCutscene(string[] args)
        {

        }

        private static void SwitchCamera(string arg)
        {
            CameraManager.SwitchCamera(GameObject.Find(arg).GetComponent<CinemachineCamera>());
        }

        private static IEnumerator PrepLunch()
        {
            yield return DialogueSystem.Instance.StartCoroutine(GameObject.Find("Player").GetComponent<PlayerController>().PrepLunch());
        }

        private static IEnumerator EatLunch()
        {
            if(GameObject.Find("Player").GetComponent<PlayerController>().lunchProgress < 3)
            {
                GameObject.Find("Player").GetComponent<PlayerController>().EatFood();
            }
            else
            {
                GameObject.Find("Player").GetComponent<PlayerController>().EatMeds();
            }
            yield return new WaitForSeconds(1f);
        }

        private static void HidePlayerRig()
        {
            GameObject.Find("Player").GetComponent<PlayerController>().ToggleRig(false);
        }

        private static void ShowPlayerRig()
        {
            GameObject.Find("Player").GetComponent<PlayerController>().ToggleRig(true);
        }

        private static void ToggleDoor()
        {
            UnityEngine.Object.FindAnyObjectByType<PlayerGrabInteraction>().currentItem.GetComponent<Door>().ToggleDoor();
        }

        private static void ToggleSpecificDoor(string arg = "")
        {
            Door door = UnityEngine.Object.FindObjectsByType<ItemInteraction>(sortMode: FindObjectsSortMode.None).First(x => x.gameObject.name == arg).GetComponent<Door>();
            if (door != null)
            {
                door.ToggleDoor();
            }
        }

        private static void CloseDoor()
        {
            UnityEngine.Object.FindAnyObjectByType<PlayerGrabInteraction>().currentItem.GetComponent<Door>().CloseDoor();
        }

        private static void CloseSpecificDoor(string arg = "")
        {
            Door door = UnityEngine.Object.FindObjectsByType<ItemInteraction>(sortMode: FindObjectsSortMode.None).First(x => x.gameObject.name == arg).GetComponent<Door>();
            if(door != null)
            {
                door.CloseDoor();
            }
        }

        private static void OpenDoor()
        {
            UnityEngine.Object.FindAnyObjectByType<PlayerGrabInteraction>().currentItem.GetComponent<Door>().OpenDoor();
        }

        private static void OpenSpecificDoor(string arg = "")
        {
            Door door = UnityEngine.Object.FindObjectsByType<ItemInteraction>(sortMode:FindObjectsSortMode.None).First(x=>x.gameObject.name == arg).GetComponent<Door>();
            if (door != null)
            {
                door.OpenDoor();
            }
        }

        private static IEnumerator TryOpenDoor()
        {
            if(++DoorAttempts < 10)
            {
                EventReference sfx = RuntimeManager.PathToEventReference(sfxPath + "RattleDoor");

                Vector3 pos = GameObject.Find("Player").transform.position;
                AudioManager.Instance.PlayOneShot(sfx, 1, 1, pos);
            }
            else
            {
                OpenDoor();
            }
            yield return null;
        }

        private static void SpawnNurseOfficeMannequin()
        {

        }

        private static void SurvivalHorrorPrep()
        {
            GameObject.Find("Trial Monster").SetActive(true);
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
