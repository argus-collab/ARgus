using UnityEngine;
using UnityEngine.Networking;

// 9999
public class TransformMessage : MessageBase
{
    public NetworkInstanceId id;
    public Vector3 p;
    public Quaternion q;
    public bool isLocal = false;
}

// 9998
public class RigidBodyStateMessage : MessageBase
{
    public NetworkInstanceId id;
    public bool isKinematic;
}

// 9997
public class UIStateMessage : MessageBase
{
    public bool displayStat = false;
}

// 9996
public class StringMessage : MessageBase
{
    public string text = "";
}

// 9995
public class VisibilityMessage : MessageBase
{
    public NetworkInstanceId id;
    public bool visibility = false;
}

// 9994
public class HierarchyMessage : MessageBase
{
    public string nameParent;
    public NetworkInstanceId idChild;
}

// 9993
public class ColorMessage : MessageBase
{
    public NetworkInstanceId id;
    public Color color;
}

// 9992
public class NameMessage : MessageBase
{
    public NetworkInstanceId id;
    public string name;
}

// 9991
public class TimeMessage : MessageBase
{
    public int time;
}

// 9990
public class LaunchMessage : MessageBase
{
    public bool launch;
}

// 9989
public class TimeStampMessage : MessageBase
{
    public float timestamp;
}

// 9988
public class MeshMessage : MessageBase
{
    public string name;
    public Vector3 p;
    public Quaternion q;
    public byte[] mesh;
}

// 9987
public class TextureMessage : MessageBase
{
    public string name;
    public byte[] texture;
}

// 9986
public class AddStickerMessage : MessageBase
{
    public string nameGO;
    public Vector3 position;
    public Quaternion rotation;
}

// 9985
public class AddSketchMessage : MessageBase
{
    public string nameGO;
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;

    public float startWidth;
    public float endWidth;

    public int size;
    public string data; 
}

// 9984
public class HandModelMessage : MessageBase
{
    public string handedness;

    public Vector3 v_wrist;
    public Vector3 v_palm;
    public Vector3 v_thumb_metacarpal;
    public Vector3 v_thumb_proximal;
    public Vector3 v_thumb_distal;
    public Vector3 v_thumb_tip;
    public Vector3 v_index_metacarpal;
    public Vector3 v_index_knuckle;
    public Vector3 v_index_middle;
    public Vector3 v_index_distal;
    public Vector3 v_index_tip;
    public Vector3 v_middle_metacarpal;
    public Vector3 v_middle_knuckle;
    public Vector3 v_middle_middle;
    public Vector3 v_middle_distal;
    public Vector3 v_middle_tip;
    public Vector3 v_ring_metacarpal;
    public Vector3 v_ring_knuckle;
    public Vector3 v_ring_middle;
    public Vector3 v_ring_distal;
    public Vector3 v_ring_tip;
    public Vector3 v_pinky_metacarpal;
    public Vector3 v_pinky_knuckle;
    public Vector3 v_pinky_middle;
    public Vector3 v_pinky_distal;
    public Vector3 v_pinky_tip;

    public Quaternion q_wrist;
    public Quaternion q_palm;
    public Quaternion q_thumb_metacarpal;
    public Quaternion q_thumb_proximal;
    public Quaternion q_thumb_distal;
    public Quaternion q_thumb_tip;
    public Quaternion q_index_metacarpal;
    public Quaternion q_index_knuckle;
    public Quaternion q_index_middle;
    public Quaternion q_index_distal;
    public Quaternion q_index_tip;
    public Quaternion q_middle_metacarpal;
    public Quaternion q_middle_knuckle;
    public Quaternion q_middle_middle;
    public Quaternion q_middle_distal;
    public Quaternion q_middle_tip;
    public Quaternion q_ring_metacarpal;
    public Quaternion q_ring_knuckle;
    public Quaternion q_ring_middle;
    public Quaternion q_ring_distal;
    public Quaternion q_ring_tip;
    public Quaternion q_pinky_metacarpal;
    public Quaternion q_pinky_knuckle;
    public Quaternion q_pinky_middle;
    public Quaternion q_pinky_distal;
    public Quaternion q_pinky_tip;
}

// 9983
public class SceneGameObjectMessage : MessageBase
{
    public string name;
    public bool local;
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;
}

// 9982
public class SceneGameObjectInstanciateMessage : MessageBase
{
    public string name;
    public string namePrefab;
    public string parent;
    public Vector3 localPosition;
    public Quaternion localRotation;

    public bool isCustomScale;
    public Vector3 localScale;

    public bool isMaster;
}

// 9981
public class SceneGameObjectChangeColorMessage : MessageBase
{
    public string name;
    public bool inChildren = false;
    public Color color;
}

// 9980
public class SceneGameObjectRemoveMessage : MessageBase
{
    public string name;
}

// 9979
public class InputUmeyamaCalibrationMessage : MessageBase
{
    // pts in first coordinate system
    public Vector3 cs1_pt1;
    public Vector3 cs1_pt2;
    public Vector3 cs1_pt3;
    public Vector3 cs1_pt4;

    // pts in second coordinate system
    public Vector3 cs2_pt1;
    public Vector3 cs2_pt2;
    public Vector3 cs2_pt3;
    public Vector3 cs2_pt4;
}

// 9978
public class OutputUmeyamaCalibrationMessage : MessageBase
{
    public Vector3 translation;
    public Quaternion rotation;
}

// 9977
public class CommandMessage : MessageBase
{
    public string command;
    public string args;
}

// 9976
public class GenericType
{
    // ugly in memory but solution to use generic types with MessageBase

    public bool isInt;
    public int varInt;

    public bool isBool;
    public bool varBool;

    public bool isFloat;
    public float varFloat;

    public bool isString;
    public string varString;

    public bool isVector3;
    public Vector3 varVector3;

    public GenericType() { }

    public GenericType(int val)
    {
        isInt = true;
        varInt = val;
    }
    public GenericType(bool val) 
    {
        isBool = true;
        varBool = val;
    }
    public GenericType(float val)
    {
        isFloat = true;
        varFloat = val;
    }
    public GenericType(string val)
    {
        isString = true;
        varString = val;
    }
    public GenericType(Vector3 val)
    {
        isVector3 = true;
        varVector3 = val;
    }

    public object GetValue()
    {
        if (isInt)
            return varInt;
        if (isFloat)
            return varFloat;
        if (isBool)
            return varBool;
        if (isString)
            return varString;
        if (isVector3)
            return varVector3;

        return null;
    }
}

public class SceneGameObjectUpdateComponentMessage : MessageBase
{
    public string GOName;
    public string componentTypeName;
    public string[] propertiesNames; 
    public GenericType[] propertiesValues;
}

// 9975 - 9974
public class SpawnPrefabMessage : MessageBase
{
    public string PrefabName;
    public string GoName;
    public Vector3 initialPosition;
    public Quaternion initialRotation;
}

// 9973
public class UserDataMessage : MessageBase
{
    public string playerName;
    public string cameraToFollow;
    public int indexRep;
}

// 9972
public class LogEntryMessage : MessageBase
{
    public string logEntry;
    public string ip;
}

// 9971
public class SynchronizeMessage : MessageBase
{
}

// 9970
public class IpAdressMessage : MessageBase
{
    public string address;
}

// 9969
public class NetworkEventMessage : MessageBase
{
    public string networkEvent;
}
