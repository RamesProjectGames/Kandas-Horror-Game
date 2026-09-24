using System.Collections.Generic;
using UnityEngine;

public class AnimationEventBehaviour : StateMachineBehaviour
{
    public List<AnimationEventData> events = new();

    private readonly HashSet<int> triggeredEvents = new();

    public override void OnStateEnter(
        Animator animator,
        AnimatorStateInfo stateInfo,
        int layerIndex)
    {
        triggeredEvents.Clear();
    }

    public override void OnStateUpdate(
        Animator animator,
        AnimatorStateInfo stateInfo,
        int layerIndex)
    {
        float normalizedTime = stateInfo.normalizedTime % 1f;

        for (int i = 0; i < events.Count; i++)
        {
            if (triggeredEvents.Contains(i))
                continue;

            AnimationEventData animationEvent = events[i];

            if (normalizedTime >= animationEvent.eventTime)
            {
                triggeredEvents.Add(i);

                AnimationEventReceiver receiver =
                    animator.GetComponent<AnimationEventReceiver>();

                if (receiver != null)
                {
                    receiver.Trigger(animationEvent.eventId);
                }
            }
        }
    }
}