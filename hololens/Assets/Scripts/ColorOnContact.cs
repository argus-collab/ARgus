using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorOnContact : MonoBehaviour
{
    public Color onTouch;
    public Color onNotTouch;

    private Collider col;
    private Renderer rend;

    private void Start()
    {
        col = GetComponent<Collider>();
        if (col == null)
            col = GetComponentInChildren<Collider>();

        rend = GetComponent<Renderer>();
        if (rend == null)
            rend = GetComponentInChildren<Renderer>();
    }

    private void Update()
    {
        //if (rend == null && rend == null)
        //    return;
    }

    void OnCollisionEnter(Collision collision)
    {
        if ((1 << collision.gameObject.layer) != (1 << LayerMask.NameToLayer("Cursor")))
        {
            rend.material.color = onTouch;
        }
        else
        {
            rend.material.color = onNotTouch;
        }
    }
}
