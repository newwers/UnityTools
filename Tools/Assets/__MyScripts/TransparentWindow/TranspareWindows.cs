/*
	newwer
*/
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


/// <summary>
/// 设置应用程序背景透明,鼠标穿透
/// </summary>
public class TranspareWindows : MonoBehaviour
{
    // 忽略检测的层级设置
    [Header("忽略检测的层级")]
    [Tooltip("勾选的层级将被忽略检测")]
    public LayerMask ignoreLayers;  // 在Inspector面板设置需要忽略的层级
    [Header("UI 所有Canvas上的GraphicRaycaster")]
    [Tooltip("用来检测鼠标是否在UI上,从而关闭鼠标穿透")]
    public List<GraphicRaycaster> raycaster;
    List<RaycastResult> results = new List<RaycastResult>();

    int TranspareColor = 0x00000000;

    private struct MARGINS
    {
        public int cxLeftWidth;
        public int cxRightWidth;
        public int cyTopHeight;
        public int cyBottomHeight;
    }

    public struct RECT
    {
        public int Width
        {
            get
            {
                return this.Right - this.Left;
            }
        }

        public int Height
        {
            get
            {
                return this.Bottom - this.Top;
            }
        }

        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [DllImport("user32.dll")]
    public static extern IntPtr GetDesktopWindow();
    [DllImport("user32.dll")]
    public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);
    [DllImport("user32.dll")]
    private static extern IntPtr FindWindow(string strClassName, string nptWindowName);

