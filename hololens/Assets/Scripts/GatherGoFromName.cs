using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GatherGoFromName : MonoBehaviour
{
    public string GOTag = "(Clone)";

    void Update()
    {
        GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
        foreach (GameObject go in allObjects)
            if (go.name.Contains(GOTag))
                go.transform.parent = transform;
    }
}
