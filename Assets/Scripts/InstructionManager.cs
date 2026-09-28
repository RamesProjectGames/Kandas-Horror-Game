using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class InstructionManager : MonoBehaviour
{
    public static InstructionManager Instance;
    public InputActionReference nextInstructionAction, prevInstructionAction; 
    public TMP_Text instructionPanel;
    public List<string> instructions;
    int currentInstructionIndex = 0;
    string nextInstructionText;
    string prevInstructionText;
    private void Awake()
    {
        Instance = this;
        if(instructions == null)
        {
            instructions = new List<string>();
        }
        nextInstructionText = $"Press {nextInstructionAction.action.GetBindingDisplayString(0)} to go to the next instruction";
        prevInstructionText = $"Press {prevInstructionAction.action.GetBindingDisplayString(0)} to go to the previous instruction";
    }
    // Update is called once per frame
    void Update()
    {
        if(instructions != null && instructions.Count > 0)
        {
            instructionPanel.transform.parent.gameObject.SetActive(true);
            if(nextInstructionAction != null && nextInstructionAction.action.triggered)
            {
                currentInstructionIndex++;
                if(currentInstructionIndex >= instructions.Count)
                {
                    currentInstructionIndex = 0;
                }
            }
            if(prevInstructionAction != null && prevInstructionAction.action.triggered)
            {
                currentInstructionIndex--;
                if(currentInstructionIndex < 0)
                    currentInstructionIndex = instructions.Count - 1;
            }
            if(currentInstructionIndex >= instructions.Count)
            {
                currentInstructionIndex = 0;
            }
            else if(currentInstructionIndex < 0)
            {
                currentInstructionIndex = instructions.Count - 1;
            }
            instructionPanel.text = instructions[currentInstructionIndex];
        }
        else
        {
            instructionPanel.transform.parent.gameObject.SetActive(false);
        }
    }
    public void AddInstruction(string instruction)
    {
        if(string.IsNullOrWhiteSpace(instruction))
        {
            return;
        }
        if(!instructions.Exists(x=>IsSame(x, nextInstructionText)))
        {
            instructions.Insert(0, nextInstructionText);
            currentInstructionIndex++;
        }
        if(!instructions.Exists(x=>IsSame(x, prevInstructionText)))
        {
            instructions.Add(prevInstructionText);
        }
        if(instructions.Exists(x=>IsSame(x, instruction)))
            return;
        instructions.Insert(instructions.Count - 1, instruction);
    }
    public void RemoveInstruction(string instruction)
    {
        if(!instructions.Exists(x=>IsSame(x, instruction)))
        {
            return;
        }
        instructions.RemoveAll(x=>IsSame(x, instruction));

        if(!HasContentInstructions())
        {
            ClearInstructions();
        }
    }
    public void ClearInstructions()
    {
        if(instructions != null)
        {
            instructions.Clear();
        }
        currentInstructionIndex = 0;
    }
    private bool HasContentInstructions()
    {
        return instructions.Exists(x=>!IsNavigationText(x));
    }
    private bool IsNavigationText(string text)
    {
        return IsSame(text, nextInstructionText) || IsSame(text, prevInstructionText);
    }
    private static bool IsSame(string a, string b)
    {
        return string.Compare(a?.ToLowerInvariant(), b?.ToLowerInvariant(), true, System.Globalization.CultureInfo.InvariantCulture) == 0;
    }
}
