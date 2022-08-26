using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
//using UnityEngine.SceneManagement;
//using System.Linq.Expressions;
//using UnityEditor.Profiling;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;

public class UDPSceneManagerBackAndForth : MonoBehaviour
{
    public delegate void NetworkEvent(string arg);
    public event NetworkEvent OnNetworkEvent;

    public string keywordToRemove = "Client";

    private string IP = "127.0.0.1";  // define in init
    public int portIn = 8052;
    public int portOut = 8053;

    IPEndPoint remoteEndPoint;
    UdpClient client;

    public GameObject rootScene;

    // where the player are poped at
    public GameObject rootPlayers;
    public string remoteRootPlayerName = "Players";
    public string remoteRootSceneName = "NetworkedScenePartScene";


    //[Header("UNET - leave empty not receiver")]
    //public string remoteRootSceneName;
    //public string remoteRootPlayerName;
    public CustomServerNetworkManager server;
    public ScenePartManager scenePartManager;

    private List<int> sceneState;
    private List<GameObject> sceneStateGOs;
    private List<int> lastSceneState;
    private List<string> lastSceneStateNames;

    private List<GameObject> playersState;
    private List<string> lastPlayersState;

    private List<string> ToAddQueue;
    private List<int> ToAddPortQueue;
    private List<string> ToDeleteQueue;

    //private List<string> ToAddScenePartQueue;
    //private List<int> ToAddPortScenePartQueue;
    //private List<string> ToDeleteScenePartQueue;
    private List<string> scenePartCommand;
    private List<string> networkEvents;

    private List<string> ToAddPlayerQueue;
    private List<int> ToAddPortPlayerQueue;
    private List<string> ToDeletePlayerQueue;

    private List<string> ToUpdateColorGOQueue;
    private List<float> ToUpdateColorColorsRQueue;
    private List<float> ToUpdateColorColorsGQueue;
    private List<float> ToUpdateColorColorsBQueue;
    private List<float> ToUpdateColorColorsAQueue;

    private List<string> ToUpdateComponentsGOQueue;
    private List<string> ToUpdateComponentsNamesQueue;
    private List<string> ToUpdateComponentsParamsQueue;
    private List<object> ToUpdateComponentsValuesQueue;

    // scheduled for update
    private List<string> actions;
    private List<string> parameters;

    // temporisation
    private float waitingTime; // ms
    private float lastTimeStamp;

    List<int> ToCallAtStartObjectsIndex;
    List<string> ToCallAtStart;

    // player prefab names
    public string hololensPrefabName = "HololensRepresentation";
    public string kinectPrefabName = "KinectRepresentation";
    public string participantPrefabName = "ParticipantRepresentation";

    private List<string> AllowedTypes = new List<string>()
    {
        "System.Boolean",
        "System.Int16",
        "System.UInt16",
        "System.Int32",
        "System.UInt32",
        "System.Int64",
        "System.UInt64",
        "System.Single",
        "System.Double",
        "System.String"
    };

    public int startingPort = 8053;

    public void Start()
    {
        ToAddQueue = new List<string>();
        ToAddPortQueue = new List<int>();
        ToDeleteQueue = new List<string>();

        //ToAddScenePartQueue = new List<string>();
        //ToAddPortScenePartQueue = new List<int>();
        //ToDeleteScenePartQueue = new List<string>();
        scenePartCommand = new List<string>();

        ToAddPlayerQueue = new List<string>();
        ToAddPortPlayerQueue = new List<int>();
        ToDeletePlayerQueue = new List<string>();

        lastSceneState = new List<int>();
        lastSceneStateNames = new List<string>();

        sceneState = new List<int>();
        sceneStateGOs = new List<GameObject>();

        playersState = new List<GameObject>();
        lastPlayersState = new List<string>();

        ToUpdateColorGOQueue = new List<string>();
        ToUpdateColorColorsRQueue = new List<float>();
        ToUpdateColorColorsGQueue = new List<float>();
        ToUpdateColorColorsBQueue = new List<float>();
        ToUpdateColorColorsAQueue = new List<float>();

        ToUpdateComponentsGOQueue = new List<string>();
        ToUpdateComponentsNamesQueue = new List<string>();
        ToUpdateComponentsParamsQueue = new List<string>();
        ToUpdateComponentsValuesQueue = new List<object>();

        ToCallAtStartObjectsIndex = new List<int>();
        ToCallAtStart = new List<string>();
        //ToCallAtUpdate = new List<MethodInfo>();

        actions = new List<string>();
        parameters = new List<string>();

        networkEvents = new List<string>();

        InitClient();
        InitServer();
    }

    public void Pause(float time)
    {
        waitingTime = time;
        lastTimeStamp = Time.time;
    }

    public void InitServer()
    {
        remoteEndPoint = new IPEndPoint(IPAddress.Parse(IP), portOut);
        client = new UdpClient();
    }

    private void InitClient()
    {
        //receiveThread = new Thread(
        //    new ThreadStart(ReceiveData));
        //receiveThread.IsBackground = true;
        //receiveThread.Start();


        Task.Run(async () =>
        {
            Debug.Log("Launching receiver");
            using (client = new UdpClient(portIn))
            {
                Debug.Log("receiver running");
                while (true)
                {
                    var receivedResults = await client.ReceiveAsync();
                    ReceiveData(receivedResults.Buffer);
                }
            }
        });
    }

