using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 让透视相机,按照目标分辨率进行设置size,保证相机显示区域在不同分辨率下一致,可以搭配 TransformAdaptation 或者 CanvasScalerAdaptation 
/// 对3D物体或者UI进行适配
/// </summary>
public class CameraAdaptation : MonoBehaviour
{
    public Camera cam;
    public Vector2 ScreenSize = new Vector2(720,1280);

    void Awake()
    {
        if (cam == null)
        {
            cam = GetComponent<Camera>();
        }
        AdjustVoewport();
    }

    void AdjustVoewport()
    {
        if (cam == null || cam.orthographic == false)//不是正交相机过滤
        {
            Debug.LogError("当前相机需要设置为 orthographic 才支持设置屏幕分辨率自适应!");
            return;
        }
        print($"分辨率:{Screen.width},{Screen.height}");

        // 原始分辨率下的orthographicSize值
        float originalSize = cam.orthographicSize;

        // 原始分辨率
        int originalWidth = (int)ScreenSize.x;
        int originalHeight = (int)ScreenSize.y;

        // 当前分辨率
        int currentWidth = Screen.width;
        int currentHeight = Screen.height;

        // 计算新的orthographicSize
        float newSize = originalSize * (originalWidth / (float)originalHeight) / (currentWidth / (float)currentHeight);

        // 设置新的orthographicSize
        cam.orthographicSize = newSize;
    }

}
