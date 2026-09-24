using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class AnimationEventData
{
    public string eventId;

    [Range(0f, 1f)]
    public float eventTime;

}