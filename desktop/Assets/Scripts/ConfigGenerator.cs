using Microsoft.MixedReality.WebRTC.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Part
{
    public int id;
    public bool[,] shape;
    public Color color;

    public Part(int id, bool[,] shape, Color color)
    {
        this.id = id;
        this.shape = shape;
        this.color = color;
    }
}

public class ConfigGenerator : MonoBehaviour
{
    public Vector2Int supportDim;

    private List<GameObject> graphicSupport;
    private List<Part> parts;

    private bool generate = false;
    public bool useMonoColor = false;
    
    // parts as 3x3 matrix
    int partDim = 3;
    bool[,] partDebug = { { true, false, false }, { false, true, false }, { false, false, true } };
    bool[,] part1 = { { true, true, false }, { false, false, false }, { false, false, false } };
    bool[,] part2 = { { true, true, true }, { false, false, false }, { false, false, false } };
    bool[,] part3 = { { true, false, false }, { false, false, false }, { false, false, false } };
    bool[,] part4 = { { true, true, false }, { true, false, false }, { false, false, false } };
    bool[,] part2Variant = { { true, true, false }, { false, false, false }, { false, false, false } };
    bool[,] part4Variant = { { true, true, false }, { true, true, false }, { false, false, false } };

    void OnGUI()
    {
        if (GUI.Button(new Rect(10, 70, 100, 30), "Generate"))
            GenerateAConfig();
    }

    private void Start()
    {
        graphicSupport = new List<GameObject>();
        for (int y = 0; y < supportDim.y; ++y)
        {
            for (int x = 0; x < supportDim.x; ++x)
            {
                GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.transform.position = new Vector3(x, y, 0);
                go.transform.localScale = new Vector3(0.95f, 0.95f, 0.95f);
                graphicSupport.Add(go);
            }
        }

        parts = new List<Part>();

        //parts.Add(new Part(1, partDebug, Color.blue));
        //parts.Add(new Part(2, partDebug, Color.green));

        parts.Add(new Part(1, part1, Color.blue));
        parts.Add(new Part(2, part2, Color.green));
        parts.Add(new Part(3, part3, Color.yellow));
        parts.Add(new Part(4, part4, new Color(1, 126f / 255f, 0)));
        parts.Add(new Part(5, part2Variant, Color.green));
        parts.Add(new Part(6, part4Variant, new Color(1, 126f / 255f, 0)));

        //parts.Add(new Part(1, part1, Color.green));
        //parts.Add(new Part(2, part2, Color.green));
        //parts.Add(new Part(3, part3, Color.green));
        //parts.Add(new Part(4, part4, Color.green));
        //parts.Add(new Part(5, part2Variant, Color.green));
        //parts.Add(new Part(6, part4Variant, Color.green));
    }

    List<List<int>> GenerateEmptySupport()
    {
        List<List<int>>  support = new List<List<int>>(supportDim.y);
        for (int i = 0; i < supportDim.y; ++i)
        {
            List<int> line = new List<int>(supportDim.x);
            for (int j = 0; j < supportDim.x; ++j)
                line.Add(0);
            support.Add(line);
        }
        return support;
    }

    Color GetPartColor(int id)
    {
        Color toReturn = Color.white;

        for(int i=0;i<parts.Count;++i)
            if (parts[i].id == id)
                toReturn = parts[i].color;

        return toReturn;
    }

    void GenerateAConfig()
    {
        List<List<int>> configSupport = FillSupport(parts, GenerateEmptySupport());

        if (configSupport.Count > 0)
            DisplaySupport(configSupport);
        else
            Debug.Log("unable to get config");
    }

    void ClearGraphicSupport()
    {
        for (int i = 0; i < graphicSupport.Count; ++i)
            graphicSupport[i].GetComponent<Renderer>().material.color = Color.white;
    }

    void DisplaySupport(List<List<int>> toDisplay)
    {
        ClearGraphicSupport();

        for(int y=0;y<toDisplay.Count;++y)
        {
            //string debugLine = "";
            for(int x=0;x<toDisplay[y].Count;++x)
            {
                if(useMonoColor && toDisplay[y][x] > 0)
                    graphicSupport[toDisplay[y].Count * y + x].GetComponent<Renderer>().material.color = Color.green;
                else
                    graphicSupport[toDisplay[y].Count * y + x].GetComponent<Renderer>().material.color = GetPartColor(toDisplay[y][x]);
                //debugLine += toDisplay[y][x] + " ";
            }
            //Debug.Log(debugLine);
        }
    }

