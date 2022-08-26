using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.WebRTC.Unity;

public class WebcamSelector : WebcamSource
{
    IReadOnlyList<Microsoft.MixedReality.WebRTC.VideoCaptureDevice> deviceList;

    public int index = 0;


    // Start is called before the first frame update
    void Awake()
    {

        deviceList = PeerConnection.GetVideoCaptureDevicesAsync().Result;

        // For example, print them to the standard output
        //foreach (var device in deviceList)
        //{
        //    Debug.Log($"Found webcam {device.name} (id: {device.id})");
        //    if (device.name == "UnityCam")
        //    {
        //        this.WebcamDevice = device;
        //        Debug.Log($"Using {device.name}");

        //    }
        //}

        int i = 0;
        foreach (var device in deviceList)
        {
            if (index == i)
            {
                this.WebcamDevice = device;
                Debug.Log($"Using {device.name}");
            }
            i++;
        }


    }

    // Update is called once per frame
    void Update()
    {

    }
}

