using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSCamera : MonoBehaviour
{
    public Camera mainCamera;
    public float horizontalSpeed = 2.0f;
    public float verticalSpeed = 2.0f;
    public maxCamera MouseNavigator;

    float yaw = 0;
    float pitch = 0;

    public bool isActive;

    void Update()
    {
        if (!isActive)
            return;

        if (Input.GetKey(KeyCode.LeftShift))
        {
            Cursor.lockState = CursorLockMode.Locked;
            MouseNavigator.isActive = false;

            yaw += horizontalSpeed * Input.GetAxis("Mouse X");
            pitch -= verticalSpeed * Input.GetAxis("Mouse Y");

            mainCamera.transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);

            if (Input.GetKey(KeyCode.Z))
            {
                Vector3 pos = mainCamera.transform.position;
                pos += Time.deltaTime * horizontalSpeed * mainCamera.transform.forward;
                //Debug.Log("moving forward from " + mainCamera.transform.position + " to " + pos + " with added " + Time.deltaTime * horizontalSpeed);
                mainCamera.transform.position = pos;
            }

            if (Input.GetKey(KeyCode.S))
            {
                Vector3 pos = mainCamera.transform.position;
                pos -= Time.deltaTime * horizontalSpeed * mainCamera.transform.forward;
                mainCamera.transform.position = pos;
            }

            if (Input.GetKey(KeyCode.D))
            {
                Vector3 pos = mainCamera.transform.position;
                pos += Time.deltaTime * horizontalSpeed * mainCamera.transform.right;
                mainCamera.transform.position = pos;
            }

            if (Input.GetKey(KeyCode.Q))
            {
                Vector3 pos = mainCamera.transform.position;
                pos -= Time.deltaTime * horizontalSpeed * mainCamera.transform.right;
                mainCamera.transform.position = pos;
            }

            ////Detect when the up arrow key has been released
            //if (Input.GetKeyUp(KeyCode.UpArrow))
            //    Debug.Log("Up Arrow key was released.");
        }
        else
        {
            MouseNavigator.isActive = true;
            Cursor.lockState = CursorLockMode.None;
            yaw = mainCamera.transform.rotation.eulerAngles.y;
            pitch = mainCamera.transform.rotation.eulerAngles.x;
        }

    }
}
