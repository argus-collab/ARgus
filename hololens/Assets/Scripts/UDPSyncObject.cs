using UnityEngine;
using System.Collections;

using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

public class UDPSyncObject : MonoBehaviour
{
    private string IP = "127.0.0.1";  // define in init
    public int port = 8051;

    IPEndPoint remoteEndPoint;
    UdpClient client;

    public bool isReceiver = false;
    public bool initAtStart = false;

    private Vector3 lastReceivedPosition;
    private Quaternion lastReceivedRotation;
    private Vector3 lastReceivedScale;

    private void Start()
    {
        if (initAtStart)
            Init();
    }

    public void Init()
    {
        if (isReceiver)
            InitClient();
        else
            InitServer();
    }

    public void InitServer()
    {
        remoteEndPoint = new IPEndPoint(IPAddress.Parse(IP), port);
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
            using (client = new UdpClient(port))
            {
                while (true)
                {
                    var receivedResults = await client.ReceiveAsync();
                    ReceiveData(receivedResults.Buffer);
                }
            }
        });
    }


    private void SendData()
    {
        try
        {
            string message = "";
            message += transform.position.x + ";";
            message += transform.position.y + ";";
            message += transform.position.z + ";";
            message += transform.rotation.x + ";";
            message += transform.rotation.y + ";";
            message += transform.rotation.z + ";";
            message += transform.rotation.w + ";";
            message += transform.localScale.x + ";";
            message += transform.localScale.y + ";";
            message += transform.localScale.z;

            byte[] data = Encoding.UTF8.GetBytes(message);
            client.Send(data, data.Length, remoteEndPoint);
        }
        catch (Exception err)
        {
            print(err.ToString());
        }
    }

    private void Update()
    {
        if (isReceiver)
        {
            transform.position = lastReceivedPosition;
            transform.rotation = lastReceivedRotation;
            transform.localScale = lastReceivedScale;
        }
        else
            SendData();
    }

    private void ReceiveData(byte[] data)
    {
        //client = new UdpClient(port);

        //while (true)
        //{
        //try
        //{
        //IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
        ////byte[] data = client.Receive(ref anyIP);
        //byte[] data = client.EndReceive(res, ref anyIP);

        string text = Encoding.UTF8.GetString(data);
        string[] s = text.Split(';');

        Vector3 p = new Vector3(
            float.Parse(s[0]),
            float.Parse(s[1]),
            float.Parse(s[2]));

        Quaternion q = new Quaternion();
        q.x = float.Parse(s[3]);
        q.y = float.Parse(s[4]);
        q.z = float.Parse(s[5]);
        q.w = float.Parse(s[6]);

        Vector3 ls = new Vector3(
            float.Parse(s[7]),
            float.Parse(s[8]),
            float.Parse(s[9]));

        lastReceivedPosition = p;
        lastReceivedRotation = q;
        lastReceivedScale = ls;

        //}
        //catch (Exception err)
        //{
        //    print(err.ToString());
        //}
        //}
    }
}
