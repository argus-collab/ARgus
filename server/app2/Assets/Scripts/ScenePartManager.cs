using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;

public class ScenePartManager : MonoBehaviour
{
    public CustomServerNetworkManager network;
    public GameObject targetScene;
    //public GameObject virtualScene;
    public string remoteSceneName;
    public bool local = true;
    public float freq = 30;

    public List<string> sceneState;
    public List<string> lastSceneState;



    public bool displayUI = false;

    //public int freq = 30;
    private float lastTimeStamp;

    private string userCommand = "";

    private string[] pump = 
    {
        "Cylinder_1",
        "Cylinder_2",
        "Cylinder_3",

        "Piston_1",
        "Piston_2",
        "Piston_3",

        "empty_structural",
        
        "polySurface2", 
        "polySurface3", 
        "polySurface9", 
        "polySurface10", 
        "polySurface11", 
        "polySurface12", 
        "polySurface13"
    };

    private void Start()
    {
        lastTimeStamp = Time.time;
        sceneState = new List<string>();
        lastSceneState = new List<string>();

    }

    void OnGUI()
    {
        if (displayUI)
        {
            userCommand = GUI.TextField(new Rect(300, 10, 200, 20), userCommand, 50);

            string[] args = userCommand.Split(' ');
            if (args[args.Length - 1] == "ok")
                ProcessUserCommand(userCommand);
        }
    }

    void UpdatePositions()
    {
        for (int i = 0; i < targetScene.transform.childCount; ++i)
        {
            Transform child = targetScene.transform.GetChild(i);

            // bypass unet network transform : transform are updated through network with this component
            NetworkTransform nt = child.GetComponent<NetworkTransform>();
            if (nt != null) nt.enabled = false;

            if (child.GetComponent<NetworkMaster>() != null)
            {
                bool useUnet = (child.GetComponent<ForceUnet>() != null);

                if (local)
                    network.SendSceneGameObjectTransform(child.name, child.localPosition, child.localRotation, child.localScale, local, useUnet);
                else
                    network.SendSceneGameObjectTransform(child.name, child.position, child.rotation, child.localScale, local, useUnet);

            }
        }
    }

    bool IsInScene(GameObject go)
    {
        bool res = false;
        for (int i = 0; !res && i < targetScene.transform.childCount; ++i)
            res = (go == targetScene.transform.GetChild(i).gameObject);
        return res;
    }

    void UpdateScene()
    {
        // remove deleted object
        //for(int i = 0; i < sceneState.Count; ++i)
        //{
        //    if (!IsInScene(sceneState[i]))
        //    {
        //        network.RemoveGameObjectInScenePart(sceneState[i].name);
        //        sceneState.Remove(sceneState[i]);
        //    }
        //}

        //// add added object
        //for (int i = 0; i < targetScene.transform.childCount; ++i)
        //{
        //    if (!sceneState.Contains(targetScene.transform.GetChild(i).gameObject))
        //        sceneState.Add(targetScene.transform.GetChild(i).gameObject);
        //}

        sceneState.Clear();

        // update scene state
        for (int i = 0; i < targetScene.transform.childCount; ++i)
            sceneState.Add(targetScene.transform.GetChild(i).gameObject.name);

        // remove
        for (int i = 0; i < lastSceneState.Count; ++i)
        {
            if (!sceneState.Contains(lastSceneState[i]))
            {
                network.RemoveGameObjectInScenePart(lastSceneState[i]);
            }
        }

        lastSceneState.Clear();
        for (int i = 0; i < sceneState.Count; ++i)
        {
            lastSceneState.Add(sceneState[i]); // copy
        }
    }

    void Update()
    {
        if (Time.time - lastTimeStamp > (1 / freq))
        {
            lastTimeStamp = Time.time;

            UpdateScene();
            UpdatePositions();
        }
    }

    public void TextualCommand(string command)
    {
        string[] args = command.Split(' ');
        if (args[args.Length - 1] == "ok")
            ProcessUserCommand(command);
    }

