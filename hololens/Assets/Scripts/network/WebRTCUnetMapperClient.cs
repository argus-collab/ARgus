using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

// TODO : simplify with C# reflection ?

public class WebRTCUnetMapperClient : MonoBehaviour
{
    public WebRTCNetworkCommunication webrtc;
    public CustomClientNetworkManager unet;

    private float lastTimeStamp;
    public bool isSynchronized;


    void Update()
    {
        if(!isSynchronized && webrtc.IsConnected())
        {
            try
            {
                WebRTCSynchronize();
                isSynchronized = true;
            }
            catch (Exception e) { }
        }

        float t = Time.time;

        if (t - lastTimeStamp > 1 / webrtc.freq)
        {
            lastTimeStamp = t;

            OnSyncObjectTransformUpdate(webrtc.IsThereAnyMessagesForMe((short)9999));

            OnAskForGameObjectInstanciateMessage(webrtc.IsThereAnyMessagesForMe((short)9982));
            OnAskForGameObjectChangeColorMessage(webrtc.IsThereAnyMessagesForMe((short)9981));
            OnAskForGameObjectRemoveMessage(webrtc.IsThereAnyMessagesForMe((short)9980));
            OnAskForGameObjectUpdateComponentMessage(webrtc.IsThereAnyMessagesForMe((short)9976));

            OnReceivedSceneGameObjectMessage(webrtc.IsThereAnyMessagesForMe((short)9983));
        }
    }


    private void OnSyncObjectTransformUpdate(List<WebRTCMessageGeneric> msg)
    {
        for (int i = 0; i < msg.Count; i++)
        {
            if (webrtc.displayLog)
                Debug.Log("OnSyncObjectTransformUpdate");

            string[] data = webrtc.GetSplittedData(msg[i].data);

            TransformMessage unetMsg = new TransformMessage();
            unetMsg.id = webrtc.StringToNetworkInstanceId(data[0]);
            unetMsg.p = webrtc.StringToVector3(data[1]);
            unetMsg.q = webrtc.StringToQuaternion(data[2]);
            unetMsg.isLocal = bool.Parse(data[3]);

            unet.OnSyncObjectTransformUpdate(unetMsg);
        }
    }
    
    private void OnAskForGameObjectInstanciateMessage(List<WebRTCMessageGeneric> msg)
    {
        for (int i = 0; i < msg.Count; i++)
        {
            if (webrtc.displayLog)
                Debug.Log("OnAskForGameObjectInstanciateMessage");

            string[] data = webrtc.GetSplittedData(msg[i].data);

            SceneGameObjectInstanciateMessage unetMsg = new SceneGameObjectInstanciateMessage();
            unetMsg.name = data[0];
            unetMsg.namePrefab = data[1];
            unetMsg.parent = data[2];
            unetMsg.isCustomScale = bool.Parse(data[3]);
            unetMsg.localPosition = webrtc.StringToVector3(data[4]);
            unetMsg.localRotation = webrtc.StringToQuaternion(data[5]);
            unetMsg.isMaster = bool.Parse(data[6]);

            if(unetMsg.isCustomScale)
                unetMsg.localScale = webrtc.StringToVector3(data[7]);

            //unetMsg.isCustomScale = bool.Parse(data[5]);
            //unetMsg.localScale = webrtc.StringToVector3(data[6]);

            unet.OnAskForGameObjectInstanciateMessage(unetMsg);
        }
    }
    private void OnAskForGameObjectChangeColorMessage(List<WebRTCMessageGeneric> msg)
    {
        for (int i = 0; i < msg.Count; i++)
        {
            if (webrtc.displayLog)
                Debug.Log("OnAskForGameObjectChangeColorMessage");

            string[] data = webrtc.GetSplittedData(msg[i].data);

            SceneGameObjectChangeColorMessage unetMsg = new SceneGameObjectChangeColorMessage();
            unetMsg.name = data[0];
            unetMsg.inChildren = bool.Parse(data[1]);
            unetMsg.color = webrtc.StringToColor(data[2]);

            unet.OnAskForGameObjectChangeColorMessage(unetMsg);
        }
    }
    private void OnAskForGameObjectRemoveMessage(List<WebRTCMessageGeneric> msg)
    {
        for (int i = 0; i < msg.Count; i++)
        {
            if (webrtc.displayLog)
                Debug.Log("OnAskForGameObjectRemoveMessage");

            string[] data = webrtc.GetSplittedData(msg[i].data);

            SceneGameObjectRemoveMessage unetMsg = new SceneGameObjectRemoveMessage();
            unetMsg.name = data[0];

            unet.OnAskForGameObjectRemoveMessage(unetMsg);
        }
    }
    private void OnAskForGameObjectUpdateComponentMessage(List<WebRTCMessageGeneric> msg)
    {
        for (int i = 0; i < msg.Count; i++)
        {
            if (webrtc.displayLog)
                Debug.Log("OnAskForGameObjectUpdateComponentMessage");

            string[] data = webrtc.GetSplittedData(msg[i].data);

            SceneGameObjectUpdateComponentMessage unetMsg = new SceneGameObjectUpdateComponentMessage();
            unetMsg.GOName = data[0];
            unetMsg.componentTypeName = data[1];
            unetMsg.propertiesNames = webrtc.StringToStringArray(data[2]);
            unetMsg.propertiesValues = webrtc.StringToGenericTypeArray(data[3]);

            unet.OnAskForGameObjectUpdateComponentMessage(unetMsg);
        }
    }

