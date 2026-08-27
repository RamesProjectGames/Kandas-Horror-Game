using Dialogue;
using Dialogue.Functions;
using FMODUnity;
using Kino;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
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
            db.AddFunction("RotateHead", new Func<string[], IEnumerator>(RotateNpcHead));
            db.AddFunction("Move", new Func<string[], IEnumerator>(MoveObject));
            db.AddFunction("MoveToTarget", new Func<string[], IEnumerator>(MoveToTargetWrapper));
            db.AddFunction("MoveBackToOriginal", new Func<string[], IEnumerator>(MoveBackToOriginalWrapper));
            db.AddFunction("PlayerFaceFront", new Action(PlayerFaceFront));
            db.AddFunction("PlayerFaceObject", new Action<string>(PlayerFaceObject));
            db.AddFunction("AllowNPCMovement", new Action<string>(AllowNPCMovement));
            db.AddFunction("SwitchCam", new Action<string>(SwitchCamera));
            db.AddFunction("ChangeCamFoV", new Func<string[], IEnumerator>(ChangeCamFoV));
            db.AddFunction("Despawn", new Action<string>(Despawn));
            db.AddFunction("MovePrep", new Action(MovePrep));
            db.AddFunction("ChangeInteractionText", new Action<string>(ChangeInteractionText));
            db.AddFunction("ChangeActivePlayer", new Action<string>(ChangeActivePlayer));
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
            db.AddFunction("ChangeAudioVolumeProgression", new Action<string[]>(ChangeAudioVolumeProgression));
            db.AddFunction("ChangeAudioPitchProgression", new Action<string[]>(ChangeAudioPitchProgression));
            #endregion
            #region Dialogue Progression
            db.AddFunction("Wait", new Func<string, IEnumerator>(Wait));
            db.AddFunction("FadeIn", new Func<string, IEnumerator>(FadeIn));
            db.AddFunction("FadeOut", new Func<string, IEnumerator>(FadeOut));
            db.AddFunction("ShowDialogue", new Func<IEnumerator>(ShowDialogue));
            db.AddFunction("HideDialogue", new Func<IEnumerator>(HideDialogue));
            db.AddFunction("NextDialogue", new Action(NextDialogue));
            db.AddFunction("StopDialogue", new Action(StopDialogue));
            db.AddFunction("Objective", new Action<string>(CompleteObjective));
            db.AddFunction("StopWithoutObj", new Action<string>(ConditionalStopDialogue));
            db.AddFunction("DemoComplete", new Action(DemoComplete));
            #endregion
            #region Misc Events
            db.AddFunction("PlayVideo", new Action<string[]>(PlayVideo));
            db.AddFunction("PlayCutscene", new Func<string[], IEnumerator>(PlayCutscene));
            db.AddFunction("Glitch", new Func<string, IEnumerator>(Glitch));
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
            db.AddFunction("SpawnNurseMannequins", new Action(SpawnNurseOfficeMannequin));
            db.AddFunction("CrossGate", new Func<IEnumerator>(CrossSchoolGate));
            db.AddFunction("CrossHole", new Func<IEnumerator>(CrossHole));
            db.AddFunction("MoveSpecificNPC", new Action<string[]>(MoveSpecificNPC));
            db.AddFunction("StopSpecificNPC", new Action<string[]>(StopSpecificNPC));
            db.AddFunction("GrabItem", new Action<string[]>(GrabItem));
            db.AddFunction("ThrowItem", new Action<string[]>(ThrowItem));
            db.AddFunction("TransferItem", new Action<string[]>(TransferItem));
            #endregion
        }


        #region Move Objects
        private static IEnumerator TeleportObject(string[] args)
        {
            float x, y, z, rot;
            var movableObjects = UnityEngine.Object.FindObjectsByType<MovableObjects>(sortMode: FindObjectsSortMode.None)
                .ToList()
                .FindAll(x => x.gameObject.name == args[0])
                .Select(x => x.GetComponent<MovableObjects>());
            var funcParams = ConvertArgsToParams(args);
            foreach (MovableObjects moveScript in movableObjects)
            {
                funcParams.TryGetValue(new string[] { "^x" }, out x, defaultValue: moveScript.transform.position.x);
                funcParams.TryGetValue(new string[] { "^y" }, out y, defaultValue: moveScript.transform.position.y);
                funcParams.TryGetValue(new string[] { "^z" }, out z, defaultValue: moveScript.transform.position.z);
                funcParams.TryGetValue(new string[] { "^r" }, out rot, defaultValue: moveScript.transform.rotation.y);
                yield return moveScript.StartCoroutine(moveScript.Teleport(new Vector3(x, y, z)));
            }
            //else
            //{
            //    yield return movableObject.transform.position = new Vector3(x, y, z);
            //}
        }

        private static void TeleportToWaypoint(string[] args)
        {
            int index;
            var movableObjects = UnityEngine.Object.FindObjectsByType<NpcMovement>(sortMode: FindObjectsSortMode.None)
                .ToList()
                .FindAll(x => x.gameObject.name == args[0])
                .Select(x => x.GetComponent<NpcMovement>());
            var funcParams = ConvertArgsToParams(args);
            funcParams.TryGetValue(new string[] { "^idx" }, out index, defaultValue: 0);
            foreach(NpcMovement moveScript in movableObjects)
            {
                moveScript.TeleportToWaypoint(index);
            }
        }

        private static IEnumerator RotateObject(string[] args)
        {
            float rot, rotSpd;
            var movableObjects = UnityEngine.Object.FindObjectsByType<MovableObjects>(sortMode: FindObjectsSortMode.None)
                .ToList()
                .FindAll(x => x.gameObject.name == args[0])
                .Select(x => x.GetComponent<MovableObjects>());
            var funcParams = ConvertArgsToParams(args);
            foreach (MovableObjects moveScript in movableObjects)
            {
                funcParams.TryGetValue(new string[] { "^r" }, out rot, defaultValue: moveScript.transform.rotation.y);
                funcParams.TryGetValue(new string[] { "^rs" }, out rotSpd, defaultValue: 5f);
                yield return moveScript.StartCoroutine(moveScript.Rotate(rot, rotSpd));
            }
            //else
            //{
            //    yield return movableObject.transform.rotation = Quaternion.Euler(0, rot, 0);
            //}
        }

        private static IEnumerator RotateNpcHead(string[] args)
        {
            float rot, rotSpd;
            var movableObjects = UnityEngine.Object.FindObjectsByType<NpcMovement>(sortMode: FindObjectsSortMode.None)
                .ToList()
                .FindAll(x => x.gameObject.name == args[0])
                .Select(x => x.GetComponent<NpcMovement>());
            var funcParams = ConvertArgsToParams(args);
            foreach (NpcMovement npcController in movableObjects)
            {
                funcParams.TryGetValue(new string[] { "^r" }, out rot, defaultValue: npcController.transform.rotation.y);
                funcParams.TryGetValue(new string[] { "^rs" }, out rotSpd, defaultValue: 5f);
                yield return npcController.StartCoroutine(npcController.RotateHead(rot, rotSpd));
            }
        }

        private static IEnumerator MoveObject(string[] args)
        {
            float x, y, z;
            var movableObjects = UnityEngine.Object.FindObjectsByType<MovableObjects>(sortMode: FindObjectsSortMode.None)
                .ToList()
                .FindAll(x => x.gameObject.name == args[0])
                .Select(x => x.GetComponent<MovableObjects>());
            FunctionParams funcParams = ConvertArgsToParams(args);
            foreach (MovableObjects moveScript in movableObjects)
            {
                var targetTransform = ResolvePosition(funcParams);
                funcParams.TryGetValue(new string[] { "^s" }, out float speed, defaultValue: 3f);
                funcParams.TryGetValue(new string[] { "^x" }, out x, defaultValue: moveScript.transform.position.x);
                funcParams.TryGetValue(new string[] { "^y" }, out y, defaultValue: moveScript.transform.position.y);
                funcParams.TryGetValue(new string[] { "^z" }, out z, defaultValue: moveScript.transform.position.z);
                if (targetTransform != Vector3.zero)
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
        private static void PlayerFaceObject(string arg)
        {
            GameObject targetObject = GameObject.Find(arg);
            if (targetObject == null || string.IsNullOrEmpty(arg))
            {
                GameObject.Find("Player").GetComponent<PlayerController>().ChangeCameraLookAt(null);
                return;
            }
            GameObject.Find("Player").GetComponent<PlayerController>().ChangeCameraLookAt(targetObject.transform);
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
            var movableObjects = UnityEngine.Object.FindObjectsByType<MovingObject>(sortMode: FindObjectsSortMode.None)
                .ToList()
                .FindAll(x => x.gameObject.name == args[0])
                .Select(x => x.GetComponent<MovingObject>());
            FunctionParams funcParams = ConvertArgsToParams(args);
            foreach (MovingObject moveScript in movableObjects)
            {
                funcParams.TryGetValue(new string[] { "^s" }, out float speed, defaultValue: 1f);

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
            var movableObjects = UnityEngine.Object.FindObjectsByType<MovingObject>(sortMode: FindObjectsSortMode.None)
                .ToList()
                .FindAll(x => x.gameObject.name == args[0])
                .Select(x => x.GetComponent<MovingObject>());
            FunctionParams funcParams = ConvertArgsToParams(args);
            foreach (MovingObject moveScript in movableObjects)
            {
                funcParams.TryGetValue(new string[] { "^s" }, out float speed, defaultValue: 1f);
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

        private static void ChangeInteractionText(string arg)
        {
            UnityEngine.Object.FindAnyObjectByType<PlayerGrabInteraction>().currentItem.ChangeInteractionText(arg);
        }

        private static void SwitchCamera(string arg)
        {
            var eventCam = UnityEngine.Object.FindObjectsByType<CinemachineCamera>(sortMode: FindObjectsSortMode.None)
                .ToList()
                .Find(x => x.gameObject.name == arg).GetComponent<CinemachineCamera>();
            CameraManager.SwitchCamera(eventCam);
        }

        private static IEnumerator ChangeCamFoV(string[] args)
        {
            var eventCam = UnityEngine.Object.FindObjectsByType<CinemachineCamera>(sortMode: FindObjectsSortMode.None)
                .ToList()
                .Find(x => x.gameObject.name == args[0]).GetComponent<CinemachineCamera>();
            FunctionParams funcParams = ConvertArgsToParams(args);
            funcParams.TryGetValue(new string[] { "^fov" }, out float targetFoV, defaultValue: eventCam.Lens.FieldOfView);
            funcParams.TryGetValue(new string[] { "^t" }, out float duration, defaultValue: 1);
            float startingFOV = eventCam.Lens.FieldOfView;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                eventCam.Lens.FieldOfView = Mathf.Lerp(startingFOV, targetFoV, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }
            eventCam.Lens.FieldOfView = targetFoV;
        }
        private static void ChangeActivePlayer(string arg)
        {
            var playerSwitchManager = UnityEngine.Object.FindAnyObjectByType<PlayerSwitchManager>();
            var targetPlayer = UnityEngine.Object.FindObjectsByType<PlayerController>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .ToList()
                .Find(x => x.gameObject.name == arg);
            if (playerSwitchManager != null && targetPlayer != null)
            {
                playerSwitchManager.SwitchToPlayer(targetPlayer);
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

        private static IEnumerator ShowDialogue()
        {
            yield return DialogueSystem.Instance.dialogueContainer.ShowDialogue();
        }

        private static IEnumerator HideDialogue()
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
        private static void ChangeAudioVolumeProgression(string[] args)
        {
            if (args == null || args.Length == 0 || string.IsNullOrWhiteSpace(args[0]))
            {
                return;
            }

            var funcParams = ConvertArgsToParams(args);
            string eventName = args[0];
            funcParams.TryGetValue(new string[] { "^cat", "^type" }, out string category, defaultValue: string.Empty);
            funcParams.TryGetValue(new string[] { "^vinc", "^volinc", "^volumelimit", "^volumeincrease", "^limit", "^l" }, out float increaseAmount, defaultValue: 0f);
            funcParams.TryGetValue(new string[] { "^dur", "^duration", "^time" }, out float duration, defaultValue: 0f);

            foreach (string path in ResolveAudioStopPaths(eventName, category))
            {
                EventReference sound = RuntimeManager.PathToEventReference(path);
                AudioManager.Instance.ChangeVolumeProgression(sound, increaseAmount, duration);
            }
        }

        private static void ChangeAudioPitchProgression(string[] args)
        {
            if (args == null || args.Length == 0 || string.IsNullOrWhiteSpace(args[0]))
            {
                return;
            }

            var funcParams = ConvertArgsToParams(args);
            string eventName = args[0];
            funcParams.TryGetValue(new string[] { "^cat", "^type" }, out string category, defaultValue: string.Empty);
            funcParams.TryGetValue(new string[] { "^pinc", "^pitchinc", "^pitchlimit", "^pitchincrease", "^limit", "^l" }, out float increaseAmount, defaultValue: 0f);
            funcParams.TryGetValue(new string[] { "^dur", "^duration", "^time" }, out float duration, defaultValue: 0f);

            foreach (string path in ResolveAudioStopPaths(eventName, category))
            {
                EventReference sound = RuntimeManager.PathToEventReference(path);
                AudioManager.Instance.ChangePitchProgression(sound, increaseAmount, duration);
            }
        }
        private static void ParseAudioProgression(FunctionParams funcParams, out float volumeIncreaseLimit, out float pitchIncreaseLimit, out float increaseDuration)
        {
            funcParams.TryGetValue(new string[] { "^vinc", "^volinc", "^volumelimit", "^volumeincrease" }, out volumeIncreaseLimit, defaultValue: 0f);
            funcParams.TryGetValue(new string[] { "^pinc", "^pitchinc", "^pitchlimit", "^pitchincrease" }, out pitchIncreaseLimit, defaultValue: 0f);
            funcParams.TryGetValue(new string[] { "^dur", "^duration", "^time" }, out increaseDuration, defaultValue: 0f);

            if (volumeIncreaseLimit == 0f || pitchIncreaseLimit == 0f)
            {
                funcParams.TryGetValue(new string[] { "^limit", "^l" }, out float genericLimit, defaultValue: 0f);
                if (volumeIncreaseLimit == 0f)
                {
                    volumeIncreaseLimit = genericLimit;
                }

                if (pitchIncreaseLimit == 0f)
                {
                    pitchIncreaseLimit = genericLimit;
                }
            }
        }

        private static void PlaySFX(string[] args)
        {
            var funcParams = ConvertArgsToParams(args);
            EventReference sfx = RuntimeManager.PathToEventReference(sfxPath + args[0]);
            funcParams.TryGetValue(new string[] { "^v" }, out float volume, defaultValue: 1);
            funcParams.TryGetValue(new string[] { "^p" }, out float pitch, defaultValue: 1);
            funcParams.TryGetValue(new string[] { "^dup" }, out bool dup, defaultValue: false);
            ParseAudioProgression(funcParams, out float volumeIncreaseLimit, out float pitchIncreaseLimit, out float increaseDuration);

            Vector3 pos = ResolveAudioPosition(funcParams);
            AudioManager.Instance.PlayOneShot3D(sfx, dup, volume, pitch, pos, volumeIncreaseLimit, pitchIncreaseLimit, increaseDuration);
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
            funcParams.TryGetValue(new string[] { "^dup" }, out bool dup, defaultValue: false);
            ParseAudioProgression(funcParams, out float volumeIncreaseLimit, out float pitchIncreaseLimit, out float increaseDuration);

            Vector3 pos = ResolveAudioPosition(funcParams);
            AudioManager.Instance.PlayOneShot3D(bgm, dup, volume, pitch, pos, volumeIncreaseLimit, pitchIncreaseLimit, increaseDuration);
        }

        private static void PlayAmbience(string[] args)
        {
            Debug.Log("Playing Ambience");
            var funcParams = ConvertArgsToParams(args);
            EventReference ambience = RuntimeManager.PathToEventReference(ambiencePath + args[0]);
            funcParams.TryGetValue(new string[] { "^v" }, out float volume, defaultValue: 1);
            funcParams.TryGetValue(new string[] { "^p" }, out float pitch, defaultValue: 1);
            funcParams.TryGetValue(new string[] { "^dup" }, out bool dup, defaultValue: false);
            ParseAudioProgression(funcParams, out float volumeIncreaseLimit, out float pitchIncreaseLimit, out float increaseDuration);

            Vector3 pos = ResolveAudioPosition(funcParams);
            AudioManager.Instance.PlayOneShot3D(ambience, dup, volume, pitch, pos, volumeIncreaseLimit, pitchIncreaseLimit, increaseDuration);
        }

        private static void PlayVoice(string[] args)
        {
            var funcParams = ConvertArgsToParams(args);
            EventReference voice = RuntimeManager.PathToEventReference(voicePath + args[0]);
            funcParams.TryGetValue(new string[] { "^v" }, out float volume, defaultValue: 1);
            funcParams.TryGetValue(new string[] { "^p" }, out float pitch, defaultValue: 1);
            funcParams.TryGetValue(new string[] { "^dup" }, out bool dup, defaultValue: false);
            ParseAudioProgression(funcParams, out float volumeIncreaseLimit, out float pitchIncreaseLimit, out float increaseDuration);

            Vector3 pos = ResolveAudioPosition(funcParams);
            AudioManager.Instance.PlayOneShot3D(voice, dup, volume, pitch, pos, volumeIncreaseLimit, pitchIncreaseLimit, increaseDuration);
        }
        #endregion

        #region Misc Events
        private static void PlayVideo(string[] args)
        {

        }
        private static IEnumerator PlayCutscene(string[] args)
        {
            GameObject cutsceneGO = GameObject.Find(args[0]);
            if(cutsceneGO != null)
            {
                PlayableDirector cutsceneManager = cutsceneGO.GetComponent<PlayableDirector>();
                if(cutsceneManager != null)
                {
                    Debug.Log($"Playing Cutscene: {args[0]}");
                    cutsceneManager.Play();

                    while (cutsceneManager.state == PlayState.Playing)
                    {
                        yield return null;
                    }
                }
            }
        }

        private static IEnumerator Glitch(string arg)
        {
            if (float.TryParse(arg, out float duration))
            {
                EventReference sfx = RuntimeManager.PathToEventReference(sfxPath + "Glitch");

                var glitch = UnityEngine.Object.FindAnyObjectByType<AnalogGlitch>();
                AudioManager.Instance.PlayOneShot2D(sfx, 1, 1);
                glitch.enabled = true;
                yield return new WaitForSeconds(duration);
                glitch.enabled = false;
            }
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
                AsyncSceneLoader.Instance.LoadScenes(scenesToLoad, scenesToUnload, nextScene);
            }
        }

        private static void MoveSpecificNPC(string[] args)
        {
            if (args == null || args.Length == 0 || string.IsNullOrWhiteSpace(args[0]))
            {
                return;
            }

            var funcParams = ConvertArgsToParams(args);
            funcParams.TryGetValue(new string[] { "^s" }, out float speed, defaultValue: 1f);
            string npcName = args[0];

            var matchingNpcs = UnityEngine.Object.FindObjectsByType<NpcMovement>(sortMode: FindObjectsSortMode.None)
                .ToList()
                .FindAll(x => x.gameObject.name == npcName)
                .Select(x => x.GetComponent<NpcMovement>());

            foreach (NpcMovement npc in matchingNpcs)
            {
                if (npc == null)
                    continue;

                npc.blocker.enabled = false;
                npc.moveMyself = true;
                npc.agent.enabled = true;
                npc.agent.speed = speed;

                if (npc.point.Length > 0)
                {
                    npc.agent.SetDestination(npc.point[npc.idxPoint].position);
                    npc.animState = NPCAnimationState.Walk;
                }
            }
        }

        private static void StopSpecificNPC(string[] args)
        {
            if (args == null || args.Length == 0 || string.IsNullOrWhiteSpace(args[0]))
            {
                return;
            }

            string npcName = args[0];
            var matchingNpcs = UnityEngine.Object.FindObjectsByType<NpcMovement>(sortMode: FindObjectsSortMode.None)
                .ToList()
                .FindAll(x => x.gameObject.name.Contains(npcName))
                .Select(x => x.GetComponent<NpcMovement>());

            foreach (NpcMovement npc in matchingNpcs)
            {
                if (npc == null)
                    continue;

                npc.moveMyself = false;
                npc.agent.enabled = false;
                npc.agent.ResetPath();

                if (npc.TryGetComponent(out Animator animator))
                {
                    animator.SetFloat("Blend", 0f);
                }
            }
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
            UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None).ToList().Find(x => x.gameObject.name == arg).SetActive(false);
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
            AudioManager.Instance.PlayOneShot3D(sfx,true, 1, 1, pos);
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
                AudioManager.Instance.PlayOneShot3D(sfx,true, 1, 1, pos);
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
        private static IEnumerator CrossHole()
        {
            Waypoint startPos, endPos;
            if (Vector3.Distance(GameObject.Find("Player").transform.position, GameObject.Find("HoleIn").GetComponent<Waypoint>().position) <
               Vector3.Distance(GameObject.Find("Player").transform.position, GameObject.Find("HoleOut").GetComponent<Waypoint>().position))
            {
                startPos = GameObject.Find("HoleIn").GetComponent<Waypoint>();
                endPos = GameObject.Find("HoleOut").GetComponent<Waypoint>();
            }
            else
            {
                endPos = GameObject.Find("HoleIn").GetComponent<Waypoint>();
                startPos = GameObject.Find("HoleOut").GetComponent<Waypoint>();
            }
            GameObject.Find("HoleCam").transform.rotation = startPos.transform.rotation;
            GameObject.Find("HoleCam").GetComponent<CinemachineCamera>().Follow = endPos.transform;
            GameObject.Find("HoleCam").transform.position = startPos.position;
            SwitchCamera("HoleCam");
            while (Vector3.Distance(CameraManager.currentActiveCamera.transform.position, endPos.position) > .1f)
            {
                CameraManager.currentActiveCamera.transform.position = Vector3.Lerp(CameraManager.currentActiveCamera.transform.position, endPos.position, Time.deltaTime * 2.0f);
                yield return null;
            }
            yield return TeleportObject(new string[] { "Player", "^x", endPos.position.x.ToString(), "^z", endPos.position.z.ToString() });
            RotateObject(new string[] { "Player", "^r", startPos.transform.rotation.y.ToString() });
            SwitchCamera("Player Camera");
            yield return new WaitForSeconds(1);
            GameObject.Find("HoleCam").GetComponent<CinemachineCamera>().Follow = startPos.transform;
        }

        private static void InspectFragment(string[] args)
        {
            int fragment;
            var funcParams = ConvertArgsToParams(args);
            funcParams.TryGetValue(new string[] { "^f" }, out fragment);
            DialogueSystem.Instance.dialogueContainer.HideDialogue();
            InspectManagerUI.Instance.OnItemSelected(FragmentManager.Instance.GetFragmentGO(fragment));
        }
        private static T FindNamedComponent<T>(string name) where T : Component
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            return UnityEngine.Object.FindObjectsByType<T>(sortMode: FindObjectsSortMode.None)
                .FirstOrDefault(x => x != null && x.gameObject != null && x.gameObject.name == name);
        }

        private static Vector3 GetThrowDirection(PlayerGrabInteraction thrower)
        {
            if (thrower == null)
                return Vector3.forward;

            if (thrower.playerCamera != null)
                return thrower.playerCamera.transform.forward;

            return thrower.transform.forward;
        }

        public static void GrabItem(string[] args)
        {
            string receiverName = args.Length > 0 ? args[0] : string.Empty;
            string itemName = args.Length > 1 ? args[1] : string.Empty;

            var funcParams = ConvertArgsToParams(args);
            funcParams.TryGetValue(new[] { "^who", "^receiver", "^target" }, out string receiverToken, defaultValue: receiverName);
            funcParams.TryGetValue(new[] { "^item", "^obj", "^name" }, out string itemToken, defaultValue: itemName);

            if (string.IsNullOrEmpty(receiverToken))
                receiverToken = receiverName;
            if (string.IsNullOrEmpty(itemToken))
                itemToken = itemName;

            PlayerGrabInteraction receiver = FindNamedComponent<PlayerGrabInteraction>(receiverToken);
            ItemInteraction item = FindNamedComponent<ItemInteraction>(itemToken);

            if (receiver == null)
            {
                if (!string.IsNullOrEmpty(receiverToken))
                    Debug.LogWarning($"GrabItem: receiver not found: {receiverToken}");
                return;
            }

            if (item == null && receiver.HeldItem != null)
                item = receiver.HeldItem;

            if (item == null)
            {
                if (!string.IsNullOrEmpty(itemToken))
                    Debug.LogWarning($"GrabItem: item not found: {itemToken}");
                return;
            }

            receiver.TryGrabItem(item);
        }

        public static void ThrowItem(string[] args)
        {
            string throwerName = args.Length > 0 ? args[0] : string.Empty;
            string itemName = args.Length > 1 ? args[1] : string.Empty;

            var funcParams = ConvertArgsToParams(args);
            funcParams.TryGetValue(new[] { "^who", "^thrower", "^actor" }, out string throwerToken, defaultValue: throwerName);
            funcParams.TryGetValue(new[] { "^item", "^obj", "^name" }, out string itemToken, defaultValue: itemName);
            funcParams.TryGetValue(new[] { "^pwr" }, out float throwForce, defaultValue: 1f);

            if (string.IsNullOrEmpty(throwerToken))
                throwerToken = throwerName;
            if (string.IsNullOrEmpty(itemToken))
                itemToken = itemName;

            PlayerGrabInteraction thrower = FindNamedComponent<PlayerGrabInteraction>(throwerToken);
            ItemInteraction item = FindNamedComponent<ItemInteraction>(itemToken);

            if (thrower == null)
            {
                if (!string.IsNullOrEmpty(throwerToken))
                    Debug.LogWarning($"ThrowItem: thrower not found: {throwerToken}");
                return;
            }

            if (item == null && thrower.HeldItem != null)
                item = thrower.HeldItem;

            if (item == null)
            {
                if (!string.IsNullOrEmpty(itemToken))
                    Debug.LogWarning($"ThrowItem: item not found: {itemToken}");
                return;
            }

            Vector3 direction = GetThrowDirection(thrower);
            if (item.IsHeld)
            {
                thrower.TryThrowHeldItem(throwForce, direction);
                return;
            }

            thrower.TryThrowItem(item, throwForce, direction);
        }

        public static void TransferItem(string[] args)
        {
            string sourceName = args.Length > 0 ? args[0] : string.Empty;
            string targetName = args.Length > 1 ? args[1] : string.Empty;
            string itemName = args.Length > 2 ? args[2] : string.Empty;

            var funcParams = ConvertArgsToParams(args);
            funcParams.TryGetValue(new[] { "^from", "^source", "^who", "^actor" }, out string sourceToken, defaultValue: sourceName);
            funcParams.TryGetValue(new[] { "^to", "^target", "^receiver", "^dest" }, out string targetToken, defaultValue: targetName);
            funcParams.TryGetValue(new[] { "^item", "^obj", "^name" }, out string itemToken, defaultValue: itemName);

            if (string.IsNullOrEmpty(sourceToken))
                sourceToken = sourceName;
            if (string.IsNullOrEmpty(targetToken))
                targetToken = targetName;
            if (string.IsNullOrEmpty(itemToken))
                itemToken = itemName;

            PlayerGrabInteraction source = FindNamedComponent<PlayerGrabInteraction>(sourceToken);
            PlayerGrabInteraction target = FindNamedComponent<PlayerGrabInteraction>(targetToken);
            ItemInteraction item = FindNamedComponent<ItemInteraction>(itemToken);

            if (source == null)
            {
                if (!string.IsNullOrEmpty(sourceToken))
                    Debug.LogWarning($"TransferItem: source not found: {sourceToken}");
                return;
            }

            if (target == null)
            {
                if (!string.IsNullOrEmpty(targetToken))
                    Debug.LogWarning($"TransferItem: target not found: {targetToken}");
                return;
            }

            if (source == target)
                return;

            if (item == null)
            {
                if (source.HeldItem != null)
                    item = source.HeldItem;
                else if (target.HeldItem != null)
                    item = target.HeldItem;
                else
                {
                    if (!string.IsNullOrEmpty(itemToken))
                        Debug.LogWarning($"TransferItem: item not found: {itemToken}");
                    return;
                }
            }

            if (item == source.HeldItem)
            {
                if (!source.TryTransferHeldItemTo(target))
                    Debug.LogWarning($"TransferItem: failed to transfer held item '{item.gameObject.name}' from {source.gameObject.name} to {target.gameObject.name}.");
                return;
            }

            if (target.HeldItem != null)
            {
                Debug.LogWarning($"TransferItem: target {target.gameObject.name} is already holding an item.");
                return;
            }

            if (item.IsHeld)
            {
                Debug.LogWarning($"TransferItem: item '{item.gameObject.name}' is already held elsewhere.");
                return;
            }

            if (!target.TryGrabItem(item))
            {
                Debug.LogWarning($"TransferItem: failed to transfer item '{item.gameObject.name}' to {target.gameObject.name}.");
            }
        }
        #endregion
    }
}