    [DllImport("user32.dll")]
    public static extern int GetWindowLong(IntPtr hWnd, int nIndex);
    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("Dwmapi.dll")]
    private static extern uint DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS margins);
    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern int SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int y, int Width, int Height, uint flags);
    [DllImport("user32", EntryPoint = "SetLayeredWindowAttributes")]
    private static extern uint SetLayeredWindowAttributes(IntPtr hwnd, int crKey, int bAlpha, int dwFlags);
    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern int MoveWindow(IntPtr hWnd, int x, int y, int nWidth, int nHeight, bool BRePaint);
    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    // 用于刷新窗口样式的API（关键）
    [DllImport("user32.dll")]
    private static extern bool UpdateWindow(IntPtr hWnd);


    const int GWL_STYLE = -16;
    const int GWL_EXSTYLE = -20;
    const uint WS_POPUP = 0x80000000;
    const uint WS_VISIBLE = 0x10000000;
    const int WS_EX_LAYERED = 0x80000;
    const int WS_EX_TRANSPARENT = 0x00000020;
    const int WS_THICKFRAME = 262144;
    const int WS_BORDER = 8388608;
    public const int WS_CAPTION = 0x00C00000;
    private static uint SWP_FRAMECHANGED = 32U;

        #region 任务栏图标

    // 常量定义
    private const int GWL_EXSTYLE = -20;                     // 扩展样式索引
    private const int WS_EX_APPWINDOW = 0x00040000;         // 强制显示任务栏图标
    private const int WS_EX_TOOLWINDOW = 0x00000080;        // 工具窗口，不显示任务栏图标

    private const uint SWP_NOZORDER = 0x0004;
    private const uint SWP_FRAMECHANGED = 0x0020;
    private const uint SWP_NOACTIVATE = 0x0010;


    #endregion


    private IntPtr hwnd;
    private static int m_GWL_STYLE;

    public void Awake()
    {
        hwnd = FindWindow(null, UnityEngine.Application.productName);
#if !UNITY_EDITOR
        SetWindowTop(true);
        SetWindowTranspareWithSetWindowLong();
        SetWindowPenetration();
        SetWindowTranspareWithSetLayeredWindowAttributes();
        SetInitializationWindowPosition();
#endif
    }


    private void FixedUpdate()
    {
#if UNITY_EDITOR
        return;
#endif

        // 检查UI和2D物体（忽略指定层级）
        if (IsPointerOverUI() /*|| EventSystem.current.IsPointerOverGameObject()*/)
        {
            SetClickThrough(false);
        }
        else
        {
            // 使用层级掩码检测，只检测非忽略层级的物体
            SetClickThrough(Physics2D.OverlapPoint(GetMouseWorldPosition(), ~ignoreLayers.value) == null);
        }
    }
    private bool IsPointerOverUI()
    {
        //LogManager.Log("EventSystem.current: " + EventSystem.current + ",Input.mousePosition:" + Input.mousePosition);
        if (EventSystem.current == null) return false;

        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        results.Clear();
        if (raycaster != null && raycaster.Count > 0)
        {
            for (int i = 0; i < raycaster.Count; i++)
            {
                raycaster[i].Raycast(eventData, results);
                LogManager.Log("检测到UI元素数量: " + results.Count);
                if (results.Count > 0) return true;
            }
        }
        return false;
    }


    public static Vector3 GetMouseWorldPosition()
    {
        Vector3 mouseWorldPositionWithZ = GetMouseWorldPositionWithZ(Input.mousePosition, Camera.main);
        mouseWorldPositionWithZ.z = 0f;
        return mouseWorldPositionWithZ;
    }

    public static Vector3 GetMouseWorldPositionWithZ(Vector3 screenPosition, Camera worldCamera)
    {
        return worldCamera.ScreenToWorldPoint(screenPosition);
    }


    private void SetClickThrough(bool setClickThrough)
    {
        // 1. 获取当前的窗口扩展样式
        int currentExStyle = GetWindowLong(hwnd, GWL_EXSTYLE);

        // 2. 根据开关，添加或移除 WS_EX_TRANSPARENT 样式
        if (setClickThrough)
        {
            // 启用点击穿透：确保 WS_EX_LAYERED 和 WS_EX_TRANSPARENT 被设置
            currentExStyle |= (WS_EX_LAYERED | WS_EX_TRANSPARENT);
        }
        else
        {
            // 禁用点击穿透：移除 WS_EX_TRANSPARENT，但保留 WS_EX_LAYERED
            currentExStyle &= ~WS_EX_TRANSPARENT;
            // 确保 WS_EX_LAYERED 样式始终存在（通常为透明窗口所需）
            currentExStyle |= WS_EX_LAYERED;
        }

        // 3. 应用修改后的样式，不会影响其他样式位（如WS_EX_TOOLWINDOW）
        SetWindowLong(hwnd, GWL_EXSTYLE, currentExStyle);
    }

    public void SetWindowTop(bool isTop)
    {
        if (isTop)
        {
            SetWindowPos(hwnd, -1, 0, 0, 0, 0, 1 | 2);
        }
        else
        {
            SetWindowPos(hwnd, 1, 0, 0, 0, 0, 1 | 2);
        }
    }

    private void SetWindowTranspareWithSetWindowLong()
    {
        int style = GetWindowLong(hwnd, GWL_STYLE);
        SetWindowLong(hwnd, GWL_EXSTYLE, style & ~WS_BORDER & ~WS_THICKFRAME & ~WS_CAPTION);
    }

    private void SetWindowPenetration()
    {
        int style = GetWindowLong(hwnd, GWL_STYLE);
        SetWindowLong(hwnd, GWL_EXSTYLE, style | WS_EX_LAYERED | WS_EX_TRANSPARENT);
    }

    private void SetWindowTranspareWithSetLayeredWindowAttributes()
    {
        // 确保整个窗口区域透明
        var margins = new MARGINS()
        {
            cxLeftWidth = -1,
            cxRightWidth = -1,
            cyTopHeight = -1,
            cyBottomHeight = -1
        };
        DwmExtendFrameIntoClientArea(hwnd, ref margins);
        SetLayeredWindowAttributes(hwnd, TranspareColor, 255, 2);
    }

    private void SetInitializationWindowPosition()
    {
        //MoveWindow(hwnd, config.InitializationPositionX, config.InitializationPositionY, Screen.width, Screen.height, true);
    }


    /// <summary>
    /// 隐藏图标从任务栏移除）
    /// </summary>
    /// <param name="enable"></param>
    public void SetTaskbarIconEnable(bool enable)
    {
        if (enable)
        {
            ShowTaskBarIcon();
        }
        else
        {
            HideTaskBarIcon();
        }
    }

    /// <summary>
    /// show TaskBar
    /// </summary>
    public void ShowTaskBarIcon()
    {
        // 1. 获取当前扩展样式
        int currentExStyle = GetWindowLong(hwnd, GWL_EXSTYLE);

        // 2. 移除工具窗口样式，添加APPWINDOW样式（强制显示任务栏）
        currentExStyle &= ~WS_EX_TOOLWINDOW;
        currentExStyle |= WS_EX_APPWINDOW;

        // 3. 应用新样式
        SetWindowLong(hwnd, GWL_EXSTYLE, currentExStyle);

        // 4. 刷新窗口（关键：Win10/11必须刷新才生效）
        RefreshWindowStyle();
    }
    /// <summary>
    /// Hide TaskBar
    /// </summary>
    public void HideTaskBarIcon()
    {
        // 1. 获取当前扩展样式
        int currentExStyle = GetWindowLong(hwnd, GWL_EXSTYLE);

        // 2. 移除APPWINDOW样式，添加工具窗口样式（隐藏任务栏）
        currentExStyle &= ~WS_EX_APPWINDOW;
        currentExStyle |= WS_EX_TOOLWINDOW;

        // 3. 应用新样式
        SetWindowLong(hwnd, GWL_EXSTYLE, currentExStyle);

        // 4. 刷新窗口（关键：Win10/11必须刷新才生效）
        RefreshWindowStyle();
    }

    /// <summary>
    /// 刷新窗口样式（让修改的扩展样式生效）
    /// </summary>
    private void RefreshWindowStyle()
    {
        // 获取当前窗口位置和大小，避免刷新后位置/大小变化
        RECT windowRect = new RECT();
        GetWindowRect(hwnd, ref windowRect);

        // 调用SetWindowPos刷新样式（SWP_FRAMECHANGED是核心）
        SetWindowPos(
            hwnd,
            0,
            windowRect.Left,
            windowRect.Top,
            windowRect.Width,
            windowRect.Height,
            SWP_NOZORDER | SWP_NOACTIVATE | SWP_FRAMECHANGED
        );

        // 额外更新窗口（兜底）
        UpdateWindow(hwnd);
    }
}