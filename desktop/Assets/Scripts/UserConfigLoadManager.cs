using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class UserConfigLoadManager : MonoBehaviour
{
    public ScenePartManager scenePartManager;
    //public UDPSceneManager udpScenePartManager;
    public ServerColorManager colorManager;
    public int nbTasks = 4;
    public bool displayGUI = true;

    List<Action> executionQueue;

    private string userId = "";

    private void Start()
    {
        executionQueue = new List<Action>();
    }

    private void OnGUI()
    {
        if (displayGUI)
        {
            userId = GUI.TextField(new Rect(800, 10, 200, 20), userId, 50);

            if (GUI.Button(new Rect(1050, 10, 100, 20), "Tuto hololens"))
            {
                LoadTutoHololens();
            }

            if (GUI.Button(new Rect(1200, 10, 100, 20), "Tuto kinect"))
            {
                LoadTutoKinect();
            }

            for (int i = 0; i < nbTasks; ++i)
            {
                int taskIndex = i;
                if (GUI.Button(new Rect(800 + 110 * i, 40, 100, 20), "task " + (taskIndex + 1)))
                {
                    executionQueue.Add(new Action(() => { scenePartManager.TextualCommand("remove all ok"); }));
                    executionQueue.Add(new Action(() => { ProcessFile("config_" + userId + "_task" + taskIndex); }));
                    //udpScenePartManager.Pause(1.0f);
                }
            }
        }
    }

    public void LoadTutoHololens()
    {
        executionQueue.Add(new Action(() => { scenePartManager.TextualCommand("remove all ok"); }));
        executionQueue.Add(new Action(() => { ProcessFile("tuto_hololens"); }));
    }

    public void LoadTutoKinect()
    {
        executionQueue.Add(new Action(() => { scenePartManager.TextualCommand("remove all ok"); }));
        executionQueue.Add(new Action(() => { ProcessFile("tuto_kinect"); }));
    }

    public void LoadTutoVirtual()
    {
        executionQueue.Add(new Action(() => { scenePartManager.TextualCommand("remove all ok"); }));
        executionQueue.Add(new Action(() => { ProcessFile("tuto_virtual"); }));
    }

    public void LoadTask(int uid, int taskid)
    {
        executionQueue.Add(new Action(() => { scenePartManager.TextualCommand("remove all ok"); }));
        executionQueue.Add(new Action(() => { ProcessFile("config_" + uid + "_task" + taskid); }));
    }

    private void Update()
    {
        if (executionQueue.Count > 0)
        {
            executionQueue[0]();
            executionQueue.RemoveAt(0);
        }
    }

    void ProcessFile(string fileName)
    {

        string path = "Assets/Files/" + fileName + ".txt";

        try
        {
            StreamReader reader = new StreamReader(path);

            string line;
            while ((line = reader.ReadLine()) != null)
            {
                string[] param = line.Split(' ');
                string type = param[0];

                if (type == "part" || type == "support")
                {
                    string objectName = param[1];
                    Vector3 position = new Vector3(float.Parse(param[2]), float.Parse(param[3]), float.Parse(param[4]));
                    //Quaternion rotation = Quaternion.identity;
                    Quaternion rotation = new Quaternion(float.Parse(param[5]), float.Parse(param[6]), float.Parse(param[7]), float.Parse(param[8]));

                    scenePartManager.AddGameObjectInScenePart(objectName, position, rotation, false);
                }
                else if (type == "sized-part")
                {
                    string objectName = param[1];
                    Vector3 position = new Vector3(float.Parse(param[2]), float.Parse(param[3]), float.Parse(param[4]));
                    //Quaternion rotation = Quaternion.identity;
                    Quaternion rotation = new Quaternion(float.Parse(param[5]), float.Parse(param[6]), float.Parse(param[7]), float.Parse(param[8]));

                    float scale = float.Parse(param[9]);

                    scenePartManager.AddGameObjectInScenePart(objectName, position, rotation, scale, false);
                }
                else if (type == "custom-support")
                {
                    string objectName = param[1];
                    Vector3 position = new Vector3(float.Parse(param[2]), float.Parse(param[3]), float.Parse(param[4]));
                    Quaternion rotation = Quaternion.identity;

                    scenePartManager.AddGameObjectInScenePart(objectName, position, rotation, false);

                    //executionQueue.Add(new Action(() => { scenePartManager.ChangeLayerDeeply(GameObject.Find(objectName).transform, 8); }));
                    executionQueue.Add(new Action(() => { scenePartManager.ChangeLayerDeeply(GameObject.Find(objectName + "Client").transform, 8); }));

                    int dimX = int.Parse(param[5]);
                    int dimY = int.Parse(param[6]);
                    float squareSize = float.Parse(param[7]);
                    bool transversalBorders = bool.Parse(param[8]);

                    SupportGenerator[] generators = GameObject.FindObjectsOfType<SupportGenerator>();
                    for (int i = 0; i < generators.Length; ++i)
                    {
                        generators[i].nbCaseX = dimX;
                        generators[i].nbCaseY = dimY;
                        generators[i].squareSize = squareSize;
                        generators[i].transversalBorders = transversalBorders;
                        generators[i].Regenerate();
                    }

                    string[] paramsNames = new string[5] { "nbCaseX", "nbCaseY", "squareSize", "regenerate", "transversalBorders" };
                    GenericType[] paramsValues = new GenericType[5]
                    {
                        new GenericType(dimX),
                        new GenericType(dimY),
                        new GenericType(squareSize),
                        new GenericType(true),
                        new GenericType(transversalBorders)
                    };

                    scenePartManager.UpdateGameObjectComponentInScenePart(objectName, "SupportGenerator", paramsNames, paramsValues);
                }
                else if(type == "color")
                {
                    colorManager.ResetScene();
                    if (param[1] == "on")
                        executionQueue.Add(new Action(() => { colorManager.RestoreColors(); }));
                    else if (param[1] == "off")
                        executionQueue.Add(new Action(() => { colorManager.HideColors(); }));
                }
            }
            reader.Close();
        }
        catch(Exception e)
        {
            Debug.LogError("file not found");
        }
    }

}
