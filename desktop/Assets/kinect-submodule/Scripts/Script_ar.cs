/****************************************************************************
 *
 * ViSP, open source Visual Servoing Platform software.
 * Copyright (C) 2005 - 2020 by Inria. All rights reserved.
 *
 * This software is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 * See the file LICENSE.txt at the root directory of this source
 * distribution for additional information about the GNU GPL.
 *
 * For using ViSP with software that can not be combined with the GNU
 * GPL, please contact Inria about acquiring a ViSP Professional
 * Edition License.
 *
 * See http://visp.inria.fr for more information.
 *
 * This software was developed at:
 * Inria Rennes - Bretagne Atlantique
 * Campus Universitaire de Beaulieu
 * 35042 Rennes Cedex
 * France
 *
 * If you have questions regarding the use of this file, please contact
 * Inria at visp@inria.fr
 *
 * This file is provided AS IS with NO WARRANTY OF ANY KIND, INCLUDING THE
 * WARRANTY OF DESIGN, MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE.
 *
 * Description:
 * Unity application that shows how to use ViSPUnity plugin.
 *
 *****************************************************************************/

/*!
 \example Script_ar.cs
 Unity CSharp script that allows to detect and AprilTag and display a cube in Augmented Reality using ViSPUnity plugin.
*/

using Microsoft.MixedReality.Toolkit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Windows.Kinect;

