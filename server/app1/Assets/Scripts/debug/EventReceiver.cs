using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class EventReceiver : MonoBehaviour
{
    public EventEmitter container;

    void Start()
    {
        container.OnAction += Ping;
    }

    void Ping(string arg)
    {
        Debug.Log("Ping : " + arg);
    }
}