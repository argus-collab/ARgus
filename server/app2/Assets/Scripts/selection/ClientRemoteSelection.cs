using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientRemoteSelection : MonoBehaviour
{
    public WebRTCNetworkCommunication network;
    private GameObject grasped;
    /*
    void Update()
    {
        List<WebRTCMessage> messages = network.IsThereAnyMessagesForMe("ClientRemoteSelection");
        for (int i = 0; i < messages.Count; ++i)
        {
            if(messages[i].action == "instantiate")
                InstantiateLocalCopy(messages[i]);
        }
    }

    public void Select(float x, float y)
    {
        network.SendMessage("ServerRemoteSelection", "select", x, y);
    }

    public void Grasp()
    {
        network.SendMessage("ServerRemoteSelection", "grasp");
    }

    public void Release()
    {
        Destroy(grasped);
        grasped = null;
        network.SendMessage("ServerRemoteSelection", "release");
    }

    public void InstantiateLocalCopy(WebRTCMessage msg)
    {
        WebRTCMessageDataGameObject msgPrecise = (WebRTCMessageDataGameObject) msg;
        if (msgPrecise.name != "null")
        {
            GameObject graspedGO = Resources.Load(msgPrecise.name) as GameObject;
            grasped = Instantiate(graspedGO, msgPrecise.position, msgPrecise.rotation);
            grasped.name = graspedGO.name;
            grasped.transform.localScale = msgPrecise.scale;
        }
        else
            Debug.Log("no selected object");
    }

    public GameObject GetGrasped()
    {
        return grasped;
    }
    */
}
