using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RosMessageTypes.Sensor;

public class PointCloudHandler : MonoBehaviour
{
    public Material pointCloudMaterial; //Shader for the Point Cloud
    [Tooltip("The scale for translating pixel distance to real-world coordinates. Max unsigned 16 bit int = 65535, divide by 1000 to convert to meter")]
    public float scale = 65.535f;
    [Tooltip("Specify the new width and height if cropping image is handled by Unity")]
    public int newWidth = 640, newHeight = 480;

    public bool updateTransform = false, unityCrop = false, compress = false;

    private CameraInfoMsg cameraIntrinsic;
    // focal length int x y axis and center pixel in x y axis
    private float fx, fy, cx, cy;
    private bool rectify;
    private int width, height, pixelTotal, srcX, srcY, x_offset, y_offset;
    private Texture2D  colorTex, depthTex;

    // Start is called before the first frame update
    void Start()
    {
        pointCloudMaterial.SetFloat("_DepthScale", scale);
        var m = Matrix4x4.TRS(this.transform.position, this.transform.rotation, this.transform.localScale);
        pointCloudMaterial.SetMatrix("transformationMatrix", m);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Called once the ros subscriber receives the camera intrinsics. Sets the variables and camera parameters needs for point cloud rendering.
    /// </summary>
    /// <param name="intrinsic"></param>
    public void SetCameraIntrinsicAndSetupPC(CameraInfoMsg intrinsic)
    {
        cameraIntrinsic = intrinsic;
        rectify = cameraIntrinsic.roi.do_rectify;

        width = (int)cameraIntrinsic.width;
        height = (int)cameraIntrinsic.height;

        if (rectify)
        {
            newWidth = (int)cameraIntrinsic.roi.width;
            newHeight = (int)cameraIntrinsic.roi.height;
            x_offset = (int)cameraIntrinsic.roi.x_offset;
            y_offset = (int)cameraIntrinsic.roi.y_offset;
        }

        fx = (float)cameraIntrinsic.k[0];
        fy = (float)cameraIntrinsic.k[4];
        cx = (float)cameraIntrinsic.k[2];
        cy = (float)cameraIntrinsic.k[5];
        Debug.Log($"fx: {fx}, fy: {fy}, cx: {cx}, cy: {cy}");

        //If image rectify was handled by ROS
        if (rectify)
        {
            colorTex = new Texture2D(2, 2);
            depthTex = new Texture2D(newWidth, newHeight, TextureFormat.R16, false);
            pointCloudMaterial.SetVector("_CameraIntrinsic", new Vector4(cx - x_offset, cy - y_offset, fx, fy));
            pointCloudMaterial.SetInt("_Width", newWidth);
            pointCloudMaterial.SetInt("_Height", newHeight);
            pixelTotal = newWidth * newHeight;
        }
        //Default original option where image has not been rectified
        else
        {
            colorTex = new Texture2D(2, 2); //For Compressed images
            //colorTex = new Texture2D(width, height, TextureFormat.RGB24, false);
            depthTex = new Texture2D(width, height, TextureFormat.R16, false);
            pointCloudMaterial.SetVector("_CameraIntrinsic", new Vector4(cx, cy, fx, fy));
            pointCloudMaterial.SetInt("_Width", width);
            pointCloudMaterial.SetInt("_Height", height);
            pixelTotal = width * height;
        }

        pointCloudMaterial.SetTexture("_ColorTex", colorTex);
        pointCloudMaterial.SetTexture("_DepthTex", depthTex);

    }

    public void UpdateColorTex(byte[] colorData)
    {
        colorTex.LoadRawTextureData(colorData);
        colorTex.Apply();
    }

    public void UpdateColorTexComp(CompressedImageMsg colorImg)
    {
        colorTex.LoadImage(colorImg.data);
    }

    public void UpdateDepthTex(byte[] depthData)
    {
        depthTex.LoadRawTextureData(depthData);
        depthTex.Apply();
    }

    public void UpdateTransform()
    {
        var m = Matrix4x4.TRS(this.transform.position, this.transform.rotation, this.transform.localScale);
        pointCloudMaterial.SetMatrix("transformationMatrix", m);
    }


    /// <summary>
    /// Render point cloud during rendering pipeline.
    /// </summary>
    void OnRenderObject()
    {
        pointCloudMaterial.SetPass(0);
        if (updateTransform)
        {
            var m = Matrix4x4.TRS(this.transform.position, this.transform.rotation, this.transform.localScale);
            pointCloudMaterial.SetMatrix("transformationMatrix", m);
        }

        Graphics.DrawProceduralNow(MeshTopology.Points, pixelTotal, 1);
    }

}
