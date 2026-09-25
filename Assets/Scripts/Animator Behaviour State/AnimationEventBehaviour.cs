using System.Collections.Generic;
using UnityEngine;

public class AnimationEventBehaviour : StateMachineBehaviour
{ 
    public List<AnimationEventData> events = new();

    private readonly HashSet<int> triggeredEvents = new();
    private int lastLoopCount = -1;
    private AnimationEventReceiver receiver;

    public override void OnStateEnter(
        Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        triggeredEvents.Clear();
        lastLoopCount = Mathf.FloorToInt(stateInfo.normalizedTime);
        receiver = animator.GetComponent<AnimationEventReceiver>();
    }

    public override void OnStateUpdate(
        Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        int loopCount = Mathf.FloorToInt(stateInfo.normalizedTime);
        float normalizedTime = stateInfo.normalizedTime - loopCount;

        // Loop boundary crossed -> new pass, reset fired flags
        if (loopCount != lastLoopCount)
        {
            triggeredEvents.Clear();
            lastLoopCount = loopCount;
        }

        if (receiver == null) return;

        for (int i = 0; i < events.Count; i++)
        {
            if (triggeredEvents.Contains(i)) continue;

            var animationEvent = events[i];
            if (normalizedTime >= animationEvent.eventTime)
            {
                triggeredEvents.Add(i);
                receiver.Trigger(animationEvent.eventId);
            }
        }
    }

    public override void OnStateExit(
        Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        triggeredEvents.Clear();
        lastLoopCount = -1;
        receiver = null;
    }
}