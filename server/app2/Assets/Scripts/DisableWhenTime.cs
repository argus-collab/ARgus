using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableWhenTime : MonoBehaviour
{
    public float duration = 3;
    public MonoBehaviour toDisable;
    private float startingTime = -1;


    void Update()
    {
        if (toDisable.enabled && startingTime < 0)
            startingTime = Time.time;

        if(Time.time - startingTime > duration)
            toDisable.enabled = false;
    }
}
