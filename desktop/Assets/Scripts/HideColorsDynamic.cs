using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideColorsDynamic : MonoBehaviour
{
    public Color defaultColor = Color.white;
    public GameObject[] exceptions;

    public bool isActive = false;

    void HideColorsDeeply(Transform t)
    {
        bool breaking = false;
        for(int i=0;!breaking && i<exceptions.Length;++i)
            breaking = (exceptions[i] == t.gameObject);

        if (breaking)
            return;

        Renderer rend = t.GetComponent<Renderer>();
        
        if(rend != null)
            for(int i=0;i< rend.materials.Length;++i)
                rend.materials[i].color = defaultColor;

        for(int i = 0; i < t.childCount; ++i)
            HideColorsDeeply(t.GetChild(i));
    }

    void Update()
    {
        if (isActive)
            HideColorsDeeply(transform); 
    }

    public void Activate()
    {
        isActive = true;
    }

    public void Desactivate()
    {
        isActive = false;
    }
}
