using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ��͸�����,����Ŀ��ֱ��ʽ�������size,��֤�����ʾ�����ڲ�ͬ�ֱ�����һ��,���Դ��� TransformAdaptation ���� CanvasScalerAdaptation 
/// ��3D�������UI��������
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
        print($"�ֱ���:{Screen.width},{Screen.height}");

        // ԭʼ�ֱ����µ�orthographicSizeֵ
        float originalSize = cam.orthographicSize;

        // ԭʼ�ֱ���
        int originalWidth = 720;
        int originalHeight = 1280;

        // ��ǰ�ֱ���
        int currentWidth = Screen.width;
        int currentHeight = Screen.height;

        // �����µ�orthographicSize
        float newSize = originalSize *  (originalWidth / (float)originalHeight) / (currentWidth / (float)currentHeight);

        // �����µ�orthographicSize
        cam.orthographicSize = newSize;
    }

}
