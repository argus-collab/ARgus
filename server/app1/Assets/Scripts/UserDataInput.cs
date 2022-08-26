using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class UserDataInput : MonoBehaviour
{
    public CustomClientNetworkManager manager;
    public GameObject userDataUI;
    public GameObject mainCamera;
    
    public TMPro.TMP_InputField inputName;
    public TMPro.TMP_InputField inputId;
    private bool dataSet = false;

    private int id = -1;
    private string name;
    private bool hasDoneTutoMultiview = false;
    private bool hasDoneTutoHololens = false;

    //private void Start()
    //{
    //    id = -1;
    //}

    void Update()
    {
        //if (!dataSet && (NetworkServer.active || manager.IsClientConnected()))
        //{
        //    DisplayUserUI();
        //}

        //if(dataSet)
        //{
        //    HideUserUI();
        //}
    }

    void DisplayUserUI()
    {
        //Debug.Log("DISPLAY UI");
        userDataUI.SetActive(true);
    }

    void HideUserUI()
    {
        //Debug.Log("HIDE UI");

        userDataUI.GetComponentInChildren<Button>().enabled = false;
        inputName.enabled = false;
        inputId.enabled = false;

        userDataUI.GetComponent<FadeRawImage>().HideAndDesactivate();
        //userDataUI.GetComponentInChildren<FadeImage>().HideAndDesactivate();
        //userDataUI.GetComponentInChildren<FadeText>().HideAndDesactivate();
    }

    public void UpdatePlayerInfos()
    {
        name = inputName.text;
        id = Int32.Parse(inputId.text);
        manager.AskForPlayerDataUpdate(name, id, mainCamera.name);

        dataSet = true;
    }

    public void DebugUpdatePlayerInfos()
    {
        manager.AskForPlayerDataUpdate("farthur", 1, mainCamera.name);
        name = "farthur";
        id = 1;
        dataSet = true;
    }

    public string GetUserName()
    {
        return name;
    }

    public void SetUser(int id, string name, bool tutoMultiview, bool tutoHololens)
    {
        manager.AskForPlayerDataUpdate(name, id, mainCamera.name);
        this.name = name;
        this.id = id;
        this.hasDoneTutoMultiview = tutoMultiview;
        this.hasDoneTutoHololens = tutoHololens;

        dataSet = true;
    }

    public int GetUserId()
    {
        return id;
    }

    public bool HasDoneTutoMultiView()
    {
        return hasDoneTutoMultiview;
    }

    public bool HasDoneTutoHololens()
    {
        return hasDoneTutoHololens;
    }

    public bool IdDataSet()
    {
        return dataSet;
    }

    public void SetTutoDoneMultiView()
    {
        hasDoneTutoMultiview = true;
    }

    public void SetTutoDoneHololens()
    {
        hasDoneTutoMultiview = true;
    }

}
