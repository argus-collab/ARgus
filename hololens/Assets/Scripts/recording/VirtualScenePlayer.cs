using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class VirtualScenePlayer : MonoBehaviour
{
    public RecordPlayer player;

    public int uid;
    public int taskid;

    private GameObject head;
    private GameObject rightHand;
    private GameObject leftHand;

    private bool replay = false;
    //private bool loaded = false;

    void Start()
    {
        //player.LoadRecord();
        LoadLogs();
    }

    void OnGUI()
    {
        if (!replay)
        {
            if (GUI.Button(new Rect(250, 60, 150, 25), "Start replay"))
            {
                StartReplay();
            }
        }
        else
        {
            if (GUI.Button(new Rect(250, 60, 150, 25), "Stop replay"))
            {
                StopReplay();
            }
        }
    }

    public void StartReplay()
    {
        //replay = true;
        //player.LoadRecord();
        player.PlayRecord();
    }

    public void StopReplay()
    {
        //replay = false;
        player.StopRecord();
    }

    private void Update()
    {
        replay = player.IsPlaying();
    }
    
    public void CreateHead()
    {
        head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = "Head";
        head.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
    }

    public void CreatePlayer()
    {
        CreateHead();

        rightHand = new GameObject("Right_hand");
        leftHand = new GameObject("Left_hand");

        CreateHand(rightHand);
        CreateHand(leftHand);

        CreateHandConsistency(rightHand);
        CreateHandConsistency(leftHand);
    }

    void CreateHand(GameObject hand)
    {
        CreateGameObject("wrist", hand.transform);
        CreateGameObject("palm", hand.transform);
        CreateGameObject("thumb_metacarpal", hand.transform);
        CreateGameObject("thumb_proximal", hand.transform);
        CreateGameObject("thumb_distal", hand.transform);
        CreateGameObject("thumb_tip", hand.transform);
        CreateGameObject("index_metacarpal", hand.transform);
        CreateGameObject("index_knuckle", hand.transform);
        CreateGameObject("index_middle", hand.transform);
        CreateGameObject("index_distal", hand.transform);
        CreateGameObject("index_tip", hand.transform);
        CreateGameObject("middle_metacarpal", hand.transform);
        CreateGameObject("middle_knuckle", hand.transform);
        CreateGameObject("middle_middle", hand.transform);
        CreateGameObject("middle_distal", hand.transform);
        CreateGameObject("middle_tip", hand.transform);
        CreateGameObject("ring_metacarpal", hand.transform);
        CreateGameObject("ring_knuckle", hand.transform);
        CreateGameObject("ring_middle", hand.transform);
        CreateGameObject("ring_distal", hand.transform);
        CreateGameObject("ring_tip", hand.transform);
        CreateGameObject("pinky_metacarpal", hand.transform);
        CreateGameObject("pinky_knuckle", hand.transform);
        CreateGameObject("pinky_middle", hand.transform);
        CreateGameObject("pinky_distal", hand.transform);
        CreateGameObject("pinky_tip", hand.transform);

    }

    void UpdateTransform(GameObject go)
    {
        go.transform.localPosition = player.GetFirstPosition(go.transform.parent.name + "/" + go.name);
        go.transform.localRotation = player.GetFirstRotation(go.transform.parent.name + "/" + go.name);
    }

    void CreateHandConsistency(GameObject hand)
    {
        UpdateTransform(GameObject.Find(hand.transform.name + "/" + "wrist"));
        UpdateTransform(GameObject.Find(hand.transform.name + "/" + "palm"));
        UpdateTransform(GameObject.Find(hand.transform.name + "/" + "thumb_metacarpal"));
        UpdateTransform(GameObject.Find(hand.transform.name + "/" + "thumb_proximal"));
        UpdateTransform(GameObject.Find(hand.transform.name + "/" + "thumb_distal"));
        UpdateTransform(GameObject.Find(hand.transform.name + "/" + "thumb_tip"));
        UpdateTransform(GameObject.Find(hand.transform.name + "/" + "index_metacarpal"));
        UpdateTransform(GameObject.Find(hand.transform.name + "/" + "index_knuckle"));
        UpdateTransform(GameObject.Find(hand.transform.name + "/" + "index_middle"));
        UpdateTransform(GameObject.Find(hand.transform.name + "/" + "index_distal"));
        UpdateTransform(GameObject.Find(hand.transform.name + "/" + "index_tip"));
        UpdateTransform(GameObject.Find(hand.transform.name + "/" + "middle_metacarpal"));
        UpdateTransform(GameObject.Find(hand.transform.name + "/" + "middle_knuckle"));
        UpdateTransform(GameObject.Find(hand.transform.name + "/" + "middle_middle"));
        UpdateTransform(GameObject.Find(hand.transform.name + "/" + "middle_distal"));
        UpdateTransform(GameObject.Find(hand.transform.name + "/" + "middle_tip"));
        UpdateTransform(GameObject.Find(hand.transform.name + "/" + "ring_metacarpal"));
        UpdateTransform(GameObject.Find(hand.transform.name + "/" + "ring_knuckle"));
        UpdateTransform(GameObject.Find(hand.transform.name + "/" + "ring_middle"));
        UpdateTransform(GameObject.Find(hand.transform.name + "/" + "ring_distal"));
        UpdateTransform(GameObject.Find(hand.transform.name + "/" + "ring_tip"));
        UpdateTransform(GameObject.Find(hand.transform.name + "/" + "pinky_metacarpal"));
        UpdateTransform(GameObject.Find(hand.transform.name + "/" + "pinky_knuckle"));
        UpdateTransform(GameObject.Find(hand.transform.name + "/" + "pinky_middle"));
        UpdateTransform(GameObject.Find(hand.transform.name + "/" + "pinky_distal"));
        UpdateTransform(GameObject.Find(hand.transform.name + "/" + "pinky_tip"));

        CreateHandConsistency(hand.transform);
    }

    public void CreateGameObject(string name, Transform parent)
    {
        GameObject newGO = GameObject.CreatePrimitive(PrimitiveType.Cube); //new GameObject(name); // 
        newGO.name = name;
        newGO.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
        newGO.transform.parent = parent;
    }

    public void CreateHandConsistency(Transform hand)
    {
        CreatePalmConsistency(hand);

        CreateThumbConsistency(hand);
        CreateIndexConsistency(hand);
        CreateMiddleConsistency(hand);
        CreateRingConsistency(hand);
        CreatePinkyConsistency(hand);
    }

    public void CreatePhalange(string name, GameObject top, GameObject bottom)
    {
        float distalPhalangeLength = (top.transform.position - bottom.transform.position).magnitude * 1 / bottom.transform.localScale.magnitude;

        GameObject distal_phalange = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        distal_phalange.name = name;
        distal_phalange.transform.parent = bottom.transform;
        distal_phalange.transform.localPosition = new Vector3(0.0f, 0.0f, distalPhalangeLength);
        distal_phalange.transform.localRotation = Quaternion.Euler(90.0f, 0.0f, 0.0f);
        distal_phalange.transform.parent = bottom.transform;
        distal_phalange.transform.localScale = new Vector3(1.5f, distalPhalangeLength, 1.5f);
    }

    public void CreatePalmPart(GameObject top, GameObject bottom)
    {
        float distalPhalangeLength = (top.transform.position - bottom.transform.position).magnitude * 1 / bottom.transform.localScale.magnitude;

        GameObject distal_phalange = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        distal_phalange.name = "palm_part";
        distal_phalange.transform.parent = bottom.transform;
        distal_phalange.transform.localPosition = new Vector3(0.0f, 0.0f, distalPhalangeLength);
        distal_phalange.transform.localRotation = Quaternion.Euler(90.0f, 0.0f, 0.0f);
        distal_phalange.transform.parent = bottom.transform;
        distal_phalange.transform.localScale = new Vector3(2.5f, distalPhalangeLength, 1f);
    }

    public void CreatePalmConsistency(Transform parent)
    {
        GameObject thumb_metacarpal = parent.Find("thumb_metacarpal").gameObject;
        GameObject thumb_proximal = parent.Find("thumb_proximal").gameObject;
        CreatePalmPart(thumb_proximal, thumb_metacarpal);

        GameObject index_metacarpal = parent.Find("index_metacarpal").gameObject;
        GameObject index_knuckle = parent.Find("index_knuckle").gameObject;
        CreatePalmPart(index_knuckle, index_metacarpal);

        GameObject middle_metacarpal = parent.Find("middle_metacarpal").gameObject;
        GameObject middle_knuckle = parent.Find("middle_knuckle").gameObject;
        CreatePalmPart(middle_knuckle, middle_metacarpal);

        GameObject ring_metacarpal = parent.Find("ring_metacarpal").gameObject;
        GameObject ring_knuckle = parent.Find("ring_knuckle").gameObject;
        CreatePalmPart(ring_knuckle, ring_metacarpal);

        GameObject pinky_metacarpal = parent.Find("pinky_metacarpal").gameObject;
        GameObject pinky_knuckle = parent.Find("pinky_knuckle").gameObject;
        CreatePalmPart(pinky_knuckle, pinky_metacarpal);
    }

    public void CreateThumbConsistency(Transform parent)
    {
        GameObject thumb_proximal = parent.Find("thumb_proximal").gameObject;
        GameObject thumb_distal = parent.Find("thumb_distal").gameObject;
        GameObject thumb_tip = parent.Find("thumb_tip").gameObject;

        CreatePhalange("distal_phalange", thumb_tip, thumb_distal);
        CreatePhalange("proximal_phalange", thumb_distal, thumb_proximal);
    }

    public void CreateIndexConsistency(Transform parent)
    {
        GameObject index_knuckle = parent.Find("index_knuckle").gameObject;
        GameObject index_middle = parent.Find("index_middle").gameObject;
        GameObject index_distal = parent.Find("index_distal").gameObject;
        GameObject index_tip = parent.Find("index_tip").gameObject;

        CreatePhalange("distal_phalange", index_tip, index_distal);
        CreatePhalange("intermediate_phalange", index_distal, index_middle);
        CreatePhalange("proximal_phalange", index_middle, index_knuckle);
    }

    public void CreateMiddleConsistency(Transform parent)
    {
        GameObject middle_knuckle = parent.Find("middle_knuckle").gameObject;
        GameObject middle_middle = parent.Find("middle_middle").gameObject;
        GameObject middle_distal = parent.Find("middle_distal").gameObject;
        GameObject middle_tip = parent.Find("middle_tip").gameObject;

        CreatePhalange("distal_phalange", middle_tip, middle_distal);
        CreatePhalange("intermediate_phalange", middle_distal, middle_middle);
        CreatePhalange("proximal_phalange", middle_middle, middle_knuckle);
    }

    public void CreateRingConsistency(Transform parent)
    {
        GameObject ring_knuckle = parent.Find("ring_knuckle").gameObject;
        GameObject ring_middle = parent.Find("ring_middle").gameObject;
        GameObject ring_distal = parent.Find("ring_distal").gameObject;
        GameObject ring_tip = parent.Find("ring_tip").gameObject;

        CreatePhalange("distal_phalange", ring_tip, ring_distal);
        CreatePhalange("intermediate_phalange", ring_distal, ring_middle);
        CreatePhalange("proximal_phalange", ring_middle, ring_knuckle);
    }

    public void CreatePinkyConsistency(Transform parent)
    {
        GameObject pinky_knuckle = parent.Find("pinky_knuckle").gameObject;
        GameObject pinky_middle = parent.Find("pinky_middle").gameObject;
        GameObject pinky_distal = parent.Find("pinky_distal").gameObject;
        GameObject pinky_tip = parent.Find("pinky_tip").gameObject;

        CreatePhalange("distal_phalange", pinky_tip, pinky_distal);
        CreatePhalange("intermediate_phalange", pinky_distal, pinky_middle);
        CreatePhalange("proximal_phalange", pinky_middle, pinky_knuckle);
    }



    public void LoadLogs()
    {
        // load scene
        ProcessFile("config_" + uid + "_task" + taskid);

        // pop player
        CreatePlayer();

        // load traj logs
        player.SetFileName("traj_" + uid + "_task" + taskid);
        //player.LoadRecord();
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

                    AddGameObject(objectName, position, rotation, 1.0f);
                }
                else if (type == "sized-part")
                {
                    string objectName = param[1];
                    Vector3 position = new Vector3(float.Parse(param[2]), float.Parse(param[3]), float.Parse(param[4]));
                    //Quaternion rotation = Quaternion.identity;
                    Quaternion rotation = new Quaternion(float.Parse(param[5]), float.Parse(param[6]), float.Parse(param[7]), float.Parse(param[8]));

                    float scale = float.Parse(param[9]);

                    AddGameObject(objectName, position, rotation, scale);
                }
                else if (type == "custom-support")
                {
                    string objectName = param[1];
                    Vector3 position = new Vector3(float.Parse(param[2]), float.Parse(param[3]), float.Parse(param[4]));
                    Quaternion rotation = Quaternion.identity;

                    AddGameObject(objectName, position, rotation, 1.0f);

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
                }
            }
            reader.Close();
        }
        catch (Exception e)
        {
            Debug.LogError("file not found");
        }
    }

    public void AddGameObject(string nameGO, Vector3 p, Quaternion q, float scale)
    {
        GameObject instanceTargetScene = Instantiate(Resources.Load(nameGO, typeof(GameObject))) as GameObject;

        if (instanceTargetScene != null)
        {
            string instNameG0 = nameGO;
            if (GameObject.Find(nameGO) != null)
                instNameG0 += "_" + Time.time;

            instanceTargetScene.name = instNameG0;

            instanceTargetScene.transform.localPosition = p;
            instanceTargetScene.transform.localRotation = q;
            Vector3 itsScale = instanceTargetScene.transform.localScale;
            itsScale.x *= scale;
            itsScale.y *= scale;
            itsScale.z *= scale;
            instanceTargetScene.transform.localScale = itsScale;
            ForcePositivescale(instanceTargetScene.transform);
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
