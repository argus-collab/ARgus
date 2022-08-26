using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;

using System.Linq;
using System.Reflection;

using UnityEngine.Networking.Match;


public class CustomClientNetworkManager : NetworkManager
{
    public delegate void NetworkEvent(string arg);
    public event NetworkEvent OnNetworkEvent;

    public string GONameAddition = "Client";
    public bool useInternet = true;

    private List<GameObject> toUpdateGO;

    private List<Vector3> position_n0;
    private List<Vector3> position_n1;

    private List<bool> isLocal;

    private List<Quaternion> rotation_n0;
    private List<Quaternion> rotation_n1;

    //private float curReceptionFrequence;
    private float curUpdateFrequence;

    private float lastTimeStampReception;
    private float lastTimeStampUpdate;

    private List<int> t;

    private List<float> timestamps;
    private List<float> frequences;

    private string lastReceivedMessage = "<no received msg>";
    private int lastReceivedIntValue = -1;
    private bool launchChrono = false;

    public WebRTCUnetMapperClient webrtc;
    public bool useWebRTC = false;

    private string ipAddress;

    public string getLastReceivedMessage() { return lastReceivedMessage; }
    public int getLastReceivedIntValue() { return lastReceivedIntValue; }
    public bool isChronoLaunched() { return launchChrono; }
    public void setChronoLauncher(bool val) { launchChrono = val; }

    public bool fullWebRTC = false;

    public string GetIp()
    {
        return ipAddress;
    }

    // stats
    public int getMaxInterpolationRate()
    {
        if (t.Count > 0)
            return t.Max(); // jujujul 
        else
            return 0;
    }

    public double getMeanInterpolateRate()
    {
        if (t.Count > 0)
            return t.Average();
        else
            return 0;
    }

    public float getUpdateFrequency()
    {
        return curUpdateFrequence;
    }

    public List<float> getObjectsUpdateFrequency()
    {
        return frequences;
    }

    void LoadSpawnableObjectsList()
    {
        Object[] toAdd = Resources.LoadAll("", typeof(GameObject));
        for (int i = 0; i < toAdd.Length; ++i)
        {
            GameObject toAddGO = (GameObject)toAdd[i];
            if (toAddGO.GetComponent<NetworkIdentity>() != null)
                spawnPrefabs.Add((GameObject)toAdd[i]);
            //Debug.Log("add to spawnable list : " + toAdd[i].name);
        }
    }

    //call this method to find a match through the matchmaker
    public void FindInternetMatch(string matchName)
    {
        matchMaker.ListMatches(0, 10, matchName, true, 0, 0, OnInternetMatchList);
    }

    //this method is called when a list of matches is returned
    private void OnInternetMatchList(bool success, string extendedInfo, List<MatchInfoSnapshot> matches)
    {
        if (success)
        {
            if (matches.Count != 0)
            {
                //Debug.Log("A list of matches was returned");

                //join the last server (just in case there are two...)
                matchMaker.JoinMatch(matches[matches.Count - 1].networkId, "", "", "", 0, 0, OnJoinInternetMatch);
            }
            else
            {
                Debug.Log("No matches in requested room!");
            }
        }
        else
        {
            Debug.LogError("Couldn't connect to match maker");
        }
    }

    //this method is called when your request to join a match is returned
    private void OnJoinInternetMatch(bool success, string extendedInfo, MatchInfo matchInfo)
    {
        if (success)
        {
            //Debug.Log("Able to join a match");

            MatchInfo hostInfo = matchInfo;
            NetworkManager.singleton.StartClient(hostInfo);

            ManageHandler();
        }
        else
        {
            Debug.LogError("Join match failed");
        }
    }

