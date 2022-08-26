#if !UNITY_WSA
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SphericalVisualization : MonoBehaviour
{
    public LogManager logger;
    public CustomClientNetworkManager network;
    public bool isActive = true;
    public Camera mainCamera;
    private GameObject sphere;
    private GameObject stick;
    private GameObject cursor;
    private bool isSphereDisplayed = false;
    //public float distanceFromSphere = 0.5f;
    public float offsetFromSphere = 0.05f;
    public float maxMouseDisplacement = 600;

    public GameObject sphereCenter1;
    public GameObject sphereCenter2;
    public GameObject sphereCenter3;

    public DisplayViewInUI miniature;
    public ViewSelection viewSelector;

    public maxCamera navigator;

    private GameObject cursorLocal;

    private bool isManipulating = false;
    private Vector2 initialMousePosition;

    private Vector3 facingVector;
    private Vector3 lateralVector;
    private Vector3 upVector;

    public Texture2D mouseIcon;
    public Texture2D deleteIcon;
    public Texture2D cameraIcon;

    //public List<GameObject> selected;
    public GameObject selected;

    public float sphereSizeIncrement = 0.01f;

    // infobulle
    private bool displayTextOnCursorSphere = false;
    private bool displayTextOnCursorCamera = false;
    private bool displayTextOnCursorDelete = false;
    public GameObject helpIcon;
    private Image helpIconImage;
    public GameObject infobulle;
    public GameObject cursorHighlight;
    public string textInfobulleSphere = "Spherical viewpoint";
    public string textInfobulleCamera = "Move camera";
    public string textInfobulleDelete = "Delete";
    public Sprite iconSphere;
    public Sprite iconCamera;
    public Sprite iconDelete;
    private Text infobulleText;
    private Image infobulleBackground;
    private RectTransform infobulleRectTransform;
    public float offset = 70f;

    private bool cursorChanged = false;

    public GameObject instructionsSphere;

    public ChangeViewCinemachine changeView;
    public VirtualViewPositionRecorder recorder;

    private int i = -1;
    private Camera toGo;

    private Vector2 cursorFixedPos;
    private float mouseOffset = 5;

    private bool isSphericalVisualizationUsed = false;

    private float clickTimeStamp;
    private float clickDuration = 0.3f;

    void ResizeIcon()
    {
        //mouseIcon.Resize(500, 500);
        //deleteIcon.Resize(500, 500);
        //cameraIcon.Resize(500, 500);

        //mouseIcon.Apply();
        //deleteIcon.Apply();
        //cameraIcon.Apply();
    }

    void Start()
    {
        //selected = new List<GameObject>();

        infobulleBackground = infobulle.GetComponent<Image>();
        infobulleText = infobulle.GetComponentInChildren<Text>();
        infobulleRectTransform = infobulle.GetComponent<RectTransform>();

        helpIconImage = helpIcon.transform.GetChild(0).GetComponent<Image>();

        ResizeIcon();
    }

    void OnGUI()
    {
        // infobulle
        Event e = Event.current;
        Vector2 guipos;
        guipos.x = e.mousePosition.x;
        guipos.y = e.mousePosition.y;

        if (displayTextOnCursorSphere)
        {
            infobulleText.text = textInfobulleSphere;
            helpIconImage.sprite = iconSphere;
            infobulle.GetComponent<Infobulle>().ShowAtNextUpdate(cursorFixedPos);
            helpIcon.GetComponent<Infobulle>().ShowAtNextUpdate(cursorFixedPos);
            cursorHighlight.GetComponent<Infobulle>().ShowAtNextUpdate(cursorFixedPos);

        }

        if (displayTextOnCursorCamera)
        {
            infobulleText.text = textInfobulleCamera;
            helpIconImage.sprite = iconCamera;
            infobulle.GetComponent<Infobulle>().ShowAtNextUpdate(cursorFixedPos);
            helpIcon.GetComponent<Infobulle>().ShowAtNextUpdate(cursorFixedPos);
            cursorHighlight.GetComponent<Infobulle>().ShowAtNextUpdate(cursorFixedPos);

        }

        if (displayTextOnCursorDelete)
        {
            infobulleText.text = textInfobulleDelete;
            helpIconImage.sprite = iconDelete;
            infobulle.GetComponent<Infobulle>().ShowAtNextUpdate(cursorFixedPos);
            helpIcon.GetComponent<Infobulle>().ShowAtNextUpdate(cursorFixedPos);
            cursorHighlight.GetComponent<Infobulle>().ShowAtNextUpdate(cursorFixedPos);

        }
    }

    private void OnEnable()
    {
        RemoveSphere();
        selected = null;
    }

    private void OnDisable()
    {
        navigator.isActive = true;
        RemoveSphere();
        ReleaseCursor();
        isManipulating = false;
    }

    private void OnApplicationQuit()
    {
        ReleaseCursor();
        RemoveSphere();
    }

    private void SynchronizeManipulationObjects()
    {
        if (isSphereDisplayed && sphere == null)
        {
            ManipulationSphere manip = GameObject.FindObjectOfType<ManipulationSphere>();
            if (manip != null)
            {
                sphere = manip.gameObject;
                sphere.transform.position = GetBarycenter(selected, 0.1f);

                float maxBounding = GetMax(GetSizeGlobalBoundingBox(selected, sphere.transform.position)) / 2;
                sphere.transform.localScale = new Vector3(maxBounding, maxBounding, maxBounding) + new Vector3(offsetFromSphere, offsetFromSphere, offsetFromSphere);
                sphere.transform.localScale -= 0.2f * sphere.transform.localScale;
                //sphere.transform.localScale = new Vector3(maxBounding, maxBounding, maxBounding) + new Vector3(0.05f, 0.05f, 0.05f);
            }
        }

        if (isSphereDisplayed && stick == null)
        {
            ManipulationStick manip2 = GameObject.FindObjectOfType<ManipulationStick>();
            if (manip2 != null)
            {
                stick = manip2.gameObject;
                Vector3 stickScale = stick.transform.localScale;
                stickScale.z = 0;
                stick.transform.localScale = stickScale;
            }
        }

        if (isSphereDisplayed && cursor == null)
        {
            ManipulationCursor manip3 = GameObject.FindObjectOfType<ManipulationCursor>();
            if (manip3 != null)
                cursor = manip3.gameObject;
        }

        //if(isSphereDisplayed && sphere != null && stick != null && cursor != null)
        //{
        //    isManipulating = true;
        //}
    }

    bool IsSelectableEntityVisual(GameObject go)
    {
        SelectableEntityVisualization[] ent = GameObject.FindObjectsOfType<SelectableEntityVisualization>();
        bool res = false;
        GameObject entityGO = SelectEntity(go);
        foreach (SelectableEntityVisualization e in ent)
            res = res || entityGO == e.gameObject;
        return res;
    }

    float Clamp(float val, float min, float max, float x, float y)
    {
        float a = (max - min) / (y - x);
        float b = min - a * x;
        float res = a * val + b;

        if (res > max) res = max;
        if (res < min) res = min;

        return res;
    }

    private void Update()
    {

        if (i >= 0)
            i++;

        // we wait for cinemachine to be ready 
        if (i == 10)
        {
            changeView.DisplayCameraView(toGo, true, true, true);
            i = -1;
        }



        SynchronizeManipulationObjects();







        RaycastHit hitMain;
        Ray rayMain = mainCamera.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f));

        if (Physics.SphereCast(rayMain, /*circleSize*/ 0.03f, out hitMain, Mathf.Infinity))
        {
            if (selected == null && IsSelectableEntityVisual(hitMain.collider.gameObject))
            {
                cursorFixedPos = mainCamera.WorldToScreenPoint(hitMain.collider.gameObject.transform.position);

                if (!cursorChanged)
                    Cursor.SetCursor(mouseIcon, new Vector2(250, 250), CursorMode.Auto);
                displayTextOnCursorSphere = true;
                cursorChanged = true;

                if (!isManipulating && Input.GetMouseButton(1))
                {
                    if(Time.time - clickTimeStamp > clickDuration)
                    {
                        GameObject entity = SelectEntity(hitMain.collider.gameObject);
                        Debug.Log("hit : " + hitMain.collider.gameObject.name);
                        if (entity != null)
                        {
                            selected = entity;
                        }

                        AddSphere();

                        if (!isSphericalVisualizationUsed)
                        {
                            isSphericalVisualizationUsed = true;
                            logger.LogStartSphericalCameraUse();
                        }

                        instructionsSphere.SetActive(true);

                        isManipulating = true;
                        initialMousePosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);

                        navigator.isActive = false;
                    }
                }
                else
                {
                    clickTimeStamp = Time.time;
                }
            }
            else
            {
                ResetCursor();
            }
        }
        else
        {
            ResetCursor();
        }








        //if (Input.GetButton("Fire1"))
        if (isManipulating && Input.GetMouseButton(1))
        {
            navigator.isActive = false;

            if (sphere != null && Input.GetAxis("Mouse ScrollWheel") != 0)
            {
                if (Input.GetAxis("Mouse ScrollWheel") > 0)
                    sphere.transform.localScale -= sphere.transform.localScale * 0.2f;
                else
                    sphere.transform.localScale += sphere.transform.localScale * 0.2f;

            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                EndManipulation();
            }

            if (sphere != null && cursorLocal == null)
            {
                CreateCursor();

                facingVector = Vector3.Normalize(mainCamera.transform.position - sphere.transform.position);
                upVector = mainCamera.transform.up;
                lateralVector = Vector3.Normalize(Vector3.Cross(facingVector, mainCamera.transform.up));
            }

            if (sphere != null && cursorLocal != null)
            {
                float theta = (Input.mousePosition.x - initialMousePosition.x) * 3 * (Mathf.PI / 2) / maxMouseDisplacement;
                float phi = (Input.mousePosition.y - initialMousePosition.y) * 3 * (Mathf.PI / 2) / maxMouseDisplacement;

                //Debug.Log("theta = " + 360 * theta / (2 * Mathf.PI));
                //Debug.Log("phi = " + 360 * phi / (2 * Mathf.PI));

                //Vector3 cursorPos;
                float x = (sphere.transform.localScale.x / 2) * Mathf.Cos(phi) * Mathf.Cos(theta);
                float y = (sphere.transform.localScale.y / 2) * Mathf.Cos(phi) * Mathf.Sin(theta);
                float z = (sphere.transform.localScale.z / 2) * Mathf.Sin(phi);

                cursorLocal.transform.position = sphere.transform.position + x * facingVector + y * lateralVector + z * upVector;
                if (cursor != null)
                    cursor.transform.position = sphere.transform.position + x * facingVector + y * lateralVector + z * upVector;


                // debug
                //----------------------------------------------------------------------------------------------
                // cylindrical coordinates instead
                ////x = (sphere.transform.localScale.x / 2) * Mathf.Cos(theta);
                //phi = Clamp(sphere.transform.localScale.x / 2, 10 * Mathf.Deg2Rad, 60 * Mathf.Deg2Rad, 0.02f, 1);
                ////z = (sphere.transform.localScale.z / 2) * Mathf.Sin(theta);

                //x = (sphere.transform.localScale.x / 2) * Mathf.Cos(phi) * Mathf.Cos(theta);
                //y = (sphere.transform.localScale.y / 2) * Mathf.Cos(phi) * Mathf.Sin(theta);
                //z = (sphere.transform.localScale.z / 2) * Mathf.Sin(phi);
                //cursorLocal.transform.position = sphere.transform.position + new Vector3(x, z, y);
                //if (cursor != null)
                //    cursor.transform.position = sphere.transform.position + new Vector3(x, z, y);
                //----------------------------------------------------------------------------------------------



                if (stick != null)
                {
                    float xStick = (sphere.transform.localScale.x / 4) * Mathf.Cos(phi) * Mathf.Cos(theta);
                    float yStick = (sphere.transform.localScale.y / 4) * Mathf.Cos(phi) * Mathf.Sin(theta);
                    float zStick = (sphere.transform.localScale.z / 4) * Mathf.Sin(phi);
                    stick.transform.position = sphere.transform.position + xStick * facingVector + yStick * lateralVector + zStick * upVector;


                    // debug
                    //----------------------------------------------------------------------------------------------
                    // cylindrical coordinates instead
                    //phi = Clamp(sphere.transform.localScale.x / 2, 10 * Mathf.Deg2Rad, 60 * Mathf.Deg2Rad, 0.02f, 1);
                    //x = (sphere.transform.localScale.x / 4) * Mathf.Cos(phi) * Mathf.Cos(theta);
                    //y = (sphere.transform.localScale.y / 4) * Mathf.Cos(phi) * Mathf.Sin(theta);
                    //z = (sphere.transform.localScale.z / 4) * Mathf.Sin(phi);
                    //stick.transform.position = sphere.transform.position + new Vector3(x, z, y);
                    //----------------------------------------------------------------------------------------------


                    Vector3 stickScale = stick.transform.localScale;
                    stickScale.x = 10f * sphere.transform.localScale.x;
                    stickScale.y = 10f * sphere.transform.localScale.y;
                    stickScale.z = sphere.transform.localScale.z / 4;
                    stick.transform.localScale = stickScale;

                    Vector3 vectorScale = sphere.transform.localScale;
                    vectorScale.x *= 0.2f;
                    vectorScale.y *= 0.2f;
                    vectorScale.z *= 0.2f;
                    cursorLocal.transform.localScale = vectorScale;
                    cursor.transform.localScale = vectorScale;

                    stick.transform.LookAt(sphere.transform.position);
                    cursorLocal.transform.LookAt(sphere.transform.position);
                    if (cursor != null)
                        cursor.transform.LookAt(sphere.transform.position);



                    // debug
                    //----------------------------------------------------------------------------------------------
                    // cylindrical coordinates instead
                    //stick.transform.LookAt(sphere.transform.position);
                    //Vector3 rotEuler = stick.transform.rotation.eulerAngles;
                    //rotEuler.x = Clamp(sphere.transform.localScale.z / 2, 15, 60, 0.2f, 1);
                    //cursorLocal.transform.rotation = Quaternion.Euler(rotEuler);
                    //if (cursor != null)
                    //    cursor.transform.rotation = Quaternion.Euler(rotEuler);
                    //----------------------------------------------------------------------------------------------
                }
            }





            //if(Input.GetMouseButtonDown(1))
            //{
            //    Debug.Log("redirect");

            //    recorder.RegisterInvisibleCameraPosition(cursorLocal.transform.position, cursorLocal.transform.rotation);
            //    i = 0;
                
            //    //toGo = cursorLocal.GetComponent<Camera>();

            //    //changeView.DisplayCameraView(cursorLocal.GetComponent<Camera>());

            //    GameObject virtualPos = recorder.GetLastVirtualCameraRepresentation();
            //    toGo = virtualPos.GetComponentInChildren<Camera>();
            //    Debug.Log("go to " + toGo.gameObject.name);
            //    i = 0;
            //}

        }
        //else if (Input.GetKey(KeyCode.Plus) || Input.GetKey(KeyCode.KeypadPlus))
        //{
        //    float scale = 0.01f;
        //    sphere.transform.localScale += new Vector3(sphereSizeIncrement * scale, sphereSizeIncrement * scale, sphereSizeIncrement * scale);
        //}
        //else if (Input.GetKey(KeyCode.Minus) || Input.GetKey(KeyCode.KeypadMinus))
        //{
        //    float scale = 0.01f;
        //    sphere.transform.localScale -= new Vector3(sphereSizeIncrement * scale, sphereSizeIncrement * scale, sphereSizeIncrement * scale);
        //}
        else
        {
            if (isManipulating)
            {
                Teleport();
            }

            EndManipulation();

            if (isSphericalVisualizationUsed)
            {
                isSphericalVisualizationUsed = false;
                logger.LogStopSphericalCameraUse();
            }
        }
    }

    void EndManipulation()
    {
        if (isManipulating)
        {
            ReleaseCursor();
            RemoveSphere(true);
            selected = null;
            isManipulating = false;
            instructionsSphere.SetActive(false);
            navigator.isActive = true;
        }
    }

    void Teleport()
    {
        if (cursorLocal == null) return;

        recorder.RegisterInvisibleCameraPosition(cursorLocal.transform.position, cursorLocal.transform.rotation);

        GameObject virtualPos = recorder.GetLastVirtualCameraRepresentation();
        toGo = virtualPos.GetComponentInChildren<Camera>();
        Debug.Log("go to " + toGo.gameObject.name);
        i = 0;

        changeView.TeleportVirtualCamera(mainCamera.transform.position, mainCamera.transform.rotation);
    }

    void ResetCursor()
    {
        if (cursorChanged)
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            displayTextOnCursorSphere = false;
            displayTextOnCursorCamera = false;
            displayTextOnCursorDelete = false;
            cursorChanged = false;
        }

    }

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

    void CreateCursor()
    {
        cursorLocal = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        cursorLocal.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        cursorLocal.transform.position = sphere.transform.position + (sphere.transform.localScale.x / 2) * Vector3.up;

        Camera cam = cursorLocal.AddComponent<Camera>();
        cam.nearClipPlane = 0.001f;
        cam.fieldOfView = mainCamera.fieldOfView;

        RedirectViewFromCamera pipe = cursorLocal.AddComponent<RedirectViewFromCamera>();
        pipe.inputCamera = cam;
        cam.backgroundColor = mainCamera.backgroundColor;
        cam.clearFlags = mainCamera.clearFlags; 

        miniature.DesactivateDisplayOnMouseOver();
        miniature.ConnectSpecificCamera(pipe, cam, sphere.transform.position, 200);
        miniature.ActivateDisplayStatic();

        cam.cullingMask &= ~(1 << LayerMask.NameToLayer("Cursor"));
        cam.cullingMask &= ~(1 << LayerMask.NameToLayer("InvisibleMiniature"));

        network.SendCommandMessage("scene-manager-command", "pop Cursor ok");
        network.SendCommandMessage("scene-manager-command", "pop FixedStick ok");
    }

    void ReleaseCursor()
    {
        if(cursorLocal!=null)
            Destroy(cursorLocal);
        miniature.HideView();
        miniature.ActivateDisplayOnMouseOver();
        network.SendCommandMessage("scene-manager-command", "remove Cursor ok");
        network.SendCommandMessage("scene-manager-command", "remove FixedStick ok");
    }

    public void SetSphereAround(List<GameObject> objects)
    {
        sphere.transform.position = GetBarycenter(objects);
        float maxBounding = GetMax(GetSizeGlobalBoundingBox(objects, sphere.transform.position));
        sphere.transform.localScale = new Vector3(maxBounding, maxBounding, maxBounding) + new Vector3(offsetFromSphere, offsetFromSphere, offsetFromSphere);
    }

    float GetMax(Vector3 v)
    {
        if (v.x >= v.y && v.x >= v.z)
            return v.x;
        else if (v.y >= v.x && v.y >= v.z)
            return v.y;
        else
            return v.z;
    }

    /*
     * barycenter of mesh bounding box, weighted by volumes of bounding box
     */
    Vector3 GetBarycenter(GameObject obj, float yOffset = 0f)
    {
        Vector3 pos = obj.transform.position;
        pos.y += yOffset;
        return pos;
    }

    Vector3 GetBarycenter(List<GameObject> objects)
    {
        Vector3 barycenter = Vector3.zero;

        //foreach (GameObject go in objects)
        //{
        //    barycenter += go.transform.position;
        //    Debug.Log(go.name + "> " + go.transform.position);
        //}

        float massSum = 0;

        for (int i = 0; i < objects.Count; ++i)
        {
            MeshRenderer[] rends = objects[i].GetComponentsInChildren<MeshRenderer>();

            if (rends != null)
            {
                for (int j = 0; j < rends.Length; ++j)
                {
                    float mass = rends[j].bounds.size.x * rends[j].bounds.size.y * rends[j].bounds.size.z; 
                    barycenter += rends[j].bounds.center * mass;
                    massSum += mass;
                }
            }
        }

        barycenter.x /= massSum;
        barycenter.y /= massSum;
        barycenter.z /= massSum;

        Debug.Log("barycenter : " + barycenter);

        return barycenter;
    }

    float max(float a, float b)
    {
        return a > b ? a : b;
    }

    Vector3 GetSizeGlobalBoundingBox(List<GameObject> objects, Vector3 center)
    {
        Vector3 size = Vector3.zero;
        int nbMesh = 0;

        for(int i = 0; i < objects.Count; ++i)
        {
            MeshRenderer[] rends = objects[i].GetComponentsInChildren<MeshRenderer>();

            if (rends != null)
            {
                for(int j = 0; j < rends.Length; ++j)
                {
                    //Vector3 pos = objects[i].transform.position;
                    Vector3 pos = rends[j].bounds.center;

                    float rendMax = Mathf.Sqrt(Mathf.Pow(rends[j].bounds.size.x, 2) + Mathf.Pow(rends[j].bounds.size.y, 2) + Mathf.Pow(rends[j].bounds.size.z, 2));

                    size.x = max(size.x, Mathf.Abs(pos.x - center.x) + rendMax / 2);
                    size.y = max(size.y, Mathf.Abs(pos.y - center.y) + rendMax / 2);
                    size.z = max(size.z, Mathf.Abs(pos.z - center.z) + rendMax / 2);

                    nbMesh++;
                }
            }
        }

        size.x *= 2;
        size.y *= 2;
        size.z *= 2;

        return size;
    }

    Vector3 GetSizeGlobalBoundingBox(GameObject obj, Vector3 center)
    {
        Vector3 size = Vector3.zero;
        int nbMesh = 0;


        MeshRenderer[] rends = obj.GetComponentsInChildren<MeshRenderer>();

        if (rends != null)
        {
            for (int j = 0; j < rends.Length; ++j)
            {
                //Vector3 pos = objects[i].transform.position;
                Vector3 pos = rends[j].bounds.center;

                float rendMax = Mathf.Sqrt(Mathf.Pow(rends[j].bounds.size.x, 2) + Mathf.Pow(rends[j].bounds.size.y, 2) + Mathf.Pow(rends[j].bounds.size.z, 2));

                size.x = max(size.x, Mathf.Abs(pos.x - center.x) + rendMax / 2);
                size.y = max(size.y, Mathf.Abs(pos.y - center.y) + rendMax / 2);
                size.z = max(size.z, Mathf.Abs(pos.z - center.z) + rendMax / 2);

                nbMesh++;
            }
        }

        size.x *= 2;
        size.y *= 2;
        size.z *= 2;

        return size;
    }

    void AddSphere(bool log = false)
    {
        isSphereDisplayed = true;
        network.SendCommandMessage("scene-manager-command", "pop 3DMouseViewPointSphere ok");

        if (log)
            logger.LogStartSphericalCameraUse();

        navigator.isActive = false;
    }

    void RemoveSphere(bool log = false)
    {
        isSphereDisplayed = false;
        network.SendCommandMessage("scene-manager-command", "remove 3DMouseViewPointSphere ok");
        
        if (log)
            logger.LogStopSphericalCameraUse();
    }
}
#endif