#if !UNITY_WSA
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConditionMultiView : MonoBehaviour, ICondition
{
    public ExperimentControllerBehaviour2 expController;

    private int _index = 1;
    public int Index
    {
        get => _index;
        set => _index = value;
    }
    public ChangeViewCinemachine viewManager;

    public maxCamera navigator;

    public StickCursorManager stick;
    public SphericalVisualization sphVisu;

    public Button virtualViewBtn;
    public Button hololensViewBtn;
    public Button kinectViewBtn;

    public Button homeBtn;
    public Button helpBtn;
    public Button findBtn;
    public Button settingsBtn;

    public UIStateViewIndicator ui;
    public SpotGenerator spots;
    public NetworkTimer timer;

    public bool isApplied = false;

    //private SymbolsRandomPlacement symbolsPlacer;

    void ICondition.ApplyCondition()
    {
        Debug.Log("condition multiview");
        SetMultiViewCondition();

        timer.StartTimer();

        //if (symbolsPlacer != null)
        //    symbolsPlacer.Initialize();
    }

    public void SetMultiViewCondition()
    {
        if (isApplied) return;

        isApplied = true;
        viewManager.ResetVirtualPosition();
        viewManager.DisplayVirtualView();

        UpdateCondition();

        spots.SetTuto(false);
        spots.Change();
    }

    void ICondition.ResetCondition()
    {
        isApplied = false;
        //viewManager.DisplayVirtualView();

        ResetCondition();
        timer.StopTimer();
    }

    void Update()
    {
        //if (isApplied)
        //    UpdateCondition();

        //if (symbolsPlacer == null)
        //    symbolsPlacer = GameObject.FindObjectOfType<SymbolsRandomPlacement>();
    }

    void ResetCondition()
    {
        navigator.isActive = true;
        ui.isActive = true;
        viewManager.isMouseNavigationActive = true;

        stick.gameObject.SetActive(true);
        sphVisu.gameObject.SetActive(true);

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
        navigator.isActive = true;
        ui.isActive = true;
        viewManager.isMouseNavigationActive = true;

        stick.gameObject.SetActive(true);
        sphVisu.gameObject.SetActive(true);

        virtualViewBtn.gameObject.SetActive(true);
        hololensViewBtn.gameObject.SetActive(true);
        kinectViewBtn.gameObject.SetActive(true);

        homeBtn.gameObject.SetActive(true);
        helpBtn.gameObject.SetActive(true);
        findBtn.gameObject.SetActive(true);
        settingsBtn.gameObject.SetActive(true);

        expController.ResetTask();
    }
}
#endif