    void ManageHandler()
    {
        NetworkManager.singleton.client.RegisterHandler((short)9999, OnSyncObjectTransformUpdate);
        NetworkManager.singleton.client.RegisterHandler((short)9997, OnUpdateUI);
        NetworkManager.singleton.client.RegisterHandler((short)9996, OnReceiveTextMessage);
        NetworkManager.singleton.client.RegisterHandler((short)9995, OnUpdateVisibility);
        NetworkManager.singleton.client.RegisterHandler((short)9994, OnUpdateHierarchy);

        // by net id
        NetworkManager.singleton.client.RegisterHandler((short)9993, OnUpdateColor);
        NetworkManager.singleton.client.RegisterHandler((short)9992, OnUpdateName);

        NetworkManager.singleton.client.RegisterHandler((short)9991, OnReceiveTimeMessage);
        NetworkManager.singleton.client.RegisterHandler((short)9990, OnReceiveLaunchMessage);
        NetworkManager.singleton.client.RegisterHandler((short)9989, SendBackTimeStampMessage);
        NetworkManager.singleton.client.RegisterHandler((short)9986, OnAddStickerMessage);
        NetworkManager.singleton.client.RegisterHandler((short)9985, OnAddSketchMessage);

        // by name - for scene part (local)
        NetworkManager.singleton.client.RegisterHandler((short)9982, OnAskForGameObjectInstanciateMessage);
        NetworkManager.singleton.client.RegisterHandler((short)9981, OnAskForGameObjectChangeColorMessage);
        NetworkManager.singleton.client.RegisterHandler((short)9980, OnAskForGameObjectRemoveMessage); 
        NetworkManager.singleton.client.RegisterHandler((short)9976, OnAskForGameObjectUpdateComponentMessage);

        NetworkManager.singleton.client.RegisterHandler((short)9983, OnReceivedSceneGameObjectMessage);
        NetworkManager.singleton.client.RegisterHandler((short)9970, OnReceivedIpAdressMessage);

        // networkEvent
        NetworkManager.singleton.client.RegisterHandler((short)9969, OnReceivedNetworkEventMessage);

    }


    void Start()
    {

        if(!fullWebRTC)
        {
            LoadSpawnableObjectsList();

            if (useInternet)
            {
                StartMatchMaker();
                FindInternetMatch("default");
            }
            else
            {
                Debug.LogError("networkAddress " + networkAddress);
                Debug.LogError("networkPort " + networkPort);
                Debug.LogError("isNetworkActive " + isNetworkActive);
                Debug.LogError("networkSceneName" + networkSceneName);


                //NetworkManager.singleton.client = new NetworkClient();
                //NetworkManager.singleton.client.Connect(networkAddress, networkPort);
                StartClient();
                ManageHandler();
            }
        }



        //("mm.unet.unity3d.com");




        toUpdateGO = new List<GameObject>();

        position_n0 = new List<Vector3>();
        position_n1 = new List<Vector3>();

        rotation_n0 = new List<Quaternion>();
        rotation_n1 = new List<Quaternion>();

        isLocal = new List<bool>();

        t = new List<int>();

        timestamps = new List<float>();
        frequences = new List<float>();
    }

    void Update()
    {
        float timestamp = Time.time;
        curUpdateFrequence = 1 / (timestamp - lastTimeStampUpdate);
        lastTimeStampUpdate = timestamp;


        for (int i = 0; i < toUpdateGO.Count; ++i)
        {
            // case GO deleted
            if (toUpdateGO[i] == null)
                continue;


            //if (curUpdateFrequence > curReceptionFrequence)
            if (curUpdateFrequence > frequences[i])
            {
                // linear interpolation to smooth displacement
                // transform is one reception late but will move smoothly

                float deltaTUpdate = 1 / curUpdateFrequence;
                //float deltaTReception = 1 / curReceptionFrequence;
                float deltaTReception = 1 / frequences[i];

                if (t[i] < (int)(deltaTReception / deltaTUpdate))
                {
                    if(isLocal[i])
                    {
                        toUpdateGO[i].transform.localPosition = Vector3.Lerp(position_n0[i], position_n1[i], deltaTUpdate * t[i]);
                        toUpdateGO[i].transform.localRotation = Quaternion.Lerp(rotation_n0[i], rotation_n1[i], deltaTUpdate * t[i]);
                    }
                    else
                    {
                        toUpdateGO[i].transform.position = Vector3.Lerp(position_n0[i], position_n1[i], deltaTUpdate * t[i]);
                        toUpdateGO[i].transform.rotation = Quaternion.Lerp(rotation_n0[i], rotation_n1[i], deltaTUpdate * t[i]);
                    }
                    t[i]++;
                }
            }
            else
            {
                // interpolation not possible
                if (isLocal[i])
                {
                    toUpdateGO[i].transform.localPosition = position_n1[i];
                    toUpdateGO[i].transform.localRotation = rotation_n1[i];
                }
                else
                {
                    toUpdateGO[i].transform.position = position_n1[i];
                    toUpdateGO[i].transform.rotation = rotation_n1[i];
                }
            }
        }


    }

