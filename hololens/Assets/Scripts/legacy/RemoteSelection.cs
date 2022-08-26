using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoteSelection : MonoBehaviour
{
    public Camera ARCamera;
    public Material selectedMaterial;

    private bool doSelectAtUpdate = false;
    private float xSelect;
    private float ySelect;

    private List<GameObject> selected;
    private List<Material> initialMaterial;

    private Vector3 lastInputPosition;

    //----
    int width;
    int height;
    private Texture2D text;

    void Start()
    {
        selected = new List<GameObject>();
        initialMaterial = new List<Material>();

        width = ARCamera.pixelWidth;
        height = ARCamera.pixelHeight;

        text = new Texture2D(1, 1);
        Color transparentColor = new Color(0, 0, 0, 0);
        text.SetPixel(0, 0, transparentColor);
        text.Apply();
        text.Resize(width, height);
    }

    void Update()
    {
        // debug
        if (Input.GetButton("Fire1"))
        {
            Debug.Log(Input.mousePosition.x + ", " + Input.mousePosition.y);
            //lastInputPosition = Input.mousePosition;
            //Select(Input.mousePosition.x, Input.mousePosition.y);
        }

        // needed because WebRTC works in another process and can't directly call Select
        if (doSelectAtUpdate)
        {
            doSelectAtUpdate = false;
            Select(xSelect, ySelect);
        }
    }

    public void AskForSelect(float x, float y)
    {
        doSelectAtUpdate = true;
        xSelect = x;
        ySelect = y;
    }


    //private void OnGUI()
    //{
    //    GUI.DrawTexture(new Rect(0, 0, width, height), text);
    //}

    public void Select(float x, float y)
    {

        //text = RemoteSelectionUI.DrawCircle(text, Color.red, (int) x, (int) y, 50);
        //Debug.Log("Select with : " + x + ", " + y);

        RaycastHit hit;
        Ray ray = ARCamera.ScreenPointToRay(new Vector3(x, y, 0f));

        if (Physics.SphereCast(ray, /*circleSize*/ 0.03f, out hit, Mathf.Infinity))
        {
            if (selected.Contains(hit.collider.gameObject))
            {
                UnselectGO(hit.collider.gameObject);
            }
            else
            {
                SelectGO(hit.collider.gameObject);
            }
        }
        else
        {
            Debug.Log("no hit");
        }
    }

    public List<GameObject> GetSelectedGameObjects()
    {
        return selected;
    }

    public void SelectGO(GameObject go)
    {
        if (!selected.Contains(go))
        {
            selected.Add(go);
            initialMaterial.Add(go.GetComponent<Renderer>().material);

            go.GetComponent<Renderer>().material = selectedMaterial;
        }
        // ELSE : go already in list
    }

    public void UnselectGO(GameObject go)
    {
        if (selected.Contains(go))
        {
            int i = selected.IndexOf(go);
            go.GetComponent<Renderer>().material = initialMaterial[i];

            selected.Remove(go);
            initialMaterial.RemoveAt(i);
        }
        // ELSE : go is not already selected
    }
}
