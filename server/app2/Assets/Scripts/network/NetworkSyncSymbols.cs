using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkSyncSymbols : MonoBehaviour
{
    private SymbolsRandomPlacement symbolsPlacer;


    public WebRTCNetworkCommunication netWebRTC;
    public UDPSceneManagerBackAndForth netUDP;
    public CustomServerNetworkManager netUnetServer;
    public CustomClientNetworkManager netUnetClient;

    private List<string> lastSyncHash;

    public bool d_sync = false;

    private bool sync = false;
    private string hash;
    private string data;

    void Start()
    {
        lastSyncHash = new List<string>();

        if (netWebRTC != null)
            netWebRTC.OnNetworkEvent += EventCatcher;

        if (netUDP != null)
            netUDP.OnNetworkEvent += EventCatcher;

        if (netUnetClient != null)
            netUnetClient.OnNetworkEvent += EventCatcher;
    }

    void Update()
    {
        if (symbolsPlacer == null)
            symbolsPlacer = FindObjectOfType<SymbolsRandomPlacement>();

        if (d_sync)
        {
            d_sync = false;
            SynchronizeFromHere();
        }

        if (sync)
        {
            sync = false;
            if (!lastSyncHash.Contains(hash))
            {
                lastSyncHash.Add(hash);

                Debug.Log("receive a syn event with hash : " + hash);

                Debug.Log("set state from string : " + data);
                if (symbolsPlacer != null)
                {
                    symbolsPlacer.SetStateFromString(data);
                    Debug.Log("done");
                }

                Synchronize(data);
            }
        }
    }

    public void InitializeAndSyncFromHere()
    {
        symbolsPlacer.Initialize();
        SynchronizeFromHere();
    }

    public void SynchronizeFromHere()
    {
        string hash = ""+Time.time;
        lastSyncHash.Add(hash);

        Synchronize(hash);
    }

    void Synchronize(string hash)
    {
        if (netWebRTC != null)
            netWebRTC.SendNetworkEvent("synchroSymbols-" + hash + "-" + symbolsPlacer.GetStateAsString());

        if (netUDP != null)
            netUDP.SendNetworkEvent("synchroSymbols-" + hash + "-" + symbolsPlacer.GetStateAsString());

        if (netUnetServer != null)
            netUnetServer.SendNetworkEvent("synchroSymbols-" + hash + "-" + symbolsPlacer.GetStateAsString());
    }

    public void EventCatcher(string arg)
    {
        Debug.Log("catch event : " + arg);

        string[] args = arg.Split('-');
        if (args[0] != "synchroSymbols") return;

        Debug.Log("event is synchro symbol");
        sync = true;
        hash = args[1];
        data = args[2];
    }
}
