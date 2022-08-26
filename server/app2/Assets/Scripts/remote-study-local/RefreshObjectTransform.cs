using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class RefreshObjectTransform : NetworkBehaviour
{
    public GameObject sceneRoot;
    public float freqUpdate = 10f;

    private List<Vector3> initialPositions;
    private List<Quaternion> initialRotations;
    private List<float> sendIntervals;

    private Vector3 posArbitrary = new Vector3(1f,1f,1f);
    private Quaternion rotArbitrary = Quaternion.Euler(45,45,45);

    private float timeStamp;
    private bool doRefresh = false;
    private bool moved = false;

    private void Start()
    {
        initialPositions = new List<Vector3>();
        initialRotations = new List<Quaternion>();
        sendIntervals = new List<float>();
    }

    void OnGUI()
    {
        if(isServer)
        {
            if (GUI.Button(new Rect(500, 50, 100, 30), "Refresh"))
                Refresh();
        }
    }

    void RecordInitialTransform()
    {
        for (int i = 0; i < sceneRoot.transform.childCount; ++i)
        {
            initialPositions.Add(sceneRoot.transform.GetChild(i).position);
            initialRotations.Add(sceneRoot.transform.GetChild(i).rotation);
        }
    }

    void AlterTransform()
    {
        for (int i = 0; i < sceneRoot.transform.childCount; ++i)
        {
            sceneRoot.transform.GetChild(i).position = posArbitrary;
            sceneRoot.transform.GetChild(i).rotation = rotArbitrary;
        }
    }

    void ResetToInitialTransform()
    {
        for (int i = 0; i < sceneRoot.transform.childCount; ++i)
        {
            sceneRoot.transform.GetChild(i).position = initialPositions[i];
            sceneRoot.transform.GetChild(i).rotation = initialRotations[i];
        }
    }

    void ForceNetworkUpdate()
    {
        sendIntervals.Clear();
        for (int i = 0; i < sceneRoot.transform.childCount; ++i)
        {
            NetworkTransform netTrans = sceneRoot.transform.GetChild(i).GetComponent<NetworkTransform>();
            sendIntervals.Add(netTrans.sendInterval);
            netTrans.sendInterval = 0.0f;
        }
    }

    void ResetNetworkUpdate()
    {
        for (int i = 0; i < sceneRoot.transform.childCount; ++i)
        {
            NetworkTransform netTrans = sceneRoot.transform.GetChild(i).GetComponent<NetworkTransform>();
            netTrans.sendInterval = sendIntervals[i];
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(doRefresh)
        {
            if(!moved)
            {
                if (Time.time - timeStamp > 1 / freqUpdate)
                {
                    AlterTransform();

                    moved = true;
                    timeStamp = Time.time;

                    Debug.Log("moved");
                }
            }
            else
            {
                if (Time.time - timeStamp > 1 / freqUpdate)
                {
                    ResetToInitialTransform();

                    initialPositions.Clear();
                    initialRotations.Clear();

                    ResetNetworkUpdate();

                    doRefresh = false;
                    moved = false;

                    Debug.Log("replaced");

                }
            }
        }
    }

    public void Refresh()
    {
        RecordInitialTransform();

        timeStamp = Time.time;
        doRefresh = true;

        ForceNetworkUpdate();

        RpcRefresh();
    }

    [ClientRpc]
    void RpcRefresh()
    {
        RecordInitialTransform();

        timeStamp = Time.time;
        doRefresh = true;

        ForceNetworkUpdate();
    }


}
