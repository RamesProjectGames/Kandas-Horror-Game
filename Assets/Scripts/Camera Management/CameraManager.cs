using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    static List<CinemachineCamera> cameras = new List<CinemachineCamera>();
    public static CinemachineCamera currentActiveCamera = null;

    private static CameraManager instance;
    private int transitionRequestId;

    private void Awake()
    {
        instance = this;
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
    
    public static bool IsActiveCamera(CinemachineCamera camera)
    {
        return currentActiveCamera == camera;
    }

    public static void SwitchCamera(CinemachineCamera newCamera, Action onTransitionCompleted = null)
    {
        if (newCamera == null)
        {
            return;
        }
        
        newCamera.Priority = 10;
        currentActiveCamera = newCamera;

        foreach (CinemachineCamera cam in cameras)
        {
            if(cam != newCamera)
            {
                cam.Priority = 0;
            }
        }

        if (instance != null)
        {
            instance.transitionRequestId++;
            instance.StartCoroutine(instance.WaitForCameraTransition(newCamera, instance.transitionRequestId, onTransitionCompleted));
        }
    }

    private IEnumerator WaitForCameraTransition(CinemachineCamera targetCamera, int requestId, Action onTransitionCompleted)
    {
        yield return null;

        CinemachineBrain brain = FindAnyObjectByType<CinemachineBrain>();
        while (brain != null && brain.IsBlending)
        {
            yield return null;
        }

        if (requestId == transitionRequestId && currentActiveCamera == targetCamera)
        {
            onTransitionCompleted?.Invoke();
        }
    }
    public static void LookAt(Transform target)
    {
        if (currentActiveCamera != null)
        {
            if (target != null)
                currentActiveCamera.transform.LookAt(target);
            currentActiveCamera.LookAt = target;
        }
    }
    public static void Register(CinemachineCamera camera)
    {
        cameras.Add(camera);
    }
    public static void Unregister(CinemachineCamera camera)
    {
        cameras.Remove(camera);
    }
}
