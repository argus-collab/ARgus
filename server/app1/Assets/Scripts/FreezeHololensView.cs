#if !UNITY_WSA
using System.Collections.Generic;
using UnityEngine;

public class FreezeHololensView : MonoBehaviour
{
    public GameObject hololensPlayer;
    public VirtualViewPositionRecorder virtualPosRecorder;
    public ChangeViewCinemachine viewManager;

    public MeshRenderer HololensVideo;
    public MeshRenderer HololensFreezedVideo;

    public GameObject freezeButton;
    public GameObject unfreezeButton;

    private Camera toGo = null;

    private int i = -1;
    private string cameraName;

    //private Texture2D hololensScreenShot;
    private Texture2D YPlane;
    private Texture2D UPlane;
    private Texture2D VPlane;

    // "real" hololens position (position at image display)
    public float latency = 0.5f;
    private Vector3 hololensPositionAtImage;
    private Quaternion hololensRotationAtImage;
    private List<Vector3> hololensPositionBuffer;
    private List<Quaternion> hololensRotationBuffer;
    private List<float> hololensTimeStamp;

    private bool isFreezed = false;
    private bool isUsedWithStick = false;

    public LogManager logger;

    private void Start()
    {
        hololensPositionBuffer = new List<Vector3>();
        hololensRotationBuffer = new List<Quaternion>();
        hololensTimeStamp = new List<float>();
    }

    public void Update()
    {
        if (hololensPlayer == null)
            return;

        if (i >= 0)
            i++;

        // we wait for cinemachine to be ready 
        if (i == 10)
        {
            viewManager.DisplayCameraView(toGo, false);//, true);
            viewManager.HideGround();
            viewManager.HideDefault();
            viewManager.HideInvisibleMiniature();
            viewManager.HideClient();
            i = -1;
            isUsedWithStick = true;
        }

        hololensPositionBuffer.Add(hololensPlayer.transform.position);
        hololensRotationBuffer.Add(hololensPlayer.transform.rotation);
        hololensTimeStamp.Add(Time.time);

        // latency management
        int index = 0;
        while(hololensTimeStamp.Count > 0 && (Time.time - hololensTimeStamp[index] > latency))
        {
            hololensPositionBuffer.RemoveAt(index);
            hololensRotationBuffer.RemoveAt(index);
            hololensTimeStamp.RemoveAt(index);
            index++;
        }

        hololensPositionAtImage = hololensPositionBuffer[0];
        hololensRotationAtImage = hololensRotationBuffer[0];







        if (isFreezed)
        {
            // user can defreeze
            //freezeButton.SetActive(false);
            //unfreezeButton.SetActive(true);
        }
        else if (viewManager.IsHololensView() && !isFreezed)
        {
            // user can freeze
            //freezeButton.SetActive(true);
            //unfreezeButton.SetActive(false);
        }
        else
        {
            // not freeze
            freezeButton.SetActive(false);
            unfreezeButton.SetActive(false);
        }

        if ((/*viewManager.IsHololensView() || */viewManager.IsInTransition() || viewManager.IsKinectView() || viewManager.IsVirtualView()) && isFreezed)
        {
            // defreeze
            freezeButton.SetActive(false);
            unfreezeButton.SetActive(false);
            isFreezed = false;
            logger.LogStopFreezeHololensView();
        }

        //if (isFreezed)
        //    Debug.Log("FREEEEEZED : " + viewManager.IsHololensView() + " - " + viewManager.IsKinectView() + " - " + viewManager.IsVirtualView());
        //else
        //    Debug.Log("NOT FREEEEEZED");


        // stick management
        if (viewManager.IsHololensView() && !isFreezed && Input.GetKeyDown(KeyCode.Space))
        {
            Freeze();
            
        }
        if (isUsedWithStick && !Input.GetKey(KeyCode.Space))
        {
            Unfreeze();
            isUsedWithStick = false;
        }

    }

