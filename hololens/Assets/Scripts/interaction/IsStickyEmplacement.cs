//using System.Collections;
//using System.Collections.Generic;
using UnityEngine;
//using Microsoft.MixedReality.Toolkit;
//using System;
//using Microsoft.MixedReality.Toolkit.UI;

//[RequireComponent(typeof(Collider))]
public class IsStickyEmplacement : MonoBehaviour
{
    //public float stickyRadius = 0.7f;
    //private List<Collider> anchorsColliders;
    //private Collider selfCollider;

    //public float refreshFrequency = 10;
    //private float timeStamp;

    //void Start()
    //{
    //    selfCollider = GetComponent<Collider>();
    //    anchorsColliders = new List<Collider>();

    //    UnityEngine.Object[] anchors = Resources.FindObjectsOfTypeAll(typeof(Anchors));

    //    for(int i = 0; i < anchors.Length; ++i)
    //    {
    //        Anchors anchor = (Anchors)anchors[i];
    //        Collider collider = anchor.gameObject.GetComponent<Collider>();
            
    //        anchorsColliders.Add(collider);
    //    }

    //    //timeStamp = Time.time;
    //}

    //private void Update()
    //{
    //    //if (Time.time - timeStamp < 1 / refreshFrequency)
    //    //    return;

    //    //timeStamp = Time.time;

    //    for (int j=0; j < anchorsColliders.Count; ++j)
    //    {
    //        GrabbingManagement grabbing = anchorsColliders[j].gameObject.GetComponent<GrabbingManagement>();

    //        if (grabbing != null
    //            && !grabbing.GetGrabbingState()
    //            && selfCollider.bounds.Intersects(anchorsColliders[j].bounds))
    //        {
    //            //Debug.Log(anchorsColliders[j].transform.name + " collide " + gameObject.transform.name);

    //            Anchors anchors = anchorsColliders[j].gameObject.GetComponent<Anchors>();

    //            if (anchors != null)
    //            {
    //                List<GameObject> anchorsList = anchors.GetAnchors();
    //                for (int i = 0; /*!anchors.IsAnchored() &&*/ i < anchorsList.Count; ++i)
    //                {
    //                    if ((anchorsList[i].transform.position - transform.position).magnitude < stickyRadius)
    //                    {
    //                        GameObject part;

    //                        if (anchors.hasParent)
    //                            part = anchors.transform.parent.gameObject;
    //                        else
    //                            part = anchors.transform.gameObject;

    //                        // rotation
    //                        float initialRotationY = part.transform.rotation.eulerAngles.y;
    //                        Vector3 finalRotation = Vector3.zero;
    //                        if (initialRotationY < 45 && initialRotationY >= 315)
    //                            finalRotation.y = 0;
    //                        else if (initialRotationY < 135 && initialRotationY >= 45)
    //                            finalRotation.y = 90;
    //                        else if (initialRotationY < 225 && initialRotationY >= 135)
    //                            finalRotation.y = 180;
    //                        else if (initialRotationY < 315 && initialRotationY >= 225)
    //                            finalRotation.y = 270;

    //                        part.transform.localRotation = Quaternion.Euler(finalRotation);

    //                        // position
    //                        Vector3 translation = transform.position - anchorsList[i].transform.position;
    //                        part.transform.position += translation;

    //                        //anchors.SetAnchored(true);
    //                    }
    //                }
    //            }
    //        }
    //        //else if (grabbing != null && grabbing.GetGrabbingState())
    //        //{
    //        //    Anchors anchors = anchorsColliders[j].gameObject.GetComponent<Anchors>();

    //        //    if (anchors != null)
    //        //    {
    //        //        anchors.SetAnchored(false);
    //        //    }
    //        //}
    //    }

    //}

}
