using UnityEditor;
using UnityEngine;


/// <summary>
/// 打印等级
/// </summary>
public enum LogLevel
{
    /// <summary>
    /// 不打印所有打印
    /// </summary>
    None = 0,
    /// <summary>
    /// 打印普通log
    /// </summary>
    Normal = 1,
    /// <summary>
    /// 打印错误log
    /// </summary>
    Error = 2,
    /// <summary>
    /// 打印错误log和普通log
    /// </summary>
    ErrorAndNormal = 3,
    /// <summary>
    /// 所有打印
    /// </summary>
    All = 4
}

public class LogManager  {


    public static LogLevel LogType = LogLevel.All;


    public static void Log(string log)
    {
        switch (LogType)
        {
            case LogLevel.None:
                break;
            case LogLevel.Normal:
                NormalLog(log);
                break;
            case LogLevel.Error:
                break;
            case LogLevel.ErrorAndNormal:
                NormalLog(log);
                break;
            case LogLevel.All:
                NormalLog(log);
                break;
            default:
                break;
        }
        
    }

    public static void LogError(string log)
    {
        switch (LogType)
        {
            case LogLevel.None:
                break;
            case LogLevel.Normal:
                break;
            case LogLevel.Error:
                ErrorLog(log);
                break;
            case LogLevel.ErrorAndNormal:
                ErrorLog(log);
                break;
            case LogLevel.All:
                ErrorLog(log);
                break;
            default:
                break;
        }
    }

    private static void NormalLog(string log)
    {
        Debug.Log("zdq:" + log);
    }

    private static void ErrorLog(string log)
    {
        Debug.LogError("zdq:" + log);
    }


}
