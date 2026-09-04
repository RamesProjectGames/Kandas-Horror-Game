using UnityEngine;
using System.Collections.Generic;

public class NPCAnimationManager : MonoBehaviour
{
    public enum NPCState { Idle, Sit, Talk, Walk, Smoking, Class, Aktifitas }

    [System.Serializable]
    public struct AnimationStateGroup
    {
        public NPCState state;
        [Tooltip("Daftar variasi clip untuk state ini")]
        public List<AnimationClip> clips;
    }

    [Header("Current State & Variant")]
    public NPCState currentState = NPCState.Idle;
    [Tooltip("Pilih indeks variasi (0 = Variasi 1, 1 = Variasi 2, dst)")]
    public int variantIndex = 0;

    [Header("Animation Setup")]
    public List<AnimationStateGroup> animationGroups;

    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        PlayCurrentState();
    }

    public void PlayCurrentState()
    {
        AnimationStateGroup group = animationGroups.Find(g => g.state == currentState);

        if (group.clips != null && group.clips.Count > 0)
        {
            // Menjaga agar index tidak melebihi jumlah animasi yang tersedia (clamp)
            int index = Mathf.Clamp(variantIndex, 0, group.clips.Count - 1);
            AnimationClip clipToPlay = group.clips[index];

            if (clipToPlay != null && animator != null)
            {
                animator.Play(clipToPlay.name);
            }
        }
    }

    // Otomatis update di Scene saat State atau Variant Index diganti di Inspector
    void OnValidate()
    {
        if (Application.isPlaying && animator != null)
        {
            PlayCurrentState();
        }
    }
}