    public void UpdateScheduledSender()
    {
        for (int i = 0; i < actions.Count; ++i)
            SendData(actions[i], parameters[i]);

        actions.Clear();
        parameters.Clear();
    }

    public void ScheduleSender(string action, string param)
    {
        actions.Add(action);
        parameters.Add(param);
    }

    public void SendNetworkEvent(string param)
    {
        ScheduleSender("network-event", param);
    }

    // ! : one depth level fo the scene is monitored
    private void Update()
    {
        // pause if any
        if (Time.time - lastTimeStamp < waitingTime)
            return;

        // sender
        //UpdateGameObjectsSender();
        UpdateGameObjectsSender();
        UpdatePlayerSender();
        UpdateScheduledSender();
        
        // receiver
        UpdateGameObjectsReceiver();
        //UpdateGameObjectsScenePartReceiver();
        UpdateComponentsReceiver();
        UpdatePlayerReceiver();
        UpdateGameObjectsColorReceiver();
        UpdateScenePartCommandReceiver();

        // network events
        UpdateNetworkEvent();
    }

    void UpdateNetworkEvent()
    {
        lock(networkEvents)
            LockedUpdateNetworkEventReceiver();
    }

    void UpdateGameObjectsReceiver()
    {
        lock (ToAddQueue)
            lock (ToAddPortQueue)
                lock (ToDeleteQueue)
                    LockedUpdateGameObjectsReceiver();
    }

    //void UpdateGameObjectsScenePartReceiver()
    //{
    //    lock (ToAddScenePartQueue)
    //        lock (ToAddPortScenePartQueue)
    //            lock (ToDeleteScenePartQueue)
    //                LockedUpdateGameObjectsReceiver();
    //}

    void UpdateComponentsReceiver()
    {
        lock (ToUpdateComponentsGOQueue)
            lock (ToUpdateComponentsNamesQueue)
                lock (ToUpdateComponentsParamsQueue)
                    lock (ToUpdateComponentsValuesQueue)
                        lock (ToCallAtStart)
                            lock (ToCallAtStartObjectsIndex)
                                LockedUpdateComponentsReceiver();
    }

    void UpdatePlayerReceiver()
    {
        lock (ToAddPlayerQueue)
            lock (ToAddPortPlayerQueue)
                lock (ToDeletePlayerQueue)
                    LockedUpdatePlayerReceiver();
    }

    void UpdateGameObjectsColorReceiver()
    {
        lock (ToUpdateColorGOQueue)
            lock (ToUpdateColorColorsRQueue)
                lock (ToUpdateColorColorsGQueue)
                    lock (ToUpdateColorColorsBQueue)
                        lock (ToUpdateColorColorsAQueue)
                            LockedUpdateGameObjectsColorReceiver();
    }

    void UpdateScenePartCommandReceiver()
    {
        lock (scenePartCommand)
            LockedUpdateScenePartCommandReceiver();
    }


    public void LockedUpdateComponentsReceiver()
    {
        /*
         ToUpdateComponentsGOQueue;
         ToUpdateComponentsNamesQueue;
         ToUpdateComponentsParamsQueue
         ToUpdateComponentsValuesQueue
         */


        if (ToUpdateComponentsGOQueue.Count > 0)
        {
            Debug.Log("UPDATE COMPONENT RECEIVER");

            for (int i = 0; i < ToUpdateComponentsGOQueue.Count; ++i)
            {
                GameObject go = GameObject.Find(ToUpdateComponentsGOQueue[i]);
                Debug.Log("search go " + ToUpdateComponentsGOQueue[i]);
                if (go != null)
                {
                    Component c = go.GetComponent(ToUpdateComponentsNamesQueue[i]);
                    Debug.Log("found ! search component : " + ToUpdateComponentsNamesQueue[i]);
                    if (c != null)
                    {
                        for (int j = 0; j < ToUpdateComponentsParamsQueue.Count; ++j)
                        {
                            foreach (FieldInfo fi in c.GetType().GetFields())
                            {
                                if (fi.Name == ToUpdateComponentsParamsQueue[j])
                                {
                                    System.Object obj = (System.Object)c;
                                    fi.SetValue(obj, ToUpdateComponentsValuesQueue[j]);
                                    Debug.Log("\tset " + fi.Name + " at " + obj);
                                }
                            }
                        }


                        if (ToCallAtStartObjectsIndex.Contains(i))
                        {
                            int index = ToCallAtStartObjectsIndex.IndexOf(i);
                            Debug.Log("about to call " + ToCallAtStart[index]);

                            MethodInfo theMethod = c.GetType().GetMethod(ToCallAtStart[index]);
                            theMethod.Invoke((System.Object)c, null);
                            Debug.Log("called " + theMethod.Name);
                        }
                    }

                }
            }
        }

        ToUpdateComponentsGOQueue.Clear();
        ToUpdateComponentsNamesQueue.Clear();
        ToUpdateComponentsParamsQueue.Clear();
        ToUpdateComponentsValuesQueue.Clear();

        ToCallAtStart.Clear();
        ToCallAtStartObjectsIndex.Clear();
    }

