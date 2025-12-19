using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using UnityEngine;
using UnityEngine.EventSystems;
using Button = UnityEngine.UI.Button;

public class WindowsMouseInput : MonoBehaviour
{
    // 单例实例，方便全局访问
    public static WindowsMouseInput Instance { get; private set; }

    // 鼠标状态数据
    [Header("鼠标状态（实时更新）")]
    public Vector2 mousePosition; // 鼠标位置（屏幕坐标）
    public bool isLeftMouseDown;  // 左键是否按下
    public bool isDKeyDown;       // D键是否按下 新增

    // Windows API相关定义
    #region Windows API 导入
    // 窗口过程委托（必须与WindowProc签名一致）
    private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    // 获取当前活动窗口句柄
    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    // 设置窗口过程函数
    [DllImport("user32.dll")]
    private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    // 调用原始窗口过程
    [DllImport("user32.dll")]
    private static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    // 从lParam解析鼠标坐标（低16位x，高16位y）
    private static int GET_X_LPARAM(IntPtr lParam) => LOWORD(lParam.ToInt32());
    private static int GET_Y_LPARAM(IntPtr lParam) => HIWORD(lParam.ToInt32());
    private static int LOWORD(int value) => value & 0xFFFF;
    private static int HIWORD(int value) => (value >> 16) & 0xFFFF;

    // Windows消息常量
    private const uint WM_LBUTTONDOWN = 0x0201; // 左键按下
    private const uint WM_LBUTTONUP = 0x0202;   // 左键松开
    private const uint WM_MOUSEMOVE = 0x0200;   // 鼠标移动
    private const int GWL_WNDPROC = -4;         // 窗口过程索引

    private const uint WM_KEYDOWN = 0x0100;     // 键盘按下 新增
    private const uint WM_KEYUP = 0x0101;       // 键盘松开 新增
    private const int VK_D = 0x44;              // D键虚拟键码 新增

    #endregion
    public static IntPtr _backgroundHandle; // 背景窗口句柄

    private static IntPtr _unityWindowHandle; // Unity窗口句柄
    private IntPtr _originalWndProc;   // 原始窗口过程指针
    private WndProcDelegate _customWndProc; // 自定义窗口过程委托

    //private readonly object _lockObj = new object(); // 线程锁


    #region GetAsyncKeyState鼠标监听

