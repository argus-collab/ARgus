using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaunchBatFile : MonoBehaviour
{
    public string scriptName = "launch_server";
    private System.Diagnostics.Process Processus;

    void Start()
    {
        Debug.Log(Application.dataPath);
        Debug.Log(scriptName);
        Processus = System.Diagnostics.Process.Start(Application.dataPath + "\\" + scriptName + ".bat");
    }

    private void OnApplicationQuit()
    {
        Processus.Kill();
    }
}
