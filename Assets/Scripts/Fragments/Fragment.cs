using Dialogue;
using UnityEngine;

public class Fragment : MonoBehaviour
{
    FragmentData fragmentData;
    public Transform itemParent;
    public Color fragmentColor = new Color(1f,1f,1f,1f);
    public void SetFragment(FragmentData fragData)
    {
        fragmentData = fragData;
    }

    public string GetFragmentName()
    {
        return fragmentData.name;
    }

    void OnEnable()
    {
        FragmentManager.Instance.RemoveFragment(this);
        gameObject.SetActive(true);
    }
    public void OnFragmentPickup()
    {
        if(!FragmentManager.Instance.FragmentOwned(this))
            FragmentManager.Instance.AddFragment(this);
        Cutscene();
        gameObject.SetActive(false);
    }

    public void Cutscene()
    {
        if (ObjectiveManager.Instance == null) return;

        ObjectiveManager.Instance.CompleteObjective(fragmentData.fragmentName);
        DialogueSystem.Instance.OpenDialogue(fragmentData.fragmentName);
    }
}
