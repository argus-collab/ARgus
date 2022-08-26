using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpotGenerator : MonoBehaviour
{
    public string titleTuto;
    public string descriptionTuto;

    public List<string> titles;
    public List<string> descriptions;
    public Text ui;

    private int index1;
    private int index2;
    private int index3;

    private bool tuto;

    void Start()
    {
        if (titles.Count != descriptions.Count)
            Debug.LogError("titles length need to fit descriptions length!");

        if (titles.Count <= 0 && descriptions.Count <= 0)
            Debug.LogError("titles and/or descriptions are needed");

        Change();
    }

    void Update()
    {
        if (tuto)
        {
            string uiText = "";
            uiText += titleTuto + ": " + descriptionTuto;
            ui.text = uiText;
        }
        else
        {
            string uiText = "";
            uiText += "- " + titles[index1] + ": " + descriptions[index1] + "\n";
            uiText += "- " + titles[index2] + ": " + descriptions[index2] + "\n";
            uiText += "- " + titles[index3] + ": " + descriptions[index3];
            ui.text = uiText;
        }
    }

    public void Change()
    {
        index1 = Random.Range(0, titles.Count);

        do
        {
            index2 = Random.Range(0, titles.Count);
        }
        while (index2 == index1);
        
        do
        {
            index3 = Random.Range(0, titles.Count);
        }
        while (index3 == index1 || index3 == index2);
    }

    public void SetTuto(bool state)
    {
        tuto = state;
    }
}
