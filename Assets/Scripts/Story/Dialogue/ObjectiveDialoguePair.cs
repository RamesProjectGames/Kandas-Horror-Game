using System;
using UnityEngine;

namespace Dialogue
{
    [Serializable]
    public class ObjectiveDialoguePair
    {
        [Tooltip("If There are no Objectives, it'd be considered default convo")]
        [SerializeField] public string[] objective;
        [SerializeField] public string dialogueAsset;
    }
}