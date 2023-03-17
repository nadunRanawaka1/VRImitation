using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;


/// <summary>
/// This script subscribes to the camera topics on the ROS PC. It will then call the callbacks in another script [Insert script here] to render the pointcloud.
/// </summary>
public class CameraSubscriber : MonoBehaviour
{
    public string cameraInfoTopic, colorTopic, depthTopic;
    public PointCloudHandler pcHandler;


    private bool isIntrinsicReceived = false;
    // Start is called before the first frame update
    void Start()
    {
        ROSConnection.GetOrCreateInstance().Subscribe<CameraInfoMsg>(cameraInfoTopic, HandleCameraInfo);
        

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void HandleCameraInfo(CameraInfoMsg cameraInfo)
    {
        if (!isIntrinsicReceived)
        {
            Debug.Log("ROS depth camera intrinsic received");
            Debug.Log(cameraInfo.ToString());
            // TODO call function in rendering script to set the required parameters.
            isIntrinsicReceived = true;
            pcHandler.SetCameraIntrinsicAndSetupPC(cameraInfo);
            // Only subscribe to image topics after the intrinsics are received.
            ROSConnection.GetOrCreateInstance().Subscribe<CompressedImageMsg>(colorTopic, HandleRGBImage);
            ROSConnection.GetOrCreateInstance().Subscribe<ImageMsg>(depthTopic, HandleDepthImage);
        }
    }

    private void HandleRGBImage(CompressedImageMsg colorImage)
    {
        //TODO call function in rendering script to handle color image. 
        pcHandler.UpdateColorTexComp(colorImage);
    }

    private void HandleDepthImage(ImageMsg depthImage)
    {
        //TODO call function in rendering script to handle depth image. 
        pcHandler.UpdateDepthTex(depthImage.data);
    }
}
