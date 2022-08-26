using UnityEngine;
using System.Collections;
using Windows.Kinect;

public class DepthSourceManager : MonoBehaviour
{   
    private KinectSensor _Sensor;
    private DepthFrameReader _Reader;
    private ushort[] _Data;

    public ushort[] GetData()
    {
        return _Data;
    }

    public void Reset()
    {
        OnApplicationQuit();
        Start();
    }

    void Start () 
    {
        _Sensor = KinectSensor.GetDefault();

        if (_Sensor != null)
        {
            _Reader = _Sensor.DepthFrameSource.OpenReader();
            Debug.Log("sensor open reader OK");
            _Data = new ushort[_Sensor.DepthFrameSource.FrameDescription.LengthInPixels];
        }
        else
            Debug.Log("sensor null");
    }
    
    void Update () 
    {
        if (_Reader != null)
        {
            var frame = _Reader.AcquireLatestFrame();
            if (frame != null)
            {
                frame.CopyFrameDataToArray(_Data);
                frame.Dispose();
                frame = null;
            }
        }
    }
    
    void OnApplicationQuit()
    {
        if (_Reader != null)
        {
            _Reader.Dispose();
            Debug.Log("Reader dispose OK");
            _Reader = null;
            Debug.Log("Reader set to null");
        }

        if (_Sensor != null)
        {
            if (_Sensor.IsOpen)
            {
                _Sensor.Close();
                Debug.Log("Sensor close OK");
            }

            _Sensor = null;
            Debug.Log("Sensor set to null");
        }
    }
}
