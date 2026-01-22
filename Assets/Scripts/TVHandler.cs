using UnityEngine;
using UnityEngine.Video;

public class TVHandler : MonoBehaviour
{
    public VideoClip[] videoClips;
    public AudioSource audioSrc;
    public VideoPlayer videoPlayer;
    private float increaseInterval = 1f, increaseRunningInterval;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        videoPlayer.SetTargetAudioSource(0, audioSrc);
        PlayVideo(videoClips[0]);
        increaseRunningInterval = increaseInterval;
    }

    // Update is called once per frame
    void Update()
    {
        increaseRunningInterval -= Time.deltaTime;
        if(increaseRunningInterval <= 0)
        {
            if (videoPlayer.isPlaying)
            {
                audioSrc.maxDistance += 1f;
            }
            increaseRunningInterval = increaseInterval;
        }
    }

    public void PlayVideo(VideoClip clip)
    {
        audioSrc.maxDistance = 5f;
        audioSrc.minDistance = .1f;
        videoPlayer.clip = clip;
        videoPlayer.Play();
    }
}
