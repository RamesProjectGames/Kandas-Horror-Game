using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Dialogue
{
    public class DialogicManager
    {
        private DialogueSystem dialogueSystem => DialogueSystem.Instance;
        public List<IDialogic> logicLines = new List<IDialogic>();

        public DialogicManager() => LoadLogicalLines();

        private void LoadLogicalLines()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Type[] lineTypes = assembly.GetTypes().Where(t => typeof(IDialogic).IsAssignableFrom(t) && !t.IsInterface).ToArray();

            foreach (var lineType in lineTypes)
            {
                IDialogic dialogic = (IDialogic) Activator.CreateInstance(lineType);
                logicLines.Add(dialogic);
            }
        }

        public bool TryGetLogic(DialogueStructure line, out Coroutine logic)
        {
            foreach (var logicLine in logicLines)
            {
                if (logicLine.Matches(line))
                {
                    logic = dialogueSystem.StartCoroutine(logicLine.Execute(line));
                    return true;
                }
            }
            logic = null;
            return false;
        }
    }
}
