/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;

public class VuforiaFixedTracking : MonoBehaviour
{
    public GameObject trackingImage;
    private ImageTargetBehaviour trackingImageTargetBehaviour;
    private Vector3 trackingImageLastPosition;
    private Quaternion trackingImageLastRotation;
    private bool haveBeenTracked = false;
    private bool activeTracking = true;

    public GameObject sceneOnTrackedImage;

    public bool GetActiveTrackingState()
    {
        return activeTracking;
    }

    public void SetActiveTrackingState(bool newState)
    {
        activeTracking = newState;
    }

    private void Start()
    {
        sceneOnTrackedImage.SetActive(false);
        //sceneOnTrackedImage.transform.localScale = trackingImage.transform.localScale;
        trackingImageTargetBehaviour = trackingImage.GetComponent<ImageTargetBehaviour>();
    }

    void Update()
    {
        if (trackingImageTargetBehaviour.CurrentStatus == TrackableBehaviour.Status.TRACKED)
        {
            trackingImageLastPosition = trackingImage.transform.position;
            trackingImageLastRotation = trackingImage.transform.rotation;

            haveBeenTracked = true;
        }

        if (haveBeenTracked && activeTracking)
        {
            sceneOnTrackedImage.SetActive(true);

            sceneOnTrackedImage.transform.position = trackingImageLastPosition;
            sceneOnTrackedImage.transform.rotation = trackingImageLastRotation;
        }
    }
}
*/