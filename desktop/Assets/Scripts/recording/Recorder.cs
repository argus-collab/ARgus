using UnityEngine;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System;


// WARNING
// physics must run at the same frequency on recorder and player
public class Recorder : MonoBehaviour
{
    private static int MAX_BUFFER_SIZE = 125 * (int)Math.Pow(10,6); // 500mo de float max par game object
    private int MAX_NB_DATA;

    private List<GameObject> toRecord;
    private List<string> toRecordNames;
    private string fileName;
    private string filePath;

    public int updateFrequency = 30;
    private float updateTime;

    private bool record = false;
    private bool export = false;

    private string history = "";

    private float[] positions;
    private float[] rotations;
    private float[] times;

    private int index = 0;

    private string sep = ";";

    private float lastTimeUpdate;
    private float debugLastTimeStamp;


    private void Start()
    {
        toRecord = new List<GameObject>();
        toRecordNames = new List<string>();
}

    public void AddToRecordGO(GameObject toRecordGO, bool isHand = false)
    {
        toRecord.Add(toRecordGO);
        if(isHand)
            toRecordNames.Add(toRecordGO.transform.parent.name + "/" + toRecordGO.name);
        else
            toRecordNames.Add(toRecordGO.name);
    }

    public void SetFileName(string name)
    {
        //fileName = name.Replace(".", "-").Replace("/", "-").Replace(":", "-");
        fileName = name;
    }

    public void StartRecording()
    {
        record = true;
    }

    public void StopRecording()
    {
        record = false;
    }

    public void Reset()
    {
        toRecord.Clear();
    }

    public void ExportInFile()
    {
        export = true;
    }

    public void Init()
    {
        MAX_NB_DATA = MAX_BUFFER_SIZE / (4 * toRecord.Count);

        //Debug.Log("MAX_BUFFER_SIZE : " + MAX_BUFFER_SIZE);
        //Debug.Log("MAX_NB_DATA : " + MAX_NB_DATA);
        //Debug.Log("you have : " + MAX_NB_DATA * Time.fixedDeltaTime + "s to record");
        //Debug.Log("you have : " + MAX_NB_DATA * Time.fixedDeltaTime / 60 + "min to record");
        //Debug.Log("you have : " + MAX_NB_DATA * Time.fixedDeltaTime / 3600 + "h to record");

        positions = new float[MAX_NB_DATA * 3 * toRecord.Count];
        rotations = new float[MAX_NB_DATA * 4 * toRecord.Count];
        times = new float[MAX_NB_DATA * toRecord.Count];

        history += "time";
        history += sep;

        history += "name";
        history += sep;

        history += "x";
        history += sep;
        history += "y";
        history += sep;
        history += "z";
        history += sep;

        history += "qx";
        history += sep;
        history += "qy";
        history += sep;
        history += "qz";
        history += sep;
        history += "qw";
        history += "\n";

        updateTime = 1 / updateFrequency;
    }

    void FixedUpdate()
    {
        if(Time.time - lastTimeUpdate > updateTime)
        {
            if (record)
            {
                RecordState();
            }

            if (export)
            {
                export = false;

                if (index * toRecord.Count > 0)
                {
                    Debug.Log("length : " + index * toRecord.Count);
                    Debug.Log("filename : " + fileName);
                    Debug.Log("LAUNCH THREAD TO EXPORT DATA IN CSV");

                    filePath = Application.dataPath + "/Logs/" + fileName + ".csv";
                    var thread = new Thread(FileExport);
                    thread.Start();
                }
                else
                {
                    Debug.LogError("no data to record !");
                }


            }

            lastTimeUpdate = Time.time;
        }

    }

    void FileExport()
    {
        //int STRING_INT_LIMIT = 4096;
        history = "";

        Debug.Log("about to format history string...");

        StreamWriter outStream = System.IO.File.CreateText(filePath);

        // write history in a single string var
        for (int i = 0; i < index; ++i)
        {
            for (int j = 0; j < toRecord.Count; ++j)
            {
                history += times[i];
                history += sep;

                history += toRecordNames[j];
                history += sep;

                history += positions[i * toRecord.Count * 3 + j * 3 + 0];
                history += sep;
                history += positions[i * toRecord.Count * 3 + j * 3 + 1];
                history += sep;
                history += positions[i * toRecord.Count * 3 + j * 3 + 2];
                history += sep;

                history += rotations[i * toRecord.Count * 4 + j * 4 + 0];
                history += sep;
                history += rotations[i * toRecord.Count * 4 + j * 4 + 1];
                history += sep;
                history += rotations[i * toRecord.Count * 4 + j * 4 + 2];
                history += sep;
                history += rotations[i * toRecord.Count * 4 + j * 4 + 3];
                //history += "\n";

                //if (history.Length > STRING_INT_LIMIT)
                //{
                    outStream.WriteLine(history);
                    history = "";
                //}
            }
        }

        //Debug.Log("history string length : " + history.Length);


        //outStream.WriteLine(history);
        //outStream.WriteLine("piloupilou");

        outStream.Close();
        Debug.Log("EXPORT DONE !");
    }

    void RecordState()
    {
        if (index < MAX_NB_DATA)
        {
            for (int i = 0; i < toRecord.Count; ++i)
            {
                positions[toRecord.Count * index * 3 + i * 3 + 0] = toRecord[i].transform.position.x;
                positions[toRecord.Count * index * 3 + i * 3 + 1] = toRecord[i].transform.position.y;
                positions[toRecord.Count * index * 3 + i * 3 + 2] = toRecord[i].transform.position.z;

                rotations[toRecord.Count * index * 4 + i * 4 + 0] = toRecord[i].transform.rotation.x;
                rotations[toRecord.Count * index * 4 + i * 4 + 1] = toRecord[i].transform.rotation.y;
                rotations[toRecord.Count * index * 4 + i * 4 + 2] = toRecord[i].transform.rotation.z;
                rotations[toRecord.Count * index * 4 + i * 4 + 3] = toRecord[i].transform.rotation.w;

                times[index] = Time.time;
            }

            index++;
            //Debug.Log("> " + index + " / " + MAX_NB_DATA);
        }
        else
        {
            Debug.LogError("TRANSFORM RECORDING OUT OF BOUNDS");
        }
    }
}
