/*
	newwer
*/
using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.EventSystems;


/// <summary>
/// 设置应用程序背景透明,鼠标穿透
/// </summary>
public class TranspareWindows : MonoBehaviour
{
    public int TranspareColor = 0x00080808;
    // 忽略检测的层级设置
    [Header("忽略检测的层级")]
    [Tooltip("勾选的层级将被忽略检测")]
    public LayerMask ignoreLayers;  // 在Inspector面板设置需要忽略的层级

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


    private IntPtr hwnd;
    private static int m_GWL_STYLE;

    public void Awake()
    {
        hwnd = GetActiveWindow();
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
        if (EventSystem.current.IsPointerOverGameObject())
        {
            SetClickThrough(false);
        }
        else
        {
            // 使用层级掩码检测，只检测非忽略层级的物体
            SetClickThrough(Physics2D.OverlapPoint(GetMouseWorldPosition(), ~ignoreLayers.value) == null);
        }
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
        var margins = new MARGINS() { cxLeftWidth = -1 };
        DwmExtendFrameIntoClientArea(hwnd, ref margins);
        SetLayeredWindowAttributes(hwnd, TranspareColor, 255, 2);
    }

    private void SetInitializationWindowPosition()
    {
        //MoveWindow(hwnd, config.InitializationPositionX, config.InitializationPositionY, Screen.width, Screen.height, true);
    }

    public void Wallpaper()
    {
        m_GWL_STYLE = GetWindowLong(hwnd, GWL_STYLE);
        IntPtr hWnd = FindWindow("Shell_TrayWnd", null);
        RECT rect = default(RECT);
        GetWindowRect(hWnd, ref rect);
        if (rect.Height < rect.Width)
        {
            //taskbarHeight = rect.Height;
        }
        SetWindowPos(hwnd, -2, 0, 0, 0, 0, 3U | SWP_FRAMECHANGED);
        ApplyDwmFix(0);
    }

    public void OrdinaryWindow()
    {
        int dwNewLong = m_GWL_STYLE;
        int dwNewLong2 = 256;
        SetWindowLong(hwnd, GWL_STYLE, dwNewLong);
        SetWindowLong(hwnd, GWL_EXSTYLE, dwNewLong2);
        SetParent(hwnd, GetDesktopWindow());
        ApplyDwmFix(1);
    }

    private void ApplyDwmFix(int inset)
    {
        MARGINS margins = new MARGINS
        {
            cxLeftWidth = inset,
            cxRightWidth = inset,
            cyTopHeight = inset,
            cyBottomHeight = inset
        };
        DwmExtendFrameIntoClientArea(hwnd, ref margins);
    }

    private void SetClickThrough(bool SetClickthrouth)
    {
        if (SetClickthrouth)
        {
            SetWindowLong(hwnd, GWL_EXSTYLE, WS_EX_LAYERED | WS_EX_TRANSPARENT);
            //Debug.Log("点击透传已启用");

        }
        else
        {
            SetWindowLong(hwnd, GWL_EXSTYLE, WS_EX_LAYERED);
            //Debug.Log("点击透传已禁用");

        }
    }
}