using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeView : MonoBehaviour
{
    public Camera mainCamera;

    public MeshRenderer HololensView;
    public GameObject HololensGO;

    public MeshRenderer KinectView;
    public GameObject KinectGO;

    public GameObject SceneRoot;
    public maxCamera MouseNavigator;
    public Text viewDescription;

    public Vector3 targetPosition;
    public Quaternion targetRotation;
    public float smoothTime = 0.3F;
    //public float rotationDuration = 1F;
    private Vector3 velocity = Vector3.zero;
    public bool move = false;

    private int index = 0;

    [Header("Debug")]
    private bool recordCamTransform = true;
    public Vector3 lastVirtualPosition;
    public Quaternion lastVirtualRotation;

    private float timeCount = 0.0f;

    void Start()
    {
        HideAll();
    }

    public void HideAll()
    {
        HololensView.enabled = false;
        KinectView.enabled = false;
        SceneRoot.SetActive(true);

    }

    public void ShowAll()
    {
        HololensView.enabled = true;
        KinectView.enabled = true;
    }

    public void Update()
    {
        if(move)
        {
            if ((mainCamera.transform.position - targetPosition).magnitude > 0.001f)
            {
                MouseNavigator.isActive = false;
                mainCamera.transform.position = Vector3.SmoothDamp(mainCamera.transform.position, targetPosition, ref velocity, smoothTime);
                mainCamera.transform.rotation = Quaternion.Slerp(mainCamera.transform.rotation, targetRotation, timeCount/10);
                timeCount = timeCount + Time.deltaTime;
            }
            else
            {
                MouseNavigator.isActive = true;
                move = false;
                timeCount = 0;

                if (index == 0)
                {
                    LateDisplayVirtualView();
                }
                else if (index == 1)
                {
                    LateDisplayARHeadsetView();
                }
                else if (index == 2)
                {
                    LateDisplayExternalCameraView();
                }
                else
                {
                    MouseNavigator.isActive = true;
                    viewDescription.text = "Custom Camera";
                    recordCamTransform = true;
                }
            }
        }

        if (recordCamTransform)
        {
            lastVirtualPosition = mainCamera.transform.position;
            lastVirtualRotation = mainCamera.transform.rotation;
        }
    }

    public void ChangeToNextView()
    {
        index = (index + 1) % 3;
        move = true;

        //Debug.Log("index : " + index);

        if (index == 0)
        {
            DisplayVirtualView();
        }
        if (index == 1)
        {
            DisplayARHeadsetView();
        }
        if (index == 2)
        {
            DisplayExternalCameraView();
        }
    }

    public void DisplayVirtualView()
    {
        HideAll();
        index = 0;
        move = true;
        recordCamTransform = false;
        targetPosition = lastVirtualPosition;
        targetRotation = lastVirtualRotation;
    }

    public void DisplayCameraView(Camera cam)
    {
        HideAll();
        move = true;
        recordCamTransform = true;
        targetPosition = cam.transform.position;
        targetRotation = cam.transform.rotation;
    }

    private void LateDisplayVirtualView()
    {
        MouseNavigator.isActive = true;
        viewDescription.text = "Virtual";
        recordCamTransform = true;
    }

    public void DisplayARHeadsetView()
    {
        HideAll();
        index = 1;
        move = true;
        recordCamTransform = false;
        targetPosition = HololensGO.transform.position;
        targetRotation = HololensGO.transform.rotation;
    }

    private void LateDisplayARHeadsetView()
    {
        HololensView.enabled = true;
        SceneRoot.SetActive(false);
        MouseNavigator.isActive = false;
        viewDescription.text = "Hololens";
    }

    public void DisplayExternalCameraView()
    {
        HideAll();
        index = 2;
        move = true;
        recordCamTransform = false;
        targetPosition = KinectGO.transform.position;
        targetRotation = KinectGO.transform.rotation;
    }

    public void LateDisplayExternalCameraView()
    {
        KinectView.enabled = true;
        SceneRoot.SetActive(false);
        MouseNavigator.isActive = false;
        viewDescription.text = "Kinect";
    }

    void OnApplicationQuit()
    {
        ShowAll();
    }
}
