using TMPro;
using UnityEngine;

public class DialoguePrompt : MonoBehaviour
{
    private RectTransform root;
    [SerializeField] TextMeshProUGUI dialogueText;
    [SerializeField] Animator anim;

    public bool isVisible => anim.gameObject.activeSelf;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        root = GetComponent<RectTransform>();
    }

    public void Show()
    {
        if(dialogueText.text == string.Empty)
        {
            if(isVisible)
                Hide();

            return;
        }

        dialogueText.ForceMeshUpdate();

        anim.gameObject.SetActive(true);
        root.transform.SetParent(dialogueText.transform);

        TMP_CharacterInfo finalChar = dialogueText.textInfo.characterInfo[dialogueText.textInfo.characterCount - 1];
        float charWidth = finalChar.pointSize * .5f;

        Vector3 targetPos = new Vector3(finalChar.bottomRight.x + charWidth, (finalChar.bottomRight.y + finalChar.topRight.y)/2f, 0);

        root.localPosition = targetPos;
    }

    public void Hide()
    {
        anim.gameObject.SetActive(false);
    }
}
