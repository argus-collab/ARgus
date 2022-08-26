﻿#if !UNITY_WSA
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConditionHololens : MonoBehaviour, ICondition
{
    public ExperimentControllerBehaviour2 expController;

    private int _index = 0;
    public int Index  
    {
        get => _index;
        set => _index = value;
    }
    public ChangeViewCinemachine viewManager;
    public FreezeHololensView freezer;

    public maxCamera navigator;

    public StickCursorManager stick;
    public SphericalVisualization sphVisu;
    public DisplayViewInUI miniatures;

    public Button virtualViewBtn;
    public Button hololensViewBtn;
    public Button kinectViewBtn;

    public Button homeBtn;
    public Button helpBtn;
    public Button findBtn;
    public Button settingsBtn;

    public UIStateViewIndicator ui;


    public bool isApplied = false;

    void ICondition.ApplyCondition()
    {
        Debug.Log("condition hololens");

        isApplied = true;
        viewManager.DisplayARHeadsetView();

        UpdateCondition();

    }

    void ICondition.ResetCondition()
    {
        isApplied = false;
        //viewManager.DisplayVirtualView();

        ResetCondition();
    }

    void Update()
    {
        //if (isApplied)
        //    UpdateCondition();
    }

    void ResetCondition()
    {
        navigator.isActive = true;
        ui.isActive = true;
        viewManager.isMouseNavigationActive = true;

        stick.gameObject.SetActive(true);
        sphVisu.gameObject.SetActive(true);
        freezer.gameObject.SetActive(true);
        miniatures.gameObject.SetActive(true);

        virtualViewBtn.gameObject.SetActive(true);
        hololensViewBtn.gameObject.SetActive(true);
        kinectViewBtn.gameObject.SetActive(true);

        homeBtn.gameObject.SetActive(true);
        helpBtn.gameObject.SetActive(true);
        findBtn.gameObject.SetActive(true);
        settingsBtn.gameObject.SetActive(true);
    }

    void UpdateCondition()
    {

        navigator.isActive = false;
        ui.isActive = false;
        viewManager.isMouseNavigationActive = false;

        stick.gameObject.SetActive(false);
        sphVisu.gameObject.SetActive(false);
        freezer.gameObject.SetActive(false);
        miniatures.gameObject.SetActive(false);

        virtualViewBtn.gameObject.SetActive(false);
        hololensViewBtn.gameObject.SetActive(false);
        kinectViewBtn.gameObject.SetActive(false);

        homeBtn.gameObject.SetActive(false);
        helpBtn.gameObject.SetActive(false);
        findBtn.gameObject.SetActive(false);
        settingsBtn.gameObject.SetActive(false);

        expController.ResetTask();
    }
}
#endif