using System;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

public class Wallpaper : MonoBehaviour
{
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
    public static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    private void OnDisable()
    {
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

    public static void ExitWallparer()
    {
        IntPtr unityHwnd = FindWindow(null, Application.productName);
        SetParent(unityHwnd, GetDesktopWindow());
        RefreshDesktop();
    }

    static void RefreshDesktop()
    {
        SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, null, 1);
        Debug.Log("退出壁纸模式，刷新桌面");
    }

    public void SetWallpaper()
    {
        print("设置壁纸1");
        // 2. 找到Unity窗口句柄
        IntPtr unityWindowHandle = FindWindow(null, Application.productName); // 在Unity中设置窗口标题
        print("设置壁纸2,unityHwnd:" + unityWindowHandle);
        IntPtr background = GetBackground();
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
        SetParent(unityWindowHandle, background);
        SetWindowLong(unityWindowHandle, -16, 1342177280U);
        SetWindowLong(unityWindowHandle, -20, 134742016U);
        SetLayeredWindowAttributes(unityWindowHandle, 0U, byte.MaxValue, 2U);
        WindowController.RECT currentScreenInfo = WindowController.Instance().GetCurrentScreenInfo();
        SetWindowPos(unityWindowHandle, IntPtr.Zero, currentScreenInfo.Left, currentScreenInfo.Top, currentScreenInfo.Width, currentScreenInfo.Height, 64U);
        if (gameObject.GetComponent<RawInputStarter>() == null)
        {
            gameObject.AddComponent<RawInputStarter>();
        }
        Screen.SetResolution(Screen.width, Screen.height, false);
    }


    private static void ApplyDwmFix(int inset, IntPtr unityHwnd)
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
