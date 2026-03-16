using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private Image loadingBar;
    [SerializeField] private List<GameObject> gameObjectsToHide;

    [Header("Scenes to load")]
    [SerializeField] private SceneField persistentScene;
    [SerializeField] private SceneField gameScene;

    private List<AsyncOperation> loadOperations = new List<AsyncOperation>();
    void Awake()
    {
        loadingPanel.SetActive(false);
    }
    public void StartGame()
    {
        HideMenu();

        loadingPanel.SetActive(true);

        loadOperations.Add(SceneManager.LoadSceneAsync(persistentScene));
        loadOperations.Add(SceneManager.LoadSceneAsync(gameScene, LoadSceneMode.Additive));

        StartCoroutine(ProgressLoadingBar());
    }
    private void HideMenu()
    {
        foreach (GameObject gameObject in gameObjectsToHide)
        {
            gameObject.SetActive(false);
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
}
