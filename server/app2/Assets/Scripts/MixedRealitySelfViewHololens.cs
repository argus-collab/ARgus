//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.XR;
//using UnityEngine.XR.LegacyInputHelpers;
//using UnityEngine.XR.Management;
//using System;
//using Unity.Collections;
//using Unity.Collections.LowLevel.Unsafe;
//using UnityEngine.XR.ARFoundation;
//using UnityEngine.XR.ARSubsystems;

//public class MixedRealitySelfViewHololens : MonoBehaviour
//{
//    public Renderer visualizer;
//    Texture2D m_Texture;
//    public ARCameraManager cameraManager;

//    private void Start()
//    {
//        m_Texture = new Texture2D(10, 10);
//        for (int i = 0; i < 10; ++i)
//            for (int j = 0; j < 10; ++j)
//                m_Texture.SetPixel(i, j, Color.white);
//        m_Texture.Apply();
//    }

//    void OnEnable()
//    {
//        cameraManager.frameReceived += OnCameraFrameReceived;
//    }

//    void OnDisable()
//    {
//        cameraManager.frameReceived -= OnCameraFrameReceived;
//    }

//    private void Update()
//    {
//        //cameraManager.frameReceived += OnCameraFrameReceived;
//        //ManageFrame();
//        if(m_Texture != null)
//            visualizer.material.mainTexture = m_Texture;
//    }

//    unsafe void OnCameraFrameReceived(ARCameraFrameEventArgs eventArgs)
//    {
//        m_Texture = new Texture2D(10, 10);
//        for (int i = 0; i < 10; ++i)
//            for (int j = 0; j < 10; ++j)
//                m_Texture.SetPixel(i, j, Color.blue);
//        m_Texture.Apply();

//        if (!cameraManager.TryAcquireLatestCpuImage(out XRCpuImage image))
//            return;

//        m_Texture = new Texture2D(10,10);
//        for (int i = 0; i < 10; ++i)
//            for(int j=0;j<10;++j)
//                m_Texture.SetPixel(i,j,Color.green);
//        m_Texture.Apply();

//        var conversionParams = new XRCpuImage.ConversionParams
//        {
//            // Get the entire image.
//            inputRect = new RectInt(0, 0, image.width, image.height),

//            // Downsample by 2.
//            outputDimensions = new Vector2Int(image.width / 2, image.height / 2),

//            // Choose RGBA format.
//            outputFormat = TextureFormat.RGBA32,

//            // Flip across the vertical axis (mirror image).
//            transformation = XRCpuImage.Transformation.MirrorY
//        };

//        // See how many bytes you need to store the final image.
//        int size = image.GetConvertedDataSize(conversionParams);

//        // Allocate a buffer to store the image.
//        var buffer = new NativeArray<byte>(size, Allocator.Temp);

//        // Extract the image data
//        image.Convert(conversionParams, new IntPtr(buffer.GetUnsafePtr()), buffer.Length);

//        // The image was converted to RGBA32 format and written into the provided buffer
//        // so you can dispose of the XRCpuImage. You must do this or it will leak resources.
//        image.Dispose();

//        // At this point, you can process the image, pass it to a computer vision algorithm, etc.
//        // In this example, you apply it to a texture to visualize it.

//        // You've got the data; let's put it into a texture so you can visualize it.
//        m_Texture = new Texture2D(
//            conversionParams.outputDimensions.x,
//            conversionParams.outputDimensions.y,
//            conversionParams.outputFormat,
//            false);

//        m_Texture.LoadRawTextureData(buffer);
//        m_Texture.Apply();

//        // Done with your temporary data, so you can dispose it.
//        buffer.Dispose();
//    }

//    //unsafe void ManageFrame()
//    //{
//    //    XRCameraImage image;
//    //    if (!cameraManager.TryGetLatestImage(out image))
//    //        return;

//    //    Debug.Log("image acquired");

//    //    var conversionParams = new XRCameraImageConversionParams
//    //    {
//    //        // Get the entire image
//    //        inputRect = new RectInt(0, 0, image.width, image.height),

//    //        // Downsample by 2
//    //        outputDimensions = new Vector2Int(image.width / 2, image.height / 2),

//    //        // Choose RGBA format
//    //        outputFormat = TextureFormat.RGBA32,

//    //        // Flip across the vertical axis (mirror image)
//    //        transformation = CameraImageTransformation.MirrorY
//    //    };

//    //    // See how many bytes we need to store the final image.
//    //    int size = image.GetConvertedDataSize(conversionParams);

//    //    // Allocate a buffer to store the image
//    //    var buffer = new NativeArray<byte>(size, Allocator.Temp);

//    //    // Extract the image data
//    //    image.Convert(conversionParams, new IntPtr(buffer.GetUnsafePtr()), buffer.Length);

//    //    // The image was converted to RGBA32 format and written into the provided buffer
//    //    // so we can dispose of the CameraImage. We must do this or it will leak resources.
//    //    image.Dispose();

//    //    // At this point, we could process the image, pass it to a computer vision algorithm, etc.
//    //    // In this example, we'll just apply it to a texture to visualize it.

//    //    // We've got the data; let's put it into a texture so we can visualize it.
//    //    m_Texture = new Texture2D(
//    //        conversionParams.outputDimensions.x,
//    //        conversionParams.outputDimensions.y,
//    //        conversionParams.outputFormat,
//    //        false);

//    //    m_Texture.LoadRawTextureData(buffer);
//    //    m_Texture.Apply();

//    //    // Done with our temporary data
//    //    buffer.Dispose();
    
//    //}
//}
