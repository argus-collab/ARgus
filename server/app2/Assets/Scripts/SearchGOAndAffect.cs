using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

public class SearchGOAndAffect : MonoBehaviour
{
    public MonoBehaviour toFill;
    public string paramName;
    public string toSearch;
    public bool enableOnFound = false;

    void Update()
    {
        GameObject go = GameObject.Find(toSearch);
        if (go != null)
        {
            if(enableOnFound)
                toFill.enabled = true;

            System.Type type = toFill.GetType();
            FieldInfo field = type.GetField(paramName);
            field.SetValue(toFill, go);
        }
        else
        {
            if (enableOnFound)
                toFill.enabled = false;
        }
    }
}
