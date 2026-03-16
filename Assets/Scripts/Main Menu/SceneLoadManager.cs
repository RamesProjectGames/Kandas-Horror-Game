using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class SceneLoadManager : MonoBehaviour
{
    public List<SceneField> sceneToLoad = new List<SceneField>();
    public List<SceneField> sceneToUnload = new List<SceneField>();
    public void LoadScenes()
    {
        for (int i = 0; i < sceneToLoad.Count; i++)
        {
            bool isSceneLoaded = false;
            for (int j = 0; j < SceneManager.sceneCount; j++)
            {
                if (SceneManager.GetSceneAt(j).name == sceneToLoad[i].SceneName)
                {
                    isSceneLoaded = true;
                    break;
                }
            }
            if(!isSceneLoaded) SceneManager.LoadSceneAsync(sceneToLoad[i].SceneName, LoadSceneMode.Additive);
        }
    }
    public void UnloadScenes()
    {
        for (int i = 0; i < sceneToUnload.Count; i++)
        {
            for (int j = 0; j < SceneManager.sceneCount; j++)
            {
                if (SceneManager.GetSceneAt(j).name == sceneToUnload[i].SceneName)
                {
                    SceneManager.UnloadSceneAsync(sceneToUnload[i].SceneName);
                    break;
                }
            }
        }
    }
}
