using System;
using System.Collections.Generic;
using Dialogue.Functions;
using FMODUnity;
using UnityEngine;

public class LocalAudioManager : MonoBehaviour
{
    public static string sfxPath = "event:/SFX/";
    public static string bgmPath = "event:/BGM/";
    public static string ambiencePath = "event:/Ambience/";
    public static string voicePath = "event:/Voice/";
    [SerializeField] private AudioManager audioManager;

    private void Awake()
    {
        if (audioManager == null)
        {
            audioManager = AudioManager.Instance;
        }

        if (audioManager == null)
        {
            Debug.LogWarning("[LocalAudioManager] AudioManager reference not found.");
        }
    }

    public void StopAudioByName(string audioName)
    {
        if (audioManager == null)
        {
            Debug.LogWarning($"[LocalAudioManager] Cannot stop audio '{audioName}' because no AudioManager is available.");
            return;
        }

        if (string.IsNullOrWhiteSpace(audioName))
        {
            Debug.LogWarning("[LocalAudioManager] Audio name is empty.");
            return;
        }

        foreach (string path in ResolveAudioPaths(audioName))
        {
            EventReference soundReference = RuntimeManager.PathToEventReference(path);
            audioManager.StopSoundInstance(soundReference);
        }
    }

    public void PlayAudioByName(string audioName, bool duplicate = false, float volume = 1f, float pitch = 1f, Vector3? position = null)
    {
        if (audioManager == null)
        {
            Debug.LogWarning($"[LocalAudioManager] Cannot play audio '{audioName}' because no AudioManager is available.");
            return;
        }

        if (string.IsNullOrWhiteSpace(audioName))
        {
            Debug.LogWarning("[LocalAudioManager] Audio name is empty.");
            return;
        }

        foreach (string path in ResolveAudioPaths(audioName))
        {
            EventReference soundReference = RuntimeManager.PathToEventReference(path);
            Vector3 playPosition = position ?? GetDefaultPlayPosition();
            audioManager.PlayOneShot3D(soundReference, duplicate, volume, pitch, playPosition);
            break;
        }
    }

    public void PlayAudio(string audioName)
    {
        PlayAudioByName(audioName);
    }

    public void StopAudio(string audioName)
    {
        StopAudioByName(audioName);
    }

    public void PlaySFX(string audioName, bool duplicate = false, float volume = 1f, float pitch = 1f, Vector3? position = null)
    {
        PlayAudioByCategory(audioName, FuncDBExtension.sfxPath, duplicate, volume, pitch, position);
    }

    public void PlayBGM(string audioName, bool duplicate = false, float volume = 1f, float pitch = 1f, Vector3? position = null)
    {
        PlayAudioByCategory(audioName, FuncDBExtension.bgmPath, duplicate, volume, pitch, position);
    }

    public void PlayAmbiance(string audioName, bool duplicate = false, float volume = 1f, float pitch = 1f, Vector3? position = null)
    {
        PlayAudioByCategory(audioName, FuncDBExtension.ambiencePath, duplicate, volume, pitch, position);
    }

    public void PlayVoice(string audioName, bool duplicate = false, float volume = 1f, float pitch = 1f, Vector3? position = null)
    {
        PlayAudioByCategory(audioName, FuncDBExtension.voicePath, duplicate, volume, pitch, position);
    }

    public void StopSFX(string audioName)
    {
        StopAudioByCategory(audioName, FuncDBExtension.sfxPath);
    }

    public void StopBGM(string audioName)
    {
        StopAudioByCategory(audioName, FuncDBExtension.bgmPath);
    }

    public void StopAmbiance(string audioName)
    {
        StopAudioByCategory(audioName, FuncDBExtension.ambiencePath);
    }

    public void StopVoice(string audioName)
    {
        StopAudioByCategory(audioName, FuncDBExtension.voicePath);
    }

    private Vector3 GetDefaultPlayPosition()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        return playerObject != null ? playerObject.transform.position : Vector3.zero;
    }

    private void PlayAudioByCategory(string audioName, string basePath, bool duplicate, float volume, float pitch, Vector3? position)
    {
        if (audioManager == null)
        {
            Debug.LogWarning($"[LocalAudioManager] Cannot play audio '{audioName}' because no AudioManager is available.");
            return;
        }

        if (string.IsNullOrWhiteSpace(audioName))
        {
            Debug.LogWarning("[LocalAudioManager] Audio name is empty.");
            return;
        }

        string fullPath = basePath + audioName;
        EventReference soundReference = RuntimeManager.PathToEventReference(fullPath);
        Vector3 playPosition = position ?? GetDefaultPlayPosition();
        audioManager.PlayOneShot3D(soundReference, duplicate, volume, pitch, playPosition);
    }

    private void StopAudioByCategory(string audioName, string basePath)
    {
        if (audioManager == null)
        {
            Debug.LogWarning($"[LocalAudioManager] Cannot stop audio '{audioName}' because no AudioManager is available.");
            return;
        }

        if (string.IsNullOrWhiteSpace(audioName))
        {
            Debug.LogWarning("[LocalAudioManager] Audio name is empty.");
            return;
        }

        string fullPath = basePath + audioName;
        EventReference soundReference = RuntimeManager.PathToEventReference(fullPath);
        audioManager.StopSoundInstance(soundReference);
    }

    private static IEnumerable<string> ResolveAudioPaths(string eventName)
    {
        if (string.IsNullOrWhiteSpace(eventName))
        {
            return Array.Empty<string>();
        }

        if (eventName.StartsWith("event:/", StringComparison.OrdinalIgnoreCase))
        {
            return new[] { eventName };
        }

        return new[]
        {
            FuncDBExtension.sfxPath + eventName,
            FuncDBExtension.bgmPath + eventName,
            FuncDBExtension.ambiencePath + eventName,
            FuncDBExtension.voicePath + eventName
        };
    }
}
