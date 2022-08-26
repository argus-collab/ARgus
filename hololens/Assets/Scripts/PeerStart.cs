using Microsoft.MixedReality.WebRTC.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PeerStart : MonoBehaviour
{
    //public GameObject players;
    //private int nbPlayers = 0;

    public PeerConnection peer;
    public int durationBeforeStart = 3;

    private bool isStarted = false;
    private float initialTimeStamp;

    public bool restart = false;

    private void Start()
    {
        initialTimeStamp = Time.time;
    }

    void Update()
    {
        //if(players != null && players.transform.childCount != nbPlayers)
        //{
        //    nbPlayers = players.transform.childCount;
        //    peer.StartConnection();
        //    if (isServer)
        //        RpcStartPeer();
        //}

        if ((!isStarted && (Time.time - initialTimeStamp) > durationBeforeStart) || restart)
        {
            Debug.Log("StartConnection");
            //peer.StartConnection();
            peer.StartConnectionIgnoreError();
            isStarted = true;
            restart = false;
        }

        //if (!isStarted && Input.GetKeyDown(KeyCode.Tab) && peer)
        //{
        //    Debug.Log("StartConnection");
        //    peer.StartConnection();
        //    isStarted = true;
        //}
    }

    public void Restart()
    {
        restart = true;
    }

    public void RestartIn(int t)
    {
        isStarted = false;
        initialTimeStamp = Time.time;
        durationBeforeStart = t;
    }

    //[ClientRpc]
    //void RpcStartPeer()
    //{
    //    Debug.Log("StartConnection");
    //    peer.StartConnection();
    //    isStarted = true;
    //}
}
