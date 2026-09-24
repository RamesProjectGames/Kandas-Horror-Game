using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class AnimationEventTarget
{
    public string eventId;

    public UnityEvent onEvent;

    public void Invoke()
    {
        onEvent?.Invoke();
    }
}