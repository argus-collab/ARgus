using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkSyncScale : NetworkBehaviour
{
    public int syncFreq = 10;
    private float lastTimeStamp;

    // Start is called before the first frame update
    void Start()
    {
        lastTimeStamp = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if (1 / (Time.time - lastTimeStamp) < syncFreq)
        {
            lastTimeStamp = Time.time;
            if (isServer)
                RpcSyncScale(transform.localScale);
        }
    }

    [ClientRpc]
    void RpcSyncScale(Vector3 newScale)
    {
        transform.localScale = newScale;
    }
}
