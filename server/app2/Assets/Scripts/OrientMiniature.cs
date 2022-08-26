using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OrientMiniature : MonoBehaviour
{
    public float offset = 0.3f;
    public Camera mainCamera;
    public GameObject go;
    public GameObject view;

    public float maxViewSize = 1f;
    public float growingSpeed = 0.5f;

    void Update()
    {
        SetViewPosition();
        transform.rotation = Quaternion.LookRotation(mainCamera.transform.forward);

        IsGOSelected();
    }

    void SetViewPosition()
    {
        float x = offset;
        float y = offset;

        if (Input.mousePosition.x > Screen.width/2)
            x = -offset;

        if (Input.mousePosition.y > Screen.height/2)
            y = -offset;

        float distanceFromCamera = (mainCamera.transform.position - go.transform.position).magnitude;
        transform.position = go.transform.position - (distanceFromCamera - offset) * mainCamera.transform.forward + y * mainCamera.transform.up + x * mainCamera.transform.right;
    }

    void IsGOSelected()
    {
        RaycastHit hit;
        Ray ray = mainCamera.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f));

        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            if (SelectEntity(hit.collider.gameObject) == go)
            {
                Debug.Log("point cursor at " + go.name);
                GrowView();
            }
            else
            {
                ShrinkView();
            }
        }
        else
        {
            ShrinkView();
        }

    }

    void GrowView()
    {
        if (view.transform.localScale.x < maxViewSize
            && view.transform.localScale.y < maxViewSize
            && view.transform.localScale.z < maxViewSize)
        {
            view.transform.localScale += new Vector3(
            Time.deltaTime * growingSpeed,
            Time.deltaTime * growingSpeed,
            Time.deltaTime * growingSpeed);
        }
    }

    void ShrinkView()
    {
        if (view.transform.localScale.x > 0.0 
            && view.transform.localScale.y > 0.0
            && view.transform.localScale.z > 0.0)
        {
            view.transform.localScale -= new Vector3(
                Time.deltaTime * growingSpeed*2,
                Time.deltaTime * growingSpeed*2,
                Time.deltaTime * growingSpeed*2);
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
}
