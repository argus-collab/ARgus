using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanarViewpointIndicator : MonoBehaviour
{
    public float angle = 0;
    private float maxRadius;
    private float minRadius;
    public float rotationOffsetImage = 90;
    public RectTransform indicatorHololens;
    public GameObject GOHololens;
    public RectTransform indicatorKinect;
    public GameObject GOKinect;
    public float radius = 250;

    public GameObject ghostcursor;

    private void Start()
    {
        maxRadius = Screen.height / 2 - 50;
        minRadius = maxRadius / 3;
    }

    void Update()
    {
        if (GOHololens == null)
            return;
        if (GOKinect == null)
            return;

        //float x = ClampRadius(radius) * Mathf.Cos(AngularClamp(angle) * Mathf.Deg2Rad);
        //float y = ClampRadius(radius) * Mathf.Sin(AngularClamp(angle) * Mathf.Deg2Rad);

        //indicatorHololens.localPosition = new Vector3(x, y, 0);
        //indicatorHololens.localRotation = Quaternion.Euler(0, 0, AngularClamp(angle) + rotationOffsetImage);

        UpdateIndicator(GOKinect, indicatorKinect);
        //UpdateIndicator(GOHololens, indicatorHololens);
    }

    void UpdateIndicator(GameObject go, RectTransform indicator)
    {
        Vector3 d = go.transform.position - transform.position;
        radius = maxRadius * (d.magnitude / 6); // 6m ~ max between view point 

        ghostcursor.transform.LookAt(go.transform);
        Vector3 proj = Vector3.ProjectOnPlane(ghostcursor.transform.forward, transform.forward);
        angle = Mathf.Atan2(proj.y, proj.x) * Mathf.Rad2Deg;

        //ghostcursor.transform.localEulerAngles.z
        //angle = -Quaternion.FromToRotation(transform.right, ghostcursor.transform.right).eulerAngles.y;

        float x = ClampRadius(radius) * Mathf.Cos(AngularClamp(angle) * Mathf.Deg2Rad);
        float y = ClampRadius(radius) * Mathf.Sin(AngularClamp(angle) * Mathf.Deg2Rad);

        indicator.localPosition = new Vector3(x, y, 0);
        indicator.localRotation = Quaternion.Euler(0, 0, AngularClamp(angle) + rotationOffsetImage);
    }

    float AngularClamp(float value)
    {
        if (value > 360)
            value -= 360;
        if (value < 0)
            value += 360;
        return value;
    }

    float ClampRadius(float r)
    {
        float result = r;

        if (r > maxRadius)
            result = maxRadius;
        else if (r < minRadius)
            result = minRadius;

        return result;
    }
    
}
