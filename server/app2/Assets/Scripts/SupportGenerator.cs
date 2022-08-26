using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SupportGenerator : MonoBehaviour
{
    public int nbCaseX = 7;
    public int nbCaseY = 13;
    public float squareSize = 0.07f;
    public bool regenerate = false;
    public bool transversalBorders = true;

    public GameObject ground;
    public GameObject emplacements;
    public GameObject consistency;
    public GameObject coordinateSystem;

    void Start()
    {
        if (emplacements == null)
            emplacements = gameObject.transform.Find("emplacements").gameObject;

        if (consistency == null)
            consistency = gameObject.transform.Find("consistency").gameObject;

        Generate();
    }

    void Generate()
    {
        // ground
        ground.transform.localScale = new Vector3(nbCaseX * squareSize, 0.01f, nbCaseY * squareSize);

        // borders
        CreateBorder(new Vector3(nbCaseX * squareSize / 2 + 0.01f, 0.0f, 0.0f), new Vector3(0.02f, 0.02f, nbCaseY * squareSize));
        CreateBorder(new Vector3(-(nbCaseX * squareSize / 2) - 0.01f, 0.0f, 0.0f), new Vector3(0.02f, 0.02f, nbCaseY * squareSize));
        CreateBorder(new Vector3(0.0f, 0.0f, nbCaseY * squareSize / 2 + 0.01f), new Vector3(nbCaseX * squareSize, 0.02f, 0.02f));
        CreateBorder(new Vector3(0.0f, 0.0f, -(nbCaseY * squareSize / 2) - 0.01f), new Vector3(nbCaseX * squareSize, 0.02f, 0.02f));

        // transversal borders
        if(transversalBorders)
        {
            for (int i = 1; i < nbCaseY; ++i)
                CreateTransversalBorder(new Vector3(0.0f, 0.0f, -(nbCaseY * squareSize / 2) + i * squareSize), new Vector3(nbCaseX * squareSize, 0.02f, 0.001f));
            for (int i = 1; i < nbCaseX; ++i)
                CreateTransversalBorder(new Vector3(-(nbCaseX * squareSize / 2) + i * squareSize, 0.0f, 0.0f), new Vector3(0.001f, 0.02f, nbCaseY * squareSize));
        }

        // emplacements
        for (int i = 0; i < nbCaseX; ++i)
            for (int j = 0; j < nbCaseY; ++j)
                CreateEmplacement(new Vector3(-nbCaseX * squareSize / 2 + squareSize /2 + i * squareSize, 0.01f, -nbCaseY * squareSize / 2 + squareSize / 2 + j * squareSize));

        // coordinate system emplacement
        float x = -((nbCaseX + 1) * squareSize / 2) - 0.03f; 
        float y = 0.03f;
        float z = 0; 
        coordinateSystem.transform.localPosition = new Vector3(x,y,z);
    }

    void CreateBorder(Vector3 p, Vector3 s)
    {
        GameObject border = GameObject.CreatePrimitive(PrimitiveType.Cube);
        border.name = "border";
        border.transform.parent = consistency.transform;
        border.transform.localPosition = p;
        border.transform.localRotation = Quaternion.identity;
        border.transform.localScale = s;
    }

    void CreateTransversalBorder(Vector3 p, Vector3 s)
    {
        GameObject border = GameObject.CreatePrimitive(PrimitiveType.Cube);
        border.name = "transversal-border";
        border.transform.parent = consistency.transform;
        border.transform.localPosition = p;
        border.transform.localRotation = Quaternion.identity;
        border.transform.localScale = s;
    }

    void CreateEmplacement(Vector3 p)
    {
        GameObject emplacement = GameObject.CreatePrimitive(PrimitiveType.Cube);
        emplacement.name = "emplacement";
        emplacement.transform.parent = emplacements.transform;
        emplacement.transform.localPosition = p;
        emplacement.transform.localRotation = Quaternion.identity;
        emplacement.transform.localScale = new Vector3(squareSize, 0.01f, squareSize);

        emplacement.AddComponent<IsStickyEmplacement>();
        emplacement.GetComponent<MeshRenderer>().material = Resources.Load("TransparentBlue") as Material;
    }

    public void Regenerate()
    {
        for (int i = 0; i < emplacements.transform.childCount; ++i)
            Destroy(emplacements.transform.GetChild(i).gameObject);

        for (int i = 0; i < consistency.transform.childCount; ++i)
            if(consistency.transform.GetChild(i).gameObject != ground)
                Destroy(consistency.transform.GetChild(i).gameObject);

        Generate();
    }

    private void Update()
    {
        if(regenerate)
        {
            regenerate = false;
            Regenerate();
        }
    }
}
