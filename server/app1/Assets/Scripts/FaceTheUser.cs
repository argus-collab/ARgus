using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceTheUser : MonoBehaviour
{
    public Camera toFace;

    void Start()
    {
        if (toFace == null)
            toFace = Camera.main;
    }

    void Update()
    {
        transform.LookAt(toFace.transform);
    }
}
