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
    [SerializeField] private float waitToAttack = 1.5f;
    private float attackTimer = 0f;

    [Header("Audio")]
    [SerializeField] private EventReference attackSound;
    private EventInstance attackSoundEvent;

    [Header("Interaction")]
    [SerializeField] private InputActionReference interactAction;
    private bool itemIsHeld = false;

    [Header("Animation")]
    [SerializeField] private string idleAnimationNotHeld = "Idle";
    [SerializeField] private string idleAnimationHeld = "BatIdle";
    [SerializeField] private string attackAnimation = "Attack";
    private bool isAttacking = false;


    private PlayerGrabInteraction playerGrabInteraction;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        hitEffect = Instantiate(hitEffect, transform.position, Quaternion.identity, transform.parent);
        hitEffect.Stop();
        attackSoundEvent = AudioManager.Instance.CreateInstance(attackSound);
        RuntimeManager.AttachInstanceToGameObject(attackSoundEvent, gameObject, false);
        playerGrabInteraction = FindFirstObjectByType<PlayerGrabInteraction>(FindObjectsInactive.Include);
    }
    void Update()
    {
        if (SettingManager.Instance.isPaused || DialogueSystem.Instance.isRunningConvo)
            return;

        if(attackTimer > 0f) 
        {
            attackTimer -= Time.deltaTime;
            return;
        }
        // Check if attack animation has finished
        if (isAttacking && animator != null)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (!stateInfo.IsName(attackAnimation))
            {
                isAttacking = false;
            }
            // If animation is still playing but has completed, mark as not attacking
            else if (stateInfo.normalizedTime >= 1f && !animator.IsInTransition(0))
            {
                isAttacking = false;
            }
        }

        if (interactAction != null && interactAction.action.WasPerformedThisFrame())
        {
            PerformAttack();
        }
    }
    public void SetHeldState(bool isHeld)
    {
        itemIsHeld = isHeld;
        
        if (animator != null)
        {
            animator.SetBool("isHold", isHeld);
            // Play different idle animations based on held state
            if (isHeld)
            {
                animator.Play(idleAnimationHeld);
            }
            else
            {
                animator.Play(idleAnimationNotHeld);
                isAttacking = false; // Reset attack state when item is dropped
            }
        }
        playerGrabInteraction.SetPlayerInteractionTexts(isHeld ? $"Press {interactAction.action.GetBindingDisplayString(0)} to Attack" : "");
    }
    public void PerformAttack()
    {
        if (!itemIsHeld)
            return; // Only allow attacking if the item is currently held by the player

        // Allow attack if not currently attacking (animation has finished)
        if (isAttacking)
            return; // Still in attack animation, wait for it to finish

        // Play attack animation
        if (animator != null)
        {
            animator.Play(attackAnimation);
            isAttacking = true;
        }
        attackTimer = waitToAttack;
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
