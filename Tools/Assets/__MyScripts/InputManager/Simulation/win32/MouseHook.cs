using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace TopGame
{

    public class MouseHook
    {
        private const int WH_MOUSE_LL = 14;
        private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_LBUTTONUP = 0x0202;
        private const int WM_RBUTTONDOWN = 0x0204;
        private const int WM_RBUTTONUP = 0x0205;

        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static LowLevelMouseProc _proc;
        private static IntPtr _hookID = IntPtr.Zero;

        public static event Action<int,bool,POINT> ButtonClick;

        public static void Start()
        {
            _proc = HookCallback;
            _hookID = SetHook(_proc);
        }

        public static void Stop()
        {
            UnhookWindowsHookEx(_hookID);
        }

        private static IntPtr SetHook(LowLevelMouseProc proc)
        {
            using (ProcessModule module = Process.GetCurrentProcess().MainModule)
            {
                return SetWindowsHookEx(WH_MOUSE_LL, proc, GetModuleHandle(module.ModuleName), 0);
            }
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && (wParam == (IntPtr)WM_LBUTTONDOWN || wParam == (IntPtr)WM_LBUTTONUP || wParam == (IntPtr)WM_RBUTTONDOWN || wParam == (IntPtr)WM_RBUTTONUP))
            {
                POINT point = default;
                GetCursorPos(ref point);

                switch ((int)wParam)
                {
                    case WM_LBUTTONDOWN:
                        ButtonClick?.Invoke(0, true, point);
                        UnityEngine.Debug.Log("LeftButtonDown");
                        break;
                    case WM_LBUTTONUP:
                        ButtonClick?.Invoke(0, false, point);
                        UnityEngine.Debug.Log("LeftButtonUP");
                        break;
                    case WM_RBUTTONDOWN:
                        ButtonClick?.Invoke(1, true, point);
                        UnityEngine.Debug.Log("RightButtonDown");
                        break;
                    case WM_RBUTTONUP:
                        ButtonClick?.Invoke(1, false, point);
                        UnityEngine.Debug.Log("RightButtonUP");
                        break;
                    default:
                        break;
                }

            }

            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        // Unity屏幕坐标从左下角开始，向右为X轴，向上为Y轴
        // Windows屏幕坐标从左上角开始，向右为X轴，向下为Y轴

        /// <summary>
        /// 移动鼠标到指定位置
        /// </summary>
        public static bool MoveTo(float x, float y)
        {
            if (x < 0 || y < 0)
                return false;

            SetCursorPos((int)x, (int)y);
            return true;
        }

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

        #region Win32 API

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll")]
        // GetCursorPos() makes everything possible
        public static extern bool GetCursorPos(ref POINT lpPoint);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int SetCursorPos(int x, int y);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern void mouse_event(MouseEventFlag dwFlags, int dx, int dy, uint dwData, UIntPtr dwExtraInfo);

        #endregion

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
}