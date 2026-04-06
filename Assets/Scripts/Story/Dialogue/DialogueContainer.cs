using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
        public Image screenCover;

        private CanvasGroup canvasGroup => dialoguePanel.GetComponent<CanvasGroup>();

        [Header("Coroutines")]
        private Coroutine showingCo = null;
        private Coroutine hidingCo = null;

        public bool active = false;
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

        public IEnumerator FadeToBlack(float duration)
        {
            float targetOpac = 1f;
            float elapsedTime = 0f;
            CanvasGroup cg = canvasGroup;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float percentage = elapsedTime / duration;

                cg.alpha = Mathf.Lerp(cg.alpha, targetOpac, percentage);
                Color scColor = screenCover.color;
                scColor.a = Mathf.Lerp(scColor.a, targetOpac, percentage);
                screenCover.color = scColor;
                yield return null;
            }
            active = false;
            DialogueSystem.Instance.screenCo = null;
        }

        public IEnumerator FadeFromBlack(float duration)
        {
            float targetOpac = 0f;
            float elapsedTime = 0f;
            CanvasGroup cg = canvasGroup;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float percentage = elapsedTime / duration;

                cg.alpha = Mathf.Lerp(cg.alpha, targetOpac, percentage);
                Color scColor = screenCover.color;
                scColor.a = Mathf.Lerp(scColor.a, targetOpac, percentage);
                screenCover.color = scColor;
                yield return null;
            }
            active = true;
            DialogueSystem.Instance.screenCo = null;
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
            active = true;

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
            active = false;

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