    Part Rotate(Part p, int r)
    {
        Part rotatedPart = new Part(p.id, Copy(p.shape), p.color);

        if (r==1)
        {
            for (int y = 0; y < partDim; ++y)
                for (int x = 0; x < partDim; ++x)
                    rotatedPart.shape[y, x] = p.shape[partDim - 1 - x, y];
        }
        else if (r==2)
        {
            for (int y = 0; y < partDim; ++y)
                for (int x = 0; x < partDim; ++x)
                    rotatedPart.shape[y, x] = p.shape[partDim - 1 - y, partDim - 1 - x];
        }
        else if (r==3)
        {
            for (int y = 0; y < partDim; ++y)
                for (int x = 0; x < partDim; ++x)
                    rotatedPart.shape[y, x] = p.shape[x, partDim - 1 - y];
        }

        return rotatedPart;
    }

    List<List<int>> Place(Part part, List<List<int>> support)
    {
        support = Copy(support);

        int i = 0;
        int maxIteration = 100;
        bool placed = false;

        int randX = 0;
        int randY = 0;
        int rotation = 0;

        Part rotatedPart = part;

        while (!placed && i < maxIteration)
        {
            randX = Random.Range(0, supportDim.x + 1 - partDim);
            randY = Random.Range(0, supportDim.y + 1 - partDim);
            rotation = Random.Range(0, 4);

            rotatedPart = Rotate(part, rotation);

            bool stopStep = false;
            
            for (int y = 0; !stopStep && y < partDim; ++y)
            {
                for (int x = 0; !stopStep && x < partDim; x++)
                {
                    if (rotatedPart.shape[y,x] && support[randY + y][randX + x] > 0)
                    {
                        // partie déja occupée
                        i++;
                        stopStep = true;
                    }
                }
            }

            if(!stopStep)
                placed = true;
        }

        //Debug.Log("rand : " + randX + ", " + randY );

        if (placed)
        {
            for (int y = 0; y < partDim; ++y)
                for (int x = 0; x < partDim; ++x)
                    if (rotatedPart.shape[y, x])
                        support[randY + y][randX + x] = rotatedPart.id;

            return support;
        }
        else
            return new List<List<int>>();
    }

    List<Part> ShallowCopy(List<Part> toCopy)
    {
        List<Part> copy = new List<Part>();
        for (int i = 0; i < toCopy.Count; ++i)
        {
            copy.Add(toCopy[i]);
        }
        return copy;
    }

    List<List<int>> Copy(List<List<int>> toCopy)
    {
        List<List<int>> copy = new List<List<int>>();

        for (int i = 0; i < toCopy.Count; ++i)
        {
            List<int> line = new List<int>();
            for(int j = 0; j < toCopy[i].Count; ++j)
                line.Add(toCopy[i][j]);
            copy.Add(line);
        }

        return copy;
    }

    bool[,] Copy(bool[,] toCopy)
    {
        bool[,] copy = new bool[partDim, partDim];

        for (int i=0;i<partDim;++i)
            for(int j=0;j< partDim;++j)
                copy[i, j] = toCopy[i, j];

        return copy;
    }

    List<List<int>> FillSupport(List<Part> remainingParts, List<List<int>> support)
    {
        //Debug.Log("nb of remaining parts : " + remainingParts.Count);

        if (remainingParts.Count == 0)
            return support;
        
        Part part = remainingParts[0];
        
        List<List<int>> updatedSupport = Place(part, support);

        if (updatedSupport.Count > 0)
        {
            List<Part> newRemmainingParts = ShallowCopy(remainingParts);
            newRemmainingParts.RemoveAt(0);
            return FillSupport(newRemmainingParts, updatedSupport);
        }
        else
            return new List<List<int>>(); // unable to place the part
    }

    // Update is called once per frame
    void Update()
    {
        if (generate)
        {
            GenerateAConfig();
            generate = false;
        }
    }
}
