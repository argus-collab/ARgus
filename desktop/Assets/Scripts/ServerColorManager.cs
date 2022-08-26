using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ServerColorManager : MonoBehaviour
{
    public GameObject hololensSceneRoot;
    public GameObject kinectSceneRoot;

    public UDPSceneManager udpSceneManager;
    public CustomServerNetworkManager customServerManager;

    public List<string> exceptions;
    public Color defaultColor;

    public bool displayGUI = false;

    private List<GameObject> hololensSceneStateAtColorChange;
    private List<GameObject> kinectSceneStateAtColorChange;
    private List<Color> hololensSceneInitialColors;
    private List<Color> kinectSceneInitialColors;

    private bool hiddenColor = false;

    private void Start()
    {
        hololensSceneStateAtColorChange = new List<GameObject>();
        kinectSceneStateAtColorChange = new List<GameObject>();
        hololensSceneInitialColors = new List<Color>();
        kinectSceneInitialColors = new List<Color>();
    }

    void OnGUI()
    {
        if (displayGUI)
        {
            if (!hiddenColor && GUI.Button(new Rect(500, 10, 100, 30), "Hide colors"))
            {
                HideColors();
                hiddenColor = true;
            }

            if (hiddenColor && GUI.Button(new Rect(500, 10, 100, 30), "Restore colors"))
            {
                RestoreColors();
                hiddenColor = false;
            }
        }
    }

    // TODO : make it automatic by updating scene state in Update call
    public void ResetScene()
    {
        hololensSceneStateAtColorChange.Clear();
        hololensSceneInitialColors.Clear();

        kinectSceneStateAtColorChange.Clear();
        kinectSceneInitialColors.Clear();
    }

    public void ChangeColorState()
    {
        if (hiddenColor)
        {
            RestoreColors();
            hiddenColor = false;
        }
        else
        {
            HideColors();
            hiddenColor = true;
        }
    }

    public void HideColors()
    {
        RecordColorBeforeChangeHololensScene();
        RecordColorBeforeChangeKinectScene();
        ApplyDefaultColor();

        HideColorOnRemoteUser();
        HideColorOnHololensUser();
    }

    public void RestoreColors()
    {
        // hololens scene
        for (int i = 0; i < hololensSceneStateAtColorChange.Count; ++i)
            ApplyColorsDeeply(hololensSceneStateAtColorChange[i].transform, hololensSceneInitialColors[i]);

        // kinect scene
        for (int i = 0; i < kinectSceneStateAtColorChange.Count; ++i)
            ApplyColorsDeeply(kinectSceneStateAtColorChange[i].transform, kinectSceneInitialColors[i]);

        RestoreColorOnRemoteUser();
        RestoreColorOnHololensUser();
    }

    void HideColorOnHololensUser()
    {
        for (int i = 0; i < hololensSceneStateAtColorChange.Count; ++i)
            customServerManager.UpdateGameObjectColorInScenePart(hololensSceneStateAtColorChange[i].name, defaultColor, true);
    }

    void RestoreColorOnHololensUser()
    {
        for (int i = 0; i < hololensSceneStateAtColorChange.Count; ++i)
            customServerManager.UpdateGameObjectColorInScenePart(hololensSceneStateAtColorChange[i].name, hololensSceneInitialColors[i], true);
    }

    void HideColorOnRemoteUser()
    {
        for (int i = 0; i < kinectSceneStateAtColorChange.Count; ++i)
            udpSceneManager.UpdateGameObjectsColorSender(kinectSceneStateAtColorChange[i], defaultColor);
    }

    void RestoreColorOnRemoteUser()
    {
        for (int i = 0; i < kinectSceneStateAtColorChange.Count; ++i)
            udpSceneManager.UpdateGameObjectsColorSender(kinectSceneStateAtColorChange[i], kinectSceneInitialColors[i]);
    }

    void ApplyDefaultColor()
    {
        // hololens scene
        for (int i = 0; i < hololensSceneStateAtColorChange.Count; ++i)
            HideColorsDeeply(hololensSceneStateAtColorChange[i].transform);

        // kinect scene
        for (int i = 0; i < kinectSceneStateAtColorChange.Count; ++i)
            HideColorsDeeply(kinectSceneStateAtColorChange[i].transform);
    }

    void RecordColorBeforeChangeHololensScene()
    {
        for (int i = 0; i < hololensSceneRoot.transform.childCount; ++i)
        {
            GameObject go = hololensSceneRoot.transform.GetChild(i).gameObject;
            if (!exceptions.Contains(go.name))
            {
                Color mainColor = GetMainColor(go.transform);
                if (mainColor != Color.clear)
                {
                    hololensSceneStateAtColorChange.Add(go);
                    hololensSceneInitialColors.Add(mainColor);
                }
            }
        }
    }

    void RecordColorBeforeChangeKinectScene()
    {
        for (int i = 0; i < kinectSceneRoot.transform.childCount; ++i)
        {
            GameObject go = kinectSceneRoot.transform.GetChild(i).gameObject;
            if (!exceptions.Contains(go.name + "Client"))
            {
                Color mainColor = GetMainColor(go.transform);
                if (mainColor != Color.clear)
                {
                    kinectSceneStateAtColorChange.Add(go);
                    kinectSceneInitialColors.Add(mainColor);
                }
            }
        }
    }

    Color GetMainColor(Transform t)
    {
        Renderer rend = t.GetComponent<Renderer>();

        if (rend != null)
            return rend.sharedMaterial.color;
        else
        {
            for (int i = 0; i < t.childCount; ++i)
            {
                Color mainColor = GetMainColor(t.GetChild(i));
                if (mainColor != Color.clear)
                    return mainColor;
            }
        }
        
        return Color.clear;
    }

    void HideColorsDeeply(Transform t)
    {
        Renderer rend = t.GetComponent<Renderer>();

        if (rend != null)
            for (int i = 0; i < rend.materials.Length; ++i)
                rend.materials[i].color = defaultColor;

        for (int i = 0; i < t.childCount; ++i)
            HideColorsDeeply(t.GetChild(i));
    }

    void ApplyColorsDeeply(Transform t, Color c)
    {
        Renderer rend = t.GetComponent<Renderer>();

        if (rend != null)
            for (int i = 0; i < rend.materials.Length; ++i)
                rend.materials[i].color = c;

        for (int i = 0; i < t.childCount; ++i)
            ApplyColorsDeeply(t.GetChild(i), c);
    }
}