    public void LockedUpdateGameObjectsReceiver()
    {
        if (ToAddQueue.Count > 0)
        {
            for (int i = 0; i < ToAddQueue.Count; ++i)
            {
                string[] splitResult = ToAddQueue[i].Split(';');
                Debug.Log(ToAddQueue[i]);
                foreach (string s in splitResult)
                    Debug.Log(s);

                //if (GameObject.Find(ToAddQueue[i]) != null)
                //    continue;

               
                if(splitResult.Length > 1)
                {
                    if (GameObject.Find(splitResult[1]) != null)
                        continue;

                    GameObject goToInstanciate = Resources.Load(splitResult[0]) as GameObject;
                    if (goToInstanciate != null)
                    {
                        GameObject go = Instantiate(goToInstanciate, new Vector3(0, 0, 0), Quaternion.identity);
                        go.name = splitResult[1];
                        if (rootScene != null)
                            go.transform.parent = rootScene.transform;

                        UDPSyncObject udpSyncComponent = go.AddComponent<UDPSyncObject>();
                        udpSyncComponent.isReceiver = true;
                        udpSyncComponent.port = ToAddPortQueue[i];
                        udpSyncComponent.Init();

                        startingPort++;

                        go.AddComponent<NetworkMaster>();

                        //CustomServerNetworkManager.AddGameObjectInScenePart(ToAddQueue[i], ToAddQueue[i],Vector3.zero, Quaternion.identity,"LocalScene");
                        //go.transform.name += "Client"; // pour réutiliser AddGameObjectInScenePart en l'état

                        //NetworkServer.Spawn(go);
                        //NetworkServer.SpawnWithClientAuthority(go, NetworkServer.connections[0]);

                        //if (!server.useWebRTC)
                        //    NetworkServer.Spawn(go);
                        //else
                            server.AddGameObjectInScenePart(go.name, go.name, go.transform.position, go.transform.rotation, remoteRootSceneName, false);
                    }
                }
                else
                {
                    if (GameObject.Find(ToAddQueue[i]) != null)
                        continue;

                    GameObject goToInstanciate = Resources.Load(ToAddQueue[i]) as GameObject;
                    if (goToInstanciate != null)
                    {
                        GameObject go = Instantiate(goToInstanciate, new Vector3(0, 0, 0), Quaternion.identity);
                        go.transform.name = ToAddQueue[i];
                        if (rootScene != null)
                            go.transform.parent = rootScene.transform;

                        UDPSyncObject udpSyncComponent = go.AddComponent<UDPSyncObject>();
                        udpSyncComponent.isReceiver = true;
                        udpSyncComponent.port = ToAddPortQueue[i];
                        udpSyncComponent.Init();

                        startingPort++;

                        go.AddComponent<NetworkMaster>();


                        //CustomServerNetworkManager.AddGameObjectInScenePart(ToAddQueue[i], ToAddQueue[i],Vector3.zero, Quaternion.identity,"LocalScene");
                        //go.transform.name += "Client"; // pour réutiliser AddGameObjectInScenePart en l'état

                        //NetworkServer.SpawnWithClientAuthority(go, NetworkServer.connections[0]);
                        //if (!server.useWebRTC)
                        //    NetworkServer.Spawn(go);
                        //else
                            server.AddGameObjectInScenePart(go.name, go.name, go.transform.position, go.transform.rotation, remoteRootSceneName, false);
                    }
                }
            }
            ToAddQueue.Clear();
            ToAddPortQueue.Clear();
        }

        if (ToDeleteQueue.Count > 0)
        {
            for (int i = 0; i < ToDeleteQueue.Count; ++i)
            {
                GameObject go = GameObject.Find(ToDeleteQueue[i]);
                if (go != null)
                    GameObject.Destroy(go);
            }
            ToDeleteQueue.Clear();
        }
    }

    //public void LockedUpdateGameObjectsScenePartReceiver()
    //{
    //    if (ToAddScenePartQueue.Count > 0)
    //    {
    //        for(int i = 0; i < ToAddScenePartQueue.Count; ++i)
    //            scenePartManager.AddGameObjectInScenePart(ToAddScenePartQueue[i]);

    //        ToAddScenePartQueue.Clear();
    //    }

    //    if (ToDeleteScenePartQueue.Count > 0)
    //    {
    //        for (int i = 0; i < ToAddScenePartQueue.Count; ++i)
    //            scenePartManager.RemoveGameObjectInScenePart(ToAddScenePartQueue[i]);

    //        ToDeleteScenePartQueue.Clear();
    //    }
    //}