public class Script_ar : MonoBehaviour
{
    // Functions imported FROM ViSPUnity wrapper (DLL on Windows, Bundle on OSX)
    [DllImport("ViSPUnity", CallingConvention = CallingConvention.Cdecl, EntryPoint = "Visp_EnableDisplayForDebug")]
    public static extern void Visp_EnableDisplayForDebug(bool enable_display);
    [DllImport("ViSPUnity", CallingConvention = CallingConvention.Cdecl, EntryPoint = "Visp_WrapperFreeMemory")]
    public static extern void Visp_WrapperFreeMemory();
    [DllImport("ViSPUnity", CallingConvention = CallingConvention.Cdecl, EntryPoint = "Visp_ImageUchar_SetFromColor32Array")]
    public static extern void Visp_ImageUchar_SetFromColor32Array(Color32[] bitmap, int height, int width);
    [DllImport("ViSPUnity", CallingConvention = CallingConvention.Cdecl, EntryPoint = "Visp_CameraParameters_Init")]
    public static extern void Visp_CameraParameters_Init(double cam_px, double cam_py, double cam_u0, double cam_v0);
    [DllImport("ViSPUnity", CallingConvention = CallingConvention.Cdecl, EntryPoint = "Visp_DetectorAprilTag_Init")]
    public static extern void Visp_DetectorAprilTag_Init(float quad_decimate, int nthreads);
    [DllImport("ViSPUnity", CallingConvention = CallingConvention.Cdecl, EntryPoint = "Visp_DetectorAprilTag_Process")]
    public static extern bool Visp_DetectorAprilTag_Process(double tag_size, float[] tag_cog, float[] tag_length, float[] tag_cMo, double[] detection_time);
    [DllImport("ViSPUnity", CallingConvention = CallingConvention.Cdecl, EntryPoint = "Visp_DetectorAprilTag_Process_custom")]
    public static extern bool Visp_DetectorAprilTag_Process_custom(double tag_size, float[] tag_cog, float[] tag_length, float[] tag_corners, float[] tag_cMo, double[] detection_time);
    [DllImport("ViSPUnity", CallingConvention = CallingConvention.Cdecl, EntryPoint = "Visp_DetectorAprilTag_Process_multiple")]
    public static extern bool Visp_DetectorAprilTag_Process_multiple(double tag_size, float nb_tag, float[] tag_cog, float[] tag_length, float[] tag_corners, float[] tag_cMo, double[] detection_time);
    [DllImport("ViSPUnity", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ExtendedVisp_PlaneFitting_Process")]
    public static extern bool ExtendedVisp_PlaneFitting_Process(float nb_pts, float[] pts, float[] result_centroid, float[] result_normal);

    public enum DebugType {
        Enabled,
        Disabled
    };

    GameObject m_cube;
    GameObject m_cube_pivot;

    // For storing tag characteristics returned by ViSPunity wrapper
    float[] m_tag_cog;
    float[] m_tag_length;
    float[] m_tag_corners;
    float[] m_tag_cMo;
    double[] m_detection_time;

    float[] pts_array;
    float[] result_centroid;
    float[] result_normal;

    float m_aspect_ratio;
    bool m_log_start = true;
    bool m_log_process = true;

    public GameObject kinectRepresentation;
    public GameObject zeroScene;

    [Header("Calibrated scene part")]
    public GameObject pivotGameObject;
    public GameObject scaleGameObject;


    [Header("Camera Identifier")]
    public GameObject ColorSourceManager;
    public GameObject DepthSourceManager;

    private ColorSourceManager _ColorManager;
    private DepthSourceManager _DepthManager;

    private CoordinateMapper _Mapper;

    private ushort[] depthData;
    private DepthSpacePoint[] depthSpace;

    [Header("Camera Parameters")] //some default values provided
    public double cam_px = 600;
    public double cam_py = 600;
    public double cam_u0 = 320;
    public double cam_v0 = 240;

    [Header("Tag infos")]
    public double tag_size = 0.053; // in meters
    public int nb_tag = 1;

    [Header("Tag Detection Settings")]
    public float quad_decimate = 1;
    public int nthreads = 1;

    [Header("Debugging Settings")]
    public Script_ar.DebugType debug_display = DebugType.Disabled;

    private Texture2D kinectRGBTexture;
    private Texture2D _texture2D;
    private Texture2D acquisitionTexture;
    
    [Range(-1,1)]
    public float k1 = 0f;

    [Range(-1, 1)]
    public float k2 = 0f;

    [Range(-1, 1)]
    public float k3 = 0f;

    public int downsizingRatio = 1;
    public bool doAcquireFrame = false;
    public bool doProcessDetection = false;
    public bool doDistortion = false;
    public bool customDebug = false;

    KinectSensor _Sensor;

    private int colorHeight;
    private int colorWidth;
    private int depthHeight;
    private int depthWidth;

    private Vector3 d_lastRecordedPosition;

    public bool horizontalMirror = true;
    public bool verticalMirror = true;

    void Start()
    {
        d_lastRecordedPosition = Vector3.zero;

        m_tag_cog = new float[nb_tag * 2];
        m_tag_length = new float[nb_tag * 6];
        m_tag_corners = new float[nb_tag * 8];
        m_tag_cMo = new float[nb_tag * 16];
        m_detection_time = new double[nb_tag * 1];

        pts_array = new float[5 * 3 * nb_tag];
        result_centroid = new float[3];
        result_normal = new float[3];

        m_cube = scaleGameObject; // GameObject.Find("Cube");
        m_cube_pivot = pivotGameObject;// GameObject.Find("Cube_pivot");

        Visp_EnableDisplayForDebug((debug_display == DebugType.Enabled) ? true : false);
        Visp_CameraParameters_Init(cam_px, cam_py, cam_u0, cam_v0);
        Visp_DetectorAprilTag_Init(quad_decimate, nthreads);

        if(m_log_start) {
            Debug.Log("Tag detection settings: quad_decimate=" + quad_decimate + " nthreads=" + nthreads);
            Debug.Log("Camera parameters: u0=" + cam_u0 + " v0=" + cam_v0 + " px=" + cam_px + " py=" + cam_py);
            Debug.Log("Tag size [m]: " + tag_size);
        }

        _Sensor = KinectSensor.GetDefault();
        if (_Sensor != null)
            _Mapper = _Sensor.CoordinateMapper;

        var colorFrameDesc = _Sensor.ColorFrameSource.FrameDescription;
        var depthFrameDesc = _Sensor.DepthFrameSource.FrameDescription;
        
        depthHeight = depthFrameDesc.Height;
        depthWidth = depthFrameDesc.Width;

        colorHeight = colorFrameDesc.Height;
        colorWidth = colorFrameDesc.Width;

        depthSpace = new DepthSpacePoint[colorWidth * colorHeight];
    }

    void OnGUI()
    {
        if(customDebug && _texture2D != null)
            GUI.DrawTexture(new Rect(0, 0, _texture2D.width, _texture2D.height), _texture2D);//, ScaleMode.ScaleAndCrop, true, 10.0F);

        if (GUI.Button(new Rect(10, Camera.main.pixelHeight - 50, 200, 20), "Calibrate"))
        {
            AcquireFrame();
            ProcessDetection();

        }
    }

    private void Update()
    {
        _Mapper = _Sensor.CoordinateMapper;

        if (ColorSourceManager == null) { return; }
        _ColorManager = ColorSourceManager.GetComponent<ColorSourceManager>();
        if (_ColorManager == null) { return; }

        kinectRGBTexture = _ColorManager.GetColorTexture();
        int newWidth = (int) kinectRGBTexture.width / downsizingRatio;
        int newHeight = (int) kinectRGBTexture.height / downsizingRatio;

        if (DepthSourceManager == null) { return; }
        _DepthManager = DepthSourceManager.GetComponent<DepthSourceManager>();
        
        if (_DepthManager == null) { return; }
        depthData = _DepthManager.GetData();
        
        if (depthData.Length < depthHeight * depthWidth) { return; }
        _Mapper.MapColorFrameToDepthSpace(depthData, depthSpace);

        if (doDistortion)
        {
            _texture2D.Resize(newWidth, newHeight);
            for (int i = 0; i < newWidth; ++i)
            {
                for (int j = 0; j < newHeight; ++j)
                {
                    _texture2D.SetPixel(i, j, acquisitionTexture.GetPixel(i, j));
                }
            }
            _texture2D = RemoveDistortion();
        }


        if (doAcquireFrame)
        {
            doAcquireFrame = false;
            AcquireFrame();
        }

        if (doProcessDetection)
        {
            doProcessDetection = false;
            ProcessDetection();
        }

        // debug
        if (Input.GetMouseButtonDown(0))
        {
            //Vector3 vec = FromVispScreenToWorld(Input.mousePosition.x, colorHeight - Input.mousePosition.y);
            Vector3 vec = FromVispScreenToWorld(/*colorWidth - */Input.mousePosition.x, colorHeight - Input.mousePosition.y);
            Debug.Log("mouse x " + Input.mousePosition.x + ", " + Input.mousePosition.y + ", vec : " + vec.ToString("F4"));
            //AddDebugSphere(vec);
            Debug.Log((d_lastRecordedPosition - vec).magnitude);
            d_lastRecordedPosition = vec;

            ScenePartManager scenePartManager = GameObject.FindObjectOfType<ScenePartManager>();
            scenePartManager.AddGameObjectInScenePart("DebugSphere", scenePartManager.targetScene.transform.InverseTransformPoint(vec), Quaternion.identity, false);
        }
    }   

    void AcquireFrame()
    {
        if (m_log_start)
        {
            Debug.Log("Image size: " + kinectRGBTexture.width + " x " + kinectRGBTexture.height);
            m_log_start = false;
        }

        int newWidth = kinectRGBTexture.width / downsizingRatio;
        int newHeight = kinectRGBTexture.height / downsizingRatio;
        _texture2D = new Texture2D(newWidth, newHeight);
        acquisitionTexture = new Texture2D(newWidth, newHeight);

        m_aspect_ratio = (float)kinectRGBTexture.width / kinectRGBTexture.height;
        transform.localScale = new Vector3(m_aspect_ratio, 1f, 1f);

        // downsize
        acquisitionTexture.Resize(newWidth, newHeight);
        _texture2D.Resize(newWidth, newHeight);

        for (int i = 0; i < newWidth; ++i)
        {
            for (int j = 0; j < newHeight; ++j)
            {
                acquisitionTexture.SetPixel(i, j, kinectRGBTexture.GetPixel(i * kinectRGBTexture.width / newWidth, j * kinectRGBTexture.height / newHeight));
                _texture2D.SetPixel(i, j, kinectRGBTexture.GetPixel(i * kinectRGBTexture.width / newWidth, j * kinectRGBTexture.height / newHeight));
            }
        }

        // horizontal mirror
        // whyyyy ????

        if(horizontalMirror)
        {
            Texture2D mirror = new Texture2D(newWidth, newHeight);
            for (int i = 0; i < newWidth; i++)
            {
                for (int j = newHeight - 1; j >= 0; j--)
                {
                    mirror.SetPixel(i, j, _texture2D.GetPixel(i, (newHeight - 1 - j)));
                }
            }


            acquisitionTexture = mirror;
            _texture2D = mirror;
        }

        if (verticalMirror)
        {
            Texture2D mirror2 = new Texture2D(newWidth, newHeight);

            for (int i = newWidth - 1; i >= 0; i--)
            {
                for (int j = 0; j < newHeight; j++)
                {
                    mirror2.SetPixel(i, j, _texture2D.GetPixel((newWidth - 1 - i), j));
                }
            }

            acquisitionTexture = mirror2;
            _texture2D = mirror2;
        }



        acquisitionTexture.Apply();
        _texture2D.Apply();
    }

    Vector3 FromVispScreenToWorld(float u_f, float v_f)
    {
        int u = (int)u_f;
        int v = (int)(colorHeight - v_f);



        // debug
        //int u = (int)(colorWidth - u_f);
        //int v = (int)(colorHeight - v_f);

        // end debug


        //int indexColor = v * colorWidth + (u - colorWidth / 2);
        
        
        //int indexColor = (colorHeight - v) * colorWidth + u;
        int indexColor = (colorHeight - v) * colorWidth + (colorWidth - u);

        if(indexColor >= depthSpace.Length || indexColor < 0)
        {
            Debug.LogError("invalid index color");
            return Vector3.zero;
        }

        int d_x = (int) depthSpace[indexColor].X;
        int d_y = (int) depthSpace[indexColor].Y;

        int indexDepth = d_y * depthWidth + d_x;

        if (indexDepth >= depthData.Length || indexDepth < 0)
        {
            Debug.LogError("invalid index depth");
            return Vector3.zero;
        }


        float z = depthData[indexDepth] * 0.001f;

        return Camera.main.ScreenToWorldPoint(new Vector3(u, v, z));
    }

    void AddDebugSphere(Vector3 pos)
    {

        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //GameObject targetScene = GameObject.Find("LocalTargetScene");
        //if (targetScene != null)
        //    sphere.transform.parent = targetScene.transform;
        sphere.transform.position = pos;
        sphere.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
    }

    void AddDebugPlane(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        plane.transform.position = p1;
        plane.transform.rotation = Quaternion.LookRotation(p2 - p1, p3 - p1);
        Vector3 rot = plane.transform.rotation.eulerAngles + new Vector3(0, 0, -90);
        plane.transform.rotation = Quaternion.Euler(rot.x, rot.y, rot.z);
    }

    Vector3 CrossProduct(Vector3 a, Vector3 b)
    {
        return new Vector3(a.y * b.z - a.z * b.y, a.z * b.x - a.x * b.z, a.x * b.y - a.y*b.x);
    }

    bool IsColinear(Vector3 a, Vector3 b)
    {
        // colinear => cross product equal to zero vector
        Vector3 ab = CrossProduct(a, b);
        return ab.x == 0 && ab.y == 0 && ab.z == 0;
    }

    // pts = set of (almost) coplanar points
    // return the mean approximated normal of this plane
    Vector3 MeanZ(List<Vector3> pts)
    {
        // list usefull vector that can be made from given pts (avoid redondancy with colinearity)
        List<Vector3> vectors = new List<Vector3>();
        for(int i = 0; i < pts.Count;++i)
        {
            for(int j = 0; j < pts.Count;++j)
            {
                if(i!=j)
                {
                    Vector3 v = pts[j] - pts[i];

                    // check if v is colinear to another already register vector
                    bool isColinearToExistingVector = false;
                    for (int k = 0; !isColinearToExistingVector && k < vectors.Count; ++k)
                    {
                        isColinearToExistingVector = IsColinear(v, vectors[k]);
                        //Debug.Log("v = " + v + " / vectors[k] = " + vectors[k] + " / isColinear : " + isColinearToExistingVector + " / cross product : " + CrossProduct(v, vectors[k]).ToString("F4"));
                    }

                    if (!isColinearToExistingVector)
                        vectors.Add(v);
                }
            }
        }

        List<Vector3> z = new List<Vector3>();
        for(int i = 0; i < vectors.Count; ++i)
            for (int j = 0; j < vectors.Count; ++j)
                if (i != j)
                {
                    Debug.Log("a = " + vectors[i] + " / b = " + vectors[j] + " / ab = " + CrossProduct(vectors[i], vectors[j]).ToString("F4"));
                    z.Add(CrossProduct(vectors[i], vectors[j]));
                }

        Vector3 zMean = Vector3.zero;
        for (int i = 0; i < z.Count; ++i)
        {
            zMean += z[i];
        }

        Debug.Log("zMean sum : " + zMean.ToString("F4"));


        zMean.x /= z.Count;
        zMean.y /= z.Count;
        zMean.z /= z.Count;

        Debug.Log("zMean : " + zMean.ToString("F4"));

        return zMean;
    }

    // https://math.stackexchange.com/questions/99299/best-fitting-plane-given-a-set-of-points

    void ProcessDetection()
    {
        Visp_ImageUchar_SetFromColor32Array(_texture2D.GetPixels32(), _texture2D.height, _texture2D.width);
        //bool success = Visp_DetectorAprilTag_Process(tag_size, m_tag_cog, m_tag_length, m_tag_cMo, m_detection_time);
        //bool success = Visp_DetectorAprilTag_Process_custom(tag_size, m_tag_cog, m_tag_length, m_tag_corners, m_tag_cMo, m_detection_time);
        bool success = Visp_DetectorAprilTag_Process_multiple(tag_size, nb_tag, m_tag_cog, m_tag_length, m_tag_corners, m_tag_cMo, m_detection_time);

        if (success)
        {
            List<Vector3> pts = new List<Vector3>();
            Vector3 planeCenter = Vector3.zero;
            List<Vector3> listForward = new List<Vector3>();
            List<Vector3> listUpward = new List<Vector3>();

            Vector3 firstForward = Vector3.zero;

            for (int i=0;i<nb_tag;++i)
            {
                if (m_log_process)
                {
                    Debug.Log("tag cog: " + m_tag_cog[i * 2 + 0] + " " + m_tag_cog[i * 2 + 1]);
                    Debug.Log("tag length: " + m_tag_length[i * 6 + 0] + " " + m_tag_length[i * 6 + 1] + " " + m_tag_length[i * 6 + 2] + " " + m_tag_length[i * 6 + 3] + " " + m_tag_length[i * 6 + 4] + " " + m_tag_length[i * 6 + 5]);
                    Debug.Log("tag corner 1 : " + m_tag_corners[i * 8 + 0] + " " + m_tag_corners[i * 8 + 1]);
                    Debug.Log("tag corner 2 : " + m_tag_corners[i * 8 + 2] + " " + m_tag_corners[i * 8 + 3]);
                    Debug.Log("tag corner 3 : " + m_tag_corners[i * 8 + 4] + " " + m_tag_corners[i * 8 + 5]);
                    Debug.Log("tag corner 4 : " + m_tag_corners[i * 8 + 6] + " " + m_tag_corners[i * 8 + 7]);
                    Debug.Log("cMo:\n" + m_tag_cMo[0] + " " + m_tag_cMo[1] + " " + m_tag_cMo[2] + " " + m_tag_cMo[3] + "\n"
                                        + m_tag_cMo[4] + " " + m_tag_cMo[5] + " " + m_tag_cMo[6] + " " + m_tag_cMo[7] + "\n"
                                        + m_tag_cMo[8] + " " + m_tag_cMo[9] + " " + m_tag_cMo[10] + " " + m_tag_cMo[11]);
                    Debug.Log("Detection process time: " + m_detection_time[0] + " ms");
                }

                //int u = (int) m_tag_cog[0];
                //int v = (int) (colorHeight - m_tag_cog[1]);

                ////int indexColor = v * colorWidth + (u - colorWidth / 2);
                //int indexColor = (colorHeight - v) * colorWidth + u;


                //int d_x = (int) depthSpace[indexColor].X;
                //int d_y = (int) depthSpace[indexColor].Y;

                //int indexDepth = d_y * depthWidth + d_x;
                //float z = depthData[indexDepth] * 0.001f;

                //Vector3 vec = Camera.main.ScreenToWorldPoint(new Vector3(u,v,z));
                Vector3 vec = FromVispScreenToWorld(m_tag_cog[i * 2 + 0], m_tag_cog[i * 2 + 1]);
                planeCenter = vec;
                //AddDebugSphere(FromVispScreenToWorld(m_tag_corners[0], m_tag_corners[1]));
                //AddDebugSphere(FromVispScreenToWorld(m_tag_corners[2], m_tag_corners[3]));
                //AddDebugSphere(FromVispScreenToWorld(m_tag_corners[4], m_tag_corners[5]));
                //AddDebugSphere(FromVispScreenToWorld(m_tag_corners[6], m_tag_corners[7]));

                //AddDebugSphere(FromVispScreenToWorld(m_tag_corners[i * 8 + 1], m_tag_corners[i * 8 + 0]));
                //AddDebugSphere(FromVispScreenToWorld(m_tag_corners[i * 8 + 3], m_tag_corners[i * 8 + 2]));
                //AddDebugSphere(FromVispScreenToWorld(m_tag_corners[i * 8 + 5], m_tag_corners[i * 8 + 4]));
                //AddDebugSphere(FromVispScreenToWorld(m_tag_corners[i * 8 + 7], m_tag_corners[i * 8 + 6]));
                //AddDebugSphere(FromVispScreenToWorld(m_tag_cog[i * 2 + 0], m_tag_cog[i * 2 + 1]));

                //AddDebugPlane(
                //    FromVispScreenToWorld(m_tag_corners[i * 8 + 1], m_tag_corners[i * 8 + 0]),
                //    FromVispScreenToWorld(m_tag_corners[i * 8 + 3], m_tag_corners[i * 8 + 2]),
                //    FromVispScreenToWorld(m_tag_corners[i * 8 + 5], m_tag_corners[i * 8 + 4])
                //    );

                m_cube.SetActive(true);
                m_cube_pivot.SetActive(true);

                m_cube_pivot.transform.position = vec;
                //kinectRepresentation.transform.position = -vec;

                float max_dim = System.Math.Max(m_tag_length[i * 6 + 4], m_tag_length[i * 6 + 5]);
                float scale = vec.z / kinectRGBTexture.height * max_dim / (float)Math.Sqrt(2);

                m_cube.transform.localPosition = new Vector3(0.5f * scale, 0, 0);
                m_cube.transform.localScale = new Vector3(scale, scale, scale);

                if(firstForward == Vector3.zero)
                    firstForward = FromVispScreenToWorld(m_tag_corners[i * 8 + 3], m_tag_corners[i * 8 + 2]) - FromVispScreenToWorld(m_tag_corners[i * 8 + 1], m_tag_corners[i * 8 + 0]);

                Vector3 forward;
                forward = FromVispScreenToWorld(m_tag_corners[i * 8 + 3], m_tag_corners[i * 8 + 2]) - FromVispScreenToWorld(m_tag_corners[i * 8 + 1], m_tag_corners[i * 8 + 0]);
                //forward.x = m_tag_cMo[0];
                //forward.y = m_tag_cMo[4];
                //forward.z = m_tag_cMo[8];
                Vector3 upwards;
                upwards = FromVispScreenToWorld(m_tag_corners[i * 8 + 5], m_tag_corners[i * 8 + 4]) - FromVispScreenToWorld(m_tag_corners[i * 8 + 1], m_tag_corners[i * 8 + 0]);
                //upwards.x = m_tag_cMo[1];
                //upwards.y = m_tag_cMo[5];
                //upwards.z = m_tag_cMo[9];

                Vector3 ang1 = FromVispScreenToWorld(m_tag_corners[i * 8 + 1], m_tag_corners[i * 8 + 0]);
                Vector3 ang2 = FromVispScreenToWorld(m_tag_corners[i * 8 + 3], m_tag_corners[i * 8 + 2]);
                Vector3 ang3 = FromVispScreenToWorld(m_tag_corners[i * 8 + 5], m_tag_corners[i * 8 + 4]);
                Vector3 ang4 = FromVispScreenToWorld(m_tag_corners[i * 8 + 7], m_tag_corners[i * 8 + 6]);

                pts.Add(ang1);
                pts.Add(ang2);
                pts.Add(ang3);
                pts.Add(ang4);
                pts.Add(FromVispScreenToWorld(m_tag_cog[i * 2 + 0], m_tag_cog[i * 2 + 1]));

                listForward.Add(ang2 - ang1);
                listUpward.Add(ang4 - ang1);

                listForward.Add(ang3 - ang2);
                listUpward.Add(ang1 - ang2);

                listForward.Add(ang4 - ang3);
                listUpward.Add(ang2 - ang3);

                listForward.Add(ang1 - ang4);
                listUpward.Add(ang3 - ang4);

                m_cube_pivot.transform.rotation = Quaternion.LookRotation(forward, upwards);
                //kinectRepresentation.transform.rotation = /*Quaternion.Inverse(Quaternion.Euler(90,0,-90)) */ Quaternion.Inverse(Quaternion.LookRotation(Vector3.Cross(forward, upwards), forward));





                kinectRepresentation.transform.position = -(Quaternion.Inverse(zeroScene.transform.rotation) * vec);
                kinectRepresentation.transform.rotation = Quaternion.Inverse(zeroScene.transform.rotation);

                m_cube.transform.position = Vector3.zero;
                m_cube_pivot.transform.position = Vector3.zero;
                m_cube.transform.rotation = Quaternion.identity;
                m_cube_pivot.transform.rotation = Quaternion.identity;


            }

            for (int i = 0; i < pts.Count; ++i)
            {
                pts_array[3 * i] = pts[i].x;
                pts_array[3 * i + 1] = pts[i].y;
                pts_array[3 * i + 2] = pts[i].z;
            }

            ExtendedVisp_PlaneFitting_Process(pts.Count, pts_array, result_centroid, result_normal);

            Vector3 centroid = new Vector3(result_centroid[0], result_centroid[1], result_centroid[2]);
            Vector3 normal = new Vector3(result_normal[0], result_normal[1], result_normal[2]);

            Debug.Log("centroid : " + centroid);
            Debug.Log("normal : " + normal);

            //GameObject planeGO = GameObject.CreatePrimitive(PrimitiveType.Plane);
            //planeGO.transform.position = centroid;
            //planeGO.transform.rotation = Quaternion.FromToRotation(Vector3.up, normal);


            //Debug.Log("distance between centers : " + (pts[4] - pts[9]).magnitude);

        }
        else
        {
            m_cube.SetActive(false);
            m_cube_pivot.SetActive(false);
        }


    }

    void OnApplicationQuit()
    {
        Visp_WrapperFreeMemory();
        Debug.Log("Application ending after " + Time.time + " seconds");
    }

    Texture2D RemoveDistortion()
    {
        int newWidth = (int)kinectRGBTexture.width / downsizingRatio;
        int newHeight = (int)kinectRGBTexture.height / downsizingRatio;


        // remove distortion
        Texture2D result = new Texture2D(newWidth, newHeight);
        int u_0 = newWidth / 2;
        int v_0 = newHeight / 2;

        Color toFill = new Color(0,0,0,0);
        for (int i = 0; i < newWidth; ++i)
            for (int j = 0; j < newHeight; ++j)
                result.SetPixel(i, j, toFill);


        for (int i = 0; i < newWidth; ++i)
        {
            for (int j = 0; j < newHeight; ++j)
            {
                Vector2 ij = AffineDistortion(i - u_0, j - v_0, i - u_0, j - v_0, 10);
                ij.x += u_0;
                ij.y += v_0;
                if (ij.x < 0) { ij.x = 0; }
                if (ij.y < 0) { ij.y = 0; }
                if (ij.x >= newWidth) { ij.x = newWidth - 1; }
                if (ij.y >= newHeight) { ij.y = newHeight - 1; }
                result.SetPixel((int)ij.x, (int)ij.y, _texture2D.GetPixel(i, j));
            }
        }

        // fill the blanks
        for (int i = 1; i < newWidth-1; ++i)
        {
            for (int j = 1; j < newHeight-1; ++j)
            {
                if (result.GetPixel(i, j) == toFill)
                {
                    float r = 0;
                    float g = 0;
                    float b = 0;

                    int size = 3;
                    for (int x = -size; x <= size; x++)
                    {
                        for(int y = -size; y <= size; y++)
                        {
                            if (x == 0 && y == 0) continue;
                            Color next = result.GetPixel(i + x, j + y);
                            r += next.r; 
                            g += next.g; 
                            b += next.b;
                        }
                    }

                    result.SetPixel(i, j, new Color(r / (4* size * size - 1), g / (4*size * size - 1), b / (4*size * size - 1)));
                }
            }
        }

        result.Apply();

        return result;
    }

    // fast gross optimization
    Vector2 AffineDistortion(int x, int y, int corrected_x, int corrected_y, int iterations)
    {
        int inputCorrected_x = corrected_x;
        int inputCorrected_y = corrected_y;

        float K1 = 0.000001f * k1;
        float K2 = 0.000001f * 0.000001f * k2;
        float K3 = 0.000001f * 0.000001f * 0.000001f * k3;

        float r2 = corrected_x * corrected_x + corrected_y * corrected_y;
        float r4 = r2 * r2;
        float r6 = r2 * r2 * r2;
        corrected_x = Mathf.RoundToInt(x * (1 + K1 * r2 + K2 * r4 + K3 * r6));

        r2 = corrected_x * corrected_x + corrected_y * corrected_y;
        r4 = r2 * r2;
        r6 = r2 * r2 * r2;
        corrected_y = Mathf.RoundToInt(y * (1 + K1 * r2 + K2 * r4 + K3 * r6));

        if(inputCorrected_x == corrected_x && inputCorrected_y == corrected_y)
            return new Vector2(corrected_x, corrected_y);

        if (iterations > 0)
            return AffineDistortion(x, y, corrected_x, corrected_y, iterations - 1);
        else
            return new Vector2(corrected_x, corrected_y);
    }

    
}
