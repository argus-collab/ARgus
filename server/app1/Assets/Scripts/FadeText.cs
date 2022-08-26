using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeText : MonoBehaviour
{
    public Text rend;
    public float increment = 0.01f;

    private bool fadeIn = false;
    private bool fadeOut = false;
    private bool desactivate = false;

    void SetAlpha(float a)
    {
        Color c = rend.color;
        c.a = a;
        rend.color = c;
    }

    void Update()
    {
        if (rend == null)
            return;

        float alpha = rend.color.a;
        if (alpha < 0)
        {
            fadeIn = false;
            SetAlpha(0);
            if (desactivate)
            {
                gameObject.SetActive(false);
                desactivate = false;
            }
        }
        if (alpha > 1)
        {
            fadeOut = false;
            SetAlpha(1);
        }


        if (fadeIn && alpha > 0)
            SetAlpha(alpha - increment);

        if (fadeOut && alpha < 1)
            SetAlpha(alpha + increment);
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
        SetAlpha(0);
    }

    public void ShowImmediately()
    {
        fadeIn = false;
        fadeOut = false;
        SetAlpha(1);
    }
}
