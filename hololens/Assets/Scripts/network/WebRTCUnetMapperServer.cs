using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class WebRTCUnetMapperServer : MonoBehaviour
{
    public WebRTCNetworkCommunication webrtc;
    public CustomServerNetworkManager unet;

    private float lastTimeStamp;

    private void Start()
    {
        webrtc.exceptions = unet.exceptions;
    }

    void Update()
    {
        float t = Time.time;

        if (t - lastTimeStamp > 1 / webrtc.freq)
        {
            OnReceivedSceneGameObjectMessage(webrtc.IsThereAnyMessagesForMe((short)9983));
            OnReceivedCommandMessage(webrtc.IsThereAnyMessagesForMe((short)9977));
            OnAskForPrefabInstantiate(webrtc.IsThereAnyMessagesForMe((short)9975));
            OnAskForPrefabSpawn(webrtc.IsThereAnyMessagesForMe((short)9974));
            OnAskForPlayerDataUpdate(webrtc.IsThereAnyMessagesForMe((short)9973));
            OnAskForLogEntry(webrtc.IsThereAnyMessagesForMe((short)9972));
            OnAskForSynchronize(webrtc.IsThereAnyMessagesForMe((short)9971));
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
    private void OnReceivedCommandMessage(List<WebRTCMessageGeneric> msg)
    {
        for (int i = 0; i < msg.Count; i++)
        {
            if (webrtc.displayLog)
                Debug.Log("OnReceivedCommandMessage");

            string[] data = webrtc.GetSplittedData(msg[i].data);

            CommandMessage unetMsg = new CommandMessage();
            unetMsg.command = data[0];
            unetMsg.args = data[1];

            unet.OnReceivedCommandMessage(unetMsg);
        }
    }
    private void OnAskForPrefabInstantiate(List<WebRTCMessageGeneric> msg)
    {
        for (int i = 0; i < msg.Count; i++)
        {
            if (webrtc.displayLog)
                Debug.Log("OnAskForPrefabInstantiate");

            string[] data = webrtc.GetSplittedData(msg[i].data);

            SpawnPrefabMessage unetMsg = new SpawnPrefabMessage();
            unetMsg.PrefabName = data[0];
            unetMsg.GoName = data[1];
            unetMsg.initialPosition = webrtc.StringToVector3(data[2]);
            unetMsg.initialRotation = webrtc.StringToQuaternion(data[3]);

            unet.OnAskForPrefabInstantiate(unetMsg);
        }
    }
    private void OnAskForPrefabSpawn(List<WebRTCMessageGeneric> msg)
    {
        for (int i = 0; i < msg.Count; i++)
        {
            if (webrtc.displayLog)
                Debug.Log("OnAskForPrefabSpawn");

            string[] data = webrtc.GetSplittedData(msg[i].data);

            SpawnPrefabMessage unetMsg = new SpawnPrefabMessage();
            unetMsg.PrefabName = data[0];
            unetMsg.GoName = data[1];
            unetMsg.initialPosition = webrtc.StringToVector3(data[2]);
            unetMsg.initialRotation = webrtc.StringToQuaternion(data[3]);

            unet.OnAskForPrefabSpawn(unetMsg);
        }
    }
    private void OnAskForPlayerDataUpdate(List<WebRTCMessageGeneric> msg)
    {
        for (int i = 0; i < msg.Count; i++)
        {
            if (webrtc.displayLog)
                Debug.Log("OnAskForPlayerDataUpdate");

            string[] data = webrtc.GetSplittedData(msg[i].data);

            UserDataMessage unetMsg = new UserDataMessage();
            unetMsg.playerName = data[0];
            unetMsg.cameraToFollow = data[1];
            unetMsg.indexRep = Int32.Parse(data[2]);

            unet.OnAskForPlayerDataUpdate(unetMsg, unet.networkAddress);
        }
    }
    private void OnAskForLogEntry(List<WebRTCMessageGeneric> msg)
    {
        for (int i = 0; i < msg.Count; i++)
        {
            if (webrtc.displayLog)
                Debug.Log("OnAskForLogEntry");

            string[] data = webrtc.GetSplittedData(msg[i].data);

            LogEntryMessage unetMsg = new LogEntryMessage();
            unetMsg.ip = data[0];
            unetMsg.logEntry = data[1];

            unet.OnAskForLogEntry(unetMsg);
        }
    }
    private void OnAskForSynchronize(List<WebRTCMessageGeneric> msg)
    {
        for (int i = 0; i < msg.Count; i++)
        {
            webrtc.Synchronize();
        }
    }


    public void WebRTCUpdateSyncObjectTransform(NetworkInstanceId id, Vector3 p, Quaternion q, bool isLocal)
    {
        if (webrtc.displayLog)
            Debug.Log("WebRTCUpdateSyncObjectTransform");

        string data = webrtc.PrepareData(new string[] {
            webrtc.NetworkInstanceIdToString(id),
            webrtc.Vector3ToString(p),
            webrtc.QuaternionToString(q),
            isLocal.ToString()
        });

        webrtc.SendMessage((short)9999, data);
    }
    public void WebRTCAddGameObjectInScenePart(string namePrefab, string nameGO, Vector3 localPos, Quaternion localRot, string remoteScene, bool isMaster)
    {
        if (webrtc.displayLog)
            Debug.Log("WebRTCAddGameObjectInScenePart");

        string data = webrtc.PrepareData(new string[] {
            nameGO,
            namePrefab,
            remoteScene,
            false.ToString(),
            webrtc.Vector3ToString(localPos),
            webrtc.QuaternionToString(localRot),
            isMaster.ToString()
        });

        webrtc.RecordInForAllHistory((short)9982, data, nameGO);
        webrtc.SendMessage((short)9982, data);
    }
    public void WebRTCAddGameObjectInScenePart(string namePrefab, string nameGO, Vector3 localPos, Quaternion localRot, Vector3 scale, string remoteScene, bool isMaster)
    {
        if (webrtc.displayLog)
            Debug.Log("WebRTCAddGameObjectInScenePart");

        string data = webrtc.PrepareData(new string[] {
            nameGO,
            namePrefab,
            remoteScene,
            true.ToString(),
            webrtc.Vector3ToString(localPos),
            webrtc.QuaternionToString(localRot),
            isMaster.ToString(),
            webrtc.Vector3ToString(scale)
        });

        webrtc.RecordInForAllHistory((short)9982, data, nameGO);
        webrtc.SendMessage((short)9982, data);
    }
    public void WebRTCUpdateGameObjectColorInScenePart(string nameGO, string color, bool inChildren = false)
    {
        if (webrtc.displayLog)
            Debug.Log("WebRTCUpdateGameObjectColorInScenePart");

        Material mat = Resources.Load(color, typeof(Material)) as Material;
        Color c;

        if (mat == null)
            c = Color.white;
        else
            c = mat.color;

        unet.ChangeColorOnGO(nameGO, inChildren, c);
        unet.ChangeColorOnGO(nameGO + unet.GONameAddition, inChildren, c);

        string data = webrtc.PrepareData(new string[] {
            nameGO,
            webrtc.ColorToString(c),
            inChildren.ToString(),
        });

        webrtc.SendMessage((short)9981, data);
    }
    public void WebRTCRemoveGameObjectInScenePart(string nameGO)
    {
        if (webrtc.displayLog)
            Debug.Log("WebRTCRemoveGameObjectInScenePart");

        GameObject go = GameObject.Find(nameGO);
        if (go != null)
            Destroy(go);
        else
            Debug.Log("GO not found");

        GameObject goCli = GameObject.Find(nameGO + unet.GONameAddition);
        if (goCli != null)
            Destroy(goCli);
        else
            Debug.Log("GO not found");

        webrtc.DeleteRecordHistory(nameGO);
        webrtc.SendMessage((short)9980, nameGO + unet.GONameAddition);

    }
    public void WebRTCUpdateComponentInScenePart(string GOName, string componentTypeName, string[] propertiesNames, GenericType[] propertiesValues)
    {
        if (webrtc.displayLog)
            Debug.Log("WebRTCUpdateComponentInScenePart");

        SceneGameObjectUpdateComponentMessage newMsg = new SceneGameObjectUpdateComponentMessage();

        newMsg.GOName = GOName;
        newMsg.componentTypeName = componentTypeName;
        newMsg.propertiesNames = propertiesNames;
        newMsg.propertiesValues = propertiesValues;

        string data = webrtc.PrepareData(new string[] {
            GOName,
            componentTypeName,
            webrtc.StringArrayToString(propertiesNames),
            webrtc.GenericTypeArrayToString(propertiesValues)
        });

        webrtc.SendMessage((short)9976, data);
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


}
