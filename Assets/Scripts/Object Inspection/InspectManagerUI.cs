using TMPro;
using UnityEngine;

public class InspectManagerUI : MonoBehaviour
{
    public static InspectManagerUI Instance;
    public GameObject InspectObjectUI;
    public TextMeshProUGUI itemTitle;
    public TextMeshProUGUI  itemDescription;
    public Camera lookAtCamera;
    public Transform currentInpectObject;
    void Awake()
    {
        Instance = this;
    }
    public void InspectUI(bool open)
    {
        SettingManager.Instance.isPaused = open;
        InspectObjectUI.SetActive(open);
        if(open)
        {
            Rigidbody objectRb = currentInpectObject.GetComponent<Rigidbody>();
            if(objectRb != null) Destroy(objectRb);
        }
    }
    public void OnItemSelected(GameObject itemPrefab)
    {
        if (Instance.currentInpectObject != null)
        {
            Destroy(Instance.currentInpectObject.gameObject);
        }
        Instance.currentInpectObject = Instantiate(itemPrefab, new Vector3(1000, 1000, 1000), Quaternion.identity).transform;
        itemTitle.text = Instance.currentInpectObject.GetComponent<InspectData>().itemTitle;
        itemDescription.text = Instance.currentInpectObject.GetComponent<InspectData>().itemDescription;
        currentInpectObject.LookAt(lookAtCamera.transform);
        Instance.InspectUI(true);
    }
    public void OnItemDeselect()
    {
        if (Instance.currentInpectObject != null)
        {
            Destroy(Instance.currentInpectObject.gameObject);
        }
        Instance.InspectUI(false);
        itemTitle.text = "";
        itemDescription.text = "";
    }
}
