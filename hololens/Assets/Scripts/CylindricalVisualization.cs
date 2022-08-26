#if !UNITY_WSA
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CylindricalVisualization : MonoBehaviour
{
    public CustomClientNetworkManager network;
    public bool isActive = true;
    public Camera mainCamera;
    private GameObject cylinder;
    private GameObject stick;
    private GameObject cursor;
    private bool isCylinderDisplayed = false;
    public float offsetFromCylinder = 0.2f;
    public float maxMouseDisplacement = 600;

    public DisplayViewInUI miniature;

    public maxCamera navigator;

    private GameObject cursorLocal;

    private bool isManipulating = false;
    private Vector2 initialMousePosition;

    private Vector3 facingVector;
    private Vector3 lateralVector;
    private Vector3 upVector;

    public List<GameObject> selected;

    void Start()
    {
        selected = new List<GameObject>();
    }

    private void OnEnable()
    {
        isActive = true;
        //sphere.SetActive(false);
        RemoveCylinder();
        selected.Clear();
    }

    private void OnDisable()
    {
        isActive = false;
        navigator.isActive = true;
        RemoveCylinder();

        ReleaseCursor();
        isManipulating = false;
    }

    private void Update()
    {
        if (isCylinderDisplayed && cylinder == null)
        {
            ManipulationCylinder manip = GameObject.FindObjectOfType<ManipulationCylinder>();
            if (manip != null)
                cylinder = manip.gameObject;

            ManipulationStick manip2 = GameObject.FindObjectOfType<ManipulationStick>();
            if (manip2 != null)
            {
                stick = manip2.gameObject;
                Vector3 stickScale = stick.transform.localScale;
                stickScale.z = 0;
                stick.transform.localScale = stickScale;
            }

            ManipulationCursor manip3 = GameObject.FindObjectOfType<ManipulationCursor>();
            if (manip3 != null)
                cursor = manip3.gameObject;
        }

        if (isActive)
            navigator.isActive = false;
        else
            navigator.isActive = true;

        if (Input.GetButton("Fire1"))
        {

            if (!isManipulating)
            {
                isManipulating = true;
                initialMousePosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);

                if (cursorLocal == null)
                {
                    CreateCursor();
                }

                facingVector = Vector3.Normalize(mainCamera.transform.position - cylinder.transform.position);
                facingVector.y = 0;
                upVector = cylinder.transform.up;
                lateralVector = Vector3.Normalize(Vector3.Cross(facingVector, mainCamera.transform.up));
            }

            float theta = (Input.mousePosition.x - initialMousePosition.x) * (Mathf.PI / 2) / maxMouseDisplacement;
            float phi = (Input.mousePosition.y - initialMousePosition.y) * (Mathf.PI / 2) / maxMouseDisplacement;

            //Debug.Log("theta = " + 360 * theta / (2 * Mathf.PI));
            //Debug.Log("phi = " + 360 * phi / (2 * Mathf.PI));

            //Vector3 cursorPos;
            float x = (cylinder.transform.localScale.x / 2) * Mathf.Cos(theta);
            float y = (cylinder.transform.localScale.y / 2) * Mathf.Sin(theta);
            float z = (cylinder.transform.localScale.z / 2) * phi;

            cursorLocal.transform.position = cylinder.transform.position + x * facingVector + y * lateralVector + z * upVector;
            cursor.transform.position = cylinder.transform.position + x * facingVector + y * lateralVector + z * upVector;

            float xStick = (cylinder.transform.localScale.x / 4) * Mathf.Cos(theta);
            float yStick = (cylinder.transform.localScale.y / 4) * Mathf.Sin(theta);
            float zStick = (cylinder.transform.localScale.z / 4) * phi;
            stick.transform.position = cylinder.transform.position + xStick * facingVector + yStick * lateralVector + zStick * upVector;

            float distance = (cylinder.transform.position - cursor.transform.position).magnitude;
            Vector3 stickScale = stick.transform.localScale;
            stickScale.z = distance / 2;
            stick.transform.localScale = stickScale;

            stick.transform.LookAt(cylinder.transform.position);
            cursorLocal.transform.LookAt(cylinder.transform.position);
            cursor.transform.LookAt(cylinder.transform.position);

        }
        else if (Input.GetMouseButtonDown(1))
        {
            RaycastHit hit;
            Ray ray = mainCamera.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f));

            if (Physics.SphereCast(ray, /*circleSize*/ 0.03f, out hit, Mathf.Infinity))
            {
                if (selected.Contains(hit.collider.gameObject))
                {
                    selected.RemoveAt(selected.IndexOf(hit.collider.gameObject));
                }
                else
                {
                    GameObject entity = SelectEntity(hit.collider.gameObject);
                    Debug.Log("hit : " + hit.collider.gameObject.name);
                    if (entity != null)
                        selected.Add(entity);
                }
            }
            else
            {
                Debug.Log("no hit");
            }
        }
        else if (Input.GetMouseButtonDown(2))
        {
            //sphere.SetActive(true);

            if (isCylinderDisplayed)
            {
                RemoveCylinder();
            }
            else
            {
                AddCylinder();
            }
        }
        else if (Input.mouseScrollDelta.y != 0)
        {
            float scale = 0.01f;
            cylinder.transform.localScale += new Vector3(Input.mouseScrollDelta.y * scale, Input.mouseScrollDelta.y * scale, Input.mouseScrollDelta.y * scale);
        }
        else
        {
            ReleaseCursor();
            isManipulating = false;
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
        cursorLocal.transform.position = cylinder.transform.position + (cylinder.transform.localScale.x / 2) * Vector3.up;

        Camera cam = cursorLocal.AddComponent<Camera>();
        //cam.targetDisplay = 1;

        RedirectViewFromCamera pipe = cursorLocal.AddComponent<RedirectViewFromCamera>();
        pipe.inputCamera = cam;
        cam.backgroundColor = mainCamera.backgroundColor;
        cam.clearFlags = mainCamera.clearFlags;

        miniature.DesactivateDisplayOnMouseOver();
        miniature.ConnectSpecificCamera(pipe, cam, cylinder.transform.position, 200);
    }

    void ReleaseCursor()
    {
        Destroy(cursorLocal);
        miniature.ActivateDisplayOnMouseOver();
    }

    public void SetSphereAround(List<GameObject> objects)
    {
        cylinder.transform.position = GetBarycenter(objects);
        float maxBounding = GetMax(GetSizeGlobalBoundingBox(objects, cylinder.transform.position));
        cylinder.transform.localScale = new Vector3(maxBounding, maxBounding, maxBounding) + new Vector3(offsetFromCylinder, offsetFromCylinder, offsetFromCylinder);
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

        for (int i = 0; i < objects.Count; ++i)
        {
            MeshRenderer[] rends = objects[i].GetComponentsInChildren<MeshRenderer>();

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
        }

        size.x *= 2;
        size.y *= 2;
        size.z *= 2;

        return size;
    }

    void AddCylinder()
    {
        Debug.Log("add cylinder");
        isCylinderDisplayed = true;
        network.SendCommandMessage("scene-manager-command", "pop 3DMouseViewPointCylinder ok");
        network.SendCommandMessage("scene-manager-command", "pop FixedStick ok");
        network.SendCommandMessage("scene-manager-command", "pop Cursor ok");
    }

    void RemoveCylinder()
    {
        Debug.Log("remove cylinder");
        isCylinderDisplayed = false;
        network.SendCommandMessage("scene-manager-command", "remove 3DMouseViewPointCylinder ok");
        network.SendCommandMessage("scene-manager-command", "remove FixedStick ok");
        network.SendCommandMessage("scene-manager-command", "remove Cursor ok");
    }
}
#endif