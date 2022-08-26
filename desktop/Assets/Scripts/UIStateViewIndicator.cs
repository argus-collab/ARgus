#if !UNITY_WSA
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIStateViewIndicator : MonoBehaviour
{
    public GameObject stateIndicator;
    public GameObject virtualButton;
    public GameObject hololensButton;
    public GameObject kinectButton;

    public ChangeViewCinemachine viewMananger;

    public bool isActive = true;

    void Update()
    {
        if (!isActive)
        {
            stateIndicator.SetActive(false);
            return;
        }

        if (viewMananger.IsInTransition())
        {
            stateIndicator.SetActive(false);
        }
        else if (viewMananger.IsVirtualView())
        {
            stateIndicator.SetActive(true);
            stateIndicator.transform.position = virtualButton.transform.position;
        }
        else if (viewMananger.IsHololensView())
        {
            stateIndicator.SetActive(true);
            stateIndicator.transform.position = hololensButton.transform.position;
        }
        else if (viewMananger.IsKinectView())
        {
            stateIndicator.SetActive(true);
            stateIndicator.transform.position = kinectButton.transform.position;
        }
    }
}
#endif