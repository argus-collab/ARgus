using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Windows.Kinect;
using System;


public class KinectOcclusionManager : MonoBehaviour
{
    public GameObject ColorSourceManager;
    public GameObject DepthSourceManager;

    private ColorSourceManager _ColorManager;
    private DepthSourceManager _DepthManager;

    private KinectSensor _Sensor;
    private CoordinateMapper _Mapper;

    //private Texture2D kinectDepthTexture;
    //public double _DepthScale = 0.1f;


    void Start()
    {
        Camera.main.depthTextureMode = DepthTextureMode.Depth;

        // kinect senseor
        _Sensor = KinectSensor.GetDefault();
        if (_Sensor != null)
        {
            _Mapper = _Sensor.CoordinateMapper;
            var colorFrameDesc = _Sensor.ColorFrameSource.FrameDescription;
            var depthFrameDesc = _Sensor.DepthFrameSource.FrameDescription;

            Debug.Log("color res : " + colorFrameDesc.Height + " / " + colorFrameDesc.Width);
            Debug.Log("depth res : " + depthFrameDesc.Height + " / " + depthFrameDesc.Width);

            // kinect color mapped depth texture
            // Single channel(R) texture format, 16 bit integer.
            //kinectDepthTexture = new Texture2D(depthFrameDesc.Width, depthFrameDesc.Height);//, TextureFormat.R16, false);

            if (!_Sensor.IsOpen) { _Sensor.Open(); }
        }

        // init shader parameters
        gameObject.GetComponent<Renderer>().material.SetTextureScale("_KinectRGBTex", new Vector2(-1, 1));
        //gameObject.GetComponent<Renderer>().material.SetTextureScale("_KinectDepthTex", new Vector2(-1, 1));
        //gameObject.GetComponent<Renderer>().material.SetTextureScale("_MainCameraDepthTex", new Vector2(-1, 1));

        Debug.Log("start OK");
    }

    

    void Update()
    {


        // get color data
        if (ColorSourceManager == null) { return; }
        _ColorManager = ColorSourceManager.GetComponent<ColorSourceManager>();
        if (_ColorManager == null) { return; }

        //get depth data
        if (DepthSourceManager == null) { return; }
        _DepthManager = DepthSourceManager.GetComponent<DepthSourceManager>();
        if (_DepthManager == null) { return; }
        ushort[] depthData = _DepthManager.GetData();

        // map depth to color
        ColorSpacePoint[] colorSpace = new ColorSpacePoint[depthData.Length];
        _Mapper.MapDepthFrameToColorSpace(depthData, colorSpace);

        // from ushort array to float array
        // the depth map have to be at a resolution of 512 * 424
        float[] depthFloatBuffer_c1 = new float[65536];
        float[] depthFloatBuffer_c2 = new float[65536];
        float[] depthFloatBuffer_c3 = new float[65536];
        float[] depthFloatBuffer_c4 = new float[20480];
        
        for (int i = 0; i < 65536; ++i)
            depthFloatBuffer_c1[i] = depthData[i];
        for (int i = 0; i < 65536; ++i)
            depthFloatBuffer_c1[i] = depthData[i + 65536];
        for (int i = 0; i < 65536; ++i)
            depthFloatBuffer_c1[i] = depthData[i + 65536 * 2];
        for (int i = 0; i < 20480; ++i)
            depthFloatBuffer_c1[i] = depthData[i + 65536 * 3];

        //gameObject.GetComponent<Renderer>().material.mainTexture = _ColorManager.GetColorTexture();
        gameObject.GetComponent<Renderer>().material.SetTexture("_KinectRGBTex", _ColorManager.GetColorTexture());
        gameObject.GetComponent<Renderer>().material.SetFloatArray("_kinectDepthMapArray_chunck1", depthFloatBuffer_c1);
        gameObject.GetComponent<Renderer>().material.SetFloatArray("_kinectDepthMapArray_chunck2", depthFloatBuffer_c2);
        gameObject.GetComponent<Renderer>().material.SetFloatArray("_kinectDepthMapArray_chunck3", depthFloatBuffer_c3);
        gameObject.GetComponent<Renderer>().material.SetFloatArray("_kinectDepthMapArray_chunck4", depthFloatBuffer_c4);
    }

    

    //private double GetAvg(ushort[] depthData, int x, int y, int width, int height)
    //{
    //    double sum = 0.0;

    //    for (int y1 = y; y1 < y + 4; y1++)
    //    {
    //        for (int x1 = x; x1 < x + 4; x1++)
    //        {
    //            int fullIndex = (y1 * width) + x1;

    //            if (depthData[fullIndex] == 0)
    //                sum += 4500;
    //            else
    //                sum += depthData[fullIndex];
    //        }
    //    }

    //    return sum / 16;
    //}

    void OnApplicationQuit()
    {
        if (_Mapper != null)
        {
            _Mapper = null;
        }

        if (_Sensor != null)
        {
            if (_Sensor.IsOpen)
            {
                _Sensor.Close();
            }

            _Sensor = null;
        }
    }
}
