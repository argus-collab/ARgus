using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;


namespace Microsoft.MixedReality.Toolkit.Input
{
    //public class Hand3DModelSync : MonoBehaviour, IMixedRealityHandMeshHandler//, IMixedRealityHandJointHandler
    //[DisallowMultipleComponent]
    //public class Hand3DModelSync : MonoBehaviour, IMixedRealityHandMeshHandler
    public class Hand3DModelSync : MonoBehaviour, IMixedRealityHandJointHandler
    {
        public float frequency = 30;
        private float lastTimeStampRightHand;
        private float lastTimeStampLeftHand;

        //public GameObject debugIndicator;
        //private MeshRenderer debugIndicatorRenderer;
        private Mesh handMesh;
        private CustomClientNetworkManager network;

        private float lastMeshUpdate = 0.0f;

        void Start()
        { 
            network = GameObject.FindObjectOfType<CustomClientNetworkManager>();

            if (network == null)
                Debug.LogError("Network Manager not found");

            //debugIndicatorRenderer = debugIndicator.GetComponent<MeshRenderer>();

            handMesh = new Mesh();

            //CoreServices.InputSystem.RegisterHandler<IMixedRealityHandMeshHandler>(this);
            CoreServices.InputSystem.RegisterHandler<IMixedRealityHandJointHandler>(this);

            lastTimeStampLeftHand = Time.time;
            lastTimeStampRightHand = Time.time;

        }

        //void IMixedRealityHandMeshHandler.OnHandMeshUpdated(InputEventData<HandMeshInfo> eventData)
        //{

        //    if(Time.time - lastMeshUpdate > 0.03f)
        //    {
        //        lastMeshUpdate = Time.time;

        //        //Debug.LogError("OnHandMeshUpdated");
        //        debugIndicatorRenderer.material.color = Color.blue;


        //        SendHandMesh(eventData);
        //    }
        //}

        void SendHandMesh(InputEventData<HandMeshInfo> eventData)
        {
            handMesh.Clear();

            handMesh.vertices = eventData.InputData.vertices;
            handMesh.triangles = eventData.InputData.triangles;
            handMesh.normals = eventData.InputData.normals;

            string name = "unidentifiedHandMesh";

            if (eventData.Handedness == Handedness.Left)
                name = "leftHandMesh";
            else if (eventData.Handedness == Handedness.Right)
                name = "rightHandMesh";

            network.SendMeshToAll(name,
                    eventData.InputData.position,
                    eventData.InputData.rotation,
                    MeshSerializer.WriteMesh(handMesh, false));
        }

