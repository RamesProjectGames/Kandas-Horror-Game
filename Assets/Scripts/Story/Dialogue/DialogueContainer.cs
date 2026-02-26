using TMPro;
using UnityEngine;

namespace Dialogue
{
    [System.Serializable]
    public class DialogueContainer
    {
        public GameObject dialoguePanel, namePanel;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI dialogueText;
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
    }
}
