using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class EventEmitter : MonoBehaviour
{
    public delegate void Action(string arg);
    public event Action OnAction;

    void Update()
    {
        if (Input.anyKeyDown)
        {
            OnAction("pouet");
        }
    }
}