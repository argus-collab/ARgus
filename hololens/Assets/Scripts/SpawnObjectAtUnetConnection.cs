using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SpawnObjectAtUnetConnection : MonoBehaviour
{
    public GameObject toSpawn;
    public GameObject root;
    public string remoteSceneName;
    public CustomServerNetworkManager serverUNET;
    //public UDPSceneManager serverUDP;

    private bool isSpawned = false;

    // Update is called once per frame
    void Update()
    {
        if(serverUNET.isNetworkActive && !isSpawned)
        {
            var spawned = Instantiate(toSpawn);
            if (root != null)
                spawned.transform.parent = root.transform;
            else
                spawned.transform.parent = transform;
            spawned.transform.name = toSpawn.name;
            spawned.transform.localPosition = Vector3.zero;
            spawned.transform.localRotation = Quaternion.identity;

            serverUNET.AddGameObjectInScenePart(
                toSpawn.name,
                toSpawn.name,
                Vector3.zero,
                Quaternion.identity,
                new Vector3(1, 1, 1),
                remoteSceneName,
                false);

            isSpawned = true;
        }
    }
}
