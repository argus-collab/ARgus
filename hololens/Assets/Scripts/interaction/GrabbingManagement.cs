using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ManipulationHandler))]
public class GrabbingManagement : MonoBehaviour
{
    private bool IsGrabbing = false;
    private ManipulationHandler manipHandler;

    private void Start()
    {
        manipHandler = GetComponent<ManipulationHandler>();

        manipHandler.OnManipulationStarted.AddListener(Grab);
        manipHandler.OnManipulationEnded.AddListener(Release);
    }

    public bool GetGrabbingState()
    {
        return IsGrabbing;
    }

    private void Grab(ManipulationEventData data)
    {
        IsGrabbing = true;
    }

    private void Release(ManipulationEventData data)
    {
        IsGrabbing = false;
    }

}
