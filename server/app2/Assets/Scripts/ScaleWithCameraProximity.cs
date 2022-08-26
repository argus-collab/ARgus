using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleWithCameraProximity : MonoBehaviour
{
    public float minRadius = 0.5f;
    public float maxRadius = 1f;

    public float minHeight = 2.5f;
    public float maxHeight = 6f;

    public float minScale = 0.25f;
    public float maxScale = 0.5f;

    public CapsuleCollider col;
    public GameObject cyl;

    public GameObject mainCamera;

    private float distance;
    private float dmin = 0.3f;
    private float dmax = 1.5f;

    void Update()
    {
        if (mainCamera == null) return;

        distance = Mathf.Abs((mainCamera.transform.position - cyl.transform.position).magnitude);

        UpdateCylindreScale();
        UpdateColliderScale();
    }

    float GetA(float min, float max)
    {
        return (max - min) / (dmax - dmin);
    }

    float GetB(float min, float max)
    {
        return min - GetA(min, max) * dmin;
    }

    void UpdateCylindreScale()
    {
        float scaleFactor = distance * GetA(minScale, maxScale) + GetB(minScale, maxScale);

        if (scaleFactor > maxScale) scaleFactor = maxScale;
        if (scaleFactor < minScale) scaleFactor = minScale;

        Vector3 scale = new Vector3();
        scale.x = scaleFactor;
        scale.y = scaleFactor;
        scale.z = scaleFactor;

        cyl.transform.localScale = scale;
    }

    void UpdateColliderScale()
    {
        float radius = distance * GetA(minRadius, maxRadius) + GetB(minRadius, maxRadius);
        float height = distance * GetA(minHeight, maxHeight) + GetB(minHeight, maxHeight);

        if (radius > maxRadius) radius = maxRadius;
        if (radius < minRadius) radius = minRadius;

        if (height > maxHeight) height = maxHeight;
        if (height < minHeight) height = minHeight;

        col.radius = radius;
        col.height = height;
    }
}
