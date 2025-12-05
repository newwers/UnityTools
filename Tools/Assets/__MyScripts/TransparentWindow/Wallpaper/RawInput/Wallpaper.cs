using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using UnityEngine;

public class Wallpaper : MonoBehaviour
{
    public Texture2D icon;
    private NotifyIcon notifyIcon;
    public static bool isWallpaperMode = false;

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
    [DllImport("user32.dll")]
    public static extern int SystemParametersInfo(int uAction, int uParam, StringBuilder lpvParam, int fuWinIni);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

    [DllImport("user32.dll")]
    public static extern int SetWindowLong(IntPtr hWnd, int nIndex, ulong dwNewLong);

    [DllImport("user32.dll")]
    private static extern IntPtr GetParent(IntPtr hWnd);

    [DllImport("dwmapi.dll")]
    private static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMarInset);

    [DllImport("user32.dll", SetLastError = true)]
    static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll", SetLastError = true)]
    static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

    [DllImport("user32.dll", SetLastError = true)]
    static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);
    delegate bool EnumWindowsProc(IntPtr hwnd, IntPtr lParam);

    [DllImport("user32.dll")]
    static extern IntPtr FindWindowEx(IntPtr parent, IntPtr child, string className, string windowTitle);

    [DllImport("user32.dll")]
    static extern IntPtr SendMessageTimeout(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam, uint fuFlags, uint timeout, out IntPtr result);

    [DllImport("user32.dll")]
    public static extern IntPtr GetDesktopWindow();

    const UInt32 SWP_NOSIZE = 0x0001;
    const UInt32 SWP_NOMOVE = 0x0002;
    const UInt32 SWP_NOACTIVATE = 0x0010;
    const Int32 SPI_SETDESKWALLPAPER = 0x14;
    static readonly IntPtr HWND_BOTTOM = new IntPtr(1);
    public static IntPtr clickWindow1;
    public static IntPtr clickWindow2;
    public static IntPtr unityHwnd;
    public static IntPtr backgroundHwnd;

    public const int GWL_STYLE = -16;//设置窗口的普通样式
    public const int GWL_EXSTYLE = -20;//设置窗口的扩展样式
    public const int WS_EX_LAYERED = 0x80000;//分层窗口
    public const int WS_EX_TRANSPARENT = 0x20;//鼠标穿透
    const uint WS_POPUP = 0x80000000;
    const uint WS_VISIBLE = 0x10000000;
    const uint WS_CHILDWINDOW = 0x40000000;//子窗口样式（不能单独使用，需配合 WS_VISIBLE，无标题栏 / 边框）
    const uint WS_CLIPCHILDREN = 0x2000000;//父窗口绘制时，裁剪子窗口区域（避免父窗口覆盖子窗口）
    public static int currentStyle;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        unityHwnd = FindWindow(null, UnityEngine.Application.productName); // 在Unity中设置窗口标题
        currentStyle = TranspareWindows.GetWindowLong(unityHwnd, GWL_STYLE);//获取当前窗口样式

        //var hand = FindWindow("UnityWndClass", UnityEngine.Application.productName);
        Debug.Log($"Unity窗口句柄1: {unityHwnd}");
        LoadContextMenu();
    }



    private void OnDisable()
    {
        if (this.notifyIcon != null)
        {
            this.notifyIcon.Visible = false;
            this.notifyIcon.Dispose();
        }

        ExitWallparer();
    }

    private void OnApplicationQuit()
    {
        ExitWallparer();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            SetWallpaper();
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            ExitWallparer();
        }
    }

    void LoadContextMenu()
    {
        LogManager.Log("开始加载菜单栏图标");
        this.notifyIcon = new NotifyIcon();
        notifyIcon.Icon = UnityContextMenu.TextureToIcon(icon);//设置图标
        this.notifyIcon.Visible = true;
        this.notifyIcon.Text = "桌面气球";


        MenuItem menuItem = new MenuItem();
        menuItem.Click += new System.EventHandler(OnWindowedClick);
        menuItem.Index = 0;
        menuItem.Text = "退出壁纸";

        MenuItem menuItem2 = new MenuItem();
        menuItem2.Click += new System.EventHandler(OnExitClick);
        menuItem2.Index = 1;
        menuItem2.Text = "退出游戏";


        var contextMenu = new System.Windows.Forms.ContextMenu();
        contextMenu.MenuItems.Add(menuItem);
        contextMenu.MenuItems.Add(menuItem2);



        notifyIcon.ContextMenu = contextMenu;
    }

    private void OnWindowedClick(object sender, EventArgs e)
    {
        ExitWallparer();
    }

    private void OnExitClick(object sender, EventArgs e)
    {
        UnityEngine.Application.Quit();
    }

    public static void ExitWallparer()
    {
        SetWindowLong(unityHwnd, GWL_STYLE, (ulong)currentStyle);
        SetWindowLong(unityHwnd, GWL_EXSTYLE, 0);//256是窗口化,0是全屏

        //设置窗口位置和大小,是否置顶
        bool isTop = false;
        SetWindowPos(unityHwnd, isTop ? (IntPtr)(-1) : (IntPtr)0, 0, 0, Display.main.systemWidth, Display.main.systemHeight, 64U);//64u SWP_ASYNCWINDOWPOS	0x4000	异步更新窗口位置（避免 UI 卡顿，建议优先使用）

        isWallpaperMode = false;
        Debug.Log($"退出壁纸模式,unityHwnd:{unityHwnd}");
        SetParent(unityHwnd, GetDesktopWindow());//IntPtr.Zero
        RefreshDesktop();

        ApplyDwmFix(0);
    }



    static void RefreshDesktop()
    {
        SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, null, 1);//设置 Windows 桌面壁纸,固定宏定义（值为 0x0014），告诉 API“我要执行「设置桌面壁纸」操作”
        Debug.Log("退出壁纸模式，刷新桌面");
    }

    public void SetWallpaper()
    {
        IntPtr background = GetBackground();
        backgroundHwnd = background;
        if (background == IntPtr.Zero)
        {
            Debug.LogError("查找壁纸窗口失败！");
            return;
        }
        if (clickWindow1 == IntPtr.Zero || clickWindow2 == IntPtr.Zero)
        {
            Debug.LogError("查找桌面鼠标点击窗口失败！");
            return;
        }
        var hwnd = SetParent(unityHwnd, background);
        SetWindowLong(unityHwnd, GWL_STYLE, WS_POPUP | WS_VISIBLE);//96000000
        //SetWindowLong(unityHwnd, GWL_EXSTYLE, WS_EX_LAYERED);//134742016U  08080000
        SetWindowLong(unityHwnd, GWL_EXSTYLE, 0);
        SetLayeredWindowAttributes(unityHwnd, 0U, byte.MaxValue, 2U);
        SetWindowPos(unityHwnd, IntPtr.Zero, 0, 0, UnityEngine.Screen.width, UnityEngine.Screen.height, 64U);//这边窗体多大壁纸就多大,如果想全屏大小就用Display.main.systemWidth, Display.main.systemHeight
        UnityEngine.Screen.SetResolution(UnityEngine.Screen.width, UnityEngine.Screen.height, false);
        isWallpaperMode = true;
        print($"设置壁纸窗体句柄,unityHwnd:{unityHwnd},background句柄:{background},hwnd:{hwnd}");
        ApplyDwmFix(0);
    }


    private static void ApplyDwmFix(int inset)
    {
        MARGINS margins = new MARGINS
        {
            left = inset,
            right = inset,
            top = inset,
            bottom = inset
        };
        DwmExtendFrameIntoClientArea(unityHwnd, ref margins);
    }

    private static IntPtr GetBackground()
    {
        IntPtr intPtr = FindWindow("Progman", "Program Manager");
        if (intPtr == IntPtr.Zero)
        {
            Debug.LogError("未找到Progman窗口！");
            return IntPtr.Zero;
        }
        IntPtr intPtr2;
        SendMessageTimeout(intPtr, 1324U, IntPtr.Zero, IntPtr.Zero, 1U, 1000U, out intPtr2);
        Debug.Log("创建workerW窗口 result: " + intPtr2.ToString());
        SendMessageTimeout(intPtr, 1324U, new IntPtr(13), new IntPtr(1), 1U, 1000U, out intPtr2);
        Debug.Log("创建workerW窗口 result: " + intPtr2.ToString());
        IntPtr workerw = IntPtr.Zero;
        for (; ; )
        {
            workerw = FindWindowEx(IntPtr.Zero, workerw, "WorkerW", null);
            if (GetParent(workerw) == intPtr)
            {
                break;
            }
            if (!(workerw != IntPtr.Zero))
            {
                goto Block_4;
            }
        }
        Debug.Log("查找到壁纸窗口(win10/11)");
        IntPtr intPtr3 = IntPtr.Zero;
        while ((intPtr3 = FindWindowEx(IntPtr.Zero, intPtr3, "WorkerW", null)) != IntPtr.Zero)
        {
            clickWindow1 = FindWindowEx(intPtr3, IntPtr.Zero, "SHELLDLL_DefView", null);
            if (clickWindow1 != IntPtr.Zero)
            {
                clickWindow2 = FindWindowEx(clickWindow1, IntPtr.Zero, "SysListView32", null);
                break;
            }
        }
        Debug.Log("点击窗口：" + clickWindow1.ToString() + ", " + clickWindow2.ToString());
        return workerw;
    Block_4:
        EnumWindows(delegate (IntPtr topHandle, IntPtr topParamHandle)
        {
            IntPtr intPtr4 = FindWindowEx(topHandle, IntPtr.Zero, "SHELLDLL_DefView", null);
            if (intPtr4 != IntPtr.Zero)
            {
                Debug.Log("(win1124H2)SHELLDLL_DefView窗口: " + intPtr4.ToString());
                workerw = FindWindowEx(topHandle, intPtr4, "workerw", null);
                clickWindow1 = intPtr4;
                clickWindow2 = FindWindowEx(intPtr4, IntPtr.Zero, "SysListView32", null);
                Debug.Log("点击窗口：" + clickWindow1.ToString() + ", " + clickWindow2.ToString());
                return false;
            }
            return true;
        }, IntPtr.Zero);
        return workerw;
    }
}