    public void OnSyncObjectTransformUpdate(NetworkMessage netMsg)
    {
        OnSyncObjectTransformUpdate(netMsg.ReadMessage<TransformMessage>());
    }
    public void OnSyncObjectTransformUpdate(TransformMessage msg)
    {
        float timestamp = Time.time;
        //curReceptionFrequence = 1 / (timestamp - lastTimeStampReception);
        //lastTimeStampReception = timestamp;

        GameObject go = ClientScene.FindLocalObject(msg.id);
        if (go != null)
        {
            NetworkSyncObject o = go.GetComponent<NetworkSyncObject>();
            if (o != null)
            {
                //o.transform.position = msg.p;
                //o.transform.rotation = msg.q;

                //toUpdateGO.Add(go);
                //positions.Add(msg.p);
                //rotations.Add(msg.q);

                int index = toUpdateGO.IndexOf(go);
                if (index >= 0)
                {
                    position_n0[index] = position_n1[index];
                    rotation_n0[index] = rotation_n1[index];

                    position_n1[index] = msg.p;
                    rotation_n1[index] = msg.q;

                    isLocal[index] = msg.isLocal;

                    t[index] = 0;

                    frequences[index] = 1 / (timestamp - timestamps[index]);
                    timestamps[index] = timestamp;
                }
                else
                {
                    toUpdateGO.Add(go);

                    position_n0.Add(msg.p);
                    rotation_n0.Add(msg.q);

                    position_n1.Add(msg.p);
                    rotation_n1.Add(msg.q);

                    isLocal.Add(msg.isLocal);

                    t.Add(0);

                    frequences.Add(float.MaxValue);
                    timestamps.Add(timestamp);
                }

                //o.MsgSendTimeStamp("UpdateObjectState");

            }
        }



    }

    // only work for pile ui at this time
    void OnUpdateUI(NetworkMessage netMsg)
    {
        UIStateMessage msg = netMsg.ReadMessage<UIStateMessage>();

        //var foundGameUI = FindObjectsOfType<ClientPileGameUIManager>();
        //foreach (ClientPileGameUIManager ui in foundGameUI)
        //    ui.displayStats = msg.displayStat;
    }

    public void SendCommandMessage(string command, string args = "")
    {
        if (useWebRTC)
        {
            webrtc.WebRTCSendCommandMessage(command, args);
            return;
        }

        if (!IsClientConnected()) return;

        CommandMessage msg = new CommandMessage();
        msg.command = command;
        msg.args = args;

        //NetworkServer.SendByChannelToAll((short)9996, msg, 2);
        NetworkManager.singleton.client.SendByChannel((short)9977, msg, 0);
    }

    public void SendStringMessage(string str)
    {
        if (!IsClientConnected()) return;

        StringMessage msg = new StringMessage();

        msg.text = str;

        //NetworkServer.SendByChannelToAll((short)9996, msg, 2);
        NetworkManager.singleton.client.SendByChannel((short)9996, msg, 2);
    }

    public void OnUpdateVisibility(NetworkMessage netMsg)
    {
        VisibilityMessage msg = netMsg.ReadMessage<VisibilityMessage>();

        GameObject go = ClientScene.FindLocalObject(msg.id);
        if (go != null)
            go.SetActive(msg.visibility);

    }

