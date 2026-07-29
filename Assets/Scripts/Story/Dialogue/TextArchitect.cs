using FMODUnity;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Dialogue
{
    public class TextArchitect
    {
        [Tooltip("For UI usage")]
        private TextMeshProUGUI tmpro_ui;
        [Tooltip("For world appearing texts")]
        private TextMeshPro tmpro_world;

        private char[] vowels = {'a', 'i', 'u', 'e', 'o', 'A', 'I', 'U', 'E', 'O'};

        public TMP_Text tmpro => tmpro_ui ? tmpro_ui : tmpro_world;

        public string currentText => tmpro_ui.text;
        public string targetText { get; private set; } = "";
        public string preText { get; private set; } = "";
        private int preTextLength = 0;

        private string fullTargetText => preText + targetText;

        public enum BuildMethod { instant, typewriter, fade };
        public BuildMethod buildMethod = BuildMethod.instant;

        public Color textColor { get { return tmpro.color; } set { tmpro.color = value; } }

        public float speed { get { return baseSpeed * speedMulti; } set { speedMulti = value; } }

        private const float baseSpeed = 1;
        private float speedMulti = 1;

        public int charPerCycle { get { return speed <= 2f ? charMulti : speed <= 2.5f ? charMulti * 2 : charMulti * 3; } }
        private int charMulti = 1;

        public bool speedUp = false;

        public TextArchitect(TextMeshProUGUI tmpro_ui)
        {
            this.tmpro_ui = tmpro_ui;
        }

        public TextArchitect(TextMeshPro tmpro_world)
        {
            this.tmpro_world = tmpro_world;
        }

        //Form New Dialogue
        public Coroutine Build(string text)
        {
            preText = "";
            targetText = text;

            StopBuildingText();

            buildProcess = tmpro.StartCoroutine(StartBuildingText());

            return buildProcess;
        }

        //Append next line to existing dialogue
        public Coroutine Append(string text)
        {
            preText = tmpro.text;
            targetText = text;

            StopBuildingText();

            buildProcess = tmpro.StartCoroutine(StartBuildingText());

            return buildProcess;
        }

        private Coroutine buildProcess = null;
        public bool isBuilding => buildProcess != null;

        //Halt visual building
        public void StopBuildingText()
        {
            if (!isBuilding)
                return;

            tmpro.StopCoroutine(buildProcess);
            buildProcess = null;
        }

        //Build Text visually
        IEnumerator StartBuildingText()
        {
            Prepare();

            switch (buildMethod)
            {
                //case BuildMethod.instant:
                //    yield return BuildInstant();
                //    break;
                case BuildMethod.typewriter:
                    yield return BuildTypewriter();
                    break;
                case BuildMethod.fade:
                    yield return BuildFade();
                    break;
            }
        }

        private void OnComplete()
        {
            buildProcess = null;
            speedUp = false;
        }

        //Force Complete Text
        public void ForceComplete()
        {
            switch (buildMethod)
            {
                case BuildMethod.typewriter:
                    tmpro.maxVisibleCharacters = tmpro.textInfo.characterCount;
                    break;
                case BuildMethod.fade:
                    tmpro.ForceMeshUpdate();
                    break;
            }

            StopBuildingText();
            OnComplete();
        }

        //Handling text build preparation
        private void Prepare()
        {
            switch (buildMethod)
            {
                case BuildMethod.instant:
                    PrepareInstant();
                    break;
                case BuildMethod.typewriter:
                    PrepareTypewriter();
                    break;
                case BuildMethod.fade:
                    PrepareFade();
                    break;
            }
        }

        private void PrepareInstant()
        {
            tmpro.color = tmpro.color;
            tmpro.text = fullTargetText;
            tmpro.ForceMeshUpdate();
            tmpro.maxVisibleCharacters = tmpro.textInfo.characterCount;
        }

        private void PrepareTypewriter()
        {
            tmpro.color = tmpro.color;
            tmpro.maxVisibleCharacters = 0;
            tmpro.text = preText;
            if (preText != "")
            {
                tmpro.ForceMeshUpdate();
                tmpro.maxVisibleCharacters = tmpro.textInfo.characterCount;
            }
            tmpro.text += targetText;
            tmpro.ForceMeshUpdate();
        }

        private void PrepareFade()
        {
            tmpro.text = preText;
            if (preText != "")
            {
                tmpro.ForceMeshUpdate();
                preTextLength = tmpro.textInfo.characterCount;
            }
            else
            {
                preTextLength = 0;
            }

            tmpro.text += targetText;
            tmpro.maxVisibleCharacters = int.MaxValue;
            tmpro.ForceMeshUpdate();

            TMP_TextInfo textInfo = tmpro.textInfo;

            Color hidden = new Color(textColor.r, textColor.g, textColor.b, 0);
            Color visible = new Color(textColor.r, textColor.g, textColor.b, 1);

            Color32[] vertexColors = textInfo.meshInfo[textInfo.characterInfo[0].materialReferenceIndex].colors32;

            for (int i = 0; i < textInfo.characterCount; i++)
            {
                TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

                if (charInfo.isVisible)
                {
                    if (i < preTextLength)
                    {
                        for (int v = 0; v < 4; v++)
                        {
                            vertexColors[charInfo.vertexIndex + v] = visible;
                        }
                    }
                    else
                    {
                        for (int v = 0; v < 4; v++)
                        {
                            vertexColors[charInfo.vertexIndex + v] = hidden;
                        }
                    }
                }
            }

            tmpro.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
        }

        private IEnumerator BuildTypewriter()
        {
            while (tmpro.maxVisibleCharacters < tmpro.textInfo.characterCount)
            {
                if(DialogueSystem.Instance.dialogueContainer.active)
                {
                    if(vowels.Contains(tmpro.textInfo.characterInfo[tmpro.maxVisibleCharacters].character))
                        if (tmpro.maxVisibleCharacters - 1 > 0 && !vowels.Contains(tmpro.textInfo.characterInfo[tmpro.maxVisibleCharacters - 1].character))
                            AudioManager.Instance.PlayOneShot3D(RuntimeManager.PathToEventReference("event:/Voice/DialogueVoice"), .33f, 1, GameObject.Find("Player").transform.position);
                    tmpro.maxVisibleCharacters += speedUp ? charPerCycle * 5 : charPerCycle;
                    yield return new WaitForSeconds(.015f / speed);
                }
                else
                    yield return null;
            }

            StopBuildingText();
            OnComplete();
        }

        private IEnumerator BuildFade()
        {
            int minRange = preTextLength;
            int maxRange = minRange + 1;

            byte alphaThreshold = 15;

            TMP_TextInfo textInfo = tmpro.textInfo;

            Color32[] vertexColors = textInfo.meshInfo[textInfo.characterInfo[0].materialReferenceIndex].colors32;
            float[] alphas = new float[textInfo.characterCount];

            while (true)
            {
                if (DialogueSystem.Instance.dialogueContainer.active)
                {
                    float fadeSpeed = (speedUp ? charPerCycle * 5 : charPerCycle) * speed * 4f;
                    for (int i = 0; i < maxRange; i++)
                    {
                        TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

                        if (charInfo.isVisible)
                        {
                            int vertexIndex = textInfo.characterInfo[i].vertexIndex;
                            alphas[i] = Mathf.MoveTowards(alphas[i], 255, fadeSpeed);

                            for (int v = 0; v < 4; v++)
                            {
                                vertexColors[charInfo.vertexIndex + v].a = (byte)alphas[i];
                            }

                            if (alphas[i] >= 255)
                                minRange++;
                        }
                    }

                    tmpro.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

                    bool lastCharInvisible = !textInfo.characterInfo[maxRange - 1].isVisible;
                    if (alphas[maxRange - 1] > alphaThreshold || lastCharInvisible)
                    {
                        if (vowels.Contains(textInfo.characterInfo[maxRange].character))
                            AudioManager.Instance.PlayOneShot3D(RuntimeManager.PathToEventReference("event:/Voice/DialogueVoice"), .33f, 1, GameObject.Find("Player").transform.position);
                        if (maxRange < textInfo.characterCount)
                        {
                            maxRange++;
                        }
                        else if (alphas[maxRange - 1] >= 255 || lastCharInvisible)
                        {
                            break;
                        }
                    }
                }

                yield return new WaitForEndOfFrame();
            }

            StopBuildingText();
            OnComplete();
        }
    }
}