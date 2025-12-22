using System.Collections;
using UnityEngine;

public class WindowModeManager : MonoBehaviour
{
    [Header("组件引用")]
    public TranspareWindows transpareWindows;
    public Wallpaper wallpaper;

    [Header("显示模式")]
    [Tooltip("当前显示模式")]
    public DisplayMode currentDisplayMode = DisplayMode.Fullscreen;

    public enum DisplayMode
    {
        Windowed,       // 窗口化模式
        Fullscreen,     // 全屏模式
        Wallpaper,      // 壁纸模式
        Transparent     // 透明穿透模式
    }


    IEnumerator Start()
    {
        yield return null;
        // 初始化显示模式
        SetDisplayMode(currentDisplayMode);
    }


    /// <summary>
    /// 设置显示模式
    /// </summary>
    public void SetDisplayMode(DisplayMode mode)
    {
        currentDisplayMode = mode;

        switch (mode)
        {
            case DisplayMode.Windowed:
                SetWindowedMode();
                break;
            case DisplayMode.Fullscreen:
                SetFullscreenMode();
                break;
            case DisplayMode.Wallpaper:
                SetWallpaperMode();
                break;
            case DisplayMode.Transparent:
                SetTransparentMode();
                break;
        }

        Debug.Log($"已切换到 {mode} 模式");
    }

    /// <summary>
    /// 窗口化模式
    /// </summary>
    private void SetWindowedMode()
    {
        // 退出壁纸模式
        if (wallpaper != null && Wallpaper.isWallpaperMode)
        {
            wallpaper.ExitWallparer();
        }

        // 禁用透明穿透
        if (transpareWindows.isSetTranspareWindows)
        {
            transpareWindows.ExitTranspareWindows();
            // 可以在这里添加代码来恢复窗口的正常样式
        }

        // 设置Unity内置的窗口化
        Screen.fullScreen = false;
        // 可以根据需要设置窗口大小
        Screen.SetResolution(Screen.width, Screen.height, false);
    }

    /// <summary>
    /// 全屏模式
    /// </summary>
    private void SetFullscreenMode()
    {
        if (transpareWindows.isSetTranspareWindows)
        {
            transpareWindows.ExitTranspareWindows();
            // 可以在这里添加代码来恢复窗口的正常样式
        }

        // 退出壁纸模式
        if (wallpaper != null && Wallpaper.isWallpaperMode)
        {
            wallpaper.ExitWallparer();
        }

        // 使用Wallpaper脚本的全屏方法（更稳定）
        if (wallpaper != null)
        {
            wallpaper.StartCoroutine(wallpaper.DelayFullScreen());
        }
        else
        {
            // 备用方案：使用Unity内置全屏
            Screen.fullScreen = true;
            Screen.SetResolution(Display.main.systemWidth, Display.main.systemHeight, true);
        }
    }

    /// <summary>
    /// 壁纸模式
    /// </summary>
    private void SetWallpaperMode()
    {
        if (wallpaper == null)
        {
            Debug.LogError("Wallpaper组件未找到，无法进入壁纸模式");
            return;
        }

        if (transpareWindows.isSetTranspareWindows)
        {
            transpareWindows.ExitTranspareWindows();
            // 可以在这里添加代码来恢复窗口的正常样式
        }

        // 如果是第一次进入壁纸模式，使用特殊处理
        wallpaper.SetWallpaper();
    }

    /// <summary>
    /// 透明穿透模式
    /// </summary>
    private void SetTransparentMode()
    {
        // 退出壁纸模式
        if (wallpaper != null && Wallpaper.isWallpaperMode)
        {
            wallpaper.ExitWallparer();
        }

        // 设置窗口化作为基础
        //SetWindowedMode();

        // 启用透明穿透
        if (transpareWindows != null)
        {
            transpareWindows.SetTranspareWindows();
        }
    }



    /// <summary>
    /// 公共方法 - 供UI按钮调用
    /// </summary>
    public void SetWindowedModeUI()
    {
        SetDisplayMode(DisplayMode.Windowed);
    }

    public void SetFullscreenModeUI()
    {
        SetDisplayMode(DisplayMode.Fullscreen);
    }

    public void SetWallpaperModeUI()
    {
        SetDisplayMode(DisplayMode.Wallpaper);
    }

    public void SetTransparentModeUI()
    {
        SetDisplayMode(DisplayMode.Transparent);
    }

    /// <summary>
    /// 切换透明穿透的开关状态
    /// </summary>
    public void ToggleClickThrough(bool enabled)
    {
        if (transpareWindows != null)
        {
            // 这里需要根据TranspareWindows脚本的实际实现来调整
            // 可能需要添加一个公共方法来控制穿透开关
            transpareWindows.SetWindowTop(enabled);
        }
    }
}