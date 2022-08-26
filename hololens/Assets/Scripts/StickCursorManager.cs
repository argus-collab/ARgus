#if !UNITY_WSA
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickCursorManager : MonoBehaviour
{
    public LogManager logger;
    public CustomClientNetworkManager network;

    public Camera mainCamera;
    public maxCamera navigator1;
    private bool navigatorFreezed;
    public DisplayViewInUI viewInUI;
    //public FPSCamera navigator2;

    public float pointerDepthOffset = 0.5f;
    public float pointerDepthIncrementScale = 0.3f;
    public float pointerVerticalOffset = 0.5f;

    public float maxLengthStick = 20f;
    public float minLengthStick = 0.2f;

    //private GameObject pointer;
    private GameObject stickRepresentation;
    private GameObject topStick;

    private GameObject debugSphere;
    //private GameObject debugSphere2;
    private Vector3 hitPoint;
    private GameObject camChild;

    private float depth;

    private Camera cam;
    private RedirectViewFromCamera pipe;

    private bool popStick = true;

    private float lastDistance = 1f;
    private Color sphereColor;

    public Color onHitColor = Color.red;
    public Color onNotHitColor = Color.grey;

    public GameObject instruction;

    public GameObject sticker;
    private Vector3 stickerPosition;
    private Quaternion stickerOrientation;
    public List<GameObject> stickerInstances;

    private float delayPop = 0f;
    private float timeStampPop;
    private bool pop = false;

    //public LineRenderer dottedProjectionLine;
    public float contactTolerance = 0.002f;

    //private Color colorStick;
    //private Color colorTopStick;

    private bool isStickUsed = false;

    public bool IsStickUsed()
    {
        return stickRepresentation != null;
    }

    void Start()
    {
        stickerInstances = new List<GameObject>();

        sphereColor = onNotHitColor;

        // debug
        debugSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        debugSphere.name = "cursorStick";// GameObject.CreatePrimitive(PrimitiveType.Sphere);
        debugSphere.layer = LayerMask.NameToLayer("Cursor");
        debugSphere.transform.localScale = new Vector3(0.03f, 0.03f, 0.03f);
        debugSphere.SetActive(false);

        camChild = new GameObject();
        camChild.transform.parent = debugSphere.transform;
        camChild.transform.localPosition = new Vector3(0, 2f, -5f);
        camChild.transform.localRotation = Quaternion.identity;

        cam = camChild.AddComponent<Camera>();
        cam.nearClipPlane = 0.05f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(1f/255f,122f/255f,204f/255f);
        cam.cullingMask &= ~(1 << LayerMask.NameToLayer("InvisibleMiniature"));


        pipe = camChild.AddComponent<RedirectViewFromCamera>();
        pipe.inputCamera = cam;


        depth = mainCamera.nearClipPlane + 2f;
    }

    private void OnDisable()
    {
        RemoveStick();
        instruction.SetActive(false);
    }

    private void OnEnable()
    {
        instruction.SetActive(true);
    }

    void UpdateInstance()
    {
        ManipulationStick manip = GameObject.FindObjectOfType<ManipulationStick>();
        if (manip != null)
        {
            stickRepresentation = manip.gameObject;
            //colorStick = stickRepresentation.GetComponent<Renderer>().material.color;
        }


        TopStick manip2 = GameObject.FindObjectOfType<TopStick>();
        if (manip2 != null)
        {
            topStick = manip2.gameObject;
            topStick.transform.position = debugSphere.transform.position;
            topStick.transform.rotation = debugSphere.transform.rotation;
            topStick.transform.localScale = debugSphere.transform.localScale;
            //colorTopStick = topStick.GetComponent<Renderer>().material.color;
        }

        stickerInstances.Clear();
        Sticker[] manip3 = GameObject.FindObjectsOfType<Sticker>();
        if (manip3 != null)
        {
            for (int i = 0; i < manip3.Length; ++i)
            {
                //if(!stickerInstances.Contains(manip3[i].gameObject))
                //{
                manip3[i].gameObject.transform.position = stickerPosition;
                manip3[i].gameObject.transform.rotation = stickerOrientation;
                stickerInstances.Add(manip3[i].gameObject);
                //}
            }
        }
    }

    void UpdatePositions()
    {
        if (topStick != null)
        {
            topStick.transform.position = debugSphere.transform.position;
            topStick.transform.rotation = debugSphere.transform.rotation;
            topStick.transform.localScale = debugSphere.transform.localScale;
        }
    }

    void ChangeTransparency(GameObject go, float a)
    {
        MeshRenderer r = go.GetComponent<MeshRenderer>();
        
        if (r != null && r.material != null)
        {
            Color c = r.material.color;
            c.a = a;
            r.material.color = c;
        }

        for (int i = 0; i < go.transform.childCount; ++i)
            ChangeTransparency(go.transform.GetChild(i).gameObject, a);
    }

    void SetOutOfWorkspaceStickRepresentation()
    {
        if (stickRepresentation != null && topStick != null)
        {
            ChangeTransparency(stickRepresentation, 0.3f);
            ChangeTransparency(topStick, 0.3f);
        }
    }

    void SetIntoWorkspaceStickRepresentation()
    {
        if (stickRepresentation != null && topStick != null)
        {
            ChangeTransparency(stickRepresentation, 1);
            ChangeTransparency(topStick, 1);
        }
    }

    void Update()
    {
        float xMouse = Input.mousePosition.x;
        float yMouse = Input.mousePosition.y;

        UpdateInstance();
         
        float maxDistance = 10000f;
        float distance = (mainCamera.transform.position - debugSphere.transform.position).magnitude;
        //float distance = GetDistance(Input.mousePosition.x, Input.mousePosition.y);

        RaycastHit hit;
        Ray ray = mainCamera.ScreenPointToRay(new Vector3(xMouse, yMouse, 0f));
        debugSphere.GetComponent<Renderer>().material.color = onNotHitColor;
        if (topStick != null)
            topStick.GetComponent<Renderer>().material.color = onNotHitColor;

        if (debugSphere != null)
            hitPoint = debugSphere.transform.position;
        else
            hitPoint = Vector3.zero;

        int mask = ~(1 << LayerMask.NameToLayer("Cursor"));
        mask &= ~(1 << LayerMask.NameToLayer("ParticipantPlayer"));
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, mask))
        {
            SetIntoWorkspaceStickRepresentation();

            maxDistance = (mainCamera.transform.position - hit.point).magnitude;
            hitPoint = hit.point;

            if (distance > maxDistance)
            {
                distance = maxDistance;
            }

            if (topStick != null && (topStick.transform.position - hitPoint).magnitude < contactTolerance)
            {
                debugSphere.GetComponent<Renderer>().material.color = onHitColor;
                topStick.GetComponent<Renderer>().material.color = onHitColor;
            }
        }
        else
        {
            SetOutOfWorkspaceStickRepresentation();
            hitPoint = mainCamera.ScreenToWorldPoint(new Vector3(xMouse, yMouse, distance));
        }

        if (distance > maxLengthStick)
            distance = maxLengthStick;

        if (distance < minLengthStick)
            distance = minLengthStick;

        // erase stickers
        if (Input.GetKey(KeyCode.C))
        {
            RemoveSticker();
        }

        // control from user
        if (Input.GetKey(KeyCode.Space))
        {
            if (!isStickUsed)
            {
                isStickUsed = true;
                logger.LogStartStickUse();
            }

            if (popStick)
            {
                popStick = false;
                AddStick();
            }

            if (stickRepresentation != null)
            {
                stickRepresentation.transform.position = mainCamera.transform.position - pointerVerticalOffset * mainCamera.transform.up;// + mainCamera.transform.rotation * new Vector3(0, -0.5f, distance / 2 + 0.5f);
                                                                                                                                         //stickRepresentation.transform.GetChild(0).localPosition = new Vector3(0, 0, distance / 2 - 0.5f);

                float sticklength = (stickRepresentation.transform.position - debugSphere.transform.position).magnitude;
                stickRepresentation.transform.localScale = new Vector3(2f, 2f, sticklength / 2);// - 0.5f);
                //stickRepresentation.transform.localScale = new Vector3(distance, distance, distance / 2f);// - 0.5f);

                viewInUI.DesactivateDisplayOnMouseOver();
                viewInUI.ConnectSpecificCamera(pipe, cam, Vector3.zero, 200);
                viewInUI.ActivateDisplayStatic();

                navigator1.isActive = false;
                navigatorFreezed = true;

                float localPointerDepthIncrementScale = pointerDepthIncrementScale;

                if (Input.mouseScrollDelta.y > 0 && (maxDistance - depth) <= pointerDepthIncrementScale)
                {
                    localPointerDepthIncrementScale = (0.5f) * (maxDistance - depth) * (1f / Input.mouseScrollDelta.y);
                }

                depth += Input.mouseScrollDelta.y * localPointerDepthIncrementScale;

                if (depth > maxDistance)
                    depth = maxDistance;

                if (depth > maxLengthStick)
                    depth = maxLengthStick;


                if (depth < minLengthStick)
                    depth = minLengthStick;

                Vector3 point;
                if (hitPoint.x==0 && hitPoint.y==0 && hitPoint.z==0)
                {
                    point = mainCamera.ScreenToWorldPoint(new Vector3(xMouse, yMouse, depth));
                }
                else
                {
                    point = mainCamera.transform.position + depth * (hitPoint - mainCamera.transform.position).normalized;
                }
                                                                                     
                debugSphere.transform.position = point;
                debugSphere.transform.rotation = Quaternion.LookRotation(mainCamera.transform.forward, mainCamera.transform.up);

                stickRepresentation.transform.LookAt(point);
            }

            if (Input.GetKeyDown(KeyCode.V))
            {
                AddSticker();
            }
        }
        else
        {

            if (isStickUsed)
            {
                isStickUsed = false;
                logger.LogStopStickUse();
            }


            if (navigatorFreezed)
            {
                navigatorFreezed = false;
                navigator1.isActive = true;
            }

            if ((stickRepresentation != null || topStick != null) && !popStick)//;!popStick)
            {
                RemoveStick();
                viewInUI.HideView();
                viewInUI.ActivateDisplayOnMouseOver();

                if (stickRepresentation != null)
                    stickRepresentation.transform.localScale = new Vector3(0.05f, 0f, 0.05f);

            }
            
            if (stickRepresentation == null && topStick == null)
            {
                popStick = true;
            }

        }

        UpdatePositions();
    }

    void AddStick(bool log = false)
    {
        Debug.Log("add stick");
        network.SendCommandMessage("scene-manager-command", "pop GrowingStick ok");
        network.SendCommandMessage("scene-manager-command", "pop TopStick ok");
        debugSphere.SetActive(true);

        if (log)
            logger.LogStartStickUse();
    }

    void RemoveStick(bool log = false)
    {
        Debug.Log("remove stick");
        network.SendCommandMessage("scene-manager-command", "remove GrowingStick ok");
        network.SendCommandMessage("scene-manager-command", "remove TopStick ok");
        debugSphere.SetActive(false);
        
        if (log)
            logger.LogStopStickUse();
    }

    void AddSticker()
    {
        network.SendCommandMessage("scene-manager-command", "pop " + sticker.name + " ok");

        // sol1
        //stickerPosition = debugSphere.transform.position;
        //stickerOrientation = debugSphere.transform.rotation;

        // sol2
        RaycastHit hit1;
        Vector3 direction = mainCamera.transform.forward;
        if (stickRepresentation != null) direction = stickRepresentation.transform.forward;


        Ray ray1 = new Ray(debugSphere.transform.position, direction);
        int mask1 = ~(1 << LayerMask.NameToLayer("Cursor"));
        mask1 &= ~(1 << LayerMask.NameToLayer("ParticipantPlayer"));
        if (Physics.Raycast(ray1, out hit1, Mathf.Infinity, mask1))
        {
            stickerPosition = hit1.point;
            stickerOrientation = Quaternion.identity;
        }

        logger.LogPlaceSticker();
    }

    void RemoveSticker()
    {
        for (int i = 0; i < stickerInstances.Count; ++i)
        {
            if (stickerInstances[i] != null)
                network.SendCommandMessage("scene-manager-command", "remove " + stickerInstances[i].name + " ok");
        }
    }
}
#endif