using Dialogue;
using Dialogue.Functions;
using FMODUnity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

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
            db.AddFunction("RotateHead", new Func<string[], IEnumerator>(RotateNpcHead));
            db.AddFunction("Move", new Func<string[], IEnumerator>(MoveObject));
            db.AddFunction("MoveToTarget", new Func<string[], IEnumerator>(MoveToTargetWrapper));
            db.AddFunction("MoveBackToOriginal", new Func<string[], IEnumerator>(MoveBackToOriginalWrapper));
            db.AddFunction("PlayerFaceFront", new Action(PlayerFaceFront));
            db.AddFunction("AllowNPCMovement", new Action<string>(AllowNPCMovement));
            db.AddFunction("Fadein", new Func<string, IEnumerator>(FadeIn));
            db.AddFunction("fadeout", new Func<string, IEnumerator>(FadeOut));
            db.AddFunction("SwitchCam", new Action<string>(SwitchCamera));
            db.AddFunction("Despawn", new Action<string>(Despawn));
            db.AddFunction("MovePrep", new Action(MovePrep));
            //db.AddFunction("WaitForInput", new Action(WaitForInput));
            #endregion
            #region Audio
            db.AddFunction("PlaySFX", new Action<string[]>(PlaySFX));
            db.AddFunction("StopSFX", new Action(StopSFX));
            db.AddFunction("PlayBGM", new Action<string[]>(PlayBGM));
            db.AddFunction("PlayAmbience", new Action<string[]>(PlayAmbience));
            db.AddFunction("StopAmbience", new Action(StopAmbience));
            db.AddFunction("PlayVoice", new Action<string[]>(PlayVoice));
            db.AddFunction("StopVoice", new Action(StopVoice));
            db.AddFunction("StopAudioByName", new Action<string[]>(StopAudioByName));
            #endregion
            #region Dialogue Progression
            db.AddFunction("ShowDialogue", new Func<string, IEnumerator>(ShowDialogue));
            db.AddFunction("HideDialogue", new Func<string, IEnumerator>(HideDialogue));
            db.AddFunction("NextDialogue", new Action(NextDialogue));
            db.AddFunction("StopDialogue", new Action(StopDialogue));
            db.AddFunction("Wait", new Func<string, IEnumerator>(Wait));
            db.AddFunction("Objective", new Action<string>(CompleteObjective));
            db.AddFunction("StopWithoutObj", new Action<string>(ConditionalStopDialogue));
            db.AddFunction("DemoComplete", new Action(DemoComplete));
            #endregion
            #region Misc Events
            db.AddFunction("PlayCutsceneVideo", new Action<string[]>(PlayCutscene));
            db.AddFunction("SwitchScene", new Action<string>(SwitchScene));
            db.AddFunction("InspectFragment", new Action<string[]>(InspectFragment));
            db.AddFunction("StartQuiz", new Action(StartQuiz));
            db.AddFunction("EndQuiz", new Action(EndQuiz));
            db.AddFunction("PrepLunch", new Func<IEnumerator>(PrepLunch));
            db.AddFunction("EatLunch", new Func<IEnumerator>(EatLunch));
            db.AddFunction("HidePlayerRig", new Action(HidePlayerRig));
            db.AddFunction("HideNpcRig", new Action<string[]>(HideNpcRig));
            db.AddFunction("ShowObject", new Action<string>(ShowObject));
            db.AddFunction("HideObject", new Action<string>(HideObject));
            db.AddFunction("ShowPlayerRig", new Action(ShowPlayerRig));
            db.AddFunction("ShowNpcRig", new Action<string>(ShowNpcRig));
            db.AddFunction("ToggleDoor", new Action(ToggleDoor));
            db.AddFunction("RattleDoor", new Action(RattleDoor));
            db.AddFunction("CloseDoor", new Action(CloseDoor));
            db.AddFunction("OpenDoor", new Action(OpenDoor));
            db.AddFunction("ToggleSpecificDoor", new Action<string>(ToggleSpecificDoor));
            db.AddFunction("CloseSpecificDoor", new Action<string>(CloseSpecificDoor));
            db.AddFunction("OpenSpecificDoor", new Action<string>(OpenSpecificDoor));
            db.AddFunction("CloseAllDoors", new Action(CloseAllDoors));
            db.AddFunction("OpenAllDoors", new Action(OpenAllDoors));
            db.AddFunction("ResetDoorAttempts", new Action(ResetDoorAttempts));
            db.AddFunction("TryOpenDoor", new Func<IEnumerator>(TryOpenDoor));
            db.AddFunction("ClosingInWalls", new Action(ClosingInWalls));
            db.AddFunction("PrepChase", new Action(SurvivalHorrorPrep));
            db.AddFunction("SpawnNurseMannequins", new Action(SpawnNurseOfficeMannequin));
            db.AddFunction("CrossGate", new Func<IEnumerator>(CrossSchoolGate));
            db.AddFunction("MoveSpecificNPC", new Action<string>(MoveSpecificNPC));
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
            float rot, rotSpd;
            GameObject movableObject = GameObject.Find(args[0]);
            var funcParams = ConvertArgsToParams(args);
            funcParams.TryGetValue(new string[] { "^r" }, out rot, defaultValue: movableObject.transform.rotation.y);
            funcParams.TryGetValue(new string[] { "^r" }, out rotSpd, defaultValue: 5f);
            movableObject.TryGetComponent(out MovableObjects moveScript);
            if (moveScript != null)
            {
                yield return moveScript.StartCoroutine(moveScript.Rotate(rot, rotSpd));
            }
            else
            {
                yield return movableObject.transform.rotation = Quaternion.Euler(0, rot, 0);
            }
        }

        private static IEnumerator RotateNpcHead(string[] args)
        {
            float rot, rotSpd;
            GameObject npcObject = GameObject.Find(args[0]);
            var funcParams = ConvertArgsToParams(args);
            funcParams.TryGetValue(new string[] { "^r" }, out rot, defaultValue: npcObject.transform.rotation.y);
            funcParams.TryGetValue(new string[] { "^r" }, out rotSpd, defaultValue: 5f);
            npcObject.TryGetComponent(out NpcMovement npcController);
            if (npcController != null)
            {
                yield return npcController.StartCoroutine(npcController.RotateHead(rot, rotSpd));
            }
        }

        private static IEnumerator MoveObject(string[] args)
        {
            float x, y, z;
            GameObject movableObject = GameObject.Find(args[0]);
            FunctionParams funcParams = ConvertArgsToParams(args);
            var targetTransform = ResolvePosition(funcParams);
            funcParams.TryGetValue(new string[] { "^s" }, out float speed, defaultValue: 3f);
            funcParams.TryGetValue(new string[] { "^x" }, out x, defaultValue: movableObject.transform.position.x);
            funcParams.TryGetValue(new string[] { "^y" }, out y, defaultValue: movableObject.transform.position.y);
            funcParams.TryGetValue(new string[] { "^z" }, out z, defaultValue: movableObject.transform.position.z);
            movableObject.TryGetComponent(out MovableObjects moveScript);
            if (moveScript != null)
            {
                if(targetTransform != Vector3.zero)
                {
                    yield return moveScript.StartCoroutine(moveScript.Move(targetTransform, speed));
                }
                else
                {
                    yield return moveScript.StartCoroutine(moveScript.Move(new Vector3(x, y, z), speed));
                }
            }
        }
        private static Vector3 ResolvePosition(FunctionParams funcParams)
        {
            if (funcParams.TryGetValue(new string[] { "^go" , "^target", "^obj" }, out string targetObjectName, defaultValue: string.Empty) &&
                !string.IsNullOrWhiteSpace(targetObjectName))
            {
                GameObject targetObject = GameObject.Find(targetObjectName);
                if (targetObject != null)
                {
                    return targetObject.transform.position;
                }
            }

            GameObject fallbackObject = null;
            return fallbackObject != null ? fallbackObject.transform.position : Vector3.zero;
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

        private static IEnumerator MoveToTargetWrapper(string[] args)
        {
            GameObject movableObject = GameObject.Find(args[0]);
            FunctionParams funcParams = ConvertArgsToParams(args);
            funcParams.TryGetValue(new string[] { "^s" }, out float speed, defaultValue: 1f);

            movableObject.TryGetComponent(out MovingObject moveScript);
            if (moveScript != null)
            {
                bool completed = false;
                float progress = 0f;

                // Subscribe to UnityEvents to track progress and completion
                moveScript.onComplete.AddListener(() =>
                {
                    completed = true;
                    // OpenDoor();
                });

                moveScript.onProgress.AddListener((float progressValue) =>
                {
                    progress = progressValue;
                    Debug.Log($"MoveToTarget Progress: {progress:P2}");
                });

                moveScript.MoveToTarget(speed);

                while (!completed)
                    yield return null;

            }
        }

        private static IEnumerator MoveBackToOriginalWrapper(string[] args)
        {
            GameObject movableObject = GameObject.Find(args[0]);
            FunctionParams funcParams = ConvertArgsToParams(args);
            funcParams.TryGetValue(new string[] { "^s" }, out float speed, defaultValue: 1f);

            movableObject.TryGetComponent(out MovingObject moveScript);
            if (moveScript != null)
            {
                bool completed = false;
                float progress = 0f;

                // Subscribe to UnityEvents to track progress and completion
                moveScript.onComplete.AddListener(() =>
                {
                    completed = true;
                    Debug.Log("MoveBackToOriginal completed");
                });

                moveScript.onProgress.AddListener((float progressValue) =>
                {
                    progress = progressValue;
                    Debug.Log($"MoveBackToOriginal Progress: {progress:P2}");
                });

                moveScript.MoveBackToOriginal(speed);

                while (!completed)
                    yield return null;

            }
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
                yield return DialogueSystem.Instance.FadeFromBlack(duration);
            }
        }

        private static IEnumerator FadeOut(string arg)
        {
            if (float.TryParse(arg, out float duration))
            {
                yield return DialogueSystem.Instance.FadeToBlack(duration);
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

        private static void DemoComplete()
        {
            UnityEngine.Object.FindAnyObjectByType<SettingsUI>().ShowDemoEnd();
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
        private static Vector3 ResolveAudioPosition(FunctionParams funcParams, string fallbackObjectName = "Player")
        {
            if (funcParams.TryGetValue(new string[] { "^go" , "^target", "^obj" }, out string targetObjectName, defaultValue: string.Empty) &&
                !string.IsNullOrWhiteSpace(targetObjectName))
            {
                GameObject targetObject = GameObject.Find(targetObjectName);
                if (targetObject != null)
                {
                    return targetObject.transform.position;
                }
            }

            GameObject fallbackObject = GameObject.Find(fallbackObjectName);
            return fallbackObject != null ? fallbackObject.transform.position : Vector3.zero;
        }

        private static void PlaySFX(string[] args)
        {
            var funcParams = ConvertArgsToParams(args);
            EventReference sfx = RuntimeManager.PathToEventReference(sfxPath + args[0]);
            funcParams.TryGetValue(new string[] { "^v" }, out float volume, defaultValue: 1);
            funcParams.TryGetValue(new string[] { "^p" }, out float pitch, defaultValue: 1);

            Vector3 pos = ResolveAudioPosition(funcParams);
            AudioManager.Instance.PlayOneShot3D(sfx, volume, pitch, pos);
        }

        private static void StopSFX()
        {
            Debug.Log("Stopping SFX");
            AudioManager.Instance.StopAllSfx();
        }

        private static void StopVoice()
        {
            Debug.Log("Stopping Voice");
            AudioManager.Instance.StopAllVoice();
        }

        private static void StopAmbience()
        {
            Debug.Log("Stopping Ambience");
            AudioManager.Instance.StopAllAmbience();
        }

        private static IEnumerable<string> ResolveAudioStopPaths(string eventName, string category)
        {
            if (string.IsNullOrWhiteSpace(eventName))
            {
                return Enumerable.Empty<string>();
            }

            if (eventName.StartsWith("event:/", StringComparison.OrdinalIgnoreCase))
            {
                return new[] { eventName };
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                switch (category.Trim().ToLowerInvariant())
                {
                    case "sfx":
                        return new[] { sfxPath + eventName };
                    case "bgm":
                    case "music":
                        return new[] { bgmPath + eventName };
                    case "ambience":
                    case "ambient":
                        return new[] { ambiencePath + eventName };
                    case "voice":
                        return new[] { voicePath + eventName };
                }
            }

            return new[]
            {
                sfxPath + eventName,
                bgmPath + eventName,
                ambiencePath + eventName,
                voicePath + eventName
            };
        }

        private static void StopAudioByName(string[] args)
        {
            if (args == null || args.Length == 0 || string.IsNullOrWhiteSpace(args[0]))
            {
                return;
            }

            var funcParams = ConvertArgsToParams(args);
            string eventName = args[0];
            funcParams.TryGetValue(new string[] { "^cat", "^type" }, out string category, defaultValue: string.Empty);
            funcParams.TryGetValue(new string[] { "^mode", "^m" }, out string stopModeArg, defaultValue: "fade");
            FMOD.Studio.STOP_MODE stopMode = stopModeArg.Equals("immediate", StringComparison.OrdinalIgnoreCase)
                ? FMOD.Studio.STOP_MODE.IMMEDIATE
                : FMOD.Studio.STOP_MODE.ALLOWFADEOUT;

            foreach (string path in ResolveAudioStopPaths(eventName, category))
            {
                EventReference sound = RuntimeManager.PathToEventReference(path);
                AudioManager.Instance.StopSoundInstance(sound, stopMode);
            }
        }

        private static void PlayBGM(string[] args)
        {
            var funcParams = ConvertArgsToParams(args);
            EventReference bgm = RuntimeManager.PathToEventReference(bgmPath + args[0]);
            funcParams.TryGetValue(new string[] { "^v" }, out float volume, defaultValue: 1);
            funcParams.TryGetValue(new string[] { "^p" }, out float pitch, defaultValue: 1);

            Vector3 pos = ResolveAudioPosition(funcParams);
            AudioManager.Instance.PlayOneShot3D(bgm, volume, pitch, pos);
        }

        private static void PlayAmbience(string[] args)
        {
            Debug.Log("Playing Ambience");
            var funcParams = ConvertArgsToParams(args);
            EventReference ambience = RuntimeManager.PathToEventReference(ambiencePath + args[0]);
            funcParams.TryGetValue(new string[] { "^v" }, out float volume, defaultValue: 1);
            funcParams.TryGetValue(new string[] { "^p" }, out float pitch, defaultValue: 1);

            Vector3 pos = ResolveAudioPosition(funcParams);
            AudioManager.Instance.PlayOneShot3D(ambience, volume, pitch, pos);
        }

        private static void PlayVoice(string[] args)
        {
            var funcParams = ConvertArgsToParams(args);
            EventReference voice = RuntimeManager.PathToEventReference(voicePath + args[0]);
            funcParams.TryGetValue(new string[] { "^v" }, out float volume, defaultValue: 1);
            funcParams.TryGetValue(new string[] { "^p" }, out float pitch, defaultValue: 1);

            Vector3 pos = ResolveAudioPosition(funcParams);
            AudioManager.Instance.PlayOneShot3D(voice, volume, pitch, pos);
        }
        #endregion

        #region Misc Events
        private static void PlayCutscene(string[] args)
        {

        }

        private static void SwitchScene(string arg)
        {
            if (ChapterDataManager.Instance == null)
                return;

            SceneField nextScene = ChapterDataManager.Instance.GetSceneByName(arg);
            SceneField reloadScene = AsyncSceneLoader.Instance.persistentScene;
            Debug.LogWarning(nextScene.SceneName);
            if (nextScene == null)
                return;

            List<SceneField> scenesToLoad = new List<SceneField> { nextScene };
            List<SceneField> scenesToUnload = new List<SceneField> { AsyncSceneLoader.Instance.currentChapterScene };

            if (SceneManager.GetActiveScene().name != nextScene.SceneName)
            {
                AsyncSceneLoader.Instance.LoadScenes(scenesToLoad, scenesToUnload, reloadScene);
            }
        }

        private static void MoveSpecificNPC(string arg)
        {
            Debug.Log($"Moving {arg}");            
            UnityEngine.Object.FindObjectsByType<NpcMovement>(sortMode: FindObjectsSortMode.None).ToList().Find(x => x.gameObject.name.Contains(arg)).GetComponent<NpcMovement>().agent.enabled = true;
            foreach(NpcMovement npc in UnityEngine.Object.FindObjectsByType<NpcMovement>(sortMode: FindObjectsSortMode.None).ToList().FindAll(x => x.gameObject.name.Contains(arg)).Select(x => x.GetComponent<NpcMovement>()))
            {
                npc.moveMyself = true;
                npc.agent.enabled = true;
                if(npc.point.Length>0) npc.agent.SetDestination(npc.point[npc.idxPoint].position);
            }
            //if(ObjectiveManager.Instance.isCompleted(""))
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
            if(GameObject.Find("Player").GetComponent<PlayerController>().lunchProgress <= 5)
            {
                GameObject.Find("Player").GetComponent<PlayerController>().EatFood();
            }
            if (GameObject.Find("Player").GetComponent<PlayerController>().lunchProgress >= 5)
            {
                DialogueSystem.Instance.convoManager.Enqueue(FileReader.ReadAsset("PostLunch"));
                //GameObject.Find("Player").GetComponent<PlayerController>().EatMeds();
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

        private static void HideNpcRig(string[] args)
        {
            Debug.Log("Hiding NPC Rig");
            var funcParams = ConvertArgsToParams(args);
            funcParams.TryGetValue(new string[] { "^t" }, out float delay, defaultValue: 0);
            NpcMovement chara = UnityEngine.Object.FindObjectsByType<NpcMovement>(sortMode: FindObjectsSortMode.None).ToList().Find(x => x.gameObject.name == args[0]);
            if(chara != null)
            {
                chara.StartCoroutine(chara.ToggleRig(false, delay));
            }
        }
        private static void ShowObject(string arg)
        {
            UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None).ToList().Find(x => x.gameObject.name == arg).SetActive(true);
        }

        private static void HideObject(string arg)
        {
            GameObject.Find(arg).SetActive(false);
        }

        private static void ShowNpcRig(string arg)
        {
            NpcMovement chara = UnityEngine.Object.FindObjectsByType<NpcMovement>(sortMode: FindObjectsSortMode.None).ToList().Find(x => x.gameObject.name == arg);
            if (chara != null)
            {
                chara.StartCoroutine(chara.ToggleRig(true, 0));
            }
        }

        private static void ToggleDoor()
        {
            UnityEngine.Object.FindAnyObjectByType<PlayerGrabInteraction>().currentItem.GetComponent<Door>().ToggleDoor();
        }

        private static void RattleDoor()
        {
            EventReference sfx = RuntimeManager.PathToEventReference(sfxPath + "RattleDoor");

            Vector3 pos = GameObject.Find("Player").transform.position;
            AudioManager.Instance.PlayOneShot3D(sfx, 1, 1, pos);
        }

        private static void ToggleSpecificDoor(string arg = "")
        {
            IEnumerable<Door> doors = UnityEngine.Object.FindObjectsByType<ItemInteraction>(sortMode: FindObjectsSortMode.None).ToList().FindAll(x => x.gameObject.name == arg).Select(x => x.GetComponent<Door>());
            foreach (Door door in doors)
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
            IEnumerable<Door> doors = UnityEngine.Object.FindObjectsByType<ItemInteraction>(sortMode: FindObjectsSortMode.None).ToList().FindAll(x => x.gameObject.name == arg).Select(x => x.GetComponent<Door>());
            foreach(Door door in doors)
            {
                door.CloseDoor();
            }
        }

        private static void CloseAllDoors()
        {
            IEnumerable<Door> doors = UnityEngine.Object.FindObjectsByType<Door>(sortMode: FindObjectsSortMode.None);
            foreach (Door door in doors)
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
            IEnumerable<Door> doors = UnityEngine.Object.FindObjectsByType<ItemInteraction>(sortMode: FindObjectsSortMode.None).ToList().FindAll(x => x.gameObject.name == arg).Select(x => x.GetComponent<Door>());
            foreach (Door door in doors)
            {
                door.OpenDoor();
            }
        }
        private static void OpenAllDoors()
        {
            IEnumerable<Door> doors = UnityEngine.Object.FindObjectsByType<Door>(sortMode: FindObjectsSortMode.None);
            foreach (Door door in doors)
            {
                door.OpenDoor();
            }
        }
        public static void ResetDoorAttempts()
        {
            DoorAttempts = 0;
        }
        private static IEnumerator TryOpenDoor()
        {
            if(DoorAttempts <= 9)
            {
                DoorAttempts++;
                EventReference sfx = RuntimeManager.PathToEventReference(sfxPath + "RattleDoor");

                Vector3 pos = GameObject.Find("Player").transform.position;
                AudioManager.Instance.PlayOneShot3D(sfx, 1, 1, pos);
            }
            yield return null;
        }
        private static void ClosingInWalls()
        {
            if(DoorAttempts == 1)
            {
                DialogueSystem.Instance.StartCoroutine(MoveToTargetWrapper(new string[] { "WallSamping-KamarMandi (1)", "^s", "5" }));
                DialogueSystem.Instance.StartCoroutine(MoveToTargetWrapper(new string[] { "WallSamping-KamarMandi (2)", "^s", "5" }));
            }
        }

        private static void SpawnNurseOfficeMannequin()
        {

        }

        private static void SurvivalHorrorPrep()
        {
            UnityEngine.Object.FindAnyObjectByType<EnemyMovement>(FindObjectsInactive.Include).gameObject.SetActive(true);
        }

        private static void StartQuiz()
        {
            UnityEngine.Object.FindAnyObjectByType<QuizChoiceSystem>(FindObjectsInactive.Include).OpenQuiz(true);
        }
        private static void EndQuiz()
        {
            UnityEngine.Object.FindAnyObjectByType<QuizChoiceSystem>(FindObjectsInactive.Include).OpenQuiz(false);
        }

        private static IEnumerator CrossSchoolGate()
        {
            Waypoint startPos, endPos;
            if (Vector3.Distance(GameObject.Find("Player").transform.position, GameObject.Find("GateIn").GetComponent<Waypoint>().position) <
               Vector3.Distance(GameObject.Find("Player").transform.position, GameObject.Find("GateOut").GetComponent<Waypoint>().position))
            {
                startPos = GameObject.Find("GateIn").GetComponent<Waypoint>();
                endPos = GameObject.Find("GateOut").GetComponent<Waypoint>();
            }
            else
            {
                endPos = GameObject.Find("GateIn").GetComponent<Waypoint>();
                startPos = GameObject.Find("GateOut").GetComponent<Waypoint>();
            }
            GameObject.Find("GateCam").transform.rotation = startPos.transform.rotation;
            GameObject.Find("GateCam").GetComponent<CinemachineCamera>().Follow = endPos.transform;
            GameObject.Find("GateCam").transform.position = startPos.position;
            SwitchCamera("GateCam");
            while (Vector3.Distance(CameraManager.currentActiveCamera.transform.position, endPos.position) > .1f)
            {
                CameraManager.currentActiveCamera.transform.position = Vector3.Lerp(CameraManager.currentActiveCamera.transform.position, endPos.position, Time.deltaTime * 2.0f);
                yield return null;
            }
            yield return TeleportObject(new string[] { "Player", "^x", endPos.position.x.ToString(), "^z", endPos.position.z.ToString() });
            RotateObject(new string[] { "Player", "^r", startPos.transform.rotation.y.ToString() });
            SwitchCamera("Player Camera");
            yield return new WaitForSeconds(1);
            GameObject.Find("GateCam").GetComponent<CinemachineCamera>().Follow = startPos.transform;
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
