using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClientRemoteSelectionUI : MonoBehaviour
{
    //public ClientRemoteSelection selector;

    public int circleSizeIncrement = 10;
    public Camera RenderingCamera;
    public Toggle selectionMode;
    public maxCamera MouseNavigator;
    public Camera mainCamera;
    public Material selectedMaterial;
    public Color circleColor;

    [Header("debug")]
    public List<GameObject> selected;
    public List<GameObject> meshSelected;

    private bool isActive = false;

    private int circleSize = 0;
    private int circleSizeMax = 200;

    private bool isDrawing = false;

    private Texture2D text;

    private int width;
    private int height;

    private Vector3 lastInputPosition;

    //private Material initialMaterial;


    //private List<GameObject> selected; 
    private List<Material> initialMaterial; 

    private void initTexture()
    {
        width = RenderingCamera.pixelWidth;
        height = RenderingCamera.pixelHeight;

        text = new Texture2D(1, 1);
        Color transparentColor = new Color(0, 0, 0, 0);
        text.SetPixel(0, 0, transparentColor);
        text.Apply();
        text.Resize(width, height);

        circleSizeMax = width > height ? width/10 : height/10;
    }

    private void Start()
    {
        selected = new List<GameObject>();
        meshSelected = new List<GameObject>();
        initialMaterial = new List<Material>();

        initTexture();
    }

    // from https://stackoverflow.com/questions/30410317/how-to-draw-circle-on-texture-in-unity
    public static Texture2D DrawCircle(Texture2D tex, Color color, int x, int y, int radius = 3)
    {
        float rSquared = radius * radius;

        for (int u = x - radius; u < x + radius + 1; u++)
            for (int v = y - radius; v < y + radius + 1; v++)
                if ((x - u) * (x - u) + (y - v) * (y - v) < rSquared
                    && (x - u) * (x - u) + (y - v) * (y - v) > rSquared-(rSquared * 0.1))
                    tex.SetPixel(u, v, color);

        tex.Apply();

        return tex;
    }

    void Update()
    {
        if (isActive)
        {
            initTexture();

            if (Input.GetButton("Fire1"))
            {
                //Debug.Log("is drawing at " + (int)Input.mousePosition.x + " / " + width + ", " + (int)Input.mousePosition.y + " / " + height);
                if (circleSize < circleSizeMax)
                    circleSize += circleSizeIncrement;

                Debug.Log("circle color : " + circleColor);

                text = DrawCircle(text, circleColor, (int)Input.mousePosition.x, (int)Input.mousePosition.y, circleSize);
                lastInputPosition = Input.mousePosition;
                isDrawing = true;
            }
            else
            {
                if (isDrawing)
                {
                    Debug.Log("select object");
                    SelectObject();
                    isDrawing = false;

                    // stop pointing mode
                    isActive = false;
                    selectionMode.isOn = false;
                }

                circleSize = 0;
            }
        }
    }

    private void SelectObject()
    {
        //Debug.Log("send selected position to server");
        //selector.Select(Input.mousePosition.x, Input.mousePosition.y);

        RaycastHit hit;
        Ray ray = mainCamera.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f));

        if (Physics.SphereCast(ray, /*circleSize*/ 0.03f, out hit, Mathf.Infinity))
        {
            if (selected.Contains(hit.collider.gameObject))
            {
                UnselectObject(selected.IndexOf(hit.collider.gameObject));
            }
            else
            {
                selected.Add(SelectEntity(hit.collider.gameObject));

                if (selected != null)
                {
                    meshSelected.Add(hit.collider.gameObject);
                    initialMaterial.Add(hit.collider.gameObject.GetComponent<Renderer>().material);
                    hit.collider.gameObject.GetComponent<Renderer>().material = selectedMaterial;
                }
            }
        }
        else
        {
            Debug.Log("no hit");
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

    public void UnselectObject(int i)
    {
        meshSelected[i].GetComponent<Renderer>().material = initialMaterial[i];
        meshSelected.RemoveAt(i);
        initialMaterial.RemoveAt(i);
        selected.RemoveAt(i);
    }

    public void OnGrasp()
    {
        //Debug.Log("send grasp order to server");
        //selector.Grasp();
    }

    public void OnRelease()
    {
        //Debug.Log("send release order to server");
        //selector.Release();
    }

    private void OnGUI()
    {
        if(isActive)
            GUI.DrawTexture(new Rect(0,0,width,height), text);
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
