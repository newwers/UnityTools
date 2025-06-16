using System;
using System.Runtime.InteropServices;
//using Framework;
using UnityEngine;

// Token: 0x0200000C RID: 12
public class WindowController : MonoBehaviour
{
    // Token: 0x0600004C RID: 76
    [DllImport("user32.dll")]
    private static extern IntPtr FindWindow(string strClassName, string nptWindowName);

    // Token: 0x0600004D RID: 77
    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, ref WindowController.RECT lpRect);

    // Token: 0x0600004E RID: 78
    [DllImport("user32.dll")]
    private static extern bool GetClientRect(IntPtr hWnd, ref WindowController.RECT lpRect);

    // Token: 0x0600004F RID: 79
    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    // Token: 0x06000050 RID: 80
    [DllImport("user32.dll")]
    private static extern IntPtr SetWindowLong(IntPtr hwnd, int _nIndex, ulong dwNewLong);

    // Token: 0x06000051 RID: 81
    [DllImport("user32.dll")]
    public static extern uint GetWindowLong(IntPtr hWnd, int nIndex);

    // Token: 0x06000052 RID: 82
    [DllImport("user32.dll")]
    private static extern bool ReleaseCapture();

    // Token: 0x06000053 RID: 83
    [DllImport("user32.dll")]
    private static extern bool SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);

    // Token: 0x06000054 RID: 84
    [DllImport("user32.dll")]
    private static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

    // Token: 0x06000055 RID: 85
    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern bool GetMonitorInfo(IntPtr hMonitor, ref WindowController.MONITORINFO lpmi);

    // Token: 0x06000056 RID: 86
    [DllImport("user32.dll")]
    public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    // Token: 0x06000057 RID: 87
    [DllImport("user32.dll")]
    public static extern bool SetActiveWindow(IntPtr hWnd);

    // Token: 0x06000058 RID: 88
    [DllImport("user32.dll")]
    public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

    // Token: 0x06000059 RID: 89
    [DllImport("user32.dll")]
    public static extern IntPtr GetDesktopWindow();

    // Token: 0x0600005A RID: 90
    [DllImport("dwmapi.dll")]
    private static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref WindowController.MARGINS pMarInset);

    // Token: 0x0600005B RID: 91 RVA: 0x000034A4 File Offset: 0x000016A4
    public static WindowController Instance()
    {
        if (!Application.isPlaying)
        {
            return null;
        }
        if (WindowController.instance == null)
        {
            GameObject gameObject = new GameObject("WindowController");
            UnityEngine.Object.DontDestroyOnLoad(gameObject);
            WindowController.instance = gameObject.AddComponent<WindowController>();
            //WindowController.windowsDrag = gameObject.AddComponent<WindowsDrag>();
        }
        return WindowController.instance;
    }

    // Token: 0x0600005C RID: 92 RVA: 0x000034F4 File Offset: 0x000016F4
    private void Start()
    {
        WindowController.myintptr = WindowController.FindWindow("UnityWndClass", "Fischer'sFishingJourney");
        if (WindowController.myintptr == IntPtr.Zero)
        {
            return;
        }
        this.newWndProc = new WindowController.WndProcDelegate(this.WindowProc);
        this.oldWndProc = WindowController.SetWindowLongPtr(WindowController.myintptr, -4, this.newWndProc);
        //WindowController.SetWindowPos(WindowController.myintptr, 0, Main.model.settingModel.windowInfo.posX, Main.model.settingModel.windowInfo.posY, Main.model.settingModel.windowInfo.width - 16, Main.model.settingModel.windowInfo.height - 9, WindowController.SWP_FRAMECHANGED | WindowController.SWP_SHOWWINDOW | 4U);
        base.Invoke("SetWindowState", 0.1f);
    }

    // Token: 0x0600005D RID: 93 RVA: 0x000035D0 File Offset: 0x000017D0
    private void OnDisable()
    {
        //if (Main.model.settingModel.windowState == WindowState.Normal)
        {
            WindowController.SaveWindowInfo();
        }
    }

    // Token: 0x0600005E RID: 94 RVA: 0x000035E8 File Offset: 0x000017E8
    private void SetWindowState()
    {
        //if (Main.model.settingModel.windowState == WindowState.Normal || Main.model.settingModel.windowState == WindowState.TablePet)
        //{
        //    this.OrdinaryWindow();
        //    Main.model.settingModel.windowState = WindowState.Normal;
        //}
        //else if (Main.model.settingModel.windowState == WindowState.FullScreen)
        //{
        //    this.FullScreen();
        //}
        //else if (Main.model.settingModel.windowState == WindowState.Wallpaper)
        //{
        //    this.Wallpaper();
        //}
        //Main.evtMgr.Send(EventType.OnWindowStateChange, null);
    }

    // Token: 0x0600005F RID: 95 RVA: 0x00003670 File Offset: 0x00001870
    private void CameraAdjust()
    {
        //if (Main.model.settingModel.windowState == WindowState.TablePet || Main.model.settingModel.windowState == WindowState.TablePet)
        //{
        //    return;
        //}
        if ((float)Screen.height != this.lastHeight || (float)Screen.width != this.lastWidth)
        {
            Camera.main.orthographicSize = this.size * 1.7777778f * ((float)Screen.height / (float)Screen.width);
        }
        this.lastWidth = (float)Screen.width;
        this.lastHeight = (float)Screen.height;
    }

    // Token: 0x06000060 RID: 96 RVA: 0x000036FC File Offset: 0x000018FC
    public void OrdinaryWindow()
    {
        WindowController.isFrameLatest = false;
        //ulong dwNewLong = (ulong)-1777991680;
        //ulong dwNewLong2 = 256UL;
        //WindowController.SetWindowLong(WindowController.myintptr, WindowController.GWL_STYLE, dwNewLong);
        //WindowController.SetWindowLong(WindowController.myintptr, WindowController.GWL_EXSTYLE, dwNewLong2);
        //if (Main.model.settingModel.windowInfo.Top)
        //{
        //    WindowController.SetWindowPos(WindowController.myintptr, -1, Main.model.settingModel.windowInfo.posX, Main.model.settingModel.windowInfo.posY, Main.model.settingModel.windowInfo.width, Main.model.settingModel.windowInfo.height, WindowController.SWP_FRAMECHANGED | WindowController.SWP_SHOWWINDOW);
        //    if (Main.model.settingModel.windowState == WindowState.Wallpaper)
        //    {
        //        WindowController.SetParent(WindowController.myintptr, WindowController.GetDesktopWindow());
        //    }
        //}
        //else
        //{
        //    WindowController.SetWindowPos(WindowController.myintptr, 0, Main.model.settingModel.windowInfo.posX, Main.model.settingModel.windowInfo.posY, Main.model.settingModel.windowInfo.width, Main.model.settingModel.windowInfo.height, WindowController.SWP_FRAMECHANGED | WindowController.SWP_SHOWWINDOW | 4U);
        //    if (Main.model.settingModel.windowState == WindowState.Wallpaper)
        //    {
        //        WindowController.SetParent(WindowController.myintptr, WindowController.GetDesktopWindow());
        //    }
        //}
        //WindowController.windowsDrag.enabled = true;
        this.ApplyDwmFix(1);
    }

    // Token: 0x06000061 RID: 97 RVA: 0x00003888 File Offset: 0x00001A88
    private void GetFrameSize()
    {
        WindowController.GetWindowRect(WindowController.myintptr, ref WindowController.rect);
        WindowController.frameWidth = WindowController.rect.Width - Screen.width;
        WindowController.frameHeight = WindowController.rect.Height - Screen.height;
        WindowController.isFrameLatest = true;
        //Log.Info(string.Format("frameWidth:{0}, frameHeight:{1}", WindowController.frameWidth, WindowController.frameHeight), false);
    }

    // Token: 0x06000062 RID: 98 RVA: 0x000038FC File Offset: 0x00001AFC
    public void FullScreen()
    {
        //WindowController.SetWindowLong(WindowController.myintptr, WindowController.GWL_STYLE, (ulong)-1778384896);
        WindowController.SetWindowLong(WindowController.myintptr, WindowController.GWL_EXSTYLE, 0UL);
        WindowController.RECT currentScreenInfo = this.GetCurrentScreenInfo();
        WindowController.SetWindowPos(WindowController.myintptr, -2, currentScreenInfo.Left, currentScreenInfo.Top, currentScreenInfo.Width, currentScreenInfo.Height, WindowController.SWP_SHOWWINDOW | WindowController.SWP_FRAMECHANGED);
        //WindowController.windowsDrag.enabled = false;
        this.ApplyDwmFix(0);
    }

    // Token: 0x06000063 RID: 99 RVA: 0x0000397C File Offset: 0x00001B7C
    public void Top(bool isTop)
    {
        if (isTop)
        {
            WindowController.SetWindowPos(WindowController.myintptr, -1, 0, 0, 0, 0, WindowController.SWP_SHOWWINDOW | 2U | 1U);
            return;
        }
        WindowController.SetWindowPos(WindowController.myintptr, -2, 0, 0, 0, 0, WindowController.SWP_SHOWWINDOW | 2U | 1U);
    }

    // Token: 0x06000064 RID: 100 RVA: 0x000039B8 File Offset: 0x00001BB8
    public void Wallpaper()
    {
        IntPtr hWnd = WindowController.FindWindow("Shell_TrayWnd", null);
        WindowController.RECT rect = default(WindowController.RECT);
        WindowController.GetWindowRect(hWnd, ref rect);
        if (rect.Height < rect.Width)
        {
            WindowController.taskbarHeight = rect.Height;
        }
        WindowController.SetWindowPos(WindowController.myintptr, -2, 0, 0, 0, 0, 3U | WindowController.SWP_FRAMECHANGED);
        //DesktopWallpaper.Instance().gameObject.SetActive(true);
        //WindowController.windowsDrag.enabled = false;
        this.ApplyDwmFix(0);
    }

    // Token: 0x06000065 RID: 101 RVA: 0x00003A38 File Offset: 0x00001C38
    public static void SaveWindowInfo()
    {
        WindowController.GetWindowRect(WindowController.myintptr, ref WindowController.rect);
        //Main.model.settingModel.windowInfo.width = WindowController.rect.Width;
        //Main.model.settingModel.windowInfo.height = WindowController.rect.Height;
        //Main.model.settingModel.windowInfo.posX = WindowController.rect.Left;
        //Main.model.settingModel.windowInfo.posY = WindowController.rect.Top;
        //Debug.Log(Main.model.settingModel.windowInfo.width.ToString() + ", " + Main.model.settingModel.windowInfo.height.ToString());
    }

    // Token: 0x06000066 RID: 102 RVA: 0x00003B10 File Offset: 0x00001D10
    public WindowController.RECT GetCurrentScreenInfo()
    {
        IntPtr hMonitor = WindowController.MonitorFromWindow(WindowController.myintptr, 2U);
        WindowController.MONITORINFO monitorinfo = default(WindowController.MONITORINFO);
        monitorinfo.cbSize = Marshal.SizeOf(typeof(WindowController.MONITORINFO));
        WindowController.GetMonitorInfo(hMonitor, ref monitorinfo);
        return monitorinfo.rcMonitor;
    }

    // Token: 0x06000067 RID: 103 RVA: 0x00003B54 File Offset: 0x00001D54
    private void ApplyDwmFix(int inset)
    {
        WindowController.MARGINS margins = new WindowController.MARGINS
        {
            left = inset,
            right = inset,
            top = inset,
            bottom = inset
        };
        WindowController.DwmExtendFrameIntoClientArea(WindowController.myintptr, ref margins);
    }

    // Token: 0x06000068 RID: 104
    [DllImport("user32.dll")]
    private static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    // Token: 0x06000069 RID: 105
    [DllImport("user32.dll")]
    private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, WindowController.WndProcDelegate newProc);

    // Token: 0x0600006A RID: 106 RVA: 0x00003B98 File Offset: 0x00001D98
    private IntPtr WindowProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        //if (Main.model.settingModel.windowState == WindowState.Normal)
        {
            if (msg == 131U)
            {
                return IntPtr.Zero;
            }
            if (msg != 132U)
            {
                return WindowController.CallWindowProc(this.oldWndProc, hWnd, msg, wParam, lParam);
            }
            return this.HitTest(hWnd, lParam);
        }
        //else if (Main.model.settingModel.windowState == WindowState.TablePet)
        {
            if (msg == 132U)
            {
                return this.HitTestDesktop(hWnd, lParam);
            }
            return WindowController.CallWindowProc(this.oldWndProc, hWnd, msg, wParam, lParam);
        }
        //else
        {
            if (msg == 131U)
            {
                return IntPtr.Zero;
            }
            return WindowController.CallWindowProc(this.oldWndProc, hWnd, msg, wParam, lParam);
        }
    }

    // Token: 0x0600006B RID: 107 RVA: 0x00003C3C File Offset: 0x00001E3C
    private IntPtr HitTest(IntPtr hWnd, IntPtr lParam)
    {
        short num = (short)(lParam.ToInt32() & 65535);
        int num2 = (int)((short)(lParam.ToInt32() >> 16));
        WindowController.RECT rect = default(WindowController.RECT);
        WindowController.GetWindowRect(hWnd, ref rect);
        int num3 = 6;
        bool flag = (int)num < rect.Left + num3;
        bool flag2 = (int)num >= rect.Right - num3;
        bool flag3 = num2 < rect.Top + num3;
        bool flag4 = num2 >= rect.Bottom - num3;
        if (flag && flag3)
        {
            return (IntPtr)13;
        }
        if (flag2 && flag3)
        {
            return (IntPtr)14;
        }
        if (flag && flag4)
        {
            return (IntPtr)16;
        }
        if (flag2 && flag4)
        {
            return (IntPtr)17;
        }
        if (flag3)
        {
            return (IntPtr)12;
        }
        if (flag)
        {
            return (IntPtr)10;
        }
        if (flag2)
        {
            return (IntPtr)11;
        }
        if (flag4)
        {
            return (IntPtr)15;
        }
        return (IntPtr)1;
    }

    // Token: 0x0600006C RID: 108 RVA: 0x00003D20 File Offset: 0x00001F20
    private IntPtr HitTestDesktop(IntPtr hWnd, IntPtr lParam)
    {
        int num = (int)((short)(lParam.ToInt32() & 65535));
        int num2 = (int)((short)(lParam.ToInt32() >> 16));
        WindowController.RECT rect = default(WindowController.RECT);
        WindowController.GetWindowRect(hWnd, ref rect);
        int num3 = 20;
        bool flag = num < rect.Left + num3;
        bool flag2 = num2 >= rect.Bottom - num3;
        if (flag && flag2)
        {
            return (IntPtr)16;
        }
        return (IntPtr)1;
    }

    // Token: 0x04000043 RID: 67
    private static uint SWP_SHOWWINDOW = 64U;

    // Token: 0x04000044 RID: 68
    private static uint SWP_FRAMECHANGED = 32U;

    // Token: 0x04000045 RID: 69
    private const int SWP_NOMOVE = 2;

    // Token: 0x04000046 RID: 70
    private const int SWP_NOSIZE = 1;

    // Token: 0x04000047 RID: 71
    private const uint SWP_NOZORDER = 4U;

    // Token: 0x04000048 RID: 72
    private static int GWL_STYLE = -16;

    // Token: 0x04000049 RID: 73
    private static int GWL_EXSTYLE = -20;

    // Token: 0x0400004A RID: 74
    private const int WM_SYSCOMMAND = 274;

    // Token: 0x0400004B RID: 75
    private const int SC_MOVE = 61456;

    // Token: 0x0400004C RID: 76
    private const int WM_LBUTTONDOWN = 513;

    // Token: 0x0400004D RID: 77
    private const int MONITOR_DEFAULTTONEAREST = 2;

    // Token: 0x0400004E RID: 78
    private const uint WS_POPUP = 2147483648U;

    // Token: 0x0400004F RID: 79
    private const uint WS_OVERLAPPEDWINDOW = 13565952U;

    // Token: 0x04000050 RID: 80
    private const uint WS_THICKFRAME = 262144U;

    // Token: 0x04000051 RID: 81
    private const uint WS_SYSMENU = 524288U;

    // Token: 0x04000052 RID: 82
    private const uint WS_MINIMIZEBOX = 131072U;

    // Token: 0x04000053 RID: 83
    private const uint WS_MAXIMIZEBOX = 65536U;

    // Token: 0x04000054 RID: 84
    private static Resolution[] resolutions;

    // Token: 0x04000055 RID: 85
    private static WindowController.RECT rect;

    // Token: 0x04000056 RID: 86
    private float lastHeight;

    // Token: 0x04000057 RID: 87
    private float lastWidth;

    // Token: 0x04000058 RID: 88
    private float size;

    // Token: 0x04000059 RID: 89
    public static int frameHeight = 0;

    // Token: 0x0400005A RID: 90
    public static int frameWidth = 0;

    // Token: 0x0400005B RID: 91
    public static bool isFrameLatest = false;

    // Token: 0x0400005C RID: 92
    public static int taskbarHeight;

    // Token: 0x0400005D RID: 93
    //private static WindowsDrag windowsDrag;

    // Token: 0x0400005E RID: 94
    public static IntPtr myintptr;

    // Token: 0x0400005F RID: 95
    private static WindowController instance;

    // Token: 0x04000060 RID: 96
    private IntPtr oldWndProc = IntPtr.Zero;

    // Token: 0x04000061 RID: 97
    private WindowController.WndProcDelegate newWndProc;

    // Token: 0x04000062 RID: 98
    private const int WM_NCCALCSIZE = 131;

    // Token: 0x04000063 RID: 99
    private const int WM_NCPAINT = 133;

    // Token: 0x04000064 RID: 100
    private const int WM_NCHITTEST = 132;

    // Token: 0x04000065 RID: 101
    private const int HTCLIENT = 1;

    // Token: 0x04000066 RID: 102
    private const int HTCAPTION = 2;

    // Token: 0x04000067 RID: 103
    private const int HTLEFT = 10;

    // Token: 0x04000068 RID: 104
    private const int HTRIGHT = 11;

    // Token: 0x04000069 RID: 105
    private const int HTTOP = 12;

    // Token: 0x0400006A RID: 106
    private const int HTTOPLEFT = 13;

    // Token: 0x0400006B RID: 107
    private const int HTTOPRIGHT = 14;

    // Token: 0x0400006C RID: 108
    private const int HTBOTTOM = 15;

    // Token: 0x0400006D RID: 109
    private const int HTBOTTOMLEFT = 16;

    // Token: 0x0400006E RID: 110
    private const int HTBOTTOMRIGHT = 17;

    // Token: 0x02000098 RID: 152
    public struct RECT
    {
        // Token: 0x170000BC RID: 188
        // (get) Token: 0x060005A4 RID: 1444 RVA: 0x00021D48 File Offset: 0x0001FF48
        public int Width
        {
            get
            {
                return this.Right - this.Left;
            }
        }

        // Token: 0x170000BD RID: 189
        // (get) Token: 0x060005A5 RID: 1445 RVA: 0x00021D57 File Offset: 0x0001FF57
        public int Height
        {
            get
            {
                return this.Bottom - this.Top;
            }
        }

        // Token: 0x040004DC RID: 1244
        public int Left;

        // Token: 0x040004DD RID: 1245
        public int Top;

        // Token: 0x040004DE RID: 1246
        public int Right;

        // Token: 0x040004DF RID: 1247
        public int Bottom;
    }

    // Token: 0x02000099 RID: 153
    private struct MONITORINFO
    {
        // Token: 0x040004E0 RID: 1248
        public int cbSize;

        // Token: 0x040004E1 RID: 1249
        public WindowController.RECT rcMonitor;

        // Token: 0x040004E2 RID: 1250
        public WindowController.RECT rcWork;

        // Token: 0x040004E3 RID: 1251
        public uint dwFlags;
    }

    // Token: 0x0200009A RID: 154
    private struct MARGINS
    {
        // Token: 0x040004E4 RID: 1252
        public int left;

        // Token: 0x040004E5 RID: 1253
        public int right;

        // Token: 0x040004E6 RID: 1254
        public int top;

        // Token: 0x040004E7 RID: 1255
        public int bottom;
    }

    // Token: 0x0200009B RID: 155
    // (Invoke) Token: 0x060005A7 RID: 1447
    private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
}
