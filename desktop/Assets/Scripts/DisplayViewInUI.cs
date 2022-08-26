#if !UNITY_WSA
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplayViewInUI : MonoBehaviour
{
    public Camera mainCamera;
    public MeshRenderer view;
    public RectTransform viewContainer;
    public Material viewMaterial;

    public MeshRenderer hololensViewMR;
    public MeshRenderer kinectViewMR;
    public GameObject hololensGO;
    public GameObject kinectGO;
    public GameObject virtualGO;

    [Header("From mesh renderer")]
    public List<MeshRenderer> inputViewMR;
    public List<GameObject> goMR;

    [Header("From camera")]
    public List<RedirectViewFromCamera> inputViewC;
    public List<GameObject> goC;

    //public int maxImageSizeX = 1;
    //public float growingSpeed = 0.5f;

    public float offset = 50;

    private bool positionSet = false;
    public bool displayOnMouseOver = true;
    private bool displaySpecificCamera = false;
    private bool displayStatic = false;

    private Vector3 centerSpecificCamera;
    private Camera specificCamera;
    private float specificOffset;

    private float xDynamic;
    private float yDynamic;

    public bool isActive = true;

    private bool wasDisplaying = false;

    [Range(0.0f, 1920.0f)]
    public float widthOffset = 0;

    [Range(0.0f, 1080.0f)]
    public float heightOffset = 0;

    public bool mouseBehaviour;
    public Vector3 cursorFixedPos;
    private float mouseOffset = 50f;

    public ChangeViewCinemachine viewChanger;
    public LogManager logger;

    private bool isPreviewUsedForViewPoint = false;


    private void Start()
    {
        if (inputViewMR == null)
            inputViewMR = new List<MeshRenderer>();
        if (goMR == null)
            goMR = new List<GameObject>();
        if (inputViewC == null)
            inputViewC = new List<RedirectViewFromCamera>();
        if (goC == null)
            goC = new List<GameObject>();
        //view.material = inputView[0].material;

        HideView();
    }

    private void CleanBuffers()
    {
        for (int i = 0; i < inputViewMR.Count; ++i)
            if (inputViewMR[i] == null)
                inputViewMR.RemoveAt(i);

        for (int i = 0; i < goMR.Count; ++i)
            if (goMR[i] == null)
                goMR.RemoveAt(i);

        for (int i = 0; i < inputViewC.Count; ++i)
            if (inputViewC[i] == null)
                inputViewC.RemoveAt(i);

        for (int i = 0; i < goC.Count; ++i)
            if (goC[i] == null)
                goC.RemoveAt(i);
    }

    private void Update()
    {
        if (!isActive)
            return;

        CleanBuffers();

        xDynamic = specificOffset;
        yDynamic = specificOffset;

        if (specificCamera != null && mainCamera.WorldToScreenPoint(specificCamera.transform.position).x < mainCamera.WorldToScreenPoint(centerSpecificCamera).x)
            xDynamic = -specificOffset;

        if (specificCamera != null && mainCamera.WorldToScreenPoint(specificCamera.transform.position).y < mainCamera.WorldToScreenPoint(centerSpecificCamera).y)
            yDynamic = -specificOffset;

        if (displayOnMouseOver)
        {
            IsGOSelected();

            //Debug.Log("display on mouse over");


        }
        else if (displaySpecificCamera)
        {
            DisplayView();
            //Debug.Log("truc truc truc");
            viewContainer.localPosition = new Vector3(//0, 0, 500);
                mainCamera.WorldToScreenPoint(specificCamera.transform.position).x - Screen.width / 2 + xDynamic, 
                mainCamera.WorldToScreenPoint(specificCamera.transform.position).y - Screen.height / 2 + yDynamic, 
                0);// 100);
        }
        else if(displayStatic)
        {
            DisplayView();
            viewContainer.localPosition = new Vector3(//200, 200, 0);
                -Screen.width/2 + 290,//widthOffset,// - 500,
                Screen.height/2 - 180,//heightOffset,// - 500,
                0);// 100);
        }
    }

    void SetViewPosition()
    {
        float x = offset;
        float y = offset;


        if (mouseBehaviour)
        {
            if (Input.mousePosition.x > Screen.width / 2)
                x = -offset;

            if (Input.mousePosition.y > Screen.height / 2)
                y = -offset;

            viewContainer.localPosition = new Vector3(//0, 0, -100);
                Input.mousePosition.x - Screen.width / 2 + x, Input.mousePosition.y - Screen.height / 2 + y, 0);// 100);
        }
        else
        {
            if (cursorFixedPos.x > Screen.width / 2)
                x = -offset;

            if (cursorFixedPos.y > Screen.height / 2)
                y = -offset;

            viewContainer.localPosition = new Vector3(//0, 0, -100);
                cursorFixedPos.x - Screen.width / 2 + x, cursorFixedPos.y - Screen.height / 2 + y, 0);// 100);
        }

    }


    void IsGOSelected()
    {
        RaycastHit hit;
        Ray ray = mainCamera.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f));

        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            for (int i = 0; i < goMR.Count; ++i)
            {
                if (SelectEntity(hit.collider.gameObject) == goMR[i])
                {
                    //GrowView();
                    DisplayView();
                    Debug.Log("point cursor at " + goMR[i].name);
                    //if (view.material != inputView[i].material)
                    view.material = inputViewMR[i].material;

                    //view.material.mainTexture = inputView[i].material.mainTexture;

                    //if (!positionSet)
                    //{

                    //    //DisplayView();
                    //    positionSet = true;
                    //}

                    SetViewPosition();


                    //viewContainer.ForceUpdateRectTransforms();
                }
                else
                {
                    //positionSet = false;
                    //HideView();
                    //ShrinkView();
                }
            }

            // hololens
            if (hololensGO != null 
                && SelectEntity(hit.collider.gameObject) == hololensGO)
            {
                DisplayHololensMiniature();
            }

            // kinect
            if (kinectGO != null 
                && SelectEntity(hit.collider.gameObject) == kinectGO 
                && !viewChanger.IsKinectView())
            {
                DisplayKinectMiniature();
            }

            // virtual
            if (virtualGO != null 
                && SelectEntity(hit.collider.gameObject) == virtualGO
                && !viewChanger.IsVirtualView())
            {
                DisplayVirtualMiniature();
            }

            for (int i = 0; i < goC.Count; ++i)
            {
                if (SelectEntity(hit.collider.gameObject) == goC[i])
                {
                    DisplayView();
                    view.material = viewMaterial;
                    inputViewC[i].outputMaterialInstance = view;
                    SetViewPosition();
                }
                else
                {
                    inputViewC[i].outputMaterialInstance = null;
                }
            }
        }
        else
        {
            //positionSet = false;
            if (wasDisplaying && ((cursorFixedPos - Input.mousePosition).magnitude > mouseOffset))
            {
                HideView();
            }
            //ShrinkView();
        }
    }

    void DisplayView()
    {
        if(!wasDisplaying)
        {
            cursorFixedPos = Input.mousePosition;

        }
        viewContainer.gameObject.SetActive(true);
        wasDisplaying = true;
    }


    public void DisplayHololensMiniature()
    {
        if (!isPreviewUsedForViewPoint)
        {
            logger.LogStartPreviewHololens();
            isPreviewUsedForViewPoint = true;
        }

        DisplayView();
        //Debug.Log("point cursor at " + hololensGO.name);
        view.material = hololensViewMR.material;
        view.GetComponent<FadeMesh>().ShowImmediately();

        SetViewPosition();
    }

    public void DisplayHololensMiniatureStatic()
    {
        if (!isPreviewUsedForViewPoint)
        {
            logger.LogStartPreviewHololens();
            isPreviewUsedForViewPoint = true;
        }

        ActivateDisplayStatic();
        view.material = hololensViewMR.material;
        view.GetComponent<FadeMesh>().ShowImmediately();
    }

    public void DisplayVirtualMiniature()
    {
        if (!isPreviewUsedForViewPoint)
        {
            logger.LogStartPreviewVirtual();
            isPreviewUsedForViewPoint = true;
        }

        DisplayView();
        //Debug.Log("point cursor at " + virtualGO.name);
        view.material = viewMaterial;
        virtualGO.GetComponentInChildren<RedirectViewFromCamera>().outputMaterialInstance = view;
        SetViewPosition();
    }

    public void DisplayVirtualMiniatureStatic()
    {
        if (!isPreviewUsedForViewPoint)
        {
            logger.LogStartPreviewVirtual();
            isPreviewUsedForViewPoint = true;
        }


        RedirectViewFromCamera pipe = virtualGO.GetComponentInChildren<RedirectViewFromCamera>();
        Camera cam = virtualGO.GetComponentInChildren<Camera>();

        if (pipe == null) Debug.LogError("NO PIPE");
        if (cam == null) Debug.LogError("NO CAM");

        //view.material = viewMaterial;
        //ActivateDisplayStatic();

        DesactivateDisplayOnMouseOver();
        ConnectSpecificCamera(pipe, cam, virtualGO.transform.position, 0);
        ActivateDisplayStatic();

        //view.material = viewMaterial;
        //virtualGO.GetComponentInChildren<RedirectViewFromCamera>().outputMaterialInstance = view;
    }

    public void DisplayKinectMiniature()
    {
        if (!isPreviewUsedForViewPoint)
        {
            logger.LogStartPreviewKinect();
            isPreviewUsedForViewPoint = true;
        }

        DisplayView();
        //Debug.Log("point cursor at " + kinectGO.name);
        view.material = kinectViewMR.material;
        view.GetComponent<FadeMesh>().ShowImmediately();
        SetViewPosition();
    }

    public void DisplayKinectMiniatureStatic()
    {
        if (!isPreviewUsedForViewPoint)
        {
            logger.LogStartPreviewKinect();
            isPreviewUsedForViewPoint = true;
        }

        ActivateDisplayStatic();
        view.material = kinectViewMR.material;
        view.GetComponent<FadeMesh>().ShowImmediately();
    }

    public void HideView()
    {
        wasDisplaying = false;

        viewContainer.gameObject.SetActive(false);

        if (isPreviewUsedForViewPoint)
        {
            logger.LogStopPreview();
            isPreviewUsedForViewPoint = false;
        }

        //if (hololensViewMR != null)
        //    hololensViewMR.GetComponent<FadeMesh>().HideImmediately();
        
        //if (hololensViewMR != null)
        //    kinectViewMR.GetComponent<FadeMesh>().HideImmediately();
    }

    //void GrowView()
    //{
    //    if(view.rectTransform.localScale.x < maxImageSizeX)
    //    {
    //        view.rectTransform.localScale += 
    //            new Vector3(
    //                growingSpeed * Time.deltaTime,
    //                growingSpeed * Time.deltaTime,
    //                growingSpeed * Time.deltaTime);
    //    }
    //}

    //void ShrinkView()
    //{
    //    if (view.rectTransform.localScale.x > 0)
    //    {
    //        view.rectTransform.localScale -=
    //            new Vector3(
    //                growingSpeed * Time.deltaTime,
    //                growingSpeed * Time.deltaTime,
    //                growingSpeed * Time.deltaTime);
    //    }
    //}

    GameObject SelectEntity(GameObject meshChild)
    {
        while (meshChild.GetComponent<SelectableEntity>() == null
            && meshChild.transform.parent != null)
            meshChild = meshChild.transform.parent.gameObject;

        if (meshChild.GetComponent<SelectableEntity>() != null)
            return meshChild;
        else
            return null;
    }

    public void ActivateDisplayOnMouseOver()
    {
        displayOnMouseOver = true;
        displaySpecificCamera = false;
        displayStatic = false;
    }

    public void DesactivateDisplayOnMouseOver()
    {
        displayOnMouseOver = false;
    }

    public void ActivateDisplayStatic()
    {
        displayStatic = true;
        displaySpecificCamera = false;
        displayOnMouseOver = false;
    }

    public void DesactivateDisplayStatic()
    {
        displayStatic = false;
    }

    public void ConnectSpecificCamera(RedirectViewFromCamera pipe, Camera cam, Vector3 center, float offset)
    {
        DesactivateDisplayOnMouseOver(); // to be sure
        DesactivateDisplayStatic(); // to be sure

        displaySpecificCamera = true;
        view.material = viewMaterial;
        pipe.outputMaterialInstance = view;

        specificCamera = cam;
        specificCamera.cullingMask = ~(1 << 5);
        centerSpecificCamera = center;
        specificOffset = offset;
    }

}
#endif