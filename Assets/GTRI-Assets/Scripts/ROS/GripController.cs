using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std;

public class GripController : MonoBehaviour
{
    public string topicName = "robot_gripper_state";
    private sbyte gripperState = -1;
    private Int8Msg gripperMsg = new Int8Msg();

    // Start is called before the first frame update
    void Start()
    {
        ROSConnection.GetOrCreateInstance().RegisterPublisher<Int8Msg>(topicName);
        gripperMsg.data = gripperState;
        ROSConnection.GetOrCreateInstance().Publish(topicName, gripperMsg);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ToggleGripper()
    {
        gripperState = (sbyte) -gripperState;
        gripperMsg.data = gripperState;
        ROSConnection.GetOrCreateInstance().Publish(topicName, gripperMsg);

    }
}
