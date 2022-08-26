using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnslaveToMainCamera : MonoBehaviour
{
    void Update()
    {
        transform.position = Camera.main.transform.position;    
        transform.rotation = Camera.main.transform.rotation;
    }
}
