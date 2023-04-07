using System.Collections;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Geometry;
public class PositionController : MonoBehaviour
{
    public GameObject rightController;
    public GameObject leftController;
    public GameObject originPrefab;
    [Tooltip("The frame to publish the pose in")]
    public Transform originFrame; //Publish the pose with respect to this Transform

    private Matrix4x4 Unity2ROS;
    private bool controlPose;
    private GameObject currentController;

    //ROS Stuff
    public string positionTopicName = "unity_position";
    public string poseTopicName = "unity_pose";
    private PointMsg positionMsg;
    private PoseStampedMsg poseMsg;

    //Debugging Stuff

    public GameObject visualMarker;
    GameObject origin;

    // Use this for initialization
    void Start()
    {
        SetupRos();

        controlPose = false;
        Unity2ROS = Matrix4x4.identity;
        Unity2ROS[0, 0] = -1;
        currentController = rightController; //TODO change this later
        visualMarker.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void SetupRos()
    {
        //TODO revert this to the previous

        //ROSConnection.GetOrCreateInstance().RegisterPublisher<PointMsg>(positionTopicName);
        //InvokeRepeating("GetAndPublishPosition", 0, 2.0f);
        //positionMsg = new PointMsg();
        ROSConnection.GetOrCreateInstance().RegisterPublisher<PoseStampedMsg>(poseTopicName);
        InvokeRepeating("GetAndPublishPose", 0, 0.05f);
        poseMsg = new PoseStampedMsg();
    }

    public void GetAndPublishPosition()
    {
        if (controlPose)
        {
            Transform original = currentController.transform.parent;
            currentController.transform.SetParent(originFrame);

            Vector3 position = currentController.transform.localPosition;
            Matrix4x4 transform = Matrix4x4.TRS(position, Quaternion.identity, Vector3.one);
            var finalTransform = Unity2ROS * transform * Unity2ROS;
            position = finalTransform.GetColumn(3);

            Utils.GetGeometryPoint(position, positionMsg);
            currentController.transform.SetParent(original);

            PublishPositionMsg();
        }
    }

    public void GetAndPublishPose()
    {
        if (controlPose)
        {
            Transform original = currentController.transform.parent;
            currentController.transform.SetParent(originFrame);

            Vector3 scale = new Vector3(0.2f, 0.2f, 0.2f);

            Vector3 position = currentController.transform.localPosition * 0.2f;
            Quaternion rotation = currentController.transform.rotation;
            Matrix4x4 transform = Matrix4x4.TRS(position, rotation, Vector3.one);
            var finalTransform = Unity2ROS * transform * Unity2ROS;

            position = finalTransform.GetColumn(3);
            rotation = finalTransform.rotation;

            Utils.GetGeometryPoint(position, poseMsg.pose.position);
            Utils.GetGeometryQuaternion(rotation, poseMsg.pose.orientation);
            currentController.transform.SetParent(original);
            poseMsg.header.frame_id = "camera_depth_frame";
            PublishPoseMsg();

        }
    }

    private void PublishPositionMsg()
    {
        Debug.Log("Publishing Position:" + positionMsg);
        ROSConnection.GetOrCreateInstance().Publish(positionTopicName, positionMsg);
    }

    private void PublishPoseMsg()
    {
        Debug.Log("Publishing Pose: " + poseMsg);
        ROSConnection.GetOrCreateInstance().Publish(poseTopicName, poseMsg);
    }

    public void TogglePublish()
    {
        controlPose = !controlPose;
        //var renderer = visualMarker.GetComponent<Renderer>();
        if (controlPose)
        {
            //renderer.material.SetColor("_Color", Color.green);
            origin = GameObject.Instantiate(originPrefab, currentController.transform.position, currentController.transform.rotation);
            originFrame = origin.transform;
            visualMarker.SetActive(true);
        } else
        {
            //renderer.material.SetColor("_Color", Color.red);
            GameObject.Destroy(origin);
            visualMarker.SetActive(false);
        }
    }

}
