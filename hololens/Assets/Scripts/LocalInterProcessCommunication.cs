using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.IO.Pipes;
using System.Text;

//using System.Diagnostics;
using System.Security.Principal;



// using https://docs.microsoft.com/en-us/dotnet/standard/io/memory-mapped-files
// or : https://docs.microsoft.com/en-us/dotnet/standard/io/how-to-use-anonymous-pipes-for-local-interprocess-communication
// or : https://docs.microsoft.com/en-us/dotnet/standard/io/how-to-use-named-pipes-for-network-interprocess-communication
// or : https://forum.unity.com/threads/simple-udp-implementation-send-read-via-mono-c.15900/
public struct TransformData
{
    public float x;
    public float y;
    public float z;

    public float qx;
    public float qy;
    public float qz;
    public float qw;
}

public class LocalInterProcessCommunication : MonoBehaviour
{
    public string serverPipeName;
    // default = sender
    public bool IsReceiver = false;
    public int freq = 60;
    private float lastTimeStamp;

    //private MemoryMappedViewAccessor accessor;
    //private MemoryMappedFile mmf;

    private string fileName;

    //long offset = 0x0;
    //long length = 0x20000000;

    NamedPipeServerStream pipeServer;
    NamedPipeClientStream pipeClient;
    StreamString sw;
    StreamString sr;

    public bool once = false;

    private void OnApplicationQuit()
    {
        if (IsReceiver)
            pipeClient.Close();
        else
            pipeServer.Close();
    }

    void Start()
    {
        //fileName = @"c:\tmp-unity\" + gameObject.name + ".data";

        //if (!IsReceiver)
        //{ 
        //    // create or replace file for memory mapping
        //    FileStream fs = File.Create(fileName);

        //    TransformData d = new TransformData();

        //    int size = Marshal.SizeOf(typeof(TransformData));
        //    byte[] arr = new byte[size];

        //    IntPtr ptr = Marshal.AllocHGlobal(size);
        //    Marshal.StructureToPtr(d, ptr, true);
        //    Marshal.Copy(ptr, arr, 0, size);
        //    Marshal.FreeHGlobal(ptr);

        //    fs.Write(arr, 0, size);
        //    fs.Close();

        //    Debug.Log("create file " + fileName + " with size : " + size + " ok");

        //    //using mmf = MemoryMappedFile.CreateFromFile(fileName, FileMode.Open, gameObject.name);
        //}

        ////accessor = mmf.CreateViewAccessor(offset, Marshal.SizeOf(typeof(TransformData)));

        lastTimeStamp = Time.time;


        if(IsReceiver)
        {
            pipeClient = new NamedPipeClientStream(".", "testpipe",
                        PipeDirection.InOut, PipeOptions.None,
                        TokenImpersonationLevel.Impersonation);
            pipeClient.Connect();
            sr = new StreamString(pipeClient);
        }
        else
        {
            pipeServer = new NamedPipeServerStream("testpipe", PipeDirection.InOut, 1);
            
            sw = new StreamString(pipeServer);

            pipeServer.WaitForConnection();
            //sw.AutoFlush = true;

        }
    }

    void WriteUsingMemoryMappedFile(TransformData d)
    {
        //using (mmf = MemoryMappedFile.CreateFromFile(fileName, FileMode.Open, gameObject.name))
        using (var mmf = MemoryMappedFile.CreateNew(gameObject.name, Marshal.SizeOf(typeof(TransformData)) *2))
        {
            using (var accessor = mmf.CreateViewAccessor(0, Marshal.SizeOf(typeof(TransformData)) *2))
            {
                accessor.Write(Marshal.SizeOf(typeof(TransformData)), ref d);
            }
        }
    }

    TransformData ReadUsingMemoryMappedFile()
    {
        TransformData d = new TransformData();

        using (var mmf = MemoryMappedFile.OpenExisting(gameObject.name))
        {
            using (var accessor = mmf.CreateViewAccessor(0, Marshal.SizeOf(typeof(TransformData)) *2))
            {
                accessor.Read(Marshal.SizeOf(typeof(TransformData)), out d);
            }
        }

        return d;
    }

    void WriteUsingPipe(TransformData d)
    {


        sw.WriteString("I am the one true server!");
        string filename = sw.ReadString();

        // Read in the contents of the file while impersonating the client.
        ReadFileToStream fileReader = new ReadFileToStream(sw, filename);

        // Display the name of the user we are impersonating.
        pipeServer.RunAsClient(fileReader.Start);


    }

    TransformData ReadUsingPipe()
    {
        TransformData d = new TransformData();

        var ss = new StreamString(pipeClient);
        // Validate the server's signature string.
        if (ss.ReadString() == "I am the one true server!")
        {
            Debug.Log("Server connected");
            // The client security token is sent with the first write.
            // Send the name of the file whose contents are returned
            // by the server.
            ss.WriteString("c:\\textfile.txt");

            // Print the file to the screen.
            Console.Write(ss.ReadString());
        }
        else
        {
            Debug.Log("Server could not be verified.");
        }

        return d;
    }



    /*
     
     */

    void Update()
    {

        if (!once)
            return;


        if (IsReceiver)
        {
            //try 
            //{
                //TransformData d = ReadUsingMemoryMappedFile();
                TransformData d = ReadUsingPipe();
                    Vector3 p = new Vector3(d.x, d.y, d.z);
                Quaternion q = new Quaternion(d.qx, d.qy, d.qz, d.qw);

                gameObject.transform.position = p;
                gameObject.transform.rotation = q;
            //} 
            //catch (Exception e) { };
        }
        else
        {
            if (Time.time - lastTimeStamp < 1 / freq)
                return;

            lastTimeStamp = Time.time;

            TransformData d = new TransformData();
            d.x = gameObject.transform.position.x;
            d.y = gameObject.transform.position.y;
            d.z = gameObject.transform.position.z;

            d.qx = gameObject.transform.rotation.x;
            d.qy = gameObject.transform.rotation.y;
            d.qz = gameObject.transform.rotation.z;
            d.qw = gameObject.transform.rotation.w;

            //WriteUsingMemoryMappedFile(d);
            WriteUsingPipe(d); 
        }

        once = false;
    }

    // Defines the data protocol for reading and writing strings on our stream
    public class StreamString
    {
        private Stream ioStream;
        private UnicodeEncoding streamEncoding;

        public StreamString(Stream ioStream)
        {
            this.ioStream = ioStream;
            streamEncoding = new UnicodeEncoding();
        }

        public string ReadString()
        {
            int len = 0;

            len = ioStream.ReadByte() * 256;
            len += ioStream.ReadByte();
            byte[] inBuffer = new byte[len];
            ioStream.Read(inBuffer, 0, len);

            return streamEncoding.GetString(inBuffer);
        }

        public int WriteString(string outString)
        {
            byte[] outBuffer = streamEncoding.GetBytes(outString);
            int len = outBuffer.Length;
            if (len > UInt16.MaxValue)
            {
                len = (int)UInt16.MaxValue;
            }
            ioStream.WriteByte((byte)(len / 256));
            ioStream.WriteByte((byte)(len & 255));
            ioStream.Write(outBuffer, 0, len);
            ioStream.Flush();

            return outBuffer.Length + 2;
        }
    }

    public class ReadFileToStream
    {
        private string fn;
        private StreamString ss;

        public ReadFileToStream(StreamString str, string filename)
        {
            fn = filename;
            ss = str;
        }

        public void Start()
        {
            string contents = File.ReadAllText(fn);
            ss.WriteString(contents);
        }
    }
}

