using System.Collections;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Geometry;
/// <summary>
/// This class will control movement using delta control
/// </summary>
public class DeltaController : MonoBehaviour
{
    private bool controlPose; // Whether or not to send the pose 
    private Transform prevControllerPose; //The previous location of the controller
    public GameObject rightController;
    public GameObject leftController;

    private Matrix4x4 Unity2ROS;
    private PoseMsg poseMsg;


    //ROS Stuff

    public string topicName = "unity_pose";

    // Use this for initialization
    void Start()
    {
        ROSConnection.GetOrCreateInstance().RegisterPublisher<PoseMsg>(topicName);

        prevControllerPose = rightController.transform;
        controlPose = false;
        Unity2ROS = Matrix4x4.identity;
        Unity2ROS[0, 0] = -1;
        poseMsg = new PoseMsg();

        InvokeRepeating("GetAndPublishPoseDelta", 0, 3.0f);
    }

    // Update is called once per frame
    void Update()
    {

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

    public void GetAndPublishPoseDelta()
    {
        Transform currControllerPose = rightController.transform;
        Vector3 posDiff = currControllerPose.position - prevControllerPose.position;
        Quaternion rotDiff = currControllerPose.rotation * Quaternion.Inverse(prevControllerPose.rotation);

        Debug.Log("This is the current controller position: " + currControllerPose.position);
        Debug.Log("This is the previous controller Position: " + prevControllerPose.position);

        Matrix4x4 transform = Matrix4x4.TRS(posDiff, rotDiff, Vector3.one);
        var finalTransform = Unity2ROS * transform * Unity2ROS;
        Vector3 posDelta = finalTransform.GetColumn(3);
        Quaternion rotDelta = finalTransform.rotation;
        GetGeometryPoint(posDelta, poseMsg.position);
        GetGeometryQuaternion(rotDelta, poseMsg.orientation);
        prevControllerPose = currControllerPose;

        if(controlPose)
        {
            PublishMessage();   
        }
    }

    public void PublishMessage()
    {
        Debug.Log(poseMsg);
        ROSConnection.GetOrCreateInstance().Publish(topicName, poseMsg);
    }

    public void TogglePublish()
    {
        controlPose = !controlPose;
    }
}
