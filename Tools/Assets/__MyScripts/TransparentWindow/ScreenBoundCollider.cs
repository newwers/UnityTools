using System;
using System.Runtime.InteropServices;
using UnityEngine;
/// <summary>
/// 使用方式创建四个2D或者3D的Cube.然后将此脚本挂载到每个Cube上，设置对应类型为上下左右的边界和厚度即可。
/// </summary>
public class ScreenBoundCollider : MonoBehaviour
{
    public enum ScreenEdge { Top, Bottom, Left, Right }

    [Header("Position Settings")]
    [SerializeField] ScreenEdge edge;
    [SerializeField] float thickness = 0.5f;

    [Header("Position Offset")]
    [SerializeField] Vector2 positionOffset = Vector2.zero; // 新增偏移变量
    [Tooltip("Offset in world units. Use negative values to move inside screen")]

    private Camera mainCam;
    private BoxCollider2D col;
    private int prevWidth, prevHeight;
    private float m_Size;

    void Start()
    {
        mainCam = Camera.main;
        col = GetComponent<BoxCollider2D>();
        prevWidth = Screen.width;
        prevHeight = Screen.height;
        m_Size = mainCam.orthographicSize;
        if (edge == ScreenEdge.Top)
        {
            GameManager.Instance.TopWallOffset = positionOffset.y; // 设置顶部墙壁偏移量
        }
        UpdateCollider();
    }

    private void Update()
    {
        // 只在分辨率变化时更新
        if (Screen.width != prevWidth || Screen.height != prevHeight || m_Size != mainCam.orthographicSize)
        {
            m_Size = mainCam.orthographicSize; // 更新相机大小
            prevWidth = Screen.width;
            prevHeight = Screen.height;
            UpdateCollider();
        }
    }


    // 添加公共方法用于外部调整偏移
    public void SetPositionOffset(Vector2 newOffset)
    {
        positionOffset = newOffset;
        UpdateCollider();
    }

    void UpdateCollider()
    {
        Vector2 camSize = GetCameraSize();
        Vector2 newSize = Vector2.zero;
        Vector2 newPos = Vector2.zero;
        var taskbarHeight = GetTaskBarHeight();
        //print("任务栏像素高度:" + taskbarHeight);

        switch (edge)
        {
            case ScreenEdge.Top:
                newSize = new Vector2(camSize.x * 2, thickness);
                newPos = new Vector2(0, camSize.y + thickness / 2) + positionOffset;
                break;
            case ScreenEdge.Bottom:
                newSize = new Vector2(camSize.x * 2, thickness);
                newPos = new Vector2(0, -camSize.y - thickness / 2) + positionOffset;

                // 新增：应用任务栏高度偏移
                if (taskbarHeight > 0)
                {
                    // 像素转世界单位换算
                    float worldUnitsPerPixel = mainCam.orthographicSize * 2 / Screen.height;
                    float offsetY = taskbarHeight * worldUnitsPerPixel;
                    newPos.y += offsetY; // 向上移动碰撞体
                    //print("任务栏偏移高度:" + offsetY);
                }

                break;
            case ScreenEdge.Left:
                newSize = new Vector2(thickness, camSize.y * 2);
                newPos = new Vector2(-camSize.x - thickness / 2, 0) + positionOffset;
                break;
            case ScreenEdge.Right:
                newSize = new Vector2(thickness, camSize.y * 2);
                newPos = new Vector2(camSize.x + thickness / 2, 0) + positionOffset;
                break;
        }

        col.size = newSize;
        transform.position = (Vector2)mainCam.transform.position + newPos;
    }

    Vector2 GetCameraSize()
    {
        if (!mainCam) mainCam = Camera.main;

        float height = mainCam.orthographicSize;
        float width = height * mainCam.aspect;
        return new Vector2(width, height);
    }

    /// <summary>
        /// 获取任务栏高度
        /// </summary>
        /// <returns>任务栏高度</returns>
    private int GetTaskBarHeight()
    {
        int taskbarHeight = 10;
        IntPtr hWnd = Win32.FindWindow("Shell_TrayWnd", null);       // 找到任务栏窗口
        Win32.RECT rect = new Win32.RECT();
        Win32.GetWindowRect(hWnd, ref rect);                      // 获取任务栏的窗口位置及大小
        taskbarHeight = (int)(rect.Bottom - rect.Top);      // 得到任务栏的高度
        return taskbarHeight;
    }

}

class Win32
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left; // 最左坐标
        public int Top; // 最上坐标
        public int Right; // 最右坐标
        public int Bottom; // 最下坐标
    }

    [DllImport("user32.dll")]
    public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll")]
    public static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);

}