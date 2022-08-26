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
    public UserDataInput userData;
    public List<GameObject> views;
    public List<GameObject> desactivateOnNextStep;
    public int indexLastTuto;
    public Text tutoIndicator;
    public int minIndexPrevious = 1;

    private bool peerStartedFromHere = false;
    public PeerStart hololensPeerStart;
    public PeerStart kinectPeerStart;

    public GameObject notLoadedView;
    public GameObject hololensPlayer;
    public GameObject kinectPlayer;

    public maxCamera navigator;
    public ChangeViewCinemachine viewManager;

    public Slider sliderYXSpeed;
    public Slider sliderPanSpeed;
    public Slider sliderZoomSpeed;
    public Slider sliderTranslationTrigger;
    public Slider sliderTransitionSpeed;

    int indexViews = 0;
    bool startApp = false;
    bool tutoDone = false;

    public bool IsTutoDone()
    {
        return tutoDone;
    }

    private void Start()
    {
        HideAll();
        notLoadedView.SetActive(true);
    }

    private bool IsLoaded()
    {
        bool res = (hololensPlayer != null) && (kinectPlayer != null);
        return res && webrtc.IsConnected() && (NetworkServer.active || manager.IsClientConnected());
    }

    private void ShutLoadingView()
    {
        notLoadedView.GetComponent<FadeRawImage>().HideAndDesactivate();
        notLoadedView.GetComponentInChildren<FadeImage>().HideAndDesactivate();
        notLoadedView.GetComponentInChildren<FadeText>().HideAndDesactivate();

        hololensPeerStart.Restart();
        kinectPeerStart.Restart();
        peerStartedFromHere = true;
    }

    private void Update()
    {

        if (!startApp && IsLoaded())
        {
            startApp = true;
            ShutLoadingView();
            if (ImportUserConfig())
            {
                indexViews = views.Count;
                tutoIndicator.gameObject.SetActive(false);
                tutoDone = true;
                userData.SetTutoDone();
            }
            UpdateView();
        }
    }

    void HideAll()
    {
        for (int i = 0; i < views.Count; ++i)
            views[i].SetActive(false);
    }

    public void SetTutoDone()
    {
        for (int i = 0; i < desactivateOnNextStep.Count; ++i)
            desactivateOnNextStep[i].SetActive(false);

        indexViews = views.Count;
        UpdateView();

        tutoIndicator.gameObject.SetActive(false);
        tutoDone = true;
        userData.SetTutoDone();
        ExportUserConfig();
    }

    void UpdateView()
    {
        if (!tutoDone && indexViews < views.Count)
            tutoIndicator.text = (indexViews + 1) + " / " + views.Count;


        //if (!tutoDone && indexViews > indexLastTuto)
        //{
        //    SetTutoDone();
        //}

        for (int i = 0; i < views.Count; ++i)
        {
            if (i == indexViews)
                views[i].SetActive(true);
            else
            {
                //FadeRawImage fade = views[i].GetComponent<FadeRawImage>();
                //if (fade != null)
                //    fade.HideAndDesactivate();
                //else
                    views[i].SetActive(false);
            }

        }
    }

    public void GoNextView()
    {
        indexViews++;

        for (int i = 0; i < desactivateOnNextStep.Count; ++i)
            desactivateOnNextStep[i].SetActive(false);

        UpdateView();
    }

    public void GoPreviousView()
    {
        indexViews = indexViews > minIndexPrevious ? indexViews - 1 : minIndexPrevious;
        
        for (int i = 0; i < desactivateOnNextStep.Count; ++i)
            desactivateOnNextStep[i].SetActive(false);

        UpdateView();
    }

    void ExportUserConfig()
    {
        string path = Application.dataPath + "/userConfig.txt";

        StreamWriter writer = new StreamWriter(path, false);

        writer.WriteLine(userData.GetUserId());
        writer.WriteLine(userData.GetUserName());
        if (tutoDone)
            writer.WriteLine("tuto done");
        else
            writer.WriteLine("tuto not done");

        writer.WriteLine(navigator.xSpeed);
        writer.WriteLine(navigator.panSpeed);
        writer.WriteLine(navigator.zoomRate);
        writer.WriteLine(navigator.translationTriggerOffset);

        writer.WriteLine(viewManager.transitionSpeed);

        writer.Close();
    }

    bool ImportUserConfig()
    {
        string path = Application.dataPath + "/userConfig.txt";

        try
        {
            StreamReader reader = new StreamReader(path);
            int id = Int32.Parse(reader.ReadLine());
            string name = reader.ReadLine();
            string tuto = reader.ReadLine();

            float yxSpeed = float.Parse(reader.ReadLine());
            float panSpeed = float.Parse(reader.ReadLine());
            float zoomSpeed = float.Parse(reader.ReadLine());
            float translationTrigger = float.Parse(reader.ReadLine());
            float transitionSpeed = float.Parse(reader.ReadLine());

            navigator.UpdateYXSpeed(yxSpeed); 
            navigator.UpdatePanSpeed(panSpeed); 
            navigator.UpdateZoomSpeed(zoomSpeed); 
            navigator.UpdateTranslationTrigger(translationTrigger);

            viewManager.UpdateTransitionSpeed(transitionSpeed);

            sliderYXSpeed.value = yxSpeed;
            sliderPanSpeed.value = panSpeed;
            sliderZoomSpeed.value = zoomSpeed;
            sliderTranslationTrigger.value = translationTrigger;
            sliderTransitionSpeed.value = transitionSpeed;

            reader.Close();

            userData.SetUser(id, name, (tuto == "tuto done"));
            Debug.Log("load user : " + id + ", " + name);

            return true;
        }
        catch(System.Exception e)
        {
            // no config files
            return false;
        }
    }
}
#endif