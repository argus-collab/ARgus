#if !UNITY_WSA
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class OnMouseOverDisplayViewInUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public enum View { virtualView, hololensView, kinectView };

    public View view;
    public DisplayViewInUI miniatureManager;
    public ChangeViewCinemachine viewmanager;
    private bool isDisplayingMiniature;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isDisplayingMiniature)
        {
            isDisplayingMiniature = true;

            if (view == View.virtualView && !viewmanager.IsVirtualView())
                miniatureManager.DisplayVirtualMiniatureStatic();
            else if (view == View.hololensView && !viewmanager.IsHololensView())
                miniatureManager.DisplayHololensMiniatureStatic();
            else if (view == View.kinectView && !viewmanager.IsKinectView())
                miniatureManager.DisplayKinectMiniatureStatic();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isDisplayingMiniature)
        {
            isDisplayingMiniature = false;

            miniatureManager.DesactivateDisplayStatic();
            miniatureManager.ActivateDisplayOnMouseOver();
            miniatureManager.HideView();
        }
    }
}
#endif