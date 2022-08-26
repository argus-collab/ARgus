//
//Filename: maxCamera.cs
//
// original: http://www.unifycommunity.com/wiki/index.php?title=MouseOrbitZoom
//
// --01-18-2010 - create temporary target, if none supplied at start

using UnityEngine;
using UnityEngine.UI;
using System.Collections;


[AddComponentMenu("Camera-Control/3dsMax Camera Style")]
public class maxCamera : MonoBehaviour
{
    public float translationMaxValueX = 2;
    public float translationMaxValueY = 2;

    public Slider DebugPanSpeedSlider;
    public Slider DebugXYSpeedSlider;
    public Slider DebugZoomSpeedSlider;

    public Camera virtualCamera;
    public Camera mainCamera;

    public Transform target;
    public Vector3 targetOffset;
    public float distance = 5.0f;
    public float maxDistance = 20;
    public float minDistance = .6f;
    public float xSpeed = 200.0f;
    public float ySpeed = 200.0f;
    public int yMinLimit = -80;
    public int yMaxLimit = 80;
    public int zoomRate = 40;
    public float panSpeed = 0.3f;
    public float zoomDampening = 5.0f;

    public bool isActive = true;

    private float xDeg = 0.0f;
    private float yDeg = 0.0f;
    private float currentDistance;
    private float desiredDistance;
    private Quaternion currentRotation;
    private Quaternion desiredRotation;
    private Quaternion rotation;
    private Vector3 position;

    private float initialDistance;

    void Start() { Init(); }
    void OnEnable() { Init(); }

    private Vector3 lastPosition;
    private Quaternion lastRotation;

    private bool isMoving = false;
    public bool isLocked = true;

    public Texture2D openHandCursor;
    public Texture2D closedHandCursor;
    private bool cursorOpenHand = false;
    private bool cursorClosedHand = false;
    private bool cursorDefault = true;

    public float translationTriggerOffset = 0.0002f;
    //public float rotationTriggerOffset = 2f;

    public CameraPositionUIIndicator cpuii;
    public UserDataInput user;



