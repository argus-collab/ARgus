#if !UNITY_WSA
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Cinemachine;

public class MoveWith : MonoBehaviour
{
    public GameObject toFollow;

    private CinemachineVirtualCamera virtualCamera;

    private void Start()
    {
        virtualCamera = GetComponent<CinemachineVirtualCamera>();
    }

    void Update()
    {
        if (virtualCamera != null)
        {
            CinemachineTransposer transp = virtualCamera.AddCinemachineComponent<CinemachineTransposer>();
            transp.m_BindingMode = CinemachineTransposer.BindingMode.WorldSpace;
            transp.m_FollowOffset = toFollow.transform.position;
            transform.rotation = toFollow.transform.rotation;
        }
        else
        {
            transform.position = toFollow.transform.position;
            transform.rotation = toFollow.transform.rotation;
        }
    }
}
#endif
