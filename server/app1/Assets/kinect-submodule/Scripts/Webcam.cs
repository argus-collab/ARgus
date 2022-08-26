using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Webcam : MonoBehaviour
{
    public Material webcamMaterial;
    public int device = 2;

    private WebCamTexture webcamTexture;

    // Start is called before the first frame update
    void Start()
    {
        webcamTexture = new WebCamTexture(WebCamTexture.devices[device].name);

        Renderer rend = GetComponent<Renderer>();
        rend.material.mainTexture = webcamTexture;
        webcamTexture.Play();
    }

    void OnApplicationQuit()
    {
        webcamTexture.Stop();
    }

}
