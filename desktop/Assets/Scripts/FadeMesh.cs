using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeMesh : MonoBehaviour
{
    public MeshRenderer rend;
    public float increment = 0.01f;

    private bool fadeIn = false;
    private bool fadeOut = false;
    private bool desactivate = false;

    void Update()
    {
        if (rend == null)
            return;

        if (!rend.sharedMaterial.HasProperty("_alpha"))
            return;

        float alpha = rend.sharedMaterial.GetFloat("_alpha");
        if (alpha < 0)
        {
            fadeIn = false;
            rend.sharedMaterial.SetFloat("_alpha", 0);
            if (desactivate)
            {
                rend.enabled = false;
                desactivate = false;
            }
        }
        if (alpha > 1)
        {
            fadeOut = false;
            rend.sharedMaterial.SetFloat("_alpha", 1);
        }


        if (fadeIn && alpha > 0)
            rend.sharedMaterial.SetFloat("_alpha", alpha - increment);

        if (fadeOut && alpha < 1)
            rend.sharedMaterial.SetFloat("_alpha", alpha + increment);
    }

    public void Hide()
    {
        fadeIn = true;
        fadeOut = false;
    }

    public void HideAndDesactivate()
    {
        fadeIn = true;
        fadeOut = false;
        desactivate = true;
    }

    public void Show()
    {
        rend.enabled = true;

        fadeIn = false;
        fadeOut = true;
    }

    public void HideImmediately()
    {
        fadeIn = false;
        fadeOut = false;
        rend.sharedMaterial.SetFloat("_alpha", 0);
    }

    public void HideImmediatelyAndDesactivate()
    {
        fadeIn = false;
        fadeOut = false;
        rend.sharedMaterial.SetFloat("_alpha", 0);
        rend.enabled = false;
    }

    public void ShowImmediately()
    {
        rend.enabled = true;

        fadeIn = false;
        fadeOut = false;
        rend.sharedMaterial.SetFloat("_alpha", 1);
    }
}
