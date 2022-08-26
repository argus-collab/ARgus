#if !UNITY_WSA
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BringWhenMove : MonoBehaviour
{
    public GameObject toBring;
    public bool prioritary;

    private MoveWith follow;

    private void Start()
    {
        follow = GetComponent<MoveWith>();
    }

    void Update()
    {
        if (prioritary)
        {
            follow.enabled = false;

            toBring.transform.position = transform.position;
            toBring.transform.rotation = transform.rotation;
        }
        else
            follow.enabled = true;
    }
}
#endif