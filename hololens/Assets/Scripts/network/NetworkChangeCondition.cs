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

public class NetworkChangeCondition : NetworkBehaviour
{
    public List<ICondition> conditions;
    private int index;
    public int defautIndex = 1;

    public int GetIndex()
    {
        return index;
    }

    [ClientRpc]
    void RpcChangeConfiguration(int i)
    {
        Debug.Log("RPC call recieved");
        conditions[index].ResetCondition();

        index = i;
        if (index >= conditions.Count) index = 0;
        conditions[index].ApplyCondition();
    }

    public void ChangeConfiguration(int i)
    {
        if (!isServer) return;

        index = i;
        RpcChangeConfiguration(index);
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
    }
}
