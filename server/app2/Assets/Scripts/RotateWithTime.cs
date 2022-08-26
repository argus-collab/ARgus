using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateWithTime : MonoBehaviour
{
    public RectTransform toRotate;
    public float timeStep;
    public float step;

    private float startingTime;

    void Start()
    {
        startingTime = Time.time;
    }

    void Update()
    {
        if(Time.time - startingTime >= timeStep)
        {
            Vector3 iconAngle = toRotate.localEulerAngles;
            iconAngle.z += step;

            toRotate.localEulerAngles = iconAngle;

            startingTime = Time.time;
        }
    }
}
