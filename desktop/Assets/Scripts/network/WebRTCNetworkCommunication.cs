using System.Text;
using Microsoft.MixedReality.WebRTC;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Globalization;
using System.Collections.Generic;

/*
 * # Message structure 
 * ===================
 * 
 * Generic sperator char is %
 * Messages are made with this nomenclatures :
 *  <recipient>%<action>%<data>
 *  
 *  Actions :
 *      - select : position2D
 *      - grasp 
 *      - instantiate : name, position, rotation
 *      - stick : name, position, rotation
 *      - draw : ... TODO ...
 */

public class WebRTCMessage
{
    public int recipient;
    public short channel;
    public long timestamp;

    public WebRTCMessage(int recipient, short channel, long timestamp)
    {
        this.recipient = recipient;
        this.channel = channel;
        this.timestamp = timestamp;
    }
}

public class WebRTCMessageGeneric : WebRTCMessage
{ 
    public string data;

    public WebRTCMessageGeneric(int recipient, short channel, long timestamp, string data) : base(recipient, channel, timestamp)
    {
        this.data = data;
    }

    override public string ToString()
    {
        return recipient + " - " + channel + " - " + timestamp + " - " + data;
    }
}

public class WebRTCMessageData2DPosition : WebRTCMessage
{
    public float x;
    public float y;

    public WebRTCMessageData2DPosition(int recipient, short channel, long timestamp, float x, float y) : base(recipient, channel, timestamp)
    {
        this.recipient = recipient;
        this.channel = channel;
        this.x = x;
        this.y = y;
    }
}

public class WebRTCMessageDataSketch : WebRTCMessage
{
    public string name;
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;

    public float startWidth;
    public float endWidth;

    public int size;
    public Vector3[] data;

    public WebRTCMessageDataSketch(int recipient, short channel, long timestamp, string name, Vector3 position, Quaternion rotation, Vector3 scale, float startWidth, float endWidth, int size, Vector3[] data) : base(recipient, channel, timestamp)
    {
        this.name = name;
        this.position = position;
        this.rotation = rotation;
        this.scale = scale;
        this.startWidth = startWidth;
        this.endWidth = endWidth;
        this.size = size;
        this.data = data;
    }
}

public class WebRTCMessageDataGameObject : WebRTCMessage
{
    public string name;
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;

    public WebRTCMessageDataGameObject(int recipient, short channel, long timestamp, string name, Vector3 position, Quaternion rotation, Vector3 scale) : base(recipient, channel, timestamp)
    {
        this.name = name;
        this.position = position;
        this.rotation = rotation;
        this.scale = scale;
    }
}




public class WebRTCNetworkCommunication : MonoBehaviour
{
    public delegate void NetworkEvent(string arg);
    public event NetworkEvent OnNetworkEvent;

    public Microsoft.MixedReality.WebRTC.Unity.PeerConnection pc;
    public bool isSender;

    public int minChannel = 9900;
    public int maxChannel = 10000;

    private DataChannel dc;

    private string sep = ":/";
    private string dataSep = "?:";
    private string dataSubSep = "!.";
    private string[] stringSeparators;
    private string[] stringDataSeparators;
    private string[] stringDataSubSeparators;

    private List<WebRTCMessageGeneric> unreadMessages;
    private List<string> toSendMessages;

    private NumberFormatInfo numberFormat;

    public int localId;
    public int remoteId;
    public float freq = 30f;
    public bool displayLog;
    public bool debugMode;
    public bool init;

    public bool restartPeerConnectionImmediatly;
    public bool restartPeerConnection;
    public bool updateTimeline;
    public bool recreatePeerConnection;
    public int recreatePeerStep = -1;
    //private float lastTimeStamp;
    private float lastTimeStamp;
    private float restartTimeStamp;
    private float restartDuration;
    private float updateTimelineTimeStamp;
    private float updateTimelineDuration;
    private float recreateConnectionTimeStamp;
    private float recreateConnectionDuration;

