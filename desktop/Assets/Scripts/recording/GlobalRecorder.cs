using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalRecorder : MonoBehaviour
{
    public VirtualSceneRecorder virtualSceneRecorder;
    public bool displayGUI = true;

    void OnGUI()
    {
        if(displayGUI)
        {
            if (!virtualSceneRecorder.IsRecording())
            {
                if (GUI.Button(new Rect(250, 60, 150, 25), "Start recording"))
                {
                    virtualSceneRecorder.StartRecording();
                }
            }
            else
            {
                if (GUI.Button(new Rect(250, 60, 150, 25), "Stop recording"))
                {
                    virtualSceneRecorder.StopRecording();
                }
            }
        }
    }
}
