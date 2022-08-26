using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SymbolsRandomPlacement : MonoBehaviour
{
    public List<Sprite> symbolsFaceA;
    public List<Sprite> symbolsFaceB;

    public List<SpriteRenderer> wallA1;
    public List<SpriteRenderer> wallA2;
    public List<SpriteRenderer> wallA3;

    public List<SpriteRenderer> wallB1;
    public List<SpriteRenderer> wallB2;
    public List<SpriteRenderer> wallB3;

    //public bool d_init = false;
    //public SpriteRenderer d_spriteX;
    //public SpriteRenderer d_spriteY;

    public Sprite spriteCoordX;
    public Sprite spriteCoordY;

    private static System.Random rng = new System.Random();

    public bool displayForbiddenArea;
    public bool displayAvailabeArea;
    public bool reInit;

    [Header("debug")]
    public List<Sprite> randomizedSymbolsFaceA;
    public List<Sprite> randomizedSymbolsFaceB;
    public List<SpriteRenderer> randomizedWallA1;
    public List<SpriteRenderer> randomizedWallA2;
    public List<SpriteRenderer> randomizedWallA3;
    public List<SpriteRenderer> randomizedWallB1;
    public List<SpriteRenderer> randomizedWallB2;
    public List<SpriteRenderer> randomizedWallB3;
    public int symbolIndexA;
    public int symbolIndexB;

    public Color available;
    public Color forbidden;

    public bool debug;

    void Start()
    {
        randomizedSymbolsFaceA = new List<Sprite>();
        randomizedSymbolsFaceB = new List<Sprite>();

        randomizedWallA1 = new List<SpriteRenderer>();
        randomizedWallA2 = new List<SpriteRenderer>();
        randomizedWallA3 = new List<SpriteRenderer>();

        randomizedWallB1 = new List<SpriteRenderer>();
        randomizedWallB2 = new List<SpriteRenderer>();
        randomizedWallB3 = new List<SpriteRenderer>();

        //Initialize();
    }

    private void Update()
    {
        if (displayForbiddenArea)
            ShowForbiddenArea();
        else
            HideForbiddenArea();

        if (displayAvailabeArea)
            ShowAvailableArea();
        else
            HideAvailableArea();

        if (reInit)
        {
            reInit = false;
            Initialize();
        }
    }

    public void ShowForbiddenArea()
    {
        wallA1[symbolIndexA].GetComponentInChildren<MeshRenderer>().enabled = true;
        wallA2[symbolIndexA].GetComponentInChildren<MeshRenderer>().enabled = true;
        wallA3[symbolIndexA].GetComponentInChildren<MeshRenderer>().enabled = true;

        wallB1[symbolIndexB].GetComponentInChildren<MeshRenderer>().enabled = true;
        wallB2[symbolIndexB].GetComponentInChildren<MeshRenderer>().enabled = true;
        wallB3[symbolIndexB].GetComponentInChildren<MeshRenderer>().enabled = true;

        wallA1[symbolIndexA].GetComponentInChildren<MeshRenderer>().material.color = forbidden;
        wallA2[symbolIndexA].GetComponentInChildren<MeshRenderer>().material.color = forbidden;
        wallA3[symbolIndexA].GetComponentInChildren<MeshRenderer>().material.color = forbidden;

        wallB1[symbolIndexB].GetComponentInChildren<MeshRenderer>().material.color = forbidden;
        wallB2[symbolIndexB].GetComponentInChildren<MeshRenderer>().material.color = forbidden;
        wallB3[symbolIndexB].GetComponentInChildren<MeshRenderer>().material.color = forbidden;
    }

    public void HideForbiddenArea()
    {
        wallA1[symbolIndexA].GetComponentInChildren<MeshRenderer>().enabled = false;
        wallA2[symbolIndexA].GetComponentInChildren<MeshRenderer>().enabled = false;
        wallA3[symbolIndexA].GetComponentInChildren<MeshRenderer>().enabled = false;

        wallB1[symbolIndexB].GetComponentInChildren<MeshRenderer>().enabled = false;
        wallB2[symbolIndexB].GetComponentInChildren<MeshRenderer>().enabled = false;
        wallB3[symbolIndexB].GetComponentInChildren<MeshRenderer>().enabled = false;
    }

    public void ShowAvailableArea()
    {
        for (int i = 0; i < wallA1.Count; ++i)
            if (i != symbolIndexA)
            {
                wallA1[i].GetComponentInChildren<MeshRenderer>().enabled = true;
                wallA1[i].GetComponentInChildren<MeshRenderer>().material.color = available;
            }

        for (int i = 0; i < wallA2.Count; ++i)
            if (i != symbolIndexA)
            {
                wallA2[i].GetComponentInChildren<MeshRenderer>().enabled = true;
                wallA2[i].GetComponentInChildren<MeshRenderer>().material.color = available;
            }

        for (int i = 0; i < wallA3.Count; ++i)
            if (i != symbolIndexA)
            {
                wallA3[i].GetComponentInChildren<MeshRenderer>().enabled = true;
                wallA3[i].GetComponentInChildren<MeshRenderer>().material.color = available;
            }

        for (int i = 0; i < wallB1.Count; ++i)
            if (i != symbolIndexB)
            {
                wallA1[i].GetComponentInChildren<MeshRenderer>().enabled = true;
                wallA1[i].GetComponentInChildren<MeshRenderer>().material.color = available;
            }

        for (int i = 0; i < wallB2.Count; ++i)
            if (i != symbolIndexB)
            {
                wallB2[i].GetComponentInChildren<MeshRenderer>().enabled = true;
                wallB2[i].GetComponentInChildren<MeshRenderer>().material.color = available;
            }

        for (int i = 0; i < wallB3.Count; ++i)
            if (i != symbolIndexB)
            {
                wallB3[i].GetComponentInChildren<MeshRenderer>().enabled = true;
                wallB3[i].GetComponentInChildren<MeshRenderer>().material.color = available;
            }
    }

    public void HideAvailableArea()
    {
        for (int i = 0; i < wallA1.Count; ++i)
            if (i != symbolIndexA)
                wallA1[i].GetComponentInChildren<MeshRenderer>().enabled = false;

        for (int i = 0; i < wallA2.Count; ++i)
            if (i != symbolIndexA)
                wallA2[i].GetComponentInChildren<MeshRenderer>().enabled = false;

        for (int i = 0; i < wallA3.Count; ++i)
            if (i != symbolIndexA)
                wallA3[i].GetComponentInChildren<MeshRenderer>().enabled = false;

        for (int i = 0; i < wallB1.Count; ++i)
            if (i != symbolIndexB)
                wallB1[i].GetComponentInChildren<MeshRenderer>().enabled = false;

        for (int i = 0; i < wallB2.Count; ++i)
            if (i != symbolIndexB)
                wallB2[i].GetComponentInChildren<MeshRenderer>().enabled = false;

        for (int i = 0; i < wallB3.Count; ++i)
            if (i != symbolIndexB)
                wallB3[i].GetComponentInChildren<MeshRenderer>().enabled = false;
    }

    public Sprite GetSpriteCoordX()
    {
        return spriteCoordX;
    }

    public Sprite GetSpriteCoordY()
    {
        return spriteCoordY;
    }

    //private void Update()
    //{
    //    if(d_init)
    //    {
    //        d_init = false;
    //        Initialize();
    //    }

    //    if (d_spriteX != null)
    //        d_spriteX.sprite = spriteCoordX;
    //    if (d_spriteY != null)
    //        d_spriteY.sprite = spriteCoordY;
    //}

    public List<T> Randomize<T>(List<T> originalList)
    {
        List<T> res = new List<T>();

        foreach (T s in originalList)
            res.Add(s);
        Shuffle<T>(res);

        return res;
    }

    public void Initialize()
    {
        randomizedSymbolsFaceA = Randomize<Sprite>(symbolsFaceA);
        randomizedSymbolsFaceB = Randomize<Sprite>(symbolsFaceB);

        randomizedWallA1 = Randomize<SpriteRenderer>(wallA1);
        randomizedWallA2 = Randomize<SpriteRenderer>(wallA2);
        randomizedWallA3 = Randomize<SpriteRenderer>(wallA3);

        randomizedWallB1 = Randomize<SpriteRenderer>(wallB1);
        randomizedWallB2 = Randomize<SpriteRenderer>(wallB2);
        randomizedWallB3 = Randomize<SpriteRenderer>(wallB3);


        int minIndexA = Mathf.Min(Mathf.Min(wallA1.Count, wallA2.Count), wallA3.Count);
        int minIndexB = Mathf.Min(Mathf.Min(wallB1.Count, wallB2.Count), wallB3.Count);

        symbolIndexA = Random.Range(0, minIndexA);
        symbolIndexB = Random.Range(0, minIndexB);

        ApplyState();
    }

    void ApplyState()
    {
        spriteCoordX = randomizedSymbolsFaceA[symbolIndexA];
        spriteCoordY = randomizedSymbolsFaceB[symbolIndexB];

        ApplySymbols(randomizedSymbolsFaceA, randomizedWallA1, symbolIndexA);
        ApplySymbols(randomizedSymbolsFaceA, randomizedWallA2, symbolIndexA);
        ApplySymbols(randomizedSymbolsFaceA, randomizedWallA3, symbolIndexA);
        
        ApplySymbols(randomizedSymbolsFaceB, randomizedWallB1, symbolIndexB);
        ApplySymbols(randomizedSymbolsFaceB, randomizedWallB2, symbolIndexB);
        ApplySymbols(randomizedSymbolsFaceB, randomizedWallB3, symbolIndexB);
    }



    public string GetStateAsString()
    {
        string sep = "_";
        string res = "";

        res += ListIntToString(GetIndexes<Sprite>(randomizedSymbolsFaceA, symbolsFaceA)) + sep;
        res += ListIntToString(GetIndexes<Sprite>(randomizedSymbolsFaceB, symbolsFaceB)) + sep;

        res += ListIntToString(GetIndexes<SpriteRenderer>(randomizedWallA1, wallA1)) + sep;
        res += ListIntToString(GetIndexes<SpriteRenderer>(randomizedWallA2, wallA2)) + sep;
        res += ListIntToString(GetIndexes<SpriteRenderer>(randomizedWallA3, wallA3)) + sep;

        res += ListIntToString(GetIndexes<SpriteRenderer>(randomizedWallB1, wallB1)) + sep;
        res += ListIntToString(GetIndexes<SpriteRenderer>(randomizedWallB2, wallB2)) + sep;
        res += ListIntToString(GetIndexes<SpriteRenderer>(randomizedWallB3, wallB3)) + sep;

        res += symbolIndexA + sep;
        res += symbolIndexB;

        return res;
    }

    public void SetStateFromString(string state)
    {
        char sep = '_';

        string[] data = state.Split(sep);

        randomizedSymbolsFaceA = ListFromIndexList<Sprite>(StringToIntList(data[0]), symbolsFaceA);
        randomizedSymbolsFaceB = ListFromIndexList<Sprite>(StringToIntList(data[1]), symbolsFaceB);

        randomizedWallA1 = ListFromIndexList<SpriteRenderer>(StringToIntList(data[2]), wallA1);
        randomizedWallA2 = ListFromIndexList<SpriteRenderer>(StringToIntList(data[3]), wallA2);
        randomizedWallA3 = ListFromIndexList<SpriteRenderer>(StringToIntList(data[4]), wallA3);

        randomizedWallB1 = ListFromIndexList<SpriteRenderer>(StringToIntList(data[5]), wallB1);
        randomizedWallB2 = ListFromIndexList<SpriteRenderer>(StringToIntList(data[6]), wallB2);
        randomizedWallB3 = ListFromIndexList<SpriteRenderer>(StringToIntList(data[7]), wallB3);

        symbolIndexA = int.Parse(data[8]);
        symbolIndexB = int.Parse(data[9]);

        ApplyState();
    }

    List<T> ListFromIndexList<T>(List<int> indexList, List<T> buffer)
    {
        List<T> result = new List<T>();

        for (int i = 0; i < indexList.Count; ++i)
            result.Add(buffer[indexList[i]]);

        return result;
    }

    List<int> GetIndexes<T>(List<T> randomizedArray, List<T> originalArray)
    {
        List<int> res = new List<int>();

        for (int i = 0; i < randomizedArray.Count; ++i)
            res.Add(originalArray.IndexOf(randomizedArray[i]));

        return res;
    }

    string ListIntToString(List<int> list)
    {
        string res = "";
        string sep = "/";

        for (int i = 0; i < list.Count; ++i)
        {
            res += list[i];
            if (i < list.Count - 1)
                res += sep;
        }

        return res;
    }

    List<int> StringToIntList(string strList)
    {
        char sep = '/';
        List<int> res = new List<int>();

        string[] strArray = strList.Split(sep);
        for (int i = 0; i < strArray.Length; ++i)
            res.Add(int.Parse(strArray[i]));
        
        return res;
    }

    void ApplySymbols(List<Sprite> sprites, List<SpriteRenderer> renderers, int d_i = -1)
    {
        for (int i = 0; i < renderers.Count && i < sprites.Count; ++i)
        {
            renderers[i].sprite = sprites[i];
            if (debug)
            {
                if (i == d_i)
                    renderers[i].color = Color.yellow;
                else
                    renderers[i].color = Color.red;
            }
        }
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
}
