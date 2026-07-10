using System;
using UnityEngine;
using UnityEngine.Events;

public class MovingObject : MonoBehaviour
{
    public Vector3 From;
    public Vector3 To;

    public UnityEvent onComplete;
    public UnityEvent<float> onProgress;

    Vector3 originalPosition;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        originalPosition = transform.position;
    }
    public void MoveToTarget(float speed = 1f)
    {
        transform.LeanMove(To, speed)
        .setOnComplete(() =>
        {
            onComplete?.Invoke();
        })
        .setOnUpdate((float progress) =>
        {
            onProgress?.Invoke(progress);
        });
    }
    public void MoveBackToOriginal(float speed = 1f)
    {
        transform.LeanMove(originalPosition, speed)
        .setOnComplete(() =>
        {
            onComplete?.Invoke();
        })
        .setOnUpdate((float progress) =>
        {
            onProgress?.Invoke(progress);
        });
    }
}
