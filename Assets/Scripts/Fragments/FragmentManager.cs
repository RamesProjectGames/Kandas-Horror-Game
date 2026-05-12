using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class FragmentManager : MonoBehaviour
{
    public static FragmentManager Instance;
    public GameObject fragmentPrefab;
    public List<FragmentData> allFragments = new List<FragmentData>();
    List<FragmentData> currentFragments = new List<FragmentData>();
    List<Fragment> fragmentGOs = new List<Fragment>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        allFragments.Clear();
    }

    public void SpawnFragmentInScene(FragmentData fragment)
    {
        GameObject fragmentObject = Instantiate(fragmentPrefab, fragment.fragmentPosition, Quaternion.identity, GameObject.Find("===Fragments===").transform);
        fragmentObject.name = fragment.fragmentName;
        Fragment fragmentComponent = fragmentObject.GetComponent<Fragment>();
        fragmentComponent.SetFragmentData(fragment);
        allFragments.Add(fragment);
        fragmentGOs.Add(fragmentComponent);
    }

    public void AddFragment(Fragment fragment)
    {
        if (!FragmentOwned(fragment))
            currentFragments.Add(fragment.GetFragmentData());
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

    public void UpdateFragmentState(FragmentData fragData)
    {
        Fragment fragmentGO = fragmentGOs.Find(x => x.GetFragmentData() == fragData);
        if (fragmentGO != null)
        {
            fragmentGO.gameObject.SetActive(ObjectiveManager.Instance.CheckIfFragmentValid(fragData));
        }
    }

    public GameObject GetFragmentGO(int fragIdx)
    {
        return fragmentGOs[fragIdx].gameObject;
    }
}
