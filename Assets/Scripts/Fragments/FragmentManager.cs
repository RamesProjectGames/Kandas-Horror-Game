using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class FragmentManager : MonoBehaviour
{
    public static FragmentManager Instance;
    public GameObject fragmentPrefab;
    public List<FragmentData> allFragments = new List<FragmentData>();
    List<Fragment> currentFragments = new List<Fragment>();

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
    }

    public void SpawnFragmentInScene()
    {
        foreach (FragmentData fragment in allFragments)
        {
            GameObject fragmentObject = Instantiate(fragmentPrefab, Vector3.zero, Quaternion.identity, transform);
            Fragment fragmentComponent = fragmentObject.GetComponent<Fragment>();
            fragmentComponent.SetFragment(fragment);
        }
    }

    public void AddFragment(Fragment fragment)
    {
        currentFragments.Add(fragment);
    }
    public void RemoveFragment(Fragment fragment)
    {
        currentFragments.Remove(fragment);
    }
    public void ClearFragment()
    {
        currentFragments.Clear();
    }
    public bool CheckCompletedFragments()
    {
        for (int i = 0; i < allFragments.Count; i++)
        {
            if(currentFragments.Find(x => x.GetFragmentName() == allFragments[i].fragmentName) == null) return false;
        }
        return true;
    }
}
