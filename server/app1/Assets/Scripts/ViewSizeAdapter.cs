using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewSizeAdapter : MonoBehaviour
{
    public Transform[] toSize;
    public Camera renderingCamera;

    void Update()
    {
        for(int i=0; i < toSize.Length; ++i)
        {
            toSize[i].transform.localPosition = new Vector3(0,0,1);
            toSize[i].transform.localScale = 
                new Vector3(
                    renderingCamera.pixelWidth, 
                    renderingCamera.pixelHeight,
                    0); // at the bottom of UI 
        }
    }
}
