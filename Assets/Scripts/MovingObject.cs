using System;
using UnityEngine;

public class MovingObject : MonoBehaviour
{
    public Vector3 From;
    public Vector3 To;

    Vector3 originalPosition;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        originalPosition = transform.position;
    }
    public void MoveToTarget(float speed = 1f,Action onComplete = null, Action<float> onProgress = null)
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
    public void MoveBackToOriginal(float speed = 1f, Action onComplete = null, Action<float> onProgress = null)
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
