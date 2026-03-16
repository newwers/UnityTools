// The SteamManager is designed to work with Steamworks.NET
// This file is released into the public domain.
// Where that dedication is not recognized you are granted a perpetual,
// irrevocable license to copy and modify this file as you see fit.
//
// Version: 1.0.13

#if !(UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || STEAMWORKS_WIN || STEAMWORKS_LIN_OSX)
#define DISABLESTEAMWORKS
#endif

using UnityEngine;
#if !DISABLESTEAMWORKS
using Steamworks;
using System.Drawing;
using System.Drawing.Imaging;
using System;
using System.Runtime.InteropServices;
#endif

//
// The SteamManager provides a base implementation of Steamworks.NET on which you can build upon.
// It handles the basics of starting up and shutting down the SteamAPI for use.
//
[DisallowMultipleComponent]
public class SteamManager : MonoBehaviour
{
#if !DISABLESTEAMWORKS
    protected static bool s_EverInitialized = false;

    protected static SteamManager s_instance;
    protected static SteamManager Instance
    {
        get
        {
            if (s_instance == null)
            {
                return new GameObject("SteamManager").AddComponent<SteamManager>();
            }
            else
            {
                return s_instance;
            }
        }
    }

    protected bool m_bInitialized = false;
    public static bool Initialized
    {
        get
        {
            return Instance.m_bInitialized;
        }
    }

    protected SteamAPIWarningMessageHook_t m_SteamAPIWarningMessageHook;

    [AOT.MonoPInvokeCallback(typeof(SteamAPIWarningMessageHook_t))]
    protected static void SteamAPIDebugTextHook(int nSeverity, System.Text.StringBuilder pchDebugText)
    {
        Debug.LogWarning(pchDebugText);
    }

#if UNITY_2019_3_OR_NEWER
    // In case of disabled Domain Reload, reset static members before entering Play Mode.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void InitOnPlayMode()
    {
        s_EverInitialized = false;
        s_instance = null;
    }
#endif

    protected virtual void Awake()
    {
        // Only one instance of SteamManager at a time!
        if (s_instance != null)
        {
            Destroy(gameObject);
            return;
        }
        s_instance = this;

        if (s_EverInitialized)
        {
            // This is almost always an error.
            // The most common case where this happens is when SteamManager gets destroyed because of Application.Quit(),
            // and then some Steamworks code in some other OnDestroy gets called afterwards, creating a new SteamManager.
            // You should never call Steamworks functions in OnDestroy, always prefer OnDisable if possible.
            throw new System.Exception("Tried to Initialize the SteamAPI twice in one session!");
        }

        // We want our SteamManager Instance to persist across scenes.
        DontDestroyOnLoad(gameObject);

        if (!Packsize.Test())
        {
            Debug.LogError("[Steamworks.NET] Packsize Test returned false, the wrong version of Steamworks.NET is being run in this platform.", this);
        }

        if (!DllCheck.Test())
        {
            Debug.LogError("[Steamworks.NET] DllCheck Test returned false, One or more of the Steamworks binaries seems to be the wrong version.", this);
        }

        try
        {
            // If Steam is not running or the game wasn't started through Steam, SteamAPI_RestartAppIfNecessary starts the
            // Steam client and also launches this game again if the User owns it. This can act as a rudimentary form of DRM.
            // Note that this will run which ever version you have installed in steam. Which may not be the precise executable
            // we were currently running.

            // Once you get a Steam AppID assigned by Valve, you need to replace AppId_t.Invalid with it and
            // remove steam_appid.txt from the game depot. eg: "(AppId_t)480" or "new AppId_t(480)".
            // See the Valve documentation for more information: https://partner.steamgames.com/doc/sdk/api#initialization_and_shutdown
            if (SteamAPI.RestartAppIfNecessary((AppId_t)AppId_t.Invalid))
            {
                //(AppId_t)4265700 Demo
                //AppId_t.Invalid
                //正式版：4211860
                Debug.Log("[Steamworks.NET] Shutting down because RestartAppIfNecessary returned true. Steam will restart the application.");

                Application.Quit();
                return;
            }
        }
        catch (System.DllNotFoundException e)
        { // We catch this exception here, as it will be the first occurrence of it.
            Debug.LogError("[Steamworks.NET] Could not load [lib]steam_api.dll/so/dylib. It's likely not in the correct location. Refer to the README for more details.\n" + e, this);

            Application.Quit();
            return;
        }

        // Initializes the Steamworks API.
        // If this returns false then this indicates one of the following conditions:
        // [*] The Steam client isn't running. A running Steam client is required to provide implementations of the various Steamworks interfaces.
        // [*] The Steam client couldn't determine the App ID of game. If you're running your application from the executable or debugger directly then you must have a [code-inline]steam_appid.txt[/code-inline] in your game directory next to the executable, with your app ID in it and nothing else. Steam will look for this file in the current working directory. If you are running your executable from a different directory you may need to relocate the [code-inline]steam_appid.txt[/code-inline] file.
        // [*] Your application is not running under the same OS user context as the Steam client, such as a different user or administration access level.
        // [*] Ensure that you own a license for the App ID on the currently active Steam account. Your game must show up in your Steam library.
        // [*] Your App ID is not completely set up, i.e. in Release State: Unavailable, or it's missing default packages.
        // Valve's documentation for this is located here:
        // https://partner.steamgames.com/doc/sdk/api#initialization_and_shutdown
        m_bInitialized = SteamAPI.Init();
        if (!m_bInitialized)
        {
            Debug.LogError("[Steamworks.NET] SteamAPI_Init() failed. Refer to Valve's documentation or the comment above this line for more information.", this);

            return;
        }

        s_EverInitialized = true;
    }

