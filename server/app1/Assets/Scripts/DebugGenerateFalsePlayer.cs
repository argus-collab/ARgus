using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugGenerateFalsePlayer : MonoBehaviour
{
    public GameObject hololensPlayer;
    public GameObject kinectPlayer;

    private void OnGUI()
    {
        if (GUI.Button(new Rect(10, 70, 50, 30), "Pop false players"))
            Init();
    }

    public void Init()
    {
        Instantiate(hololensPlayer);
        Instantiate(kinectPlayer);
    }
}
