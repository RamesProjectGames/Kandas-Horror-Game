using UnityEngine;
using UnityEngine.Events;
[CreateAssetMenu(fileName = "FragmentData")]
public class FragmentData : ScriptableObject
{
    public string fragmentName;
    public GameObject itemPrefab;
    public UnityEvent onFragmentPickup;

}
