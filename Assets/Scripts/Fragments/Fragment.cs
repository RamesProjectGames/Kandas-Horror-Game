using Dialogue;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class Fragment : ItemInteraction
{
    FragmentData fragmentData;
    public Transform itemParent;
    public Color fragmentColor = new Color(1f,1f,1f,1f);
    public void SetFragmentData(FragmentData fragData)
    {
        fragmentData = fragData;
    }

    public FragmentData GetFragmentData()
    {
        return fragmentData;
    }    

    public string GetFragmentName()
    {
        return fragmentData.fragmentName;
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
    }

    public void Cutscene()
    {
        if (ObjectiveManager.Instance == null) return;

        ObjectiveManager.Instance.CompleteObjective(fragmentData.fragmentName);
        DialogueSystem.Instance.OpenDialogue(fragmentData.fragmentName);
    }
}
