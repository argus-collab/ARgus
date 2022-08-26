using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideShowUI : MonoBehaviour
{
    public GameObject[] UIGOs;

    public bool isHiddenAtStart = false;
    private bool hidden = false;

    private void Start()
    {
        hidden = isHiddenAtStart;

        if (isHiddenAtStart)
            HideUI();
        else
            ShowUI();
    }

    void Update()
    {
        //if (Input.GetKeyDown("h"))
        //{
        //    OnChangeState();
        //}
    }

    public void OnChangeState()
    {
        if (hidden)
        {
            hidden = false;
            ShowUI();
        }
        else
        {
            hidden = true;
            HideUI();
        }
    }

    public void HideUI()
    {
        foreach (GameObject go in UIGOs)
            go.SetActive(false);
    }

    public void ShowUI()
    {
        foreach (GameObject go in UIGOs)
            go.SetActive(true);
    }
}
