using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionFromMouse : MonoBehaviour
{
    public Camera mainCamera;
    public GameObject sphere;
    public float distanceFromSphere = 0.5f;
    public float maxMouseDisplacement = 600;

    private GameObject cursor;

    private bool isManipulating = false;
    private Vector2 initialMousePosition;

    private Vector3 facingVector;
    private Vector3 lateralVector;

    private void Update()
    {
        if (Input.GetButton("Fire1"))
        {
            if(!isManipulating)
            {
                isManipulating = true;
                initialMousePosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);

                if(cursor == null)
                {
                    cursor = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    cursor.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                    cursor.transform.position = sphere.transform.position + distanceFromSphere * Vector3.up;

                    Camera cam = cursor.AddComponent<Camera>();
                    cam.targetDisplay = 1;
                }

                facingVector = Vector3.Normalize(mainCamera.transform.position - sphere.transform.position);
                lateralVector = Vector3.Cross(facingVector, Vector3.up);
            }

            float theta = (Input.mousePosition.x - initialMousePosition.x) * (Mathf.PI / 2) / maxMouseDisplacement;
            float phi = (Input.mousePosition.y - initialMousePosition.y) * (Mathf.PI / 2) / maxMouseDisplacement;

            Debug.Log("theta = " + 360 * theta / (2 * Mathf.PI));
            Debug.Log("phi = " + 360 * phi / (2 * Mathf.PI));

            //Vector3 cursorPos;
            float x = distanceFromSphere * Mathf.Cos(phi) * Mathf.Cos(theta);
            float y = distanceFromSphere * Mathf.Cos(phi) * Mathf.Sin(theta);
            float z = distanceFromSphere * Mathf.Sin(phi);

            cursor.transform.position = sphere.transform.position + x * facingVector + y * lateralVector + z * Vector3.up;

            cursor.transform.LookAt(sphere.transform.position);
        }
        else
        {
            //Destroy(cursor);
            isManipulating = false;
        }
    }
}
