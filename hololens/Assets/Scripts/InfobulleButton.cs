using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InfobulleButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject infobulleGO;

    public void OnPointerEnter(PointerEventData eventData)
    {
        infobulleGO.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        infobulleGO.SetActive(false);
    }
}