    public void LockedUpdatePlayerReceiver()
    {
        if (ToAddPlayerQueue.Count > 0)
        {
            for (int i = 0; i < ToAddPlayerQueue.Count; ++i)
            {
                //if (ToAddPlayerQueue[i].Contains("Kinect"))
                if (ToAddPlayerQueue[i].Contains(kinectPrefabName))
                {
                    InstanciatePlayerKinect(ToAddPlayerQueue[i], ToAddPortPlayerQueue[i]);
                }
                //else if (ToAddPlayerQueue[i].Contains("Participant"))
                else if (ToAddPlayerQueue[i].Contains(participantPrefabName))
                {
                    InstanciatePlayerParticipant(ToAddPlayerQueue[i], ToAddPortPlayerQueue[i]);
                }
                //else if (ToAddPlayerQueue[i].Contains("Head"))
                else if (ToAddPlayerQueue[i].Contains(hololensPrefabName))
                {
                    InstanciatePlayerHead(ToAddPlayerQueue[i], ToAddPortPlayerQueue[i]);
                }
                //else if (ToAddPlayerQueue[i].Contains("RightHand"))
                else if (ToAddPlayerQueue[i].Contains("right_"))
                {
                    InstanciateHandJoint("right", ToAddPlayerQueue[i], ToAddPortPlayerQueue[i]);
                }
                //else if (ToAddPlayerQueue[i].Contains("LeftHand"))
                else if (ToAddPlayerQueue[i].Contains("left_"))
                {
                    InstanciateHandJoint("left", ToAddPlayerQueue[i], ToAddPortPlayerQueue[i]);
                }


            }
            ToAddPlayerQueue.Clear();
            ToAddPortPlayerQueue.Clear();
        }

        if (ToDeletePlayerQueue.Count > 0)
        {
            for (int i = 0; i < ToDeletePlayerQueue.Count; ++i)
            {
                GameObject go = GameObject.Find(ToDeletePlayerQueue[i]);
                if (go != null)
                    GameObject.Destroy(go);
            }
            ToDeletePlayerQueue.Clear();
        }
    }
    
    private void LockedUpdateGameObjectsColorReceiver()
    {
        if (ToUpdateColorGOQueue.Count > 0)
        {
            for (int i = 0; i < ToUpdateColorGOQueue.Count; ++i)
            {
                GameObject go = GameObject.Find(ToUpdateColorGOQueue[i]);
                if (go != null)
                {
                    Color c = new Color(
                        ToUpdateColorColorsRQueue[i],
                        ToUpdateColorColorsGQueue[i],
                        ToUpdateColorColorsBQueue[i],
                        ToUpdateColorColorsAQueue[i]
                        );
                    ApplyColorsDeeply(go.transform, c);

                    NetworkIdentity id = go.GetComponent<NetworkIdentity>();
                    if (id != null)
                        server.UpdateColorForAll(id.netId, c);
                }
            }

            ToUpdateColorGOQueue.Clear();
            ToUpdateColorColorsRQueue.Clear();
            ToUpdateColorColorsGQueue.Clear();
            ToUpdateColorColorsBQueue.Clear();
            ToUpdateColorColorsAQueue.Clear();
        }
    }

    private void LockedUpdateScenePartCommandReceiver()
    {
        for(int i = 0; i < scenePartCommand.Count; ++i)
            scenePartManager.TextualCommand(scenePartCommand[i]);

        scenePartCommand.Clear();
    }

    private void LockedUpdateNetworkEventReceiver()
    {
        for (int i = 0; i < networkEvents.Count; ++i)
            OnNetworkEvent(networkEvents[i]);

        networkEvents.Clear();
    }

    public void UpdateGameObjectsSender()
    {

        sceneState.Clear();
        sceneStateGOs.Clear();

        // update scene state
        for (int i = 0; i < rootScene.transform.childCount; ++i)
        {
            sceneState.Add(rootScene.transform.GetChild(i).GetInstanceID());

            GameObject go = rootScene.transform.GetChild(i).gameObject;
            sceneStateGOs.Add(go);
        }

        //string d = "";
        //for (int i = 0; i < sceneState.Count; ++i)
        //    d += sceneState[i].name + " ";
        //Debug.Log("scene state = " + d);

        //d = "";
        //for (int i = 0; i < lastSceneState.Count; ++i)
        //    d += lastSceneState[i].name + " ";
        //Debug.Log("last scene state = " + d);

        // remove
        for (int i = 0; i < lastSceneState.Count; ++i)
        {
            if (!sceneState.Contains(lastSceneState[i]))
            {
                string prefabName = lastSceneStateNames[i];
                if (keywordToRemove.Length > 0)
                    prefabName = prefabName.Replace(keywordToRemove, "");
                SendData("remove-go", prefabName);
            }
        }

        // add
        for (int i = 0; i < sceneState.Count; ++i)
        {
            if (!lastSceneState.Contains(sceneState[i]))
            {
                string prefabName = sceneStateGOs[i].name;
                
                if(keywordToRemove.Length > 0)
                    prefabName = prefabName.Replace(keywordToRemove, "");

                // is object a prefab instance ?
                string[] splitResult = prefabName.Split('-');
                if (splitResult.Length > 1)
                    SendData("add-go", startingPort + ";" + splitResult[0] + ";" + prefabName);// sceneState[i].name);
                else
                    SendData("add-go", startingPort + ";" + prefabName);// sceneState[i].name);

                UDPSyncObject udpSync = sceneStateGOs[i].AddComponent<UDPSyncObject>();
                udpSync.port = startingPort;
                startingPort++;
                udpSync.Init();

                // update also component ?
                UDPSyncComponent c = sceneStateGOs[i].GetComponent<UDPSyncComponent>();
                if (c != null)
                {
                    Debug.Log("contain UDP sync component");
                    UpdateComponentsSender(prefabName, c);
                }
            }
        }

        lastSceneState.Clear();
        for (int i = 0; i < sceneState.Count; ++i)
        {
            lastSceneState.Add(sceneState[i]); // copy
        }
        lastSceneStateNames.Clear();
        for (int i = 0; i < rootScene.transform.childCount; ++i)
        {
            lastSceneStateNames.Add(rootScene.transform.GetChild(i).name);
        }
    }

