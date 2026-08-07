using Dialogue;
using UnityEngine;

public class LocalCutSceneManager : MonoBehaviour
{
    public void FadeOut(float duration = 1f)
    {
        if(DialogueSystem.Instance != null)
        {
            StartCoroutine(DialogueSystem.Instance.FadeToBlack(duration));
        }
    }
    public void FadeIn(float duration = 1f)
    {
        if(DialogueSystem.Instance != null)
        {
            StartCoroutine(DialogueSystem.Instance.FadeFromBlack(duration));
        }
    }
}