    private void ProcessUserCommand(string command)
    {
        userCommand = "";
        string[] args = command.Split(' ');

        if (args.Length < 2)
            return;

        if (args[0] == "color")
        {
            bool hasChildren = (args[3] == "true");

            network.UpdateGameObjectColorInScenePart(args[1], args[2], hasChildren);
        }
        else if(args[0] == "pop")
        {
            if(args[1] == "pump")
            {
                for(int i=0;i<pump.Length;++i)
                    //network.AddGameObjectInScenePart(pump[i], targetScene.name, virtualScene.name, remoteSceneName);
                    AddGameObjectInScenePart(pump[i], targetScene.name, /*virtualScene.name,*/ remoteSceneName, false);
            }
            else
            {
                //network.AddGameObjectInScenePart(args[1], targetScene.name, virtualScene.name, remoteSceneName, Vector3.zero, Quaternion.identity);
                AddGameObjectInScenePart(args[1], targetScene.name, /*virtualScene.name,*/ remoteSceneName, Vector3.zero, Quaternion.identity, false);
            }
        }
        else if (args[0] == "remove")
        {
            if (args[1] == "all")
            {
                for(int i=0;i< targetScene.transform.childCount;++i)
                {
                    Debug.Log("remove " + targetScene.transform.GetChild(i).name);
                    network.RemoveGameObjectInScenePart(targetScene.transform.GetChild(i).name);
                    
                }
            }
            else
            {
                network.RemoveGameObjectInScenePart(args[1]);
            }
        }
        else if (args[0] == "file")
        {
            string path = "Assets/Files/study1/" + args[1] + ".txt";

            StreamReader reader = new StreamReader(path);

            if (reader != null)
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] param = line.Split(' ');
                    string type = param[0];

                    if (type == "part" || type == "support")
                    {
                        string objectName = param[1];
                        Vector3 position = new Vector3(float.Parse(param[2]), float.Parse(param[3]), float.Parse(param[4]));

                        // rotation not supported - reason : lazyness and not usefull for now
                        Quaternion rotation = Quaternion.identity;

                        //AddGameObjectInScenePart(line, targetScene.name, virtualScene.name, remoteSceneName);
                        AddGameObjectInScenePart(objectName, position, rotation, true);//, targetScene.name, virtualScene.name, remoteSceneName, position, rotation);
                    }
                    else if (type == "sized-part")
                    {
                        string objectName = param[1];
                        Vector3 position = new Vector3(float.Parse(param[2]), float.Parse(param[3]), float.Parse(param[4]));

                        // rotation not supported - reason : lazyness and not usefull for now
                        Quaternion rotation = Quaternion.identity;

                        float scale = float.Parse(param[9]);

                        //AddGameObjectInScenePart(line, targetScene.name, virtualScene.name, remoteSceneName);
                        AddGameObjectInScenePart(objectName, position, rotation, scale, true);//, targetScene.name, virtualScene.name, remoteSceneName, position, rotation, scale);
                    }
                    else if (type == "custom-support")
                    {
                        string objectName = param[1];
                        Vector3 position = new Vector3(float.Parse(param[2]), float.Parse(param[3]), float.Parse(param[4]));

                        // rotation not supported - reason : lazyness and not usefull for now
                        Quaternion rotation = Quaternion.identity;

                        int dimX = int.Parse(param[5]);
                        int dimY = int.Parse(param[6]);
                        float squareSize = float.Parse(param[7]);

                        //AddGameObjectInScenePart(line, targetScene.name, virtualScene.name, remoteSceneName);
                        AddGameObjectInScenePart(objectName, position, rotation, true);//, targetScene.name, virtualScene.name, remoteSceneName, position, rotation);

                        SupportGenerator[] generators = GameObject.FindObjectsOfType<SupportGenerator>();
                        for (int i = 0; i < generators.Length; ++i)
                        {
                            generators[i].nbCaseX = dimX;
                            generators[i].nbCaseY = dimY;
                            generators[i].squareSize = squareSize;
                            generators[i].Regenerate();
                        }

                        string[] paramsNames = new string[4] { "nbCaseX", "nbCaseY", "squareSize", "regenerate" };
                        GenericType[] paramsValues = new GenericType[4]
                        {
                            new GenericType(dimX),
                            new GenericType(dimY),
                            new GenericType(squareSize),
                            new GenericType(true)
                        };

                        UpdateGameObjectComponentInScenePart(objectName, "SupportGenerator", paramsNames, paramsValues);
                    }
                }
                reader.Close();
            }
            else
            {
                Debug.LogError("file not found");
            }
        }

    }

    public void ChangeLayerDeeply(Transform t, int layer)
    {
        t.gameObject.layer = layer;
        for (int i = 0; i < t.childCount; ++i)
            ChangeLayerDeeply(t.GetChild(i), layer);
    }

    public void UpdateGameObjectComponentInScenePart(string nameGO, string componentName, string[] paramsNames, GenericType[] paramsValues)
    {
        network.UpdateComponentInScenePart(nameGO, componentName, paramsNames, paramsValues);
    }

    public void RemoveGameObjectInScenePart(string nameGO)
    {
        network.RemoveGameObjectInScenePart(nameGO);
    }

    public void AddGameObjectInScenePart(string nameGO, bool isMaster)
    {
        AddGameObjectInScenePart(nameGO, targetScene.name, /*virtualScene.name,*/ remoteSceneName, isMaster);
    }

    public void AddGameObjectInScenePart(string nameGO, Vector3 p, Quaternion q, bool isMaster)
    {
        AddGameObjectInScenePart(nameGO, targetScene.name, /*virtualScene.name,*/ remoteSceneName, p, q, isMaster);
    }

    public void AddGameObjectInScenePart(string nameGO, Vector3 p, Quaternion q, float s, bool isMaster)
    {
        AddGameObjectInScenePart(nameGO, targetScene.name, /*virtualScene.name,*/ remoteSceneName, p, q, s, isMaster);
    }

    public void AddGameObjectInScenePart(string nameGO, string targetScene, /*string virtualScene,*/ string remoteScene, bool isMaster)
    {
        if (GameObject.Find(nameGO) != null) return;


        GameObject instanceTargetScene = Instantiate(Resources.Load(nameGO, typeof(GameObject))) as GameObject;
        //GameObject instanceVirtualScene = Instantiate(Resources.Load(nameGO, typeof(GameObject))) as GameObject;

        if (instanceTargetScene != null)// && instanceVirtualScene != null)
        {
            string instNameG0 = nameGO;
            GameObject alreadyExistingGO = GameObject.Find(nameGO);
            if (alreadyExistingGO != null)
            {
                //instNameG0 += "_" + Time.time;

                // remove previous instance
                Destroy(alreadyExistingGO);

            }

            instanceTargetScene.name = nameGO;
            //instanceVirtualScene.name = nameGO + "Client";

            GameObject targetSceneGO = GameObject.Find(targetScene);
            //GameObject virtualSceneGO = GameObject.Find(virtualScene);
            if (targetSceneGO != null)// && virtualSceneGO != null)
            {
                instanceTargetScene.transform.parent = targetSceneGO.transform;
                ForcePositivescale(instanceTargetScene.transform);

                //instanceVirtualScene.transform.parent = virtualSceneGO.transform;
                //ForcePositivescale(instanceVirtualScene.transform);

                //ChangeLayerDeeply(instanceVirtualScene.transform, 8);

                network.AddGameObjectInScenePart(
                    nameGO,
                    instNameG0,
                    instanceTargetScene.transform.localPosition,
                    instanceTargetScene.transform.localRotation,
                    new Vector3(1, 1, 1),
                    remoteScene,
                    !isMaster);
            }
        }
    }

    public void AddGameObjectInScenePart(string nameGO, string targetScene, /*string virtualScene,*/ string remoteScene, Vector3 p, Quaternion q, bool isMaster)
    {
        if (GameObject.Find(nameGO) != null) return;



        GameObject instanceTargetScene = Instantiate(Resources.Load(nameGO, typeof(GameObject))) as GameObject;
        //GameObject instanceVirtualScene = Instantiate(Resources.Load(nameGO, typeof(GameObject))) as GameObject;

        if (instanceTargetScene != null)// && instanceVirtualScene != null)
        {
            string instNameG0 = nameGO;
            if (GameObject.Find(nameGO) != null)
                instNameG0 += "-" + Time.time;

            instanceTargetScene.name = instNameG0;
            //instanceVirtualScene.name = instNameG0 + "Client";

            GameObject targetSceneGO = GameObject.Find(targetScene);
            //GameObject virtualSceneGO = GameObject.Find(virtualScene);
            if (targetSceneGO != null)// && virtualSceneGO != null)
            {
                instanceTargetScene.transform.parent = targetSceneGO.transform;
                //instanceTargetScene.transform.localScale = new Vector3(1, 1, 1);
                instanceTargetScene.transform.localPosition = p;
                instanceTargetScene.transform.localRotation = q;
                ForcePositivescale(instanceTargetScene.transform);

                if(isMaster)
                    instanceTargetScene.AddComponent<NetworkMaster>();
                else
                    instanceTargetScene.AddComponent<NetworkSlave>();
                //instanceVirtualScene.transform.parent = virtualSceneGO.transform;
                ////instanceVirtualScene.transform.localScale = new Vector3(1, 1, 1);
                //instanceVirtualScene.transform.localPosition = p;
                //instanceVirtualScene.transform.localRotation = q;
                //ForcePositivescale(instanceVirtualScene.transform);
                //ChangeLayerDeeply(instanceVirtualScene.transform, 8);

                // TODO : add difference between nameGO and instNameGO
                network.AddGameObjectInScenePart(
                    nameGO,
                    instNameG0,
                    p,
                    q,
                    remoteScene,
                    !isMaster);
            }
        }
    }

    public void AddGameObjectInScenePart(string nameGO, string targetScene, /*string virtualScene,*/ string remoteScene, Vector3 p, Quaternion q, float scale, bool isMaster)
    {
        if (GameObject.Find(nameGO) != null) return;


        GameObject instanceTargetScene = Instantiate(Resources.Load(nameGO, typeof(GameObject))) as GameObject;
        //GameObject instanceVirtualScene = Instantiate(Resources.Load(nameGO, typeof(GameObject))) as GameObject;

        if (instanceTargetScene != null)// && instanceVirtualScene != null)
        {
            string instNameG0 = nameGO;
            if (GameObject.Find(nameGO) != null)
                instNameG0 += "_" + Time.time;

            instanceTargetScene.name = instNameG0;
            //instanceVirtualScene.name = instNameG0 + "Client";

            GameObject targetSceneGO = GameObject.Find(targetScene);
            //GameObject virtualSceneGO = GameObject.Find(virtualScene);
            if (targetSceneGO != null)// && virtualSceneGO != null)
            {
                instanceTargetScene.transform.parent = targetSceneGO.transform;
                //instanceTargetScene.transform.localScale = new Vector3(1, 1, 1);

                instanceTargetScene.transform.localPosition = p;
                instanceTargetScene.transform.localRotation = q;
                Vector3 itsScale = instanceTargetScene.transform.localScale;
                itsScale.x *= scale;
                itsScale.y *= scale;
                itsScale.z *= scale;
                instanceTargetScene.transform.localScale = itsScale;
                ForcePositivescale(instanceTargetScene.transform);

                //instanceVirtualScene.transform.parent = virtualSceneGO.transform;
                ////instanceVirtualScene.transform.localScale = new Vector3(1, 1, 1);

                //instanceVirtualScene.transform.localPosition = p;
                //instanceVirtualScene.transform.localRotation = q;
                //Vector3 ivsScale = instanceTargetScene.transform.localScale;
                //ivsScale.x *= scale;
                //ivsScale.y *= scale;
                //ivsScale.z *= scale;
                //instanceTargetScene.transform.localScale = ivsScale;
                //ForcePositivescale(instanceVirtualScene.transform);
                //ChangeLayerDeeply(instanceVirtualScene.transform, 8);

                // TODO : add difference between nameGO and instNameGO
                network.AddGameObjectInScenePart(
                    nameGO,
                    instNameG0,
                    p,
                    q,
                    itsScale,
                    remoteScene,
                    !isMaster);

                Debug.Log("size part " + nameGO + " ok");
            }
        }
    }

    void ForcePositivescale(Transform t)
    {
        Vector3 localScale = t.localScale;
        if (localScale.x < 0)
            localScale.x *= -1.0f;
        if (localScale.y < 0)
            localScale.y *= -1.0f;
        if (localScale.z < 0)
            localScale.z *= -1.0f;
        t.localScale = localScale;
    }
}
