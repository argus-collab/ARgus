using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class ImportCalibration : MonoBehaviour
{
    public GameObject sceneRoot;
    public string fileName = "visp_calibration.txt";

    private void OnGUI()
    {
        if (GUI.Button(new Rect(10, 100, 200, 20), "import calibration"))
            ReadCalibrationFromFile();
    }

    public void ReadCalibrationFromFile()
    {
        StreamReader reader = new StreamReader(fileName);
        sceneRoot.transform.localPosition = StringToVector3(reader.ReadLine());
        sceneRoot.transform.localRotation = Quaternion.Euler(StringToVector3(reader.ReadLine()));
        reader.Close();

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