    public void UpdatePlayerSender()
    {
        playersState = new List<GameObject>();
        List<string> playersStateFullName = new List<string>();

        for (int i = 0; i < rootPlayers.transform.childCount; ++i)
        {
            string ip = rootPlayers.transform.GetChild(i).name;

            Transform head = rootPlayers.transform.GetChild(i).Find("Head");
            Transform participant = rootPlayers.transform.GetChild(i).Find("Participant");
            Transform kinect = rootPlayers.transform.GetChild(i).Find("Kinect");
            Transform rightHand = rootPlayers.transform.GetChild(i).Find("Right_hand");
            Transform leftHand = rootPlayers.transform.GetChild(i).Find("Left_hand");

            if (kinect != null)
            {
                playersState.Add(kinect.gameObject);
                //playersStateFullName.Add(ip + "-Kinect-" + kinect.name);
                playersStateFullName.Add(kinectPrefabName);
            }

            if (participant != null)
            {
                playersState.Add(participant.gameObject);
                //playersStateFullName.Add(ip + "-Participant-" + participant.name);
                playersStateFullName.Add(participantPrefabName);
            }

            if (head != null)
            {
                playersState.Add(head.gameObject);
                //playersStateFullName.Add(ip + "-Head-" + head.name);
                playersStateFullName.Add(hololensPrefabName);
            }

            if (rightHand != null)
            {
                for (int j = 0; j < rightHand.childCount; ++j)
                {
                    playersState.Add(rightHand.GetChild(j).gameObject);
                    //playersStateFullName.Add(ip + "-RightHand-" + rightHand.GetChild(j).name);
                    playersStateFullName.Add("right_" + rightHand.GetChild(j).name);
                }
            }

            if (leftHand != null)
            {
                for (int j = 0; j < leftHand.childCount; ++j)
                {
                    playersState.Add(leftHand.GetChild(j).gameObject);
                    //playersStateFullName.Add(ip + "-LeftHand-" + leftHand.GetChild(j).name);
                    playersStateFullName.Add("left_" + leftHand.GetChild(j).name);
                }
            }
        }

        // remove
        for (int i = 0; i < lastPlayersState.Count; ++i)
        {
            if (!playersStateFullName.Contains(lastPlayersState[i]))
            {
                SendData("remove-player", lastPlayersState[i]);
            }
        }

        // add
        for (int i = 0; i < playersState.Count; ++i)
        {
            if (!lastPlayersState.Contains(playersStateFullName[i]))
            {
                SendData("add-player", startingPort + ";" + playersStateFullName[i]);
                UDPSyncObject udpSync = playersState[i].AddComponent<UDPSyncObject>();
                udpSync.port = startingPort;
                startingPort++;
                udpSync.Init();
            }
        }

        lastPlayersState = playersStateFullName;
    }

    public void UpdateGameObjectsColorSender(GameObject go, Color color)
    {
        string action = "apply-color";

        string param = go.name + ";";
        param += color.r + ";";
        param += color.g + ";";
        param += color.b + ";";
        param += color.a;

        //SendData(action, param);
        ScheduleSender(action, param);
    }

    public void UpdateComponentsSender(string GOName, UDPSyncComponent c)
    {
        // ;supportCustomFormat;SupportGenerator;4;NbCaseX;int;
        string action = "update-component";

        string param = GOName + ";";
        param += c.component.GetType() + ";";

        Debug.Log("COMPONENT : " + c.component);
        Debug.Log("COMPONENT NAME : " + c.component.name);

        // param
        int nbComponentParam = 0;
        string componentParam = "";

        System.Object obj = (System.Object)c.component;

        foreach (FieldInfo fi in c.component.GetType().GetFields())
        {
            object var = fi.GetValue(obj);
            componentParam += fi.Name + ";";
            componentParam += var.GetType() + ";";
            componentParam += var + ";";
            nbComponentParam++;
        }

        param += nbComponentParam + ";";
        param += componentParam;

        // to call at start
        param += c.ToCallAtStart.Length + ";";
        for (int i = 0; i < c.ToCallAtStart.Length; ++i)
        {
            param += c.ToCallAtStart[i] + ";";
        }

        // to call at update
        // ... TODO

        SendData(action, param);
    }


