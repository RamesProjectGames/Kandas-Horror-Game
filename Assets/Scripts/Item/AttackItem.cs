using Dialogue;
using FMOD.Studio;
using FMODUnity;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class AttackItem : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private ParticleSystem hitEffect;
    [SerializeField] private Animator animator;

    [Header("Audio")]
    [SerializeField] private EventReference attackSound;
    private EventInstance attackSoundEvent;

    [Header("Interaction")]
    [SerializeField] private InputActionReference interactAction;
    [SerializeField] private TMP_Text interactionText;
    private bool itemIsHeld = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        hitEffect.Stop();
        animator.Play("Idle");
        attackSoundEvent = AudioManager.Instance.CreateInstance(attackSound);
        RuntimeManager.AttachInstanceToGameObject(attackSoundEvent, gameObject, false);
    }
    void Update()
    {
        if (SettingManager.Instance.isPaused || DialogueSystem.Instance.isRunningConvo)
            return;
        if (interactAction != null && interactAction.action.WasPerformedThisFrame())
        {
            PerformAttack();
        }
    }
    public void SetHeldState(bool isHeld)
    {
        itemIsHeld = isHeld;
        if (interactionText != null)
        {
            interactionText.gameObject.SetActive(isHeld);
            interactionText.text = isHeld ? $"Press {interactAction.action.GetBindingDisplayString(0)} to Attack" : "";
        }
    }
    public void PerformAttack()
    {
        if(!itemIsHeld)
            return; // Only allow attacking if the item is currently held by the player
        // Play attack animation
        if (animator != null)
        {
            if(animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
                return; // Prevent spamming attack while already attacking
            animator.Play("Attack");
            PlayAttackSound();
        }
    }
    public void PlayEffect(Vector3 position = default)
    {
        StopAttackSound();
        if (hitEffect != null)
        {
            hitEffect.transform.position = position;
            hitEffect.Play();
        }
    }
   public void PlayAttackSound()
    {
        PLAYBACK_STATE playbackState;
        attackSoundEvent.getPlaybackState(out playbackState);
        if (playbackState != PLAYBACK_STATE.PLAYING)
        {
            RuntimeManager.AttachInstanceToGameObject(attackSoundEvent, gameObject, false);
            attackSoundEvent.start();
        }
    }
    public void StopAttackSound()
    {
        attackSoundEvent.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
    }
    void OnCollisionEnter(Collision collision)
    {
        PlayEffect(collision.contacts[0].point);
    }
}