    void ResetCursor()
    {
        if (!cursorDefault)
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            cursorDefault = true;
            cursorOpenHand = false;
            cursorClosedHand = false;
        }
    }

    void SetOpenHandCursor()
    {
        if (!cursorOpenHand)
        {
            Cursor.SetCursor(openHandCursor, new Vector2(256, 256), CursorMode.Auto);
            cursorDefault = false;
            cursorOpenHand = true;
            cursorClosedHand = false;
        }
    }

    void SetClosedHandCursor()
    {
        if (!cursorClosedHand)
        {
            Cursor.SetCursor(closedHandCursor, new Vector2(256, 256), CursorMode.Auto);
            cursorDefault = false;
            cursorOpenHand = false;
            cursorClosedHand = true;
        }
    }

    public void UpdateYXSpeed(System.Single val)
    {
        xSpeed = val;
        ySpeed = val;
    }

    public void UpdatePanSpeed(System.Single val)
    {
        panSpeed = val;
    }

    public void UpdateZoomSpeed(System.Single val)
    {
        zoomRate = (int) val;
    }

    public void UpdateTranslationTrigger(System.Single val)
    {
        translationTriggerOffset = val;
    }

    public float GetYTarget(Vector3 camPos, Vector3 direction)
    {
        float y = 0;
        
        if (camPos.y != 0)
        {
            float d = -(camPos.x * direction.x + camPos.y * direction.y + camPos.z * direction.z);
            y = -d / direction.y;
        }

        return y; 
    }

    float GetDistanceToPlaneXZ(Vector3 direction, Vector3 p)
    {
        Vector3 proj = Vector3.zero;
        float t = -p.y / direction.y;

        proj.x = direction.x * t + p.x;
        proj.z = direction.z * t + p.z;

        return (proj - p).magnitude;
    }

    public void Init()
    {
        initialDistance = distance;

        //If there is no target, create a temporary target at 'distance' from the cameras current viewpoint
        if (!target)
        {
            GameObject go = new GameObject("Cam Target");
            go.transform.position = virtualCamera.transform.position + (virtualCamera.transform.forward * distance);
            target = go.transform;
        }
        else
        {
            //target.position = virtualCamera.transform.position + (virtualCamera.transform.forward * distance);
            //target.position = new Vector3(0, GetYTarget(virtualCamera.transform.position, virtualCamera.transform.forward), 0);

            if(isLocked)
            {
                initialDistance = GetDistanceToPlaneXZ(virtualCamera.transform.forward, virtualCamera.transform.position);

                Vector3 p = virtualCamera.transform.position + (virtualCamera.transform.forward * initialDistance);
                target.position = new Vector3(p.x, 0, p.z);

                ClampTarget();

            }
            else
            {
                distance = GetDistanceToPlaneXZ(virtualCamera.transform.forward, virtualCamera.transform.position);
                target.position = virtualCamera.transform.position + (virtualCamera.transform.forward * distance);
            }

        }


        distance = Vector3.Distance(virtualCamera.transform.position, target.position);
        currentDistance = distance;
        desiredDistance = distance;

        //be sure to grab the current rotations as starting points.
        position = virtualCamera.transform.position;
        rotation = virtualCamera.transform.rotation;
        currentRotation = virtualCamera.transform.rotation;
        desiredRotation = virtualCamera.transform.rotation;

        xDeg = Vector3.SignedAngle(Vector3.right, virtualCamera.transform.right, virtualCamera.transform.up);
        yDeg = Vector3.SignedAngle(Vector3.up, virtualCamera.transform.up, virtualCamera.transform.right);
    }

    /*
     * Camera logic on LateUpdate to only update after all character movement logic has been handled. 
     */
    void LateUpdate()
    {


        if (!isActive)
        {
            //isLocked = false;

            Init();

            //target.position = virtualCamera.transform.position + (virtualCamera.transform.forward * initialDistance);
            //target.position = new Vector3(0, GetYTarget(virtualCamera.transform.position, virtualCamera.transform.forward), 0);

            if (isLocked)
            {
                initialDistance = GetDistanceToPlaneXZ(virtualCamera.transform.forward, virtualCamera.transform.position);

                Vector3 p = virtualCamera.transform.position + (virtualCamera.transform.forward * initialDistance);
                target.position = new Vector3(p.x, 0, p.z);

                ClampTarget();

            }
            else
            {
                initialDistance = GetDistanceToPlaneXZ(virtualCamera.transform.forward, virtualCamera.transform.position);
                target.position = virtualCamera.transform.position + (virtualCamera.transform.forward * initialDistance);
            }



            lastPosition = virtualCamera.transform.position;
            lastRotation = virtualCamera.transform.rotation;

            return;
        }



        // If Control and Alt and Middle button? ZOOM!
        if (Input.GetMouseButton(0) && Input.GetKey(KeyCode.LeftAlt) && Input.GetKey(KeyCode.LeftControl))
        {
            desiredDistance -= Input.GetAxis("Mouse Y") * Time.deltaTime * zoomRate * 0.125f * Mathf.Abs(desiredDistance);
        }
        else if (Input.GetMouseButton(0) && Input.GetKey(KeyCode.LeftAlt))
        {
            SetClosedHandCursor();

            //grab the rotation of the camera so we can move in a psuedo local XY space
            target.rotation = virtualCamera.transform.rotation;
            target.Translate(Vector3.right * -Input.GetAxis("Mouse X") * panSpeed);
            target.Translate(virtualCamera.transform.up * -Input.GetAxis("Mouse Y") * panSpeed, Space.World);

            //if (isLocked)
                ClampTarget();

            if (user.HasDoneTuto())
                cpuii.gameObject.SetActive(true);
        }
        else if (Input.GetMouseButton(0))
        {
            xDeg += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
            yDeg -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;

            ////////OrbitAngle

            //Clamp the vertical axis for the orbit
            yDeg = ClampAngle(yDeg, yMinLimit, yMaxLimit);

            // set camera rotation 
            desiredRotation = Quaternion.Euler(yDeg, xDeg, 0);
            currentRotation = virtualCamera.transform.rotation;

            rotation = Quaternion.Lerp(currentRotation, desiredRotation, Time.deltaTime * zoomDampening);
            virtualCamera.transform.rotation = rotation;

            if (user.HasDoneTuto())
                cpuii.gameObject.SetActive(true);
        }
        else if (Input.GetKey(KeyCode.LeftAlt))
        {
            SetOpenHandCursor();

            if(user.HasDoneTuto())
                cpuii.gameObject.SetActive(true);
        }
        else
        {
            ResetCursor();

            if (user.HasDoneTuto())
                cpuii.gameObject.SetActive(false);
        }
        // otherwise if middle mouse is selected, we pan by way of transforming the target in screenspace


        ////////Orbit Position
        //Debug.Log("--------- " + Input.GetAxis("Mouse ScrollWheel"));
        // affect the desired Zoom distance if we roll the scrollwheel
        desiredDistance -= Mathf.Clamp(Input.GetAxis("Mouse ScrollWheel"), -0.3f, 0.3f) * Time.deltaTime * zoomRate * Mathf.Abs(desiredDistance);
        //clamp the zoom min/max
        desiredDistance = Mathf.Clamp(desiredDistance, minDistance, maxDistance);
        // For smoothing of the zoom, lerp distance
        currentDistance = Mathf.Lerp(currentDistance, desiredDistance, Time.deltaTime * zoomDampening);
        //currentDistance = Mathf.SmoothStep(currentDistance, desiredDistance, Time.deltaTime * zoomDampening);

        // calculate position based on the new currentDistance 
        position = target.position - (rotation * Vector3.forward * currentDistance + targetOffset);
        virtualCamera.transform.position = position;

        isMoving = ((lastPosition - virtualCamera.transform.position).magnitude > translationTriggerOffset*Time.deltaTime);/* 
            || (Quaternion.Inverse(lastRotation) * virtualCamera.transform.rotation).eulerAngles.x > rotationTriggerOffset 
            || (Quaternion.Inverse(lastRotation) * virtualCamera.transform.rotation).eulerAngles.y > rotationTriggerOffset
            || (Quaternion.Inverse(lastRotation) * virtualCamera.transform.rotation).eulerAngles.z > rotationTriggerOffset;*/





        //Debug.Log("isMoving : " + isMoving + " - trans = " + (lastPosition - virtualCamera.transform.position).magnitude + " - ang = " + (Quaternion.Inverse(lastRotation) * virtualCamera.transform.rotation).eulerAngles);

        lastPosition = virtualCamera.transform.position;
        lastRotation = virtualCamera.transform.rotation;

        //isLocked = true;
    }

    public bool IsMoving()
    {
        return isMoving;
    }

    public void Teleport(Vector3 position, Quaternion rotation, float offset = 0)
    {
        virtualCamera.transform.position = position + new Vector3(offset, offset, offset);
        virtualCamera.transform.rotation = rotation;
        Init();
    }

    private static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360)
            angle += 360;
        if (angle > 360)
            angle -= 360;
        return Mathf.Clamp(angle, min, max);
    }

    private void ClampTarget()
    {
        Vector3 finalTargetPos = target.position;
        finalTargetPos.y = 0;

        if (finalTargetPos.x > translationMaxValueX)
            finalTargetPos.x = translationMaxValueX;
        if (finalTargetPos.x < -translationMaxValueX)
            finalTargetPos.x = -translationMaxValueX;

        if (finalTargetPos.z > translationMaxValueY)
            finalTargetPos.z = translationMaxValueY;
        if (finalTargetPos.z < -translationMaxValueY)
            finalTargetPos.z = -translationMaxValueY;

        target.position = finalTargetPos;
    }

}