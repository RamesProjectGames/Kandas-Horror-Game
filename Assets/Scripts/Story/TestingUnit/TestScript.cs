using UnityEngine;
using Dialogue;
using System.Collections.Generic;
using System.Linq;

namespace TestingPurposes
{
    public class TestScript : MonoBehaviour
    {
        DialogueSystem ds;
        TextArchitect archi;
        public TextArchitect.BuildMethod buildMethod = TextArchitect.BuildMethod.typewriter;

        string[] testLines = new string[5]
        {
            "Alyssa \"I'm in the mood for some pousea\" Pousea()",
            "May \"You mean Poutine?\" Poutine()",
            "Alyssa \"Did I stutter?\"",
            "Whatsapp \"Address me\" Wine()",
            "Addressed()"
        };
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            ds = DialogueSystem.Instance;
            archi = new TextArchitect(ds.dialogueContainer.dialogueText);
            //archi.buildMethod = buildMethod;
            //archi.speed = .5f;

            //Test with script innate "lines"
            //List<string> lines = testLines.ToList();


            //Test with external txt file (resource asset)
            List<string> lines = FileReader.ReadAsset("Dialogue");

            //Read each line and debug it
            //foreach (string line in lines)
            //{
            //    DialogueStructure dl = DialogueParser.Parse(line);
            //}

            DialogueSystem.Instance.Say(lines);
        }

        // Update is called once per frame
        void Update()
        {
            if (buildMethod != archi.buildMethod)
            {
                archi.buildMethod = buildMethod;
                archi.StopBuildingText();
            }
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Z))
            {
                DialogueSystem.Instance.OnUserPrompt();
            }
            //else if (Input.GetKeyDown(KeyCode.A))
            //{
            //    if (archi.isBuilding)
            //    {
            //        if (archi.speedUp)
            //            archi.ForceComplete();
            //        else
            //            archi.speedUp = true;
            //    }
            //    else
            //    {
            //        archi.Append(testLines[Random.Range(0, testLines.Length)]);
            //    }
            //}
        }
    }
}