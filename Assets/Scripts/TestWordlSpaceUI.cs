using UnityEngine;
using UnityEngine.UI;

public class TestWordlSpaceUI : MonoBehaviour
{
    [Header("Reference")]
    public Canvas canvas;
    public Button button;

    [Header("Behavior")]
    public Transform player;
    public Camera uiCamera;
    void Start() {      
    }
    void OnEnable() {
        if(AsyncSceneLoader.Instance == null)
        {
            Debug.LogError("[TestWordlSpaceUI] AsyncSceneLoader instance missing.", this);
            SetupWorldSpaceUI();
            return;
        }
        else
        {

            AsyncSceneLoader.Instance.Completed += SetupWorldSpaceUI;
        }
    }

    private void SetupWorldSpaceUI()
    {
        if (!canvas)
        {
            Debug.LogError("[EnemyEmbeddedUI] Canvas reference missing.", this);
            return;
        }

        if (!player) player = GameObject.FindGameObjectWithTag("Player").transform;
        if (!uiCamera) uiCamera = Camera.main;
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = uiCamera;
        // canvas.gameObject.SetActive(false);

        if (button)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => Debug.Log($"[{name}] Click me"));
        }
    }

    void OnDisable() {}
}
