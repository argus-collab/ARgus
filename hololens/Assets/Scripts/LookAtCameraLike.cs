using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCameraLike : MonoBehaviour
{
    public GameObject toFace;
    public GameObject like;

    // Update is called once per frame
    void Update()
    {
        if (toFace != null && like != null)
        {
            like.transform.LookAt(toFace.transform.position);
            transform.rotation = like.transform.rotation;
        }
            
    }
}
