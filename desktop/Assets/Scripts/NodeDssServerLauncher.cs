using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeDssServerLauncher : MonoBehaviour
{
    public string scriptName = "launch_server.bat";
    private System.Diagnostics.Process nodeJSServerProcessus;

    void Start()
    {
        Debug.Log(Application.dataPath);
        Debug.Log(scriptName);
        nodeJSServerProcessus = System.Diagnostics.Process.Start(Application.dataPath + "\\" + scriptName);
    }

    private void OnApplicationQuit()
    {
        nodeJSServerProcessus.Kill();
    }
}
