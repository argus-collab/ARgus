#if !UNITY_WSA
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ViewInteractableManagement : MonoBehaviour
{
    public Button button;
    public ChangeViewCinemachine viewManager;

    public bool virtualView;
    public bool hololensView;
    public bool kinectView;

    void Update()
    {
        if ((virtualView && viewManager.IsVirtualView())
        || (hololensView && viewManager.IsHololensView())
        || (kinectView && viewManager.IsKinectView())
        || viewManager.IsInTransition())
            button.interactable = false;
        else
            button.interactable = true;
    }
}
#endif