    public void OnUpdateHierarchy(NetworkMessage netMsg)
    {
        HierarchyMessage msg = netMsg.ReadMessage<HierarchyMessage>();

        GameObject parent = GameObject.Find(msg.nameParent);
        GameObject child = ClientScene.FindLocalObject(msg.idChild);

        if (parent != null && child != null)
        {
            child.transform.parent = parent.transform;
        }
        else
        {
            Debug.Log("hierarchy affectation ERROR - parent : " + parent + ", child : " + child);
        }
    }

    // only change color of first material in the renderer
    public void OnUpdateColor(NetworkMessage netMsg)
    {
        ColorMessage msg = netMsg.ReadMessage<ColorMessage>();

        GameObject go = ClientScene.FindLocalObject(msg.id);
        if (go != null)
            go.GetComponent<Renderer>().material.color = msg.color;
    }

    public void OnUpdateName(NetworkMessage netMsg)
    {
        NameMessage msg = netMsg.ReadMessage<NameMessage>();

        GameObject go = ClientScene.FindLocalObject(msg.id);
        if (go != null)
            go.name = msg.name;
    }

    public void OnReceiveTextMessage(NetworkMessage netMsg)
    {
        StringMessage msg = netMsg.ReadMessage<StringMessage>();

        lastReceivedMessage = msg.text;
    }

    public void OnReceiveLocalTextMessage(string msg)
    {
        lastReceivedMessage = msg;
    }

    public void OnReceiveTimeMessage(NetworkMessage netMsg)
    {
        TimeMessage msg = netMsg.ReadMessage<TimeMessage>();

        lastReceivedIntValue = msg.time;
    }

    public void OnReceiveLaunchMessage(NetworkMessage netMsg)
    {
        LaunchMessage msg = netMsg.ReadMessage<LaunchMessage>();

        launchChrono = msg.launch;
    }

    public void SendBackTimeStampMessage(NetworkMessage netMsg)
    {
        if (!IsClientConnected()) return;

        TimeStampMessage newMsg = netMsg.ReadMessage<TimeStampMessage>();
        NetworkManager.singleton.client.Send((short)9989, newMsg);
    }

    public void SendMeshToAll(string name, Vector3 p, Quaternion q, byte[] buffer)
    {
        if (!IsClientConnected()) return;

        MeshMessage newMsg = new MeshMessage();

        newMsg.name = name;
        newMsg.p = p;
        newMsg.q = q;
        newMsg.mesh = buffer;

        //Debug.Log("send mesh " + name + ", p : " + p + ", q : " + q + ", l : " + buffer.Length);

        NetworkManager.singleton.client.Send((short)9988, newMsg);
        //NetworkManager.singleton.client.SendByChannel((short)9988, newMsg, 2);
    }

    public void SendSceneGameObjectTransform(string name, Vector3 position, Quaternion rotation, Vector3 scale, bool local)
    {
        if (useWebRTC)
        {
            webrtc.WebRTCSendSceneGameObjectTransform(name, position, rotation, scale, local);
            return;
        }

        if (!IsClientConnected()) return;

        SceneGameObjectMessage newMsg = new SceneGameObjectMessage();

        newMsg.name = name;
        newMsg.local = local;
        newMsg.position = position;
        newMsg.rotation = rotation;
        newMsg.scale = scale;

        NetworkManager.singleton.client.SendByChannel((short)9983, newMsg, 2);

    }

