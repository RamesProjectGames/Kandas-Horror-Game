using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class Fragment : MonoBehaviour
{
    public string fragmentName;
    public Transform itemParent;
    public Color fragmentColor = new Color(1f,1f,1f,1f);
    public UnityEvent onFragmentPickup;
    public void SetItemObject(GameObject itemPrefab)
    {
        var itemSpawnPrefab = Instantiate(itemPrefab, itemParent);
    }
    void OnEnable()
    {
        FragmentManager.Instance.RemoveFragment(this);
        gameObject.SetActive(true);
    }
    public void OnFragmentPickup()
    {
        FragmentManager.Instance.AddFragment(this);
        onFragmentPickup?.Invoke();
        gameObject.SetActive(false);
    }
}