    public int unreadMessageSize = 0;

    public List<string> exceptions;
    public List<string> messagesDataHistory;
    public List<short> messagesChannelsHistory;

    private bool isAlreadyRunning;

    public GameObject playerRoot;
    public GameObject playerPrefab;
    private GameObject playerInstance;

    public bool createPlayerAtStart = false;

    private System.Threading.Tasks.Task<DataChannel> outerTask;

    public void RecordInForAllHistory(short channel, string data, string goName)
    {
        // don't record if prefab given as exception
        if (exceptions.Contains(goName))
            return;

        messagesChannelsHistory.Add(channel);
        messagesDataHistory.Add(data);
    }

    public void DeleteRecordHistory(string goName)
    {
        for (int i = 0; i < messagesDataHistory.Count; ++i)
        {
            if (messagesDataHistory[i].Contains(goName))
            {
                messagesDataHistory.RemoveAt(i);
                messagesChannelsHistory.RemoveAt(i);
            }
        }
    }
    public void UpdateTimelineForClient()
    {
        for (int i = 0; i < messagesDataHistory.Count; ++i)
            SendMessage(messagesChannelsHistory[i], messagesDataHistory[i]);
    }
    public void UpdateTimelineForClientIn(float t)
    {
        updateTimeline = true;
        updateTimelineTimeStamp = Time.time;
        updateTimelineDuration = t;
    }
    public void AskForSynchronization()
    {
        string syncMsg = remoteId + sep + ((short)9971) + sep + DateTimeOffset.Now.ToUnixTimeMilliseconds() + sep + "";
        dc.SendMessage(Encoding.ASCII.GetBytes(syncMsg));

    }
    public void Synchronize()
    {
        //RecreatePeerConnection();
        //RestartPeerConnectionIn(3);
        UpdateTimelineForClientIn(3);
    }
    public void RecreatePeerConnection()
    {
        restartPeerConnectionImmediatly = true;
    }
    public void RecreatePeerConnectionIn(float t)
    {
        recreatePeerConnection = true;
        recreateConnectionTimeStamp = Time.time;
        recreateConnectionDuration = t;
    }
    public void RestartPeerConnectionIn(float t)
    {
        restartPeerConnection = true;
        restartTimeStamp = Time.time;
        restartDuration = t;
    }


    public bool IsConnected()
    {
        try
        {
            dc.SendMessage(Encoding.ASCII.GetBytes(""));
            return true;
        }
        catch(Exception e)
        {
            return false;
        }
    }

    public bool IsPlayerInstancied()
    {
        return playerInstance != null;
    }

    public void CreatePlayer()
    {
        playerInstance = (GameObject)Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);

        if (playerPrefab.name == "HololensRepresentation")
            playerInstance.name = "Head";
        if (playerPrefab.name == "ParticipantRepresentation")
            playerInstance.name = "Participant";

        playerInstance.layer = 8;
        GameObject playerCoordinateSystem = new GameObject("0.0.0.0");
        playerCoordinateSystem.layer = 8;
        playerInstance.transform.parent = playerCoordinateSystem.transform;

