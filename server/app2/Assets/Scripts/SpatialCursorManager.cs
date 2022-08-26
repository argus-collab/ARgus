using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpatialCursorManager : MonoBehaviour
{
    public Camera mainCamera;
    public GameObject pointer;
    public maxCamera navigator1;
    public FPSCamera navigator2;

    public bool isActive = true;
    public float scale = 0.01f;
    public float smoothTime = 0.3F;

    private Vector3 velocity = Vector3.zero;
    public Vector3 targetPosition = Vector3.zero;

    public float horizontalSpeed = 2.0f;
    public float verticalSpeed = 2.0f;

    float x = 0;
    float y = 0;
    float z = 0;

    private void Start()
    {
        targetPosition = pointer.transform.position;
    }

    void Update()
    {
        //if (isActive)
        //    navigator.enabled = false;
        //else
        //    navigator.enabled = true;

        // move cursor accroding to target position
        if ((pointer.transform.position - targetPosition).magnitude > 0.001)
        {
            pointer.transform.position = Vector3.SmoothDamp(pointer.transform.position, targetPosition, ref velocity, smoothTime);
        }

        // control from user
        if (Input.GetKey(KeyCode.X))
        {
            Vector3 direction = mainCamera.transform.forward;

            Cursor.lockState = CursorLockMode.Locked;
            navigator1.isActive = false;
            navigator2.enabled = false;

            x = horizontalSpeed * Input.GetAxis("Mouse X");
            y = Input.mouseScrollDelta.y;
            z = verticalSpeed * Input.GetAxis("Mouse Y");


            //if (Input.GetKey(KeyCode.A))
            //    direction = mainCamera.transform.forward;
            //if (Input.GetKey(KeyCode.S))
            //    direction = -mainCamera.transform.forward;
            //if (Input.GetKey(KeyCode.D))
            //    direction = mainCamera.transform.right;
            //if (Input.GetKey(KeyCode.Q))
            //    direction = -mainCamera.transform.right;

            targetPosition = pointer.transform.position + x * mainCamera.transform.right + y * transform.up + z * Vector3.Cross(mainCamera.transform.right, transform.up);
        }
        else
        {
            x = 0;
            y = 0;
            z = 0;
            Cursor.lockState = CursorLockMode.None;

            navigator1.isActive = true;
            navigator2.enabled = true;

        }
    }
}
