using System.Collections;
using UnityEngine;

namespace Dialogue
{
    public interface IDialogic
    {
        string keyword { get; }

        bool Matches(DialogueStructure structure);

        IEnumerator Execute(DialogueStructure structure);
    }
}
