using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Globalization;


public class RecordPlayer : MonoBehaviour
{
    private string fileName;

    private bool load = false;
    private bool play = false;

    private bool physicsActivated = true;

    private List<GameObject> objects;
    private List<Rigidbody> rbs;

    private List<List<Vector3>> positions;
    private List<List<Quaternion>> rotations;

    private List<List<Vector3>> positionsToRead;
    private List<List<Quaternion>> rotationsToRead;

    void Start()
    {
        objects = new List<GameObject>();
        rbs = new List<Rigidbody>();

        positions = new List<List<Vector3>>();
        rotations = new List<List<Quaternion>>();

        positionsToRead = new List<List<Vector3>>();
        rotationsToRead = new List<List<Quaternion>>();
    }



    public Vector3 GetFirstPosition(string joint)
    {
        int i=0;
        for(;i<objects.Count;++i)
        {
            if (objects[i].transform.parent != null && objects[i].transform.parent.name + "/" + objects[i].name == joint)
                break;
            if (objects[i].transform.parent == null && objects[i].name == joint)
                break;
        }

        if (i == objects.Count)
            return Vector3.zero;

        return positions[i][0];
    }

    public Quaternion GetFirstRotation(string joint)
    {
        int i = 0;
        for (; i < objects.Count; ++i)
        {
            if (objects[i].transform.parent != null && objects[i].transform.parent.name + "/" + objects[i].name == joint)
                break;
            if (objects[i].transform.parent == null && objects[i].name == joint)
                break;
        }

        if (i == objects.Count)
            return Quaternion.identity;

        return rotations[i][0];
    }

    public void LoadRecord()
    {
        Load();
    }

    public void PlayRecord()
    {
        LoadRecord();
        play = true;
    }

    public void StopRecord()
    {
        play = false;
    }

    void FixedUpdate()
    {
        if(load)
        {
            load = false;
            Load();
        }
        else if(play)
        {
            if(physicsActivated)
            {
                foreach (Rigidbody rb in rbs)
                    rb.isKinematic = true;
                physicsActivated = false;
            }

            // do play
            if(positionsToRead.Count > 0 && positionsToRead[0].Count > 0)
            {
                for(int i=0;i<objects.Count;i++)
                {
                    objects[i].transform.position = positionsToRead[i][0];
                    objects[i].transform.rotation = rotationsToRead[i][0];

                    positionsToRead[i].RemoveAt(0);
                    rotationsToRead[i].RemoveAt(0);
                }
            }
            else
            {
                play = false;

                positionsToRead = positions;
                rotationsToRead = rotations;

                foreach (Rigidbody rb in rbs)
                    rb.isKinematic = false;
                physicsActivated = true;
            }
        }

        
    }

    void Load()
    {
        string filePath = Application.dataPath + "/Logs/" + fileName + ".csv";
        StreamReader reader = new StreamReader(filePath);
        reader.ReadLine(); // legend

        while (!reader.EndOfStream)
        {
            string line = reader.ReadLine();
            string[] splitArray = line.Split(char.Parse(";"));

            if (splitArray.Length > 8)
            {
                //times.Add(float.Parse(splitArray[0], CultureInfo.InvariantCulture));

                if (splitArray[1].Contains("Client"))
                    splitArray[1] = splitArray[1].Remove(splitArray[1].Length - 6);

                
                GameObject go = null;

                if (splitArray[1].Contains("/"))
                {
                    string[] splitArrayName = splitArray[1].Split(char.Parse("/"));
                    string[] popedSplitArrayName = new string[splitArrayName.Length - 1];
                    for (int i = 0; i < popedSplitArrayName.Length; ++i)
                        popedSplitArrayName[i] = splitArrayName[i + 1];

                    go = FindRecursively(GameObject.Find(splitArrayName[0]), popedSplitArrayName);
                }
                else
                {
                    go = GameObject.Find(splitArray[1]);
                }


                if (go == null)
                {
                    Debug.Log("Scene altered since recording. Please fix this mess.");
                    Debug.Log("GO name : " + splitArray[1]);
                    break;
                }

                Vector3 position;
                position.x = float.Parse(splitArray[2].Replace(",", "."), CultureInfo.InvariantCulture);
                position.y = float.Parse(splitArray[3].Replace(",", "."), CultureInfo.InvariantCulture);
                position.z = float.Parse(splitArray[4].Replace(",", "."), CultureInfo.InvariantCulture);

                Quaternion rotation;
                rotation.x = float.Parse(splitArray[5].Replace(",", "."), CultureInfo.InvariantCulture);
                rotation.y = float.Parse(splitArray[6].Replace(",", "."), CultureInfo.InvariantCulture);
                rotation.z = float.Parse(splitArray[7].Replace(",", "."), CultureInfo.InvariantCulture);
                rotation.w = float.Parse(splitArray[8].Replace(",", "."), CultureInfo.InvariantCulture);

                if (objects.Contains(go))
                {
                    int i = objects.IndexOf(go);

                    positions[i].Add(position);
                    rotations[i].Add(rotation);
                }
                else
                {
                    objects.Add(go);

                    Rigidbody rb = go.GetComponent<Rigidbody>();
                    if (rb != null)
                        rbs.Add(rb);

                    positions.Add(new List<Vector3>());
                    positions[positions.Count - 1].Add(position);

                    rotations.Add(new List<Quaternion>());
                    rotations[rotations.Count - 1].Add(rotation);
                }
            }
        }

        reader.Close();

        positionsToRead = positions;
        rotationsToRead = rotations;

        Debug.Log("loading OK");
        Debug.Log("positionsToRead size : " + positionsToRead.Count);
        Debug.Log("rotationsToRead size : " + rotationsToRead.Count);
    }

    public GameObject FindRecursively(GameObject parent, string[] names)
    {
        if (names.Length == 0)
            return null;

        if (parent == null)
            return null;

        for (int i = 0; i < parent.transform.childCount; ++i)
        {
            if (parent.transform.GetChild(i).name == names[0])
            {
                if (names.Length == 1)
                    return parent.transform.GetChild(i).gameObject;
                else
                {
                    string[] newNames = new string[names.Length - 1];
                    for (int j = 0; j < newNames.Length; ++j)
                        newNames[j] = names[j+1];
                    FindRecursively(parent.transform.GetChild(i).gameObject, newNames);
                }
            }
        }

        return null; // no child
    }

    public bool IsPlaying()
    {
        return play;
    }

    public void SetFileName(string fileName)
    {
        this.fileName = fileName;
    }
}
