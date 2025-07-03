/*
	newwer
*/
using System;
using System.Runtime.InteropServices;
using UnityEngine;


/// <summary>
/// 设置应用程序背景透明,鼠标穿透
/// </summary>
public class TranspareWindows : MonoBehaviour
{
    private struct MARGINS
    {
        public int cxLeftWidth;
        public int cxRightWidth;
        public int cyTopHeight;
        public int cyBottomHeight;
    }


    /// <summary>
    /// 设置窗体透明
    /// </summary>
    /// <param name="hWnd"></param>
    /// <param name="nIndex"></param>
    /// <param name="dwNewLong"></param>
    /// <returns></returns>
    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

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
    private static extern int SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int y, int Width, int Height, int flags);

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
    /// 弹出窗口
    /// </summary>
    const uint WS_POPUP = 0x80000000;//10000000000000000000000000000000
    /// <summary>
    /// 可见状态
    /// </summary>
    const uint WS_VISIBLE = 0x10000000;//10000000000000000000000000000    两个相或10010000000000000000000000000000
    /// <summary>
    /// 分层或透明窗口,该样式可使用混合特效
    /// </summary>
    const uint WS_EX_LAYERED = 0x80000;
    /// <summary>
    /// 当前窗体句柄
    /// </summary>
    private IntPtr hwnd;


    public void Awake()
    {
#if !UNITY_EDITOR//在非unity编辑器下运行,打包成exe文件运行
        hwnd = GetActiveWindow();

        SetWindowTop();
        SetWindowTranspareWithSetWindowLong();
        SetWindowPenetration();
        SetWindowTranspareWithSetLayeredWindowAttributes();
        SetInitializationWindowPosition();
#endif
    }

    /// <summary>
    /// 根据配置文件,设置窗体是否置顶
    /// </summary>
    private void SetWindowTop()
    {
        //if (config.isWindowTop.Equals("true"))
        {
            //窗口置顶,第二个参数就是控制窗口置顶的选项
            //public const int HWND_TOP = 0;//顶部
            //public const int HWND_BOTTOM = 1;//底部
            //public const int HWND_TOPMOST = -1;//最顶部
            //public const int HWND_NOTOPMOST = -2;//不是最顶部
            SetWindowPos(hwnd, -1, 0, 0, 0, 0, 1 | 2);
        }
        //else
        //{
        //    SetWindowPos(hwnd, 1, 0, 0, 0, 0, 1 | 2);
        //}
    }
    /// <summary>
    /// 根据配置文件,设置窗体是否透明
    /// </summary>
    private void SetWindowTranspareWithSetWindowLong()
    {
        //设置窗体属性
        SetWindowLong(hwnd, GWL_STYLE, WS_POPUP | WS_VISIBLE);//背景透明,但是鼠标没有穿透
        //如果单单设置这个为false,效果为:有标题栏,意思就是说有缩小和关闭按钮,然后背景不透明,全白,窗体可以穿透
        //也就是说这个函数是用来设置标题栏隐藏和背景透明相关
    }
    /// <summary>
    /// 根据配置文件,设置窗体是否穿透
    /// </summary>
    private void SetWindowPenetration()
    {
        SetWindowLong(hwnd, GWL_EXSTYLE, WS_EX_LAYERED);//穿透,不透明
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
        //SetLayeredWindowAttributes(hwnd, 0, 255, 1);
        SetLayeredWindowAttributes(hwnd, 0x00080808, 255, 1);
    }
    /// <summary>
    /// 设置刚开启窗体的位置,默认左上角坐标为0,0
    /// </summary>
    private void SetInitializationWindowPosition()
    {
        //MoveWindow(hwnd, config.InitializationPositionX, config.InitializationPositionY, Screen.width, Screen.height, true);
    }




}



