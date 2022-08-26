#if !UNITY_WSA
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class OnMouseOverDesactivate : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public ChangeViewCinemachine toDesactivate;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (toDesactivate != null)
            toDesactivate.gameObject.SetActive(false);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (toDesactivate != null)
            toDesactivate.gameObject.SetActive(true);
    }

    private void OnDestroy()
    {
        if (toDesactivate != null)
            toDesactivate.gameObject.SetActive(true);
    }

    private void OnDisable()
    {
        if (toDesactivate != null)
            toDesactivate.gameObject.SetActive(true);
    }
}
#endif