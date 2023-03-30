using System.Collections;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Geometry;

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
}
