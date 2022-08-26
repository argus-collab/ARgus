using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientRemoteManipulation : MonoBehaviour
{
    public WebRTCNetworkCommunication network;
    public ClientRemoteSelection selector;

    public Camera renderingCamera;
    public GameObject sticker;

    private GameObject grasped;

    public float mouseSensitivity = 1.0f;
    public float lineWidth = 0.003f;
    public float lineTolerance = 0.01f;

    private bool move = false;
    private bool isManipulating = false;
    private bool isRotating = false;
    private bool isSketching = false;

    // moving go
    private Vector3 endingPos;
    private Vector3 startingPos;

    // sketching
    private GameObject sketchGO;
    private GameObject stickersGO;
    private GameObject curSketch;
    private LineRenderer curLine;
    private List<LineRenderer> sketch;
    private List<Vector3> linePts;

    private float startTime;
    public float journeyTime = 1.0f;
    /*
    private void Start()
    {
        sketch = new List<LineRenderer>();
    }

    void Update()
    {
        if (grasped == null && selector.GetGrasped() != null)
        {
            grasped = selector.GetGrasped();

            // sketch management
            Transform sketchTransform = grasped.transform.Find("sketch");
            if (sketchTransform == null)
            {
                sketchGO = new GameObject("sketch");
                sketchGO.transform.parent = grasped.transform;
                sketchGO.transform.localPosition = Vector3.zero;
                sketchGO.transform.localRotation = Quaternion.identity;
                sketchGO.transform.localScale = new Vector3(1,1,1);

            }
            else
            {
                sketchGO = sketchTransform.gameObject;
            }

            // stickers management
            Transform stickersTransform = grasped.transform.Find("stickers");
            if (stickersTransform == null)
            {
                stickersGO = new GameObject("stickers");
                stickersGO.transform.parent = grasped.transform;
                stickersGO.transform.localPosition = Vector3.zero;
                stickersGO.transform.localRotation = Quaternion.identity;
                stickersGO.transform.localScale = new Vector3(1, 1, 1);

            }
            else
            {
                stickersGO = stickersTransform.gameObject;
            }

            move = true;
            isManipulating = true;
            BringToUser(grasped);
        }

        if (grasped != null && selector.GetGrasped() == null)
        {
            grasped = null;
        }

        if (move)
        {
            Vector3 center = (grasped.transform.position + startingPos) * 0.5F;

            // move the center a bit downwards to make the arc vertical
            center -= new Vector3(0, 1, 0);

            // Interpolate over the arc relative to center
            Vector3 riseRelCenter = startingPos - center;
            Vector3 setRelCenter = endingPos - center;

            float fracComplete = (Time.time - startTime) / journeyTime;

            grasped.transform.position = Vector3.Slerp(riseRelCenter, setRelCenter, fracComplete);
            grasped.transform.position += center;

            if (Mathf.Abs((grasped.transform.position - endingPos).magnitude) < 0.01)
            {
                move = false;

                if (!isManipulating)
                {
                    // sketch management
                    if (sketchGO != null)
                    {
                        Transform sketchGOTransform = grasped.transform.Find("sketch");

                        GameObject sketchGOSelected;
                        if (sketchGOTransform == null)
                        {
                            sketchGOSelected = new GameObject("sketch");
                            sketchGOSelected.transform.parent = grasped.transform;
                            sketchGOSelected.transform.localScale = new Vector3(1, 1, 1);

                        }
                        else
                        {
                            Destroy(sketchGOTransform.gameObject);

                            sketchGOSelected = new GameObject("sketch");
                            sketchGOSelected.transform.parent = grasped.transform;
                            sketchGOSelected.transform.localScale = new Vector3(1, 1, 1);
                        }

                        for (int i = 0; i < sketchGO.transform.childCount; ++i)
                        {
                            GameObject part = Instantiate(sketchGO.transform.GetChild(i).gameObject);
                            part.name = "sketch-part";
                            part.transform.parent = sketchGOSelected.transform;
                        }
                    }

                    // stickers management
                    if (stickersGO != null)
                    {
                        Transform stickersGOTransform = grasped.transform.Find("stickers");

                        GameObject stickersGOSelected;
                        if (stickersGOTransform == null)
                        {
                            stickersGOSelected = new GameObject("stickers");
                            stickersGOSelected.transform.parent = grasped.transform;
                            stickersGOSelected.transform.localPosition = Vector3.zero;
                            stickersGOSelected.transform.localRotation = Quaternion.identity;
                            stickersGOSelected.transform.localScale = new Vector3(1, 1, 1);
                        }
                        else
                        {
                            Destroy(stickersGOTransform.gameObject);

                            stickersGOSelected = new GameObject("stickers");
                            stickersGOSelected.transform.parent = grasped.transform;
                            stickersGOSelected.transform.localPosition = Vector3.zero;
                            stickersGOSelected.transform.localRotation = Quaternion.identity;
                            stickersGOSelected.transform.localScale = new Vector3(1, 1, 1);
                        }

                        for (int i = 0; i < stickersGO.transform.childCount; ++i)
                        {
                            GameObject part = Instantiate(stickersGO.transform.GetChild(i).gameObject);
                            part.name = "sticker";
                            part.transform.parent = stickersGOSelected.transform;
                        }
                    }

                    //Destroy(manipulated);
                }
            }
        }

        if (isManipulating)
        {
            ObjectRotationUpdate();
            SketchingUpdate();
            StickerUpdate();
        }
    }
    void ObjectRotationUpdate()
    {
        if (Input.GetMouseButton(0))
        {
            isRotating = true;

            Vector3 relativeUp = renderingCamera.transform.TransformDirection(Vector3.up);
            Vector3 relativeRight = renderingCamera.transform.TransformDirection(Vector3.right);

            Vector3 objectRelativeUp = grasped.transform.InverseTransformDirection(relativeUp);
            Vector3 objectRelaviveRight = grasped.transform.InverseTransformDirection(relativeRight);

            Quaternion rotateBy = Quaternion.AngleAxis(-Input.GetAxis("Mouse X") / grasped.transform.localScale.x * mouseSensitivity, objectRelativeUp)
              * Quaternion.AngleAxis(Input.GetAxis("Mouse Y") / grasped.transform.localScale.x * mouseSensitivity, objectRelaviveRight);

            //Debug.Log("rotate by : " + rotateBy);

            grasped.transform.rotation = rotateBy * grasped.transform.rotation;

        }
        else
        {
            isRotating = false;
        }
    }

    void SketchingUpdate()
    {
        if (Input.GetMouseButton(1))
        {
            if (!isSketching)
            {
                curSketch = new GameObject("sketch-part");
                curSketch.transform.parent = sketchGO.transform;

                curLine = curSketch.AddComponent<LineRenderer>();
                curLine.startWidth = lineWidth;
                curLine.endWidth = lineWidth;
                curLine.material = new Material(Shader.Find("Diffuse"));
                curLine.useWorldSpace = false;

                sketch.Add(curLine);

                linePts = new List<Vector3>();

                isSketching = true;
            }

            float z = Mathf.Abs((grasped.transform.position - renderingCamera.transform.position).magnitude);
            Matrix4x4 m = renderingCamera.cameraToWorldMatrix;
            Vector3 p = renderingCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, z));

            linePts.Add(p);
        }
        else
        {
            if (isSketching)
            {
                sketch[sketch.Count - 1].positionCount = linePts.Count;
                sketch[sketch.Count - 1].SetPositions(linePts.ToArray());

                network.SendMessage("ServerRemoteManipulation", "draw", 
                    grasped.name,
                    curSketch.transform.localPosition, 
                    curSketch.transform.localRotation, 
                    curSketch.transform.localScale,
                    curLine.startWidth, 
                    curLine.endWidth, 
                    linePts.ToArray());
            }

            isSketching = false;
        }
    }

    void StickerUpdate()
    {
        if (Input.GetMouseButtonDown(2))
        {
            RaycastHit hit;
            Ray ray = renderingCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray.origin, ray.direction, out hit))
            {
                GameObject stickerGO = Instantiate(sticker, hit.point, Quaternion.identity);
                stickerGO.name = "sticker";
                stickerGO.transform.parent = stickersGO.transform;

                network.SendMessage("ServerRemoteManipulation", "stick", 
                    grasped.name,
                    stickerGO.transform.localPosition, 
                    stickerGO.transform.localRotation, 
                    stickerGO.transform.localScale);
            }
        }
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

    float GetMaxScaled(Vector3 v, Vector3 s)
    {
        if (v.x * s.x >= v.y * s.y && v.x * s.x >= v.z * s.z)
            return v.x * s.x;
        else if (v.y * s.y >= v.x * s.x && v.y * s.y >= v.z * s.z)
            return v.y * s.y;
        else
            return v.z * s.z;
    }

    void BringToUser(GameObject go)
    {
        Debug.Log("bring to user");
        startTime = Time.time;

        startingPos = go.transform.position;
        
        
        Debug.Log(" ----------------------------------------------------- ");
        Debug.Log(" bounds.size " + go.GetComponent<Collider>().bounds.size);
        Debug.Log(" bounds.extents " + go.GetComponent<Collider>().bounds.extents);
        Debug.Log(" bounds.max " + go.GetComponent<Collider>().bounds.max);
        Debug.Log(" local scale " + go.transform.localScale.x + ", " + go.transform.localScale.y + ", " + go.transform.localScale.z);
        Debug.Log(" lossy scale " + go.transform.lossyScale.x + ", " + go.transform.lossyScale.y + ", " + go.transform.lossyScale.z);
        Debug.Log(" name " + go.name);

        Vector3 scaledBounds = new Vector3(
            go.GetComponent<Renderer>().bounds.max.x * go.transform.localScale.x,
            go.GetComponent<Renderer>().bounds.max.y * go.transform.localScale.y,
            go.GetComponent<Renderer>().bounds.max.z * go.transform.localScale.z);

        Debug.Log(" scaledBounds " + scaledBounds.x + ", " + scaledBounds.y + ", " + scaledBounds.z);
        Debug.Log(" ----------------------------------------------------- ");


        //float distToCenter = (GetMax(go.GetComponent<Collider>().bounds.size) * 1.4f / 2) / Mathf.Tan(Mathf.Deg2Rad * (renderingCamera.fieldOfView / 2));
        float distToCenter = (GetMax(scaledBounds) * 1.4f / 2) / Mathf.Tan(Mathf.Deg2Rad * (renderingCamera.fieldOfView / 2));

        if(distToCenter - GetMax(scaledBounds) < renderingCamera.nearClipPlane)
            endingPos = renderingCamera.transform.position + renderingCamera.transform.forward * (renderingCamera.nearClipPlane + GetMax(scaledBounds));
        else
            endingPos = renderingCamera.transform.position + renderingCamera.transform.forward * distToCenter;


        Debug.Log("starting pos : " + startingPos);
        Debug.Log("ending pos : " + endingPos);
    }

    void ReleaseFromUser()
    {
        startTime = Time.time;

        Vector3 pos = endingPos;
        endingPos = new Vector3(0.0f, 0.0f, 100.0f);
        startingPos = pos;
    }
    */
}
