using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    public GameObject toFace;

    // Update is called once per frame
    void Update()
    {
        if (toFace != null)
            transform.LookAt(toFace.transform.position);
    }
}
