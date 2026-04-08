using System.Collections;
using UnityEngine;

namespace Dialogue
{
    public interface IDialogic
    {
        string keyword { get; }

        bool Matches(DialogueStructure line);

        IEnumerator Execute(DialogueStructure line);
    }
}
