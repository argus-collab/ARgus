using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProcessLauncher : MonoBehaviour
{
    public string scriptName = "launch_server.bat";
    private System.Diagnostics.Process processus;
    public bool launchAtStart = false;

    void Start()
    {
        if (launchAtStart)
        {
            Debug.Log(Application.dataPath);
            Debug.Log(scriptName);
            processus = System.Diagnostics.Process.Start(Application.dataPath + "\\" + scriptName);
        }
    }

    public void Launch()
    {
        if (processus != null && !processus.HasExited)
            processus.Kill();
        processus = System.Diagnostics.Process.Start(Application.dataPath + "\\" + scriptName);
    }

    public bool HasProcessExited()
    {
        return processus.HasExited;
    }

    private void OnApplicationQuit()
    {
        if (processus!=null)
            processus.Kill();
    }
}
