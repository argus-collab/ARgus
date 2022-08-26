#if !UNITY_WSA
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ViewSelection : MonoBehaviour
{
    public int circleSizeIncrement = 10;
    public Camera RenderingCamera;
    public Toggle selectionMode;
    public maxCamera MouseNavigator;
    public Camera mainCamera;
    //public Material selectedMaterial;

    public Texture2D cursorTexture;

    public ChangeViewCinemachine viewChanger; 

    public GameObject KinectGO;
    public GameObject HololensGO;
    public GameObject VirtualGO;

    public Color circleColor;


    [Header("debug")]
    public GameObject selected;
    //public GameObject meshSelected;

    private Material initialMaterial;

    private bool isActive = false;

    private int circleSize = 0;
    private int circleSizeMax = 200;

    private bool isDrawing = false;

    private Texture2D text;

    private int width;
    private int height;

    private Vector3 lastInputPosition;

    // infobulle
    private bool displayTextOnCursor = false;
    public GameObject helpIcon;
    private Image helpIconImage;
    public GameObject infobulle;
    public string textInfobulle = "Change viewpoint";
    public Sprite icon;
    private Text infobulleText;
    private Image infobulleBackground;
    private RectTransform infobulleRectTransform;
    public float offset = 70f;
    public GameObject cursorHighlight;

    private bool cursorChanged = false;

    public GameObject highlightedObject;
    public GameObject circleBlinker;

    private Vector3 cursorFixedPos;
    private float mouseOffset = 50;

    //private List<GameObject> selected;
    //private List<Material> initialMaterial;

    void ResizeIcon()
    {
        //cursorTexture.Resize(500, 500);
        //cursorTexture.Apply();
    }


    void Start()
    {
        infobulleBackground = infobulle.GetComponent<Image>();
        infobulleText = infobulle.GetComponentInChildren<Text>();
        infobulleRectTransform = infobulle.GetComponent<RectTransform>();

        helpIconImage = helpIcon.transform.GetChild(0).GetComponent<Image>();

        initTexture();

        ResizeIcon();
    }

    private void OnDisable()
    {
        ChangeToNormalCursor();
    }

    void OnGUI()
    {
        // infobulle
        Event e = Event.current;
        Vector2 guipos;
        guipos.x = e.mousePosition.x;
        guipos.y = e.mousePosition.y;

        if (displayTextOnCursor)
        {

            //.anchoredPosition = new Vector3(guipos.x, guipos.y);
            //Vector2 localPoint;
            //Vector2 mousePosInfobulle = Input.mousePosition;
            //Vector2 mousePosHelpIcon = Input.mousePosition;

            //float halfHeight = mainCamera.scaledPixelHeight / 2;
            //float halfWidth = mainCamera.scaledPixelWidth / 2;

            //// offset infobulle
            //if (mousePosInfobulle.y > halfHeight)
            //    mousePosInfobulle.y += offset;
            //else
            //    mousePosInfobulle.y -= offset;

            //if (mousePosInfobulle.x > halfWidth)
            //    mousePosInfobulle.x += offset;
            //else
            //    mousePosInfobulle.x -= offset;

            //// offset help icon
            //if (mousePosHelpIcon.x > halfWidth)
            //    mousePosHelpIcon.x += offset;
            //else
            //    mousePosHelpIcon.x -= offset;

            //RectTransformUtility.ScreenPointToLocalPointInRectangle(infobulle.transform.parent.GetComponent<RectTransform>(), mousePosInfobulle, null, out localPoint);
            //infobulle.transform.localPosition = localPoint;

            //RectTransformUtility.ScreenPointToLocalPointInRectangle(helpIcon.transform.parent.GetComponent<RectTransform>(), mousePosHelpIcon, null, out localPoint);
            //helpIcon.transform.localPosition = localPoint;

            //Debug.Log(localPoint);

            infobulleText.text = textInfobulle;
            helpIconImage.sprite = icon;
            infobulle.GetComponent<Infobulle>().ShowAtNextUpdate(cursorFixedPos);
            helpIcon.GetComponent<Infobulle>().ShowAtNextUpdate(cursorFixedPos);
            cursorHighlight.GetComponent<Infobulle>().ShowAtNextUpdate(cursorFixedPos);
        }

        if (isActive)
            GUI.DrawTexture(new Rect(0, 0, width, height), text);
    }

    private void initTexture()
    {
        width = RenderingCamera.pixelWidth;
        height = RenderingCamera.pixelHeight;

        text = new Texture2D(1, 1);
        Color transparentColor = new Color(0, 0, 0, 0);
        text.SetPixel(0, 0, transparentColor);
        text.Apply();
        text.Resize(width, height);

        circleSizeMax = width > height ? width / 10 : height / 10;
    }


    // from https://stackoverflow.com/questions/30410317/how-to-draw-circle-on-texture-in-unity
    public static Texture2D DrawCircle(Texture2D tex, Color color, int x, int y, int radius = 3)
    {
        float rSquared = radius * radius;

        for (int u = x - radius; u < x + radius + 1; u++)
            for (int v = y - radius; v < y + radius + 1; v++)
                if ((x - u) * (x - u) + (y - v) * (y - v) < rSquared
                    && (x - u) * (x - u) + (y - v) * (y - v) > rSquared - (rSquared * 0.1))
                    tex.SetPixel(u, v, color);

        tex.Apply();

        return tex;
    }

    
    void ChangeToCustomCursor()
    {
        if (cursorChanged)
            return;

        Debug.Log("change to custom cursor");
        cursorFixedPos = Input.mousePosition;

        Cursor.SetCursor(cursorTexture, new Vector3(700/2,700/2), CursorMode.Auto);
        displayTextOnCursor = true;
        cursorChanged = true;
    }

    void ChangeToNormalCursor()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        displayTextOnCursor = false;
        cursorChanged = false;
    }

    void CursorChangeManagement()
    {
        RaycastHit hit;
        Ray ray = mainCamera.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f));

        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        //if (Physics.SphereCast(ray, /*circleSize*/ 0.03f, out hit, Mathf.Infinity))
        {
            //Debug.Log("cursor change function - point at " + hit.collider.gameObject.name);
            //Debug.Log("HasCameraInIt(hit.collider.gameObject) " + HasCameraInIt(hit.collider.gameObject));
            //Debug.Log("SelectEntity(hit.collider.gameObject) " + SelectEntity(hit.collider.gameObject));
            if (HasCameraInIt(hit.collider.gameObject)
                || (KinectGO != null && SelectEntity(hit.collider.gameObject) == KinectGO)
                || (HololensGO != null && SelectEntity(hit.collider.gameObject) == HololensGO)
                || (VirtualGO != null && SelectEntity(hit.collider.gameObject) == VirtualGO))
            {

                cursorFixedPos = mainCamera.WorldToScreenPoint(hit.collider.gameObject.transform.position);

                ChangeToCustomCursor();
                //HightlightGameObject(hit.collider.gameObject);

                if (Input.GetMouseButtonDown(1))
                {
                    selected = SelectEntity(hit.collider.gameObject);

                    if (selected != null)
                        BehaviourOnSelectedObject();
                }
            }
            else
            {
                if(cursorChanged && ((cursorFixedPos - Input.mousePosition).magnitude > mouseOffset))
                {
                    ChangeToNormalCursor();
                    //UnHighlightGameObject();
                }

            }
        }
        else
        {
            if(cursorChanged && ((cursorFixedPos - Input.mousePosition).magnitude > mouseOffset))
            {
                ChangeToNormalCursor();
                //UnHighlightGameObject();
            }
        }
    }

    void HightlightGameObject(GameObject go)
    {
        Debug.Log("selected go : " + go.name);

        if (highlightedObject != null)
            return;

        highlightedObject = go;
        Debug.Log("highlightedObject go : " + highlightedObject.name);

        GameObject circleBlinkerInstance = Instantiate(circleBlinker, highlightedObject.transform, false);
        circleBlinkerInstance.name = "blinker";
        //circleBlinkerInstance.transform.localPosition = Vector3.zero;
        //circleBlinkerInstance.transform.localRotation = Quaternion.identity;

        //ParticleSystem part = circleBlinkerInstance.GetComponent<ParticleSystem>();
        //ParticleSystem.SizeOverLifetimeModule sizeOverTime = part.sizeOverLifetime;
        //sizeOverTime.sizeMultiplier = go.GetComponent<Collider>().bounds.size.magnitude;
    }

    void UnHighlightGameObject()
    {
        Transform t = highlightedObject.transform.Find("blinker");
        if (t != null)
            Destroy(t.gameObject);
        highlightedObject = null;
    }

    bool HasCameraInIt(GameObject go)
    {
        GameObject entity = SelectEntity(go);
        if (entity != null)
            return SelectEntity(go).GetComponentInChildren<Camera>() != null;
        else 
            return false;
    }

    void Update()
    {

        CursorChangeManagement();

        //if (isActive)
        //{
        //    CursorChangeManagement();


        //    if (Input.GetButton("Fire1"))
        //    {
        //        Debug.Log("select object");
        //        SelectObject();
        //    }
        //}
        //else
        //{
        //    ChangeToNormalCursor();
        //}



        //if (isActive)
        //{
        //    initTexture();

        //    if (Input.GetButton("Fire1"))
        //    {
        //        //Debug.Log("is drawing at " + (int)Input.mousePosition.x + " / " + width + ", " + (int)Input.mousePosition.y + " / " + height);
        //        if (circleSize < circleSizeMax)
        //            circleSize += circleSizeIncrement;

        //        text = DrawCircle(text, circleColor /*Color.blue*/, (int)Input.mousePosition.x, (int)Input.mousePosition.y, circleSize);
        //        lastInputPosition = Input.mousePosition;
        //        isDrawing = true;
        //    }
        //    else
        //    {
        //        if (isDrawing)
        //        {
        //            Debug.Log("select object");
        //            SelectObject();
        //            isDrawing = false;

        //            // stop pointing mode
        //            isActive = false;
        //            selectionMode.isOn = false;
        //        }

        //        circleSize = 0;
        //    }
        //}
    }

    private void SelectObject()
    {
        RaycastHit hit;
        Ray ray = mainCamera.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f));

        if (Physics.SphereCast(ray, /*circleSize*/ 0.03f, out hit, Mathf.Infinity))
        {
            Debug.Log("hit " + hit.collider.gameObject.name);
            if (hit.collider.gameObject == selected)
            {
                UnselectObject();
            }
            else
            {
                selected = SelectEntity(hit.collider.gameObject);

                if (selected != null)
                {
                    BehaviourOnSelectedObject();
                }
            }
        }
        //else
        //{
        //    Debug.Log("no hit");
        //}
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

    public void UnselectObject()
    {
        selected = null;
    }

    public void BehaviourOnSelectedObject()
    {
        if (selected == HololensGO)
        {
            Debug.Log("go change to hololens view");
            viewChanger.DisplayARHeadsetView();
            UnselectObject();
        }
        else if (selected == KinectGO)
        {
            Debug.Log("go change to kinect view");
            viewChanger.DisplayExternalCameraView();
            UnselectObject();
        }
        else if (selected == VirtualGO)
        {
            Debug.Log("go change to virtual view");
            viewChanger.DisplayVirtualView();
            UnselectObject();
        }
        else
        {
            Camera cam = selected.GetComponentInChildren<Camera>();
            if (cam != null)
            {
                viewChanger.DisplayCameraView(cam);
            }
        }
    }

    public void OnGrasp()
    {
        //if (selected != null)
        //{
        //    // scale management
        //    Vector3 scale = new Vector3(
        //        selected.transform.localScale.x * selected.transform.parent.localScale.x,
        //        selected.transform.localScale.y * selected.transform.parent.localScale.y,
        //        selected.transform.localScale.z * selected.transform.parent.localScale.z);

        //    //selected.SetActive(false);
        //    network.SendMessage("ClientRemoteSelection", "instantiate", selected.name, selected.transform.position, selected.transform.rotation, scale);
        //}
        //else
        //    Debug.Log("no selected object to grasp !");
    }

    public void OnRelease()
    {
        selected.GetComponent<Renderer>().material = initialMaterial;
        selected = null;
    }



    //public List<GameObject> GetSelectedGameObjects()
    //{
    //    return selected;
    //}

    public void ToggleActive(bool val = false)
    {
        // val is needed for UI interaction with toogle component...

        isActive = selectionMode.isOn;
        MouseNavigator.isActive = !isActive;
        //Debug.Log("is active : " + isActive);
    }
}
#endif