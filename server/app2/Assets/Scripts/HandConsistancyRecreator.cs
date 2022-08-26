using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandConsistancyRecreator : MonoBehaviour
{
    public float maxPhalangeLength = 5f;
    public float maxPalmPartLength = 10f;

    public GameObject left_thumb_metacarpal;
    public GameObject left_thumb_proximal;
    public GameObject left_index_metacarpal;
    public GameObject left_index_knuckle;
    public GameObject left_middle_metacarpal;
    public GameObject left_middle_knuckle;
    public GameObject left_ring_metacarpal;
    public GameObject left_ring_knuckle;
    public GameObject left_pinky_metacarpal;
    public GameObject left_pinky_knuckle;
    public GameObject left_thumb_distal;
    public GameObject left_thumb_tip;
    public GameObject left_index_middle;
    public GameObject left_index_distal;
    public GameObject left_index_tip;
    public GameObject left_middle_middle;
    public GameObject left_middle_distal;
    public GameObject left_middle_tip;
    public GameObject left_ring_middle;
    public GameObject left_ring_distal;
    public GameObject left_ring_tip;
    public GameObject left_pinky_middle;
    public GameObject left_pinky_distal;
    public GameObject left_pinky_tip;
    bool leftHandCreated = false;

    public GameObject right_thumb_metacarpal;
    public GameObject right_thumb_proximal;
    public GameObject right_index_metacarpal;
    public GameObject right_index_knuckle;
    public GameObject right_middle_metacarpal;
    public GameObject right_middle_knuckle;
    public GameObject right_ring_metacarpal;
    public GameObject right_ring_knuckle;
    public GameObject right_pinky_metacarpal;
    public GameObject right_pinky_knuckle;
    public GameObject right_thumb_distal;
    public GameObject right_thumb_tip;
    public GameObject right_index_middle;
    public GameObject right_index_distal;
    public GameObject right_index_tip;
    public GameObject right_middle_middle;
    public GameObject right_middle_distal;
    public GameObject right_middle_tip;
    public GameObject right_ring_middle;
    public GameObject right_ring_distal;
    public GameObject right_ring_tip;
    public GameObject right_pinky_middle;
    public GameObject right_pinky_distal;
    public GameObject right_pinky_tip;
    bool rightHandCreated = false;

    public bool go = false;

    bool LeftHandIdentification()
    {
        if (left_thumb_metacarpal == null)
            left_thumb_metacarpal = GameObject.Find("left_thumb_metacarpal");
        if (left_thumb_proximal == null)
            left_thumb_proximal = GameObject.Find("left_thumb_proximal");
        if (left_index_metacarpal == null)
            left_index_metacarpal = GameObject.Find("left_index_metacarpal");
        if (left_index_knuckle == null)
            left_index_knuckle = GameObject.Find("left_index_knuckle");
        if (left_middle_metacarpal == null)
            left_middle_metacarpal = GameObject.Find("left_middle_metacarpal");
        if (left_middle_knuckle == null)
            left_middle_knuckle = GameObject.Find("left_middle_knuckle");
        if (left_ring_metacarpal == null)
            left_ring_metacarpal = GameObject.Find("left_ring_metacarpal");
        if (left_ring_knuckle == null)
            left_ring_knuckle = GameObject.Find("left_ring_knuckle");
        if (left_pinky_metacarpal == null)
            left_pinky_metacarpal = GameObject.Find("left_pinky_metacarpal");
        if (left_pinky_knuckle == null)
            left_pinky_knuckle = GameObject.Find("left_pinky_knuckle");
        if (left_thumb_distal == null)
            left_thumb_distal = GameObject.Find("left_thumb_distal");
        if (left_thumb_tip == null)
            left_thumb_tip = GameObject.Find("left_thumb_tip");
        if (left_index_middle == null)
            left_index_middle = GameObject.Find("left_index_middle");
        if (left_index_distal == null)
            left_index_distal = GameObject.Find("left_index_distal");
        if (left_index_tip == null)
            left_index_tip = GameObject.Find("left_index_tip");
        if (left_middle_middle == null)
            left_middle_middle = GameObject.Find("left_middle_middle");
        if (left_middle_distal == null)
            left_middle_distal = GameObject.Find("left_middle_distal");
        if (left_middle_tip == null)
            left_middle_tip = GameObject.Find("left_middle_tip");
        if (left_ring_middle == null)
            left_ring_middle = GameObject.Find("left_ring_middle");
        if (left_ring_distal == null)
            left_ring_distal = GameObject.Find("left_ring_distal");
        if (left_ring_tip == null)
            left_ring_tip = GameObject.Find("left_ring_tip");
        if (left_pinky_middle == null)
            left_pinky_middle = GameObject.Find("left_pinky_middle");
        if (left_pinky_distal == null)
            left_pinky_distal = GameObject.Find("left_pinky_distal");
        if (left_pinky_tip == null)
            left_pinky_tip = GameObject.Find("left_pinky_tip");


        if (left_thumb_metacarpal == null
        || left_thumb_proximal == null
        || left_index_metacarpal == null
        || left_index_knuckle == null
        || left_middle_metacarpal == null
        || left_middle_knuckle == null
        || left_ring_metacarpal == null
        || left_ring_knuckle == null
        || left_pinky_metacarpal == null
        || left_pinky_knuckle == null
        || left_thumb_distal == null
        || left_thumb_tip == null
        || left_index_middle == null
        || left_index_distal == null
        || left_index_tip == null
        || left_middle_middle == null
        || left_middle_distal == null
        || left_middle_tip == null
        || left_ring_middle == null
        || left_ring_distal == null
        || left_ring_tip == null
        || left_pinky_middle == null
        || left_pinky_distal == null
        || left_pinky_tip == null)
            return false;
        else
            return true;
    }

    bool RightHandIdentification()
    {
        if (right_thumb_metacarpal == null)
            right_thumb_metacarpal = GameObject.Find("right_thumb_metacarpal");
        if(right_thumb_proximal == null) 
            right_thumb_proximal = GameObject.Find("right_thumb_proximal");
        if (right_index_metacarpal == null) 
            right_index_metacarpal = GameObject.Find("right_index_metacarpal");
        if (right_index_knuckle == null) 
            right_index_knuckle = GameObject.Find("right_index_knuckle");
        if (right_middle_metacarpal == null) 
            right_middle_metacarpal = GameObject.Find("right_middle_metacarpal");
        if (right_middle_knuckle == null) 
            right_middle_knuckle = GameObject.Find("right_middle_knuckle");
        if (right_ring_metacarpal == null) 
            right_ring_metacarpal = GameObject.Find("right_ring_metacarpal");
        if (right_ring_knuckle == null) 
            right_ring_knuckle = GameObject.Find("right_ring_knuckle");
        if (right_pinky_metacarpal == null) 
            right_pinky_metacarpal = GameObject.Find("right_pinky_metacarpal");
        if (right_pinky_knuckle == null) 
            right_pinky_knuckle = GameObject.Find("right_pinky_knuckle");
        if (right_thumb_distal == null) 
            right_thumb_distal = GameObject.Find("right_thumb_distal");
        if (right_thumb_tip == null) 
            right_thumb_tip = GameObject.Find("right_thumb_tip");
        if (right_index_middle == null) 
            right_index_middle = GameObject.Find("right_index_middle");
        if (right_index_distal == null) 
            right_index_distal = GameObject.Find("right_index_distal");
        if (right_index_tip == null) 
            right_index_tip = GameObject.Find("right_index_tip");
        if (right_middle_middle == null) 
            right_middle_middle = GameObject.Find("right_middle_middle");
        if (right_middle_distal == null) 
            right_middle_distal = GameObject.Find("right_middle_distal");
        if (right_middle_tip == null) 
            right_middle_tip = GameObject.Find("right_middle_tip");
        if (right_ring_middle == null) 
            right_ring_middle = GameObject.Find("right_ring_middle");
        if (right_ring_distal == null) 
            right_ring_distal = GameObject.Find("right_ring_distal");
        if (right_ring_tip == null) 
            right_ring_tip = GameObject.Find("right_ring_tip");
        if (right_pinky_middle == null) 
            right_pinky_middle = GameObject.Find("right_pinky_middle");
        if (right_pinky_distal == null) 
            right_pinky_distal = GameObject.Find("right_pinky_distal");
        if (right_pinky_tip == null) 
            right_pinky_tip = GameObject.Find("right_pinky_tip");


        if (right_thumb_metacarpal == null
        || right_thumb_proximal == null
        || right_index_metacarpal == null
        || right_index_knuckle == null
        || right_middle_metacarpal == null
        || right_middle_knuckle == null
        || right_ring_metacarpal == null
        || right_ring_knuckle == null
        || right_pinky_metacarpal == null
        || right_pinky_knuckle == null
        || right_thumb_distal == null
        || right_thumb_tip == null
        || right_index_middle == null
        || right_index_distal == null
        || right_index_tip == null
        || right_middle_middle == null
        || right_middle_distal == null
        || right_middle_tip == null
        || right_ring_middle == null
        || right_ring_distal == null
        || right_ring_tip == null
        || right_pinky_middle == null
        || right_pinky_distal == null
        || right_pinky_tip == null)
            return false;
        else
            return true;

    }

    bool LeftHandFullyLoaded()
    {
        return (left_thumb_metacarpal.transform.position != Vector3.zero
         && left_thumb_proximal.transform.position != Vector3.zero
         && left_index_metacarpal.transform.position != Vector3.zero
         && left_index_knuckle.transform.position != Vector3.zero
         && left_middle_metacarpal.transform.position != Vector3.zero
         && left_middle_knuckle.transform.position != Vector3.zero
         && left_ring_metacarpal.transform.position != Vector3.zero
         && left_ring_knuckle.transform.position != Vector3.zero
         && left_pinky_metacarpal.transform.position != Vector3.zero
         && left_pinky_knuckle.transform.position != Vector3.zero
         && left_thumb_distal.transform.position != Vector3.zero
         && left_thumb_tip.transform.position != Vector3.zero
         && left_index_middle.transform.position != Vector3.zero
         && left_index_distal.transform.position != Vector3.zero
         && left_index_tip.transform.position != Vector3.zero
         && left_middle_middle.transform.position != Vector3.zero
         && left_middle_distal.transform.position != Vector3.zero
         && left_middle_tip.transform.position != Vector3.zero
         && left_ring_middle.transform.position != Vector3.zero
         && left_ring_distal.transform.position != Vector3.zero
         && left_ring_tip.transform.position != Vector3.zero
         && left_pinky_middle.transform.position != Vector3.zero
         && left_pinky_distal.transform.position != Vector3.zero
         && left_pinky_tip);
    }

    bool RightHandFullyLoaded()
    {
        return (right_thumb_metacarpal.transform.position != Vector3.zero
         && right_thumb_proximal.transform.position != Vector3.zero
         && right_index_metacarpal.transform.position != Vector3.zero
         && right_index_knuckle.transform.position != Vector3.zero
         && right_middle_metacarpal.transform.position != Vector3.zero
         && right_middle_knuckle.transform.position != Vector3.zero
         && right_ring_metacarpal.transform.position != Vector3.zero
         && right_ring_knuckle.transform.position != Vector3.zero
         && right_pinky_metacarpal.transform.position != Vector3.zero
         && right_pinky_knuckle.transform.position != Vector3.zero
         && right_thumb_distal.transform.position != Vector3.zero
         && right_thumb_tip.transform.position != Vector3.zero
         && right_index_middle.transform.position != Vector3.zero
         && right_index_distal.transform.position != Vector3.zero
         && right_index_tip.transform.position != Vector3.zero
         && right_middle_middle.transform.position != Vector3.zero
         && right_middle_distal.transform.position != Vector3.zero
         && right_middle_tip.transform.position != Vector3.zero
         && right_ring_middle.transform.position != Vector3.zero
         && right_ring_distal.transform.position != Vector3.zero
         && right_ring_tip.transform.position != Vector3.zero
         && right_pinky_middle.transform.position != Vector3.zero
         && right_pinky_distal.transform.position != Vector3.zero
         && right_pinky_tip);
    }

    void Update()
    {
        //if(go)
        //{
            if (rightHandCreated && !RightHandIdentification())
                rightHandCreated = false;
            if (leftHandCreated && !LeftHandIdentification())
                leftHandCreated = false;

            if (!rightHandCreated && RightHandIdentification() && RightHandFullyLoaded())
            {
                rightHandCreated = CreateHandConsistency("right");
            }   

            if (!leftHandCreated && LeftHandIdentification() && LeftHandFullyLoaded())
            {
                leftHandCreated = CreateHandConsistency("left");
            }

            //go = false;
        //}

    }

    public bool CreateHandConsistency(string laterality)
    {
        return CreatePalmConsistency(laterality)

        && CreateThumbConsistency(laterality)
        && CreateIndexConsistency(laterality)
        && CreateMiddleConsistency(laterality)
        && CreateRingConsistency(laterality)
        && CreateRingConsistency(laterality)
        && CreatePinkyConsistency(laterality);
    }


    public bool CreatePhalange(string name, GameObject top, GameObject bottom, string laterality)
    {
        float phalangeLength = (top.transform.position - bottom.transform.position).magnitude * 1 / bottom.transform.localScale.magnitude;

        if (phalangeLength > maxPalmPartLength)
            return false;

        GameObject phalange = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        phalange.name = laterality + "_" + name;
        phalange.layer = 8;
        phalange.transform.parent = bottom.transform;
        phalange.transform.localPosition = new Vector3(0.0f, 0.0f, phalangeLength);
        phalange.transform.localRotation = Quaternion.Euler(90.0f, 0.0f, 0.0f);
        phalange.transform.parent = bottom.transform;
        phalange.transform.localScale = new Vector3(1.5f, phalangeLength, 1.5f);

        return true;
    }

    public bool CreatePalmPart(GameObject top, GameObject bottom, string laterality)
    {
        float palmPartLength = (top.transform.position - bottom.transform.position).magnitude * 1 / bottom.transform.localScale.magnitude;

        if (palmPartLength > maxPalmPartLength)
            return false;

        GameObject palmPart = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        palmPart.name = laterality + "_palm_part";
        //palmPart.layer = 8;
        palmPart.transform.parent = bottom.transform;
        palmPart.transform.localPosition = new Vector3(0.0f, 0.0f, palmPartLength);
        palmPart.transform.localRotation = Quaternion.Euler(90.0f, 0.0f, 0.0f);
        palmPart.transform.parent = bottom.transform;
        palmPart.transform.localScale = new Vector3(2.5f, palmPartLength, 1f);

        return true;
    }

    public bool CreatePalmConsistency(string laterality)
    {
        if(laterality == "right")
        {
            return CreatePalmPart(right_thumb_proximal, right_thumb_metacarpal, laterality)
            && CreatePalmPart(right_index_knuckle, right_index_metacarpal, laterality)
            && CreatePalmPart(right_middle_knuckle, right_middle_metacarpal, laterality)
            && CreatePalmPart(right_ring_knuckle, right_ring_metacarpal, laterality)
            && CreatePalmPart(right_pinky_knuckle, right_pinky_metacarpal, laterality);
        }
        else
        {
            return CreatePalmPart(left_thumb_proximal, left_thumb_metacarpal, laterality)
            && CreatePalmPart(left_index_knuckle, left_index_metacarpal, laterality)
            && CreatePalmPart(left_middle_knuckle, left_middle_metacarpal, laterality)
            && CreatePalmPart(left_ring_knuckle, left_ring_metacarpal, laterality)
            && CreatePalmPart(left_pinky_knuckle, left_pinky_metacarpal, laterality);
        }
    }

    public bool CreateThumbConsistency(string laterality)
    {
        if (laterality == "right")
        {
            return CreatePhalange("distal_phalange", right_thumb_tip, right_thumb_distal, laterality)
            && CreatePhalange("proximal_phalange", right_thumb_distal, right_thumb_proximal, laterality);
        }
        else
        {
            return CreatePhalange("distal_phalange", left_thumb_tip, left_thumb_distal, laterality)
            && CreatePhalange("proximal_phalange", left_thumb_distal, left_thumb_proximal, laterality);
        }
    }

    public bool CreateIndexConsistency(string laterality)
    {
        if (laterality == "right")
        {
            return CreatePhalange("distal_phalange", right_index_tip, right_index_distal, laterality)
            && CreatePhalange("intermediate_phalange", right_index_distal, right_index_middle, laterality)
            && CreatePhalange("proximal_phalange", right_index_middle, right_index_knuckle, laterality);
        }
        else
        {
            return CreatePhalange("distal_phalange", left_index_tip, left_index_distal, laterality)
            && CreatePhalange("intermediate_phalange", left_index_distal, left_index_middle, laterality)
            && CreatePhalange("proximal_phalange", left_index_middle, left_index_knuckle, laterality);
        }
    }

    public bool CreateMiddleConsistency(string laterality)
    {
        if (laterality == "right")
        {
            return CreatePhalange("distal_phalange", right_middle_tip, right_middle_distal, laterality)
            && CreatePhalange("intermediate_phalange", right_middle_distal, right_middle_middle, laterality)
            && CreatePhalange("proximal_phalange", right_middle_middle, right_middle_knuckle, laterality);
        }
        else
        {
            return CreatePhalange("distal_phalange", left_middle_tip, left_middle_distal, laterality)
            && CreatePhalange("intermediate_phalange", left_middle_distal, left_middle_middle, laterality)
            && CreatePhalange("proximal_phalange", left_middle_middle, left_middle_knuckle, laterality);
        }
    }

    public bool CreateRingConsistency(string laterality)
    {
        if (laterality == "right")
        {
            return CreatePhalange("distal_phalange", right_ring_tip, right_ring_distal, laterality)
            && CreatePhalange("intermediate_phalange", right_ring_distal, right_ring_middle, laterality)
            && CreatePhalange("proximal_phalange", right_ring_middle, right_ring_knuckle, laterality);
        }
        else
        {
            return CreatePhalange("distal_phalange", left_ring_tip, left_ring_distal, laterality)
            && CreatePhalange("intermediate_phalange", left_ring_distal, left_ring_middle, laterality)
            && CreatePhalange("proximal_phalange", left_ring_middle, left_ring_knuckle, laterality);
        }
    }

    public bool CreatePinkyConsistency(string laterality)
    {
        if (laterality == "right")
        {
            return CreatePhalange("distal_phalange", right_pinky_tip, right_pinky_distal, laterality)
            && CreatePhalange("intermediate_phalange", right_pinky_distal, right_pinky_middle, laterality)
            && CreatePhalange("proximal_phalange", right_pinky_middle, right_pinky_knuckle, laterality);
        }
        else
        {
            return CreatePhalange("distal_phalange", left_pinky_tip, left_pinky_distal, laterality)
            && CreatePhalange("intermediate_phalange", left_pinky_distal, left_pinky_middle, laterality)
            && CreatePhalange("proximal_phalange", left_pinky_middle, left_pinky_knuckle, laterality);
        }
    }
}
