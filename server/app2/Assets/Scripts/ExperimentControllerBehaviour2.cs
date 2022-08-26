#if !UNITY_WSA
using System.Collections;
using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExperimentControllerBehaviour2 : MonoBehaviour
{
    public CustomClientNetworkManager network;
    private bool popedScene = false;

    public Button loadSceneButton;
    public GameObject[] virtualObjects;

    public UserDataInput userData;
    public Text uid;
    private bool userInit;

    //public Image spot;
    public Color isMark;
    public Color isNotMark;
    public Image furniture;

    public List<Image> room1marks;
    public List<Sprite> room1furnitures;
    public List<Image> room2marks;
    public List<Sprite> room2furnitures;
    public List<Image> room3marks;
    public List<Sprite> room3furnitures;

    private List<Image> marks;
    private List<Sprite> furnitures;
    public Text indexText;
    private int index;

    public Image symbolImage;
    private List<Sprite> symbols;
    private List<Color> colors;

    public Color room1Color;
    public Color room2Color;
    public Color room3Color;

    public List<Sprite> room1Symbols;
    public List<Sprite> room2Symbols;
    public List<Sprite> room3Symbols;
    // TODO deal with random order
    // using userData.GetUserId() at Init();

    //private Room room1;
    //private Room room2;
    //private Room room3;

    [Header("Last version")]
    public List<Image> unitsEmplacement;
    public List<Sprite> furnituresGeneral;
    public List<Sprite> furnitureFace;
    public List<Sprite> furnituresLateral;

    private List<Furniture> furnituresSided;
    private List<int> randomIndexes;

    private List<Furniture> furnituresSidedTuto;
    private List<int> randomIndexesTuto;



    public Image mainImage;
    public Image positionImage;

    public UIManager ui;

    public int nbStepTuto = 5;

    //private RandomHousePlacementGenerator house;
    private bool houseInit = false;

    private static System.Random rng = new System.Random();

    private struct Room
    {
        public List<Image> roomMarks;
        public List<Sprite> roomFurnitures;
        public List<int> indexesMarks;
        public List<int> indexesFurnitures;
    }
    private List<Room> house;// = new List<Room>();

    public struct Emplacement
    {
        public Color c;
        public Sprite s;
        public Emplacement(Color c, Sprite s)
        {
            this.c = c;
            this.s = s;
        }
    };

    public struct Furniture
    {
        public Sprite g;
        public Sprite l;
        public Sprite f;

        public Furniture(Sprite gen, Sprite lat, Sprite fac)
        {
            g = gen;
            l = lat;
            f = fac;
        }
    };

    private void Start()
    {
        symbols = new List<Sprite>();
        colors = new List<Color>();

        house = new List<Room>();
        marks = new List<Image>();
        furnitures = new List<Sprite>();

        List<Room> houseFull = new List<Room>();
        List<Image> marksFull = new List<Image>();
        List<Sprite> furnituresFull = new List<Sprite>();

        // debug, TODO : get random placement from file
        //GenerateRandomPlacement();
        GenerateRandomPlacementShort();
        //GenerateConfigFiles(20);

        LoadHouseInBuffers();

        //for (int i = 0; i < house.Count; ++i)
        //{
        //    int maxIndex = house[i].indexesFurnitures.Count > house[i].indexesMarks.Count ? house[i].indexesMarks.Count : house[i].indexesFurnitures.Count;

        //    for (int j = 0; j < maxIndex; ++j)
        //    {
        //        marks.Add(house[i].roomMarks[house[i].indexesMarks[j]]);
        //        furnitures.Add(house[i].roomFurnitures[house[i].indexesFurnitures[j]]);
        //    }
        //}

        //// only takes 3 for tuto at start
        //List<int> rand = new List<int>();
        //for (int i = 0; i < 3; ++i)
        //{
        //    int index = 0;

        //    int rand_i = UnityEngine.Random.Range(0, marks.Count - 1);
        //    while (index < 1000 && rand.Contains(rand_i))
        //    {
        //        rand_i = UnityEngine.Random.Range(0, marks.Count - 1);
        //        Debug.Log("rand_i = " + rand_i + " = " + rand);
        //        index++;
        //    }
        //    rand.Add(rand_i);
        //    if (index == 1000)
        //        Debug.LogError("error during the initial room set-up");
        //}
        //marks.Clear();
        //furnitures.Clear();

        //for(int i=0;i< houseFull.Count;++i)
        //{
        //    house.Add(houseFull[i]);
        //    marks.Add(marksFull[i]);
        //    furnitures.Add(furnituresFull[i]);
        //} 

        Shuffle(room1Symbols);
        Shuffle(room2Symbols);
        Shuffle(room3Symbols);

        List<Emplacement> buffer = new List<Emplacement>();


        for (int i = 0; i < room1furnitures.Count; ++i)
            buffer.Add(new Emplacement(room1Color, room1Symbols[i]));

        for (int i = 0; i < room2furnitures.Count; ++i)
            buffer.Add(new Emplacement(room2Color, room2Symbols[i]));

        for (int i = 0; i < room3furnitures.Count; ++i)
            buffer.Add(new Emplacement(room3Color, room3Symbols[i]));

        Shuffle(buffer);

        for(int i=0;i<buffer.Count;++i)
        {
            colors.Add(buffer[i].c);
            symbols.Add(buffer[i].s);
        }

        ResetTask();
    }

    public void ResetTask()
    {
        List<Furniture> furnituresSidedTmp = new List<Furniture>();
        for (int i = 0; i < furnituresGeneral.Count; ++i)
            furnituresSidedTmp.Add(new Furniture(furnituresGeneral[i], furnituresLateral[i], furnitureFace[i]));
        Shuffle(furnituresSidedTmp);

        furnituresSided = new List<Furniture>();
        randomIndexes = new List<int>();
        //for (int i = 0; i < buffer.Count; ++i)
        for (int i = 0; i < furnituresGeneral.Count; ++i)
        {
            randomIndexes.Add(UnityEngine.Random.Range(0, unitsEmplacement.Count - 1));
            furnituresSided.Add(furnituresSidedTmp[i]);
        }

        Shuffle(furnituresSidedTmp);
        furnituresSidedTuto = new List<Furniture>();
        randomIndexesTuto = new List<int>();
        for (int i = 0; i < nbStepTuto; ++i)
        {
            randomIndexesTuto.Add(UnityEngine.Random.Range(0, unitsEmplacement.Count - 1));
            furnituresSidedTuto.Add(furnituresSidedTmp[i]);
        }

        UpdateState();
    }

    void ExportConfiguration(string fileName)
    {
        string path = "Assets/Files/study2/" + fileName + ".txt";

        StreamWriter writer = new StreamWriter(path, true);

        string room1Marks = "";
        string room1Furnitures = "";
        for (int i = 0; i < house[0].indexesMarks.Count; ++i)
            room1Marks += house[0].indexesMarks[i] + " ";
        for (int i = 0; i < house[0].indexesFurnitures.Count; ++i)
            room1Furnitures += house[0].indexesFurnitures[i] + " ";

        string room2Marks = "";
        string room2Furnitures = "";
        for (int i = 0; i < house[1].indexesMarks.Count; ++i)
            room2Marks += house[1].indexesMarks[i] + " ";
        for (int i = 0; i < house[1].indexesFurnitures.Count; ++i)
            room2Furnitures += house[1].indexesFurnitures[i] + " ";

        string room3Marks = "";
        string room3Furnitures = "";
        for (int i = 0; i < house[2].indexesMarks.Count; ++i)
            room3Marks += house[2].indexesMarks[i] + " ";
        for (int i = 0; i < house[2].indexesFurnitures.Count; ++i)
            room3Furnitures += house[2].indexesFurnitures[i] + " ";

        writer.WriteLine(room1Marks);
        writer.WriteLine(room1Furnitures);
        writer.WriteLine(room2Marks);
        writer.WriteLine(room2Furnitures);
        writer.WriteLine(room3Marks);
        writer.WriteLine(room3Furnitures);

        writer.Close();
    }

    void LoadRoom1(List<int> indexesMarks, List<int> indexesFurnitures)
    {
        Room room1;
        room1.roomMarks = room1marks;
        room1.roomFurnitures = room1furnitures;
        room1.indexesMarks = indexesMarks;
        room1.indexesFurnitures = indexesFurnitures;
        house.Add(room1);
    }

    void LoadRoom2(List<int> indexesMarks, List<int> indexesFurnitures)
    {
        Room room2;
        room2.roomMarks = room2marks;
        room2.roomFurnitures = room2furnitures;
        room2.indexesMarks = indexesMarks;
        room2.indexesFurnitures = indexesFurnitures;
        house.Add(room2);
    }

    void LoadRoom3(List<int> indexesMarks, List<int> indexesFurnitures)
    {
        Room room3;
        room3.roomMarks = room3marks;
        room3.roomFurnitures = room3furnitures;
        room3.indexesMarks = indexesMarks;
        room3.indexesFurnitures = indexesFurnitures;
        house.Add(room3);
    }

    void LoadHouseInBuffers()
    {
        //Debug.Log("load house in buffers");
        for (int i = 0; i < house.Count; ++i)
        {
            int maxIndex = house[i].indexesFurnitures.Count > house[i].indexesMarks.Count ? house[i].indexesMarks.Count : house[i].indexesFurnitures.Count;


            //Debug.Log("house["+i+"].indexesFurnitures.Count = " + house[i].indexesFurnitures.Count);
            //Debug.Log("house[" + i + "].indexesMarks.Count  = " + house[i].indexesMarks.Count);
            //Debug.Log("maxIndex = " + maxIndex);


            for (int j = 0; j < maxIndex; ++j)
            {
                //Debug.Log("["+j+"] " + house[i].indexesMarks[j] + " / " + house[i].roomMarks.Count);
                //Debug.Log("["+j+"] " + house[i].indexesFurnitures[j] + " / " + house[i].roomFurnitures.Count);
                marks.Add(house[i].roomMarks[house[i].indexesMarks[j]]);
                furnitures.Add(house[i].roomFurnitures[house[i].indexesFurnitures[j]]);
            }
        }
    }

    List<int> ExtractIndexes(string line)
    {
        List<int> indexes = new List<int>();
        string[] subs = line.Split(' ');

        foreach (string sub in subs)
            try { indexes.Add(Int32.Parse(sub)); } catch(Exception e) { Debug.Log("error loading : " + sub); }

        return indexes;
    }

    void ImportConfiguration(string fileName)
    {
        //string path = Application.dataPath + "/Resources/" + fileName + ".txt";
        //StreamReader reader = new StreamReader(path);

        //Load a text file (Assets/Resources/Text/textFile01.txt)
        var textFile = Resources.Load<TextAsset>(fileName);
        string[] data = textFile.text.Split('\n');

        List<int> marksIndexes;
        List<int> furnituresIndexes;

        house.Clear();

        //marksIndexes = ExtractIndexes(reader.ReadLine());
        marksIndexes = ExtractIndexes(data[0]);
        //furnituresIndexes = ExtractIndexes(reader.ReadLine());
        furnituresIndexes = ExtractIndexes(data[1]);
        LoadRoom1(marksIndexes, furnituresIndexes);

        //marksIndexes = ExtractIndexes(reader.ReadLine());
        marksIndexes = ExtractIndexes(data[2]);
        //furnituresIndexes = ExtractIndexes(reader.ReadLine());
        furnituresIndexes = ExtractIndexes(data[3]);
        LoadRoom2(marksIndexes, furnituresIndexes);

        //marksIndexes = ExtractIndexes(reader.ReadLine());
        marksIndexes = ExtractIndexes(data[4]);
        //furnituresIndexes = ExtractIndexes(reader.ReadLine());
        furnituresIndexes = ExtractIndexes(data[5]);
        LoadRoom3(marksIndexes, furnituresIndexes);

        //reader.Close();

        marks.Clear();
        furnitures.Clear();
        LoadHouseInBuffers();
        UpdateState();
    }

    private void GenerateConfigFiles(int nbConfig)
    {
        for(int i = 0; i < nbConfig; ++i)
        {
            house.Clear();
            GenerateRandomPlacement();
            ExportConfiguration("config_" + i);
        }
    }

    private void GenerateRandomPlacement()
    {
        Room room1;
        room1.roomMarks = room1marks;
        room1.roomFurnitures = room1furnitures;
        room1.indexesMarks = GenerateIndexArray(room1marks.Count);
        room1.indexesFurnitures = GenerateIndexArray(room1furnitures.Count);
        house.Add(room1);

        Room room2;
        room2.roomMarks = room2marks;
        room2.roomFurnitures = room2furnitures;
        room2.indexesMarks = GenerateIndexArray(room2marks.Count);
        room2.indexesFurnitures = GenerateIndexArray(room2furnitures.Count);
        house.Add(room2);

        Room room3;
        room3.roomMarks = room3marks;
        room3.roomFurnitures = room3furnitures;
        room3.indexesMarks = GenerateIndexArray(room3marks.Count);
        room3.indexesFurnitures = GenerateIndexArray(room3furnitures.Count);
        house.Add(room3);

        //Shuffle<Image>(room1.roomMarks);
        //Shuffle<Image>(room2.roomMarks);
        //Shuffle<Image>(room3.roomMarks);

        Shuffle<int>(room1.indexesMarks);
        Shuffle<int>(room2.indexesMarks);
        Shuffle<int>(room3.indexesMarks);

        Shuffle<int>(room1.indexesFurnitures);
        Shuffle<int>(room2.indexesFurnitures);
        Shuffle<int>(room3.indexesFurnitures);

        Shuffle<Room>(house);
    }

    private void GenerateRandomPlacementShort()
    {
        List<Image> room1marksAltered = new List<Image>();
        List<Sprite> room1furnituresAltered = new List<Sprite>();
        room1marksAltered.Add(room1marks[UnityEngine.Random.Range(0, room1marks.Count)]);
        room1furnituresAltered.Add(room1furnitures[UnityEngine.Random.Range(0, room1furnitures.Count)]);
        
        List<Image> room2marksAltered = new List<Image>();
        List<Sprite> room2furnituresAltered = new List<Sprite>();
        room2marksAltered.Add(room2marks[UnityEngine.Random.Range(0, room2marks.Count)]);
        room2furnituresAltered.Add(room2furnitures[UnityEngine.Random.Range(0, room2furnitures.Count)]);

        List<Image> room3marksAltered = new List<Image>();
        List<Sprite> room3furnituresAltered = new List<Sprite>();
        room3marksAltered.Add(room3marks[UnityEngine.Random.Range(0, room3marks.Count)]);
        room3furnituresAltered.Add(room3furnitures[UnityEngine.Random.Range(0, room3furnitures.Count)]);

        Room room1;
        room1.roomMarks = room1marksAltered;
        room1.roomFurnitures = room1furnituresAltered;
        room1.indexesMarks = GenerateIndexArray(room1marksAltered.Count);
        room1.indexesFurnitures = GenerateIndexArray(room1furnituresAltered.Count);
        house.Add(room1);

        Room room2;
        room2.roomMarks = room2marksAltered;
        room2.roomFurnitures = room2furnituresAltered;
        room2.indexesMarks = GenerateIndexArray(room2marksAltered.Count);
        room2.indexesFurnitures = GenerateIndexArray(room2furnituresAltered.Count);
        house.Add(room2);

        Room room3;      
        room3.roomMarks = room3marksAltered;
        room3.roomFurnitures = room3furnituresAltered;
        room3.indexesMarks = GenerateIndexArray(room3marksAltered.Count);
        room3.indexesFurnitures = GenerateIndexArray(room3furnituresAltered.Count);
        house.Add(room3);

        Shuffle<Room>(house);
    }

    private List<int> GenerateIndexArray(int size)
    {
        List<int> result = new List<int>();

        for (int i = 0; i < size; ++i)
            result.Add(i);

        return result;
    }

    public void Shuffle<T>(IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    private void Init()
    {
        index = 0;
        UpdateState();

        LoadConfig();



        //if(!popedScene)
        //{
        //    LoadScene();
        //}
    }

    private void OnGUI()
    {
        //if (!popedScene && GUI.Button(new Rect(10, 100, 100, 30), "start experiment"))
        //{
        //    LoadScene();
        //}
    }

    public void OnClickLoadScene()
    {
        LoadConfig();
        //LoadScene();
        loadSceneButton.enabled = false;
        Debug.Log("on click load scene");
    }

    private void LoadScene()
    {
        for (int i = 0; i < virtualObjects.Length; ++i)
            network.SendCommandMessage("scene-manager-command-remote", "pop " + virtualObjects[i].name + " ok");

        popedScene = true;
    }

    private void LoadConfig()
    {
        Debug.Log("about to load : " + "config_" + userData.GetUserId());
        ImportConfiguration("config_" + userData.GetUserId());
        Debug.Log("loaded : " + "config_" + userData.GetUserId());

    }

    private void ResetColors()
    {
        for(int i = 0; i < marks.Count; i++)
        {
            marks[i].color = isNotMark;
        }
    }

    public void GoToNextFurniture()
    {
        ////if (index < furnitures.Count-1)
        //if (ui.IsTutoDone())
        //{
        //    if (index < furnituresSided.Count - 1)
        //        index++;
        //}
        //else
        //{
        //    if (index < furnituresSidedTuto.Count - 1)
        //        index++;
        //}

        UpdateState();
    }

    public void GoToPreviousFurniture()
    {
        if (index > 0)
            index--;

        UpdateState();
    }

    private void UpdateStateTuto()
    {
        ResetColors();
        //marks[index].color = isMark;

        //furniture.sprite = furnitures[index];
        symbolImage.sprite = symbols[index];
        symbolImage.color = colors[index];

        for (int i = 0; i < unitsEmplacement.Count; ++i)
            unitsEmplacement[i].gameObject.SetActive(false);//.color = new Color(0, 0, 0, 0);

        //int id = UnityEngine.Random.Range(0, unitsEmplacement.Count - 1);
        unitsEmplacement[randomIndexesTuto[index]].gameObject.SetActive(true);
        unitsEmplacement[randomIndexesTuto[index]].transform.GetChild(0).GetComponent<Image>().sprite = symbolImage.sprite;
        unitsEmplacement[randomIndexesTuto[index]].transform.GetChild(0).GetComponent<Image>().color = colors[index];

        //indexText.text = (index + 1) + " / " + furnitures.Count;
        indexText.text = (index + 1) + " / " + furnituresSidedTuto.Count;

        mainImage.sprite = furnituresSidedTuto[index].g;

        if (randomIndexesTuto[index] >= (unitsEmplacement.Count - 1) / 2)
            positionImage.sprite = furnituresSidedTuto[index].l;
        else
            positionImage.sprite = furnituresSidedTuto[index].f;

        positionImage.preserveAspect = true;
    }

    private void UpdateStateTask()
    {
        ResetColors();
        //marks[index].color = isMark;

        //furniture.sprite = furnitures[index];
        symbolImage.sprite = symbols[index];
        symbolImage.color = colors[index];

        for (int i = 0; i < unitsEmplacement.Count; ++i)
            unitsEmplacement[i].gameObject.SetActive(false);// color = new Color(0, 0, 0, 0);

        //int id = UnityEngine.Random.Range(0, unitsEmplacement.Count - 1);
        unitsEmplacement[randomIndexes[index]].gameObject.SetActive(true);
        unitsEmplacement[randomIndexes[index]].transform.GetChild(0).GetComponent<Image>().sprite = symbolImage.sprite;
        unitsEmplacement[randomIndexes[index]].transform.GetChild(0).GetComponent<Image>().color = colors[index];

        //indexText.text = (index + 1) + " / " + furnitures.Count;
        indexText.text = (index + 1) + " / " + furnituresSided.Count;

        mainImage.sprite = furnituresSided[index].g;

        if (randomIndexes[index] >= (unitsEmplacement.Count - 1) / 2)
            positionImage.sprite = furnituresSided[index].l;
        else
            positionImage.sprite = furnituresSided[index].f;

        positionImage.preserveAspect = true;
    }

    private void UpdateState()
    {
        //if (ui.IsTutoDone())
        //    UpdateStateTask();
        //else
        //    UpdateStateTuto();
    }

    private void Update()
    {
        if(!userInit && userData.GetUserId() != -1)// && userData.HasDoneTuto())
        {
            userInit = true;
            SetUID();
            Init();
        }
    }

    private void SetUID()
    {
        uid.text = userData.GetUserId() + " - " + userData.GetUserName();
    }
}
#endif