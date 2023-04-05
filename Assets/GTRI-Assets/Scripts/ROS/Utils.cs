using System.Collections;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Geometry;
using System;

public class Utils : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public static void GetGeometryPoint(Vector3 unityVector, PointMsg rosPoint)
    {
        rosPoint.x = unityVector.x;
        rosPoint.y = unityVector.y;
        rosPoint.z = unityVector.z;
    }

    public static void GetGeometryQuaternion(Quaternion unityQuat, QuaternionMsg rosQuat)
    {
        rosQuat.x = unityQuat.x;
        rosQuat.y = unityQuat.y;
        rosQuat.z = unityQuat.z;
        rosQuat.w = unityQuat.w;
    }

    long UnixTimeNanoseconds()
    {
        DateTime epochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        return (DateTime.UtcNow - epochStart).Ticks * 100;
    }

    public static void AddTimeStamp(PointStampedMsg msg)
    {
        DateTime currentTime = DateTime.UtcNow;
        DateTime epochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        uint nanoseconds = (uint)(currentTime - epochStart).Ticks * 100;
        uint seconds = (uint)(currentTime - epochStart).TotalSeconds;
        msg.header.stamp.sec = seconds;
        msg.header.stamp.nanosec = nanoseconds;
    }
}
