using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class NetworkTimer : MonoBehaviour
{
    public WebRTCNetworkCommunication net;
    public Text timerUI;
    
    private float startingTime;
    private bool startTimer = false;
    private bool stopTimer = false;
    private bool isRunning = false;

    void Start()
    {
        timerUI.enabled = false;
        net.OnNetworkEvent += EventCatcher;
    }

    void Update()
    {
        if(startTimer)
        {
            startTimer = false;
            startingTime = Time.time;
            isRunning = true;
            timerUI.enabled = true;
        }

        if (stopTimer)
        {
            stopTimer = false;
            isRunning = false;
            timerUI.enabled = false;
        }

        if (isRunning)
        {
            TimeSpan elapsed = TimeSpan.FromSeconds(Time.time - startingTime);
            if (elapsed.Seconds < 10)
                timerUI.text = elapsed.Minutes + ":0" + elapsed.Seconds;
            else
                timerUI.text = elapsed.Minutes + ":" + elapsed.Seconds;
        }
    }

    public void StartTimer()
    {
        startTimer = true;
    }

    public void StopTimer()
    {
        stopTimer = true;
    }

    public void EventCatcher(string arg)
    {
        string[] args = arg.Split('-');
        if (args[0] == "startTimer")
            StartTimer();
        else if (args[0] == "stopTimer")
            StopTimer();

    }
}
