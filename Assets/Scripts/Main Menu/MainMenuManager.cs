using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class MainMenuManager : MonoBehaviour
{
    public static MainMenuManager Instance { get; private set; }
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private Image loadingBar;
    [SerializeField] private List<GameObject> gameObjectsToHide;

    [Header("Scenes to load")]
    [SerializeField] private SceneField persistentScene;
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
    public void StartGame()
    {
        HideMenu();

        loadingPanel.SetActive(true);

        loadOperations.Add(SceneManager.LoadSceneAsync(persistentScene));
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
    private void ShowMenu()
    {
        foreach (GameObject gameObject in gameObjectsToHide)
        {
            gameObject.SetActive(true);
        }
    }
    private IEnumerator ProgressLoadingBar()
    {
        float loadProgress = 0f;
        for (int i = 0; i < loadOperations.Count; i++)
        {
            while(!loadOperations[i].isDone)
            {
                loadProgress += loadOperations[i].progress;
                loadingBar.fillAmount = loadProgress / loadOperations.Count;
                yield return null;
            }
        }
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}
