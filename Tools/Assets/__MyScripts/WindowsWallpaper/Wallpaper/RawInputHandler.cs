using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

// Token: 0x02000028 RID: 40
public class RawInputHandler
{
    // Token: 0x0600014D RID: 333
    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool RegisterRawInputDevices([In] RawInputHandler.RAWINPUTDEVICE[] pRawInputDevices, uint uiNumDevices, uint cbSize);

    // Token: 0x0600014E RID: 334
    [DllImport("user32.dll", SetLastError = true)]
    public static extern uint GetRawInputData(IntPtr hRawInput, uint uiCommand, IntPtr pData, ref uint pcbSize, uint cbSizeHeader);

    // Token: 0x0600014F RID: 335
    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    // Token: 0x06000150 RID: 336
    [DllImport("user32.dll")]
    private static extern IntPtr WindowFromPoint(RawInputHandler.POINT pt);

    // Token: 0x06000151 RID: 337
    [DllImport("user32.dll")]
    public static extern IntPtr SetFocus(IntPtr hWnd);

    // Token: 0x06000152 RID: 338
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetCursorPos(out RawInputHandler.POINT lpPoint);

    // Token: 0x06000153 RID: 339
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool ScreenToClient(IntPtr hWnd, ref RawInputHandler.POINT lpPoint);

    // Token: 0x06000154 RID: 340 RVA: 0x00007B68 File Offset: 0x00005D68
    public static IntPtr MakeLParam(int x, int y)
    {
        uint num = (uint)(x & 65535);
        return (IntPtr)((long)((ulong)((y & 65535) << 16 | (int)num)));
    }

    // Token: 0x06000155 RID: 341 RVA: 0x00007B8F File Offset: 0x00005D8F
    public static IntPtr MakeWParam_MouseWheel(short wheelDelta)
    {
        return (IntPtr)((long)((ulong)((ulong)((ushort)wheelDelta) << 16)));
    }

    // Token: 0x06000157 RID: 343 RVA: 0x00007BA4 File Offset: 0x00005DA4
    public void RegisterDevices(IntPtr hwnd)
    {
        RawInputHandler.RAWINPUTDEVICE[] array = new RawInputHandler.RAWINPUTDEVICE[1];
        array[0].usUsagePage = 1;
        array[0].usUsage = 2;
        array[0].dwFlags = 48U;
        array[0].hwndTarget = hwnd;
        array[0].dwFlags = 256U;
        if (!RawInputHandler.RegisterRawInputDevices(array, (uint)array.Length, (uint)Marshal.SizeOf(typeof(RawInputHandler.RAWINPUTDEVICE))))
        {
            throw new Win32Exception(Marshal.GetLastWin32Error(), "RegisterRawInputDevices 失败");
        }
    }

    // Token: 0x06000158 RID: 344 RVA: 0x00007C28 File Offset: 0x00005E28
    public void ProcessRawInputMessage(IntPtr lParam)
    {
        uint num = 0U;
        RawInputHandler.GetRawInputData(lParam, 268435459U, IntPtr.Zero, ref num, (uint)Marshal.SizeOf(typeof(RawInputHandler.RAWINPUTHEADER)));
        if (num == 0U)
        {
            return;
        }
        IntPtr intPtr = Marshal.AllocHGlobal((int)num);
        try
        {
            uint num2 = num;
            if (RawInputHandler.GetRawInputData(lParam, 268435459U, intPtr, ref num2, (uint)Marshal.SizeOf(typeof(RawInputHandler.RAWINPUTHEADER))) == num2)
            {
                RawInputHandler.RAWINPUT rawinput = (RawInputHandler.RAWINPUT)Marshal.PtrToStructure(intPtr, typeof(RawInputHandler.RAWINPUT));
                if (rawinput.header.dwType == 0U)
                {
                    ushort usButtonFlags = rawinput.mouse.usButtonFlags;
                    bool flag = (usButtonFlags & 1) > 0;
                    bool flag2 = (usButtonFlags & 2) > 0;
                    RawInputHandler.POINT pt;
                    RawInputHandler.GetCursorPos(out pt);
                    if (this.IsMouseOnDesktop(pt))
                    {
                        if (flag)
                        {
                            UnityEngine.Debug.Log("鼠标左键按下,lParam:" + lParam);
                            RawInputHandler.PostMessage(Wallpaper.unityHwnd, 513U, IntPtr.Zero, lParam);
                        }
                        else if (flag2)
                        {
                            UnityEngine.Debug.Log("鼠标左键抬起,lParam:" + lParam);
                            RawInputHandler.PostMessage(Wallpaper.unityHwnd, 514U, IntPtr.Zero, lParam);
                        }
                    }
                }
            }
        }
        finally
        {
            Marshal.FreeHGlobal(intPtr);
        }
    }

