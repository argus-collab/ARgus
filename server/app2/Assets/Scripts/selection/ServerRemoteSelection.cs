using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerRemoteSelection : MonoBehaviour
{
    public WebRTCNetworkCommunication network;

    private GameObject selected;
    private Material initialMaterial;
    public Material selectedMaterial;

    public Camera mainCamera;
    int width;
    int height;
    private Texture2D text;
    /*

    void Start()
    {

        width = mainCamera.pixelWidth;
        height = mainCamera.pixelHeight;

        text = new Texture2D(1, 1);
        Color transparentColor = new Color(0, 0, 0, 0);
        text.SetPixel(0, 0, transparentColor);
        text.Apply();
        text.Resize(width, height);
    }

    void Update()
    {
        List<WebRTCMessage> messages = network.IsThereAnyMessagesForMe("ServerRemoteSelection");
        for (int i = 0; i < messages.Count; ++i)
        {
            if (messages[i].action == "select")
                Select(messages[i]);

            if (messages[i].action == "grasp")
                Grasp();

            if (messages[i].action == "release")
                Release();
        }
    }

    private void Select(WebRTCMessage msg)
    {
        Debug.Log("select");

        WebRTCMessageData2DPosition msgPrecise = (WebRTCMessageData2DPosition)msg;

        RaycastHit hit;
        Ray ray = mainCamera.ScreenPointToRay(new Vector3(msgPrecise.x, msgPrecise.y, 0f));

        if (Physics.SphereCast(ray,  0.03f, out hit, Mathf.Infinity))
        {
            if(hit.collider.gameObject == selected)
            {
                selected.GetComponent<Renderer>().material = initialMaterial;
                selected = null;
            }
            else
            {
                selected = hit.collider.gameObject;
                initialMaterial = selected.GetComponent<Renderer>().material;
                selected.GetComponent<Renderer>().material = selectedMaterial;
            }
        }
        else
        {
            Debug.Log("no hit");
        }
    }

    private void Grasp()
    {
        if (selected != null)
        {
            // scale management
            Vector3 scale = new Vector3(
                selected.transform.localScale.x * selected.transform.parent.localScale.x,
                selected.transform.localScale.y * selected.transform.parent.localScale.y,
                selected.transform.localScale.z * selected.transform.parent.localScale.z);

            //selected.SetActive(false);
            network.SendMessage("ClientRemoteSelection", "instantiate", selected.name, selected.transform.position, selected.transform.rotation, scale);
        }
        else
            Debug.Log("no selected object to grasp !");
    }

    private void Release()
    {
        //selected.SetActive(true);
        selected.GetComponent<Renderer>().material = initialMaterial;
        selected = null;
    }
*/
}
