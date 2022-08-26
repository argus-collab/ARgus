#if !UNITY_WSA
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Cinemachine;

public class ChangeViewCinemachine : MonoBehaviour
{
    public LogManager logger;
    public CinemachineManager cinemachine;
    public Camera mainCamera;
    public Camera virtualCamera;

    public GameObject kinectCameraGO;
    private Camera kinectCamera;
    public GameObject hololensCameraGO;
    private Camera hololensCamera;

    public MeshRenderer HololensView;
    public MeshRenderer FreezedHololensView;
    public GameObject HololensGO;

    public MeshRenderer KinectView;
    public GameObject KinectGO;

    public GameObject SceneRoot;
    public maxCamera MouseNavigator;
    public FPSCamera FPSNavigator;
    public Text viewDescription;

    public ViewSelection viewSelector;

    private bool move = false;
    private bool teleport = false;
    private int index = 0;

    private Vector3 lastCameraPosition;
    private Vector3 startingCameraPosition;
    private Vector3 endingCameraPosition;

    private bool goKinect = false;
    private bool goHololens = false;
    private bool goVirtual = false;

    private bool isHololensView = false;
    private bool isVirtualView = false;
    private bool isCustomCameraView = false;
    private bool isKinectView = false;

    private bool bringVirtualCamera = false;
    private bool switchToVirtualCamera = false;
    private bool deleteCamAtEndMove = false;
    private Camera toDeleteAtEndMove;
    //private bool hideFreezedHololensView = false;

    private int initialLayerMask;

    public GameObject stickManager;
    public GameObject sphericalVisualizer;

    private float movingTimeStamp;
    private float movingTimeLimit = 5f;

    public bool isMouseNavigationActive = true;

    public VirtualViewPositionRecorder virtualViewPositionRecorder;
    public int hololensFov = 39;

    public float transitionSpeed = 2f;

    private bool desactivateMouseNavigationAtNextStep = false;

    void Start()
    {
        HololensView.enabled = false;
        KinectView.enabled = false;
        FreezedHololensView.enabled = false;
        //ShowScene();

        isVirtualView = true;
    }

    public void UpdateTransitionSpeed(System.Single val)
    {
        transitionSpeed = 1 / val;
    }

    //public void SetHideFreezedHololensView(bool state)
    //{
    //    hideFreezedHololensView = state;
    //}