    private void OnReceivedSceneGameObjectMessage(List<WebRTCMessageGeneric> msg)
    {
        for (int i = 0; i < msg.Count; i++)
        {
            if (webrtc.displayLog)
                Debug.Log("OnReceivedSceneGameObjectMessage");

            string[] data = webrtc.GetSplittedData(msg[i].data);

            SceneGameObjectMessage unetMsg = new SceneGameObjectMessage();
            unetMsg.name = data[0];
            unetMsg.local = bool.Parse(data[1]);
            unetMsg.position = webrtc.StringToVector3(data[2]);
            unetMsg.rotation = webrtc.StringToQuaternion(data[3]);
            unetMsg.scale = webrtc.StringToVector3(data[4]);

            unet.OnReceivedSceneGameObjectMessage(unetMsg);
        }
    }

    public void WebRTCSendSceneGameObjectTransform(string name, Vector3 position, Quaternion rotation, Vector3 scale, bool local)
    {
        if (webrtc.displayLog)
            Debug.Log("WebRTCSendSceneGameObjectTransform");

        string data = webrtc.PrepareData(new string[] {
            name,
            local.ToString(),
            webrtc.Vector3ToString(position),
            webrtc.QuaternionToString(rotation),
            webrtc.Vector3ToString(scale)
        });

        webrtc.SendMessage((short)9983, data);
    }
    public void WebRTCSendCommandMessage(string command, string args = "")
    {
        if (webrtc.displayLog)
            Debug.Log("WebRTCSendCommandMessage");

        string data = webrtc.PrepareData(new string[] {
            command,
            args
        });

        webrtc.SendMessage((short)9977, data);
    }
    public void WebRTCAskForPrefabInstantiate(string prefabName, string goName, Vector3 initialPosition, Quaternion initialRotation)
    {
        if (webrtc.displayLog)
            Debug.Log("WebRTCAskForPrefabInstantiate");

        string data = webrtc.PrepareData(new string[] {
            prefabName,
            goName,
            webrtc.Vector3ToString(initialPosition),
            webrtc.QuaternionToString(initialRotation)
        });

        webrtc.SendMessage((short)9975, data);
    }
    public void WebRTCAskForPrefabSpawn(string prefabName, string goName, Vector3 initialPosition, Quaternion initialRotation)
    {
        if (webrtc.displayLog)
            Debug.Log("WebRTCAskForPrefabSpawn");

        string data = webrtc.PrepareData(new string[] {
            prefabName,
            goName,
            webrtc.Vector3ToString(initialPosition),
            webrtc.QuaternionToString(initialRotation)
        });

        webrtc.SendMessage((short)9974, data);
    }

    public void WebRTCAskForPlayerDataUpdate(string playerName, int indexRep, string cameraToFollow)
    {
        if (webrtc.displayLog)
            Debug.Log("WebRTCAskForPlayerDataUpdate");

        string data = webrtc.PrepareData(new string[] {
            playerName,
            cameraToFollow,
            indexRep.ToString()
        });

        webrtc.SendMessage((short)9973, data);
    }
    public void WebRTCAskForLogEntry(string logEntry)
    {
        if (webrtc.displayLog)
            Debug.Log("WebRTCAskForLogEntry");

        string data = webrtc.PrepareData(new string[] {
            NetworkManager.singleton.client.connection.address,
            logEntry
        });

        webrtc.SendMessage((short)9972, data);
    }

    public void WebRTCSynchronize()
    {
        webrtc.AskForSynchronization();
        //webrtc.RecreatePeerConnectionIn(2);
    }
}
