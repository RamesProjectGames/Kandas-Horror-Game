using Unity.Cinemachine;
using UnityEngine;
[RequireComponent(typeof(CinemachineCamera))]
public class CameraRegister : MonoBehaviour
{
    void OnEnable()
    {
        CameraManager.Register(GetComponent<CinemachineCamera>());
    }
    void OnDisable()
    {
        CameraManager.Unregister(GetComponent<CinemachineCamera>());
    }
}