    private void InstanciatePlayerHead(string name, int port)
    {
        Debug.Log("UDP Instaciate player head call");
        GameObject goToInstanciate = Resources.Load(hololensPrefabName) as GameObject;
        if (goToInstanciate != null)
        {
            Debug.Log("UDP Instaciate ");

            GameObject go = Instantiate(goToInstanciate, new Vector3(0, 0, 0), Quaternion.identity);
            //go.transform.name = name;
            go.transform.name = hololensPrefabName;
            if (rootScene != null)
                go.transform.parent = rootPlayers.transform;

            UDPSyncObject udpSyncComponent = go.AddComponent<UDPSyncObject>();
            udpSyncComponent.isReceiver = true;
            udpSyncComponent.port = port;
            udpSyncComponent.Init();
            startingPort++;

            go.AddComponent<NetworkMaster>();

            //NetworkServer.Spawn(go);
            server.AddGameObjectInScenePart(hololensPrefabName, hololensPrefabName, go.transform.position, go.transform.rotation, remoteRootPlayerName, false);

        }
    }

    private void InstanciatePlayerParticipant(string name, int port)
    {
        Debug.Log("UDP Instaciate player participant call");
        GameObject goToInstanciate = Resources.Load(participantPrefabName) as GameObject;
        if (goToInstanciate != null)
        {
            Debug.Log("UDP Instaciate ");

            GameObject go = Instantiate(goToInstanciate, new Vector3(0, 0, 0), Quaternion.identity);
            //go.transform.name = name;
            go.transform.name = participantPrefabName;
            if (rootScene != null)
                go.transform.parent = rootPlayers.transform;

            UDPSyncObject udpSyncComponent = go.AddComponent<UDPSyncObject>();
            udpSyncComponent.isReceiver = true;
            udpSyncComponent.port = port;
            udpSyncComponent.Init();
            startingPort++;

            go.AddComponent<NetworkMaster>();

            //if(!server.useWebRTC)
            //    NetworkServer.Spawn(go);
            //else
                server.AddGameObjectInScenePart(participantPrefabName, participantPrefabName, go.transform.position, go.transform.rotation, remoteRootPlayerName, false);

        }
    }

    private void InstanciatePlayerKinect(string name, int port)
    {
        Debug.Log("UDP Instaciate player participant call");
        GameObject goToInstanciate = Resources.Load(kinectPrefabName) as GameObject;
        if (goToInstanciate != null)
        {
            Debug.Log("UDP Instaciate ");

            GameObject go = Instantiate(goToInstanciate, new Vector3(0, 0, 0), Quaternion.identity);
            //go.transform.name = name;
            go.transform.name = kinectPrefabName;
            if (rootScene != null)
                go.transform.parent = rootPlayers.transform;

            UDPSyncObject udpSyncComponent = go.AddComponent<UDPSyncObject>();
            udpSyncComponent.isReceiver = true;
            udpSyncComponent.port = port;
            udpSyncComponent.Init();
            startingPort++;

            go.AddComponent<NetworkMaster>();

            //NetworkServer.Spawn(go);
            server.AddGameObjectInScenePart(kinectPrefabName, kinectPrefabName, go.transform.position, go.transform.rotation, remoteRootPlayerName, false);
        }
    }

    private void InstanciateHandJoint(string laterality, string name, int port)
    {
        string goti = "";

        if (name.Contains("wrist")) { goti = laterality + "_wrist"; }
        if (name.Contains("palm")) { goti = laterality + "_palm"; }
        if (name.Contains("thumb_metacarpal")) { goti = laterality + "_thumb_metacarpal"; }
        if (name.Contains("thumb_proximal")) { goti = laterality + "_thumb_proximal"; }
        if (name.Contains("thumb_distal")) { goti = laterality + "_thumb_distal"; }
        if (name.Contains("thumb_tip")) { goti = laterality + "_thumb_tip"; }
        if (name.Contains("index_metacarpal")) { goti = laterality + "_index_metacarpal"; }
        if (name.Contains("index_knuckle")) { goti = laterality + "_index_knuckle"; }
        if (name.Contains("index_middle")) { goti = laterality + "_index_middle"; }
        if (name.Contains("index_distal")) { goti = laterality + "_index_distal"; }
        if (name.Contains("index_tip")) { goti = laterality + "_index_tip"; }
        if (name.Contains("middle_metacarpal")) { goti = laterality + "_middle_metacarpal"; }
        if (name.Contains("middle_knuckle")) { goti = laterality + "_middle_knuckle"; }
        if (name.Contains("middle_middle")) { goti = laterality + "_middle_middle"; }
        if (name.Contains("middle_distal")) { goti = laterality + "_middle_distal"; }
        if (name.Contains("middle_tip")) { goti = laterality + "_middle_tip"; }
        if (name.Contains("ring_metacarpal")) { goti = laterality + "_ring_metacarpal"; }
        if (name.Contains("ring_knuckle")) { goti = laterality + "_ring_knuckle"; }
        if (name.Contains("ring_middle")) { goti = laterality + "_ring_middle"; }
        if (name.Contains("ring_distal")) { goti = laterality + "_ring_distal"; }
        if (name.Contains("ring_tip")) { goti = laterality + "_ring_tip"; }
        if (name.Contains("pinky_metacarpal")) { goti = laterality + "_pinky_metacarpal"; }
        if (name.Contains("pinky_knuckle")) { goti = laterality + "_pinky_knuckle"; }
        if (name.Contains("pinky_middle")) { goti = laterality + "_pinky_middle"; }
        if (name.Contains("pinky_distal")) { goti = laterality + "_pinky_distal"; }
        if (name.Contains("pinky_tip")) { goti = laterality + "_pinky_tip"; }

        Debug.Log("name : " + name + " - goti : " + goti);

        GameObject goToInstanciate = Resources.Load(goti) as GameObject;
        Debug.Log("go : " + goToInstanciate);
        if (goToInstanciate != null)
        {
            GameObject go = Instantiate(goToInstanciate, new Vector3(0, 0, 0), Quaternion.identity);
            go.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            go.transform.name = name;
            if (rootScene != null)
                go.transform.parent = rootPlayers.transform;

            UDPSyncObject udpSyncComponent = go.AddComponent<UDPSyncObject>();
            udpSyncComponent.isReceiver = true;
            udpSyncComponent.port = port;
            udpSyncComponent.Init();
            startingPort++;

            go.AddComponent<NetworkMaster>();

            //NetworkServer.Spawn(go);
            server.AddGameObjectInScenePart(goti, name, go.transform.position, go.transform.rotation, remoteRootPlayerName, false);
        }
    }

    

