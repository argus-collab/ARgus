using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RemoteUserGazeDirectionsManager : MonoBehaviour
{
    public int offset = 400;

    public Camera appCamera;

    public Image arrowRright;
    public Image arrowLeft;

    public Text textRight;
    public Text textLeft;

    public string textRightContent = "Partner's RIGHT";
    public string textLeftContent = "Partner's LEFT";

    public Sprite arrowRightSprite;
    public Sprite arrowRightSpriteReverse;
    public Sprite arrowLeftSprite;
    public Sprite arrowLeftSpriteReverse;

    public RectTransform rightIndicator;

    private GameObject remoteUserGO;
    public string remoteUserName = "Player(Clone)";

    
    void Update()
    {
        if (remoteUserGO == null)
        {
            remoteUserGO = GameObject.Find(remoteUserName);
        }
        else
        {
            Vector3 camZonXZ = Vector3.ProjectOnPlane(appCamera.transform.forward, transform.up);
            Vector3 remoteUserZonXZ = Vector3.ProjectOnPlane(remoteUserGO.transform.forward, transform.up);
            float angle = Vector3.Angle(camZonXZ, remoteUserZonXZ);

            if (angle < 90 || angle > 270)
                RightIsRight();
            else
                RightIsLeft();

        }
    }

    void RightIsRight()
    {
        arrowRright.sprite = arrowRightSpriteReverse;
        arrowLeft.sprite = arrowLeftSpriteReverse;

        textRight.text = textRightContent;
        textRight.color = Color.green;
        textLeft.text = textLeftContent;
        textLeft.color = Color.red;
    }

    void RightIsLeft()
    {
        arrowRright.sprite = arrowLeftSprite;
        arrowLeft.sprite = arrowRightSprite;

        textRight.text = textLeftContent;
        textRight.color = Color.red;
        textLeft.text = textRightContent; 
        textLeft.color = Color.green;
    }

}
