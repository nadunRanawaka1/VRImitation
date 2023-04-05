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
                              // 
    private Transform prevControllerTransform;
    private Vector3 prevControllerPos; //The previous location of the controller
    private Quaternion prevControllerRot; //Previous rotation of controller
    private Vector3 prevControllerEuler;
    public GameObject rightController;
    public GameObject leftController;
    public GameObject originPrefab;

    
    private Matrix4x4 Unity2ROS;
    
    private GameObject origin;
    private GameObject currentController;
    private Transform controllerOriginalParent;
    private delegate void PublishMessage();
    private PublishMessage publisher;

    //ROS Stuff

    public string poseTopicName = "unity_pose_delta";
    public string positionTopicName = "unity_position_delta";
    public string angleTopicName = "unity_angle_delta";
    private PoseMsg poseMsg;
    private PointStampedMsg positionDeltaMsg;
    private PointStampedMsg angleDeltaMsg;

    // Debugging Stuff
    private double totalMovement;

    // Use this for initialization
    void Start()
    {
        //ROSConnection.GetOrCreateInstance().RegisterPublisher<PoseMsg>(topicName);
        SetupRos();

        prevControllerPos = rightController.transform.localPosition;
        prevControllerRot = rightController.transform.localRotation;
        controlPose = false;
        Unity2ROS = Matrix4x4.identity;
        Unity2ROS[0, 0] = -1;
        
        currentController = rightController; //TODO change this later

        InvokeRepeating("GetAndPublishPoseDeltaV2", 0, 0.05f);
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void SetupRos()
    {
        //ROSConnection.GetOrCreateInstance().RegisterPublisher<PoseMsg>(poseTopicName);
        //publisher = new PublishMessage(PublishPoseMessage);
        //poseMsg = new PoseMsg();

        ROSConnection.GetOrCreateInstance().RegisterPublisher<PointStampedMsg>(positionTopicName);
        ROSConnection.GetOrCreateInstance().RegisterPublisher<PointStampedMsg>(angleTopicName);
        positionDeltaMsg = new PointStampedMsg();
        angleDeltaMsg = new PointStampedMsg();
        publisher = new PublishMessage(PublishPositionAndAngleDelta);
    }

    public Vector3 RectifyEulerAngle(Vector3 eulerAngles)
    {
        //Rectify x
        if (eulerAngles.x > 180)
        {
            eulerAngles.x -= 360;
        }
        if (eulerAngles.x < -180)
        {
            eulerAngles.x += 360;
        }
        //Rectify y
        if (eulerAngles.y > 180)
        {
            eulerAngles.y -= 360;
        }
        if (eulerAngles.y < -180)
        {
            eulerAngles.y += 360;
        }
        // Rectify z
        if (eulerAngles.z > 180)
        {
            eulerAngles.z -= 360;
        }
        if (eulerAngles.z < -180)
        {
            eulerAngles.z += 360;
        }
        return eulerAngles;
    }

    public void GetAndPublishPoseDeltaV2()
    {
        if (controlPose)
        {
            Transform currControllerTransform = currentController.transform;
            /*Transform originalParent = currControllerTransform.parent;
            currControllerTransform.SetParent(prevControllerTransform);*/

            Vector3 currentEuler = currControllerTransform.localEulerAngles;
            currentEuler = RectifyEulerAngle(currentEuler);

            Vector3 posDiff = currControllerTransform.localPosition -  prevControllerPos;
            posDiff /= currControllerTransform.localScale.x; //Dividing by scale to get the delta in world coordinate scale.
            Vector3 rotDiffAsEuler = currentEuler -  prevControllerEuler;

            
            /*Debug.Log("Position Difference: " + posDiff);
            Debug.Log("Angle difference: " + rotDiffAsEuler);*/


            prevControllerPos = currControllerTransform.localPosition;
            prevControllerEuler = currControllerTransform.localEulerAngles;
            prevControllerEuler = RectifyEulerAngle(prevControllerEuler);

            Utils.GetGeometryPoint(posDiff, positionDeltaMsg.point);
            Utils.GetGeometryPoint(rotDiffAsEuler, angleDeltaMsg.point);
            Utils.AddTimeStamp(positionDeltaMsg);
            Utils.AddTimeStamp(angleDeltaMsg);
            publisher();

            //currControllerTransform.SetParent(originalParent);
            //prevControllerTransform = currControllerTransform;
        }
    }
    public void GetAndPublishPoseDelta()
    {
        if (controlPose)
        {
            Transform currControllerPose = currentController.transform;
            Vector3 posDiff = currControllerPose.localPosition - prevControllerPos;
            posDiff = Vector3.Scale(posDiff, currControllerPose.transform.parent.localScale);
            Quaternion rotDiff = currControllerPose.localRotation * Quaternion.Inverse(prevControllerRot);

           
            
            Debug.Log("This is the current controller position: " + currControllerPose.localPosition);
           

            Matrix4x4 transform = Matrix4x4.TRS(posDiff, rotDiff, Vector3.one);
            var finalTransform = Unity2ROS * transform * Unity2ROS;
            Vector3 posDelta = finalTransform.GetColumn(3);


            Quaternion rotDelta = finalTransform.rotation;

            Utils.GetGeometryPoint(posDelta, poseMsg.position);
            Utils.GetGeometryQuaternion(rotDelta, poseMsg.orientation);

            //Transforms into robot coordinate frame
            // TODO: handle this in a new function or on the ROS side.
            double x = poseMsg.position.x;
            double y = poseMsg.position.y;
            double z = poseMsg.position.z;

            poseMsg.position.x = -z;
            poseMsg.position.y = -x;
            poseMsg.position.z = y;


            prevControllerPos = currControllerPose.localPosition;
            prevControllerRot = currControllerPose.localRotation;

            
            Debug.Log("This is the pos delta: " + posDelta.ToString("F5"));
            //Debug.Log("This is the rot delta: " + rotDelta);
            Debug.Log("This is the difference in Euler angles: " + rotDiff.eulerAngles);
            PublishPoseMessage();
           
        }

        
    }

    public void PublishPositionAndAngleDelta()
    {
        Debug.Log("Publishing position and angle delta separately");
        Debug.Log("x delta: " + positionDeltaMsg.point.x);
        totalMovement += positionDeltaMsg.point.x;
        Debug.Log("total Movement" + totalMovement);
        ROSConnection.GetOrCreateInstance().Publish(positionTopicName, positionDeltaMsg);
        ROSConnection.GetOrCreateInstance().Publish(angleTopicName, angleDeltaMsg);
    }

    public void PublishPoseMessage()
    {
        Debug.Log(poseMsg);
        ROSConnection.GetOrCreateInstance().Publish(poseTopicName, poseMsg);
    }

    public void TogglePublish()
    {
        Debug.Log("Toggled Publish");
        controlPose = !controlPose;
        if (controlPose)
        {
            origin = GameObject.Instantiate(originPrefab, currentController.transform.position, currentController.transform.rotation);
            controllerOriginalParent = currentController.transform.parent;
            currentController.transform.SetParent(origin.transform);
            prevControllerTransform = currentController.transform;
            prevControllerPos = currentController.transform.localPosition;
            prevControllerRot = currentController.transform.localRotation;
            prevControllerEuler = currentController.transform.localEulerAngles;
            prevControllerEuler = RectifyEulerAngle(prevControllerEuler);
        }
        else
        {
            currentController.transform.SetParent(controllerOriginalParent);
            GameObject.Destroy(origin);
        }

    }
}
