using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ViewRecorder : MonoBehaviour
{
    public Camera mainCamera;
    public Camera virtualCamera;
    public MeshRenderer kinectRenderer;
    public MeshRenderer hololensRenderer;
    public UINotification notification;

    public GameObject hololensCamera;
    public GameObject kinectCamera;

    public GameObject miniatures;


    // Take a shot immediately
    //IEnumerator Start()
    //{
    //    UploadPNG();
    //    yield return null;
    //}

    //IEnumerator UploadPNG()
    //{
    //    // We should only read the screen buffer after rendering is complete
    //    yield return new WaitForEndOfFrame();

    //    // Create a texture the size of the screen, RGB24 format
    //    int width = Screen.width;
    //    int height = Screen.height;
    //    Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);

    //    // Read screen contents into the texture
    //    tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
    //    tex.Apply();

    //    // Encode texture into PNG
    //    byte[] bytes = tex.EncodeToPNG();
    //    Destroy(tex);

    //    // For testing purposes, also write to a file in the project folder
    //    // File.WriteAllBytes(Application.dataPath + "/../SavedScreen.png", bytes);


    //    // Create a Web Form
    //    WWWForm form = new WWWForm();
    //    form.AddField("frameCount", Time.frameCount.ToString());
    //    form.AddBinaryData("fileUpload", bytes);

    //    // Upload to a cgi script
    //    var w = UnityWebRequest.Post("http://localhost/cgi-bin/env.cgi?post", form);
    //    yield return w.SendWebRequest();
    //    if (w.result != UnityWebRequest.Result.Success)
    //        print(w.error);
    //    else
    //        print("Finished Uploading Screenshot");
    //    yield return null;
    //}


    void Update()
    {
        if (Input.GetKeyDown("s"))
        {
            StartCoroutine(Capture());
        }
    }

    IEnumerator Capture()
    {
        yield return new WaitForEndOfFrame();

        float time = Time.time;

        CameraCapture(mainCamera, "main", time, true);
        //CameraCapture(virtualCamera, "virtual", time, true);
        VirtualCapture(time);
        if (hololensCamera != null) HololensCapture(time);
        if (kinectCamera != null) KinectCapture(time);

        notification.DisplayTemporaryNotification("screenshot");

        yield return null;
    }

    void CameraCapture(Camera cam, string name, float time, bool colorBackground = false)
    {
        if (colorBackground) cam.backgroundColor = new Color(7f/255f, 122f/255f, 204f/255f, 1f);

        RenderTexture temp = new RenderTexture(1920, 1080, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
        cam.targetTexture = temp;

        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = cam.targetTexture;

        cam.Render();

        Texture2D Image = new Texture2D(cam.targetTexture.width, cam.targetTexture.height);
        Image.ReadPixels(new Rect(0, 0, cam.targetTexture.width, cam.targetTexture.height), 0, 0);
        Image.Apply();
        RenderTexture.active = currentRT;

        var Bytes = Image.EncodeToPNG();

        cam.targetTexture = null;

        Destroy(Image);
        Destroy(temp);

        File.WriteAllBytes(Application.dataPath + "/" + name + "_" + time + ".png", Bytes);
    }

    void VirtualCapture(float time)
    {
        Camera virtualCam = virtualCamera;
        RenderTexture temp = new RenderTexture(1920, 1080, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
        mainCamera.targetTexture = temp;

        Vector3 initialPosition = mainCamera.transform.position;
        Quaternion initialRotation = mainCamera.transform.rotation;

        mainCamera.transform.position = virtualCam.transform.position;
        mainCamera.transform.rotation = virtualCam.transform.rotation;

        int initialMask = mainCamera.cullingMask;

        //mainCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("Client"));
        //mainCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("VideoRenderer"));
        //mainCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("ParticipantPlayer"));
        //mainCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("Default"));
        //mainCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("Ground"));
        //mainCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("InvisibleMiniature"));
        mainCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("UI"));

        miniatures.SetActive(false);
        CameraCapture(mainCamera, "virtual", time, true);
        miniatures.SetActive(true);

        mainCamera.cullingMask = initialMask;
        //mainCamera.cullingMask = ~0;
        //mainCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("ParticipantPlayer"));

        mainCamera.transform.position = initialPosition;
        mainCamera.transform.rotation = initialRotation;

        mainCamera.targetTexture = null;

        Destroy(temp);

        //Texture2D tex = GetTextureFromSurfaceShader(hololensRenderer.material, 1920, 1080);
        //var Bytes = tex.EncodeToPNG();
        //File.WriteAllBytes(Application.dataPath + "/Images/hololens_" + time + ".png", Bytes);
    }

    void HololensCapture(float time)
    {
        float initialAlpha = hololensRenderer.material.GetFloat("_alpha");

        bool initialRendererState = hololensRenderer.enabled;
        hololensRenderer.enabled = true;
        hololensRenderer.material.SetFloat("_alpha", 1f);

        Camera hololensCam = hololensCamera.GetComponentInChildren<Camera>();
        RenderTexture temp = new RenderTexture(1920, 1080, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
        mainCamera.targetTexture = temp;

        Vector3 initialPosition = mainCamera.transform.position;
        Quaternion initialRotation = mainCamera.transform.rotation;

        mainCamera.transform.position = hololensCam.transform.position;
        mainCamera.transform.rotation = hololensCam.transform.rotation;

        int initialMask = mainCamera.cullingMask;

        mainCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("Client"));
        mainCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("VideoRenderer"));
        mainCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("ParticipantPlayer"));
        mainCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("Default"));
        mainCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("Ground"));
        mainCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("InvisibleMiniature"));
        mainCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("UI"));

        miniatures.SetActive(false);
        CameraCapture(mainCamera, "hololens", time);
        miniatures.SetActive(true);

        mainCamera.cullingMask = initialMask;
        //mainCamera.cullingMask = ~0;
        //mainCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("ParticipantPlayer"));

        mainCamera.transform.position = initialPosition;
        mainCamera.transform.rotation = initialRotation;

        hololensRenderer.enabled = initialRendererState;
        hololensRenderer.material.SetFloat("_alpha", initialAlpha);
        mainCamera.targetTexture = null;

        Destroy(temp);

        //Texture2D tex = GetTextureFromSurfaceShader(hololensRenderer.material, 1920, 1080);
        //var Bytes = tex.EncodeToPNG();
        //File.WriteAllBytes(Application.dataPath + "/Images/hololens_" + time + ".png", Bytes);
    }

    void KinectCapture(float time)
    {
        float initialAlpha = kinectRenderer.material.GetFloat("_alpha");

        bool initialRendererState = kinectRenderer.enabled;
        kinectRenderer.enabled = true;
        kinectRenderer.material.SetFloat("_alpha", 1f);

        Camera kinectCam = kinectCamera.GetComponentInChildren<Camera>();
        RenderTexture temp = new RenderTexture(1920, 1080, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
        mainCamera.targetTexture = temp; 

        Vector3 initialPosition = mainCamera.transform.position;
        Quaternion initialRotation = mainCamera.transform.rotation;

        mainCamera.transform.position = kinectCam.transform.position;
        mainCamera.transform.rotation = kinectCam.transform.rotation;

        int initialMask = mainCamera.cullingMask;

        mainCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("Client"));
        mainCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("VideoRenderer"));
        mainCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("ParticipantPlayer"));
        mainCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("Default"));
        mainCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("Ground"));
        mainCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("VisibleVirtualAndHololens"));
        mainCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("InvisibleMiniature"));
        mainCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("UI"));

        miniatures.SetActive(false);

        CameraCapture(mainCamera, "kinect", time);

        miniatures.SetActive(true);

        mainCamera.cullingMask = initialMask;
        //mainCamera.cullingMask = ~0;
        //mainCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("ParticipantPlayer"));

        mainCamera.transform.position = initialPosition;
        mainCamera.transform.rotation = initialRotation;

        kinectRenderer.enabled = initialRendererState;
        kinectRenderer.material.SetFloat("_alpha", initialAlpha);
        mainCamera.targetTexture = null;

        Destroy(temp);

        //Texture2D tex = GetTextureFromSurfaceShader(kinectRenderer.material, 1920, 1080);
        //var Bytes = tex.EncodeToPNG();
        //File.WriteAllBytes(Application.dataPath + "/Images/kinect_" + time + ".png", Bytes);
    }

    // adapted fromfrom: https://forum.unity.com/threads/getting-output-textures-from-a-surface-shader.690619/
    public Texture2D GetTextureFromSurfaceShader(Material mat, int width, int height)
    {
        float initialAlpha = mat.GetFloat("_alpha");
        mat.SetFloat("_alpha", 1f);


        //Create render texture:
        RenderTexture temp = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);

        //Create a Quad:
        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        MeshRenderer rend = quad.GetComponent<MeshRenderer>();
        rend.material = mat;
        //Vector3 quadScale = quad.transform.localScale / (float)((Screen.height / 2.0) / Camera.main.orthographicSize);
        quad.transform.position = Vector3.forward;
        quad.transform.localScale = new Vector3(width, height,0);

        //Setup camera:
        GameObject camGO = new GameObject("CaptureCam");
        Camera cam = camGO.AddComponent<Camera>();
        cam.transform.position = new Vector3(0,0,-1);
        cam.renderingPath = RenderingPath.Forward;
        cam.orthographic = true;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0, 0, 0, 1);
        //if (cam.rect.width < 1 || cam.rect.height < 1)
        //{
        //    cam.rect = new Rect(cam.rect.x, cam.rect.y, 1, 1);
        //}

        //cam.rect = new Rect(0, 0, quadScale.x, quadScale.y);
        //cam.aspect = quadScale.x / quadScale.y;

        cam.aspect = 1.8f;
        cam.rect = new Rect(0, 0, 1, 1);
        cam.orthographicSize = height / 2;


        cam.targetTexture = temp;
        cam.allowHDR = false;


        //Capture image and write to the render texture:
        cam.Render();
        temp = cam.targetTexture;

        //Apply changes:
        Texture2D newTex = new Texture2D(temp.width, temp.height, TextureFormat.ARGB32, true, true);
        RenderTexture.active = temp;
        newTex.ReadPixels(new Rect(0, 0, temp.width, temp.height), 0, 0);
        newTex.Apply();

        //Clean up:
        RenderTexture.active = null;
        temp.Release();
        Destroy(quad);
        Destroy(camGO);

        mat.SetFloat("_alpha", initialAlpha);


        return newTex;
    }
}
