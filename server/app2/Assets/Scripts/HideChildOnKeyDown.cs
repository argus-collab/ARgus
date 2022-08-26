using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideChildOnKeyDown : MonoBehaviour
{
    private bool previousState = false;
    private bool hide = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
            hide = !hide;
        
        if (hide != previousState)
        {
            previousState = hide;

            if (hide)
            {
                for (int i = 0; i < transform.childCount; ++i)
                    transform.GetChild(i).gameObject.SetActive(false);
            }
            else
            {
                for (int i = 0; i < transform.childCount; ++i)
                    transform.GetChild(i).gameObject.SetActive(true);
            }

        }
    }

    public bool IsHidden()
    {
        return hide;
    }
}
