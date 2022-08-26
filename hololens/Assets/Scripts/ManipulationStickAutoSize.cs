using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManipulationStickAutoSize : MonoBehaviour
{
    void Start()
    {
        UpdateSize();
    }

    void Update()
    {
        UpdateSize();
    }

    void UpdateSize()
    {
        transform.localPosition = new Vector3(0, 0, transform.parent.localScale.y / 2);
    }
}
