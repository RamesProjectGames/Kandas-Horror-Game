using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ChapterManager : MonoBehaviour
{
    public GameObject chapterPanel;
    public List<Button> chapters = new List<Button>();
    public List<ChapterData> chaptersData = new List<ChapterData>();
    public TextMeshProUGUI title;
    public TextMeshProUGUI desc;
    public Image content;

    string chapterSceneName = "";

    public void ChangePanelState(bool state) { chapterPanel.SetActive(state); }
    public void ChapterSelect(int index)
    {
        for (int i = 0; i < chapters.Count; i++)
        {
            chapters[i].interactable = i != index;
            chapters[i].gameObject.GetComponent<TextMeshProUGUI>().fontStyle = i == index ? FontStyles.Underline : FontStyles.Normal; 
        }

        title.text = chaptersData[index].chapterTitle;
        desc.text = chaptersData[index].chapterDesc;
        content.sprite = chaptersData[index].chapterContent;
        chapterSceneName = chaptersData[index].chapterScene;
    }

    public void StartGame()
    {
        if(string.IsNullOrEmpty(chapterSceneName)) return;
        try
        {
            SceneManager.LoadScene(chapterSceneName);
        }
        catch 
        {
            Debug.Log("Scene doesn't exist");            
        }
    }
}
[System.Serializable]
public class ChapterData
{
    public string chapterTitle;
    public string chapterDesc;
    public Sprite chapterContent;
    public string chapterScene;
}