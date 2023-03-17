using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Geometry;


public class SendPose : MonoBehaviour
{
    public string topicName = "EE_Pose";
    public Transform publishedTransform;
    public Transform parentTransform;
    public string frameId = "Unity";


    private bool publishMessage = true;
    private bool updatePose = false;
    private Vector3 localPosition;
    private Quaternion localRotation;
    private PoseStampedMsg message;
   
    private Matrix4x4 Unity2ROS;

    // Start is called before the first frame update
    void Start()
    {
        ROSConnection.GetOrCreateInstance().RegisterPublisher<PoseStampedMsg>(topicName);
        InitializeMessage();
        Unity2ROS = Matrix4x4.identity;
        Unity2ROS[0, 0] = -1;

    }

    private void FixedUpdate()
    {
        if (updatePose)
        {
            UpdateMessage();
            updatePose = false;
            if (publishMessage)
            {
                PublishMessage();
            }
        }
    }

    public void UpdatePose(Transform transform)
    {
        publishedTransform = transform;
        localPosition = transform.localPosition;
        localRotation = transform.localRotation;
        updatePose = true;
    }

    public void UpdatePose(Vector3 position, Quaternion rotation)
    {
        localPosition = position;
        localRotation = rotation;
        updatePose = true;
    }

    public void PublishMessage()
    {
        ROSConnection.GetOrCreateInstance().Publish(topicName, message);
    }

    public void gripPress()
    {
        Transform orig = publishedTransform.parent;
        publishedTransform.SetParent(parentTransform);
        UpdatePose(publishedTransform);
        publishedTransform.SetParent(orig);
    }

    private void UpdateMessage()
    {
        // Create matrix from relative position and rotation
        Matrix4x4 transform = Matrix4x4.TRS(localPosition, localRotation, Vector3.one);
        // Use transformation matrix to convert to right-handed coordinate frame
        var finalTransform = Unity2ROS * transform * Unity2ROS;
        Vector3 position = finalTransform.GetColumn(3);
        Quaternion rotation = finalTransform.rotation;
        GetGeometryPoint(position, message.pose.position);
        GetGeometryQuaternion(rotation, message.pose.orientation);
    }


    private void InitializeMessage()
    {
        message = new PoseStampedMsg
        {
            header = new RosMessageTypes.Std.HeaderMsg()
            {
                frame_id = frameId
            }
        };
    }

    private static void GetGeometryPoint(Vector3 position, PointMsg geometryPoint)
    {
        geometryPoint.x = position.x;
        geometryPoint.y = position.y;
        geometryPoint.z = position.z;
    }

    private static void GetGeometryQuaternion(Quaternion quaternion, QuaternionMsg geometryQuaternion)
    {
        geometryQuaternion.x = quaternion.x;
        geometryQuaternion.y = quaternion.y;
        geometryQuaternion.z = quaternion.z;
        geometryQuaternion.w = quaternion.w;
    }

   

    public void SetTimeStamp(RosMessageTypes.BuiltinInterfaces.TimeMsg timeMsg)
    {
        message.header.stamp = timeMsg;
    }

    public void SetTimeStamp(uint sec, uint nanosec)
    {
        message.header.stamp.sec = sec;
        message.header.stamp.nanosec = nanosec;
    }


}
