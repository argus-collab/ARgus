using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Xml.Serialization;
public class Personnage
{
    [XmlAttribute("name")] public string Name;
    [XmlAttribute("hp")] public int Hp;
    [XmlAttribute("mp")] public int Mp;
    public string Classe;
}

public class ImportExport : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
