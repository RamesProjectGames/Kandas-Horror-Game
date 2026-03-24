using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
[System.Serializable]
public class LoadingBarPercentages
{
    public float progresThreshold;
    public Sprite loadingBarThreshold;
}
public class MainMenuManager : MonoBehaviour
{
    public static MainMenuManager Instance { get; private set; }
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private List<LoadingBarPercentages>loadingPercentages = new List<LoadingBarPercentages>();
    [SerializeField] private Image loadingBar;
    [SerializeField] private List<GameObject> gameObjectsToHide;
    [SerializeField] private List<GameObject> gameObjectsToDestroy;

    [Header("Scenes to load")]
    [SerializeField] private SceneField persistentScene;
    [SerializeField] private SceneField mainMenuScene;
    private SceneField currentChapter;

    private List<AsyncOperation> loadOperations = new List<AsyncOperation>();
    void Awake()
    {
        loadingPanel.SetActive(false);
        Instance = this;
    }
    public void ChapterSelect(SceneField sceneName)
    {        
        currentChapter = sceneName;
    }
    public void ConfirmStartGame()
    {
        ConfirmationUI.Instance.SetConfirmationUI("Enter Chapter?", () => StartGame());
    }
    public void StartGame()
    {
        HideMenu();
        DestroyItems();

        loadingPanel.SetActive(true);

        loadOperations.Clear();
        totalProgress = 0f;

        loadOperations.Add(SceneManager.LoadSceneAsync(persistentScene, LoadSceneMode.Additive));
        loadOperations.Add(SceneManager.LoadSceneAsync(currentChapter, LoadSceneMode.Additive));

        StartCoroutine(ProgressLoadingBar());
    }
    private void HideMenu()
    {
        foreach (GameObject gameObject in gameObjectsToHide)
        {
            gameObject.SetActive(false);
        }
    }
    private void DestroyItems()
    {
        foreach (GameObject gameObject in gameObjectsToDestroy)
        {
            Destroy(gameObject);
        }
    }
    [SerializeField] private float progressSpeed = 0.5f;
    [SerializeField] private float delayAfterDone = 1.5f;
    float totalProgress; // just to show the progress bar
    private IEnumerator ProgressLoadingBar()
    {
        totalProgress = 0f;
        UpdateLoadingSprite();

        while (true)
        {
            float actualProgress = loadOperations.Average(op => op.progress);

            bool isDone = actualProgress >= 1f;
            // While loading, only allow progress to go up to 0.9
            float targetProgress = isDone ? 1f : Mathf.Clamp(actualProgress, 0f, 0.9f);

            // Smoothly move displayed progress toward target
            totalProgress = Mathf.MoveTowards(totalProgress, targetProgress, progressSpeed * Time.deltaTime);
            Debug.Log($"Actual: {actualProgress:F3} | Display: {totalProgress:F3} | Done: {isDone}");

            UpdateLoadingSprite();

            // Only break when everything is done AND displayed progress reached 1
            if (isDone && totalProgress >= 1f)
            {
                yield return new WaitForSeconds(delayAfterDone);
                SceneManager.SetActiveScene(SceneManager.GetSceneByName(persistentScene.SceneName));
                SceneManager.UnloadSceneAsync(mainMenuScene);
                break;
            }

            yield return null;
        }

    }
    private void UpdateLoadingSprite()
    {
        foreach (var item in loadingPercentages.OrderBy(x => x.progresThreshold))
        {
            if (totalProgress >= item.progresThreshold)
            {
                loadingBar.sprite = item.loadingBarThreshold;
            }
        }
    }
    public void ConfirmQuitGame() => ConfirmationUI.Instance.SetConfirmationUI("Are you sure you want to quit?", () => QuitGame());
    public void QuitGame()
    {
        Application.Quit();
    }
}
