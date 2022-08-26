using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandCalibration : MonoBehaviour
{
    public GameObject calibratedGO;
    public NetworkSyncScenePart net;

    [Header("Calibration cube")]
    public GameObject cubePt1;
    public GameObject cubePt2;
    public GameObject cubePt3;
    public GameObject cubePt4;

    [Header("Hand placed points")]
    public GameObject handPt1;
    public GameObject handPt2;
    public GameObject handPt3;
    public GameObject handPt4;

    [Header("Remote calibration")]
    //public string remoteScenePartName;
    public RemoteCalibrationClient remoteCalib;

    private void Start()
    {
        //calibratedGO.SetActive(false);
        CalibrateCube();
    }

    public void HideCalibration()
    {
        Debug.Log("hide calibration");
        gameObject.SetActive(false);
    }

    public void HideScene()
    {
        Debug.Log("hide scene");
        calibratedGO.SetActive(false);
    }

    public void CalibrateCube()
    {
        Debug.Log("calibrate with cube");
        
        calibratedGO.SetActive(true);

        //calibratedGO.transform.position = -(Quaternion.Inverse(transform.rotation) * transform.position);
        //calibratedGO.transform.rotation = Quaternion.Inverse(transform.rotation);

        calibratedGO.transform.position = transform.position;
        calibratedGO.transform.rotation = transform.rotation;

        //net.CalibrateScene(remoteScenePartName, transform.position, transform.rotation);
        string name = net.network.GetIp();
        net.CalibrateScene(name, -(Quaternion.Inverse(transform.rotation) * transform.position), Quaternion.Inverse(transform.rotation));

        GameObject player = GameObject.Find("HololensRepresentation(Clone)");
        player.GetComponent<NetworkPlayer>().SetOffset(-(Quaternion.Inverse(transform.rotation) * transform.position), Quaternion.Inverse(transform.rotation));
    }

    public void CalibrateSphere()
    {
        Debug.Log("calibrate with spheres");

        //calibratedGO.SetActive(true);
        remoteCalib.AskForUmeyamaCalibration(
                cubePt1.transform.position,
                cubePt2.transform.position,
                cubePt3.transform.position,
                cubePt4.transform.position,

                handPt1.transform.position,
                handPt2.transform.position,
                handPt3.transform.position,
                handPt4.transform.position
            );
    }
}
