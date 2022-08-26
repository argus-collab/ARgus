using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

using UnityEngine.Networking;
using Unity.Collections;

using UnityEngine.Networking.Match;


using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;
using Microsoft.MixedReality.Toolkit.Utilities;

public class GameObjectLog
{
    public GameObject go;

    public List<Vector3> askedPositions;
    public List<Vector3> realPositions;
    public List<float> timeAskedPositions;
    public List<float> timeRealPositions;


    public GameObjectLog()
    {
        askedPositions = new List<Vector3>();
        realPositions = new List<Vector3>();

        timeAskedPositions = new List<float>();
        timeRealPositions = new List<float>();
    }
}

public class SceneLog
{
    List<GameObjectLog> gos;

    public SceneLog()
    {
        gos = new List<GameObjectLog>();
    }

    public void addLogRealPositionOnGo(GameObject go, Vector3 p, float t)
    {
        bool found = false;
        for (int i = 0; !found && i < gos.Count; ++i)
        {
            if(gos[i].go == go)
            {
                gos[i].realPositions.Add(p);
                gos[i].timeRealPositions.Add(t);

                found = true;
            }
        }
        
        if(!found)
        {
            GameObjectLog log = new GameObjectLog();
            
            log.go = go;
            log.realPositions.Add(p);
            log.timeRealPositions.Add(t);

            gos.Add(log);
        }
    }

    public void addLogAskedPositionOnGo(GameObject go, Vector3 p, float t)
    {
        bool found = false;
        for (int i=0; !found && i < gos.Count; ++i)
        {
            if (gos[i].go == go)
            {
                gos[i].askedPositions.Add(p);
                gos[i].timeAskedPositions.Add(t);

                found = true;
            }
        }

        if (!found)
        {
            GameObjectLog log = new GameObjectLog();

            log.go = go;
            log.askedPositions.Add(p);
            log.timeAskedPositions.Add(t);

            gos.Add(log);
        }
    }

    public void writeCSVFile()
    {
        TextWriter csvAsked = new StreamWriter("debug_asked.csv");

        string str = "";

        // legend
        str += "go name;";
        str += "x asked;y asked;z asked;t asked";
        str += "\n";

        // data
        for (int i = 0; i < gos.Count; ++i)
        {
            for (int j = 0; j < gos[i].askedPositions.Count; ++j)
            {
                str += gos[i].go.name;
                str += ";";
                str += gos[i].askedPositions[j].x;
                str += ";";
                str += gos[i].askedPositions[j].y;
                str += ";";
                str += gos[i].askedPositions[j].z;
                str += ";";
                str += gos[i].timeAskedPositions[j];
                str += "\n";
            }
        }

        csvAsked.WriteLine(str);
        csvAsked.Close();



        TextWriter csvReal = new StreamWriter("debug_real.csv");

        str = "";

        // legend
        str += "go name;";
        str += "x real;y real;z real;t real";
        str += "\n";

        // data
        for (int i = 0; i < gos.Count; ++i)
        {
            for (int j = 0; j < gos[i].realPositions.Count; ++j)
            {
                str += gos[i].go.name;
                str += ";";
                str += gos[i].realPositions[j].x;
                str += ";";
                str += gos[i].realPositions[j].y;
                str += ";";
                str += gos[i].realPositions[j].z;
                str += ";";
                str += gos[i].timeRealPositions[j];
                str += "\n";
            }
        }
        
        csvReal.WriteLine(str);
        csvReal.Close();
    }
}

public class CustomServerNetworkManager : NetworkManager
{
    [Header("Options")]
    public bool useInternet = true;
    public bool isHost = false;
    public bool getClientLatency = false;
    public bool showStats = false;
    public int sceneIndex = 0;
    public bool loadScene = false;

    private bool previousShowStats = false;

    [Header("Custom parameters")]
    public GameObject meshRoot;
    public Material meshMaterial;
    public GameObject playersGO;
    public GameObject sceneGO;
    public List<string> scenesNames;
    public string GONameAddition = "Client";
    public LogManager logger;
    public ScenePartManager mainScenePartManager;


    [ReadOnly]
    public List<NetworkConnection> clients;

    private List<GameObject> playersGOList;
    private List<NetworkConnection> playersConnectionsList;

    // debug
    public bool exportInFile = false;
    private SceneLog export;


    // history
    public bool loadHistoryAtConnection = true;
    private List<int> msgTypes;
    private List<MessageBase> msgs;
    public List<string> exceptions = new List<string>()
        {
            "3DMouseViewPointSphere",
            "Cursor",
            "FixedStick",
            "GrowingStick",
            "TopStick"
        };

    //private List<int> channelIds;

    public WebRTCUnetMapperServer webrtc;
    public bool useWebRTC = false;
    public bool fullWebRTC = false;

    void RecordInForAllHistory(int msgType, MessageBase msg)
    {
        // don't record if prefab given as exception
        if (msgType == 9982 && exceptions.Contains(((SceneGameObjectInstanciateMessage)msg).namePrefab))
            return;

        msgTypes.Add(msgType);
        msgs.Add(msg);
    }

    void LoadForAllHistory(int connectionId)
    {
        Debug.Log("Load history");
        for (int i = 0; i < msgs.Count; ++i)
        {
            Debug.Log(msgs[i].ToString());
            NetworkServer.SendToClient(connectionId, (short)msgTypes[i], msgs[i]);
        }
    }

    void LoadSpawnableObjectsList()
    {
        UnityEngine.Object[] toAdd = Resources.LoadAll("", typeof(GameObject));
        for (int i = 0; i < toAdd.Length; ++i)
        {
            GameObject toAddGO = (GameObject)toAdd[i];
            if (toAddGO.GetComponent<NetworkIdentity>() != null)
                spawnPrefabs.Add((GameObject)toAdd[i]);
            //Debug.Log("add to spawnable list : " + toAdd[i].name);
        }
    }


