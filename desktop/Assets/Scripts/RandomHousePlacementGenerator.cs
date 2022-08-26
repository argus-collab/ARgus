using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomHousePlacementGenerator : MonoBehaviour
{
    [Header("Colors")]
    public Material[] landmarks;

    [Header("Room 1")]
    public MeshRenderer[] room1landmarks;

    [Header("Room 2")]
    public MeshRenderer[] room2landmarks;

    [Header("Room 3")]
    public MeshRenderer[] room3landmarks;

    [Header("Room 4")]
    public MeshRenderer[] room4landmarks;
    

    private System.Random _random = new System.Random();

    void Start()
    {
        if (!Check()) return;
    }

    private void Awake()
    {
        Init();
        HideLandmarks();
    }

    private bool Check()
    {
        if (landmarks.Length != 4
            || room1landmarks.Length != 4
            || room2landmarks.Length != 4
            || room3landmarks.Length != 4
            || room4landmarks.Length != 4)
        {
            Debug.LogError("[RandomHousePlacementGenerator] array size error");
            return false;
        }
        else
            return true;
    }

    public void HideLandmarks()
    {
        LandmarksVisibility(false);
    }

    public void ShowLandmarks()
    {
        LandmarksVisibility(true);
    }

    private void LandmarksVisibility(bool visible)
    {
        for (int i = 0; i < 4; ++i)
        {
            room1landmarks[i].gameObject.SetActive(visible);
            room2landmarks[i].gameObject.SetActive(visible);
            room3landmarks[i].gameObject.SetActive(visible);
            room4landmarks[i].gameObject.SetActive(visible);
        }
    }

    private void Init()
    {
        InitRoom1();
        InitRoom2();
        InitRoom3();
        InitRoom4();
    }

    private void InitRoom1()
    {
        int[] index = { 0, 1, 2, 3 };
        index = Shuffle<int>(index);

        for (int i = 0; i < 4; ++i)
            room1landmarks[i].material = landmarks[index[i]];
    }

    private void InitRoom2()
    {
        int[] index = { 0, 1, 2, 3 };
        index = Shuffle<int>(index);

        for (int i = 0; i < 4; ++i)
            room2landmarks[i].material = landmarks[index[i]];
    }

    private void InitRoom3()
    {
        int[] index = { 0, 1, 2, 3 };
        index = Shuffle<int>(index);

        for (int i = 0; i < 4; ++i)
            room3landmarks[i].material = landmarks[index[i]];
    }

    private void InitRoom4()
    {
        int[] index = { 0, 1, 2, 3 };
        index = Shuffle<int>(index);

        for (int i = 0; i < 4; ++i)
            room4landmarks[i].material = landmarks[index[i]];
    }

    public T[] Shuffle<T>(T[] array)
    {
        var random = _random;
        for (int i = array.Length; i > 1; i--)
        {
            int j = random.Next(i); 
                                    
            T tmp = array[j];
            array[j] = array[i - 1];
            array[i - 1] = tmp;
        }
        return array;
    }

    void Update()
    {
        if (!Check()) return;

    }
}
