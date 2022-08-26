#if !UNITY_WSA
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DesactivateOnStickUse : MonoBehaviour
{
    private StickCursorManager stickManager;

    public List<GameObject> toDesactivate;

    void Update()
    {
        if (stickManager != null)
        {
            if (stickManager.IsStickUsed())
            {
                foreach (GameObject go in toDesactivate)
                    go.SetActive(false);
            }   
            else
            {
                foreach (GameObject go in toDesactivate)
                    go.SetActive(false);
            }
        }
        else
        {
            stickManager = GetComponent<StickCursorManager>();
        }
    }
}
#endif