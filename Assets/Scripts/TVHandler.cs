using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.Video;

public class TVHandler : MonoBehaviour
{
    public VideoClip[] videoClips;
    public AudioSource audioSrc;
    public VideoPlayer videoPlayer;
    public float volumeTriggerRadius;
    private List<IAudioRadiusListener> objectsInRadius = new List<IAudioRadiusListener>();
    //private float increaseInterval = 1f, increaseRunningInterval;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        videoPlayer.SetTargetAudioSource(0, audioSrc);
        PlayVideo(videoClips[0]);
        volumeTriggerRadius = audioSrc.maxDistance;
        //increaseRunningInterval = increaseInterval;
    }

    // Update is called once per frame
    void Update()
    {
        //increaseRunningInterval -= Time.deltaTime;
        //if(increaseRunningInterval <= 0)
        //{
        //    if (videoPlayer.isPlaying)
        //    {
        //        audioSrc.maxDistance += 1f;
        //        volumeTrigger.radius = audioSrc.maxDistance;
        //    }
        //    increaseRunningInterval = increaseInterval;
        //}

        if (videoPlayer.isPlaying)
        {
            audioSrc.maxDistance += Time.deltaTime;
            volumeTriggerRadius = audioSrc.maxDistance;
            CheckObjectsInRadius();
        }
    }

    public void PlayVideo(VideoClip clip)
    {
        audioSrc.maxDistance = 4f;
        volumeTriggerRadius = 4f;
        audioSrc.minDistance = .1f;
        videoPlayer.clip = clip;
        videoPlayer.Play();
    }

    void CheckObjectsInRadius()
    {
        // Get all objects with a specific tag or component
        Collider[] surroundingObjects = Physics.OverlapSphere(this.transform.position, volumeTriggerRadius);

        foreach (Collider nearbyObject in surroundingObjects)
        {
            if (!nearbyObject.TryGetComponent(out IAudioRadiusListener listener))
                continue;
            float distance = Vector3.Distance(transform.position, nearbyObject.transform.position);
            bool isInRadius = distance <= audioSrc.maxDistance;

            // Check if state changed
            bool wasInRadius = objectsInRadius.Contains(listener);

            if (isInRadius && !wasInRadius)
            {
                // Entered radius
                Debug.Log($"Object {nearbyObject.gameObject} listened");
                objectsInRadius.Add(listener);
                listener.OnEnterAudioRadius(this.gameObject);
            }
            else if (!isInRadius && wasInRadius)
            {
                // Exited radius
                Debug.Log($"Object {nearbyObject.gameObject} stopped listening");
                objectsInRadius.Remove(listener);
                listener.OnExitAudioRadius(this.gameObject);
            }
        }
    }
}

// Optional interface for objects that should react
public interface IAudioRadiusListener
{
    void OnEnterAudioRadius(GameObject audioSource);
    void OnExitAudioRadius(GameObject audioSource);
}