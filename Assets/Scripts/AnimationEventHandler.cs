using UnityEngine;
using UnityEngine.Events;
[RequireComponent(typeof(Animator))]
public class AnimationEventHandler : MonoBehaviour
{
    public UnityEvent OnAnimationEvent;
    public void TriggerAnimationEvent()
    {
        if (OnAnimationEvent != null)
        {
            OnAnimationEvent.Invoke();
        }
    }
}