    // Token: 0x06000159 RID: 345 RVA: 0x00007D2C File Offset: 0x00005F2C
    private bool IsMouseOnDesktop(RawInputHandler.POINT pt)
    {
        IntPtr value = RawInputHandler.WindowFromPoint(pt);
        return value == Wallpaper.clickWindow1 || value == Wallpaper.clickWindow2;
    }

    // Token: 0x0600015A RID: 346 RVA: 0x00007D60 File Offset: 0x00005F60
    public void RemoveRegister()
    {
        RawInputHandler.RAWINPUTDEVICE[] array = new RawInputHandler.RAWINPUTDEVICE[1];
        array[0].usUsagePage = 1;
        array[0].usUsage = 2;
        array[0].dwFlags = 256U;
        array[0].hwndTarget = Wallpaper.unityHwnd;// WindowController.myintptr;
        RawInputHandler.RegisterRawInputDevices(array, (uint)array.Length, (uint)Marshal.SizeOf(typeof(RawInputHandler.RAWINPUTDEVICE)));
    }

    // Token: 0x0400010D RID: 269
    private const int RIM_TYPEMOUSE = 0;

    // Token: 0x0400010E RID: 270
    private const uint RID_INPUT = 268435459U;

    // Token: 0x0400010F RID: 271
    private const uint RIDEV_NOLEGACY = 48U;

    // Token: 0x04000110 RID: 272
    private const uint RIDEV_INPUTSINK = 256U;

    // Token: 0x04000111 RID: 273
    public const int RI_MOUSE_MIDDLE_BUTTON_DOWN = 16;

    // Token: 0x04000112 RID: 274
    public const int RI_MOUSE_MIDDLE_BUTTON_UP = 32;

    // Token: 0x04000113 RID: 275
    public const int RI_MOUSE_WHEEL = 1024;

    // Token: 0x04000114 RID: 276
    private const int WM_LBUTTONDOWN = 513;

    // Token: 0x04000115 RID: 277
    private const int WM_LBUTTONUP = 514;

    // Token: 0x04000116 RID: 278
    private const int WM_MOUSEMOVE = 512;

    // Token: 0x04000117 RID: 279
    private const int WM_MOUSEWHEEL = 522;

    // Token: 0x020000B0 RID: 176
    public struct POINT
    {
        // Token: 0x04000524 RID: 1316
        public int X;

        // Token: 0x04000525 RID: 1317
        public int Y;
    }

    // Token: 0x020000B1 RID: 177
    public struct RAWINPUTDEVICE
    {
        // Token: 0x04000526 RID: 1318
        public ushort usUsagePage;

        // Token: 0x04000527 RID: 1319
        public ushort usUsage;

        // Token: 0x04000528 RID: 1320
        public uint dwFlags;

        // Token: 0x04000529 RID: 1321
        public IntPtr hwndTarget;
    }

    // Token: 0x020000B2 RID: 178
    public struct RAWINPUTHEADER
    {
        // Token: 0x0400052A RID: 1322
        public uint dwType;

        // Token: 0x0400052B RID: 1323
        public uint dwSize;

        // Token: 0x0400052C RID: 1324
        public IntPtr hDevice;

        // Token: 0x0400052D RID: 1325
        public IntPtr wParam;
    }

    // Token: 0x020000B3 RID: 179
    public struct RAWMOUSE
    {
        // Token: 0x170000C9 RID: 201
        // (get) Token: 0x060005DC RID: 1500 RVA: 0x0002285B File Offset: 0x00020A5B
        public ushort usButtonFlags
        {
            get
            {
                return (ushort)(this._buttonData & 65535U);
            }
        }

        // Token: 0x170000CA RID: 202
        // (get) Token: 0x060005DD RID: 1501 RVA: 0x0002286A File Offset: 0x00020A6A
        public ushort usButtonData
        {
            get
            {
                return (ushort)(this._buttonData >> 16);
            }
        }

        // Token: 0x0400052E RID: 1326
        public ushort usFlags;

        // Token: 0x0400052F RID: 1327
        private readonly uint _buttonData;

        // Token: 0x04000530 RID: 1328
        public uint ulRawButtons;

        // Token: 0x04000531 RID: 1329
        public int lLastX;

        // Token: 0x04000532 RID: 1330
        public int lLastY;

        // Token: 0x04000533 RID: 1331
        public uint ulExtraInformation;
    }

    // Token: 0x020000B4 RID: 180
    [StructLayout(LayoutKind.Explicit)]
    public struct RAWINPUT
    {
        // Token: 0x04000534 RID: 1332
        [FieldOffset(0)]
        public RawInputHandler.RAWINPUTHEADER header;

        // Token: 0x04000535 RID: 1333
        [FieldOffset(24)]
        public RawInputHandler.RAWMOUSE mouse;
    }
}
