using System;
using Unity.Mathematics;
using UnityEngine;

public class FootstepsSoundManager : MonoBehaviour
{
    public AudioClip[] FootstepSounds;
    public AudioClip[] grassFootstepSounds;
    public AudioClip[] woodFootstepSounds;
    public AudioClip[] metalFootstepSounds;
    public AudioClip[] waterFootstepSounds;
    public AudioClip[] roadFootstepSounds;
    public AudioClip[] dirtFootstepSounds;
    public Animator Animator;
    public LayerMask Enviroment;
    private float _lastfootstep;
    void OnValidate()
    {
        if(!Animator)
        {
            Animator = GetComponent<Animator>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        var footstep = Animator.GetFloat("Footstep");
        if(Math.Abs(footstep)< .000001f) footstep = 0f;
        if (_lastfootstep > 0 && footstep < 0 || _lastfootstep < 0 && footstep > 0)
        {
            var clips = GetClipsForSurface();
            var randomIndex = UnityEngine.Random.Range(0, clips.Length);
            AudioSource.PlayClipAtPoint(clips[randomIndex], transform.position);
        }
        _lastfootstep = footstep;
    }
    public AudioClip[] GetClipsForSurface()
    {
        var isHit = Physics.Raycast(transform.position + Vector3.up * .1f, Vector3.down, out var hitInfo, .1f, Enviroment);
        if (isHit)
        {
            var surfaceIdentifier = hitInfo.collider.GetComponent<SurfaceIdentifier>();
            if (surfaceIdentifier != null)
            {
                switch (surfaceIdentifier.surfaceType)
                {
                    case SurfaceType.Grass:
                        return grassFootstepSounds;
                    case SurfaceType.Wood:
                        return woodFootstepSounds;
                    case SurfaceType.Metal:
                        return metalFootstepSounds;
                    case SurfaceType.Water:
                        return waterFootstepSounds;
                    case SurfaceType.Road:
                        return roadFootstepSounds;
                    case SurfaceType.Dirt:
                        return dirtFootstepSounds;
                }
            }
        }
        return FootstepSounds; // Default footstep sounds
    }
}
