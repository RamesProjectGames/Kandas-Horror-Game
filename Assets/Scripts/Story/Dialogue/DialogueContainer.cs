using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Dialogue
{
    [System.Serializable]
    public class DialogueContainer
    {
        [Header("Components")]
        public GameObject dialoguePanel, namePanel;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI dialogueText;
        public Image screenCover;

        private CanvasGroup canvasGroup => dialoguePanel.GetComponent<CanvasGroup>();

        [Header("Canvas Group Controller")]
        private const float fadeSpeed = 5f;

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
            float screenOpac = 1f;
            float elapsedTime = 0f;
            active = false;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float percentage = elapsedTime / duration;

                Color scColor = screenCover.color;
                scColor.a = Mathf.Lerp(scColor.a, screenOpac, percentage);
                screenCover.color = scColor;
                yield return null;
            }
            DialogueSystem.Instance.screenCo = null;
        }

        public IEnumerator FadeFromBlack(float duration)
        {
            float screenOpac = 0f;
            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float percentage = elapsedTime / duration;
                if(percentage >= 0.1f)
                    if(!active)
                        active = true;
                Color scColor = screenCover.color;
                scColor.a = Mathf.Lerp(scColor.a, screenOpac, percentage);
                screenCover.color = scColor;
                yield return null;
            }
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

            active = false;
            hidingCo = DialogueSystem.Instance.StartCoroutine(Fading(0));

            return hidingCo;
        }

        private IEnumerator Fading(float targetOpac)
        {
            CanvasGroup cg = canvasGroup;
            while (cg.alpha != targetOpac)
            {
                cg.alpha = Mathf.MoveTowards(cg.alpha, targetOpac, Time.deltaTime * fadeSpeed);
                cg.blocksRaycasts = targetOpac > 0f ? true : false;
                yield return null;
            }

            showingCo = null;
            hidingCo = null;
        }
    }
}
