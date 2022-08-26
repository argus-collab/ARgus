using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

//[RequireComponent(typeof(Collider))]
public class Anchor : MonoBehaviour
{
    public GameObject anchor;
    public bool hasParent = false;
    public float stickyRadius = 0.06f;

    private List<GameObject> emplacements;
    private GrabbingManagement grabMan;

    private void Start()
    {
        grabMan = gameObject.GetComponent<GrabbingManagement>();

        emplacements = new List<GameObject>();

        UpdateEmplacements();
    }

    void UpdateEmplacements()
    {
        emplacements.Clear();
        // get emplacements colliders
        UnityEngine.Object[] empl = Resources.FindObjectsOfTypeAll(typeof(IsStickyEmplacement));
        for (int i = 0; i < empl.Length; ++i)
           emplacements.Add(((IsStickyEmplacement)empl[i]).gameObject);
    }

    private void Update()
    {
        UpdateEmplacements();

        if (grabMan.GetGrabbingState()) return;

        for (int i = 0; i < emplacements.Count; ++i)
        {
            if ((anchor.transform.position - emplacements[i].transform.position).magnitude < stickyRadius)
            {
                Vector3 finalRotation = Vector3.zero;


                // rotation X
                float initialRotationX = transform.localRotation.eulerAngles.x;

                if (initialRotationX < 90 && initialRotationX >= 270)
                    finalRotation.x = 0;
                else if (initialRotationX < 270 && initialRotationX >= 90)
                    finalRotation.x = 180;


                // rotation Z
                float initialRotationZ = transform.localRotation.eulerAngles.z;

                if (initialRotationZ < 90 && initialRotationZ >= 270)
                    finalRotation.z = 0;
                else if (initialRotationZ < 270 && initialRotationZ >= 90)
                    finalRotation.z = 180;


                // rotation Y
                float initialRotationY = transform.localRotation.eulerAngles.y;

                if (initialRotationY < 45 && initialRotationY >= 315)
                    finalRotation.y = 0;
                else if (initialRotationY < 135 && initialRotationY >= 45)
                    finalRotation.y = 90;
                else if (initialRotationY < 225 && initialRotationY >= 135)
                    finalRotation.y = 180;
                else if (initialRotationY < 315 && initialRotationY >= 225)
                    finalRotation.y = 270;


                transform.localRotation = Quaternion.Euler(finalRotation);



                // position
                Vector3 translation = emplacements[i].transform.position - anchor.transform.position;
                transform.position += translation;
            }

        }
    }
}
