using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HideOnToggle : MonoBehaviour
{
    public Toggle toggle;

    private void Start()
    {
        
    }

    public void OnToggleChange(bool val = true)
    {
        gameObject.SetActive(toggle.isOn);
    }
}