    // This should only ever get called on first load and after an Assembly reload, You should never Disable the Steamworks Manager yourself.
    protected virtual void OnEnable()
    {
        if (s_instance == null)
        {
            s_instance = this;
        }

        if (!m_bInitialized)
        {
            return;
        }

        if (m_SteamAPIWarningMessageHook == null)
        {
            // Set up our callback to receive warning messages from Steam.
            // You must launch with "-debug_steamapi" in the launch args to receive warnings.
            m_SteamAPIWarningMessageHook = new SteamAPIWarningMessageHook_t(SteamAPIDebugTextHook);
            SteamClient.SetWarningMessageHook(m_SteamAPIWarningMessageHook);
        }

    }

    // OnApplicationQuit gets called too early to shutdown the SteamAPI.
    // Because the SteamManager should be persistent and never disabled or destroyed we can shutdown the SteamAPI here.
    // Thus it is not recommended to perform any Steamworks work in other OnDestroy functions as the order of execution can not be garenteed upon Shutdown. Prefer OnDisable().
    protected virtual void OnDestroy()
    {
        if (s_instance != this)
        {
            return;
        }

        s_instance = null;

        if (!m_bInitialized)
        {
            return;
        }

        SteamAPI.Shutdown();
    }

    protected virtual void Update()
    {
        if (!m_bInitialized)
        {
            return;
        }

        // Run Steam client callbacks
        SteamAPI.RunCallbacks();
    }

    /// <summary>
    /// steam截图功能
    /// </summary>
    public void TriggerScreenshot()
    {
        LogManager.Log("触发截图功能1");
        if (!Initialized)
        {
            Debug.LogWarning("Steam未初始化，无法调用截图功能！");
            return;
        }

        // 调用Steam内置截图功能（关键API）
        // 参数说明：
        // - 第一个bool：是否显示Steam截图成功的提示弹窗（true显示，false不显示）
        // - 第二个bool：是否在截图时包含Steam覆盖层（false不包含，true包含）
        SteamScreenshots.TriggerScreenshot();
        LogManager.Log("触发截图功能2");
    }

