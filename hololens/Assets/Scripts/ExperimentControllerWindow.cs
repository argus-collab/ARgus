#if !UNITY_WSA
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;


public class ExperimentControllerWindow : EditorWindow
{
    string uid;
    string textCommand;
    ExperimentControllerBehaviour experimentController;

    private Color tutoColorState = Color.yellow;
    private Color tutoColorText = Color.black;

    private Color defaultColorState = Color.white;
    private Color defaultColorText = Color.black;

    private Color recordingColorState = Color.red;
    private Color recordingColorText = Color.white;

    private Color warningColorState = Color.green;
    private Color warningColorText = Color.black;

    string stateText;
    Color stateColor = Color.white;

    [MenuItem("Experiment Controller/Display window")]
    static void DisplayWindow()
    {
        ExperimentControllerWindow window = (ExperimentControllerWindow)EditorWindow.GetWindow(typeof(ExperimentControllerWindow));
        Debug.Log("Display experiment controller window");
    }

    private void ResetInfoBar()
    {
        stateText = "waiting command...";
        stateColor = defaultColorState;
    }

    private void OnEnable()
    {
        ResetInfoBar();
    }

    void DisplayTitle(string title, GUIStyle style)
    {
        GUIStyle styleEspace = new GUIStyle();
        styleEspace.fontSize = 6;

        EditorGUILayout.LabelField(" ", styleEspace);
        EditorGUILayout.LabelField(title, style);
        //EditorGUILayout.LabelField(" ", styleEspace);
    }

    private void OnGUI()
    {
        GUIStyle styleTitre = new GUIStyle();
        styleTitre.fontSize = 14;
        styleTitre.fontStyle = FontStyle.Bold;


        GUIStyle styleInfo = new GUIStyle();
        styleInfo.fontSize = 14;
        styleInfo.alignment = TextAnchor.MiddleCenter;
        styleInfo.margin = new RectOffset(10,10,10,10);

        // uid
        uid = EditorGUILayout.TextField("UID:", uid);

        experimentController = GameObject.FindObjectOfType<ExperimentControllerBehaviour>();
        if (experimentController == null)
        {
            EditorGUILayout.HelpBox("Please add experiment controller in scene", MessageType.Error);
            this.Repaint();
        }
        else
        {
            if(!experimentController.IsParametersFilled())
            {
                EditorGUILayout.HelpBox("Please add parameters in experiment controller", MessageType.Error);
                this.Repaint();
            }
        }

        GUILayout.BeginHorizontal();

        // state
        GUILayout.BeginVertical();
        DisplayTitle("State", styleTitre);
        Texture2D texture = new Texture2D(50, 50);
        for (int y = 0; y < texture.height; y++)
            for (int x = 0; x < texture.width; x++)
                texture.SetPixel(x, y, stateColor);
        texture.Apply();

        //GUILayout.Label(texture);
        //GUILayout.Label(stateText, styleInfo);
        GUIContent content = new GUIContent();
        content.text = stateText;
        content.image = texture;
        GUILayout.Label(content);
        GUILayout.EndVertical();

        // logs
        GUILayout.BeginVertical();
        DisplayTitle("Logs", styleTitre);
        experimentController.ListLogs(uid);
        List<string> logs = experimentController.GetListLogs();
        foreach(string log in logs)
        {
            GUILayout.Label(log);
        }
        GUILayout.EndVertical();


        GUILayout.EndHorizontal();

        // resets
        DisplayTitle("Resets", styleTitre);
        GUILayout.BeginHorizontal("box");
        if (GUILayout.Button("Kinect"))
        {
            experimentController.ResetSourceManager();
        }

        if (GUILayout.Button("Scene"))
        {
            experimentController.ResetScene();
            ResetInfoBar();
        }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        textCommand = EditorGUILayout.TextField("Command:", textCommand);
        if (GUILayout.Button("Process"))
        {
            experimentController.InputTextualCommand(textCommand);
            textCommand = "";
        }
        GUILayout.EndHorizontal();

        // tutos
        DisplayTitle("Tutorials", styleTitre);
        GUILayout.BeginHorizontal("box");
        if (GUILayout.Button("Hololens"))
        {
            experimentController.LoadTutoHololens();
            stateText = "TUTORIAL hololens";
            stateColor = tutoColorState;
        }

        if (GUILayout.Button("Kinect"))
        {
            experimentController.LoadTutoKinect();
            stateText = "TUTORIAL kinect";
            stateColor = tutoColorState;
        }

        if (GUILayout.Button("Virtual"))
        {
            experimentController.LoadTutoVirtual();
            stateText = "TUTORIAL virtual";
            stateColor = tutoColorState;
        }
        GUILayout.EndHorizontal();

        // task
        DisplayTitle("Tasks", styleTitre);
        if (uid.Length > 0)
        {
            int uid_int = int.Parse(uid);
            GUILayout.BeginHorizontal("box");
            if (GUILayout.Button("Task 1")) StartTask(0, uid_int);
            if (GUILayout.Button("Task 2")) StartTask(1, uid_int);
            if (GUILayout.Button("Task 3")) StartTask(2, uid_int);
            if (GUILayout.Button("Task 4")) StartTask(3, uid_int);
            GUILayout.EndHorizontal();

            if (experimentController.IsTaskRunning())
            {
                if (GUILayout.Button("STOP "))
                {
                    experimentController.StopTask();
                    ResetInfoBar();
                }
            }
        }
    }

    void StartTask(int taskid, int uid)
    {
        experimentController.StartTask(taskid, uid);
        stateText = "RECORDING task " + (taskid + 1) + " - uid " + uid;
        stateColor = recordingColorState;
    }
}
#endif