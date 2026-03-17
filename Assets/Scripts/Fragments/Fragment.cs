using Dialogue;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class Fragment : MonoBehaviour
{
    FragmentData fragmentData;
    public Transform itemParent;
    public Color fragmentColor = new Color(1f,1f,1f,1f);
    public UnityEvent onFragmentPickup;
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
        FragmentManager.Instance.AddFragment(this);
        onFragmentPickup?.Invoke();
        gameObject.SetActive(false);
    }

    public void Cutscene()
    {
        if (ObjectiveManager.Instance == null) return;

        ObjectiveManager.Instance.CompleteObjective(fragmentData.fragmentName);
        DialogueSystem.Instance.OpenDialogue(fragmentData.fragmentName);
    }
}
