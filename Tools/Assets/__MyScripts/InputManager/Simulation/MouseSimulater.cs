using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace InputModule
{
    public class MouseSimulater
    {
        #region DLLs
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int SetCursorPos(int x, int y);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern void mouse_event(MouseEventFlag dwFlags, int dx, int dy, uint dwData, UIntPtr dwExtraInfo);

        // 方法参数说明
        // VOID mouse_event(
        //     DWORD dwFlags,         // motion and click options
        //     DWORD dx,              // horizontal position or change
        //     DWORD dy,              // vertical position or change
        //     DWORD dwData,          // wheel movement
        //     ULONG_PTR dwExtraInfo  // application-defined information
        // );

        [Flags]
        enum MouseEventFlag : uint
        {
            Move = 0x0001,
            LeftDown = 0x0002,
            LeftUp = 0x0004,
            RightDown = 0x0008,
            RightUp = 0x0010,
            MiddleDown = 0x0020,
            MiddleUp = 0x0040,
            XDown = 0x0080,
            XUp = 0x0100,
            Wheel = 0x0800,
            VirtualDesk = 0x4000,
            Absolute = 0x8000
        }

        /// <summary>
        /// 获取当前窗体的位置
        /// </summary>
        /// <param name="hwnd">窗体句柄</param>
        /// <param name="lpRect">存放窗体位置的结构</param>
        /// <returns></returns>
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

        /// <summary>
        /// 获取窗口句柄
        /// </summary>
        /// <returns></returns>
        [DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();

        /// <summary>
        /// 获得鼠标在屏幕上的位置
        /// </summary>
        /// <param name="lpPoint"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        // GetCursorPos() makes everything possible
        public static extern bool GetCursorPos(ref POINT lpPoint);

        #endregion

        // Unity屏幕坐标从左下角开始，向右为X轴，向上为Y轴
        // Windows屏幕坐标从左上角开始，向右为X轴，向下为Y轴

        /// <summary>
        /// 移动鼠标到指定位置（使用Unity屏幕坐标而不是Windows屏幕坐标）
        /// </summary>
        public static bool MoveTo(float x, float y)
        {
            if (x < 0 || y < 0 || x > 1920 || y > 1080)
                return false;

            SetCursorPos((int)x, (int)y);
            return true;
        }

        /// <summary>
        /// unity内的坐标系转换到屏幕坐标系
        /// //1.获得当前显示器的分辨率(假设1920*1080)
        /// //2.获得unity的窗体大小(Screen.width,Screen.height)
        /// //3.获取窗体位置 GetWindowRect
        /// //4.将鼠标在unity内点击的坐标转化成 0到窗体分辨率大小的取值范围
        /// //5.将窗体位置 + 转化后点击位置的一半 = 屏幕目标位置
        /// </summary>
        /// <param name="unityPos"></param>
        /// <returns></returns>
        public static Vector2 UnityScreenToWindowPos(Vector2 unityPos)
        {
            Vector2 windowScreen = new Vector2(1920, 1080);

            RECT windowPos;
            GetWindowRect(GetActiveWindow(), out windowPos);

            //unity中点击的位置  + (窗体位置 - unity窗体x的一半) = 点击屏幕上的位置

            Vector2 windowPosVector2 = new Vector2(windowPos.Left + Screen.width / 2, windowPos.Top + Screen.height / 2);

            float x = unityPos.x + windowPosVector2.x - Screen.width / 2;
            float y = Screen.height / 2 - unityPos.y + windowPosVector2.y;

            Vector2 targetPos = new Vector2(x, y);

            Debug.Log("unityPos=" + unityPos + ",targetPos=" + targetPos + ",windowPosVector2=" + windowPosVector2 + ",windowPos.Left=" + windowPos.Left + ",Top=" + windowPos.Top);

            return targetPos;
        }

        // 左键单击
        public static void LeftClick(float x = -1, float y = -1)
        {
            if (MoveTo(x, y))
            {
                mouse_event(MouseEventFlag.LeftDown, 0, 0, 0, UIntPtr.Zero);
                mouse_event(MouseEventFlag.LeftUp, 0, 0, 0, UIntPtr.Zero);
            }
        }

        public static void LeftClickDown()
        {
            mouse_event(MouseEventFlag.LeftDown, 0, 0, 0, UIntPtr.Zero);
        }

        public static void LeftClickUp()
        {
            mouse_event(MouseEventFlag.LeftUp, 0, 0, 0, UIntPtr.Zero);
        }

        // 右键单击
        public static void RightClick(float x = -1, float y = -1)
        {
            if (MoveTo(x, y))
            {
                mouse_event(MouseEventFlag.RightDown, 0, 0, 0, UIntPtr.Zero);
                mouse_event(MouseEventFlag.RightUp, 0, 0, 0, UIntPtr.Zero);
            }
        }

        public static void RightClickDown()
        {
            mouse_event(MouseEventFlag.RightDown, 0, 0, 0, UIntPtr.Zero);
        }

        public static void RightClickUp()
        {
            mouse_event(MouseEventFlag.RightUp, 0, 0, 0, UIntPtr.Zero);
        }

        // 中键单击
        public static void MiddleClick(float x = -1, float y = -1)
        {
            if (MoveTo(x, y))
            {
                mouse_event(MouseEventFlag.MiddleDown, 0, 0, 0, UIntPtr.Zero);
                mouse_event(MouseEventFlag.MiddleUp, 0, 0, 0, UIntPtr.Zero);
            }
        }



        // 中键按下
        public static void MiddleDown()
        {
            mouse_event(MouseEventFlag.MiddleDown, 0, 0, 0, UIntPtr.Zero);
        }

        // 中键抬起
        public static void MiddleUp()
        {
            mouse_event(MouseEventFlag.MiddleUp, 0, 0, 0, UIntPtr.Zero);
        }

        // 滚轮滚动
        public static void ScrollWheel(float value)
        {
            mouse_event(MouseEventFlag.Wheel, 0, 0, (uint)value, UIntPtr.Zero);
        }
    }

    public struct RECT
    {
        /// <summary>
        /// 窗体的左上角的点到屏幕最左边的x距离
        /// </summary>
        public int Left;        // x position of upper-left corner
                                /// <summary>
                                /// 窗体左上角的点到屏幕最上面的y轴距离
                                /// </summary>
        public int Top;         // y position of upper-left corner
                                /// <summary>
                                /// 窗体右下角的点到屏幕最右边的x轴距离
                                /// </summary>
        public int Right;       // x position of lower-right corner
                                /// <summary>
                                /// 窗体右下角的点到屏幕最右边的Y轴距离
                                /// </summary>
        public int Bottom;      // y position of lower-right corner
    }

    public struct POINT
    {
        public int X;
        public int Y;

        public POINT(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public override string ToString()
        {
            return ("X:" + X + ", Y:" + Y);
        }

        public bool Equal(POINT point)
        {
            if (this.X == point.X && this.Y == point.Y)
            {
                return true;
            }
            return false;
        }
    }

}