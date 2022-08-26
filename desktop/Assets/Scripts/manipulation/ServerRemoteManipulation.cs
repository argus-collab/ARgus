using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerRemoteManipulation : MonoBehaviour
{
    public WebRTCNetworkCommunication webrtcNetwork;
    public CustomServerNetworkManager unetNetwork;
    /*
    void Update()
    {
        List<WebRTCMessage> messages = webrtcNetwork.IsThereAnyMessagesForMe("ServerRemoteManipulation");
        for (int i = 0; i < messages.Count; ++i)
        {
            if (messages[i].action == "stick")
                AddSticker(messages[i]);

            if (messages[i].action == "draw")
                DrawSketch(messages[i]);
        }
    }

    void AddSticker(WebRTCMessage msg)
    {
        WebRTCMessageDataGameObject msgPrecise = (WebRTCMessageDataGameObject)msg;
        if (msgPrecise.name != "null")
        {
            GameObject go = GameObject.Find(msgPrecise.name);
            if (go != null)
            {
                GameObject sticker = Resources.Load("Sticker") as GameObject;
                GameObject addable = Instantiate(sticker);
                addable.name = sticker.name;

                Transform stickerRootTransform = go.transform.Find("stickers");
                if (stickerRootTransform == null)
                {
                    GameObject stickersRootGO = new GameObject("stickers");
                    stickersRootGO.transform.parent = go.transform;
                    stickerRootTransform = stickersRootGO.transform;
                    stickerRootTransform.transform.localPosition = Vector3.zero;
                    stickerRootTransform.transform.localRotation = Quaternion.identity;
                    stickerRootTransform.transform.localScale = new Vector3(1, 1, 1);
                }

                addable.transform.parent = stickerRootTransform.transform;
                addable.transform.localPosition = msgPrecise.position;
                addable.transform.localRotation = msgPrecise.rotation;
                addable.transform.localScale = msgPrecise.scale;

                unetNetwork.AddSticker(msgPrecise.name, msgPrecise.position, msgPrecise.rotation);
            }
            else
                Debug.Log("no object to stick on");
        }
        else
            Debug.Log("invalid message");

    }

    void DrawSketch(WebRTCMessage msg)
    {
        WebRTCMessageDataSketch msgPrecise = (WebRTCMessageDataSketch) msg;
        if (msgPrecise.name != "null")
        {
            GameObject go = GameObject.Find(msgPrecise.name);
            if (go != null)
            {
                GameObject addable = new GameObject("sketch-part");

                LineRenderer curLine = addable.AddComponent<LineRenderer>();
                curLine.startWidth = msgPrecise.startWidth;
                curLine.endWidth = msgPrecise.endWidth;
                curLine.material = new Material(Shader.Find("Diffuse"));
                curLine.useWorldSpace = false;

                Transform sketchRootTransform = go.transform.Find("sketch");
                if (sketchRootTransform == null)
                {
                    GameObject stickersRootGO = new GameObject("sketch");
                    stickersRootGO.transform.parent = go.transform;
                    sketchRootTransform = stickersRootGO.transform;
                    sketchRootTransform.transform.localPosition = Vector3.zero;
                    sketchRootTransform.transform.localRotation = Quaternion.identity;
                    sketchRootTransform.transform.localScale = new Vector3(1, 1, 1);

                }

                addable.transform.parent = sketchRootTransform.transform;
                addable.transform.localPosition = msgPrecise.position;
                addable.transform.localRotation = msgPrecise.rotation;
                addable.transform.localScale = msgPrecise.scale;

                curLine.positionCount = msgPrecise.size;
                curLine.SetPositions(msgPrecise.data);

                unetNetwork.AddSketch(msgPrecise.name, msgPrecise.position, msgPrecise.rotation, msgPrecise.scale,
                    msgPrecise.startWidth, msgPrecise.endWidth, msgPrecise.size, msgPrecise.data);

            }
            else
                Debug.Log("no object to stick on");
        }
        else
            Debug.Log("invalid message");
    }
    */
}
