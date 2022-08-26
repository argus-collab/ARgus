using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UINotification : MonoBehaviour
{
    public float duration = 2;

    public GameObject UIroot;
    public GameObject notificationPrefab;
    private GameObject notificationInstance;
    private RawImage notificationBackground;
    private Text notificationText;

    private float startingDisplay;
    private bool isDisplaying;

    public void DisplayTemporaryNotification(string message)
    {
        startingDisplay = Time.time;

        notificationInstance = Instantiate(notificationPrefab);
        notificationInstance.name = "notif";
        notificationInstance.transform.SetParent(UIroot.transform, false);
        notificationBackground = notificationInstance.GetComponentInChildren<RawImage>();
        notificationText = notificationInstance.GetComponentInChildren<Text>();
        notificationText.text = message;

        isDisplaying = true;
    }

    void Update()
    {
        float t = Time.time - startingDisplay;

        if (isDisplaying && t < duration)
        {
            Color colorBackground = notificationBackground.color;
            Color colorText = notificationText.color;

            colorBackground.a = 1 - Mathf.Exp(10 * (t - duration));
            colorText.a = 1 - Mathf.Exp(10 * (t - duration));

            notificationBackground.color = colorBackground;
            notificationText.color = colorText;
        }
        else if (isDisplaying)
        {
            isDisplaying = false;
            Destroy(notificationInstance);
        }
    }
}
