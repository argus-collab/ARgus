using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Networking;



public class NetworkSyncObject : NetworkBehaviour
{
    public bool isLocal = false;
    public bool temporaryClientAuthority = false;
    public float movementThreshold = 0.001f;

    private Vector3 lastPosition;
    private Quaternion lastRotation;

    public int messagePerSeconds = 90;
    private float lastTimeStamp;

    private Vector3 requestedPosition;
    private Quaternion requestedRotation;
    private bool requestedTransform = false;

    [Header("Debug")]
    public Vector3 askedPosition;
    public bool isKinematic = false;
    public bool askPosition = false;
    public bool askKinematic = false;

    private Rigidbody rb;


    void Start()
    {
        lastTimeStamp = Time.realtimeSinceStartup;


        //if (isClient)
        //    NetworkManager.singleton.client.RegisterHandler(MsgType.Connect, OnConnected);

        rb = GetComponent<Rigidbody>();

        lastPosition = Vector3.zero;
        lastRotation = Quaternion.identity;
    }

    public void RequestTransform(Vector3 p, Quaternion q)
    {
        requestedPosition = p;
        requestedRotation = q;

        requestedTransform = true;

        // reset speed ??
    }

    public void UpdateRigidBodyState(bool isKinematic)
    {
        if (rb != null)
            rb.isKinematic = isKinematic;
    }

    // update wanted transform before physics
    void FixedUpdate() 
    {
        if (isServer && requestedTransform)
        {
            //requestedTransform = false;

            transform.position = requestedPosition;
            transform.rotation = requestedRotation;
        }
    }

    void Update()
    {

        if (askPosition)
        { 
            //ask = false;
            AskNewTransform(askedPosition, Quaternion.identity, false);
        }

        if(temporaryClientAuthority)
        {
            AskNewTransform(transform.position, transform.rotation, false);
        }

        if (askKinematic)
        {
            askKinematic = false;
            AskRigidBodyStateUpdate(isKinematic);
        }

        float currentTimeStamp = Time.realtimeSinceStartup;


        if(isServer && currentTimeStamp - lastTimeStamp > 1 / messagePerSeconds)
        {
            // TODO update : send only the changing values

            Vector3 deltaPosition = transform.position - lastPosition;
            Quaternion deltaRotation = transform.rotation * Quaternion.Inverse(lastRotation);

            if(!requestedTransform)
            {
                if (Mathf.Abs(deltaPosition.x) > movementThreshold
                || Mathf.Abs(deltaPosition.y) > movementThreshold
                || Mathf.Abs(deltaPosition.z) > movementThreshold
                || Mathf.Abs(deltaRotation.x) > movementThreshold
                || Mathf.Abs(deltaRotation.y) > movementThreshold
                || Mathf.Abs(deltaRotation.z) > movementThreshold)
                {
                    if (isLocal)
                    {
                        GameObject.FindObjectOfType<CustomServerNetworkManager>().UpdateSyncObjectTransform(
                            gameObject.GetComponent<NetworkIdentity>().netId,
                            transform.localPosition,
                            transform.localRotation,
                            true);
                    }
                    else
                    {
                        GameObject.FindObjectOfType<CustomServerNetworkManager>().UpdateSyncObjectTransform(
                            gameObject.GetComponent<NetworkIdentity>().netId,
                            transform.position,
                            transform.rotation,
                            false);
                    }
                }
            }
            else
            {
                requestedTransform = false;
            }
        }

        lastPosition = transform.position;
        lastRotation = transform.rotation;

    }

    // call if a client want to move the object
    public void AskNewTransform(Vector3 p, Quaternion q, bool isLocal)
    {
        float currentTimeStamp = Time.realtimeSinceStartup;

        //if (currentTimeStamp - lastTimeStamp > 1 / messagePerSeconds)
        {
            MsgAskForNewTransform(p, q, isLocal);

            lastTimeStamp = currentTimeStamp;
        }

        // send time stamp when client ask for position
        //MsgSendTimeStamp("AskForNewTransform");
    }

    public void AskRigidBodyStateUpdate(bool isKinematic)
    {
        MsgRigidBodyStateUpdate(isKinematic);
    }

    public void MsgSendTimeStamp(string subject)
    {
        StringMessage msg = new StringMessage();

        msg.text = "[ " + subject + " ] ";
        msg.text += NetworkManager.singleton.client.connection.address;
        msg.text += " - ";
        msg.text += gameObject.GetComponent<NetworkIdentity>().netId.ToString();
        msg.text += " - ";
        msg.text += Time.realtimeSinceStartup;
        
        NetworkManager.singleton.client.SendByChannel((short)9996, msg, 2);
    }


    void MsgAskForNewTransform(Vector3 p, Quaternion q, bool isLocal)
    {
        TransformMessage msg = new TransformMessage();

        msg.id = gameObject.GetComponent<NetworkIdentity>().netId;
        msg.p = p;
        msg.q = q;
        msg.isLocal = isLocal;

        NetworkManager.singleton.client.SendByChannel((short)9999, msg, 2);
    }

    void MsgRigidBodyStateUpdate(bool isKinematic)
    {
        RigidBodyStateMessage msg = new RigidBodyStateMessage();

        msg.id = gameObject.GetComponent<NetworkIdentity>().netId;
        msg.isKinematic = isKinematic;

        NetworkManager.singleton.client.SendByChannel((short)9998, msg, 0);
    }

    public void SetRemoteIsLocal(bool local)
    {
        RpcSetIsLocal(local);
    }

    [ClientRpc]
    void RpcSetIsLocal(bool local)
    {
        isLocal = local;
    }

    public void AllowClientAuthority()
    {
        temporaryClientAuthority = true;
    }

    public void DisableClientAuthority()
    {
        temporaryClientAuthority = false;
    }
}
