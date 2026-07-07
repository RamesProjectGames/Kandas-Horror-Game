using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ChapterDataManager : MonoBehaviour
{
    public static ChapterDataManager Instance { get; private set; }

    [Header("Progress")]
    public int highestChapterUnlocked = 1;
    public int currentChapterIndex = 0;
    public List<int> collectedFragments = new List<int>();
    public List<SceneField> chapterScenes = new List<SceneField>();

    [SerializeField] private string saveFileName = "chapter_progress.json";

    private string SavePath => Path.Combine(Application.persistentDataPath, saveFileName);

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
        LoadProgress();
    }

    public void SaveProgress()
    {
        ChapterProgressData data = new ChapterProgressData
        {
            highestChapterUnlocked = highestChapterUnlocked,
            collectedFragments = new List<int>(collectedFragments)
        };

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
    }

    public void LoadProgress()
    {
        if (!File.Exists(SavePath))
        {
            highestChapterUnlocked = 1;
            collectedFragments = new List<int>();
            return;
        }

        string json = File.ReadAllText(SavePath);
        ChapterProgressData data = JsonUtility.FromJson<ChapterProgressData>(json);

        if (data != null)
        {
            highestChapterUnlocked = Mathf.Max(1, data.highestChapterUnlocked);
            collectedFragments = data.collectedFragments != null ? data.collectedFragments : new List<int>();
        }
        else
        {
            highestChapterUnlocked = 1;
            collectedFragments = new List<int>();
        }
    }

    public void CollectFragment(int fragmentId)
    {
        if (!collectedFragments.Contains(fragmentId))
        {
            collectedFragments.Add(fragmentId);
            SaveProgress();
        }
    }

    public bool HasCollectedFragment(int fragmentId)
    {
        return collectedFragments.Contains(fragmentId);
    }

    public void UnlockChapter(int chapterIndex)
    {
        int normalizedChapter = Mathf.Max(1, chapterIndex);
        highestChapterUnlocked = Mathf.Max(highestChapterUnlocked, normalizedChapter);
        SaveProgress();
    }
    public void SelectChapter(int chapterIndex)
    {
        currentChapterIndex = chapterIndex;
    }

    public bool IsChapterUnlocked(int chapterIndex)
    {
        return chapterIndex <= highestChapterUnlocked;
    }

    public SceneField GetChapterScene(int chapterIndex)
    {
        int safeIndex = Mathf.Max(1, chapterIndex) - 1;
        if (safeIndex >= 0 && safeIndex < chapterScenes.Count)
        {
            return chapterScenes[safeIndex];
        }

        return null;
    }

    private void OnApplicationQuit()
    {
        SaveProgress();
    }
}

[System.Serializable]
public class ChapterProgressData
{
    public int highestChapterUnlocked = 1;
    public List<int> collectedFragments = new List<int>();
}
