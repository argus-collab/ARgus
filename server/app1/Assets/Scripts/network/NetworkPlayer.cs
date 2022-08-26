using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Networking;

public class NetworkPlayer : NetworkBehaviour
{
    public string mainCameraName;

    private Camera playerCamera;

    private Vector3 transOffset; 
    private Quaternion rotOffset; 

    public void SetOffset(Vector3 v, Quaternion q)
    {
        transOffset = v;
        rotOffset = q;
    }

    // Start is called before the first frame update
    void Start()
    {
        if(GameObject.Find(mainCameraName) != null)
            playerCamera = GameObject.Find(mainCameraName).GetComponent<Camera>();

        // hide camera if client
        //if (isClient)
        //    transform.Find("PlayerServerAppearance").gameObject.SetActive(false);

        transOffset = Vector3.zero;
        rotOffset = Quaternion.identity;
    }

    // Update is called once per frame
    void Update()
    {
        if (playerCamera != null)
        {
            gameObject.transform.localPosition = transOffset + playerCamera.transform.position;
            gameObject.transform.localRotation = rotOffset * playerCamera.transform.rotation;
        }
        else
        {
            GameObject go = GameObject.Find(mainCameraName);
            if(go!=null)
                playerCamera = go.GetComponent<Camera>();
        }
    }
}
