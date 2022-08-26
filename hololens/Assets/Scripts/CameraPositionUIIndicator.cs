using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraPositionUIIndicator : MonoBehaviour
{
    enum PositionState
    {
        RightBorder,
        LeftBorder,
        BottomBorder,
        UpBorder,
        Hidden
    };

    public Camera mainCamera;
    public RectTransform rt;
    public Sprite arrowImage;
    public Sprite hololensImage;
    public Sprite kinectImage;
    public Sprite cameraImage;

    private List<Camera> cameras;
    private List<Image> representations;
    private List<Image> icons;
    private List<Image> arrows;
    private List<Text> texts;
    private List<PositionState> positionsStates;

    public int heightScreen = 1080;
    public int widthScreen = 1920;
    public int heightDisplayArea = 900;
    public int widthDisplayArea = 1400;



    private void Start()
    {
        cameras = new List<Camera>();
        representations = new List<Image>();
        icons = new List<Image>();
        arrows = new List<Image>();
        texts = new List<Text>();

        positionsStates = new List<PositionState>();
    }

    void Update()
    {
        UpdateCameraList();
        UpdateRepresentations();
    }

    void UpdateCameraList()
    {
        Camera[] camInScene = FindObjectsOfType<Camera>();
        List<Camera> camInSceneList = new List<Camera>();

        for (int i = 0; i < camInScene.Length; ++i)
            camInSceneList.Add(camInScene[i]);

        for (int i = 0; i < camInScene.Length; ++i)
        {
            if (camInScene[i].GetComponent<IsViewPoint>() == null) continue;
            if (!cameras.Contains(camInScene[i]) && camInScene[i] != mainCamera
                
                // debug
                && (camInScene[i].name == "CameraKinect" || camInScene[i].name == "CameraHololens")

                )
            {
                cameras.Add(camInScene[i]);
                
                GameObject rep = new GameObject("cam" + camInScene[i].GetInstanceID() + "-" + camInScene[i].name);
                rep.transform.parent = gameObject.transform;

                GameObject arrow = new GameObject("arrow");
                arrow.transform.parent = rep.transform;

                GameObject icon = new GameObject("icon");
                icon.transform.parent = rep.transform;

                GameObject text = new GameObject("text");
                text.transform.parent = rep.transform;


                Image repImg = rep.AddComponent<Image>();
                repImg.color = new Color(1, 1, 1, 0);
                repImg.gameObject.GetComponent<RectTransform>().anchorMin = Vector2.zero;
                repImg.gameObject.GetComponent<RectTransform>().anchorMax = Vector2.zero;
                representations.Add(repImg);

                Image arrowImg = arrow.AddComponent<Image>();
                arrowImg.sprite = arrowImage;
                arrowImg.rectTransform.localScale = new Vector3(0.8f, 1f, 1f);
                arrows.Add(arrowImg);

                Text desc = text.AddComponent<Text>();
                Image iconImg = icon.AddComponent<Image>();

                if (camInScene[i].name == "CameraKinect")
                {
                    iconImg.sprite = kinectImage;
                    desc.text = "Fixed camera";
                }
                else if (camInScene[i].name == "CameraHololens")
                {
                    iconImg.sprite = hololensImage;
                    desc.text = "AR headset";
                }
                else
                {
                    iconImg.sprite = cameraImage;
                    desc.text = "Camera";
                }
                iconImg.rectTransform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                desc.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                desc.fontSize = 20;

                icons.Add(iconImg);
                texts.Add(desc);
            }
        }

        for (int i = 0; i < cameras.Count; ++i)
            if (!camInSceneList.Contains(cameras[i]))
            {
                cameras.RemoveAt(i);
                representations.RemoveAt(i);
                icons.RemoveAt(i);
                texts.RemoveAt(i);
                arrows.RemoveAt(i);
            }
    }

    float Shrink(float x, float min, float max)
    {
        float res = x;
        
        if (res > max)
            res = max;
        if (res < min)
            res = min;

        return res;
    }

    void UpdateRepresentations()
    {
        for(int i = 0; i < representations.Count; ++i)
            positionsStates.Add(PositionState.Hidden);

        for (int i = 0; i < representations.Count; ++i)
        {
            Vector3 pos = mainCamera.WorldToScreenPoint(cameras[i].transform.position);
            //Debug.Log(cameras[i].gameObject.name + " pos : " + pos);

            float x = 0;
            float y = 0;

            float offsetIconX = 0;
            float offsetIconY = 0;

            float offsetTextX = 0;
            float offsetTextY = 0;

            float globalOffset = 100;

            if ((pos.x < 0 && pos.y > 0 && pos.y < heightScreen) 
                || (pos.x < 0 && pos.y < 0 && Mathf.Abs(pos.x) > Mathf.Abs(pos.y))
                || (pos.x < 0 && pos.y > heightScreen && Mathf.Abs(pos.x) > Mathf.Abs(pos.y - heightScreen)))
            {
                // déplacement sur l'axe vertical gauche
                Show(i);
                positionsStates[i] = PositionState.LeftBorder;

                x = 0;
                y = Shrink(pos.y - (heightScreen - heightDisplayArea) / 2, 0, heightDisplayArea);

                offsetIconX = globalOffset;
                offsetIconY = 0;

                offsetTextX = globalOffset;
                offsetTextY = globalOffset;
            }
            else if ((pos.x > widthScreen && pos.y > 0 && pos.y < heightScreen)
                || (pos.x > widthScreen && pos.y < 0 && Mathf.Abs(pos.x - widthScreen) > Mathf.Abs(pos.y))
                || (pos.x > widthScreen && pos.y > heightScreen && Mathf.Abs(pos.x - widthScreen) > Mathf.Abs(pos.y - heightScreen)))
            {
                // déplacement sur l'axe vertical droit
                Show(i);
                positionsStates[i] = PositionState.RightBorder;

                x = widthDisplayArea;
                y = Shrink(pos.y - (heightScreen - heightDisplayArea) / 2, 0, heightDisplayArea);


                offsetIconX = -globalOffset;
                offsetIconY = 0;

                offsetTextX = -globalOffset;
                offsetTextY = -globalOffset;
            }
            else if ((pos.y < 0 && pos.x > 0 && pos.x < widthScreen)
                || (pos.y < 0 && pos.x < 0 && Mathf.Abs(pos.y) > Mathf.Abs(pos.x))
                || (pos.y < 0 && pos.x > widthScreen && Mathf.Abs(pos.y) > Mathf.Abs(pos.x - widthScreen)))
            {
                // déplacement sur l'axe horizontal bas
                Show(i);
                positionsStates[i] = PositionState.BottomBorder;

                x = Shrink(pos.x - (widthScreen - widthDisplayArea) / 2, 0, widthDisplayArea);
                y = 0;

                offsetIconX = 0;
                offsetIconY = globalOffset;

                offsetTextX = globalOffset;
                offsetTextY = globalOffset;
            }
            else if ((pos.y > heightScreen && pos.x > 0 && pos.x < widthScreen)
                || (pos.y > heightScreen && pos.x < 0 && Mathf.Abs(pos.y - heightScreen) > Mathf.Abs(pos.x))
                || (pos.y > heightScreen && pos.x > widthScreen && Mathf.Abs(pos.y - heightScreen) > Mathf.Abs(pos.x - widthScreen)))
            {
                // déplacement sur l'axe horizontal haut
                Show(i);
                positionsStates[i] = PositionState.UpBorder;

                x = Shrink(pos.x - (widthScreen - widthDisplayArea) / 2, 0, widthDisplayArea);
                y = heightDisplayArea;

                offsetIconX = 0;
                offsetIconY = -globalOffset;

                offsetTextX = -globalOffset;
                offsetTextY = -globalOffset;
            }
            else
            {
                // hide
                Hide(i);
                positionsStates[i] = PositionState.Hidden;
            }

            Vector2 dir = new Vector2(pos.x - x - (widthScreen - widthDisplayArea) / 2, pos.y - y - (heightScreen - heightDisplayArea) / 2);
            float rot = Mathf.Atan2(dir.y, dir.x);

            representations[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(x, y);
            
            icons[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(offsetIconX, offsetIconY);
            texts[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(offsetTextX, offsetTextY);

            arrows[i].GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, Mathf.Rad2Deg * rot);
        }

        // gestion des superpositions
        float offsetSuperposition = 200;
        for (int i = 0; i < representations.Count; ++i)
        {
            for (int j = i + 1; j < representations.Count; ++j)
            {
                if (CloseEnough(representations[j].GetComponent<RectTransform>().anchoredPosition,
                    representations[i].GetComponent<RectTransform>().anchoredPosition, offsetSuperposition))
                {
                    if (positionsStates[j] == PositionState.BottomBorder 
                        || positionsStates[j] == PositionState.UpBorder)
                    {
                        if (representations[j].GetComponent<RectTransform>().anchoredPosition.x > widthDisplayArea / 2)
                            representations[j].GetComponent<RectTransform>().anchoredPosition += new Vector2(-offsetSuperposition, 0);
                        else
                            representations[j].GetComponent<RectTransform>().anchoredPosition += new Vector2(offsetSuperposition, 0);
                    }
                    else if (positionsStates[j] == PositionState.RightBorder 
                        || positionsStates[j] == PositionState.LeftBorder)
                    {
                        if (representations[j].GetComponent<RectTransform>().anchoredPosition.y > heightDisplayArea / 2)
                            representations[j].GetComponent<RectTransform>().anchoredPosition += new Vector2(0, -offsetSuperposition);
                        else
                            representations[j].GetComponent<RectTransform>().anchoredPosition += new Vector2(0, offsetSuperposition);
                    }
                }
            }
        }


    }

    bool CloseEnough(Vector2 a, Vector2 b, float tol)
    {
        return ((a - b).magnitude < tol);
    }

    void Hide(int i)
    {
        representations[i].gameObject.SetActive(false);
        icons[i].gameObject.SetActive(false);
        texts[i].gameObject.SetActive(false);
        arrows[i].gameObject.SetActive(false);
    }

    void Show(int i)
    {
        representations[i].gameObject.SetActive(true);
        icons[i].gameObject.SetActive(true);
        texts[i].gameObject.SetActive(true);
        arrows[i].gameObject.SetActive(true);
    }
}

