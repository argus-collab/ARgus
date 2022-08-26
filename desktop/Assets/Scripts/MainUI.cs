using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using UnityEngine.UI;
using System;

public class MainUI : MonoBehaviour
{
    public VirtualSceneRecorder sceneRecorder;
    public UserConfigLoadManager userFileManager;

    public Text state;
    public Image graphicState;
    public InputField uidField;

    public bool show;

    private Color tutoColorState = Color.yellow;
    private Color tutoColorText = Color.black;

    private Color defaultColorState = Color.white;
    private Color defaultColorText = Color.black;

    private Color recordingColorState = Color.red;
    private Color recordingColorText = Color.white;

    private Color warningColorState = Color.green;
    private Color warningColorText = Color.black;


    //private int uid;
    private bool isTaskRunning = false;

    List<Action> executionQueue;


    // Add a menu item named "Do Something" to MyMenu in the menu bar.
    public void Coucou()
    {
        Debug.Log("Coucou");
    }


    private void Start()
    {
        state.text = "waiting command...";
        executionQueue = new List<Action>();
    }

    public void OnClickButtonTask(int taskid)
    {
        if (CheckUID())
        {
            if (isTaskRunning)
            {
                StopTask();
                isTaskRunning = false;
            }
            else
            {
                StartTask(taskid);
                isTaskRunning = true;
            }
        }
    }

    bool CheckUID()
    {
        bool fieldCompleted = !(uidField.text == "");

        if(!fieldCompleted)
        {
            state.text = "Please fill the UID";
            state.color = warningColorText;
            graphicState.color = warningColorState;
        }
        else
            ResetInfoBar();

        return fieldCompleted;
    }

    void StartTask(int taskid)
    {
        Debug.Log(uidField.text);
        //Debug.Log(int.Parse(uidString));

        executionQueue.Add(new Action(() => { userFileManager.LoadTask(int.Parse(uidField.text), taskid); }));

        sceneRecorder.uid = int.Parse(uidField.text);
        sceneRecorder.taskid = taskid;
        executionQueue.Add(new Action(() => { sceneRecorder.StartRecording(); }));

        state.text = "RECORDING task " + (taskid + 1) + " - uid " + uidField.text;
        state.color = recordingColorText;
        graphicState.color = recordingColorState;
    }

    void StopTask()
    {
        sceneRecorder.StopRecording();
        ResetInfoBar();
    }

    public void LoadTutoHololens()
    {
        userFileManager.LoadTutoHololens();

        state.text = "TUTORIAL hololens";
        state.color = tutoColorText;
        graphicState.color = tutoColorState;
    }

    public void LoadTutoKinect()
    {
        userFileManager.LoadTutoKinect();
        
        state.text = "TUTORIAL kinect";
        state.color = tutoColorText;
        graphicState.color = tutoColorState;
    }

    public void LoadTutoVirtual()
    {
        userFileManager.LoadTutoVirtual();

        state.text = "TUTORIAL virtual";
        state.color = tutoColorText;
        graphicState.color = tutoColorState;
    }

    private void Update()
    {
        if (executionQueue.Count > 0)
        {
            executionQueue[0]();
            executionQueue.RemoveAt(0);
        }
    }

    private void ResetInfoBar()
    {
        state.text = "waiting command...";
        state.color = defaultColorText;
        graphicState.color = defaultColorState;
    }
}
