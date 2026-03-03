using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
[Serializable]
public class FootstepAudioData
{
    public List<AudioClip> footStepSound = new List<AudioClip>();
    public SurfaceType surfaceType;
}
public class FootstepsSoundManager : MonoBehaviour
{
    public List<FootstepAudioData> FootstepAudioData = new List<FootstepAudioData>();
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
            var randomIndex = UnityEngine.Random.Range(0, clips.Count);
            AudioSource.PlayClipAtPoint(clips[randomIndex], transform.position);
        }
        _lastfootstep = footstep;
    }
    public List<AudioClip> GetClipsForSurface()
    {
        var clips = new List<AudioClip>();
        var isHit = Physics.Raycast(transform.position + Vector3.up * .1f, Vector3.down, out var hitInfo, .1f, Enviroment);
        if (isHit)
        {
            var surfaceIdentifier = hitInfo.collider.GetComponent<SurfaceIdentifier>();
            if (surfaceIdentifier != null)
            {
                foreach (var audioData in FootstepAudioData)
                {
                    if(audioData.surfaceType == surfaceIdentifier.surfaceType)
                    {
                        return audioData.footStepSound;
                    }
                }
            }
        }
        return clips; // Default footstep sounds
    }
}
