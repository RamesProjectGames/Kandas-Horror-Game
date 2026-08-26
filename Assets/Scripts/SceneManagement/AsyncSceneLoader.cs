using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AsyncSceneLoader : MonoBehaviour
{
    public static AsyncSceneLoader Instance { get; private set; }

    public bool IsBusy { get; private set; }
    public float CurrentProgress { get; private set; }

    public event Action<float> ProgressUpdated;
    public event Action Completed;

    public SceneField currentChapterScene;

    public SceneField persistentScene;

    private void Awake()
    {
        transform.SetParent(null);
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    public static AsyncSceneLoader GetOrCreateInstance()
    {
        if (Instance == null)
        {
            GameObject loaderObject = new GameObject("AsyncSceneLoader");
            Instance = loaderObject.AddComponent<AsyncSceneLoader>();
        }

        return Instance;
    }

    public Coroutine LoadScenes(List<SceneField> scenesToLoad, List<SceneField> scenesToUnload, SceneField activeScene, Action onComplete = null, Action<float> onProgress = null)
    {
        return GetOrCreateInstance().StartCoroutine(LoadScenesRoutine(scenesToLoad, scenesToUnload, activeScene, onComplete, onProgress));
    }
    private IEnumerator LoadScenesRoutine(List<SceneField> scenesToLoad, List<SceneField> scenesToUnload, SceneField activeScene, Action onComplete, Action<float> onProgress)
    {
        IsBusy = true;
        CurrentProgress = 0f;

        // 1. Store the currently active scene (the "old" one)
        Scene oldActiveScene = SceneManager.GetActiveScene();

        // ---------- Step A: Unload non‑active scenes (first) ----------
        List<AsyncOperation> unloadOperations = new List<AsyncOperation>();

        foreach (SceneField scene in scenesToUnload ?? new List<SceneField>())
        {
            if (scene == null || string.IsNullOrEmpty(scene.SceneName))
                continue;

            Scene sceneToUnload = SceneManager.GetSceneByName(scene.SceneName);
            if (!sceneToUnload.IsValid() || !sceneToUnload.isLoaded)
                continue;

            // Do NOT unload the currently active scene yet – keep it for later
            if (sceneToUnload == oldActiveScene)
                continue;

            unloadOperations.Add(SceneManager.UnloadSceneAsync(sceneToUnload));
        }

        // Wait for these unloads to finish
        while (unloadOperations.Any(op => !op.isDone))
        {
            float totalProgress = 0f;
            foreach (var op in unloadOperations)
                totalProgress += op.progress;
            CurrentProgress = unloadOperations.Count > 0 ? totalProgress / unloadOperations.Count : 0.5f;
            ProgressUpdated?.Invoke(CurrentProgress);
            onProgress?.Invoke(CurrentProgress);
            yield return null;
        }

        // ---------- Step B: Load new scenes ----------
        List<AsyncOperation> loadOperations = new List<AsyncOperation>();

        foreach (SceneField scene in scenesToLoad ?? new List<SceneField>())
        {
            if (scene == null || string.IsNullOrEmpty(scene.SceneName))
                continue;

            if (scene != persistentScene)
            {
                currentChapterScene = scene;
            }

            Scene existing = SceneManager.GetSceneByName(scene.SceneName);
            if (!existing.isLoaded)
            {
                loadOperations.Add(SceneManager.LoadSceneAsync(scene.SceneName, LoadSceneMode.Additive));
            }
        }

        while (loadOperations.Any(op => !op.isDone))
        {
            float totalProgress = 0f;
            foreach (var op in loadOperations)
                totalProgress += op.progress;
            CurrentProgress = loadOperations.Count > 0 ? totalProgress / loadOperations.Count : 0.8f;
            ProgressUpdated?.Invoke(CurrentProgress);
            onProgress?.Invoke(CurrentProgress);
            yield return null;
        }

        // ---------- Step C: Make Persistent the Active Scene so old active scene can be reloaded ----------
        if (persistentScene != null && !string.IsNullOrEmpty(persistentScene.SceneName))
        {
            Scene activePersistScene = SceneManager.GetSceneByName(persistentScene.SceneName);
            if (activePersistScene.IsValid() && activePersistScene.isLoaded)
            {
                SceneManager.SetActiveScene(activePersistScene);
            }
        }

        // ---------- Step D: Finally unload the old active scene ----------
        // (only if it's still loaded and not the same as the new active)
        if (oldActiveScene.IsValid() && oldActiveScene.isLoaded)
        {
            AsyncOperation finalUnload = SceneManager.UnloadSceneAsync(oldActiveScene);
            while (!finalUnload.isDone)
            {
                CurrentProgress = 0.95f + 0.05f * finalUnload.progress;
                ProgressUpdated?.Invoke(CurrentProgress);
                onProgress?.Invoke(CurrentProgress);
                yield return null;
            }
        }

        // ---------- Step E: Reload and set the new active scene ----------
        if (activeScene != null || !string.IsNullOrEmpty(activeScene.SceneName))
        {
            currentChapterScene = activeScene;
            Scene existing = SceneManager.GetSceneByName(activeScene.SceneName);
            if (!existing.isLoaded)
            {
                loadOperations.Add(SceneManager.LoadSceneAsync(activeScene.SceneName, LoadSceneMode.Additive));

                while (loadOperations.Any(op => !op.isDone))
                {
                    float totalProgress = 0f;
                    foreach (var op in loadOperations)
                        totalProgress += op.progress;
                    CurrentProgress = loadOperations.Count > 0 ? totalProgress / loadOperations.Count : 0.8f;
                    ProgressUpdated?.Invoke(CurrentProgress);
                    onProgress?.Invoke(CurrentProgress);
                    yield return null;
                }
            }

            Scene targetScene = SceneManager.GetSceneByName(activeScene.SceneName);
            if (targetScene.IsValid() && targetScene.isLoaded)
            {
                SceneManager.SetActiveScene(targetScene);
            }
        }

        // ---------- Done ----------
        IsBusy = false;
        CurrentProgress = 1f;
        ProgressUpdated?.Invoke(CurrentProgress);
        onProgress?.Invoke(CurrentProgress);
        onComplete?.Invoke();
        Completed?.Invoke();
    }
}
