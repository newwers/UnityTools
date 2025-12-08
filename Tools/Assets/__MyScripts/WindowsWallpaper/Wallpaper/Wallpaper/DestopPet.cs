using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x0200001D RID: 29
public class DestopPet : MonoBehaviour
{
    // Token: 0x06000112 RID: 274
    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    // Token: 0x06000113 RID: 275
    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, ref DestopPet.RECT lpRect);

    // Token: 0x06000114 RID: 276
    [DllImport("user32.dll")]
    private static extern int SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int X, int Y, int cx, int cy, int uFlags);

    // Token: 0x06000115 RID: 277
    [DllImport("Dwmapi.dll")]
    private static extern uint DwmExtendFrameIntoClientArea(IntPtr hWnd, ref DestopPet.MARGINS margins);

    // Token: 0x06000116 RID: 278
    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, ulong dwNewLong);

    // Token: 0x06000117 RID: 279
    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    // Token: 0x06000118 RID: 280 RVA: 0x00006E20 File Offset: 0x00005020
    private void OnEnable()
    {
        WindowController.SaveWindowInfo();
        this.hwnd = DestopPet.FindWindow("UnityWndClass", "Fischer'sFishingJourney");
        int num = DestopPet.GetWindowLong(this.hwnd, -16);
        num |= 131072;
        DestopPet.SetWindowLong(this.hwnd, -16, (ulong)((long)num));
        base.Invoke("SetWindowStyle", 0.1f);
        this.petModeDrag.SetActive(true);
        //this.spot = this.spotContent.transform.GetChild(0).GetComponent<Spot>();
        //this.spot.transform.Find("Mask").transform.Find("NormalMode").gameObject.SetActive(false);
        //for (int i = 0; i < this.spot.petModeMaterials.Count; i++)
        //{
        //    this.spot.petModeMaterials[i].EnableKeyword("_ENABLEMASK_ON");
        //}
        //this.mainCamera.orthographicSize = this.spot.orthographicSize;
        //this.mainCamera.transform.position = new Vector3(this.spot.cameraPosition.x, this.spot.cameraPosition.y, -10f);
        this.mask.color = new Color(0f, 0f, 0f, 0f);
        GameObject.Find("UI/HUD/ani/UpperLeft/time").GetComponent<Canvas>().overrideSorting = false;
        GameObject.Find("UI/HUD/ani/UpperRight2/offset3/coin").GetComponent<Canvas>().overrideSorting = false;
        //Main.model.settingModel.windowState = WindowState.TablePet;
    }

    // Token: 0x06000119 RID: 281 RVA: 0x00006FB8 File Offset: 0x000051B8
    private void OnDisable()
    {
        WindowController.SaveWindowInfo();
        this.petModeDrag.SetActive(false);
        //this.spot.transform.Find("Mask").transform.Find("NormalMode").gameObject.SetActive(true);
        //for (int i = 0; i < this.spot.petModeMaterials.Count; i++)
        //{
        //    this.spot.petModeMaterials[i].DisableKeyword("_ENABLEMASK_ON");
        //}
        this.mainCamera.orthographicSize = 7.186449f;
        this.mainCamera.transform.position = new Vector3(0f, 0f, -10f);
        this.UIAni.Play("UIShow");
        ulong dwNewLong = 0;// (ulong)-1777991680;
        ulong dwNewLong2 = 256UL;
        DestopPet.SetWindowLong(this.hwnd, -16, dwNewLong);
        DestopPet.SetWindowLong(this.hwnd, -20, dwNewLong2);
        base.Invoke("SetNormalWindowStyle", 0.1f);
        this.mask.color = new Color(0f, 0f, 0f, 0.5f);
        GameObject.Find("UI/HUD/ani/UpperLeft/time").GetComponent<Canvas>().overrideSorting = true;
        GameObject.Find("UI/HUD/ani/UpperRight2/offset3/coin").GetComponent<Canvas>().overrideSorting = true;
        this.ApplyDwmFix(1);
        //Main.model.settingModel.windowState = WindowState.Normal;
    }

    // Token: 0x0600011A RID: 282 RVA: 0x00007124 File Offset: 0x00005324
    private void ApplyDwmFix(int inset)
    {
        DestopPet.MARGINS margins = new DestopPet.MARGINS
        {
            Left = inset,
            Right = inset,
            Top = inset,
            Bottom = inset
        };
        DestopPet.DwmExtendFrameIntoClientArea(this.hwnd, ref margins);
    }

    // Token: 0x0600011B RID: 283 RVA: 0x0000716C File Offset: 0x0000536C
    private void SetWindowStyle()
    {
        //if (Main.model.settingModel.windowInfo.Top)
        //{
        //    DestopPet.SetWindowPos(this.hwnd, -1, 0, 0, Main.model.settingModel.windowInfo.width, Main.model.settingModel.windowInfo.height, 2);
        //}
        //if (Main.model.settingModel.windowInfo.Top)
        //{
        //    DestopPet.SetWindowPos(this.hwnd, 0, 0, 0, Main.model.settingModel.windowInfo.width, Main.model.settingModel.windowInfo.height, 6);
        //}
    }

    // Token: 0x0600011C RID: 284 RVA: 0x00007218 File Offset: 0x00005418
    private void SetNormalWindowStyle()
    {
        //if (Main.model.settingModel.windowState != WindowState.Normal)
        //{
        //    return;
        //}
        //DestopPet.SetWindowPos(this.hwnd, 0, 0, 0, Main.model.settingModel.windowInfo.width, Main.model.settingModel.windowInfo.height, 70);
    }

    // Token: 0x040000DD RID: 221
    public GameObject petModeDrag;

    // Token: 0x040000DE RID: 222
    public GameObject spotContent;

    // Token: 0x040000DF RID: 223
    public Animation UIAni;

    // Token: 0x040000E0 RID: 224
    public Image mask;

    // Token: 0x040000E1 RID: 225
    public Camera mainCamera;

    // Token: 0x040000E2 RID: 226
    public RectTransform pageLayer;

    // Token: 0x040000E3 RID: 227
    private const int SWP_SHOWWINDOW = 64;

    // Token: 0x040000E4 RID: 228
    private static int SWP_FRAMECHANGED = 32;

    // Token: 0x040000E5 RID: 229
    private const int GWL_EXSTYLE = -20;

    // Token: 0x040000E6 RID: 230
    private const int GWL_STYLE = -16;

    // Token: 0x040000E7 RID: 231
    private const int SWP_NOMOVE = 2;

    // Token: 0x040000E8 RID: 232
    private const int SWP_NOSIZE = 1;

    // Token: 0x040000E9 RID: 233
    private const int SWP_NOZORDER = 4;

    // Token: 0x040000EA RID: 234
    private static DestopPet.RECT rect;

    // Token: 0x040000EB RID: 235
    private IntPtr hwnd;

    // Token: 0x040000EC RID: 236
    //private Spot spot;

    // Token: 0x020000A8 RID: 168
    private struct RECT
    {
        // Token: 0x0400050E RID: 1294
        public int Left;

        // Token: 0x0400050F RID: 1295
        public int Top;

        // Token: 0x04000510 RID: 1296
        public int Right;

        // Token: 0x04000511 RID: 1297
        public int Bottom;
    }

    // Token: 0x020000A9 RID: 169
    private struct MARGINS
    {
        // Token: 0x060005D3 RID: 1491 RVA: 0x0002282C File Offset: 0x00020A2C
        public MARGINS(int all)
        {
            this.Bottom = all;
            this.Top = all;
            this.Right = all;
            this.Left = all;
        }

        // Token: 0x04000512 RID: 1298
        public int Left;

        // Token: 0x04000513 RID: 1299
        public int Right;

        // Token: 0x04000514 RID: 1300
        public int Top;

        // Token: 0x04000515 RID: 1301
        public int Bottom;
    }
}
