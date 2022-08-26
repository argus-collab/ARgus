using UnityEngine;
using Windows.Kinect;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Globalization;

[ExecuteInEditMode]
public class RenderOccludedRGBKinect : MonoBehaviour
{
    [Range(0f, 3f)]
    public float kinectDepthScale = 0.0001f;

    [Range(0f, 3f)]
    public float unityDepthScale = 1f;

    [Range(0f, 100f)]
    public float depthClippingDistance = 20f;

    public bool VerticalMirrorProcessedImage = true;
    public bool HorizontalMirrorProcessedImage = true;
    public bool UnityCamSupport = false; 
    public bool DebugDisplayDepth = false;

    private Shader _shader;
    private Shader shader
    {
        get { return _shader != null ? _shader : (_shader = Shader.Find("Custom/KinectOcclusion")); }
    }

    private Material _material;
    private Material material
    {
        get
        {
            if (_material == null)
            {
                _material = new Material(shader);
                _material.hideFlags = HideFlags.HideAndDontSave;
            }
            return _material;
        }
    }

    public GameObject ColorSourceManager;
    public GameObject DepthSourceManager;

    private ColorSourceManager _ColorManager;
    private DepthSourceManager _DepthManager;

    private KinectSensor _Sensor;
    private CoordinateMapper _Mapper;

    private ushort[] depthData;
    private DepthSpacePoint[] depthSpace;


    private int depthHeight;
    private int depthWidth;
    private int colorHeight;
    private int colorWidth;

    private ComputeBuffer depthBuffer;
    private List<float> depthFloatBuffer;

    private ComputeBuffer depthSpaceBufferX;
    private ComputeBuffer depthSpaceBufferY;
    private ComputeBuffer depthSpaceBuffer;
    private List<float> depthSpaceFloatBufferX;
    private List<float> depthSpaceFloatBufferY;

    public void Reset()
    {
        OnApplicationQuit();
        Start();
    }

    private void Start()
    {
        depthData = new ushort[1];



        if (shader == null || !shader.isSupported)
        {
            enabled = false;
            print("Shader " + shader.name + " is not supported");
            return;
        }

        // turn on depth rendering for the camera so that the shader can access it via _CameraDepthTexture
        Camera.main.depthTextureMode = DepthTextureMode.Depth;

        // kinect senseor
        _Sensor = KinectSensor.GetDefault();
        if (_Sensor != null)
        {
            _Mapper = _Sensor.CoordinateMapper;
            var colorFrameDesc = _Sensor.ColorFrameSource.FrameDescription;
            var depthFrameDesc = _Sensor.DepthFrameSource.FrameDescription;

            depthHeight = depthFrameDesc.Height;
            depthWidth = depthFrameDesc.Width;
            colorHeight = colorFrameDesc.Height;
            colorWidth = colorFrameDesc.Width;

            Debug.Log("color res : " + colorHeight + " / " + colorWidth);
            Debug.Log("depth res : " + depthHeight + " / " + depthWidth);

            material.SetInt("_KinectDepthHeight", depthHeight);
            material.SetInt("_KinectDepthWidth", depthWidth);
            material.SetInt("_KinectColorHeight", colorHeight);
            material.SetInt("_KinectColorWidth", colorWidth);

            depthBuffer = new ComputeBuffer(depthHeight * depthWidth, sizeof(float), ComputeBufferType.Default);
            depthFloatBuffer = new List<float>(depthHeight * depthWidth);

            depthSpaceBufferX = new ComputeBuffer(colorHeight * colorWidth, sizeof(float), ComputeBufferType.Default);
            depthSpaceBufferY = new ComputeBuffer(colorHeight * colorWidth, sizeof(float), ComputeBufferType.Default);
            depthSpaceBuffer = new ComputeBuffer(colorHeight * colorWidth, sizeof(float) * 2, ComputeBufferType.Default);

            depthSpaceFloatBufferX = new List<float>(colorHeight * colorWidth);
            depthSpaceFloatBufferY = new List<float>(colorHeight * colorWidth);

            for (int i = 0; i < depthHeight * depthWidth; ++i)
            {
                depthFloatBuffer.Add(0.5f);// depthData[i];
            }
            for (int i = 0; i < colorHeight * colorWidth; ++i)
            { 
                depthSpaceFloatBufferX.Add(0.5f);
                depthSpaceFloatBufferY.Add(0.5f);
            }

            depthBuffer.SetData<float>(depthFloatBuffer);
            depthSpaceBufferX.SetData<float>(depthSpaceFloatBufferX);
            depthSpaceBufferY.SetData<float>(depthSpaceFloatBufferY);

            material.SetBuffer("_KinectDepthBuffer", depthBuffer);
            material.SetBuffer("_KinectDepthSpaceBufferX", depthSpaceBufferX);
            material.SetBuffer("_KinectDepthSpaceBufferY", depthSpaceBufferY);
            material.SetBuffer("_KinectDepthSpaceBuffer", depthSpaceBuffer);

            depthSpace = new DepthSpacePoint[colorWidth * colorHeight];


            if (!_Sensor.IsOpen) { _Sensor.Open(); }
        }

        // init shader parameters
        material.SetTextureScale("_KinectRGBTex", new Vector2(-1, 1));
        material.SetTextureScale("_MainCameraRGBTex", new Vector2(-1, 1));

        material.SetFloat("_KinectDepthScale", kinectDepthScale);
        material.SetFloat("_UnityDepthScale", unityDepthScale);

        material.SetInt("_KinectDepthHeight", depthHeight);
        material.SetInt("_KinectDepthWidth", depthWidth);

        material.SetInt("_KinectColorHeight", colorHeight);
        material.SetInt("_KinectColorWidth", colorWidth);

        Debug.Log("start OK");
    }

    private void OnDisable()
    {
        if (_material != null)
            DestroyImmediate(_material);
    }

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

    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (shader != null)
        {
            material.SetInt("_UnityCamSupport", UnityCamSupport ? 1 : 0);
            material.SetInt("_DebugDisplayDepth", DebugDisplayDepth ? 1 : 0);
            material.SetInt("_VerticalMirrorProcessedImage", VerticalMirrorProcessedImage ? 1 : 0);
            material.SetInt("_HorizontalMirrorProcessedImage", HorizontalMirrorProcessedImage ? 1 : 0);

            material.SetFloat("_KinectDepthScale", kinectDepthScale);
            material.SetFloat("_UnityDepthScale", unityDepthScale);
            material.SetFloat("_DepthClippingDistance", depthClippingDistance);
            material.SetTexture("_MainCameraRGBTex", src);

            Graphics.Blit(src, dest, material);
        }
        else
        {
            Graphics.Blit(src, dest);
        }
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
        depthData = _DepthManager.GetData();
        if (depthData == null) { return; }

        if (depthData.Length < depthHeight * depthWidth) { return; }

        _Mapper.MapColorFrameToDepthSpace(depthData, depthSpace);

        for (int i = 0; i < depthData.Length; ++i)
            depthFloatBuffer[i] = depthData[i];
        
        depthBuffer.SetData<float>(depthFloatBuffer);
        depthSpaceBuffer.SetData(depthSpace);

        material.SetBuffer("_KinectDepthBuffer", depthBuffer);
        material.SetBuffer("_KinectDepthSpaceBuffer", depthSpaceBuffer);
        material.SetTexture("_KinectRGBTex", _ColorManager.GetColorTexture());
    }
}