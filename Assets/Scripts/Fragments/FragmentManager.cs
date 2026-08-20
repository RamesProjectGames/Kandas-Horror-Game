using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class FragmentManager : MonoBehaviour
{
    public static FragmentManager Instance;
    public List<FragmentData> allFragments = new List<FragmentData>();
    List<FragmentData> currentFragments = new List<FragmentData>();
    List<Fragment> fragmentGOs = new List<Fragment>();

    private void Awake()
    {
        transform.SetParent(null);
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
        // allFragments.Clear();
    }

    public void SpawnFragmentInScene(FragmentData fragment)
    {
        if(fragment == null || fragment.fragmentPrefab == null)
        {
            Debug.LogWarning("Fragment or its prefab is null. Cannot spawn.");
            return;
        }
        GameObject fragmentObject = Instantiate(fragment.fragmentPrefab, fragment.fragmentPosition, Quaternion.identity, GameObject.Find("===Fragments===").transform);
        fragmentObject.name = fragment.fragmentName;
        Fragment fragmentComponent = fragmentObject.GetComponent<Fragment>();
        fragmentComponent.SetFragmentData(fragment);
        fragmentGOs.Add(fragmentComponent);
    }

    public void AddFragment(Fragment fragment)
    {
        if (!FragmentOwned(fragment))
        {
            currentFragments.Add(fragment.GetFragmentData());

            if (ChapterDataManager.Instance != null)
            {
                FragmentData fragmentData = fragment.GetFragmentData();
                int fragmentId = fragmentData != null && fragmentData.fragmentId >= 0
                    ? fragmentData.fragmentId
                    : GetStableFragmentId(fragmentData);

                ChapterDataManager.Instance.CollectFragment(fragmentId);
            }
        }
    }
    public void RemoveFragment(Fragment fragment)
    {
        if (FragmentOwned(fragment))
            currentFragments.Remove(fragment.GetFragmentData());
    }

    public void ClearFragment()
    {
        currentFragments.Clear();
    }

    public bool CheckCompletedFragments()
    {
        List<string> allFragments = ObjectiveManager.Instance.objectiveDatas.FindAll(x => x.fragmentData != null).Select(x => x.fragmentData.fragmentName).ToList();
        for (int i = 0; i < allFragments.Count; i++)
        {
            if(currentFragments.Find(x => x.fragmentName == allFragments[i]) == null) return false;
        }
        return true;
    }

    public bool FragmentOwned(Fragment fragment)
    {
        return currentFragments.Contains(fragment.GetFragmentData());
    }

    private int GetStableFragmentId(FragmentData fragmentData)
    {
        if (fragmentData == null) return -1;

        int hash = 0;
        foreach (char c in fragmentData.fragmentName)
        {
            hash = (hash * 31) + c;
        }

        return hash;
    }

    public void UpdateFragmentState(FragmentData fragData)
    {
        Fragment fragmentGO = fragmentGOs.Find(x => x.GetFragmentData() == fragData);
        if (fragmentGO != null)
        {
            fragmentGO.gameObject.SetActive(ObjectiveManager.Instance.CheckIfFragmentValid(fragData));
        }
    }

    public Fragment GetFragment(int fragIdx)
    {
        return fragmentGOs[fragIdx];
    }
}
