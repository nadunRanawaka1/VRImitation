using System.Collections;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Geometry;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
/// <summary>
/// This class will control movement using delta control
/// </summary>
public class DeltaController : MonoBehaviour
{
    private bool controlPose; // Whether or not to send the pose 
    
    private Vector3 prevControllerPos; //The previous location of the controller
    private Quaternion prevControllerRot; //Previous rotation of controller
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

        prevControllerPos = rightController.transform.localPosition;
        prevControllerRot = rightController.transform.localRotation;
        controlPose = false;
        Unity2ROS = Matrix4x4.identity;
        Unity2ROS[0, 0] = -1;
        poseMsg = new PoseMsg();

        InvokeRepeating("GetAndPublishPoseDelta", 0, 0.05f);
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
        Vector3 posDiff = currControllerPose.localPosition - prevControllerPos;
        Quaternion rotDiff = currControllerPose.localRotation * Quaternion.Inverse(prevControllerRot);

        Debug.Log("This is the current controller position: " + currControllerPose.localPosition);
        Debug.Log("This is the previous controller Position: " + prevControllerPos);

        Matrix4x4 transform = Matrix4x4.TRS(posDiff, rotDiff, Vector3.one);
        var finalTransform = Unity2ROS * transform * Unity2ROS;
        Vector3 posDelta = finalTransform.GetColumn(3);
        Debug.Log("This is the pos delta: " + posDelta.ToString("F5"));
        
        Quaternion rotDelta = finalTransform.rotation;
        Debug.Log("This is the rot delta: " + rotDelta);
        GetGeometryPoint(posDelta, poseMsg.position);
        GetGeometryQuaternion(rotDelta, poseMsg.orientation);

        //Transforms into robot coordinate frame
        double x = poseMsg.position.x;
        double y = poseMsg.position.y;
        double z = poseMsg.position.z;

        poseMsg.position.y = x;
        poseMsg.position.x = z;
        poseMsg.position.z = y;


        prevControllerPos = currControllerPose.localPosition;
        prevControllerRot = currControllerPose.localRotation;

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
        Debug.Log("Toggled Publish");
        controlPose = !controlPose;
    }
}
