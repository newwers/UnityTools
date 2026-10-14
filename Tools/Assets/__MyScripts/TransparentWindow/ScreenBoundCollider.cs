using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class ScreenBoundCollider : MonoBehaviour
{
    public enum ScreenEdge { Top, Bottom, Left, Right }

    [Header("Position Settings")]
    [SerializeField] ScreenEdge edge;
    [SerializeField] float thickness = 0.5f;

    [Header("Position Offset")]
    [SerializeField] Vector2 positionOffset = Vector2.zero;

    private Camera mainCam;
    private BoxCollider2D col;
    private int prevWidth, prevHeight;
    private float m_Size;
    public Action OnColliderChange;
    public int taskbarInfoOffset = 0;

    // Windows API
    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    [DllImport("user32.dll")]
    private static extern IntPtr FindWindow(string strClassName, string nptWindowName);

    [DllImport("user32.dll")]
    private static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

    private const uint MONITOR_DEFAULTTONEAREST = 2;

    private void Awake()
    {
        mainCam = Camera.main;
        col = GetComponent<BoxCollider2D>();
        prevWidth = Screen.width;
        prevHeight = Screen.height;
        m_Size = mainCam.orthographicSize;
        UpdateCollider();
    }

    private void Update()
    {
        if (Screen.width != prevWidth || Screen.height != prevHeight || m_Size != mainCam.orthographicSize)//|| m_Size != mainCam.orthographicSize
        {
            m_Size = mainCam.orthographicSize;
            prevWidth = Screen.width;
            prevHeight = Screen.height;
            UpdateCollider();
        }

    }

    public void SetPositionOffset(Vector2 newOffset)
    {
        positionOffset = newOffset;
        UpdateCollider();
    }

    public void UpdateCollider()
    {
        Vector2 camSize = GetCameraSize();
        Vector2 newSize = Vector2.zero;
        Vector2 newPos = Vector2.zero;

        // 获取当前显示器的任务栏信息
        var taskbarInfo = GetCurrentMonitorTaskbarInfo();

        switch (edge)
        {
            case ScreenEdge.Top:
                newSize = new Vector2(camSize.x * 2, thickness);
                newPos = new Vector2(0, camSize.y + thickness / 2) + positionOffset;
                break;

            case ScreenEdge.Bottom:
                newSize = new Vector2(camSize.x * 2, thickness);
                newPos = new Vector2(0, -camSize.y - thickness / 2) + positionOffset;

                // 如果任务栏在底部，应用偏移
                //LogManager.Log($"taskbarInfo height: {taskbarInfo.height}px, isAtBottom: {taskbarInfo.isAtBottom}");
                //if (taskbarInfo is { isAtBottom: true, height: >= 0 })
                {
                    float worldUnitsPerPixel = mainCam.orthographicSize * 2 / Screen.height;
                    float offsetY = (taskbarInfo.height + taskbarInfoOffset) * worldUnitsPerPixel;
                    newPos.y += offsetY;
                    LogManager.Log($"任务栏高度: {taskbarInfo.height}px, 应用偏移: {offsetY} world units,newpos:{newPos}");
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

        if (col)
        {
            col.size = newSize;
        }
        transform.position = new Vector2(transform.position.x, mainCam.transform.position.y + newPos.y);
        OnColliderChange?.Invoke();
    }

    Vector2 GetCameraSize()
    {
        float height = mainCam.orthographicSize;
        float width = height * mainCam.aspect;
        return new Vector2(width, height);
    }

    /// <summary>
    /// 任务栏信息结构体
    /// </summary>
    public struct TaskbarInfo
    {
        public int height;
        public int width;
        public bool isAtTop;
        public bool isAtBottom;
        public bool isAtLeft;
        public bool isAtRight;
    }

    /// <summary>
    /// 获取当前显示器任务栏信息（多显示器支持）
    /// </summary>
    private TaskbarInfo GetCurrentMonitorTaskbarInfo()
    {
        TaskbarInfo info = new TaskbarInfo();

        try
        {
#if !UNITY_EDITOR
            // 获取当前活动窗口（Unity窗口）
            IntPtr hWnd = FindWindow(null, Application.productName);
            if (hWnd == IntPtr.Zero) return info;
            
            // 获取当前窗口所在的显示器
            IntPtr hMonitor = MonitorFromWindow(hWnd, MONITOR_DEFAULTTONEAREST);
            
            // 获取显示器信息
            MONITORINFO monitorInfo = new MONITORINFO();
            monitorInfo.cbSize = Marshal.SizeOf(typeof(MONITORINFO));
            
            if (GetMonitorInfo(hMonitor, ref monitorInfo))
            {
                int screenWidth = monitorInfo.rcMonitor.Right - monitorInfo.rcMonitor.Left;
                int screenHeight = monitorInfo.rcMonitor.Bottom - monitorInfo.rcMonitor.Top;
                int workWidth = monitorInfo.rcWork.Right - monitorInfo.rcWork.Left;
                int workHeight = monitorInfo.rcWork.Bottom - monitorInfo.rcWork.Top;
                
                // 判断任务栏位置并计算大小
                if (monitorInfo.rcWork.Top > monitorInfo.rcMonitor.Top)
                {
                    // 任务栏在顶部
                    info.height = monitorInfo.rcWork.Top - monitorInfo.rcMonitor.Top;
                    info.isAtTop = true;
                }
                else if (monitorInfo.rcWork.Left > monitorInfo.rcMonitor.Left)
                {
                    // 任务栏在左侧
                    info.width = monitorInfo.rcWork.Left - monitorInfo.rcMonitor.Left;
                    info.isAtLeft = true;
                }
                else if (monitorInfo.rcWork.Right < monitorInfo.rcMonitor.Right)
                {
                    // 任务栏在右侧
                    info.width = monitorInfo.rcMonitor.Right - monitorInfo.rcWork.Right;
                    info.isAtRight = true;
                }
                else
                {
                    // 任务栏在底部（最常见情况）
                    info.height = monitorInfo.rcMonitor.Bottom - monitorInfo.rcWork.Bottom;
                    info.isAtBottom = true;
                }
            }
#endif
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"获取任务栏信息失败: {ex.Message}");
            // 失败时回退到简单方法
            info = GetTaskbarInfoFallback();
        }

        return info;
    }

    /// <summary>
    /// 备用方法：使用简单的工作区域计算
    /// </summary>
    private TaskbarInfo GetTaskbarInfoFallback()
    {
        TaskbarInfo info = new TaskbarInfo();

        try
        {
            // 使用System.Windows.Forms（如果可用）
#if !UNITY_EDITOR
            if (System.Windows.Forms.Screen.AllScreens.Length > 0)
            {
                var currentScreen = FindCurrentScreen();
                if (currentScreen != null)
                {
                    var bounds = currentScreen.Bounds;
                    var workArea = currentScreen.WorkingArea;
                    
                    if (workArea.Top > bounds.Top)
                    {
                        info.height = workArea.Top - bounds.Top;
                        info.isAtTop = true;
                    }
                    else if (workArea.Left > bounds.Left)
                    {
                        info.width = workArea.Left - bounds.Left;
                        info.isAtLeft = true;
                    }
                    else if (workArea.Right < bounds.Right)
                    {
                        info.width = bounds.Right - workArea.Right;
                        info.isAtRight = true;
                    }
                    else
                    {
                        info.height = bounds.Bottom - workArea.Bottom;
                        info.isAtBottom = true;
                    }
                }
            }
#endif
        }
        catch
        {
            // 如果所有方法都失败，使用默认值
            info.height = 40; // 默认任务栏高度
            info.isAtBottom = true;
        }

        return info;
    }

    /// <summary>
    /// 查找当前Unity窗口所在的屏幕
    /// </summary>
    private System.Windows.Forms.Screen FindCurrentScreen()
    {
#if !UNITY_EDITOR
        foreach (var screen in System.Windows.Forms.Screen.AllScreens)
        {
            // 简单的匹配逻辑：使用屏幕分辨率匹配
            if (screen.Bounds.Width == Screen.width && screen.Bounds.Height == Screen.height)
            {
                return screen;
            }
        }
#endif
        return System.Windows.Forms.Screen.PrimaryScreen;
    }


}

[StructLayout(LayoutKind.Sequential)]
public struct RECT
{
    public int Left;
    public int Top;
    public int Right;
    public int Bottom;

    public int Width => Right - Left;
    public int Height => Bottom - Top;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
public struct MONITORINFO
{
    public int cbSize;
    public RECT rcMonitor;
    public RECT rcWork;
    public uint dwFlags;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
    public string szDevice;
}