    /// <summary>
    /// 截取整个显示器屏幕（Windows专用）并且提交给Steam作为截图（需要Steamworks.NET支持）
    /// </summary>
    public void TakeFullScreenScreenshot()
    {
        try
        {
            // 1. 获取主显示器的分辨率
            int screenWidth = Screen.width; // Unity窗口宽度（若要截整个显示器，用下面的System.Windows.Forms）
            int screenHeight = Screen.height;

            // 【关键】获取整个显示器的真实分辨率（而非Unity窗口大小）
            // 需额外引用System.Windows.Forms（可选，若只用Unity窗口大小可跳过）
            // 若要截所有显示器，需遍历Screen.AllScreens

            var primaryScreen = System.Windows.Forms.Screen.AllScreens[0];
            screenWidth = primaryScreen.Bounds.Width;
            screenHeight = primaryScreen.Bounds.Height;


            // 2. 创建Bitmap对象（用于存储屏幕像素）
            using (Bitmap bitmap = new Bitmap(screenWidth, screenHeight, PixelFormat.Format24bppRgb))
            {
                // 3. 创建Graphics对象，从屏幕复制像素
                using (System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(bitmap))
                {
                    // 核心方法：CopyFromScreen(源起点X, 源起点Y, 目标起点X, 目标起点Y, 截图大小)
                    graphics.CopyFromScreen(
                        sourceX: 0,
                        sourceY: 0,
                        destinationX: 0,
                        destinationY: 0,
                        blockRegionSize: new Size(screenWidth, screenHeight),
                        copyPixelOperation: CopyPixelOperation.SourceCopy
                    );
                }


                // 4. 【关键】将Bitmap转换为Steam需要的RGB字节数组
                // Steam要求：RGB格式、从上到下、无对齐
                byte[] steamRgbData = ConvertBitmapToSteamRgb(bitmap, screenWidth, screenHeight);

                // 5. 提交截图到Steam
                // 参数：像素数据指针、数据长度、宽度、高度
                GCHandle handle = GCHandle.Alloc(steamRgbData, GCHandleType.Pinned);
                try
                {
                    IntPtr ptr = handle.AddrOfPinnedObject();
                    var currentScreenshotHandle = SteamScreenshots.WriteScreenshot(
                        pubRGB: steamRgbData,
                        cubRGB: (uint)steamRgbData.Length,
                        nWidth: screenWidth,
                        nHeight: screenHeight
                    );

                    Debug.Log("已向Steam提交截图请求，等待回调...");
                }
                finally
                {
                    handle.Free();
                }



                //  保存截图到本地文件
                //string fileName = $"Screenshot_{System.DateTime.Now:yyyyMMdd_HHmmss}.png";
                //string savePath = Path.Combine(Application.persistentDataPath, fileName);

                //bitmap.Save(savePath, ImageFormat.Png);

                //Debug.Log($"全屏截图已保存到：{savePath}");
                //// 可选：打开截图所在文件夹
                //System.Diagnostics.Process.Start(savePath);

            }
        }
        catch (Exception e)
        {
            Debug.LogError($"全屏截图失败：{e.Message}");
        }
    }

    /// <summary>
    /// 转换Bitmap为Steam要求的RGB格式（处理BGR→RGB、上下翻转）
    /// </summary>
    private byte[] ConvertBitmapToSteamRgb(Bitmap bitmap, int width, int height)
    {
        // 锁定Bitmap的像素数据（直接操作内存更快）
        BitmapData bmpData = bitmap.LockBits(
            rect: new Rectangle(0, 0, width, height),
            flags: ImageLockMode.ReadOnly,
            format: PixelFormat.Format24bppRgb
        );

        try
        {
            // Steam需要的RGB数组（每个像素3字节：R, G, B）
            byte[] result = new byte[width * height * 3];

            // Bitmap的实际步长（Stride可能因对齐大于width*3）
            int stride = bmpData.Stride;
            IntPtr ptr = bmpData.Scan0;

            // 逐行复制像素（注意：Bitmap是从下到上存储的，Steam需要从上到下）
            for (int y = 0; y < height; y++)
            {
                // Bitmap的当前行指针（从最后一行开始，实现上下翻转）
                IntPtr srcRow = ptr + y * stride;//（正序读行，恢复正常方向）ptr + (height - 1 - y) * stride;倒序读行，导致上下翻转）
                // Result的当前行指针
                int dstRowOffset = y * width * 3;

                // 复制当前行的像素，并转换BGR→RGB
                for (int x = 0; x < width; x++)
                {
                    int srcOffset = x * 3;
                    int dstOffset = dstRowOffset + x * 3;

                    // Bitmap是BGR顺序，Steam需要RGB，所以交换B和R
                    result[dstOffset] = Marshal.ReadByte(srcRow, srcOffset + 2); // R
                    result[dstOffset + 1] = Marshal.ReadByte(srcRow, srcOffset + 1); // G
                    result[dstOffset + 2] = Marshal.ReadByte(srcRow, srcOffset); // B
                }
            }

            return result;
        }
        finally
        {
            bitmap.UnlockBits(bmpData);
        }
    }
#else
	public static bool Initialized {
		get {
			return false;
		}
	}
#endif // !DISABLESTEAMWORKS
}
