using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CallByRandomName : MonoBehaviour
{
    public string prefix;

    void Start()
    {
        string name = prefix;
        if (prefix.Length > 0)
            name += "_";
        name += Guid.NewGuid();

        gameObject.name = name;
    }
}
