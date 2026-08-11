using UnityEngine;

public class AnimationTrigger : MonoBehaviour
{
    public bool triggerOnStart = false;
    public void TriggerAnimation(string triggerName)
    {
        Animator animator = GetComponent<Animator>();
        triggerOnStart = !triggerOnStart;
        if (animator != null)
        {
            animator.SetBool(triggerName, triggerOnStart);
        }
    }
}
