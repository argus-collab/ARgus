using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdentifyViewPoint : MonoBehaviour
{
    public LogManager logger;
    public GameObject circleBlinker;

    public void Identify()
    {
        IsViewPoint[] viewpoints = GameObject.FindObjectsOfType<IsViewPoint>();   
        
        foreach(IsViewPoint vp in viewpoints)
        {
            GameObject circleBlinkerInstance = Instantiate(circleBlinker, vp.gameObject.transform, false);
            circleBlinkerInstance.name = "blinker";
        }

        logger.LogShowCameraPosition();
    }
}
