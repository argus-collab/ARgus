using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientScaleManager : MonoBehaviour
{
    public int initialSizeX;
    public int initialSizeY;
    private float initialCubeSize = 0.1f;

    private int sizeX;
    private int sizeY;
    private float cubeSize;

    public int delta;
    private int previousDelta;
    
    public GameObject support;
    public List<GameObject> parts;

    private void Start()
    {
        sizeX = initialSizeX;
        sizeY = initialSizeY;


    }

    void Update()
    {
        if (delta > previousDelta)
            IncrementSupportSize();

        if (delta < previousDelta)
            DecrementSupportSize();
    }

    void AddEmplacement(Transform parent, Vector3 localPosition)
    {
        GameObject emp = GameObject.CreatePrimitive(PrimitiveType.Cube);
        emp.transform.parent = parent;
        emp.transform.localScale = new Vector3(0.1f, 0.01f, 0.1f);
        emp.transform.localPosition = localPosition;
    }

    void IncrementSupportSize()
    {
        previousDelta = delta;

        sizeX++;
        sizeY++;

        //for (int i=0;i<support.transform.childCount;++i)
        //{
        //    if (support.transform.GetChild(i).name == "emplacements")
        //    {
        //        Transform emplacementsRoot = support.transform.GetChild(i);

        //        // new emplacement
        //        for (int j = 0; j < sizeX; ++j)
        //            AddEmplacement(emplacementsRoot, new Vector3(, 0.01f,));


        //        // scale
        //        for (int j = 0; j < emplacementsRoot.childCount; ++j)
        //        {
        //            emplacementsRoot.GetChild
        //        }
        //    }
        //}


    }

    void DecrementSupportSize()
    {
        previousDelta = delta;

        sizeX--;
        sizeY--;
    }
}
