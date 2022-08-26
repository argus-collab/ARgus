using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorTemporaryFix : MonoBehaviour
{
    public List<GameObject> monoColoredPart;
    public Color color = Color.white;

    void Update()
    {
        foreach (GameObject part in monoColoredPart)
        {
            GameObject go = GameObject.Find(part.name+"(Clone)");
            if (go != null)
                ApplyColorsDeeply(go.transform);
        }
    }

    void ApplyColorsDeeply(Transform t)
    {
        Renderer rend = t.GetComponent<Renderer>();

        if (rend != null)
            for (int i = 0; i < rend.materials.Length; ++i)
                rend.materials[i].color = color;

        for (int i = 0; i < t.childCount; ++i)
            ApplyColorsDeeply(t.GetChild(i));
    }
}
