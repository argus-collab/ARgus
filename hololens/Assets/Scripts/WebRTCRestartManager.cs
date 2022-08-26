using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.WebRTC.Unity;

public class WebRTCRestartManager : MonoBehaviour
{
    public PeerConnection webRTC;
    public GameObject player;
    public ProcessLauncher dssFlusher;

    private bool flushed = false;

    void Update()
    {
        if (player != null)
        {
            if (!flushed)
            {
                dssFlusher.Launch();
                flushed = true;
            }
            webRTC.enabled = true;
        }
        else
        {
            if(webRTC.Peer != null && webRTC.Peer.IsConnected)
            {
                Debug.Log("clean webrtc peer");
                webRTC.Peer.Close();
                webRTC.Peer.Dispose();
            }
            webRTC.enabled = false;
            flushed = false;
        }
    }
}
