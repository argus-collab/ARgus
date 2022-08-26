using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class RemoteCalibrationClient : MonoBehaviour
{
    public GameObject toCalibrate;

    public void AskForUmeyamaCalibration(
            Vector3 cs1_pt1,
            Vector3 cs1_pt2,
            Vector3 cs1_pt3,
            Vector3 cs1_pt4,
            Vector3 cs2_pt1,
            Vector3 cs2_pt2,
            Vector3 cs2_pt3,
            Vector3 cs2_pt4)
    {
        if (!NetworkManager.singleton.client.isConnected) return;

        NetworkManager.singleton.client.RegisterHandler((short)9978, OnReceivedUmeyamaCalibrationOuput);

        InputUmeyamaCalibrationMessage newMsg = new InputUmeyamaCalibrationMessage();

        newMsg.cs1_pt1 = cs1_pt1;
        newMsg.cs1_pt2 = cs1_pt2;
        newMsg.cs1_pt3 = cs1_pt3;
        newMsg.cs1_pt4 = cs1_pt4;
        
        newMsg.cs2_pt1 = cs2_pt1;
        newMsg.cs2_pt2 = cs2_pt2;
        newMsg.cs2_pt3 = cs2_pt3;
        newMsg.cs2_pt4 = cs2_pt4;

        NetworkManager.singleton.client.Send((short)9979, newMsg);
    }
    

    void OnReceivedUmeyamaCalibrationOuput(NetworkMessage netMsg)
    {
        OutputUmeyamaCalibrationMessage msg = netMsg.ReadMessage<OutputUmeyamaCalibrationMessage>();

        toCalibrate.transform.rotation = msg.rotation * toCalibrate.transform.rotation;
        toCalibrate.transform.position = msg.rotation * toCalibrate.transform.position + msg.translation;
    }
}