    public void SendHandModelToAll(string handedness, List<Vector3> p, List<Quaternion> q)
    {
        if (!IsClientConnected()) return;

        HandModelMessage newMsg = new HandModelMessage();

        newMsg.handedness = handedness;

        newMsg.v_wrist = p[0];
        newMsg.v_palm = p[1];
        newMsg.v_thumb_metacarpal = p[2];
        newMsg.v_thumb_proximal = p[3];
        newMsg.v_thumb_distal = p[4];
        newMsg.v_thumb_tip = p[5];
        newMsg.v_index_metacarpal = p[6];
        newMsg.v_index_knuckle = p[7];
        newMsg.v_index_middle = p[8];
        newMsg.v_index_distal = p[9];
        newMsg.v_index_tip = p[10];
        newMsg.v_middle_metacarpal = p[11];
        newMsg.v_middle_knuckle = p[12];
        newMsg.v_middle_middle = p[13];
        newMsg.v_middle_distal = p[14];
        newMsg.v_middle_tip = p[15];
        newMsg.v_ring_metacarpal = p[16];
        newMsg.v_ring_knuckle = p[17];
        newMsg.v_ring_middle = p[18];
        newMsg.v_ring_distal = p[19];
        newMsg.v_ring_tip = p[20];
        newMsg.v_pinky_metacarpal = p[21];
        newMsg.v_pinky_knuckle = p[22];
        newMsg.v_pinky_middle = p[23];
        newMsg.v_pinky_distal = p[24];
        newMsg.v_pinky_tip = p[25];

        newMsg.q_wrist = q[0];
        newMsg.q_palm = q[1];
        newMsg.q_thumb_metacarpal = q[2];
        newMsg.q_thumb_proximal = q[3];
        newMsg.q_thumb_distal = q[4];
        newMsg.q_thumb_tip = q[5];
        newMsg.q_index_metacarpal = q[6];
        newMsg.q_index_knuckle = q[7];
        newMsg.q_index_middle = q[8];
        newMsg.q_index_distal = q[9];
        newMsg.q_index_tip = q[10];
        newMsg.q_middle_metacarpal = q[11];
        newMsg.q_middle_knuckle = q[12];
        newMsg.q_middle_middle = q[13];
        newMsg.q_middle_distal = q[14];
        newMsg.q_middle_tip = q[15];
        newMsg.q_ring_metacarpal = q[16];
        newMsg.q_ring_knuckle = q[17];
        newMsg.q_ring_middle = q[18];
        newMsg.q_ring_distal = q[19];
        newMsg.q_ring_tip = q[20];
        newMsg.q_pinky_metacarpal = q[21];
        newMsg.q_pinky_knuckle = q[22];
        newMsg.q_pinky_middle = q[23];
        newMsg.q_pinky_distal = q[24];
        newMsg.q_pinky_tip = q[25];


        NetworkManager.singleton.client.Send((short)9984, newMsg);
        //NetworkManager.singleton.client.SendByChannel((short)9988, newMsg, 2);
    }

    public void OnReceivedTextureMessage(NetworkMessage netMsg)
    {
        TextureMessage msg = netMsg.ReadMessage<TextureMessage>();

        //GameObject newMesh = GameObject.Find(msg.name);
        //MeshFilter mshF;

        //if (newMesh == null)
        //{
        //    newMesh = new GameObject(msg.name);
        //    mshF = newMesh.AddComponent<MeshFilter>();
        //    MeshRenderer mshR = newMesh.AddComponent<MeshRenderer>();
        //    mshR.material = meshMaterial;

        //    if (meshRoot != null)
        //        newMesh.transform.parent = meshRoot.transform;
        //}
        //else
        //{
        //    mshF = newMesh.GetComponent<MeshFilter>();
        //}


        //newMesh.transform.localPosition = msg.p;
        //newMesh.transform.localRotation = msg.q;

        //mshF.mesh = MeshSerializer.ReadMesh(msg.mesh);

        //mshF.mesh.RecalculateBounds();

    }

    public void OnAddStickerMessage(NetworkMessage netMsg)
    {
        AddStickerMessage msg = netMsg.ReadMessage<AddStickerMessage>();

        GameObject go = GameObject.Find(msg.nameGO);
        if (go != null)
        {
            Transform stickersTransform = go.transform.Find("stickers");
            if(stickersTransform == null)
            {
                GameObject stickersGoNew = new GameObject("stickers");
                stickersGoNew.transform.parent = go.transform;
                stickersGoNew.transform.localPosition = Vector3.zero;
                stickersGoNew.transform.localRotation = Quaternion.identity;
                stickersGoNew.transform.localScale = new Vector3(1, 1, 1);

                stickersTransform = stickersGoNew.transform;
            }

            GameObject sticker = Instantiate(Resources.Load("Sticker") as GameObject);
            sticker.transform.parent = stickersTransform;
            sticker.transform.localPosition = msg.position;
            sticker.transform.localRotation = msg.rotation;
        }
        else
            Debug.Log("no game object to add sticker on");
    }