        void SendHandModel(InputEventData<IDictionary<TrackedHandJoint, MixedRealityPose>> eventData)
        {
            List<Vector3> handModelPositions = new List<Vector3>();
            List<Quaternion> handModelRotations = new List<Quaternion>();

            /*
             doc suggest 
            if (eventData.Handedness == myHandedness)
            {
                if (eventData.InputData.TryGetValue(TrackedHandJoint.IndexTip, out MixedRealityPose pose))
                {
                    // ...
                }
            }
             */

            // positions
            handModelPositions.Add(eventData.InputData[TrackedHandJoint.Wrist].Position);
            handModelPositions.Add(eventData.InputData[TrackedHandJoint.Palm].Position);

            handModelPositions.Add(eventData.InputData[TrackedHandJoint.ThumbMetacarpalJoint].Position);
            handModelPositions.Add(eventData.InputData[TrackedHandJoint.ThumbProximalJoint].Position);
            handModelPositions.Add(eventData.InputData[TrackedHandJoint.ThumbDistalJoint].Position);
            handModelPositions.Add(eventData.InputData[TrackedHandJoint.ThumbTip].Position);

            handModelPositions.Add(eventData.InputData[TrackedHandJoint.IndexMetacarpal].Position);
            handModelPositions.Add(eventData.InputData[TrackedHandJoint.IndexKnuckle].Position);
            handModelPositions.Add(eventData.InputData[TrackedHandJoint.IndexMiddleJoint].Position);
            handModelPositions.Add(eventData.InputData[TrackedHandJoint.IndexDistalJoint].Position);
            handModelPositions.Add(eventData.InputData[TrackedHandJoint.IndexTip].Position);

            handModelPositions.Add(eventData.InputData[TrackedHandJoint.MiddleMetacarpal].Position);
            handModelPositions.Add(eventData.InputData[TrackedHandJoint.MiddleKnuckle].Position);
            handModelPositions.Add(eventData.InputData[TrackedHandJoint.MiddleMiddleJoint].Position);
            handModelPositions.Add(eventData.InputData[TrackedHandJoint.MiddleDistalJoint].Position);
            handModelPositions.Add(eventData.InputData[TrackedHandJoint.MiddleTip].Position);

            handModelPositions.Add(eventData.InputData[TrackedHandJoint.RingMetacarpal].Position);
            handModelPositions.Add(eventData.InputData[TrackedHandJoint.RingKnuckle].Position);
            handModelPositions.Add(eventData.InputData[TrackedHandJoint.RingMiddleJoint].Position);
            handModelPositions.Add(eventData.InputData[TrackedHandJoint.RingDistalJoint].Position);
            handModelPositions.Add(eventData.InputData[TrackedHandJoint.RingTip].Position);

            handModelPositions.Add(eventData.InputData[TrackedHandJoint.PinkyMetacarpal].Position);
            handModelPositions.Add(eventData.InputData[TrackedHandJoint.PinkyKnuckle].Position);
            handModelPositions.Add(eventData.InputData[TrackedHandJoint.PinkyMiddleJoint].Position);
            handModelPositions.Add(eventData.InputData[TrackedHandJoint.PinkyDistalJoint].Position);
            handModelPositions.Add(eventData.InputData[TrackedHandJoint.PinkyTip].Position);


            // rotations
            handModelRotations.Add(eventData.InputData[TrackedHandJoint.Wrist].Rotation);
            handModelRotations.Add(eventData.InputData[TrackedHandJoint.Palm].Rotation);

            handModelRotations.Add(eventData.InputData[TrackedHandJoint.ThumbMetacarpalJoint].Rotation);
            handModelRotations.Add(eventData.InputData[TrackedHandJoint.ThumbProximalJoint].Rotation);
            handModelRotations.Add(eventData.InputData[TrackedHandJoint.ThumbDistalJoint].Rotation);
            handModelRotations.Add(eventData.InputData[TrackedHandJoint.ThumbTip].Rotation);

            handModelRotations.Add(eventData.InputData[TrackedHandJoint.IndexMetacarpal].Rotation);
            handModelRotations.Add(eventData.InputData[TrackedHandJoint.IndexKnuckle].Rotation);
            handModelRotations.Add(eventData.InputData[TrackedHandJoint.IndexMiddleJoint].Rotation);
            handModelRotations.Add(eventData.InputData[TrackedHandJoint.IndexDistalJoint].Rotation);
            handModelRotations.Add(eventData.InputData[TrackedHandJoint.IndexTip].Rotation);

            handModelRotations.Add(eventData.InputData[TrackedHandJoint.MiddleMetacarpal].Rotation);
            handModelRotations.Add(eventData.InputData[TrackedHandJoint.MiddleKnuckle].Rotation);
            handModelRotations.Add(eventData.InputData[TrackedHandJoint.MiddleMiddleJoint].Rotation);
            handModelRotations.Add(eventData.InputData[TrackedHandJoint.MiddleDistalJoint].Rotation);
            handModelRotations.Add(eventData.InputData[TrackedHandJoint.MiddleTip].Rotation);

            handModelRotations.Add(eventData.InputData[TrackedHandJoint.RingMetacarpal].Rotation);
            handModelRotations.Add(eventData.InputData[TrackedHandJoint.RingKnuckle].Rotation);
            handModelRotations.Add(eventData.InputData[TrackedHandJoint.RingMiddleJoint].Rotation);
            handModelRotations.Add(eventData.InputData[TrackedHandJoint.RingDistalJoint].Rotation);
            handModelRotations.Add(eventData.InputData[TrackedHandJoint.RingTip].Rotation);

            handModelRotations.Add(eventData.InputData[TrackedHandJoint.PinkyMetacarpal].Rotation);
            handModelRotations.Add(eventData.InputData[TrackedHandJoint.PinkyKnuckle].Rotation);
            handModelRotations.Add(eventData.InputData[TrackedHandJoint.PinkyMiddleJoint].Rotation);
            handModelRotations.Add(eventData.InputData[TrackedHandJoint.PinkyDistalJoint].Rotation);
            handModelRotations.Add(eventData.InputData[TrackedHandJoint.PinkyTip].Rotation);

            network.SendHandModelToAll(eventData.Handedness.ToString(), handModelPositions, handModelRotations);
        }

        void IMixedRealityHandJointHandler.OnHandJointsUpdated(InputEventData<IDictionary<TrackedHandJoint, MixedRealityPose>> eventData)
        {
            //debugIndicatorRenderer.material.color = Color.green;

            if (eventData.Handedness.IsLeft() && Time.time - lastTimeStampLeftHand > (1 / frequency))
            {
                lastTimeStampLeftHand = Time.time;

                SendHandModel(eventData);
            }

            if (eventData.Handedness.IsRight() && Time.time - lastTimeStampRightHand > (1 / frequency))
            {
                lastTimeStampRightHand = Time.time;

                SendHandModel(eventData);
            }
        }

     















        //void MapLeftHand()
        //{

        ////"None Proxy Transform"
        ////"Wrist Proxy Transform"
        ////"Palm Proxy Transform"
        ////"
        ////"ThumbMetacarpalJoint Proxy Transform
        ////"ThumbProximalJoint Proxy Transform
        ////"ThumbDistalJoint Proxy Transform
        ////"ThumbTip Proxy Transform
        ////"
        ////"IndexMetacarpal Proxy Transform
        ////"IndexKnuckle Proxy Transform
        ////"IndexMiddleJoint Proxy Transform
        ////"IndexDistalJoint Proxy Transform
        ////"IndexTip Proxy Transform
        ////"
        ////"MiddleMetacarpal Proxy Transform
        ////"MiddleKnuckle Proxy Transform
        ////"MiddleMiddleJoint Proxy Transform
        ////"MiddleDistalJoint Proxy Transform
        ////"MiddleTip Proxy Transform
        ////"
        ////"RingMetacarpal Proxy Transform
        ////"RingKnuckle Proxy Transform
        ////"RingMiddleJoint Proxy Transform
        ////"RingDistalJoint Proxy Transform
        ////"RingTip Proxy Transform
        ////"
        ////"PinkyMetacarpal Proxy Transform
        ////"PinkyKnuckle Proxy Transform
        ////"PinkyMiddleJoint Proxy Transform
        ////"PinkyDistalJoint Proxy Transform
        ////"PinkyTip Proxy Transform
        //}

        //void MapRightHand()
        //{

        //}
    }
}

