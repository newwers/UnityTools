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
    //public Material m_Material;
    private struct MARGINS
    {
        public int cxLeftWidth;
        public int cxRightWidth;
        public int cyTopHeight;
        public int cyBottomHeight;
    }

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

    [DllImport("user32.dll")]
    public static extern IntPtr GetDesktopWindow();
    [DllImport("user32.dll")]
    public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);
    // Token: 0x0600004C RID: 76
    [DllImport("user32.dll")]
    private static extern IntPtr FindWindow(string strClassName, string nptWindowName);

    [DllImport("user32.dll")]
    public static extern int GetWindowLong(IntPtr hWnd, int nIndex);
    /// <summary>
    /// 设置窗体透明
    /// </summary>
    /// <param name="hWnd"></param>
    /// <param name="nIndex"></param>
    /// <param name="dwNewLong"></param>
    /// <returns></returns>
    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("Dwmapi.dll")]
    private static extern uint DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS margins);
    /// <summary>
    /// 设置窗口置顶
    /// </summary>
    /// <param name="hWnd"></param>
    /// <param name="hWndInsertAfter"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="Width"></param>
    /// <param name="Height"></param>
    /// <param name="flags"></param>
    /// <returns></returns>
    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern int SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int y, int Width, int Height, uint flags);

    /// <summary>
    /// 设置鼠标穿透
    /// </summary>
    /// <param name="hwnd"></param>
    /// <param name="crKey"></param>
    /// <param name="bAlpha"></param>
    /// <param name="dwFlags"></param>
    /// <returns></returns>
    [DllImport("user32", EntryPoint = "SetLayeredWindowAttributes")]
    private static extern uint SetLayeredWindowAttributes(IntPtr hwnd, int crKey, int bAlpha, int dwFlags);


    /// <summary>
    /// 设置目标窗体大小，位置
    /// </summary>
    /// <param name="hWnd">目标句柄</param>
    /// <param name="x">目标窗体新位置X轴坐标</param>
    /// <param name="y">目标窗体新位置Y轴坐标</param>
    /// <param name="nWidth">目标窗体新宽度</param>
    /// <param name="nHeight">目标窗体新高度</param>
    /// <param name="BRePaint">是否刷新窗体</param>
    /// <returns></returns>
    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern int MoveWindow(IntPtr hWnd, int x, int y, int nWidth, int nHeight, bool BRePaint);

    /// <summary>
    /// 获取窗口句柄
    /// </summary>
    /// <returns></returns>
    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();


    //定义窗体样式,-16表示设定一个新的窗口风格。
    const int GWL_STYLE = -16;
    //设定一个新的扩展风格
    const int GWL_EXSTYLE = -20;
    /// <summary>
    /// 无边框窗口
    /// </summary>
    const uint WS_POPUP = 0x80000000;//10000000000000000000000000000000
    /// <summary>
    /// 窗口可见 可见状态
    /// </summary>
    const uint WS_VISIBLE = 0x10000000;//10000000000000000000000000000    两个相或10010000000000000000000000000000
    /// <summary>
    /// 分层或透明窗口,该样式可使用混合特效 支持透明度混合
    /// </summary>
    const int WS_EX_LAYERED = 0x80000;
    const int WS_EX_TRANSPARENT = 0x00000020;

    const int WS_THICKFRAME = 262144;
    const int WS_BORDER = 8388608;
    public const int WS_CAPTION = 0x00C00000;

    private static uint SWP_FRAMECHANGED = 32U;


    /// <summary>
    /// 当前窗体句柄
    /// </summary>
    private IntPtr hwnd;
    private static int m_GWL_STYLE;

    public void Awake()
    {
        //hwnd = FindWindow("UnityWndClass", "DesktopBalloon");
        hwnd = GetActiveWindow();
#if !UNITY_EDITOR//在非unity编辑器下运行,打包成exe文件运行

        SetWindowTop(true);
        SetWindowTranspareWithSetWindowLong();
        SetWindowPenetration();
        SetWindowTranspareWithSetLayeredWindowAttributes();
        SetInitializationWindowPosition();
#endif
    }

    private void Update()
    {
#if UNITY_EDITOR
        return;
#endif
        if (EventSystem.current.IsPointerOverGameObject())
        {
            this.SetClickThrough(false);
        }
        else
        {
            this.SetClickThrough(Physics2D.OverlapPoint(GetMouseWorldPosition()) == null);
        }

        //todo:是否需要检测3D的物体
    }

    void Check3DGameObject()
    {
        if (Input.GetMouseButtonDown(0)) // 左键点击
        {
            // 获取鼠标在屏幕上的位置
            Vector3 mouseScreenPosition = Input.mousePosition;

            // 将鼠标的屏幕坐标转换为世界坐标
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);
            worldPosition.z = 0; // 设置 z 坐标为 0，因为我们是在 2D 空间中

            // 发射射线检测鼠标位置是否有物体碰撞
            RaycastHit2D hit = Physics2D.Raycast(worldPosition, Vector2.zero);

            if (hit.collider != null)
            {
                // 如果射线击中了物体
                Debug.Log("点击到物体: " + hit.collider.gameObject.name);

                // 可以根据物体的条件进一步筛选，例如检查物体是否启用
                if (hit.collider.gameObject.activeSelf)
                {
                    Debug.Log("该物体处于启用状态，继续处理...");
                }
                else
                {
                    Debug.Log("该物体处于禁用状态，跳过...");
                }
                // 禁用点击透传，使得游戏物体可以接收到点击事件
                SetClickThrough(false);
            }
            else
            {
                // 如果没有物体被点击到
                Debug.Log("没有点击到任何物体");
                // 启用点击透传，允许点击穿透透明窗口
                SetClickThrough(true);
            }
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

    /// <summary>
    /// 根据配置文件,设置窗体是否置顶
    /// </summary>
    public void SetWindowTop(bool isTop)
    {
        if (isTop)
        {
            //窗口置顶,第二个参数就是控制窗口置顶的选项
            //public const int HWND_TOP = 0;//顶部
            //public const int HWND_BOTTOM = 1;//底部
            //public const int HWND_TOPMOST = -1;//最顶部
            //public const int HWND_NOTOPMOST = -2;//不是最顶部
            SetWindowPos(hwnd, -1, 0, 0, 0, 0, 1 | 2);
        }
        else
        {
            SetWindowPos(hwnd, 1, 0, 0, 0, 0, 1 | 2);
        }
    }
    /// <summary>
    /// 根据配置文件,设置窗体是否透明
    /// </summary>
    private void SetWindowTranspareWithSetWindowLong()
    {
        int style = GetWindowLong(hwnd, GWL_STYLE);
        //设置窗体属性
        //SetWindowLong(hwnd, GWL_STYLE, WS_POPUP | WS_VISIBLE);//背景透明,但是鼠标没有穿透
        SetWindowLong(hwnd, GWL_EXSTYLE, style & ~WS_BORDER & ~WS_THICKFRAME & ~WS_CAPTION);
    }
    /// <summary>
    /// 根据配置文件,设置窗体是否穿透
    /// </summary>
    private void SetWindowPenetration()
    {
        int style = GetWindowLong(hwnd, GWL_STYLE);
        //SetWindowLong(hwnd, GWL_EXSTYLE, WS_EX_LAYERED);//穿透,不透明 支持透明度混合
        SetWindowLong(hwnd, GWL_EXSTYLE, style | WS_EX_LAYERED | WS_EX_TRANSPARENT);
        //如果单单设置和这个为false,效果:标题栏隐藏,背景透明,但是鼠标没有穿透
        //说明这和函数和鼠标穿透相关
    }
    /// <summary>
    /// 根据配置文件,设置窗体是否穿透透明
    /// </summary>
    private void SetWindowTranspareWithSetLayeredWindowAttributes()
    {
        var margins = new MARGINS() { cxLeftWidth = -1 };



        //扩展窗口到客户端区域
        //详情:https://msdn.microsoft.com/en-us/library/windows/desktop/aa969512%28v=vs.85%29.aspx
        DwmExtendFrameIntoClientArea(hwnd, ref margins);

        //设置窗体可穿透点击的透明.
        //参数1:窗体句柄
        //参数2:透明颜色  0为黑色,按照从000000到FFFFFF的颜色,转换为10进制的值
        //参数3:透明度,设置成255就是全透明
        //参数4:透明方式,1表示将该窗口颜色为0的部分设置为透明,2表示根据透明度设置窗体的透明度
        SetLayeredWindowAttributes(hwnd, TranspareColor, 255, 2);//相机设置为solid color 并且颜色设置为080808
        //SetLayeredWindowAttributes(hwnd, TranspareColor, 125, 1 | 2);//窗体半透明+指定颜色穿透
    }
    /// <summary>
    /// 设置刚开启窗体的位置,默认左上角坐标为0,0
    /// </summary>
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

    private static void ApplyDwmFix(int inset)
    {
        MARGINS margins = new MARGINS
        {
            cxLeftWidth = inset,
            cxRightWidth = inset,
            cyTopHeight = inset,
            cyBottomHeight = inset
        };
        DwmExtendFrameIntoClientArea(WindowController.myintptr, ref margins);
    }
    /// <summary>
    /// 设置穿透
    /// </summary>
    /// <param name="SetClickthrouth"></param>
    private void SetClickThrough(bool SetClickthrouth)
    {
        if (SetClickthrouth)
        {
            //添加透明或者点击通过
            SetWindowLong(hwnd, GWL_EXSTYLE, WS_EX_LAYERED | WS_EX_TRANSPARENT);
            Debug.Log("点击透传已启用");

        }
        else
        {
            // 恢复点击
            SetWindowLong(hwnd, GWL_EXSTYLE, WS_EX_LAYERED);
            Debug.Log("点击透传已禁用");

        }
    }
}



