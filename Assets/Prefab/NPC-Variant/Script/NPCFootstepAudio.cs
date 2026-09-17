using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class NPCFootstepAudio : MonoBehaviour
{
    [Header("Footstep Sounds")]
    public AudioClip[] footstepClips;
    [Range(0f, 1f)] public float volume = 0.5f;

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Fungsi ini dipanggil via Animation Event di Editor Animasi
    public void PlayFootstep()
    {
        if (footstepClips == null || footstepClips.Length == 0) return;

        // Acak suara langkah agar tidak monoton
        AudioClip clip = footstepClips[Random.Range(0, footstepClips.Length)];
        audioSource.PlayOneShot(clip, volume);
    }
}
