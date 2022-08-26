#if !UNITY_WSA
using System.Collections;
using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class UIManager : MonoBehaviour
{
    public CustomClientNetworkManager manager;
    public WebRTCNetworkCommunication webrtc;

    public PeerStart hololensPeerStart;
    public PeerStart kinectPeerStart;

    public GameObject notLoadedView;
    public GameObject firstView;
    public List<GameObject> firstViews;

    public UISettingsManager settingsManager;
    private int indexViews = 0;
    public Text tutoIndicator;


    bool startApp = false;

    private void Start()
    {
        notLoadedView.SetActive(true);

        settingsManager.ImportUserConfig();
    }

    private bool IsLoaded()
    {
        return webrtc.IsConnected();
    }

    private void ShutLoadingView()
    {
        notLoadedView.GetComponent<FadeRawImage>().HideAndDesactivate();
        notLoadedView.GetComponentInChildren<FadeImage>().HideAndDesactivate();
        notLoadedView.GetComponentInChildren<FadeText>().HideAndDesactivate();

        hololensPeerStart.Restart();
        kinectPeerStart.Restart();
    }

    private void Update()
    {
        if (!startApp && IsLoaded())
        {
            startApp = true;
            ShutLoadingView();
            if (settingsManager.userData.GetUserId() == -1)
                firstView.SetActive(true);
        }
    }

    public void HideFirstView()
    {
        firstView.SetActive(false);
        StartViews();
    }

    public void StartViews()
    {
        indexViews = 0;
        UpdateView();
    }

    public void HideAll()
    {
        for (int i = 0; i < firstViews.Count; ++i)
            firstViews[i].SetActive(false);
    }

    void UpdateView()
    {
        if (indexViews < firstViews.Count)
            tutoIndicator.text = (indexViews + 1) + " / " + firstViews.Count;
        else
            tutoIndicator.text = "";

        for (int i = 0; i < firstViews.Count; ++i)
        {
            if (i == indexViews)
                firstViews[i].SetActive(true);
            else
                firstViews[i].SetActive(false);
        }
    }

    public void GoNextView()
    {
        indexViews++;
        UpdateView();
    }

    public void GoPreviousView()
    {
        indexViews = indexViews > 0 ? indexViews - 1 : 0;
        UpdateView();
    }
}
#endif