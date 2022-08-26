using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkSyncScenePart : MonoBehaviour
{
    public CustomClientNetworkManager network;
    public GameObject sceneRoot;
    public float freq = 30;
    public bool local = true;

    private float lastTimeStamp;

    private bool popedDebugScene = false;


    private void Start()
    {
        lastTimeStamp = Time.time;
    }

    void OnGUI()
    {
        //if (!popedDebugScene && GUI.Button(new Rect(10, 70, 50, 30), "pop debug scene"))
        //{
        //    network.SendCommandMessage("scene-manager-command", "pop villa_RDC_empty-white ok");
        //    popedDebugScene = true;
        //}
    }

    void UpdatePosition()
    {
        for (int i = 0; i < sceneRoot.transform.childCount; ++i)
        {
            Transform child = sceneRoot.transform.GetChild(i);

            // bypass unet network transform : transform are updated through network with this component
            NetworkTransform nt = child.GetComponent<NetworkTransform>();
            if (nt != null) nt.enabled = false;

            if (child.GetComponent<NetworkMaster>() != null)
            {
                if (local)
                    network.SendSceneGameObjectTransform(child.name, child.localPosition, child.localRotation, child.localScale, local);
                else
                    network.SendSceneGameObjectTransform(child.name, child.position, child.rotation, child.localScale, local);

            }
        }
    }

    void Update()
    {
        if (Time.time - lastTimeStamp > (1/freq))
        {
            lastTimeStamp = Time.time;

            UpdatePosition();
        }
    }

    public void CalibrateScene(string sceneName, Vector3 position, Quaternion rotation)
    {
        network.SendSceneGameObjectTransform(sceneName, position, rotation, new Vector3(1,1,1), false);
    }
}
