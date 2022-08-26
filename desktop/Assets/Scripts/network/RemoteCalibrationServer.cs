using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Runtime.InteropServices;
using System;

public class RemoteCalibrationServer : MonoBehaviour
{
    private Matrix4x4 fromHolo2ToHolo1;

    private Vector3 pt11;
    private Vector3 pt12;
    private Vector3 pt13;
    private Vector3 pt14;

    private Vector3 pt21;
    private Vector3 pt22;
    private Vector3 pt23;
    private Vector3 pt24;

    private bool handlerRecorded;

    // DLL imports
    [DllImport("eigen_unity_api_64")]
    public static extern int test();
    [DllImport("eigen_unity_api_64")]
    public static extern int create_calibration_obj();
    [DllImport("eigen_unity_api_64")]
    public static extern void delete_calibration_obj(int calibration_obj);
    [DllImport("eigen_unity_api_64")]
    public static extern void add_calibration_pt_src_obj(int calibration_obj, float x, float y, float z);
    [DllImport("eigen_unity_api_64")]
    public static extern void delete_calibration_pt_src_obj(int calibration_obj);
    [DllImport("eigen_unity_api_64")]
    public static extern void add_calibration_pt_dest_obj(int calibration_obj, float x, float y, float z);
    [DllImport("eigen_unity_api_64")]
    public static extern void delete_calibration_pt_dest_obj(int calibration_obj);
    [DllImport("eigen_unity_api_64")]
    public static extern void compute_calibration(int calibration_obj);
    [DllImport("eigen_unity_api_64")]
    public static extern IntPtr get_calibration_result(int calibration_obj);



    void Update()
    {
        if(!handlerRecorded && NetworkServer.active)
        {
            NetworkServer.RegisterHandler((short)9979, OnAskForUmeyamaCalibration);
            handlerRecorded = true;
        }
    }

    void OnAskForUmeyamaCalibration(NetworkMessage netMsg)
    {
        InputUmeyamaCalibrationMessage msg = netMsg.ReadMessage<InputUmeyamaCalibrationMessage>();

        pt11 = msg.cs1_pt1;
        pt12 = msg.cs1_pt2;
        pt13 = msg.cs1_pt3;
        pt14 = msg.cs1_pt4;

        pt21 = msg.cs2_pt1;
        pt22 = msg.cs2_pt2;
        pt23 = msg.cs2_pt3;
        pt24 = msg.cs2_pt4;

        UmeyamaCalibration();
        SendCalibrationResult();
    }

    void UmeyamaCalibration()
    {
        float[] H_12 = new float[12]; // TODO : change rotation matrix to quaternion

        int calibration_obj = create_calibration_obj();

        add_calibration_pt_src_obj(calibration_obj, pt11.x, pt11.y, pt11.z);
        add_calibration_pt_src_obj(calibration_obj, pt12.x, pt12.y, pt12.z);
        add_calibration_pt_src_obj(calibration_obj, pt13.x, pt13.y, pt13.z);
        add_calibration_pt_src_obj(calibration_obj, pt14.x, pt14.y, pt14.z);

        add_calibration_pt_dest_obj(calibration_obj, pt21.x, pt21.y, pt21.z);
        add_calibration_pt_dest_obj(calibration_obj, pt22.x, pt22.y, pt22.z);
        add_calibration_pt_dest_obj(calibration_obj, pt23.x, pt23.y, pt23.z);
        add_calibration_pt_dest_obj(calibration_obj, pt24.x, pt24.y, pt24.z);

        compute_calibration(calibration_obj);

        IntPtr buf = get_calibration_result(calibration_obj);
        Marshal.Copy(buf, H_12, 0, H_12.Length);

        fromHolo2ToHolo1.SetRow(0, new Vector4(H_12[0], H_12[1], H_12[2], H_12[9]));
        fromHolo2ToHolo1.SetRow(1, new Vector4(H_12[3], H_12[4], H_12[5], H_12[10]));
        fromHolo2ToHolo1.SetRow(2, new Vector4(H_12[6], H_12[7], H_12[8], H_12[11]));
    }

    void SendCalibrationResult()
    {
        Quaternion q = Quaternion.LookRotation(
            fromHolo2ToHolo1.GetColumn(2),
            fromHolo2ToHolo1.GetColumn(1));

        Vector3 t;
        t.x = fromHolo2ToHolo1.m03;
        t.y = fromHolo2ToHolo1.m13;
        t.z = fromHolo2ToHolo1.m23;

        OutputUmeyamaCalibrationMessage msg = new OutputUmeyamaCalibrationMessage();
        msg.translation = t;
        msg.rotation = q;

        NetworkServer.SendToAll((short)9978, msg);
    }

}
