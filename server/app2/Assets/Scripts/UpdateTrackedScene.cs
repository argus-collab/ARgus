using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateTrackedScene : MonoBehaviour
{
    void Update()
    {
        for(int i = 0; i < transform.childCount; ++i)
        {
            Transform targetScenePart = transform.GetChild(i);

            GameObject clientScenePart = GameObject.Find(targetScenePart.name);// + "Client");
            if(clientScenePart==null)
            {
                Debug.LogError("check scene integrity for " + targetScenePart.name);// + "Client");
                return;
            }

            targetScenePart.localPosition = clientScenePart.transform.localPosition;
            targetScenePart.localRotation = clientScenePart.transform.localRotation;
        }
    }
}
