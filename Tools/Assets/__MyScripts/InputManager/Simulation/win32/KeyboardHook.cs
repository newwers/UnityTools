/*
 注意:Unity自身不支持直接在游戏运行时中全局监听键盘事件。这是因为Unity使用自己的输入系统来处理键盘事件，而不是依赖于Windows API的钩子机制
 */

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace TopGame
{
    public class KeyboardHook
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static LowLevelKeyboardProc _proc;
        private static IntPtr _hookID = IntPtr.Zero;

        public static event Action<int> KeyPressed;

        public static void Start()
        {
            _proc = HookCallback;
            _hookID = SetHook(_proc);
        }

        public static void Stop()
        {
            UnhookWindowsHookEx(_hookID);
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (ProcessModule module = Process.GetCurrentProcess().MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(module.ModuleName), 0);
            }
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                KeyPressed?.Invoke(vkCode);
                UnityEngine.Debug.Log($"keycode:{vkCode}");
            }

            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        #region Win32 API

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        #endregion
    }
}