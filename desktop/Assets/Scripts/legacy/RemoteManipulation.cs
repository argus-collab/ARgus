using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoteManipulation : MonoBehaviour
{
    public RemoteSelection selection;
    public Camera renderingCamera;
    public GameObject sticker;
    public float mouseSensitivity = 1.0f;
    public float lineWidth = 0.003f;
    public float lineTolerance = 0.01f;

    private bool move = false;
    private bool isManipulating = false;
    private bool isRotating = false;
    private bool isSketching = false;

    private GameObject manipulated;
    private GameObject selected;

    // moving go
    private Vector3 endingPos;
    private Vector3 startingPos;

    // sketching
    private GameObject sketchGO;
    private GameObject stickersGO;
    private List<LineRenderer> sketch;
    private List<Vector3> linePts;

    private float startTime;
    public float journeyTime = 1.0f;

    private bool doSelectAtUpdate = false;
    private bool doReleaseAtUpdate = false;


    private void Start()
    {
        sketch = new List<LineRenderer>();
    }

    void Update()
    {
        if (doSelectAtUpdate)
        {
            doSelectAtUpdate = false;
            Select();
        }

        if (doReleaseAtUpdate)
        {
            doReleaseAtUpdate = false;
            Release();
        }

        if (move)
        {
            Vector3 center = (selected.transform.position + startingPos) * 0.5F;

            // move the center a bit downwards to make the arc vertical
            center -= new Vector3(0, 1, 0);

            // Interpolate over the arc relative to center
            Vector3 riseRelCenter = startingPos - center;
            Vector3 setRelCenter = endingPos - center;

            float fracComplete = (Time.time - startTime) / journeyTime;

            manipulated.transform.position = Vector3.Slerp(riseRelCenter, setRelCenter, fracComplete);
            manipulated.transform.position += center;

            if (Mathf.Abs((manipulated.transform.position - endingPos).magnitude) < 0.01)
            {
                move = false;
                selected.SetActive(false);

                if (!isManipulating)
                {
                    // sketch management
                    if (sketchGO != null)
                    {
                        Transform sketchGOTransform = selected.transform.Find("sketch");

                        GameObject sketchGOSelected;
                        if (sketchGOTransform == null)
                        {
                            sketchGOSelected = new GameObject("sketch");
                            sketchGOSelected.transform.parent = selected.transform;
                        }
                        else
                        {
                            Destroy(sketchGOSelected = sketchGOTransform.gameObject);

                            sketchGOSelected = new GameObject("sketch");
                            sketchGOSelected.transform.parent = selected.transform;
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
                        Transform stickersGOTransform = selected.transform.Find("stickers");

                        GameObject stickersGOSelected;
                        if (stickersGOTransform == null)
                        {
                            stickersGOSelected = new GameObject("stickers");
                            stickersGOSelected.transform.parent = selected.transform;
                        }
                        else
                        {
                            Destroy(stickersGOSelected = stickersGOTransform.gameObject);

                            stickersGOSelected = new GameObject("stickers");
                            stickersGOSelected.transform.parent = selected.transform;
                        }

                        for (int i = 0; i < stickersGO.transform.childCount; ++i)
                        {
                            GameObject part = Instantiate(stickersGO.transform.GetChild(i).gameObject);
                            part.name = "sticker";
                            part.transform.parent = stickersGOSelected.transform;
                        }
                    }

                    Destroy(manipulated);
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

            Vector3 objectRelativeUp = manipulated.transform.InverseTransformDirection(relativeUp);
            Vector3 objectRelaviveRight = manipulated.transform.InverseTransformDirection(relativeRight);

            Quaternion rotateBy = Quaternion.AngleAxis(-Input.GetAxis("Mouse X") / manipulated.transform.localScale.x * mouseSensitivity, objectRelativeUp)
              * Quaternion.AngleAxis(Input.GetAxis("Mouse Y") / manipulated.transform.localScale.x * mouseSensitivity, objectRelaviveRight);

            //Debug.Log("rotate by : " + rotateBy);

            manipulated.transform.rotation = rotateBy * manipulated.transform.rotation;

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
                GameObject curSketch = new GameObject("sketch-part");
                curSketch.transform.parent = sketchGO.transform;
                LineRenderer newLine = curSketch.AddComponent<LineRenderer>();
                newLine.startWidth = lineWidth;
                newLine.endWidth = lineWidth;
                newLine.material = new Material(Shader.Find("Diffuse"));
                newLine.useWorldSpace = false;
                sketch.Add(newLine);

                linePts = new List<Vector3>();

                isSketching = true;
            }

            float z = Mathf.Abs((manipulated.transform.position - renderingCamera.transform.position).magnitude);
            Matrix4x4 m = renderingCamera.cameraToWorldMatrix;
            //Vector3 p = m.MultiplyPoint(
            //    new Vector3(
            //        Input.mousePosition.x, 
            //        Input.mousePosition.y, 
            //        -Mathf.Abs((manipulated.transform.position - endingPos).magnitude)));

            Vector3 p = renderingCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, z));

            //Debug.Log("renderingCamera.nearClipPlane : " + renderingCamera.nearClipPlane);
            //Debug.Log("z : " + z);

            linePts.Add(p);

        }
        else
        {
            if (isSketching)
            {
                sketch[sketch.Count - 1].positionCount = linePts.Count;
                sketch[sketch.Count - 1].SetPositions(linePts.ToArray());
                //sketch[sketch.Count - 1].Simplify(lineTolerance);
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
                //Debug.Log("hop, stickers at !");
                GameObject stickerGO = Instantiate(sticker, hit.point, Quaternion.identity);
                stickerGO.name = "sticker";
                stickerGO.transform.parent = stickersGO.transform;
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

    void BringToUser(GameObject go)
    {
        startTime = Time.time;

        startingPos = go.transform.position;

        float distToCenter = GetMax(go.GetComponent<Collider>().bounds.max) * 1.2f / Mathf.Tan(Mathf.Deg2Rad * (renderingCamera.fieldOfView / 2));
        endingPos = renderingCamera.transform.position + renderingCamera.transform.forward * (distToCenter + GetMax(go.GetComponent<Collider>().bounds.max) * 1.2f);
    }

    void ReleaseFromUser()
    {
        startTime = Time.time;

        Vector3 pos = endingPos;
        endingPos = startingPos;
        startingPos = pos;
    }

    public void Select()
    {
        if (!isManipulating)
        {
            List<GameObject> selectedList = selection.GetSelectedGameObjects();

            if (selectedList.Count < 1)
                return;

            // TODO : generalize to multiple selection = only take the first selected for the moment
            // we work with a copy
            selected = selectedList[0];
            manipulated = Instantiate(selectedList[0]);

            // sketch management
            Transform sketchTransform = manipulated.transform.Find("sketch");
            if (sketchTransform == null)
            {
                sketchGO = new GameObject("sketch");
                sketchGO.transform.parent = manipulated.transform;
            }
            else
            {
                sketchGO = sketchTransform.gameObject;
            }

            // stickers management
            Transform stickersTransform = manipulated.transform.Find("stickers");
            if (stickersTransform == null)
            {
                stickersGO = new GameObject("stickers");
                stickersGO.transform.parent = manipulated.transform;
            }
            else
            {
                stickersGO = stickersTransform.gameObject;
            }

            BringToUser(manipulated);

            move = true;
            isManipulating = true;
        }
    }

    public void Release()
    {
        if (isManipulating)
        {
            selected.SetActive(true);

            ReleaseFromUser();

            move = true;
            isManipulating = false;
        }

    }

    public void AskForSelect()
    {
        doSelectAtUpdate = true;
    }

    public void AskForRelease()
    {
        doReleaseAtUpdate = true;
    }
}