    //this method is called when your request for creating a match is returned
    private void OnInternetMatchCreate(bool success, string extendedInfo, MatchInfo matchInfo)
    {
        if (success)
        {
            //Debug.Log("Create match succeeded");

            Debug.Log("Create match succeeded");
            Debug.Log("> ip : " + matchInfo.address);
            Debug.Log("> port : " + matchInfo.port);
            Debug.Log("> domain : " + matchInfo.domain);

            MatchInfo hostInfo = matchInfo;
            NetworkServer.Listen(hostInfo, 9000);

            if (isHost)
                NetworkManager.singleton.StartHost(hostInfo);
            else
                StartServer(matchInfo);

            ManageHandler();
        }
        else
        {
            Debug.LogError("Create match failed");
        }
    }

    void ManageHandler()
    {
        NetworkServer.RegisterHandler((short)9999, OnAskingForTransform);
        NetworkServer.RegisterHandler((short)9998, OnRigidBodyStateUpdate);
        NetworkServer.RegisterHandler((short)9996, OnReceivedStringMessage);
        NetworkServer.RegisterHandler((short)9989, OnReceivedTimeStampMessage);
        NetworkServer.RegisterHandler((short)9988, OnReceivedMeshMessage);
        NetworkServer.RegisterHandler((short)9984, OnReceivedHandModelMessage);
        NetworkServer.RegisterHandler((short)9983, OnReceivedSceneGameObjectMessage);
        NetworkServer.RegisterHandler((short)9977, OnReceivedCommandMessage);
        NetworkServer.RegisterHandler((short)9975, OnAskForPrefabInstantiate);
        NetworkServer.RegisterHandler((short)9974, OnAskForPrefabSpawn);
        NetworkServer.RegisterHandler((short)9973, OnAskForPlayerDataUpdate);
        NetworkServer.RegisterHandler((short)9972, OnAskForLogEntry);
        NetworkServer.RegisterHandler((short)9970, OnAskForIp);

        NetworkServer.RegisterHandler((short)9982, OnAskForGameObjectInstanciateMessage);
        NetworkServer.RegisterHandler((short)9980, OnAskForGameObjectRemoveMessage);

    }

    void Start()
    {
        if(!fullWebRTC)
        {
            LoadSpawnableObjectsList();

            if (useInternet)
            {
                NetworkManager.singleton.StartMatchMaker();
                NetworkManager.singleton.matchMaker.CreateMatch(matchName, 4, true, "", "", "", 0, 0, OnInternetMatchCreate);
            }
            else
            {
                NetworkManager.singleton.StartServer();
                ManageHandler();
            }

        }

        clients = new List<NetworkConnection>();
        playersGOList = new List<GameObject>();
        playersConnectionsList = new List<NetworkConnection>();

        export = new SceneLog();

        msgTypes = new List<int>();
        msgs = new List<MessageBase>();
        //channelIds = new List<int>();
    }

    public void LoadScene(int i)
    {
        Debug.Log("load scene " + i);
        ServerChangeScene(scenesNames[i]);
    }

    public void LoadNextScene()
    {
        sceneIndex = (sceneIndex + 1) % scenesNames.Count;
        LoadScene(sceneIndex);
    }

    void Update()
    {
        if (loadScene)
        {
            loadScene = false;
            LoadScene(sceneIndex);
        }

        if (showStats != previousShowStats)
        {
            previousShowStats = showStats;

            UIStateMessage msg = new UIStateMessage();
            msg.displayStat = showStats;
            RecordInForAllHistory((short)9997, msg);
            NetworkServer.SendByChannelToAll((short)9997, msg, 2);
        }

        if (exportInFile)
        {
            exportInFile = false;

            export.writeCSVFile();
        }

        if (getClientLatency)
        {
            SendTimeStampToAll();
        }
    }

    public void OnReceivedTimeStampMessage(NetworkMessage netMsg)
    {
        TimeStampMessage msg = netMsg.ReadMessage<TimeStampMessage>();
        float receivedTimeStamp = msg.timestamp;
        Debug.Log("received message from " + netMsg.conn.address + " : " + receivedTimeStamp);
        Debug.Log("diff from cur and received timestamp : " + (Time.time - receivedTimeStamp));
    }


    public override void OnServerConnect(NetworkConnection conn)
    {
        clients.Add(conn);
        Debug.Log("client connected at " + conn.address);

        if (loadHistoryAtConnection)
            LoadForAllHistory(conn.connectionId);
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        clients.Remove(conn);
        RemovePlayer(conn);
        Debug.Log("client removed at " + conn.address);

    }

    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
        //base.OnServerAddPlayer(conn, playerControllerId);

