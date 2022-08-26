using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class ExperimentControllerBehaviour : MonoBehaviour
{
    List<Action> executionQueue;
    public VirtualSceneRecorder sceneRecorder;
    public UserConfigLoadManager userConfigFileManager;
    public ScenePartManager scenePartManager;
#if !UNITY_WSA
    public DepthSourceManager depthManager;
    public ColorSourceManager colorManager;
    public RenderOccludedRGBKinect renderManager;
#endif 

    List<string> logs = new List<string>();

    public bool IsParametersFilled()
    {
        bool result = true;
        result &= (sceneRecorder != null);
        result &= (userConfigFileManager != null);
        result &= (scenePartManager != null);
#if !UNITY_WSA
        result &= (depthManager != null);
        result &= (colorManager != null);
        result &= (renderManager != null);
#endif
        return result;
    }

    private bool isTaskRunning = false;

    public bool IsTaskRunning() { return isTaskRunning; }

    private void Start()
    {
        executionQueue = new List<Action>();
    }

    public void StartTask(int taskid, int uid)
    {
        isTaskRunning = true;
        executionQueue.Add(new Action(() => { userConfigFileManager.LoadTask(uid, taskid); }));

        sceneRecorder.uid = uid;
        sceneRecorder.taskid = taskid;
        executionQueue.Add(new Action(() => { sceneRecorder.StartRecording(); }));
    }

    public void StopTask()
    {
        isTaskRunning = false;
        sceneRecorder.StopRecording();
    }

    public void LoadTutoHololens()
    {
        userConfigFileManager.LoadTutoHololens();
    }

    public void LoadTutoKinect()
    {
        userConfigFileManager.LoadTutoKinect();
    }

    public void LoadTutoVirtual()
    {
        userConfigFileManager.LoadTutoVirtual();
    }

    private void Update()
    {
        if (executionQueue.Count > 0)
        {
            executionQueue[0]();
            executionQueue.RemoveAt(0);
        }
    }

    public void ResetSourceManager()
    {
#if !UNITY_WSA
        depthManager.Reset();
        colorManager.Reset();
        renderManager.Reset();
#endif
    }

    public void ListLogs(string uid)
    {
        // TODO optimize with constant size for log string array
        logs.Clear();
        string[] rawLogs = Directory.GetFiles(@Application.dataPath+"/Logs");
        foreach(string log in rawLogs)
        {
            if (log.Contains("traj_" + uid) && !log.Contains(".meta"))
                logs.Add(log.Split('/')[5]);
        }
        for (int i = logs.Count; i < 4; ++i)
            logs.Add("...");
    }

    public List<string> GetListLogs()
    {
        return logs;
    }

    public void ResetScene()
    {
        scenePartManager.TextualCommand("remove all ok");   
    }

    public void InputTextualCommand(string command)
    {
        scenePartManager.TextualCommand(command);
    }
}
