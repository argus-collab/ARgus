using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;

public interface ICondition
{
    int Index
    {
        get;
        set;
    }
    void ApplyCondition();
    void ResetCondition();
}

public class NetworkChangeCondition : MonoBehaviour
{
    public WebRTCNetworkCommunication net;
    public List<ICondition> conditions;
    private int index;
    private int nextIndex;
    public int defautIndex = 1;
    public bool isServer = false;

    private bool changeConfig = false;

    public int GetIndex()
    {
        return index;
    }

    //[ClientRpc]
    //void RpcChangeConfiguration(int i)
    //{
    //    Debug.Log("RPC call recieved");

    //}

    public void ChangeConfiguration(int i)
    {
        if (!isServer) return;

        index = i;
        net.SendNetworkEvent("changeConfig-"+i);
        //RpcChangeConfiguration(index);
    }

    public class C : ICondition
    {
        int _index;
        public int Index
        {
            get => _index;
            set => _index = value;
        }
        public C() { }

        void ICondition.ApplyCondition() { }
        void ICondition.ResetCondition() { }
    };

    public void Start()
    {
        index = defautIndex;
        var conditionsInScene = GameObject.FindObjectsOfType<MonoBehaviour>().OfType<ICondition>();

        conditions = new List<ICondition>();
        //conditions.Capacity = conditionsInScene.ToList<ICondition>().Count;
        // caca mais bellec j'en ai ma claque
        for (int i = 0; i < conditionsInScene.ToList<ICondition>().Count; ++i)
            conditions.Add(new C());

        foreach (ICondition c in conditionsInScene)
            conditions[c.Index] = c;

        net.OnNetworkEvent += EventCatcher;
    }

    void Update()
    {
        if (changeConfig)
        {
            changeConfig = false;

            conditions[index].ResetCondition();

            if (nextIndex >= conditions.Count) nextIndex = 0;
            conditions[nextIndex].ApplyCondition();
            index = nextIndex;
        }
    }

    public void EventCatcher(string arg)
    {
        string[] args = arg.Split('-');
        if (args[0] != "changeConfig") return;
        nextIndex = int.Parse(args[1]);
        changeConfig = true;
        // in update to avoid execution in thread of message receiving
    }
}
