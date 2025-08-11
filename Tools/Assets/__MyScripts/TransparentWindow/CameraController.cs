using UnityEngine;
using UnityEngine.U2D;

public class CameraController : MonoBehaviour
{
    public Camera cam;
    public Color transpareColor;
    public PixelPerfectCamera pixelPerfectCamera;

    int m_BaseMagnification = 2; // 基础放大倍数，假设为2倍

    private void Awake()
    {
#if !UNITY_EDITOR
        cam.backgroundColor = transpareColor;
#endif

        //根据屏幕分辨率设置相机的像素完美
        if (pixelPerfectCamera != null)
        {
            pixelPerfectCamera.assetsPPU = 48; // 假设游戏设计为9:16的纵向屏幕

            if (Screen.width == 1920 && Screen.height == 1080)
            {
                pixelPerfectCamera.refResolutionX = (int)(Screen.width / 2f);
                pixelPerfectCamera.refResolutionY = (int)(Screen.height / 2f);
                LogManager.Log("屏幕1920*1080 放大2");
            }
            else
            {
                pixelPerfectCamera.refResolutionX = Screen.width / m_BaseMagnification; // 设置参考分辨率宽度
                pixelPerfectCamera.refResolutionY = Screen.height / m_BaseMagnification; // 设置参考分辨率高度
            }
        }
    }

    public void SetMagnification(int mul)
    {
        pixelPerfectCamera.refResolutionX = Screen.width / (m_BaseMagnification + mul - 1); // 设置参考分辨率宽度
        pixelPerfectCamera.refResolutionY = Screen.height / (m_BaseMagnification + mul - 1); // 设置参考分辨率高度
    }
}
