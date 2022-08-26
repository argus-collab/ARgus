#if !UNITY_WSA
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirtualViewPositionRecorder : MonoBehaviour
{
    public LogManager logger;
    public CustomClientNetworkManager unet;

    public Camera mainCamera;
    public GameObject virtualCameraRepresentation;
    public GameObject invisibleVirtualCameraRepresentation;
    //public GameObject MiniatureViewRoot;
    public DisplayViewInUI miniatures;
    public Material viewMaterial;

    public ChangeViewCinemachine viewChanger;
    private int indexView = 0;

    private List<Vector3> cameraPosition;
    private List<Quaternion> cameraRotation;
    private List<GameObject> virtualCameraRepresentations;


    void Start()
    {
        cameraPosition = new List<Vector3>();
        cameraRotation = new List<Quaternion>();
        virtualCameraRepresentations = new List<GameObject>();
    }

    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.A))
        //{
        //    Debug.Log("add a virtual camera object");
        //    RegisterVirtualCameraPosition();
        //}

    }

    public void FlushInvisibleCamera()
    {
        for (int i = 0; i < virtualCameraRepresentations.Count; ++i)
            Destroy(virtualCameraRepresentations[i]);
        virtualCameraRepresentations.Clear();
    }

    public GameObject GetLastVirtualCameraRepresentation()
    {
        if (virtualCameraRepresentations.Count > 0)
            return virtualCameraRepresentations[virtualCameraRepresentations.Count - 1];
        else
            return null;
    }

    // actually more main camera position (can be other than the virtual camera)
    public void RegisterVirtualCameraPosition()
    {
        logger.LogCameraPositionRecord();
        RegisterCameraPosition(mainCamera.transform.position, mainCamera.transform.rotation);
    }

    public string RegisterInvisibleCameraPosition(Vector3 position, Quaternion rotation)
    {
        cameraPosition.Add(position);
        cameraRotation.Add(rotation);

        // add virtual camera GO
        GameObject vCamGO = Instantiate(invisibleVirtualCameraRepresentation, position, rotation);
        virtualCameraRepresentations.Add(vCamGO);
        vCamGO.name = "InvisibleVirtualCamera-" + cameraPosition.Count;
        vCamGO.transform.parent = transform;
        vCamGO.transform.position = position;
        vCamGO.transform.rotation = rotation;

        vCamGO.GetComponentInChildren<Camera>().name = "Camera" + virtualCameraRepresentations.Count;
        vCamGO.GetComponentInChildren<Camera>().fieldOfView = mainCamera.fieldOfView;
        vCamGO.GetComponentInChildren<Camera>().nearClipPlane = mainCamera.nearClipPlane;
        vCamGO.GetComponentInChildren<Camera>().farClipPlane = mainCamera.farClipPlane;

        miniatures.goC.Add(vCamGO);

        RedirectViewFromCamera red = vCamGO.transform.GetComponentInChildren<RedirectViewFromCamera>();
        miniatures.inputViewC.Add(red);

        // unet
        unet.AskForPrefabInstantiate(invisibleVirtualCameraRepresentation.name, "InvisibleVirtualCameraRepresentation-" + cameraPosition.Count, position, rotation);

        return "InvisibleVirtualCamera-" + cameraPosition.Count;
    }

    public void DeleteInvisibleCamera(string cameraName)
    {
        // TODO
    }

    public void RegisterCameraPosition(Vector3 position, Quaternion rotation)
    {

        cameraPosition.Add(position);
        cameraRotation.Add(rotation);

        // add miniature


        //GameObject newVCamMin = new GameObject("VirtualCameraMiniature"+cameraPosition.Count);
        //newVCamMin.transform.parent = MiniatureViewRoot.transform;
        //OrientMiniature orComp = newVCamMin.AddComponent<OrientMiniature>();
        //orComp.mainCamera = mainCamera;
        //orComp.offset = 0.7f;

        //GameObject view = GameObject.CreatePrimitive(PrimitiveType.Plane);
        //view.transform.parent = newVCamMin.transform;
        //view.transform.rotation = Quaternion.Euler(-90, 0, 0);
        //view.transform.localScale = new Vector3(-0.06f, 0.06f, -0.06f);
        //MeshRenderer meshRend = view.GetComponent<MeshRenderer>();
        //meshRend.material = viewMaterial;






        // add virtual camera GO
        GameObject vCamGO = Instantiate(virtualCameraRepresentation, position, rotation);
        virtualCameraRepresentations.Add(vCamGO);
        vCamGO.name = "VirtualCamera-" + cameraPosition.Count;
        vCamGO.transform.parent = transform;
        vCamGO.transform.position = position;
        vCamGO.transform.rotation = rotation;

        vCamGO.GetComponentInChildren<Camera>().name = "Camera" + virtualCameraRepresentations.Count;

        miniatures.goC.Add(vCamGO);

        RedirectViewFromCamera red = vCamGO.transform.GetComponentInChildren<RedirectViewFromCamera>();
        miniatures.inputViewC.Add(red);




        // unet
        unet.AskForPrefabInstantiate(virtualCameraRepresentation.name, "VirtualCameraRepresentation-" + cameraPosition.Count, position, rotation);
        //unet.AskForPrefabSpawn(virtualCameraRepresentation.name, "VirtualCameraRepresentation-" + cameraPosition.Count, mainCamera.transform.position, mainCamera.transform.rotation);

        //red.outputMaterialInstance = meshRend;
        //orComp.go = vCamGO;
    }

    public void GoToNextCameraView()
    {
        if (virtualCameraRepresentations.Count > 0)
        {
            indexView = (indexView + 1) % virtualCameraRepresentations.Count;
            viewChanger.DisplayCameraView(virtualCameraRepresentations[indexView].GetComponentInChildren<Camera>());
        }
    }




}
#endif