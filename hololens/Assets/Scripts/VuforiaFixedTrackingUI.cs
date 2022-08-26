/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VuforiaFixedTrackingUI : MonoBehaviour
{
    public VuforiaFixedTracking component;

    void OnGUI()
    {
        if (component.GetActiveTrackingState())
            component.SetActiveTrackingState(!GUI.Button(new Rect(10, 10, 150, 25), "Fix target"));
        else
            component.SetActiveTrackingState(GUI.Button(new Rect(10, 10, 150, 25), "Dynamize target"));
    }
}
*/