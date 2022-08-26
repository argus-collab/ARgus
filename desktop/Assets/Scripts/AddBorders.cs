using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddBorders : MonoBehaviour
{
    public List<GameObject> borders;
    private List<GameObject> bordersInstances;

    void Start()
    {
        bordersInstances = new List<GameObject>();

        foreach(GameObject go in borders)
            bordersInstances.Add(Instantiate(go));
    }

    void Update()
    {
        foreach(GameObject go in bordersInstances)
        {
            go.transform.position = transform.position;
            go.transform.rotation = transform.rotation;
        }
    }

    private void OnDestroy()
    {
        foreach (GameObject go in bordersInstances)
            Destroy(go);
    }
}
