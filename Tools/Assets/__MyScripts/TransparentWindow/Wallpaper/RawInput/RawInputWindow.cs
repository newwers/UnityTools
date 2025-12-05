using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

public class RawInputWindow : NativeWindow, IDisposable
{
    // Token: 0x0600015B RID: 347
    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    // Token: 0x0600015C RID: 348 RVA: 0x00007DC8 File Offset: 0x00005FC8
    public RawInputWindow()
    {
        this.CreateHandle(new CreateParams
        {
            Caption = "RawInputMessageWindow",
            ClassName = null,
            Style = 0,
            ExStyle = 0,
            X = 0,
            Y = 0,
            Width = 0,
            Height = 0,
            Parent = IntPtr.Zero
        });
        this._handler = new RawInputHandler();
        this._handler.RegisterDevices(base.Handle);
    }

    // Token: 0x0600015D RID: 349 RVA: 0x00007E4B File Offset: 0x0000604B
    protected override void WndProc(ref Message m)
    {
        if (m.Msg == 255)
        {
            this._handler.ProcessRawInputMessage(m.LParam);
            return;
        }
        base.WndProc(ref m);
    }

    // Token: 0x0600015E RID: 350 RVA: 0x00007E73 File Offset: 0x00006073
    public void Dispose()
    {
        this._handler.RemoveRegister();
        this.DestroyHandle();
    }

    // Token: 0x04000118 RID: 280
    private const int WM_INPUT = 255;

    // Token: 0x04000119 RID: 281
    private RawInputHandler _handler;
}
