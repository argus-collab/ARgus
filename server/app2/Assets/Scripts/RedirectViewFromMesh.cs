using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedirectViewFromMesh : MonoBehaviour
{
    public MeshRenderer inputMaterialInstance;
    public MeshRenderer outputMaterialInstance;

    void Update()
    {
        outputMaterialInstance.material = inputMaterialInstance.material;
    }
}
