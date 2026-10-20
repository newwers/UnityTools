using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

/// <summary>
/// 顶层窗口信息（屏幕矩形为物理像素，原点在左上角，Y轴向下）
/// </summary>
public struct WindowInfo
{
    public IntPtr hWnd;
    public string title;
    public RectInt screenRect;
    /// <summary>是否为最大化或全屏窗口</summary>
    public bool isFullscreen;
}

/// <summary>
/// 枚举 Windows 桌面上可见的顶层窗口信息（句柄、标题、屏幕矩形）
/// </summary>
public static class WindowEnumerator
{
    #region Win32 API

    [DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);//枚举所有顶层窗口
    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr hWnd);//判断窗口是否可见

    [DllImport("user32.dll")]
    private static extern bool IsIconic(IntPtr hWnd);//判断窗口是否最小化

    [DllImport("user32.dll")]
    private static extern bool IsZoomed(IntPtr hWnd);//判断窗口是否最大化

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowTextW(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll", EntryPoint = "GetWindowLongW")]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("dwmapi.dll")]
    private static extern int DwmGetWindowAttribute(IntPtr hwnd, int dwAttribute, out int pvAttribute, int cbAttribute);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr FindWindowW(string lpClassName, string lpWindowName);

    private const int GWL_EXSTYLE = -20;
    private const int WS_EX_TOOLWINDOW = 0x00000080;
    private const int DWMWA_CLOAKED = 14;

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    #endregion

    /// <summary>
    /// 获取当前可见的顶层窗口列表（排除自身、最小化、cloaked、工具窗口）
    /// </summary>
    public static List<WindowInfo> GetVisibleWindows(IntPtr excludeHwnd)
    {
        List<WindowInfo> windows = new List<WindowInfo>();
        EnumWindows((hWnd, lParam) =>
        {
            if (hWnd == excludeHwnd) return true;
            if (!IsWindowVisible(hWnd)) return true;
            if (IsIconic(hWnd)) return true;

            // cloaked 窗口（UWP挂起等）IsWindowVisible 为 true 但实际不可见
            if (DwmGetWindowAttribute(hWnd, DWMWA_CLOAKED, out int cloaked, sizeof(int)) == 0 && cloaked != 0)
            {
                return true;
            }

            // 工具窗口（提示气泡、悬浮小部件）
            if ((GetWindowLong(hWnd, GWL_EXSTYLE) & WS_EX_TOOLWINDOW) != 0)
            {
                return true;
            }

            if (!GetWindowRect(hWnd, out RECT rect)) return true;
            int width = rect.Right - rect.Left;
            int height = rect.Bottom - rect.Top;
            if (width <= 0 || height <= 0) return true;

            // 最大化（IsZoomed）或矩形覆盖整个主屏幕（无边框全屏窗口）
            bool isFullscreen = IsZoomed(hWnd) ||
                (width >= Display.main.systemWidth && height >= Display.main.systemHeight);

            StringBuilder titleBuilder = new StringBuilder(256);
            GetWindowTextW(hWnd, titleBuilder, 256);

            windows.Add(new WindowInfo
            {
                hWnd = hWnd,
                title = titleBuilder.ToString(),
                screenRect = new RectInt(rect.Left, rect.Top, width, height),
                isFullscreen = isFullscreen
            });
            return true;
        }, IntPtr.Zero);
        return windows;
    }

    /// <summary>
    /// 获取本程序窗口句柄（优先使用 Wallpaper 缓存，兜底按产品名查找）
    /// </summary>
    public static IntPtr GetSelfWindowHandle()
    {
        IntPtr hwnd = Wallpaper.unityHwnd;
        if (hwnd == IntPtr.Zero)
        {
            hwnd = FindWindowW(null, Application.productName);
        }
        return hwnd;
    }
}
