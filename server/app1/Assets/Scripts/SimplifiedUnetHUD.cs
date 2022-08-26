using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SimplifiedUnetHUD : MonoBehaviour
{
    public NetworkManager manager;
    public GameObject errorMessage;
    public GameObject connectionMessage;
    public WebRTCNetworkCommunication webrtc;

    public bool fullWebRTC = false;


    void Awake()
    {
        manager = GetComponent<NetworkManager>();
        //DisplayConnectionMessage();
    }

    void DisplayConnectionMessage()
    {
        errorMessage.SetActive(false);
        connectionMessage.SetActive(true);
    }

    void DisplayErrorMessage()
    {
        errorMessage.SetActive(true);
        connectionMessage.SetActive(false);
    }

    void DisplayApplication()
    {
        errorMessage.SetActive(false);

        connectionMessage.SetActive(false);
        //connectionMessage.GetComponent<FadeRawImage>().HideAndDesactivate();
        //connectionMessage.GetComponentInChildren<FadeImage>().HideAndDesactivate();
        //connectionMessage.GetComponentInChildren<FadeText>().HideAndDesactivate();
    }

    void Update()
    {

        if (fullWebRTC)
        {
            if (webrtc.IsConnected())
            {
                DisplayApplication();
            }
            else
            {
                DisplayConnectionMessage();
            }
        }
        else
        {
            bool noConnection = (manager.client == null || manager.client.connection == null ||
            manager.client.connection.connectionId == -1);

            if (webrtc.IsConnected() && (NetworkServer.active || manager.IsClientConnected()))
            {
                DisplayApplication();
            }

            else if (!manager.IsClientConnected() && !NetworkServer.active && manager.matchMaker == null && noConnection)
            {
                DisplayErrorMessage();
            }
            else
            {
                DisplayConnectionMessage();
            }
        }






        //if (manager.matchMaker != null && manager.matchInfo == null && manager.matches == null)
        //{
        //    Debug.Log("truc 2");

        //    DisplayErrorMessage();
        //}
    }
}