    public void HideGround()
    {
        mainCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("Ground"));
    }

    public void TeleportVirtualCamera(Vector3 p, Quaternion q, float offset = 0)
    {
        if (MouseNavigator.isActive)
            MouseNavigator.Teleport(p, q);
        else
        {
            MouseNavigator.isActive = true;
            MouseNavigator.Teleport(p, q);
            MouseNavigator.isActive = false;
        }
    }

    public void TeleportVirtualCameraAtMainCamera(bool dontSwitch = true)
    {
        TeleportVirtualCamera(mainCamera.transform.position, mainCamera.transform.rotation);
        desactivateMouseNavigationAtNextStep = dontSwitch;
    }

    public void TeleportVirtualCameraAnndDisplayVirtual()
    {
        TeleportVirtualCameraAtMainCamera();

        HideAllImmediately();

        cinemachine.stateDrivenCamera.m_DefaultBlend.m_Style = CinemachineBlendDefinition.Style.Cut;
        cinemachine.stateDrivenCamera.m_AnimatedTarget.SetTrigger("GoRedirectState");
        cinemachine.stateDrivenCamera.m_AnimatedTarget.SetTrigger("Go" + virtualCamera.name);

        LateDisplayVirtualView();
    }

    public void ResetVirtualPosition()
    {
        logger.LogResetVirtualCameraPosition();
        TeleportVirtualCamera(new Vector3(-2.8f, 2.25f, 0), Quaternion.Euler(37.51f, 90f, 0f));
    }

    public bool IsHololensView()
    {
        return isHololensView;
    }

    public bool IsVirtualView()
    {
        return isVirtualView;
    }

    public bool IsKinectView()
    {
        return isKinectView;
    }

    public bool IsInTransition()
    {
        return goHololens || goKinect || goVirtual;
    }

    public void HideAll(bool hideFreezedHololensView = true)
    {
        isHololensView = false;
        isVirtualView = false;
        isCustomCameraView = false;
        isKinectView = false;

        stickManager.SetActive(true);
        sphericalVisualizer.SetActive(true);

        HololensView.GetComponent<FadeMesh>().HideAndDesactivate();
        if (hideFreezedHololensView)
            FreezedHololensView.GetComponent<FadeMesh>().HideAndDesactivate();
        KinectView.GetComponent<FadeMesh>().HideAndDesactivate();

        //HololensView.enabled = false;
        //KinectView.enabled = false;

        ShowScene();
    }

    public void HideAllImmediately(bool hideFreezedHololensView = true)
    {
        isHololensView = false;
        isVirtualView = false;
        isCustomCameraView = false;
        isKinectView = false;

        stickManager.SetActive(true);
        sphericalVisualizer.SetActive(true);

        HololensView.GetComponent<FadeMesh>().HideImmediatelyAndDesactivate();
        if (hideFreezedHololensView)
            FreezedHololensView.GetComponent<FadeMesh>().HideImmediatelyAndDesactivate();
        KinectView.GetComponent<FadeMesh>().HideImmediatelyAndDesactivate();

        //HololensView.enabled = false;
        //KinectView.enabled = false;

        ShowScene();
    }

    public void ShowAll()
    {
        HololensView.GetComponent<FadeMesh>().Show();
        KinectView.GetComponent<FadeMesh>().Show();
    }

    public void SetVirtualCameraAtMainCamera()
    {
        // set virtual camera at main camera if hololens or kinect
        virtualCamera.transform.position = mainCamera.transform.position;
        virtualCamera.transform.rotation = mainCamera.transform.rotation;

        HideAll();
        cinemachine.stateDrivenCamera.m_AnimatedTarget.SetTrigger("GoRedirectState");
        cinemachine.stateDrivenCamera.m_AnimatedTarget.SetTrigger("Go" + virtualCamera.name);
        LateDisplayVirtualView();
    }

    public void BringVirtualCameraOnNextMove()
    {
        bringVirtualCamera = true;
    }

    public void Update()
    {
        // debug
        //Debug.Log("mainCamera.cullingMask : " + mainCamera.cullingMask);

        if (kinectCamera == null && kinectCameraGO != null)
            kinectCamera = kinectCameraGO.GetComponent<Camera>();

        if (hololensCamera == null && hololensCameraGO != null)
            hololensCamera = hololensCameraGO.GetComponent<Camera>();

        if (move && Time.time - movingTimeStamp < movingTimeLimit)
        {
            if ((goHololens && ((mainCamera.transform.position - lastCameraPosition).magnitude > 0
                || mainCamera.transform.position == startingCameraPosition))
                || (!goHololens && mainCamera.transform.position != endingCameraPosition))
            {
                MouseNavigator.isActive = false;
                FPSNavigator.enabled = false;
                viewSelector.enabled = false;
                sphericalVisualizer.SetActive(false);
                stickManager.SetActive(false);
                Debug.Log("is moooooooooooving");
                // moving done with cinemachine
                //mainCamera.transform.position = Vector3.SmoothDamp(mainCamera.transform.position, targetPosition, ref velocity, smoothTime);
                //mainCamera.transform.rotation = Quaternion.Slerp(mainCamera.transform.rotation, targetRotation, timeCount / 10);
                //timeCount = timeCount + Time.deltaTime;
            }
            else
            {
                TeleportVirtualCameraAtMainCamera();

                MouseNavigator.isActive = true;
                FPSNavigator.enabled = true;
                viewSelector.enabled = true;
                sphericalVisualizer.SetActive(true);
                stickManager.SetActive(true);

                move = false;

                if (kinectCamera != null && goKinect)//mainCamera.transform.position == kinectCamera.transform.position)
                {
                    goKinect = false;
                    Debug.Log("KINECT POSITION");
                    LateDisplayExternalCameraView();
                }
                else if (hololensCamera != null && goHololens)//mainCamera.transform.position == hololensCamera.transform.position)
                {
                    goHololens = false;
                    Debug.Log("HOLOLENS POSITION");
                    LateDisplayARHeadsetView();
                }
                else if (goVirtual)//mainCamera.transform.position == virtualCamera.transform.position)
                {
                    goVirtual = false;
                    Debug.Log("VIRTUAL POSITION");
                    LateDisplayVirtualView();
                }
                else
                {
                    Debug.Log("CUSTOM CAM POSITION");
                    //MouseNavigator.isActive = true;
                    viewSelector.enabled = true;
                    viewDescription.text = "Custom Camera";
                }

                if (bringVirtualCamera)
                {
                    bringVirtualCamera = false;
                    TeleportVirtualCamera(mainCamera.transform.position, mainCamera.transform.rotation);
                }

                if (switchToVirtualCamera)
                {
                    switchToVirtualCamera = false;
                    DisplayVirtualView();
                }

                if (deleteCamAtEndMove)
                {
                    //Destroy(toDeleteAtEndMove);
                    virtualViewPositionRecorder.FlushInvisibleCamera();
                }
            }
        }
        else
        {
            if(desactivateMouseNavigationAtNextStep)
            {
                desactivateMouseNavigationAtNextStep = false;
            }
            else if (isMouseNavigationActive && !isVirtualView && MouseNavigator.IsMoving())
            {
                //Vector3 offset = new Vector3(0.001f, 0.001f, 0.001f);
                if (isHololensView)
                    TeleportVirtualCamera(HololensGO.transform.position, HololensGO.transform.rotation);
                else
                    TeleportVirtualCamera(mainCamera.transform.position, mainCamera.transform.rotation);

                HideAllImmediately();

                cinemachine.stateDrivenCamera.m_DefaultBlend.m_Style = CinemachineBlendDefinition.Style.Cut;
                cinemachine.stateDrivenCamera.m_AnimatedTarget.SetTrigger("GoRedirectState");
                cinemachine.stateDrivenCamera.m_AnimatedTarget.SetTrigger("Go" + virtualCamera.name);

                teleport = true;
            }
        }

        if (teleport && !cinemachine.stateDrivenCamera.m_AnimatedTarget.GetCurrentAnimatorStateInfo(0).IsTag(virtualCamera.name))
        {
            teleport = false;
            LateDisplayVirtualView();
        }

        lastCameraPosition = mainCamera.transform.position;
    }

    public void ChangeToNextView()
    {
        //AnimatorStateInfo state = cinemachine.stateDrivenCamera.m_AnimatedTarget.GetCurrentAnimatorStateInfo(0);
        //Debug.Log(state.shortNameHash);

        //int nbParam = cinemachine.stateDrivenCamera.m_AnimatedTarget.parameterCount;
        //AnimatorControllerParameter[] param = cinemachine.stateDrivenCamera.m_AnimatedTarget.parameters;

        //CinemachineVirtualCamera[] virtualCameras = cinemachine.GetComponentsInChildren<CinemachineVirtualCamera>();

        ////index = (index + 1) % nbParam;
        //move = true;

        //Debug.Log("index : " + index);
        startingCameraPosition = mainCamera.transform.position;

        // activate event to change current state in animator
        cinemachine.stateDrivenCamera.m_AnimatedTarget.SetTrigger("GoNextState");

        // display video stream in full screen if needed
        //Debug.Log(cinemachine.stateDrivenCamera.LiveChild.VirtualCameraGameObject.GetComponent<MoveWith>().toFollow);


        //GameObject vCamChild = cinemachine.stateDrivenCamera.LiveChild.VirtualCameraGameObject;
        //GameObject vCamToFollow = vCamChild.GetComponent<MoveWith>().toFollow;
        //Camera selectedCamera = vCamToFollow.GetComponent<Camera>();



        //Debug.Log("vCamChild = " + vCamChild.name);
        //Debug.Log("vCamToFollow = " + vCamToFollow.name);

        move = true;
        movingTimeStamp = Time.time;
        HideAll();

        //if (selectedCamera == kinectCamera)
        //    DisplayExternalCameraView();
        //else if (selectedCamera == hololensCamera)
        //    DisplayARHeadsetView();
        //else
        //    DisplayVirtualView();

        //DisplayVirtualView();
        //DisplayARHeadsetView();
        //DisplayExternalCameraView();

        //if (index == 0) 
        //{
        //    DisplayVirtualView();
        //}
        //if (index == 1)
        //{
        //    DisplayARHeadsetView();
        //}
        //if (index == 2)
        //{
        //    DisplayExternalCameraView();
        //}
    }

    public void DisplayVirtualView()
    {
        Debug.Log("virtual camera view");

        HideAll();
        //index = 0;
        move = true;
        movingTimeStamp = Time.time;

        startingCameraPosition = mainCamera.transform.position;
        endingCameraPosition = virtualCamera.transform.position;


        cinemachine.stateDrivenCamera.m_DefaultBlend.m_Style = CinemachineBlendDefinition.Style.EaseInOut;
        cinemachine.stateDrivenCamera.m_DefaultBlend.m_Time = transitionSpeed;
        cinemachine.stateDrivenCamera.m_AnimatedTarget.SetTrigger("GoRedirectState");
        cinemachine.stateDrivenCamera.m_AnimatedTarget.SetTrigger("Go" + virtualCamera.name);

        goVirtual = true;
    }

    public void DisplayCameraView(Camera cam, bool hideFreezedViewOnNextDisplacement = true, bool switchVirtual = false, bool deleteCam = false)
    {

        Debug.Log(cam.gameObject.name + "view");

        HideAll(hideFreezedViewOnNextDisplacement);

        movingTimeStamp = Time.time;
        move = true;

        // wip
        startingCameraPosition = mainCamera.transform.position;
        endingCameraPosition = cam.transform.position;


        // to keep ??
        //TeleportVirtualCamera(cam.gameObject.transform.position, cam.gameObject.transform.rotation);

        if (!hideFreezedViewOnNextDisplacement)
        {
            // we teleport to freezed view => no blending
            cinemachine.stateDrivenCamera.m_DefaultBlend.m_Style = CinemachineBlendDefinition.Style.Cut;
        }
        else
        {
            cinemachine.stateDrivenCamera.m_DefaultBlend.m_Style = CinemachineBlendDefinition.Style.EaseInOut;
            cinemachine.stateDrivenCamera.m_DefaultBlend.m_Time = transitionSpeed;
        }

 
        cinemachine.stateDrivenCamera.m_AnimatedTarget.SetTrigger("GoRedirectState");
        cinemachine.stateDrivenCamera.m_AnimatedTarget.SetTrigger("Go" + cam.gameObject.name);

        //if (hideFreezedViewOnNextDisplacement)
        //SetHideFreezedHololensView(true);

        bringVirtualCamera = switchVirtual;
        //switchToVirtualCamera = switchVirtual;
        deleteCamAtEndMove = deleteCam;
    }

    private void LateDisplayVirtualView()
    {
        //MouseNavigator.enabled = true;
        //FPSNavigator.enabled = true;
        viewSelector.enabled = true;
        viewDescription.text = "Virtual";

        //cinemachine.GetMainVirtualCamera().m_Lens.FieldOfView = 39;


        //mainCamera.cullingMask = -2147480777;
        mainCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("ParticipantPlayer"));

        isVirtualView = true;

        logger.LogVirtualView();
    }

    public void DisplayARHeadsetView()
    {
        Debug.Log("hololens camera view");

        HideAll();
        //index = 1;
        movingTimeStamp = Time.time;
        move = true;
        startingCameraPosition = mainCamera.transform.position;
        endingCameraPosition = hololensCamera.transform.position;


        cinemachine.stateDrivenCamera.m_DefaultBlend.m_Style = CinemachineBlendDefinition.Style.EaseInOut;
        cinemachine.stateDrivenCamera.m_DefaultBlend.m_Time = transitionSpeed;
        cinemachine.stateDrivenCamera.m_AnimatedTarget.SetTrigger("GoRedirectState");
        cinemachine.stateDrivenCamera.m_AnimatedTarget.SetTrigger("Go" + hololensCamera.name);

        goHololens = true;
    }

    private void LateDisplayARHeadsetView()
    {
        HololensView.enabled = true;

        HololensView.GetComponent<FadeMesh>().Show();
        HideScene();
        //MouseNavigator.enabled = false;
        viewDescription.text = "Hololens";

        viewSelector.enabled = false;

        isHololensView = true;

        stickManager.SetActive(false);
        sphericalVisualizer.SetActive(false);

        //ResetVirtualPosition();

        //cinemachine.GetMainVirtualCamera().m_Lens.FieldOfView = 39;
        cinemachine.GetMainVirtualCamera().m_Lens.FieldOfView = 65;

        mainCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("InvisibleMiniature"));


        logger.LogHololensView();

    }

    public void DisplayExternalCameraView()
    {
        Debug.Log("kinect view");
        HideAll();
        //index = 2;
        movingTimeStamp = Time.time;
        move = true;
        startingCameraPosition = mainCamera.transform.position;
        endingCameraPosition = kinectCamera.transform.position;


        cinemachine.stateDrivenCamera.m_DefaultBlend.m_Style = CinemachineBlendDefinition.Style.EaseInOut;
        cinemachine.stateDrivenCamera.m_DefaultBlend.m_Time = transitionSpeed;
        cinemachine.stateDrivenCamera.m_AnimatedTarget.SetTrigger("GoRedirectState");
        cinemachine.stateDrivenCamera.m_AnimatedTarget.SetTrigger("Go" + kinectCamera.name);

        goKinect = true;
    }

    public void LateDisplayExternalCameraView()
    {
        KinectView.enabled = true;

        KinectView.GetComponent<FadeMesh>().Show();
        HideScene();
        //MouseNavigator.enabled = false;
        viewSelector.enabled = true;
        viewDescription.text = "Kinect";

        isKinectView = true;

        cinemachine.GetMainVirtualCamera().m_Lens.FieldOfView = 54;

        //TeleportVirtualCamera(KinectGO.transform.position, kinectCameraGO.transform.rotation);
        mainCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("VisibleVirtualAndHololens"));
        mainCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("InvisibleMiniature"));

        logger.LogKinectView();

    }

    void OnApplicationQuit()
    {
        ShowAll();
    }

    void HideScene()
    {
        //int layerMask = 1 << 2;
        //layerMask = ~layerMask;

        //SceneRoot.SetActive(false);
        //initialLayerMask = mainCamera.cullingMask;
        //mainCamera.cullingMask = -2147483594;

        mainCamera.cullingMask = ~0;

        mainCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("Client"));
        mainCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("VideoRenderer"));
        mainCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("ParticipantPlayer"));
        mainCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("Default"));
        mainCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("Ground"));


    }

    void ShowScene()
    {
        //SceneRoot.SetActive(true);
        //mainCamera.cullingMask = initialLayerMask;
        mainCamera.cullingMask = ~0;
    }
}
#endif