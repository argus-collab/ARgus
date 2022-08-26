using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CreateAsPlayer : MonoBehaviour
{
    public GameObject toSpawn;
    public string namePlayer;
    public GameObject rootPlayers;
    //public string remoteSceneName;
    public CustomServerNetworkManager serverUNET;
    //public UDPSceneManager serverUDP;

    private bool isSpawned = false;

    // Update is called once per frame
    void Update()
    {
        if (serverUNET.isNetworkActive && !isSpawned)
        {
            var spawned = Instantiate(toSpawn);

            GameObject playerRoot = new GameObject(NetworkManager.singleton.networkAddress);
            playerRoot.transform.position = Vector3.zero;
            playerRoot.transform.rotation = Quaternion.identity;

            if (rootPlayers != null)
            {
                spawned.transform.parent = playerRoot.transform;
                playerRoot.transform.parent = rootPlayers.transform;
            }
            else
                spawned.transform.parent = transform;
            spawned.transform.name = namePlayer;
            spawned.transform.localPosition = Vector3.zero;
            spawned.transform.localRotation = Quaternion.identity;

            NetworkServer.Spawn(spawned);

            isSpawned = true;
        }
    }
}
