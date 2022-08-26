#if !UNITY_WSA
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;

public class UISettingsManager : MonoBehaviour
{
    public UserDataInput userData;
    private int userId;

    public maxCamera navigator;
    public ChangeViewCinemachine viewManager;

    public Slider sliderYXSpeed;
    public Slider sliderPanSpeed;
    public Slider sliderZoomSpeed;
    public Slider sliderTranslationTrigger;
    public Slider sliderTransitionSpeed;

    public UITutorialManager tutoMultiView;
    private bool tutoMultiViewState;
    public UITutorialManager tutoHololens;
    private bool tutoHololensState;

    private void Start()
    {
        tutoMultiViewState = tutoMultiView.IsTutoDone();
        tutoHololensState = tutoHololens.IsTutoDone();
        userId = userData.GetUserId();
    }

    private void Update()
    {
        if (tutoMultiViewState != tutoMultiView.IsTutoDone())
        {
            tutoMultiViewState = tutoMultiView.IsTutoDone();
            ExportUserConfig();
        }

        if (tutoHololensState != tutoHololens.IsTutoDone())
        {
            tutoHololensState = tutoHololens.IsTutoDone();
            ExportUserConfig();
        }

        if (userId != userData.GetUserId())
        {
            userId = userData.GetUserId();
            ExportUserConfig();
        }
    }

    public void ExportUserConfig()
    {
        string path = Application.dataPath + "/userConfig.txt";

        StreamWriter writer = new StreamWriter(path, false);

        writer.WriteLine(userData.GetUserId());
        writer.WriteLine(userData.GetUserName());

        if (tutoMultiView.IsTutoDone())
            writer.WriteLine("tuto multiview done");
        else
            writer.WriteLine("tuto multiview not done");

        if (tutoHololens.IsTutoDone())
            writer.WriteLine("tuto hololens done");
        else
            writer.WriteLine("tuto hololens not done");

        writer.WriteLine(navigator.xSpeed);
        writer.WriteLine(navigator.panSpeed);
        writer.WriteLine(navigator.zoomRate);
        writer.WriteLine(navigator.translationTriggerOffset);
        writer.WriteLine(viewManager.transitionSpeed);

        writer.Close();
    }

    public bool ImportUserConfig()
    {
        string path = Application.dataPath + "/userConfig.txt";

        try
        {
            StreamReader reader = new StreamReader(path);
            int id = Int32.Parse(reader.ReadLine());
            Debug.Log("loaded id : " + id);

            string name = reader.ReadLine();
            Debug.Log("loaded name : " + name);

            string tutoMultiViewVal = reader.ReadLine();
            string tutoHololensVal = reader.ReadLine();

            float yxSpeed = float.Parse(reader.ReadLine());
            Debug.Log("loaded yx speed : " + yxSpeed);

            float panSpeed = float.Parse(reader.ReadLine());
            Debug.Log("loaded pan speed : " + yxSpeed);

            float zoomSpeed = float.Parse(reader.ReadLine());
            Debug.Log("loaded zoom speed : " + yxSpeed);

            float translationTrigger = float.Parse(reader.ReadLine());
            Debug.Log("loaded translation trigger : " + yxSpeed);

            float transitionSpeed = float.Parse(reader.ReadLine());
            Debug.Log("loaded transition speed : " + yxSpeed);


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

            userData.SetUser(id, name, (tutoMultiViewVal == "tuto multiview done"), (tutoHololensVal == "tuto hololens done"));

            if (tutoMultiViewVal == "tuto multiview done")
                tutoMultiView.SetTutoDone();
            if (tutoHololensVal == "tuto hololens done")
                tutoHololens.SetTutoDone();

            Debug.Log("load user : " + id + ", " + name);

            return true;
        }
        catch (System.Exception e)
        {
            // no config files
            Debug.Log(e);
            return false;
        }
    }
}
#endif