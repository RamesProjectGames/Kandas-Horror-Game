using System.Collections.Generic;
using UnityEngine;

public class AnimationEventReceiver : MonoBehaviour
{
    [SerializeField]
    private List<AnimationEventTarget> targets = new();

    private Dictionary<string, AnimationEventTarget> targetLookup;

    private void Awake()
    {
        BuildLookup();
    }

    private void BuildLookup()
    {
        targetLookup = new Dictionary<string, AnimationEventTarget>();

        foreach (var target in targets)
        {
            if (target == null)
                continue;

            if (string.IsNullOrEmpty(target.eventId))
                continue;

            targetLookup[target.eventId] = target;
        }
    }

    public void Trigger(string eventId)
    {
        if (targetLookup.TryGetValue(eventId, out var target))
        {
            target.Invoke();
        }
        else
        {
            Debug.LogWarning(
                $"Animation event '{eventId}' has no target.",
                this
            );
        }
    }
}