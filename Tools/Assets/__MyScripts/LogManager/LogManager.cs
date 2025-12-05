using UnityEngine;

public static class LogManager
{
    // 日志开关设置
    public static bool EnableLogs { get; set; } = true;      // 控制普通日志
    public static bool EnableWarnings { get; set; } = true;   // 控制警告日志
    public static bool EnableErrors { get; set; } = true;     // 控制错误日志

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
        if (EnableLogs) Debug.Log(message);
    }

    public static void Log(object message, Object context)
    {
        if (EnableLogs) Debug.Log(message, context);
    }

    // 警告日志输出
    public static void LogWarning(object message)
    {
        if (EnableWarnings) Debug.LogWarning(message);
    }

    public static void LogWarning(object message, Object context)
    {
        if (EnableWarnings) Debug.LogWarning(message, context);
    }

    // 错误日志输出
    public static void LogError(object message)
    {
        if (EnableErrors) Debug.LogError(message);
    }

    public static void LogError(object message, Object context)
    {
        if (EnableErrors) Debug.LogError(message, context);
    }

    // 带标签的日志方法（可选）
    public static void TaggedLog(string tag, object message)
    {
        if (EnableLogs) Debug.Log($"[{tag}] {message}");
    }

    //打印红色log
    public static void Log_Red(object message)
    {
        if (EnableLogs) Debug.Log($"<color=red>{message}</color>");
    }
    //打印绿色log
    public static void Log_Green(object message)
    {
        if (EnableLogs) Debug.Log($"<color=green>{message}</color>");
    }

    public static void PhoneSystemInfo(StreamWriter sw)
        {
            Process process = new Process();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.Arguments = "/c ver";
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.Start();
            string str = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            sw.WriteLine("*********************************************************************************************************start");
            sw.WriteLine("By " + SystemInfo.deviceName);
            DateTime now = DateTime.Now;
            sw.WriteLine(string.Concat(new object[]
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
            sw.WriteLine("操作系统详细版本:  " + str);
            sw.WriteLine();
            sw.WriteLine("操作系统:  " + SystemInfo.operatingSystem);
            sw.WriteLine("系统内存大小:  " + SystemInfo.systemMemorySize.ToString());
            sw.WriteLine("设备模型:  " + SystemInfo.deviceModel);
            sw.WriteLine("设备唯一标识符:  " + SystemInfo.deviceUniqueIdentifier);
            sw.WriteLine("处理器数量:  " + SystemInfo.processorCount.ToString());
            sw.WriteLine("处理器类型:  " + SystemInfo.processorType);
            sw.WriteLine("显卡标识符:  " + SystemInfo.graphicsDeviceID.ToString());
            sw.WriteLine("显卡名称:  " + SystemInfo.graphicsDeviceName);
            sw.WriteLine("显卡厂商:  " + SystemInfo.graphicsDeviceVendor);
            sw.WriteLine("显卡驱动版本:  " + SystemInfo.graphicsDeviceVersion);
            sw.WriteLine("显存大小:  " + SystemInfo.graphicsMemorySize.ToString());
            sw.WriteLine("显卡着色器级别:  " + SystemInfo.graphicsShaderLevel.ToString());
            sw.WriteLine("*********************************************************************************************************end");
            sw.WriteLine("LogInfo:");
            sw.WriteLine();
        }
}