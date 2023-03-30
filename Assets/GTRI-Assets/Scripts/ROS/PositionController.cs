using System.Collections;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Geometry;
public class PositionController : MonoBehaviour
{
    public GameObject rightController;
    public GameObject leftController;
    [Tooltip("The frame to publish the pose in")]
    public Transform originFrame; //Publish the pose with respect to this Transform

    private Matrix4x4 Unity2ROS;
    private bool controlPose;
    private GameObject currentController;

    //ROS Stuff
    public string positionTopicName = "unity_position";
    private PointMsg positionMsg;

    //Debugging Stuff

    public GameObject visualMarker;

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
        ROSConnection.GetOrCreateInstance().RegisterPublisher<PointMsg>(positionTopicName);
        InvokeRepeating("GetAndPublishPosition", 0, 5-.0f);
        positionMsg = new PointMsg();
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

    private void PublishPositionMsg()
    {
        Debug.Log("Publishing Position:" + positionMsg);
        ROSConnection.GetOrCreateInstance().Publish(positionTopicName, positionMsg);
    }

    public void TogglePublish()
    {
        controlPose = !controlPose;
        //var renderer = visualMarker.GetComponent<Renderer>();
        if (controlPose)
        {
            //renderer.material.SetColor("_Color", Color.green);
            visualMarker.SetActive(true);
        } else
        {
            //renderer.material.SetColor("_Color", Color.red);
            visualMarker.SetActive(false);
        }
    }

}
