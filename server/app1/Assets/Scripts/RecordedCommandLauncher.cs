using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecordedCommandLauncher : MonoBehaviour
{
    public ScenePartManager spm;
    public UDPSceneManagerBackAndForth udp;
    public List<string> commands;

    public bool controledByHololens;

    public void LaunchCommand(int i)
    {
        if(controledByHololens)
        {
            udp.SendSceneManagerCommand(commands[i]);
        }
        else
            spm.TextualCommand(commands[i]);
    }
}