    void ApplyColorsDeeply(Transform t, Color c)
    {
        Renderer rend = t.GetComponent<Renderer>();

        if (rend != null)
            for (int i = 0; i < rend.materials.Length; ++i)
                rend.materials[i].color = c;

        for (int i = 0; i < t.childCount; ++i)
            ApplyColorsDeeply(t.GetChild(i), c);
    }

    
    
    private void SendData(string action, string param)
    {
        try
        {
            string message = action + ";" + param;

            byte[] data = Encoding.UTF8.GetBytes(message);
            client.Send(data, data.Length, remoteEndPoint);
            Debug.Log("sendt message : " + message);
        }
        catch (Exception err)
        {
            Debug.LogError(err.ToString());
        }
    }

    private void ReceiveData(byte[] data)
    {
        //client = new UdpClient(port);

        //while (true)
        //{
        //try
        //    {
        //IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
        //byte[] data = client.Receive(ref anyIP);
        //byte[] data = client.EndReceive(res, ref anyIP);


        string text = Encoding.UTF8.GetString(data);
        string[] s = text.Split(';');

        Debug.Log("received message : " + text);

        if (s[0] == "add-go")
        {
            ReceiveAddGO(s);
        }
        else if (s[0] == "remove-go")
        {
            ReceiveRemoveGO(s);
        }
        else if (s[0] == "add-player")
        {
            ReceiveAddPlayer(s);
        }
        else if (s[0] == "remove-player")
        {
            ReceiveRemovePlayer(s);
        }
        else if (s[0] == "apply-color")
        {
            ReceiveApplyColor(s);
        }
        else if (s[0] == "update-component")
        {
            ReceiveUpdateComponent(s);
        }
        //else if (s[0] == "scene-part-add")
        //{
        //    ReceiveAddGOScenePart(s);
        //}
        //else if (s[0] == "scene-part-remove")
        //{
        //    ReceiveRemoveGOScenePart(s);
        //}
        else if (s[0] == "scene-part-command")
        {
            ReceiveScenePartCommand(s);
        }
        else if (s[0] == "network-event")
        {
            ReceiveNetworkEvent(s);
        }


        //}
        //catch (Exception err)
        //{
        //    Debug.LogError(err.ToString());
        //}
        //}
    }





    void ReceiveAddGO(string[] s)
    {
        lock (ToAddPortQueue)
            lock (ToAddQueue)
                LockedReceiveAddGO(s);
    }

    void ReceiveRemoveGO(string[] s)
    {
        lock (ToDeleteQueue)
            LockedReceiveRemoveGO(s);
    }

    void ReceiveAddPlayer(string[] s)
    {
        lock (ToAddPortPlayerQueue)
            lock (ToAddPlayerQueue)
                LockedReceiveAddPlayer(s);
    }

    void ReceiveRemovePlayer(string[] s)
    {
        lock (ToDeletePlayerQueue)
            LockedReceiveRemovePlayer(s);
    }

    void ReceiveApplyColor(string[] s)
    {
        lock (ToUpdateColorColorsRQueue)
            lock (ToUpdateColorColorsGQueue)
                lock (ToUpdateColorColorsBQueue)
                    lock (ToUpdateColorColorsAQueue)
                        lock (ToUpdateColorGOQueue)
                            LockedReceiveApplyColor(s);
    }

    void ReceiveUpdateComponent(string[] s)
    {
        lock (ToUpdateComponentsNamesQueue)
            lock (ToUpdateComponentsParamsQueue)
                lock (ToUpdateComponentsValuesQueue)
                    lock (ToCallAtStart)
                        lock (ToUpdateComponentsGOQueue)
                            lock (ToCallAtStartObjectsIndex)
                                LockedReceiveUpdateComponent(s);
    }