        GameObject player = (GameObject)Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);

        if (playerPrefab.name == "HololensRepresentation")
            player.name = "Head";
        if (playerPrefab.name == "ParticipantRepresentation")
            player.name = "Participant";

        player.layer = 8;
        GameObject playerCoordinateSystem = new GameObject(conn.address);
        playerCoordinateSystem.layer = 8;

        if (playersConnectionsList.Contains(conn))
        {
            // scene change
            RemovePlayer(conn);
        }

        playersGOList.Add(playerCoordinateSystem);
        playersConnectionsList.Add(conn);

        player.transform.parent = playerCoordinateSystem.transform;

        if (playersGO)
            playerCoordinateSystem.transform.parent = playersGO.transform;

        NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);

        IpAdressMessage msg = new IpAdressMessage();
        msg.address = conn.address;
        NetworkServer.SendToClient(conn.connectionId, (short)9970, msg);
    }

    private void RemovePlayer(NetworkConnection conn)
    {
        int i = playersConnectionsList.IndexOf(conn);

        GameObject toDelete = playersGOList[i];
        playersGOList.RemoveAt(i);
        playersConnectionsList.RemoveAt(i);

        Destroy(toDelete);
    }

    public override void OnServerRemovePlayer(NetworkConnection conn, PlayerController player)
    {
        RemovePlayer(conn);
    }

    public override void OnStopServer()
    {
        //base.OnStopServer();
        //GameObject players = GameObject.Find("Players");

        for (int i = 0; i < playersGOList.Count; ++i)
            Destroy(playersGOList[i].gameObject);
    }

    // TODO : make static all that can be static

    public void OnReceivedCommandMessage(NetworkMessage netMsg)
    {
        OnReceivedCommandMessage(netMsg.ReadMessage<CommandMessage>());
    }
    public void OnReceivedCommandMessage(CommandMessage msg)
    {
        switch (msg.command)
        {
            case "change-color-state":
                ServerColorManager colorMan = GameObject.FindObjectOfType<ServerColorManager>();
                colorMan.ChangeColorState();
                break;
            case "scene-manager-command":
                mainScenePartManager.TextualCommand(msg.args);
                break;
            case "scene-manager-command-remote":
                UDPSceneManagerBackAndForth udpMan = GameObject.FindObjectOfType<UDPSceneManagerBackAndForth>();
                udpMan.SendSceneManagerCommand(msg.args);
                break;
            default:
                Debug.LogError("unknow command");
                break;
        }

    }

    public void OnReceivedStringMessage(NetworkMessage netMsg)
    {
        StringMessage msg = netMsg.ReadMessage<StringMessage>();
        Debug.Log("received message from " + netMsg.conn.address + " : " + msg.text);
    }

    public void OnReceivedMeshMessage(NetworkMessage netMsg)
    {
        MeshMessage msg = netMsg.ReadMessage<MeshMessage>();

        //Debug.Log("receive mesh message : " + msg.name + " - " + msg.mesh.Length);
        //Debug.Log("p : " + msg.p + " - q : " + msg.q);

        GameObject newMesh = GameObject.Find(msg.name);
        MeshFilter mshF;

        if (newMesh == null)
        {
            newMesh = new GameObject(msg.name);
            mshF = newMesh.AddComponent<MeshFilter>();
            MeshRenderer mshR = newMesh.AddComponent<MeshRenderer>();
            mshR.material = meshMaterial;

            if (meshRoot != null)
                newMesh.transform.parent = meshRoot.transform;
        }
        else
        {
            mshF = newMesh.GetComponent<MeshFilter>();
        }


        newMesh.transform.localPosition = msg.p;
        newMesh.transform.localRotation = msg.q;

        mshF.mesh.Clear();
        mshF.mesh = MeshSerializer.ReadMesh(msg.mesh);

        mshF.mesh.RecalculateBounds();

    }

    // object synchronisation
    public void OnAskingForTransform(NetworkMessage netMsg)
    {
        TransformMessage msg = netMsg.ReadMessage<TransformMessage>();

        GameObject go = NetworkServer.FindLocalObject(msg.id);
        if (go != null)
        {
            NetworkSyncObject o = go.GetComponent<NetworkSyncObject>();
            if (o != null)
            {
                o.RequestTransform(msg.p, msg.q);

                export.addLogAskedPositionOnGo(go, msg.p, Time.realtimeSinceStartup);
            }
        }
    }

    public void OnRigidBodyStateUpdate(NetworkMessage netMsg)
    {
        RigidBodyStateMessage msg = netMsg.ReadMessage<RigidBodyStateMessage>();

        GameObject go = NetworkServer.FindLocalObject(msg.id);
        if (go != null)
        {
            NetworkSyncObject o = go.GetComponent<NetworkSyncObject>();
            if (o != null)
                o.UpdateRigidBodyState(msg.isKinematic);
        }
    }

    // game object name dependant
    public void OnReceivedSceneGameObjectMessage(NetworkMessage netMsg)
    {
        OnReceivedSceneGameObjectMessage(netMsg.ReadMessage<SceneGameObjectMessage>());
    }
    public void OnReceivedSceneGameObjectMessage(SceneGameObjectMessage msg)
    {
        GameObject go = GameObject.Find(msg.name);
        if (go != null)
        {
            if (msg.local)
            {
                go.transform.localPosition = msg.position;
                go.transform.localRotation = msg.rotation;
                go.transform.localScale = msg.scale;
            }
            else
            {
                go.transform.position = msg.position;
                go.transform.rotation = msg.rotation;
                go.transform.localScale = msg.scale;
            }
        }
    }

    public void CreateGameObject(string name, Vector3 p, Quaternion q, Transform parent)
    {
        GameObject newGO = GameObject.CreatePrimitive(PrimitiveType.Cube); //new GameObject(name); // 
        newGO.name = name;
        newGO.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
        newGO.transform.parent = parent;
        newGO.transform.localPosition = p;
        newGO.transform.localRotation = q;
        newGO.layer = 8;
    }

    public void UpdateGameObject(string name, Vector3 p, Quaternion q, Transform parent)
    {
        GameObject joint = parent.Find(name).gameObject;
        joint.transform.localPosition = p;
        joint.transform.localRotation = q;
    }

    public void OnReceivedHandModelMessage(NetworkMessage netMsg)
    {
        string[] handJointsNames = new string[26] {"wrist", "palm", "thumb_metacarpal", "thumb_proximal",
            "thumb_distal", "thumb_tip", "index_metacarpal", "index_knuckle", "index_middle",
            "index_distal", "index_tip", "middle_metacarpal", "middle_knuckle", "middle_middle",
            "middle_distal", "middle_tip", "ring_metacarpal", "ring_knuckle", "ring_middle",
            "ring_distal", "ring_tip", "pinky_metacarpal", "pinky_knuckle", "pinky_middle", "pinky_distal", "pinky_tip"};

        HandModelMessage msg = netMsg.ReadMessage<HandModelMessage>();

        GameObject hand = GameObject.Find(msg.handedness + "_hand");

        //Debug.Log("received wrist position : " + msg.v_wrist);

        if (hand == null)
        {
            hand = new GameObject(msg.handedness + "_hand");

            // we assume just one cplayer connection when hand are retrieved
            hand.transform.parent = playersGOList[0].transform;

            CreateGameObject("wrist", msg.v_wrist, msg.q_wrist, hand.transform);
            CreateGameObject("palm", msg.v_palm, msg.q_palm, hand.transform);
            CreateGameObject("thumb_metacarpal", msg.v_thumb_metacarpal, msg.q_thumb_metacarpal, hand.transform);
            CreateGameObject("thumb_proximal", msg.v_thumb_proximal, msg.q_thumb_proximal, hand.transform);
            CreateGameObject("thumb_distal", msg.v_thumb_distal, msg.q_thumb_distal, hand.transform);
            CreateGameObject("thumb_tip", msg.v_thumb_tip, msg.q_thumb_tip, hand.transform);
            CreateGameObject("index_metacarpal", msg.v_index_metacarpal, msg.q_index_metacarpal, hand.transform);
            CreateGameObject("index_knuckle", msg.v_index_knuckle, msg.q_index_knuckle, hand.transform);
            CreateGameObject("index_middle", msg.v_index_middle, msg.q_index_middle, hand.transform);
            CreateGameObject("index_distal", msg.v_index_distal, msg.q_index_distal, hand.transform);
            CreateGameObject("index_tip", msg.v_index_tip, msg.q_index_tip, hand.transform);
            CreateGameObject("middle_metacarpal", msg.v_middle_metacarpal, msg.q_middle_metacarpal, hand.transform);
            CreateGameObject("middle_knuckle", msg.v_middle_knuckle, msg.q_middle_knuckle, hand.transform);
            CreateGameObject("middle_middle", msg.v_middle_middle, msg.q_middle_middle, hand.transform);
            CreateGameObject("middle_distal", msg.v_middle_distal, msg.q_middle_distal, hand.transform);
            CreateGameObject("middle_tip", msg.v_middle_tip, msg.q_middle_tip, hand.transform);
            CreateGameObject("ring_metacarpal", msg.v_ring_metacarpal, msg.q_ring_metacarpal, hand.transform);
            CreateGameObject("ring_knuckle", msg.v_ring_knuckle, msg.q_ring_knuckle, hand.transform);
            CreateGameObject("ring_middle", msg.v_ring_middle, msg.q_ring_middle, hand.transform);
            CreateGameObject("ring_distal", msg.v_ring_distal, msg.q_ring_distal, hand.transform);
            CreateGameObject("ring_tip", msg.v_ring_tip, msg.q_ring_tip, hand.transform);
            CreateGameObject("pinky_metacarpal", msg.v_pinky_metacarpal, msg.q_pinky_metacarpal, hand.transform);
            CreateGameObject("pinky_knuckle", msg.v_pinky_knuckle, msg.q_pinky_knuckle, hand.transform);
            CreateGameObject("pinky_middle", msg.v_pinky_middle, msg.q_pinky_middle, hand.transform);
            CreateGameObject("pinky_distal", msg.v_pinky_distal, msg.q_pinky_distal, hand.transform);
            CreateGameObject("pinky_tip", msg.v_pinky_tip, msg.q_pinky_tip, hand.transform);

            CreateHandConsistency(hand.transform);
            //Debug.Log("msg.GetType() = " + msg.GetType());
            //Debug.Log("msg.GetType().GetProperty(\"v_wrist\") = " + msg.GetType().GetProperty("v_wrist"));

            //for(int i=0;i< msg.GetType().GetProperties().Length;++i)
            //{
            //    Debug.Log(msg.GetType().GetProperties()[i].Name);
            //}

            //Debug.Log("msg.GetType().GetProperty(\"v_wrist\") = " + msg.GetType().GetProperties().Length);
            //Debug.Log("msg.GetType().GetProperty(\"v_wrist\").GetValue(msg) = " + msg.GetType().GetProperty("v_wrist").GetValue(msg));

            //for (int i = 0; i < handJointsNames.Length; ++i)
            //{
            //    GameObject newGO = new GameObject(handJointsNames[i]);
            //    newGO.transform.parent = hand.transform;
            //    newGO.transform.localPosition = (Vector3) msg.GetType().GetProperty("v_" + handJointsNames[i]).GetValue(msg);
            //    newGO.transform.localRotation = (Quaternion) msg.GetType().GetProperty("q_" + handJointsNames[i]).GetValue(msg);
            //}
        }
        else
        {
            UpdateGameObject("wrist", msg.v_wrist, msg.q_wrist, hand.transform);
            UpdateGameObject("palm", msg.v_palm, msg.q_palm, hand.transform);
            UpdateGameObject("thumb_metacarpal", msg.v_thumb_metacarpal, msg.q_thumb_metacarpal, hand.transform);
            UpdateGameObject("thumb_proximal", msg.v_thumb_proximal, msg.q_thumb_proximal, hand.transform);
            UpdateGameObject("thumb_distal", msg.v_thumb_distal, msg.q_thumb_distal, hand.transform);
            UpdateGameObject("thumb_tip", msg.v_thumb_tip, msg.q_thumb_tip, hand.transform);
            UpdateGameObject("index_metacarpal", msg.v_index_metacarpal, msg.q_index_metacarpal, hand.transform);
            UpdateGameObject("index_knuckle", msg.v_index_knuckle, msg.q_index_knuckle, hand.transform);
            UpdateGameObject("index_middle", msg.v_index_middle, msg.q_index_middle, hand.transform);
            UpdateGameObject("index_distal", msg.v_index_distal, msg.q_index_distal, hand.transform);
            UpdateGameObject("index_tip", msg.v_index_tip, msg.q_index_tip, hand.transform);
            UpdateGameObject("middle_metacarpal", msg.v_middle_metacarpal, msg.q_middle_metacarpal, hand.transform);
            UpdateGameObject("middle_knuckle", msg.v_middle_knuckle, msg.q_middle_knuckle, hand.transform);
            UpdateGameObject("middle_middle", msg.v_middle_middle, msg.q_middle_middle, hand.transform);
            UpdateGameObject("middle_distal", msg.v_middle_distal, msg.q_middle_distal, hand.transform);
            UpdateGameObject("middle_tip", msg.v_middle_tip, msg.q_middle_tip, hand.transform);
            UpdateGameObject("ring_metacarpal", msg.v_ring_metacarpal, msg.q_ring_metacarpal, hand.transform);
            UpdateGameObject("ring_knuckle", msg.v_ring_knuckle, msg.q_ring_knuckle, hand.transform);
            UpdateGameObject("ring_middle", msg.v_ring_middle, msg.q_ring_middle, hand.transform);
            UpdateGameObject("ring_distal", msg.v_ring_distal, msg.q_ring_distal, hand.transform);
            UpdateGameObject("ring_tip", msg.v_ring_tip, msg.q_ring_tip, hand.transform);
            UpdateGameObject("pinky_metacarpal", msg.v_pinky_metacarpal, msg.q_pinky_metacarpal, hand.transform);
            UpdateGameObject("pinky_knuckle", msg.v_pinky_knuckle, msg.q_pinky_knuckle, hand.transform);
            UpdateGameObject("pinky_middle", msg.v_pinky_middle, msg.q_pinky_middle, hand.transform);
            UpdateGameObject("pinky_distal", msg.v_pinky_distal, msg.q_pinky_distal, hand.transform);
            UpdateGameObject("pinky_tip", msg.v_pinky_tip, msg.q_pinky_tip, hand.transform);
        }
    }

    public void UpdateSyncObjectTransform(NetworkInstanceId id, Vector3 p, Quaternion q, bool isLocal)
    {
        if (useWebRTC)
        {
            webrtc.WebRTCUpdateSyncObjectTransform(id, p, q, isLocal);
            return;
        }

        TransformMessage msg = new TransformMessage();

        msg.id = id;
        msg.p = p;
        msg.q = q;
        msg.isLocal = isLocal;

        RecordInForAllHistory((short)9999, msg);
        NetworkServer.SendByChannelToAll((short)9999, msg, 2);

        GameObject go = NetworkServer.FindLocalObject(msg.id);
        if (go != null)
            export.addLogRealPositionOnGo(go, msg.p, Time.realtimeSinceStartup);
    }

    public void CreateHandConsistency(Transform hand)
    {
        CreatePalmConsistency(hand);

        CreateThumbConsistency(hand);
        CreateIndexConsistency(hand);
        CreateMiddleConsistency(hand);
        CreateRingConsistency(hand);
        CreatePinkyConsistency(hand);
    }

    public void CreatePhalange(string name, GameObject top, GameObject bottom)
    {
        float phalangeLength = (top.transform.position - bottom.transform.position).magnitude * 1 / bottom.transform.localScale.magnitude;

        GameObject phalange = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        phalange.name = name;
        phalange.layer = 8;
        phalange.transform.parent = bottom.transform;
        phalange.transform.localPosition = new Vector3(0.0f, 0.0f, phalangeLength);
        phalange.transform.localRotation = Quaternion.Euler(90.0f, 0.0f, 0.0f);
        phalange.transform.parent = bottom.transform;
        phalange.transform.localScale = new Vector3(1.5f, phalangeLength, 1.5f);
    }

    public void CreatePalmPart(GameObject top, GameObject bottom)
    {
        float palmPartLength = (top.transform.position - bottom.transform.position).magnitude * 1 / bottom.transform.localScale.magnitude;

        GameObject palmPart = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        palmPart.name = "palm_part";
        palmPart.layer = 8;
        palmPart.transform.parent = bottom.transform;
        palmPart.transform.localPosition = new Vector3(0.0f, 0.0f, palmPartLength);
        palmPart.transform.localRotation = Quaternion.Euler(90.0f, 0.0f, 0.0f);
        palmPart.transform.parent = bottom.transform;
        palmPart.transform.localScale = new Vector3(2.5f, palmPartLength, 1f);
    }

    public void CreatePalmConsistency(Transform parent)
    {
        GameObject thumb_metacarpal = parent.Find("thumb_metacarpal").gameObject;
        GameObject thumb_proximal = parent.Find("thumb_proximal").gameObject;
        CreatePalmPart(thumb_proximal, thumb_metacarpal);

        GameObject index_metacarpal = parent.Find("index_metacarpal").gameObject;
        GameObject index_knuckle = parent.Find("index_knuckle").gameObject;
        CreatePalmPart(index_knuckle, index_metacarpal);

        GameObject middle_metacarpal = parent.Find("middle_metacarpal").gameObject;
        GameObject middle_knuckle = parent.Find("middle_knuckle").gameObject;
        CreatePalmPart(middle_knuckle, middle_metacarpal);

        GameObject ring_metacarpal = parent.Find("ring_metacarpal").gameObject;
        GameObject ring_knuckle = parent.Find("ring_knuckle").gameObject;
        CreatePalmPart(ring_knuckle, ring_metacarpal);

        GameObject pinky_metacarpal = parent.Find("pinky_metacarpal").gameObject;
        GameObject pinky_knuckle = parent.Find("pinky_knuckle").gameObject;
        CreatePalmPart(pinky_knuckle, pinky_metacarpal);
    }

    public void CreateThumbConsistency(Transform parent)
    {
        GameObject thumb_proximal = parent.Find("thumb_proximal").gameObject;
        GameObject thumb_distal = parent.Find("thumb_distal").gameObject;
        GameObject thumb_tip = parent.Find("thumb_tip").gameObject;

        CreatePhalange("distal_phalange", thumb_tip, thumb_distal);
        CreatePhalange("proximal_phalange", thumb_distal, thumb_proximal);
    }

    public void CreateIndexConsistency(Transform parent)
    {
        GameObject index_knuckle = parent.Find("index_knuckle").gameObject;
        GameObject index_middle = parent.Find("index_middle").gameObject;
        GameObject index_distal = parent.Find("index_distal").gameObject;
        GameObject index_tip = parent.Find("index_tip").gameObject;

        CreatePhalange("distal_phalange", index_tip, index_distal);
        CreatePhalange("intermediate_phalange", index_distal, index_middle);
        CreatePhalange("proximal_phalange", index_middle, index_knuckle);
    }

    public void CreateMiddleConsistency(Transform parent)
    {
        GameObject middle_knuckle = parent.Find("middle_knuckle").gameObject;
        GameObject middle_middle = parent.Find("middle_middle").gameObject;
        GameObject middle_distal = parent.Find("middle_distal").gameObject;
        GameObject middle_tip = parent.Find("middle_tip").gameObject;

        CreatePhalange("distal_phalange", middle_tip, middle_distal);
        CreatePhalange("intermediate_phalange", middle_distal, middle_middle);
        CreatePhalange("proximal_phalange", middle_middle, middle_knuckle);
    }

    public void CreateRingConsistency(Transform parent)
    {
        GameObject ring_knuckle = parent.Find("ring_knuckle").gameObject;
        GameObject ring_middle = parent.Find("ring_middle").gameObject;
        GameObject ring_distal = parent.Find("ring_distal").gameObject;
        GameObject ring_tip = parent.Find("ring_tip").gameObject;

        CreatePhalange("distal_phalange", ring_tip, ring_distal);
        CreatePhalange("intermediate_phalange", ring_distal, ring_middle);
        CreatePhalange("proximal_phalange", ring_middle, ring_knuckle);
    }

    public void CreatePinkyConsistency(Transform parent)
    {
        GameObject pinky_knuckle = parent.Find("pinky_knuckle").gameObject;
        GameObject pinky_middle = parent.Find("pinky_middle").gameObject;
        GameObject pinky_distal = parent.Find("pinky_distal").gameObject;
        GameObject pinky_tip = parent.Find("pinky_tip").gameObject;

        CreatePhalange("distal_phalange", pinky_tip, pinky_distal);
        CreatePhalange("intermediate_phalange", pinky_distal, pinky_middle);
        CreatePhalange("proximal_phalange", pinky_middle, pinky_knuckle);
    }

    public void UpdateVisibility(NetworkConnection conn, NetworkInstanceId id, bool visibility)
    {
        VisibilityMessage msg = new VisibilityMessage();

        msg.id = id;
        msg.visibility = visibility;

        NetworkServer.SendToClient(conn.connectionId, (short)9995, msg);
    }

    // working with names only 
    // BE CAREFUL for gameobjects with same name
    public void UpdateHierarchyForAll(string nameParent, NetworkInstanceId idChild)
    {
        HierarchyMessage msg = new HierarchyMessage();

        msg.nameParent = nameParent;
        msg.idChild = idChild;

        RecordInForAllHistory((short)9994, msg);
        NetworkServer.SendByChannelToAll((short)9994, msg, 0);

    }

    public void UpdateColorForAll(NetworkInstanceId id, Color color)
    {
        ColorMessage msg = new ColorMessage();

        msg.id = id;
        msg.color = color;

        RecordInForAllHistory((short)9993, msg);
        NetworkServer.SendByChannelToAll((short)9993, msg, 0);
    }

    public void UpdateNameForAll(NetworkInstanceId id, string name)
    {
        NameMessage msg = new NameMessage();

        msg.id = id;
        msg.name = name;

        RecordInForAllHistory((short)9992, msg);
        NetworkServer.SendByChannelToAll((short)9992, msg, 0);
    }

    public void SendTextMessageForAll(string text)
    {
        StringMessage msg = new StringMessage();

        msg.text = text;

        RecordInForAllHistory((short)9996, msg);
        NetworkServer.SendByChannelToAll((short)9996, msg, 0);
    }

    public void SendTimeMessageForAll(int time)
    {
        TimeMessage msg = new TimeMessage();

        msg.time = time;

        RecordInForAllHistory((short)9991, msg);
        NetworkServer.SendByChannelToAll((short)9991, msg, 0);
    }

    public void SendChronoLaunchMessageForAll()
    {
        LaunchMessage msg = new LaunchMessage();

        msg.launch = true;

        RecordInForAllHistory((short)9990, msg);
        NetworkServer.SendByChannelToAll((short)9990, msg, 0);
    }

    public void SendTimeStampToAll()
    {
        TimeStampMessage msg = new TimeStampMessage();

        msg.timestamp = Time.time;

        RecordInForAllHistory((short)9989, msg);
        NetworkServer.SendByChannelToAll((short)9989, msg, 0);
    }

    public void SendTextureToAll(string name, byte[] buffer)
    {
        TextureMessage newMsg = new TextureMessage();

        newMsg.name = name;
        newMsg.texture = buffer;

        RecordInForAllHistory((short)9987, newMsg);
        NetworkServer.SendByChannelToAll((short)9987, newMsg, 2);
    }

    public void AddSticker(string nameGO, Vector3 p, Quaternion q)
    {
        AddStickerMessage newMsg = new AddStickerMessage();

        newMsg.nameGO = nameGO;
        newMsg.position = p;
        newMsg.rotation = q;

        RecordInForAllHistory((short)9986, newMsg);
        NetworkServer.SendByChannelToAll((short)9986, newMsg, 0);
    }

    public void AddSketch(string nameGO, Vector3 p, Quaternion q, Vector3 s, float sw, float ew, int size, Vector3[] data)
    {
        AddSketchMessage newMsg = new AddSketchMessage();

        newMsg.nameGO = nameGO;
        newMsg.position = p;
        newMsg.rotation = q;
        newMsg.scale = s;

        newMsg.startWidth = sw;
        newMsg.endWidth = ew;

        newMsg.size = size;

        string dataStr = "";
        if (data.Length > 0)
        {
            dataStr = data[0].x + "%" + data[0].y + "%" + data[0].z;
            for (int i = 1; i < size; ++i)
                dataStr += "%" + data[i].x + "%" + data[i].y + "%" + data[i].z;
        }
        newMsg.data = dataStr;

        RecordInForAllHistory((short)9985, newMsg);
        NetworkServer.SendByChannelToAll((short)9985, newMsg, 0);
    }

    //void ChangeLayerDeeply(Transform t, int layer)
    //{
    //    t.gameObject.layer = layer;
    //    for (int i = 0; i < t.childCount; ++i)
    //        ChangeLayerDeeply(t.GetChild(i), layer);
    //}


    public void AddGameObjectInScenePart(string namePrefab, string nameGO, Vector3 localPos, Quaternion localRot, string remoteScene, bool isMaster)
    {
        if (useWebRTC)
        {
            webrtc.WebRTCAddGameObjectInScenePart(namePrefab, nameGO, localPos, localRot, remoteScene, isMaster);
            return;
        }

        SceneGameObjectInstanciateMessage newMsg = new SceneGameObjectInstanciateMessage();
        newMsg.name = nameGO;
        newMsg.namePrefab = namePrefab;
        newMsg.parent = remoteScene;

        newMsg.isCustomScale = false;
        newMsg.localPosition = localPos;
        newMsg.localRotation = localRot;

        newMsg.isMaster = isMaster;

        RecordInForAllHistory((short)9982, newMsg);
        NetworkServer.SendByChannelToAll((short)9982, newMsg, 0);
    }

    public void AddGameObjectInScenePart(string namePrefab, string nameGO, Vector3 localPos, Quaternion localRot, Vector3 scale, string remoteScene, bool isMaster)
    {
        // TODO : add scale
        if (useWebRTC)
        {
            webrtc.WebRTCAddGameObjectInScenePart(namePrefab, nameGO, localPos, localRot, scale, remoteScene, isMaster);
            return;
        }

        SceneGameObjectInstanciateMessage newMsg = new SceneGameObjectInstanciateMessage();
        newMsg.name = nameGO;
        newMsg.namePrefab = namePrefab;
        newMsg.parent = remoteScene;

        newMsg.isCustomScale = true;
        newMsg.localScale = scale;
        newMsg.localPosition = localPos;
        newMsg.localRotation = localRot;

        newMsg.isMaster = isMaster;


        RecordInForAllHistory((short)9982, newMsg);
        NetworkServer.SendByChannelToAll((short)9982, newMsg, 0);
    }

    public void UpdateComponentInScenePart(string GOName, string componentTypeName, string[] propertiesNames, GenericType[] propertiesValues)
    {
        if (useWebRTC)
        {
            webrtc.WebRTCUpdateComponentInScenePart(GOName, componentTypeName, propertiesNames, propertiesValues);
            return;
        }

        SceneGameObjectUpdateComponentMessage newMsg = new SceneGameObjectUpdateComponentMessage();

        newMsg.GOName = GOName;
        newMsg.componentTypeName = componentTypeName;
        newMsg.propertiesNames = propertiesNames;
        newMsg.propertiesValues = propertiesValues;

        RecordInForAllHistory((short)9976, newMsg);
        NetworkServer.SendByChannelToAll((short)9976, newMsg, 0);
    }

    public void ChangeColorOnGO(string nameGO, bool inChildren, Color c)
    {
        GameObject go = GameObject.Find(nameGO);
        if (go != null)
        {
            if (inChildren)
            {
                for (int i = 0; i < go.transform.childCount; ++i)
                {
                    GameObject child = go.transform.GetChild(i).gameObject;
                    Renderer childRend = child.GetComponent<Renderer>();
                    if (childRend != null)
                        childRend.material.color = c;
                }
            }
            else
            {
                Renderer rend = go.GetComponent<Renderer>();
                if (rend != null)
                    rend.material.color = c;
                else
                    Debug.Log("unable to find renderer");
            }
        }
        else
        {
            Debug.Log("GO not found");
        }
    }

    public void UpdateGameObjectColorInScenePart(string nameGO, string color, bool inChildren = false)
    {
        if (useWebRTC)
        {
            webrtc.WebRTCUpdateGameObjectColorInScenePart(nameGO, color, inChildren);
            return;
        }

        Material mat = Resources.Load(color, typeof(Material)) as Material;
        Color c;

        if (mat == null)
            c = Color.white;
        else
            c = mat.color;

        ChangeColorOnGO(nameGO, inChildren, c);
        ChangeColorOnGO(nameGO + GONameAddition, inChildren, c);

        SceneGameObjectChangeColorMessage newMsg = new SceneGameObjectChangeColorMessage();
        newMsg.name = nameGO;
        newMsg.color = c;
        newMsg.inChildren = inChildren;

        RecordInForAllHistory((short)9981, newMsg);
        NetworkServer.SendByChannelToAll((short)9981, newMsg, 0);
    }

    public void UpdateGameObjectColorInScenePart(string nameGO, Color color, bool inChildren = false)
    {
        SceneGameObjectChangeColorMessage newMsg = new SceneGameObjectChangeColorMessage();
        newMsg.name = nameGO;
        newMsg.color = color;
        newMsg.inChildren = inChildren;

        RecordInForAllHistory((short)9981, newMsg);
        NetworkServer.SendByChannelToAll((short)9981, newMsg, 0);
    }

    public void RemoveGameObjectInScenePart(string nameGO)
    {
        if (useWebRTC)
        {
            webrtc.WebRTCRemoveGameObjectInScenePart(nameGO);
            return;
        }

        GameObject go = GameObject.Find(nameGO);
        if (go != null)
            Destroy(go);
        else
            Debug.Log("GO not found");

        GameObject goCli = GameObject.Find(nameGO + GONameAddition);
        if (goCli != null)
            Destroy(goCli);
        else
            Debug.Log("GO not found");

        SceneGameObjectRemoveMessage newMsg = new SceneGameObjectRemoveMessage();
        newMsg.name = nameGO + GONameAddition;

        RecordInForAllHistory((short)9980, newMsg);
        NetworkServer.SendByChannelToAll((short)9980, newMsg, 0);
    }

    public void OnAskForPrefabInstantiate(NetworkMessage netMsg)
    {
        OnAskForPrefabInstantiate(netMsg.ReadMessage<SpawnPrefabMessage>());
    }
    public void OnAskForPrefabInstantiate(SpawnPrefabMessage msg)
    {
        GameObject toSpawn = Instantiate(Resources.Load(msg.PrefabName) as GameObject);

        if (sceneGO != null)
            toSpawn.transform.parent = sceneGO.transform;

        toSpawn.name = msg.GoName;
        toSpawn.transform.position = msg.initialPosition;
        toSpawn.transform.rotation = msg.initialRotation;

        //NetworkServer.Spawn(toSpawn);
    }

    public void OnAskForPrefabSpawn(NetworkMessage netMsg)
    {
        OnAskForPrefabSpawn(netMsg.ReadMessage<SpawnPrefabMessage>());
    }
    public void OnAskForPrefabSpawn(SpawnPrefabMessage msg)
    {
        GameObject toSpawn = Instantiate(Resources.Load(msg.PrefabName) as GameObject);

        if (sceneGO != null)
            toSpawn.transform.parent = sceneGO.transform;

        toSpawn.name = msg.GoName;
        toSpawn.transform.position = msg.initialPosition;
        toSpawn.transform.rotation = msg.initialRotation;

        NetworkServer.Spawn(toSpawn);
    }

    public void OnAskForPlayerDataUpdate(NetworkMessage netMsg)
    {
        OnAskForPlayerDataUpdate(netMsg.ReadMessage<UserDataMessage>(), netMsg.conn.address);
    }
    public void OnAskForPlayerDataUpdate(UserDataMessage msg, string address)
    {
        int index = -1;
        for (int i = 0; i < playersConnectionsList.Count; ++i)
        {
            if (playersConnectionsList[i].address == address)
            {
                index = i;
                break;
            }
        }

        if (index >= 0)
        {
            PlayerState state = playersGOList[index].GetComponentInChildren<PlayerState>();
            state.UpdatePlayerData(msg.playerName, playersConnectionsList.Count - 1, msg.cameraToFollow);
        }
        else
        {
            Debug.LogError("no player for asked data update");
        }

        //GameObject toSpawn = Instantiate(Resources.Load(msg.PrefabName) as GameObject);

        //if (sceneGO != null)
        //    toSpawn.transform.parent = sceneGO.transform;

        //toSpawn.name = msg.GoName;
        //toSpawn.transform.position = msg.initialPosition;
        //toSpawn.transform.rotation = msg.initialRotation;

        //NetworkServer.Spawn(toSpawn);
    }

    public void OnAskForLogEntry(NetworkMessage netMsg)
    {
        OnAskForLogEntry(netMsg.ReadMessage<LogEntryMessage>());
    }
    public void OnAskForLogEntry(LogEntryMessage msg)
    {
        logger.LogEntry(msg.ip + "," + msg.logEntry);
    }

    public void OnAskForIp(NetworkMessage netMsg)
    {
        OnAskForLogEntry(netMsg.ReadMessage<LogEntryMessage>());
    }

    public void OnAskForGameObjectInstanciateMessage(NetworkMessage netMsg)
    {
        OnAskForGameObjectInstanciateMessage(netMsg.ReadMessage<SceneGameObjectInstanciateMessage>());
    }
    public void OnAskForGameObjectInstanciateMessage(SceneGameObjectInstanciateMessage msg)
    {
        Debug.Log("about to pop " + msg.namePrefab + " / " + msg.name);
        GameObject instance = Instantiate(Resources.Load(msg.namePrefab, typeof(GameObject))) as GameObject;
        if (instance != null)
        {
            GameObject parent = GameObject.Find(msg.parent);
            Debug.Log("parent with name : " + msg.parent);
            if (parent != null)
            {
                Debug.Log("found");
                instance.transform.name = msg.name + GONameAddition;// "Client";
                Debug.Log("instance name at this point : " + instance.transform.name);
                instance.transform.parent = parent.transform;
                instance.transform.localPosition = msg.localPosition;
                instance.transform.localRotation = msg.localRotation;
                if (msg.isCustomScale)
                    instance.transform.localScale = msg.localScale;

                if(msg.isMaster)
                    instance.AddComponent<NetworkMaster>();
                else
                    instance.AddComponent<NetworkSlave>();

            }
        }
    }

    public void OnAskForGameObjectRemoveMessage(NetworkMessage netMsg)
    {
        OnAskForGameObjectRemoveMessage(netMsg.ReadMessage<SceneGameObjectRemoveMessage>());

    }
    public void OnAskForGameObjectRemoveMessage(SceneGameObjectRemoveMessage msg)
    {
        GameObject go = GameObject.Find(msg.name);
        if (go != null)
            Destroy(go);
        else
            Debug.Log("GO not found");
    }

    public void SendSceneGameObjectTransform(string name, Vector3 position, Quaternion rotation, Vector3 scale, bool local, bool forceUnet = false)
    {
        if (useWebRTC && !forceUnet)
        {
            webrtc.WebRTCSendSceneGameObjectTransform(name, position, rotation, scale, local);
            return;
        }

        //if (!IsClientConnected()) return;

        SceneGameObjectMessage newMsg = new SceneGameObjectMessage();

        newMsg.name = name;
        newMsg.local = local;
        newMsg.position = position;
        newMsg.rotation = rotation;
        newMsg.scale = scale;

        NetworkServer.SendByChannelToAll((short)9983, newMsg, 2);

    }

    public void SendNetworkEvent(string networkEvent)
    {
        NetworkEventMessage newMsg = new NetworkEventMessage();
        newMsg.networkEvent = networkEvent;
        NetworkServer.SendByChannelToAll((short)9969, newMsg, 0);
    }
}