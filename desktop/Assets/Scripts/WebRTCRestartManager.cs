using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Microsoft.MixedReality.WebRTC.Unity;

public class WebRTCRestartManager : MonoBehaviour
{
    public PeerConnection webRTC;
    public ProcessLauncher dssFlusher;
    public WebRTCNetworkCommunication communication;
    public NodeDssSignaler signaler;

    private bool flushed = false;
    private bool startSession = false;

    [Header("UI")]
    public Button startSessionBtn;
    public Button stopSessionBtn;
    public Text playerCreationState;

    private void Start()
    {
        webRTC.enabled = false;
        signaler.enabled = false;
    }

    public void StartSessions()
    {
        startSession = true;
        dssFlusher.Launch();
        communication.CreatePlayer();
    }

    public void StopSession()
    {
        communication.RemovePlayer();

        if (webRTC.Peer != null && webRTC.Peer.IsConnected)
        {
            Debug.Log("clean webrtc peer");
            webRTC.Peer.Close();
            webRTC.Peer.Dispose();
        }
        webRTC.enabled = false;
        signaler.enabled = false;
    }

    void Update()
    {
        if (startSession && dssFlusher.HasProcessExited())
        {
            startSession = false;

            webRTC.enabled = true;
            signaler.enabled = true;
        }


        //if (communication.IsPlayerInstancied())
        //{
        //    if (!flushed)
        //    {
        //        dssFlusher.Launch();
        //        flushed = true;
        //    }
        //    webRTC.enabled = true;
        //}
        //else
        //{
        //    if(webRTC.Peer != null && webRTC.Peer.IsConnected)
        //    {
        //        Debug.Log("clean webrtc peer");
        //        webRTC.Peer.Close();
        //        webRTC.Peer.Dispose();
        //    }
        //    webRTC.enabled = false;
        //    flushed = false;
        //}

        UpdateUI();
    }

    void UpdateUI()
    {
        if (startSession)
        {
            startSessionBtn.interactable = false;
            stopSessionBtn.interactable = false;
            playerCreationState.text = "connecting...";
        }
        else if (communication.IsPlayerInstancied())
        {
            startSessionBtn.interactable = false;
            stopSessionBtn.interactable = true;
            playerCreationState.text = "the participant can connect";
        }
        
        else
        {
            startSessionBtn.interactable = true;
            stopSessionBtn.interactable = false;
            playerCreationState.text = "create player before participant launch application";

        }
    }
}
