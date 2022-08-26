using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KinectPositionRetriever : MonoBehaviour
{
    public string CameraName;
    public string WorldRefName;

    private GameObject kinect;
    private GameObject worldRef;

    void Start()
    {
        kinect = GameObject.Find(CameraName);
        worldRef = GameObject.Find(WorldRefName);
    }

    void Update()
    {
        transform.localPosition = - worldRef.transform.position;
        transform.rotation = Quaternion.Inverse(worldRef.transform.rotation); 
    }
}
