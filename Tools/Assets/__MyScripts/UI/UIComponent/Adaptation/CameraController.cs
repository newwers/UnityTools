using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 让透视相机,按照目标分辨率进行设置size,保证相机显示区域在不同分辨率下一致,可以搭配 TransformAdaptation 或者 CanvasScalerAdaptation 
/// 对3D物体或者UI进行适配
/// </summary>
public class CameraController : MonoBehaviour
{
    public Camera cam;

    void Awake()
    {
        if (cam == null)
        {
            cam = GetComponent<Camera>();
        }
        print($"分辨率:{Screen.width},{Screen.height}");

        // 原始分辨率下的orthographicSize值
        float originalSize = cam.orthographicSize;

        // 原始分辨率
        int originalWidth = 720;
        int originalHeight = 1280;

        // 当前分辨率
        int currentWidth = Screen.width;
        int currentHeight = Screen.height;

        // 计算新的orthographicSize
        float newSize = originalSize *  (originalWidth / (float)originalHeight) / (currentWidth / (float)currentHeight);

        // 设置新的orthographicSize
        cam.orthographicSize = newSize;
    }

}
