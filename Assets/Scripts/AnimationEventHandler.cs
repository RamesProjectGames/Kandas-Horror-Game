using UnityEngine;
using UnityEngine.Events;
[RequireComponent(typeof(Animator))]
public class AnimationEventHandler : MonoBehaviour
{
    public UnityEvent OnAnimationEnded;
    public UnityEvent OnAnimationStart;
    public void TriggerAnimationEnded()
    {
        if (OnAnimationEnded != null)
        {
            OnAnimationEnded.Invoke();
        }
    }
    public void TriggerAnimationStart()
    {
        if(OnAnimationStart != null)
        {
            OnAnimationStart.Invoke();
        }
    }
}