    public void OnAddSketchMessage(NetworkMessage netMsg)
    {
        AddSketchMessage msg = netMsg.ReadMessage<AddSketchMessage>();

        GameObject go = GameObject.Find(msg.nameGO);
        if (go != null)
        {
            Transform sketchTransform = go.transform.Find("sketch");
            if (sketchTransform == null)
            {
                GameObject sketchGoNew = new GameObject("sketch");
                sketchGoNew.transform.parent = go.transform;
                sketchGoNew.transform.localPosition = Vector3.zero;
                sketchGoNew.transform.localRotation = Quaternion.identity;
                sketchGoNew.transform.localScale = new Vector3(1, 1, 1);

                sketchTransform = sketchGoNew.transform;
            }

            GameObject sketchPart = new GameObject("sketch-part");
            sketchPart.transform.parent = sketchTransform;
            sketchPart.transform.localPosition = msg.position;
            sketchPart.transform.localRotation = msg.rotation;
            sketchPart.transform.localScale = msg.scale;

            LineRenderer curLine = sketchPart.AddComponent<LineRenderer>();
            curLine.startWidth = msg.startWidth;
            curLine.endWidth = msg.endWidth;
            curLine.material = new Material(Shader.Find("Diffuse"));
            curLine.useWorldSpace = false;

            var currentCulture = System.Globalization.CultureInfo.InstalledUICulture;
            System.Globalization.NumberFormatInfo numberFormat = (System.Globalization.NumberFormatInfo)currentCulture.NumberFormat.Clone();
            numberFormat.NumberDecimalSeparator = ",";
            string[] data = msg.data.Split('%');
            Vector3[] array = new Vector3[msg.size];

            for (int i = 0; i < msg.size; ++i)
            {
                array[i] = new Vector3(
                    float.Parse(data[i * 3 + 0], numberFormat),
                    float.Parse(data[i * 3 + 1], numberFormat),
                    float.Parse(data[i * 3 + 2], numberFormat));
            }

            curLine.positionCount = msg.size;
            curLine.SetPositions(array);
        }
        else
            Debug.Log("no game object to add sticker on");
    }