    //void ReceiveAddGOScenePart(string[] s)
    //{
    //    lock (ToAddPortScenePartQueue)
    //        lock (ToAddScenePartQueue)
    //            LockedReceiveAddGOScenePart(s);
    //}

    //void ReceiveRemoveGOScenePart(string[] s)
    //{
    //    lock (ToDeleteScenePartQueue)
    //        LockedReceiveRemoveGOScenePart(s);
    //}

    void ReceiveScenePartCommand(string[] s)
    {
        lock (scenePartCommand)
            LockedReceiveScenePartCommand(s);
    }

    void ReceiveNetworkEvent(string[] s)
    {
        lock (networkEvents)
            LockedReceiveNetworkEvent(s);
    }




    void LockedReceiveAddGO(string[] s)
    {
        ToAddPortQueue.Add(Int32.Parse(s[1]));
        
        if (s.Length > 3)
            ToAddQueue.Add(s[2] + ";" + s[3]);
        else
            ToAddQueue.Add(s[2]);
    }

    void LockedReceiveRemoveGO(string[] s)
    {
        ToDeleteQueue.Add(s[1]);
    }

    void LockedReceiveAddPlayer(string[] s)
    {
        ToAddPortPlayerQueue.Add(Int32.Parse(s[1]));
        ToAddPlayerQueue.Add(s[2]);
    }

    void LockedReceiveRemovePlayer(string[] s)
    {
        ToDeletePlayerQueue.Add(s[1]);
    }

    void LockedReceiveApplyColor(string[] s)
    {
        ToUpdateColorColorsRQueue.Add(float.Parse(s[2]));
        ToUpdateColorColorsGQueue.Add(float.Parse(s[3]));
        ToUpdateColorColorsBQueue.Add(float.Parse(s[4]));
        ToUpdateColorColorsAQueue.Add(float.Parse(s[5]));
        ToUpdateColorGOQueue.Add(s[1]); // at the end to indicate all data have been received
    }

    void LockedReceiveUpdateComponent(string[] s)
    {
        ToUpdateComponentsNamesQueue.Add(s[2]);

        int nbParam = int.Parse(s[3]);

        Debug.Log("update component");
        Debug.Log("\tgo : " + s[1]);
        Debug.Log("\tcomponent name : " + s[2]);

        for (int i = 0; i < nbParam; ++i)
        {
            string type = s[4 + 3 * i + 1];

            if (AllowedTypes.Contains(type))
            {
                string value = s[4 + 3 * i + 2];
                object typedValue = GetTypedValue(type, value);

                ToUpdateComponentsParamsQueue.Add(s[4 + 3 * i]);
                ToUpdateComponentsValuesQueue.Add(typedValue);

                Debug.Log("\t" + s[4 + 3 * i] + " = " + typedValue);
            }
        }

        int nbCallAtStart = int.Parse(s[4 + 3 * nbParam]);

        for (int i = 0; i < nbCallAtStart; ++i)
        {

            ToCallAtStart.Add(s[4 + 3 * nbParam + 1 + i]);
            ToCallAtStartObjectsIndex.Add(ToUpdateComponentsGOQueue.Count);
            Debug.Log("method to call at start = " + s[4 + 3 * nbParam + 1 + i]);
        }


        ToUpdateComponentsGOQueue.Add(s[1]); // at the end to indicate all data have been received
    }

    //void LockedReceiveAddGOScenePart(string[] s)
    //{
    //    ToAddPortScenePartQueue.Add(Int32.Parse(s[1]));

    //    if (s.Length > 3)
    //        ToAddScenePartQueue.Add(s[2] + ";" + s[3]);
    //    else
    //        ToAddScenePartQueue.Add(s[2]);
    //}

    //void LockedReceiveRemoveGOScenePart(string[] s)
    //{
    //    ToDeleteScenePartQueue.Add(s[1]);
    //}

    void LockedReceiveScenePartCommand(string[] s)
    {
        scenePartCommand.Add(s[1]);
    }

    void LockedReceiveNetworkEvent(string[] s)
    {
        networkEvents.Add(s[1]);
    }





    object GetTypedValue(string type, string value)
    {
        // type from allowed type param
        if (type == "System.Boolean") { return bool.Parse(value); }
        if (type == "System.Int16") { return int.Parse(value); }
        if (type == "System.UInt16") { return int.Parse(value); }
        if (type == "System.Int32") { return int.Parse(value); }
        if (type == "System.UInt32") { return int.Parse(value); }
        if (type == "System.Int64") { return int.Parse(value); }
        if (type == "System.UInt64") { return int.Parse(value); }
        if (type == "System.Single") { return Single.Parse(value); }
        if (type == "System.Double") { return double.Parse(value); }
        if (type == "System.String") { return value; }

        return value;
    }

    //public void AddGameObjectScenePartManager(string nameGO)
    //{
    //    ScheduleSender("scene-part-add", nameGO);
    //}

    //public void RemoveGameObjectScenePartManager(string nameGO)
    //{
    //    ScheduleSender("scene-part-remove", nameGO);
    //}

    public void SendSceneManagerCommand(string command)
    {
        ScheduleSender("scene-part-command", command);
    }
}
