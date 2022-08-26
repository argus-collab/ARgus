using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class LogManager : MonoBehaviour
{
    public CustomClientNetworkManager clientManager;
    private List<string> logs;
    private string startingTime;
    public bool isServer = false;

    // state
    enum ViewPoint
    {
        Virtual,
        Hololens,
        Kinect
    }
    private ViewPoint curVp = ViewPoint.Virtual;

    private float lastViewTimeStamp;
    private float lastStickTimeStamp;
    private float lastSphericalViewTimeStamp;
    private float lastPreviewTimeStamp;
    private float lastFreezeTimeStamp;

    private void Start()
    {
        logs = new List<string>();
        startingTime = System.DateTime.Now.ToString();
    }

    void OnApplicationQuit()
    {
        if (isServer)
        {
            ExportLog();
        }
    }

    string GetWellFormattedTime()
    {
        return Time.time.ToString().Replace(",", ".");
    }

    void ExportLog()
    {
        startingTime = startingTime.Replace(" ", "_").Replace("/", "-").Replace(":", "-");
        string path = "Assets/Logs/Log_" + startingTime + ".csv";

        StreamWriter writer = new StreamWriter(path, true);

        for (int i = 0; i < logs.Count; ++i)
            writer.WriteLine(logs[i]);

        writer.Close();
    }

    public void LogHololensView() { LogEntry(GetWellFormattedTime() + ",hololens view"); }
    public void LogKinectView() { LogEntry(GetWellFormattedTime() + ",kinect view"); }
    public void LogVirtualView() { LogEntry(GetWellFormattedTime() + ",virtual view"); }

    public void LogStartStickUse() { LogEntry(GetWellFormattedTime() + ",start using stick"); }
    public void LogStopStickUse() { LogEntry(GetWellFormattedTime() + ",stop using stick"); }
    public void LogPlaceSticker() { LogEntry(GetWellFormattedTime() + ",place sticker"); }

    public void LogStartSphericalCameraUse() { LogEntry(GetWellFormattedTime() + ",start use spherical camera"); }
    public void LogStopSphericalCameraUse() { LogEntry(GetWellFormattedTime() + ",stop use spherical camera"); }

    //public void LogShowHelp() { LogEntry(Time.time + " show help"); }
    public void LogShowCameraPosition() { LogEntry(GetWellFormattedTime() + ",show camera position"); }
    public void LogCameraPositionRecord() { LogEntry(GetWellFormattedTime() + ",camera position record"); }
    public void LogResetVirtualCameraPosition() { LogEntry(GetWellFormattedTime() + ",reset virtual camera position"); }

    public void LogStartPreviewHololens() { LogEntry(GetWellFormattedTime() + ",start preview hololens"); }
    public void LogStartPreviewKinect() { LogEntry(GetWellFormattedTime() + ",start preview kinect"); }
    public void LogStartPreviewVirtual() { LogEntry(GetWellFormattedTime() + ",start preview virtual"); }
    public void LogStopPreview() { LogEntry(GetWellFormattedTime() + ",stop preview"); }

    public void LogStartFreezeHololensView() { LogEntry(GetWellFormattedTime() + ",start freeze hololens view"); }
    public void LogStopFreezeHololensView() { LogEntry(GetWellFormattedTime() + ",stop freeze hololens view"); }

    public string GetLogsAsString()
    {
        string result = "";
        for (int i = 0; i < logs.Count; ++i)
            result += logs[i] + "\n";
        return result;
    }

    public string GetLogLineAsString(int i)
    {
        if (i < logs.Count)
            return logs[i];
        else
            return "no log line for " + i;
    }

    public void LogEntry(string entry)
    {
        if (!isServer)
            clientManager.AskForLogEntry(entry);

        InterpretLog(entry);
        //Debug.Log("log entry : " + entry);
    }

    void InterpretLog(string log)
    {
        System.Globalization.NumberFormatInfo numberFormat = (System.Globalization.NumberFormatInfo)System.Globalization.CultureInfo.InstalledUICulture.NumberFormat.Clone();
        numberFormat.NumberDecimalSeparator = ".";

        float timeStamp;
        string msg;
        if (isServer)
        {
            string[] line = log.Split(',');
            msg = line[2];
            timeStamp = float.Parse(line[1], numberFormat);
        }
        else
        {
            string[] line = log.Split(',');
            msg = line[1];
            timeStamp = float.Parse(line[0], numberFormat);
        }

        //Debug.Log("time stamp : " + timeStamp);
        //Debug.Log("msg : " + msg);


        if (msg == "hololens view") curVp = ViewPoint.Hololens;
        if (msg == "kinect view") curVp = ViewPoint.Kinect;
        if (msg == "virtual view") curVp = ViewPoint.Virtual;

        if (msg == "start using stick"
            || msg == "stop using stick"
            || msg == "place sticker"
            || msg == "start use spherical camera"
            || msg == "stop use spherical camera"
            || msg == "start preview hololens"
            || msg == "start preview virtual"
            || msg == "start preview kinect"
            || msg == "stop preview")
            log += "," + GetCurrentViewPoint();
        else
            log += ",na";




        if (msg == "start using stick") lastStickTimeStamp = timeStamp;
        if (msg == "start use spherical camera") lastSphericalViewTimeStamp = timeStamp;
        if (msg == "start preview hololens") lastPreviewTimeStamp = timeStamp;
        if (msg == "start preview kinect") lastPreviewTimeStamp = timeStamp;
        if (msg == "start preview virtual") lastPreviewTimeStamp = timeStamp;
        if (msg == "start freeze hololens view") lastFreezeTimeStamp = timeStamp;



        if (msg == "hololens view"
            || msg == "kinect view"
            || msg == "virtual view")
        {
            log += "," + (timeStamp - lastViewTimeStamp);
            lastViewTimeStamp = timeStamp;
        }

        else if (msg == "stop using stick") log += "," + (timeStamp - lastStickTimeStamp);
        else if(msg == "stop use spherical camera") log += "," + (timeStamp - lastSphericalViewTimeStamp);
        else if(msg == "stop preview") log += "," + (timeStamp - lastPreviewTimeStamp);
        else if(msg == "stop freeze hololens view") log += "," + (timeStamp - lastFreezeTimeStamp);
        else log += ",na";

        //Debug.Log(log);

        logs.Add(log);
    }

    string GetCurrentViewPoint()
    {
        if (curVp == ViewPoint.Hololens) return "hololens";
        else if (curVp == ViewPoint.Kinect) return "kinect";
        else return "virtual";
    }

    public void ClearLogs()
    {
        logs.Clear();
    }
}