    public void OnAskForGameObjectInstanciateMessage(NetworkMessage netMsg)
    {
        OnAskForGameObjectInstanciateMessage(netMsg.ReadMessage<SceneGameObjectInstanciateMessage>());
    }
    public void OnAskForGameObjectInstanciateMessage(SceneGameObjectInstanciateMessage msg)
    {
        GameObject instance = Instantiate(Resources.Load(msg.namePrefab, typeof(GameObject))) as GameObject;
        if (instance != null)
        {
            GameObject parent = GameObject.Find(msg.parent);
            //Debug.Log("parent with name : " + msg.parent);
            if(parent!=null)
            {
                //Debug.Log("found");
                instance.transform.name = msg.name + GONameAddition;// "Client";
                //Debug.Log("instance name at this point : " + instance.transform.name);
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

    public void OnAskForGameObjectUpdateComponentMessage(NetworkMessage netMsg)
    {
        OnAskForGameObjectUpdateComponentMessage(netMsg.ReadMessage<SceneGameObjectUpdateComponentMessage>());
    }
    public void OnAskForGameObjectUpdateComponentMessage(SceneGameObjectUpdateComponentMessage msg)
    {
        Debug.Log("component update");
        GameObject go = GameObject.Find(msg.GOName + GONameAddition);
        if (go != null)
        {
            Debug.Log("GO found");
            Component c = go.GetComponent(msg.componentTypeName);
            if (c != null)
            {
                Debug.Log("component found");
                for (int i = 0; i < msg.propertiesNames.Length; ++i)
                {
                    foreach (FieldInfo fi in c.GetType().GetFields())
                    {
                        Debug.Log("reach field : " + fi.Name);
                        if (fi.Name == msg.propertiesNames[i])
                        {
                            System.Object obj = (System.Object)c;

                            fi.SetValue(obj, msg.propertiesValues[i].GetValue());
                            Debug.Log("set value at " + msg.propertiesValues[i].GetValue());
                        }
                    }
                }
            }
        }
    }

    public void OnAskForGameObjectChangeColorMessage(NetworkMessage netMsg)
    {
        OnAskForGameObjectChangeColorMessage(netMsg.ReadMessage<SceneGameObjectChangeColorMessage>());
    }
    public void OnAskForGameObjectChangeColorMessage(SceneGameObjectChangeColorMessage msg)
    {
        GameObject go = GameObject.Find(msg.name);
        if (go != null)
        {
            if(msg.inChildren)
            {
                ApplyColorsDeeply(go.transform, msg.color);
            }
            else
            {
                Renderer rend = go.GetComponent<Renderer>();
                if(rend != null)
                    rend.material.color = msg.color;
            }
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

    public void AskForPrefabInstantiate(string prefabName, string goName, Vector3 initialPosition, Quaternion initialRotation)
    {
        if (useWebRTC)
        {
            webrtc.WebRTCAskForPrefabInstantiate(prefabName, goName, initialPosition, initialRotation);
            return;
        }

        if (!IsClientConnected()) return;

        SpawnPrefabMessage newMsg = new SpawnPrefabMessage();

        newMsg.PrefabName = prefabName;
        newMsg.GoName = goName;
        newMsg.initialPosition = initialPosition;
        newMsg.initialRotation = initialRotation;

        NetworkManager.singleton.client.Send((short)9975, newMsg);
    }

    public void AskForPrefabSpawn(string prefabName, string goName, Vector3 initialPosition, Quaternion initialRotation)
    {
        if (useWebRTC)
        {
            webrtc.WebRTCAskForPrefabSpawn(prefabName, goName, initialPosition, initialRotation);
            return;
        }

        if (!IsClientConnected()) return;

        SpawnPrefabMessage newMsg = new SpawnPrefabMessage();

        newMsg.PrefabName = prefabName;
        newMsg.GoName = goName;
        newMsg.initialPosition = initialPosition;
        newMsg.initialRotation = initialRotation;

        NetworkManager.singleton.client.Send((short)9974, newMsg);
    }

    public void AskForPlayerDataUpdate(string playerName, int indexRep, string cameraToFollow)
    {
        if (useWebRTC)
        {
            webrtc.WebRTCAskForPlayerDataUpdate(playerName, indexRep, cameraToFollow);
            return;
        }

        if (!IsClientConnected()) return;

        UserDataMessage newMsg = new UserDataMessage();

        newMsg.playerName = playerName;
        newMsg.cameraToFollow = cameraToFollow;
        newMsg.indexRep = indexRep;

        NetworkManager.singleton.client.Send((short)9973, newMsg);
    }

    public void AskForLogEntry(string logEntry)
    {
        if (useWebRTC)
        {
            webrtc.WebRTCAskForLogEntry(logEntry);
            return;
        }

        if (!IsClientConnected()) return;

        LogEntryMessage newMsg = new LogEntryMessage();

        newMsg.ip = NetworkManager.singleton.client.connection.address;
        newMsg.logEntry = logEntry;

        NetworkManager.singleton.client.Send((short)9972, newMsg);
    }

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

    public void OnReceivedIpAdressMessage(NetworkMessage netMsg)
    {
        OnReceivedIpAdressMessage(netMsg.ReadMessage<IpAdressMessage>());
    }
    public void OnReceivedIpAdressMessage(IpAdressMessage msg)
    {
        ipAddress = msg.address;
    }

    public void OnReceivedNetworkEventMessage(NetworkMessage netMsg)
    {
        OnReceivedNetworkEventMessage(netMsg.ReadMessage<NetworkEventMessage>());
    }
    public void OnReceivedNetworkEventMessage(NetworkEventMessage msg)
    {
        OnNetworkEvent(msg.networkEvent);
    }
}