    // 导入GetAsyncKeyState函数
    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);

    // 鼠标左键的虚拟键码
    private const int VK_LBUTTON = 0x01;
    bool wasPressed = false;

    Action OnLeftMouseDown;
    Action OnLeftMouseUp;

    // 检测鼠标左键是否按下
    public static bool IsLeftMousePressed()
    {
        // 检查最高位是否被设置（值为负数表示键被按下）
        return (GetAsyncKeyState(VK_LBUTTON) & 0x8000) != 0;
    }

    // 监控鼠标左键的按下和释放
    public void MonitorMouseLeftButton()
    {
        bool isPressed = IsLeftMousePressed();

        if (isPressed && !wasPressed)
        {
            Debug.Log($"[{DateTime.Now:HH:mm:ss.fff}] 鼠标左键按下");
            OnLeftMouseDown?.Invoke();
        }
        else if (!isPressed && wasPressed)
        {
            Debug.Log($"[{DateTime.Now:HH:mm:ss.fff}] 鼠标左键释放");
            OnLeftMouseUp();
        }

        wasPressed = isPressed;
    }

    #endregion

    private void Awake()
    {
        // 单例初始化
        //if (Instance != null && Instance != this)
        //{
        //    Destroy(gameObject);
        //    return;
        //}
        //Instance = this;
        //DontDestroyOnLoad(gameObject);

        // 仅在Windows平台执行
        if (!UnityEngine.Application.isEditor && UnityEngine.Application.platform != RuntimePlatform.WindowsPlayer)
        {
            Debug.LogError("该脚本仅支持Windows平台！");
            enabled = false;
            return;
        }

        OnLeftMouseDown += OnLeftMouseDownHandler;
        OnLeftMouseUp += OnLeftMouseUpHandler;
    }


    private void Start()
    {
        // 获取Unity窗口句柄（编辑器/打包后均生效）
        _unityWindowHandle = GetActiveWindow();
        if (_unityWindowHandle == IntPtr.Zero)
        {
            Debug.LogError("获取Unity窗口句柄失败！");
            return;
        }

        // 初始化自定义窗口过程
        _customWndProc = CustomWindowProc;
        // 替换窗口过程为自定义实现
        _originalWndProc = SetWindowLongPtr(_unityWindowHandle, GWL_WNDPROC, Marshal.GetFunctionPointerForDelegate(_customWndProc));

        if (_originalWndProc == IntPtr.Zero)
        {
            Debug.LogError("替换窗口过程失败！");
        }
        else
        {
            Debug.Log($"成功拦截Unity窗口消息，窗口句柄：{_unityWindowHandle}");
        }
    }
    private void Update()
    {
        if (Wallpaper.isWallpaperMode)
        {
            //MonitorMouseLeftButton();
            //ProcessRawInputMessage();
        }
        //if (Input.GetKeyDown(KeyCode.D))
        //{
        //    //从当前鼠标位置创建一个点击事件
        //    SimulateMouseClick();
        //}
        //if (Input.GetKeyDown(KeyCode.F))
        //{
        //    //从当前鼠标位置创建一个点击事件
        //    SendLeftClickAtCurrentPosition();
        //}
    }

    /// <summary>
    /// 鼠标左键按下
    /// </summary>
    private void OnLeftMouseDownHandler()
    {
        //SimulateMouseClick();
        SendLeftClickAtCurrentPosition();
    }
    /// <summary>
    /// 鼠标左键松开
    /// </summary>
    private void OnLeftMouseUpHandler()
    {

    }



    public void SimulateMouseClick()
    {
        // 获取EventSystem实例
        EventSystem eventSystem = EventSystem.current;
        if (eventSystem == null) return;

        // 创建指针事件数据
        PointerEventData pointerEventData = new PointerEventData(eventSystem);
        pointerEventData.position = Input.mousePosition;

        // 进行射线检测，获取鼠标位置下的所有UI元素
        var results = new System.Collections.Generic.List<RaycastResult>();
        eventSystem.RaycastAll(pointerEventData, results);

        // 遍历检测结果，寻找Button组件
        foreach (var result in results)
        {
            Button button = result.gameObject.GetComponent<Button>();
            if (button != null)
            {
                // 触发Button的点击事件
                button.onClick.Invoke();
                break; // 只触发第一个找到的Button
            }
        }
    }

    /// <summary>
    /// 自定义窗口过程（拦截Windows消息）
    /// </summary>
    private IntPtr CustomWindowProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        //lock (_lockObj)
        {
            // 处理鼠标移动消息
            if (msg == WM_MOUSEMOVE)
            {
                // 解析鼠标屏幕坐标
                int x = GET_X_LPARAM(lParam);
                int y = GET_Y_LPARAM(lParam);
                mousePosition = new Vector2(x, y);
            }
            // 处理左键按下
            else if (msg == WM_LBUTTONDOWN)
            {
                isLeftMouseDown = true;
                // 可选：解析按下时的鼠标位置
                int x = GET_X_LPARAM(lParam);
                int y = GET_Y_LPARAM(lParam);
                mousePosition = new Vector2(x, y);
                Debug.Log($"左键按下，位置：{mousePosition}");
            }
            // 处理左键松开
            else if (msg == WM_LBUTTONUP)
            {
                isLeftMouseDown = false;
                // 可选：解析松开时的鼠标位置
                int x = GET_X_LPARAM(lParam);
                int y = GET_Y_LPARAM(lParam);
                mousePosition = new Vector2(x, y);
                Debug.Log($"左键松开，位置：{mousePosition}");
            }
            //else if (msg == WM_KEYDOWN)
            //{
            //    // 检查是否是D键（虚拟键码0x44）
            //    if (wParam.ToInt32() == VK_D)
            //    {
            //        isDKeyDown = true;
            //        Debug.Log("D键按下");
            //    }
            //}
            //// 处理键盘松开 新增
            //else if (msg == WM_KEYUP)
            //{
            //    // 检查是否是D键
            //    if (wParam.ToInt32() == VK_D)
            //    {
            //        isDKeyDown = false;
            //        Debug.Log("D键松开");
            //    }
            //}
        }

        // 调用原始窗口过程，保证窗口基础功能正常
        return CallWindowProc(_originalWndProc, hWnd, msg, wParam, lParam);
    }

    private void OnDestroy()
    {
        // 还原原始窗口过程（防止程序崩溃）
        if (_unityWindowHandle != IntPtr.Zero && _originalWndProc != IntPtr.Zero)
        {
            SetWindowLongPtr(_unityWindowHandle, GWL_WNDPROC, _originalWndProc);
            Debug.Log("已还原原始窗口过程");
        }
    }

    private void OnGUI()
    {
        // 可视化显示鼠标状态（调试用）
        GUILayout.BeginArea(new Rect(20, 20, 300, 300));
        GUILayout.Label($"Win32鼠标屏幕位置：{mousePosition}");
        GUILayout.Label($"Win32左键是否按下：{isLeftMouseDown}");
        GUILayout.Label($"Win32 D键是否按下：{isDKeyDown}"); // 新增
        GUILayout.Label($"GetAsyncKeyState 左键是否按下：{IsLeftMousePressed()}"); // 新增
        GUILayout.Label($"Unity鼠标屏幕位置：{Input.mousePosition}");
        GUILayout.Label($"Unity左键是否按下：{Input.GetMouseButton(0)}");
        GUILayout.EndArea();
    }

    // 可选：将屏幕坐标转换为Unity世界坐标
    public Vector3 ScreenToWorldPoint(Camera camera = null)
    {
        if (camera == null) camera = Camera.main;
        return camera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, camera.nearClipPlane));
    }

    #region 消息点击事件

    // 导入Windows API函数
    [DllImport("user32.dll")]
    private static extern bool PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern bool SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out Point lpPoint);

    [DllImport("user32.dll")]
    private static extern IntPtr WindowFromPoint(Point point);


    /// <summary>
    /// 在当前鼠标位置发送左键点击消息给顶层窗口
    /// </summary>
    public static void SendLeftClickAtCurrentPosition()
    {
        try
        {
            // 1. 获取当前鼠标位置（屏幕坐标）
            GetCursorPos(out Point cursorPos);

            // 2. 获取鼠标位置下的窗口句柄
            IntPtr targetWindow = _backgroundHandle;// _unityWindowHandle;// WindowFromPoint(cursorPos);


            if (targetWindow != IntPtr.Zero)
            {
                // 3. 发送鼠标按下和弹起消息
                // lParam参数：低16位是X坐标，高16位是Y坐标（客户区坐标）
                // 需要将屏幕坐标转换为客户区坐标
                SendLeftClick(targetWindow, cursorPos);
            }
            else
            {
                MessageBox.Show("未找到目标窗口", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"发送消息失败: {ex.Message}", "错误",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// 在指定窗口的当前位置发送左键点击
    /// </summary>
    /// <param name="targetWindow">目标窗口句柄</param>
    /// <param name="screenPoint">屏幕坐标</param>
    public static void SendLeftClick(IntPtr targetWindow, Point screenPoint)
    {
        if (targetWindow == IntPtr.Zero) return;

        // 将屏幕坐标转换为客户区坐标
        Point clientPoint = ScreenToClient(targetWindow, screenPoint);

        // 创建lParam参数：低16位是X坐标，高16位是Y坐标
        IntPtr lParam = MakeLParam(clientPoint.X, clientPoint.Y);

        // 方法1：使用PostMessage（异步，放入消息队列）
        PostMessage(targetWindow, WM_LBUTTONDOWN, (IntPtr)1, lParam); // wParam=1表示左键
        PostMessage(targetWindow, WM_LBUTTONUP, IntPtr.Zero, lParam);

        // 方法2：使用SendMessage（同步，立即处理）
        // SendMessage(targetWindow, WM_LBUTTONDOWN, (IntPtr)1, lParam);
        // SendMessage(targetWindow, WM_LBUTTONUP, IntPtr.Zero, lParam);
    }

    /// <summary>
    /// 将屏幕坐标转换为窗口客户区坐标
    /// </summary>
    private static Point ScreenToClient(IntPtr hWnd, Point screenPoint)
    {
        // 如果有更复杂的转换需求，可以使用ScreenToClient API
        // 这里简化处理，直接使用屏幕坐标（对于顶级窗口通常是可接受的）
        // 对于需要精确转换的情况，使用下面的完整实现

        // 完整实现：
        // 调用user32.dll中的ScreenToClient函数
        ScreenToClient(hWnd, ref screenPoint);
        return screenPoint;
    }

    [DllImport("user32.dll")]
    private static extern bool ScreenToClient(IntPtr hWnd, ref Point lpPoint);

    /// <summary>
    /// 将X,Y坐标组合成lParam参数
    /// </summary>
    private static IntPtr MakeLParam(int x, int y)
    {
        return (IntPtr)((y << 16) | (x & 0xFFFF));
    }

    #endregion

    #region GetRawInputData

    //public struct RAWINPUTHEADER
    //{
    //    // Token: 0x0400052A RID: 1322
    //    public uint dwType;

    //    // Token: 0x0400052B RID: 1323
    //    public uint dwSize;

    //    // Token: 0x0400052C RID: 1324
    //    public IntPtr hDevice;

    //    // Token: 0x0400052D RID: 1325
    //    public IntPtr wParam;
    //}

    //[StructLayout(LayoutKind.Explicit)]
    //public struct RAWINPUT
    //{
    //    // Token: 0x04000534 RID: 1332
    //    [FieldOffset(0)]
    //    public RAWINPUTHEADER header;

    //    // Token: 0x04000535 RID: 1333
    //    [FieldOffset(24)]
    //    public RAWMOUSE mouse;
    //}

    //public struct RAWMOUSE
    //{
    //    // Token: 0x170000C9 RID: 201
    //    // (get) Token: 0x060005DC RID: 1500 RVA: 0x0002285B File Offset: 0x00020A5B
    //    public ushort usButtonFlags
    //    {
    //        get
    //        {
    //            return (ushort)(this._buttonData & 65535U);
    //        }
    //    }

    //    // Token: 0x170000CA RID: 202
    //    // (get) Token: 0x060005DD RID: 1501 RVA: 0x0002286A File Offset: 0x00020A6A
    //    public ushort usButtonData
    //    {
    //        get
    //        {
    //            return (ushort)(this._buttonData >> 16);
    //        }
    //    }

    //    // Token: 0x0400052E RID: 1326
    //    public ushort usFlags;

    //    // Token: 0x0400052F RID: 1327
    //    private readonly uint _buttonData;

    //    // Token: 0x04000530 RID: 1328
    //    public uint ulRawButtons;

    //    // Token: 0x04000531 RID: 1329
    //    public int lLastX;

    //    // Token: 0x04000532 RID: 1330
    //    public int lLastY;

    //    // Token: 0x04000533 RID: 1331
    //    public uint ulExtraInformation;
    //}

    //public struct POINT
    //{
    //    // Token: 0x04000524 RID: 1316
    //    public int X;

    //    // Token: 0x04000525 RID: 1317
    //    public int Y;
    //}


    //[DllImport("user32.dll", SetLastError = true)]
    //public static extern uint GetRawInputData(IntPtr hRawInput, uint uiCommand, IntPtr pData, ref uint pcbSize, uint cbSizeHeader);

    //public void ProcessRawInputMessage(IntPtr lParam)
    //{
    //    uint num = 0U;
    //    GetRawInputData(lParam, 268435459U, IntPtr.Zero, ref num, (uint)Marshal.SizeOf(typeof(RAWINPUTHEADER)));
    //    if (num == 0U)
    //    {
    //        return;
    //    }
    //    IntPtr intPtr = Marshal.AllocHGlobal((int)num);
    //    try
    //    {
    //        uint num2 = num;
    //        if (GetRawInputData(lParam, 268435459U, intPtr, ref num2, (uint)Marshal.SizeOf(typeof(RAWINPUTHEADER))) == num2)
    //        {
    //            RAWINPUT rawinput = (RAWINPUT)Marshal.PtrToStructure(intPtr, typeof(RAWINPUT));
    //            if (rawinput.header.dwType == 0U)
    //            {
    //                ushort usButtonFlags = rawinput.mouse.usButtonFlags;
    //                bool flag = (usButtonFlags & 1) > 0;
    //                bool flag2 = (usButtonFlags & 2) > 0;
    //                Point pt;
    //                GetCursorPos(out pt);
    //                //if (this.IsMouseOnDesktop(pt))
    //                {
    //                    if (flag)
    //                    {
    //                        PostMessage(WindowController.myintptr, 513U, IntPtr.Zero, lParam);
    //                    }
    //                    else if (flag2)
    //                    {
    //                        PostMessage(WindowController.myintptr, 514U, IntPtr.Zero, lParam);
    //                    }
    //                }
    //            }
    //        }
    //    }
    //    finally
    //    {
    //        Marshal.FreeHGlobal(intPtr);
    //    }
    //}

    #endregion
}