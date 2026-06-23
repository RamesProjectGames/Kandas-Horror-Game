using UnityEngine;
using UnityEngine.Events;
[CreateAssetMenu(fileName = "FragmentData")]
public class FragmentData : ScriptableObject
{
    public GameObject fragmentPrefab;
    public string fragmentName;
    public string fragmentItemName, fragmentItemDetails;
    public Vector3 fragmentPosition;
}
