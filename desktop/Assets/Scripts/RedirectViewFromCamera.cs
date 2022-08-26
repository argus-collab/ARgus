using UnityEngine;

public class RedirectViewFromCamera : MonoBehaviour
{
    public Camera inputCamera;
    public MeshRenderer outputMaterialInstance;

    private RenderTexture renderTexture;

    void Start()
    {

        renderTexture = new RenderTexture(inputCamera.pixelWidth, inputCamera.pixelHeight, 24);
        renderTexture.Create();
        renderTexture.name = "Camera render texture";

        RenderTexture.active = renderTexture;
    }

    void OnPostRender()
    {
        if (inputCamera != null && outputMaterialInstance != null)
        {
            inputCamera.targetTexture = renderTexture;
            //inputCamera.Render();
            outputMaterialInstance.material.SetTexture("_MainTex", renderTexture);
        }

    }
}
