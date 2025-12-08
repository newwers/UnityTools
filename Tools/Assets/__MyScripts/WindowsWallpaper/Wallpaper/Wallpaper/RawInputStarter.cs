using UnityEngine;

public class RawInputStarter : MonoBehaviour
{
    // Token: 0x0600013A RID: 314 RVA: 0x000076E5 File Offset: 0x000058E5
    private void OnEnable()
    {
        print($"RawInputStarter OnEnable");
        this._rawWindow = new RawInputWindow();
    }

    // Token: 0x0600013B RID: 315 RVA: 0x000076F2 File Offset: 0x000058F2
    private void OnDisable()
    {
        if (this._rawWindow != null)
        {
            print("销毁RawInputWindow");
            this._rawWindow.Dispose();
            this._rawWindow = null;
        }
    }

    // Token: 0x04000100 RID: 256
    private RawInputWindow _rawWindow;
}