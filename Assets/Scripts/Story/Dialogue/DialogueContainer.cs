using System.Collections;
using TMPro;
using UnityEngine;

namespace Dialogue
{
    [System.Serializable]
    public class DialogueContainer
    {
        private const float fadeSpeed = 5f;

        [Header("Components")]
        public GameObject dialoguePanel, namePanel;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI dialogueText;
        private CanvasGroup canvasGroup => dialoguePanel.GetComponent<CanvasGroup>();

        [Header("Coroutines")]
        private Coroutine showingCo = null;
        private Coroutine hidingCo = null;

        public bool isShowing => showingCo != null;
        public bool isHiding => hidingCo != null;
        public bool isFading => isShowing || isHiding;

        public void ShowName(string speakerName = "")
        {
            namePanel.SetActive(true);

            if (speakerName != string.Empty)
            {
                nameText.text = speakerName;
            }
        }

        public void HideName()
        {
            namePanel.SetActive(false);
        }

        public Coroutine ShowDialogue()
        {
            if (isShowing)
                return null;
            else if (isHiding)
            {
                DialogueSystem.Instance.StopCoroutine(hidingCo);
                hidingCo = null;
            }

            showingCo = DialogueSystem.Instance.StartCoroutine(Fading(1));

            return showingCo;
        }

        public Coroutine HideDialogue()
        {
            if (isHiding)
                return null;
            else if (isShowing)
            {
                DialogueSystem.Instance.StopCoroutine(showingCo);
                showingCo = null;
            }

            hidingCo = DialogueSystem.Instance.StartCoroutine(Fading(0));

            return hidingCo;
        }

        private IEnumerator Fading(float targetOpac)
        {
            CanvasGroup cg = canvasGroup;
            while (cg.alpha != targetOpac)
            {
                cg.alpha = Mathf.MoveTowards(cg.alpha, targetOpac, Time.deltaTime * fadeSpeed);
                yield return null;
            }

            showingCo = null;
            hidingCo = null;
        }
    }
}
