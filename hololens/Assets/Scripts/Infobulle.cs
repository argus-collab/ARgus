using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Infobulle : MonoBehaviour
{
    public float offset = 70f;
    public Camera mainCamera;
    public bool pos1 = false;
    public bool pos2 = false;

    private bool showAtNextUpdate = false;

    private List<Image> images;
    private List<Text> texts;
    private Vector2 screenPos;

    private void Start()
    {
        images = new List<Image>();
        texts = new List<Text>();

        Image curIm = GetComponent<Image>();
        if(curIm != null)
            images.Add(curIm);
        Image[] imgs = GetComponentsInChildren<Image>();
        for(int i = 0; i < imgs.Length; ++i)
            images.Add(imgs[i]);

        Text curTex = GetComponent<Text>();
        if (curTex != null)
            texts.Add(GetComponent<Text>());
        Text[] txts = GetComponentsInChildren<Text>();
        for (int i = 0; i < txts.Length; ++i)
            texts.Add(txts[i]);
    }

    void Update()
    {
        Vector2 localPoint;

        //Vector2 mousePos = Input.mousePosition;
        Vector2 mousePos = screenPos;

        float halfHeight = mainCamera.scaledPixelHeight / 2;
        float halfWidth = mainCamera.scaledPixelWidth / 2;

        // offset infobulle
        if(pos1)
        {
            if (mousePos.y > halfHeight)
                mousePos.y += offset;
            else
                mousePos.y -= offset;

            if (mousePos.x > halfWidth)
                mousePos.x += offset;
            else
                mousePos.x -= offset;
        }

        // offset help icon
        if (pos2)
        {
            if (mousePos.x > halfWidth)
                mousePos.x += offset;
            else
                mousePos.x -= offset;
        }


        RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.parent.GetComponent<RectTransform>(), mousePos, null, out localPoint);
        transform.localPosition = localPoint;

        if (showAtNextUpdate)
        {
            for (int i = 0; i < texts.Count; ++i)
                texts[i].enabled = true;
            for (int i = 0; i < images.Count; ++i)
                images[i].enabled = true;

            showAtNextUpdate = false;
        }
        else
        {
            for (int i = 0; i < texts.Count; ++i)
                texts[i].enabled = false;
            for (int i = 0; i < images.Count; ++i)
                images[i].enabled = false;
        }
    }

    public void ShowAtNextUpdate(Vector2 screenPosition)
    {
        screenPos = screenPosition;
        showAtNextUpdate = true;
        //Debug.Log("show at next update");
    }
}