    private void CreateTexture()
    {
        TextureFormat yplane_format = (((Texture2D)HololensVideo.material.GetTexture("_YPlane")).format);
        UnityEngine.Experimental.Rendering.GraphicsFormat yplane_formatGF = HololensVideo.material.GetTexture("_YPlane").graphicsFormat;
        int yplane_width = (HololensVideo.material.GetTexture("_YPlane").width);
        int yplane_height = (HololensVideo.material.GetTexture("_YPlane").height);

        TextureFormat uplane_format = (((Texture2D)HololensVideo.material.GetTexture("_UPlane")).format);
        UnityEngine.Experimental.Rendering.GraphicsFormat uplane_formatGF = HololensVideo.material.GetTexture("_UPlane").graphicsFormat;
        int uplane_width = (HololensVideo.material.GetTexture("_UPlane").width);
        int uplane_height = (HololensVideo.material.GetTexture("_UPlane").height);

        TextureFormat vplane_format = (((Texture2D)HololensVideo.material.GetTexture("_VPlane")).format);
        UnityEngine.Experimental.Rendering.GraphicsFormat vplane_formatGF = HololensVideo.material.GetTexture("_VPlane").graphicsFormat;
        int vplane_width = (HololensVideo.material.GetTexture("_VPlane").width);
        int vplane_height = (HololensVideo.material.GetTexture("_VPlane").height);

        //hololensScreenShot = new Texture2D(2, 2, TextureFormat.ARGB32, false);
        YPlane = new Texture2D(yplane_width, yplane_height, yplane_format, false);
        UPlane = new Texture2D(uplane_width, uplane_height, uplane_format, false);
        VPlane = new Texture2D(vplane_width, vplane_height, vplane_format, false);

        //YPlane = new Texture2D(yplane_width, yplane_height, yplane_formatGF, 9, UnityEngine.Experimental.Rendering.TextureCreationFlags.MipChain);
        //UPlane = new Texture2D(uplane_width, uplane_height, uplane_formatGF, 9, UnityEngine.Experimental.Rendering.TextureCreationFlags.MipChain);
        //VPlane = new Texture2D(vplane_width, vplane_height, vplane_formatGF, 9, UnityEngine.Experimental.Rendering.TextureCreationFlags.MipChain);
    }

    public void Freeze()
    {
        //viewManager.SetHideFreezedHololensView(false);
        Debug.Log("FREEZE");
        if (hololensPlayer==null)
        {
            Debug.LogError("no hololens in scene");
            return;
        }

        virtualPosRecorder.FlushInvisibleCamera();

        // with latency support
        //cameraName = virtualPosRecorder.RegisterInvisibleCameraPosition(hololensPositionAtImage, hololensRotationAtImage);

        // without latency support
        cameraName = virtualPosRecorder.RegisterInvisibleCameraPosition(hololensPlayer.transform.position, hololensPlayer.transform.rotation);
        


        GameObject virtualPos = virtualPosRecorder.GetLastVirtualCameraRepresentation();

        Camera cam = virtualPos.GetComponentInChildren<Camera>();
        toGo = cam;
        Debug.Log("go to " + cam.gameObject.name);
        Debug.Log("at " + cam.gameObject.transform.position);
        i = 0;

        // solution 1
        //StartCoroutine(RecordFrame());

        // solution 2
        CreateTexture();
        Graphics.CopyTexture(HololensVideo.material.GetTexture("_YPlane"), YPlane);
        Graphics.CopyTexture(HololensVideo.material.GetTexture("_UPlane"), UPlane);
        Graphics.CopyTexture(HololensVideo.material.GetTexture("_VPlane"), VPlane);
        HololensVideo.gameObject.GetComponent<FadeMesh>().HideImmediately();
        HololensVideo.enabled = false;

        HololensFreezedVideo.material.SetTexture("_YPlane", YPlane);
        HololensFreezedVideo.material.SetTexture("_UPlane", UPlane);
        HololensFreezedVideo.material.SetTexture("_VPlane", VPlane);
        HololensFreezedVideo.enabled = true;
        HololensFreezedVideo.gameObject.GetComponent<FadeMesh>().ShowImmediately();

        isFreezed = true;
        logger.LogStartFreezeHololensView();

    }

    //public void SetNotFreezed()
    //{
    //    isFreezed = false;
    //}

    public void Unfreeze()
    {
        Debug.Log("UNFREEZE");

        if (hololensPlayer == null)
        {
            Debug.LogError("no hololens in scene");
            return;
        }
        viewManager.DisplayARHeadsetViewImmediately();
        isFreezed = false;

        
        
        //virtualPosRecorder.FlushInvisibleCamera();



        //virtualPosRecorder.DeleteInvisibleCamera(cameraName);

        //HololensFreezedVideo.gameObject.GetComponent<FadeMesh>().Hide();

        //Destroy(hololensScreenShot);
        //Destroy(YPlane);
        //Destroy(UPlane);
        //Destroy(VPlane);

        //isFreezed = false;
    }

    // solution 1
    //IEnumerator RecordFrame()
    //{
    //    yield return new WaitForEndOfFrame();
    //    hololensScreenShot = ScreenCapture.CaptureScreenshotAsTexture();
    //}
}
#endif