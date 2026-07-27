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

        List<AsyncOperation> loadOperations = new List<AsyncOperation>();
        List<AsyncOperation> unloadOperations = new List<AsyncOperation>();

        foreach (SceneField scene in scenesToLoad ?? new List<SceneField>())
        {
            if (scene == null || string.IsNullOrEmpty(scene.SceneName))
                continue;

            if(scene != persistentScene)
                currentChapterScene = scene;

            Scene sceneToLoad = SceneManager.GetSceneByName(scene.SceneName);
            if (!sceneToLoad.isLoaded)
            {
                loadOperations.Add(SceneManager.LoadSceneAsync(scene.SceneName, LoadSceneMode.Additive));
            }
        }

        while (loadOperations.Any(op => !op.isDone))
        {
            float totalProgress = 0f;
            int progressCount = 0;

            foreach (AsyncOperation operation in loadOperations)
            {
                totalProgress += operation.progress;
                progressCount++;
            }

            CurrentProgress = progressCount > 0 ? totalProgress / progressCount : 0f;
            ProgressUpdated?.Invoke(CurrentProgress);
            onProgress?.Invoke(CurrentProgress);

            yield return null;
        }

        if (activeScene != null && !string.IsNullOrEmpty(activeScene.SceneName))
        {
            Scene targetScene = SceneManager.GetSceneByName(activeScene.SceneName);
            if (targetScene.IsValid() && targetScene.isLoaded)
            {
                SceneManager.SetActiveScene(targetScene);
            }
        }

        foreach (SceneField scene in scenesToUnload ?? new List<SceneField>())
        {
            if (scene == null || string.IsNullOrEmpty(scene.SceneName))
                continue;

            Scene sceneToUnload = SceneManager.GetSceneByName(scene.SceneName);
            if (sceneToUnload.IsValid() && sceneToUnload.isLoaded && sceneToUnload != SceneManager.GetActiveScene())
            {
                unloadOperations.Add(SceneManager.UnloadSceneAsync(scene.SceneName));
            }
        }

        while (unloadOperations.Any(op => !op.isDone))
        {
            float totalProgress = 0f;
            int progressCount = 0;

            foreach (AsyncOperation operation in unloadOperations)
            {
                totalProgress += operation.progress;
                progressCount++;
            }

            CurrentProgress = progressCount > 0 ? totalProgress / progressCount : 1f;
            ProgressUpdated?.Invoke(CurrentProgress);
            onProgress?.Invoke(CurrentProgress);

            yield return null;
        }

        IsBusy = false;
        CurrentProgress = 1f;
        ProgressUpdated?.Invoke(CurrentProgress);
        onProgress?.Invoke(CurrentProgress);
        onComplete?.Invoke();
        Completed?.Invoke();
    }
}
