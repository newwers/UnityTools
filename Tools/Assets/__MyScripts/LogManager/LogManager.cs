using System.Text;
using UnityEngine;

public static class LogManager
{
    // 日志开关设置
    public static bool EnableLogs { get; set; } = true;      // 控制普通日志
    public static bool EnableWarnings { get; set; } = true;   // 控制警告日志
    public static bool EnableErrors { get; set; } = true;     // 控制错误日志

    // 获取时间前缀 [时时分分秒秒]
    private static string GetTimePrefix()
    {
        System.DateTime now = System.DateTime.Now;
        return string.Format("[{0:D2}:{1:D2}:{2:D2}]", now.Hour, now.Minute, now.Second);
    }

    // 初始化方法（在游戏启动时自动调用）
    //    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    //    private static void Initialize()
    //    {
    //#if !UNITY_EDITOR
    //        // 发布版本默认关闭普通日志（可通过代码动态开启）
    //        EnableLogs = false;
    //#endif
    //    }

    // 普通日志输出
    public static void Log(object message)
    {
        if (EnableLogs) Debug.Log($"{GetTimePrefix()} {message}");
    }

    public static void Log(object message, Object context)
    {
        if (EnableLogs) Debug.Log($"{GetTimePrefix()} {message}", context);
    }

    // 警告日志输出
    public static void LogWarning(object message)
    {
        if (EnableWarnings) Debug.LogWarning($"{GetTimePrefix()} {message}");
    }

    public static void LogWarning(object message, Object context)
    {
        if (EnableWarnings) Debug.LogWarning($"{GetTimePrefix()} {message}", context);
    }

    // 错误日志输出
    public static void LogError(object message)
    {
        if (EnableErrors) Debug.LogError($"{GetTimePrefix()} {message}");
    }

    public static void LogError(object message, Object context)
    {
        if (EnableErrors) Debug.LogError($"{GetTimePrefix()} {message}", context);
    }

    // 带标签的日志方法（可选）
    public static void TaggedLog(string tag, object message)
    {
        if (EnableLogs) Debug.Log($"{GetTimePrefix()} [{tag}] {message}");
    }

    //打印红色log
    public static void Log_Red(object message)
    {
        if (EnableLogs) Debug.Log($"<color=red>{GetTimePrefix()} {message}</color>");
    }
    //打印绿色log
    public static void Log_Green(object message)
    {
        if (EnableLogs) Debug.Log($"<color=green>{GetTimePrefix()} {message}</color>");
    }

    public static void PhoneSystemInfo()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("********************************************** Start ***********************************************************");
        sb.AppendLine("游戏版本号:" + Application.version);
        sb.AppendLine("By " + SystemInfo.deviceName);
        System.DateTime now = System.DateTime.Now;
        sb.AppendLine(string.Concat(new object[]
        {
                now.Year.ToString(),
                "年",
                now.Month.ToString(),
                "月",
                now.Day,
                "日  ",
                now.Hour.ToString(),
                ":",
                now.Minute.ToString(),
                ":",
                now.Second.ToString()
        }));
        sb.AppendLine("操作系统:  " + SystemInfo.operatingSystem);
        sb.AppendLine("系统内存大小:  " + SystemInfo.systemMemorySize.ToString());
        sb.AppendLine("设备模型:  " + SystemInfo.deviceModel);
        sb.AppendLine("设备唯一标识符:  " + SystemInfo.deviceUniqueIdentifier);
        sb.AppendLine("处理器数量:  " + SystemInfo.processorCount.ToString());
        sb.AppendLine("处理器类型:  " + SystemInfo.processorType);
        sb.AppendLine("显卡标识符:  " + SystemInfo.graphicsDeviceID.ToString());
        sb.AppendLine("显卡名称:  " + SystemInfo.graphicsDeviceName);
        sb.AppendLine("显卡厂商:  " + SystemInfo.graphicsDeviceVendor);
        sb.AppendLine("显卡驱动版本:  " + SystemInfo.graphicsDeviceVersion);
        sb.AppendLine("显存大小:  " + SystemInfo.graphicsMemorySize.ToString());
        sb.AppendLine("显卡着色器级别:  " + SystemInfo.graphicsShaderLevel.ToString());

        //sb.AppendLine($"当前显示器分辨率:  {Screen.currentResolution.width} × {Screen.currentResolution.height}");
        // 显示器数量和每个显示器的分辨率
        sb.AppendLine("显示器数量:  " + Display.displays.Length.ToString());
        for (int i = 0; i < Display.displays.Length; i++)
        {
            // 激活当前显示器（确保能获取到正确的分辨率）
            //Display.displays[i].Activate();(这段代码会导致透明效果失效,白屏)
            sb.AppendLine($"显示器 {i + 1} 分辨率:  {Display.displays[i].systemWidth} × {Display.displays[i].systemHeight}");
        }

        sb.AppendLine("******************************************** end *************************************************************");

        Log(sb.ToString());
    }
}