        if (playerRoot != null)
            playerCoordinateSystem.transform.parent = playerRoot.transform;
    }

    public void RemovePlayer()
    {
        Destroy(playerInstance.transform.parent.gameObject);
    }

    public void UpdatePlayer()
    {
        bool local = false;
        string data = PrepareData(new string[] {
            playerInstance.name,
            local.ToString(),
            Vector3ToString(playerInstance.transform.position),
            QuaternionToString(playerInstance.transform.rotation),
            Vector3ToString(playerInstance.transform.localScale)
        });

        SendMessage((short)9983, data);
    }

    /*
     * INIT
     */
    private void Start()
    {
        messagesDataHistory = new List<string>();
        messagesChannelsHistory = new List<short>();
        exceptions = new List<string>();
        toSendMessages = new List<String>();

        var currentCulture = CultureInfo.InstalledUICulture;
        numberFormat = (NumberFormatInfo)currentCulture.NumberFormat.Clone();
        numberFormat.NumberDecimalSeparator = ",";

        stringSeparators = new string[] { sep };
        stringDataSeparators = new string[] { dataSep };
        stringDataSubSeparators = new string[] { dataSubSep };

        Init();
    }
    void Init()
    {
        if (isAlreadyRunning)
            AddDataChannel();
        else
            pc.OnInitialized.AddListener(AddDataChannel);
        isAlreadyRunning = true;
        unreadMessages = new List<WebRTCMessageGeneric>();
    }

    private void OnDisable()
    {
        DestroyConnection();
    }

    private void OnDestroy()
    {
        DestroyConnection();
    }

    private void OnApplicationQuit()
    {
        DestroyConnection();
    }

    private void DestroyConnection()
    {
        if (outerTask == null) return;
        outerTask.Dispose();
    }

    private void OnDataChannelAdded(DataChannel dc)
    {
        Debug.Log("Data Channel added");

        dc.MessageReceived += MessageReceived;

        this.dc = dc;
    }

    private void AddDataChannel()
    {
        pc.Peer.DataChannelAdded += OnDataChannelAdded;

        if (isSender)
        {
            outerTask = pc.Peer.AddDataChannelAsync("DATA", true, true); ;
            outerTask.ContinueWith(task =>
            {
                Debug.Log("Data Channel created");
            });
        }

    }

    /*
     * SENDING / RECEIVING PROCESS
     */

    private void MessageReceivedLegacy(byte[] obj)
    {
        /*string msg = System.Text.Encoding.Default.GetString(obj);

        if (msg.Length < 3)
            return;

        string[] data = msg.Split(stringSeparators, StringSplitOptions.None);

        if (data[1] == "select")
        {
            float x = float.Parse(data[2], numberFormat);
            float y = float.Parse(data[3], numberFormat);

            unreadMessages.Add(new WebRTCMessageData2DPosition(data[0], data[1], x, y));
        }
        else if (data[1] == "grasp" || data[1] == "release")
        {
            unreadMessages.Add(new WebRTCMessage(data[0], data[1]));

        }
        else if (data[1] == "instantiate" || data[1] == "stick")
        {
            float px = float.Parse(data[3], numberFormat);
            float py = float.Parse(data[4], numberFormat);
            float pz = float.Parse(data[5], numberFormat);
            Vector3 p = new Vector3(px, py, pz);

            float qx = float.Parse(data[6], numberFormat);
            float qy = float.Parse(data[7], numberFormat);
            float qz = float.Parse(data[8], numberFormat);
            float qw = float.Parse(data[9], numberFormat);
            Quaternion q = new Quaternion(qx, qy, qz, qw);

            float sx = float.Parse(data[10], numberFormat);
            float sy = float.Parse(data[11], numberFormat);
            float sz = float.Parse(data[12], numberFormat);
            Vector3 s = new Vector3(sx, sy, sz);

            unreadMessages.Add(new WebRTCMessageDataGameObject(data[0], data[1], data[2], p, q, s));
        }
        else if (data[1] == "draw")
        {
            float px = float.Parse(data[3], numberFormat);
            float py = float.Parse(data[4], numberFormat);
            float pz = float.Parse(data[5], numberFormat);
            Vector3 p = new Vector3(px, py, pz);

            float qx = float.Parse(data[6], numberFormat);
            float qy = float.Parse(data[7], numberFormat);
            float qz = float.Parse(data[8], numberFormat);
            float qw = float.Parse(data[9], numberFormat);
            Quaternion q = new Quaternion(qx, qy, qz, qw);

            float sx = float.Parse(data[10], numberFormat);
            float sy = float.Parse(data[11], numberFormat);
            float sz = float.Parse(data[12], numberFormat);
            Vector3 s = new Vector3(sx, sy, sz);

            float startWidth = float.Parse(data[13], numberFormat);
            float endWidth = float.Parse(data[14], numberFormat);

            int size = int.Parse(data[15], numberFormat);

            Vector3[] array = new Vector3[size];

            for (int i = 0; i < size; ++i)
            {
                array[i] = new Vector3(
                    float.Parse(data[16 + i * 3 + 0], numberFormat),
                    float.Parse(data[16 + i * 3 + 1], numberFormat),
                    float.Parse(data[16 + i * 3 + 2], numberFormat));
            }

            unreadMessages.Add(new WebRTCMessageDataSketch(data[0], data[1], data[2], p, q, s, startWidth, endWidth, size, array));
        }*/
    }

    private void MessageReceived(byte[] obj)
    {
        //MessageReceivedLegacy(obj);

        //string msg = System.Text.Encoding.Default.GetString(obj);
        string msg = System.Text.Encoding.UTF8.GetString(obj);

        string[] data = GetSplittedMessage(msg);

        if (displayLog)
            Debug.Log("received message : " + msg + " with length of " + data.Length);

        if (data.Length != 4)
            return;

        short channel = short.Parse(data[1]);
        if (channel == 0)
            OnNetworkEvent(data[3]);
        else
            unreadMessages.Add(new WebRTCMessageGeneric(Int32.Parse(data[0]), channel, long.Parse(data[2]), data[3]));
    }

    void UpdateReceiving(short channel)
    {
        List<WebRTCMessageGeneric> messages = IsThereAnyMessagesForMe(channel);
        for (int i = 0; i < messages.Count; ++i)
        {
            string duration = (DateTimeOffset.Now.ToUnixTimeMilliseconds() - messages[i].timestamp).ToString();
            Debug.Log(i + " > " + duration + " - " + messages[i].data);
        }
    }

    void Update()
    {
        if (init)
        {
            init = false;
            Init();
        }

        if (restartPeerConnectionImmediatly)
        {
            restartPeerConnectionImmediatly = false;
            pc.enabled = false;
            recreatePeerStep = 1;
        }

        if (recreatePeerStep >= 100)
        {
            pc.enabled = true;
            recreatePeerStep = -1;
        }
        else if (recreatePeerStep > 0)
        {
            recreatePeerStep++;
        }

        if (recreatePeerConnection && (Time.time - recreateConnectionTimeStamp) > recreateConnectionDuration)
        {
            recreatePeerConnection = false;
            Debug.Log("recreatePeerConnection connection");
            restartPeerConnectionImmediatly = true;
        }

        if (restartPeerConnection && (Time.time - restartTimeStamp) > restartDuration)
        {
            Debug.Log("peer start connection");
            restartPeerConnection = false;
            pc.StartConnection();
        }

        if (updateTimeline && (Time.time - updateTimelineTimeStamp) > updateTimelineDuration)
        {
            Debug.Log("update time line for client");
            updateTimeline = false;
            UpdateTimelineForClient();
        }



        if (debugMode)
        {
            UpdateReceiving((short)9901);
            SendMessage((short)9901, localId + " says hello to " + remoteId);
        }

        unreadMessageSize = unreadMessages.Count;
        //if (Input.GetKeyDown(KeyCode.Tab) && isSender)
        //{
        //    Debug.Log("Send Offer");
        //    //pc.SendOffer();
        //}

        //if (Input.GetKeyDown(KeyCode.Tab))
        //{
        //    Debug.Log("Sending Hoyoyo!");

        //    dc.SendMessage(Encoding.ASCII.GetBytes("Hoyoyo!"));
        //}

        float t = Time.time;

        if (t - lastTimeStamp > 1 / freq)
        {
            lastTimeStamp = t;

            for (int i = 0; i < toSendMessages.Count; ++i)
            {
                try
                {
                    dc.SendMessage(Encoding.UTF8.GetBytes(toSendMessages[i]));
                }
                catch (Exception e) { }
            }
            toSendMessages.Clear();
        }

        if(createPlayerAtStart && IsConnected())
        {
            createPlayerAtStart = false;
            CreatePlayer();
        }

        if (playerInstance != null)
            UpdatePlayer();
    }


    /*
     * COMMANDS
     */

    public string PrepareData(string[] s)
    {
        string res = "";
        for(int i=0;i<s.Length;++i)
            res += s[i] + dataSep;
        return res;
    }
    public string PrepareArrayData(string[] a)
    {
        string res = "";
        for (int i = 0; i < a.Length; ++i)
            res += a[i] + dataSubSep;
        return res;
    }

    public string[] GetSplittedMessage(string data)
    {
        return data.Split(stringSeparators, StringSplitOptions.None);
    }
    public string[] GetSplittedData(string data)
    {
        return data.Split(stringDataSeparators, StringSplitOptions.None);
    }
    public string[] GetSplittedArrayData(string data)
    {
        return data.Split(stringDataSubSeparators, StringSplitOptions.None);
    }

    public Vector3 StringToVector3(string v)
    {
        string[] split = GetSplittedArrayData(v);
        if (split.Length == 3)
            return new Vector3(float.Parse(split[0]), float.Parse(split[1]), float.Parse(split[2]));
        else
            return Vector3.zero;
    }
    public string Vector3ToString(Vector3 v)
    {
        return v.x + dataSubSep + v.y + dataSubSep + v.z;
    }

    public Quaternion StringToQuaternion(string v)
    {
        string[] split = GetSplittedArrayData(v);
        if (split.Length == 4)
            return new Quaternion(float.Parse(split[0]), float.Parse(split[1]), float.Parse(split[2]), float.Parse(split[3]));
        else
            return Quaternion.identity;
    }
    public string QuaternionToString(Quaternion v)
    {
        return v.x + dataSubSep + v.y + dataSubSep + v.z + dataSubSep + v.w;
    }

    public Color StringToColor(string c)
    {
        string[] split = GetSplittedArrayData(c);
        if (split.Length == 4)
            return new Color(float.Parse(split[0]), float.Parse(split[1]), float.Parse(split[2]), float.Parse(split[3]));
        else
            return Color.white;
    }
    public string ColorToString(Color c)
    {
        return c.r + dataSubSep + c.g + dataSubSep + c.b + dataSubSep + c.a;
    }

    public GenericType StringToGenericType(string t)
    {
        string[] split = GetSplittedArrayData(t);

        if (split.Length != 2)
            return new GenericType(); ;

        switch (split[0])
        {
            case "bool":
                return new GenericType(bool.Parse(split[1]));
            case "int":
                return new GenericType(Int32.Parse(split[1]));
            case "float":
                return new GenericType(float.Parse(split[1]));
            case "string":
                return new GenericType(split[1]);
            case "Vector3":
                return new GenericType(StringToVector3(split[1]));
            default:
                return new GenericType();
        }
    }
    public string GenericTypeToString(GenericType t)
    {
        string typeString = "Null";
        string valueString = "0";
        if (t.isBool)
        {
            typeString = "bool";
            valueString = t.varBool.ToString();
        }
        else if (t.isInt)
        {
            typeString = "int";
            valueString = t.varInt.ToString();
        }
        else if (t.isFloat)
        {
            typeString = "float";
            valueString = t.varFloat.ToString();
        }
        else if (t.isString)
        {
            typeString = "string";
            valueString = t.varString.ToString();
        }
        else if (t.isVector3)
        {
            typeString = "Vector3";
            valueString = Vector3ToString(t.varVector3);
        }
        return typeString + dataSubSep + valueString;
    }

    public GenericType[] StringToGenericTypeArray(string t)
    {
        List<GenericType> types = new List<GenericType>();
        string[] split = GetSplittedArrayData(t);
        for(int i = 0; i < split.Length; ++i)
            types.Add(StringToGenericType(split[i]));
        return types.ToArray();
    }
    public string GenericTypeArrayToString(GenericType[] t)
    {
        string res = "";// t.Length + subSep;
        for(int i = 0; i < t.Length; ++i)
        {
            if (i < t.Length - 1)
                res += GenericTypeToString(t[i]) + dataSubSep;
            else
                res += GenericTypeToString(t[i]);
        }

        return res;
    }

    public string[] StringToStringArray(string t)
    {
        List<string> types = new List<string>();
        string[] split = GetSplittedArrayData(t);
        for (int i = 0; i < split.Length; ++i)
            types.Add(split[i]);
        return types.ToArray();
    }
    public string StringArrayToString(string[] t)
    {
        string res = "";// t.Length + subSep;
        for (int i = 0; i < t.Length; ++i)
        {
            if (i < t.Length - 1)
                res += t[i] + dataSubSep;
            else
                res += t[i];
        }

        return res;
    }

    public NetworkInstanceId StringToNetworkInstanceId(string i)
    {
        return new NetworkInstanceId(uint.Parse(i));
    }
    public string NetworkInstanceIdToString(NetworkInstanceId i)
    {
        return i.Value.ToString();
    }

    public List<WebRTCMessageGeneric> IsThereAnyMessagesForMe(short channel)
    {
        return IsThereAnyMessagesForMe(localId, channel);
    }

    public List<WebRTCMessageGeneric> IsThereAnyMessagesForMe(int recipient, short channel)
    {
        List<WebRTCMessageGeneric> messages = new List<WebRTCMessageGeneric>();

        for (int i = 0; i < unreadMessages.Count; ++i)
        {
            try
            {
                if (unreadMessages[i].recipient == recipient
                    && unreadMessages[i].channel == channel)
                {
                    messages.Add(unreadMessages[i]);
                    unreadMessages.RemoveAt(i);
                }
            }
            catch(Exception e)
            {
                Debug.LogError("exception read messge : " + unreadMessages[i]);
                unreadMessages.RemoveAt(i);
            }
        }

        return messages;
    }

    public void SendNetworkEvent(string data)
    {
        SendMessage(0, data);
    }

    public void SendMessage(short channel, string data)
    {
        SendMessage(remoteId, channel, data);
    }

    public void SendMessage(int recipient, short channel, string data)
    {
        if (toSendMessages == null) return;
        string msg = recipient + sep + channel + sep + DateTimeOffset.Now.ToUnixTimeMilliseconds() + sep + data;
        toSendMessages.Add(msg);
    }

    //public void SendMessage(string recipient, string action, string data)
    //{
    //    string msg = recipient + sep + action + sep + data;
    //    dc.SendMessage(Encoding.ASCII.GetBytes(msg));
    //}

    public void SendMessage(int recipient, short channel, float x, float y)
    {
        string data = x + sep + y;
        SendMessage(recipient, channel, data);
    }

    public void SendMessage(int recipient, short channel, string name, Vector3 p, Quaternion q, Vector3 s)
    {
        string data = name + sep;
        data += p.x + sep + p.y + sep + p.z + sep;
        data += q.x + sep + q.y + sep + q.z + sep + q.w + sep;
        data += s.x + sep + s.y + sep + s.z;
        SendMessage(recipient, channel, data);
    }

    public void SendMessage(int recipient, short channel, string name, Vector3 p, Quaternion q, Vector3 s, float startWidth, float endWidth, Vector3[] pts)
    {
        string data = name + sep;
        data += p.x + sep + p.y + sep + p.z + sep;
        data += q.x + sep + q.y + sep + q.z + sep + q.w + sep;
        data += s.x + sep + s.y + sep + s.z + sep;
        data += startWidth + sep;
        data += endWidth + sep;
        data += pts.Length;

        for (int i = 0; i < pts.Length; ++i)
            data += sep + pts[i].x + sep + pts[i].y + sep + pts[i].z;

        SendMessage(recipient, channel, data);
    }

}
