using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class ExportCalibration : MonoBehaviour
{
    public GameObject sceneRoot;
    public string fileName = "visp_calibration.txt";

    private void OnGUI()
    {
        if (GUI.Button(new Rect(10, 130, 200, 20), "export calibration"))
            WriteCalibrationInFile();
    }

    public void WriteCalibrationInFile()
    {
        StreamWriter writer = new StreamWriter(fileName, false);
        writer.WriteLine(sceneRoot.transform.position);
        writer.WriteLine(sceneRoot.transform.rotation.eulerAngles);
        writer.Close();

        Debug.Log("Calibration exported");
    }

    private static Vector3 StringToVector3(string sVector)
    {
        if (sVector.StartsWith("(") && sVector.EndsWith(")"))
            sVector = sVector.Substring(1, sVector.Length - 2);

        string[] sArray = sVector.Split(',');

        return new Vector3(
            float.Parse(sArray[0]),
            float.Parse(sArray[1]),
            float.Parse(sArray[2]));
    }

}
