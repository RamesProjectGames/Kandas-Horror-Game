using UnityEngine;
using UnityEngine.Events;

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
