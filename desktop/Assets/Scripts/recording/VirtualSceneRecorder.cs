using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class VirtualSceneRecorder : MonoBehaviour
{
    private GameObject head;
    private GameObject rightHand;
    private GameObject leftHand;
    public Recorder recorder;
    public GameObject virtualSceneRoot;

    public List<GameObject> additionnalObjectsToRecord = new List<GameObject>();

    public int uid;
    public int taskid;

    private bool record = false;
    private bool askForRecording = false;

    public bool IsRecording()
    {
        return record;
    }

    void Update()
    {
        SearchPlayerToRecord(); 

        if(askForRecording)
        {
            Debug.Log("ask for recording");
            askForRecording = false;
            // at least one hand visible
            //if (!record && head != null && (rightHand != null || leftHand != null))
            if (!record)
                StartRecorder();
        }
    }

    void AddHandToRecorder(GameObject hand)
    {
        for(int i=0;i<hand.transform.childCount;++i)
            recorder.AddToRecordGO(hand.transform.GetChild(i).gameObject, true);
    }

    void StartRecorder()
    {
        record = true;
        recorder.Reset();

        // add additional objects
        foreach (GameObject go in additionnalObjectsToRecord)
            recorder.AddToRecordGO(go);

        if (head != null)
            recorder.AddToRecordGO(head);

        if (rightHand != null)
            AddHandToRecorder(rightHand);

        if (leftHand != null)
            AddHandToRecorder(leftHand);

        for(int i = 0; i < virtualSceneRoot.transform.childCount; ++i)
        {
            Debug.Log("add to record : " + virtualSceneRoot.transform.GetChild(i).name);
            recorder.AddToRecordGO(virtualSceneRoot.transform.GetChild(i).gameObject);
        }

        //recorder.SetFileName(head.transform.parent.name);
        Debug.Log("set file name : " + "traj_" + uid + "_task" + taskid);
        recorder.SetFileName("traj_" + uid + "_task" + taskid);
        recorder.Init();
        recorder.StartRecording();

        Debug.Log("head : " + head);
        Debug.Log("right hand : " + rightHand);
        Debug.Log("left hand : " + leftHand);
        //Debug.Log("file name : " + head.transform.parent.name);
    }

    public void StartRecording()
    {
        askForRecording = true;
    }

    public void StopRecording()
    {
        recorder.StopRecording();
        recorder.ExportInFile();
        record = false;
    }

    void SearchPlayerToRecord()
    {
        
        if(head == null)
            head = GameObject.Find("Head");
        
        if (rightHand == null)
            rightHand = GameObject.Find("Right_hand");
        
        if (leftHand == null)
            leftHand = GameObject.Find("Left_hand");
    }

}
