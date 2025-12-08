using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class RawMouseInput : MonoBehaviour
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RAWINPUTDEVICE
    {
        public ushort usUsagePage;
        public ushort usUsage;
        public uint dwFlags;
        public IntPtr hwndTarget;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RAWINPUTHEADER
    {
        public uint dwType;
        public uint dwSize;
        public IntPtr hDevice;
        public IntPtr wParam;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct RAWMOUSE
    {
        [FieldOffset(0)]
        public ushort usFlags;
        [FieldOffset(4)]
        public uint ulButtons;
        [FieldOffset(8)]
        public int lLastX;
        [FieldOffset(12)]
        public int lLastY;
        [FieldOffset(16)]
        public uint ulExtraInformation;
    }

    [DllImport("user32.dll")]
    private static extern uint GetRawInputData(IntPtr hRawInput, uint uiCommand,
        IntPtr pData, ref uint pcbSize, uint cbSizeHeader);

    [DllImport("user32.dll")]
    private static extern uint GetRawInputDeviceList(IntPtr pRawInputDeviceList,
        ref uint uiNumDevices, uint cbSize);

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    private const int RIM_TYPEMOUSE = 0;
    private const int RI_MOUSE_LEFT_BUTTON_DOWN = 0x0001;
    private const int RI_MOUSE_LEFT_BUTTON_UP = 0x0002;

    private void Update()
    {
        // 在Update中处理消息
        // 注意：在Unity中直接使用Windows消息循环比较复杂
        // 通常需要额外的消息处理机制
    }

    // 处理原始输入的回调函数示例
    private void ProcessRawInput(IntPtr lParam)
    {
        uint dwSize = 0;
        GetRawInputData(lParam, 0x10000003, IntPtr.Zero, ref dwSize, (uint)Marshal.SizeOf(typeof(RAWINPUTHEADER)));

        if (dwSize > 0)
        {
            IntPtr buffer = Marshal.AllocHGlobal((int)dwSize);
            try
            {
                if (GetRawInputData(lParam, 0x10000003, buffer, ref dwSize, (uint)Marshal.SizeOf(typeof(RAWINPUTHEADER))) == dwSize)
                {
                    RAWINPUTHEADER header = (RAWINPUTHEADER)Marshal.PtrToStructure(buffer, typeof(RAWINPUTHEADER));

                    if (header.dwType == RIM_TYPEMOUSE)
                    {
                        RAWMOUSE mouse = (RAWMOUSE)Marshal.PtrToStructure(buffer + Marshal.SizeOf(typeof(RAWINPUTHEADER)), typeof(RAWMOUSE));

                        // 检测左键按下
                        if ((mouse.ulButtons & RI_MOUSE_LEFT_BUTTON_DOWN) != 0)
                        {
                            Debug.Log("原始输入：鼠标左键按下");
                        }

                        // 检测左键松开
                        if ((mouse.ulButtons & RI_MOUSE_LEFT_BUTTON_UP) != 0)
                        {
                            Debug.Log("原始输入：鼠标左键松开");
                        }
                    }
                }
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }
    }
}