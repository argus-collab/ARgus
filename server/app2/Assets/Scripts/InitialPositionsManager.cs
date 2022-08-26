using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitialPositionsManager : MonoBehaviour
{
    public GameObject sceneRoot;

    private List<GameObject> sceneStateAtRecord;
    private List<Vector3> initialPositions;
    private List<Quaternion> initialRotations;

    private void Start()
    {
        sceneStateAtRecord = new List<GameObject>();
        initialPositions = new List<Vector3>();
        initialRotations = new List<Quaternion>();
    }

    public void RecordInitialPositions()
    {
        for(int i = 0; i < sceneRoot.transform.childCount; ++i)
        {
            sceneStateAtRecord.Add(sceneRoot.transform.GetChild(i).gameObject);
            initialPositions.Add(sceneRoot.transform.GetChild(i).position);
            initialRotations.Add(sceneRoot.transform.GetChild(i).rotation);
        }
    }

    public void ResetPositions()
    {
        for(int i=0;i<sceneStateAtRecord.Count;++i)
        {
            if(sceneStateAtRecord[i] != null)
            {
                sceneStateAtRecord[i].transform.position = initialPositions[i];
                sceneStateAtRecord[i].transform.rotation = initialRotations[i];
            }
        }
    }
    
}
