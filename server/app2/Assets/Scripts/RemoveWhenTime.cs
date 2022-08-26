using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoveWhenTime : MonoBehaviour
{
    public float duration = 3;
    private float startingTime = -1;

    void Update()
    {
        if (startingTime < 0)
            startingTime = Time.time;

        if (Time.time - startingTime > duration)
            Destroy(gameObject);
    }
}
