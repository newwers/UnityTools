using System;
using System.Text;
using UnityEngine;


/// <summary>
/// 打印等级
/// </summary>
public enum LogLevel
{
    /// <summary>
    /// 不打印所有打印
    /// </summary>
    None,
    /// <summary>
    /// 打印测试log
    /// </summary>
    Test,
    /// <summary>
    /// 打印普通log
    /// </summary>
    Normal,
    /// <summary>
    /// 打印错误log
    /// </summary>
    Error,
    /// <summary>
    /// 打印错误log和普通log
    /// </summary>
    ErrorAndNormal,
    /// <summary>
    /// 所有打印
    /// </summary>
    All
}

/// <summary>
/// Log除了可以管理是否打印外,还可以将打印写到硬盘中
/// 在开发阶段使用Test等级的打印,当开发结束不需要log打印时,可以切换到none或者其他等级来过滤不需要的打印
/// </summary>
public class LogManager : MonoBehaviour
{



    public static LogManager Instance;
    /// <summary>
    /// 当前打印的log等级
    /// </summary>
    public LogLevel m_CurrentLogLevel = LogLevel.Test;
    public static LogLevel m_CurrentLogLevelStatic = LogLevel.Test;

    /// <summary>
    /// 是否将log写到文件
    /// </summary>
    public static bool m_IsWriteLogStatic = true;

    public bool m_IsWriteLog = true;



    /// <summary>
    /// 当打印log的时候,进行存储,当达到一定次数的时候,写入到硬盘
    /// </summary>
    private static StringBuilder m_StringBuilder = new StringBuilder();
    /// <summary>
    /// 写入log频率
    /// </summary>
    public int WriteLogFrequency = 5;
    public static int WriteLogFrequencyStatic = 5;
    /// <summary>
    /// 记录写入次数,当满足写入频率时,清空
    /// </summary>
    private static int m_WriteLogCount = 0;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Application.logMessageReceived -= DebugHandles;
            Application.logMessageReceived += DebugHandles;
        }

    }
    /// <summary>
    /// 当Inspector面板属性发生改变,
    /// </summary>
    private void OnValidate()
    {
        if (WriteLogFrequency != WriteLogFrequencyStatic)
        {
            WriteLogFrequencyStatic = WriteLogFrequency;
            Debug.Log("设置打印次数,WriteLogFrequencyStatic=" + WriteLogFrequencyStatic);
        }
        if (m_CurrentLogLevel != m_CurrentLogLevelStatic)
        {
            m_CurrentLogLevelStatic = m_CurrentLogLevel;
            Debug.Log("设置打印次数,m_CurrentLogLevelStatic=" + m_CurrentLogLevelStatic);
        }
        if (m_IsWriteLog != m_IsWriteLogStatic)
        {
            m_IsWriteLogStatic = m_IsWriteLog;
        }
    }

    private void OnDestroy()
    {
        if (Instance != null)
        {
            Instance = null;
        }
        Application.logMessageReceived -= DebugHandles;
    }

    private void DebugHandles(string logString, string stackTrace, LogType type)
    {
        WriteNormalLog(logString, LogLevel.Normal);
        if (type == LogType.Error || type == LogType.Exception)
        {
            WriteErrorLog(string.Format("异常log: {0} ,堆栈记录: {1} ", logString, stackTrace), LogLevel.Error);
        }
    }


    public static void Log(string log, LogLevel logLevel = LogLevel.Normal)
    {
        if (m_CurrentLogLevelStatic > logLevel)
        {
            return;
        }
        switch (logLevel)
        {
            case LogLevel.None:
                break;
            case LogLevel.Normal:
            case LogLevel.Test:
            case LogLevel.Error:
            case LogLevel.ErrorAndNormal:
            case LogLevel.All:
                NormalLog(log, logLevel);
                break;
            default:
                break;
        }

    }

    public static void LogError(string log, LogLevel logLevel = LogLevel.Error)
    {
        if (m_CurrentLogLevelStatic > logLevel)//当前设置打印等级大于log打印等级,就不进行打印
        {
            return;
        }
        switch (logLevel)
        {
            case LogLevel.None:
                break;
            case LogLevel.Test:
            case LogLevel.Normal:
            case LogLevel.Error:
            case LogLevel.ErrorAndNormal:
            case LogLevel.All:
                ErrorLog(log, logLevel);
                break;
            default:
                break;
        }
    }
    /// <summary>
    /// 打印log
    /// </summary>
    /// <param name="log"></param>
    /// <param name="logLevel"></param>
    private static void NormalLog(string log, LogLevel logLevel)
    {
        Debug.Log(log);
    }

    /// <summary>
    /// 普通log写入到本地
    /// </summary>
    /// <param name="log"></param>
    /// <param name="logLevel"></param>
    private static void WriteNormalLog(string log, LogLevel logLevel)
    {
        m_WriteLogCount++;

        m_StringBuilder.Append(DateTime.Now.ToString());
        m_StringBuilder.Append(" -> ");
        m_StringBuilder.Append(logLevel.ToString());
        m_StringBuilder.Append(" -> ");
        m_StringBuilder.AppendLine(log);
        if (m_WriteLogCount >= WriteLogFrequencyStatic)
        {
            WriteLog();
            m_WriteLogCount = 0;
        }
    }
    /// <summary>
    /// 打印异常log
    /// </summary>
    /// <param name="log"></param>
    /// <param name="logLevel"></param>
    private static void ErrorLog(string log, LogLevel logLevel)
    {
        Debug.LogError(log);
    }
    /// <summary>
    /// 异常log写入到本地
    /// </summary>
    /// <param name="log"></param>
    /// <param name="logLevel"></param>
    private static void WriteErrorLog(string log, LogLevel logLevel)
    {
        m_WriteLogCount++;

        m_StringBuilder.Append(DateTime.Now.ToString());
        m_StringBuilder.Append(" -> ");
        m_StringBuilder.Append(logLevel.ToString());
        m_StringBuilder.Append(" -> ");
        m_StringBuilder.AppendLine(log);
        if (m_WriteLogCount >= WriteLogFrequencyStatic)
        {
            WriteLog();
            m_WriteLogCount = 0;
        }
    }

    /// <summary>
    /// 写入log到硬盘
    /// </summary>
    private static void WriteLog()
    {
        string log = m_StringBuilder.ToString();
        m_StringBuilder.Clear();//这边先清空,是为了防止在写入文件时,如果再次进行打印,会出现打印两遍的情况
        if (m_IsWriteLogStatic == false)
        {
            return;
        }
        Z.FileTool.FileTools.WriteFile(Application.streamingAssetsPath + "/Log.txt", log, Encoding.UTF8, true);

    }


    public static void LogColor(string log, LogColorEnum color)
    {
        switch (color)
        {
            case LogColorEnum.Red:
                Debug.Log("<color=#ff0000>" + log + "</color>");
                break;
            case LogColorEnum.Green:
                Debug.Log("<color=#00ff00>" + log + "</color>");
                break;
            case LogColorEnum.Blue:
                Debug.Log("<color=#0000ff>" + log + "</color>");
                break;
            case LogColorEnum.Yellow:
                Debug.Log("<color=#ffff00>" + log + "</color>");
                break;
            case LogColorEnum.White:
                Debug.Log("<color=#ffffff>" + log + "</color>");
                break;
            case LogColorEnum.Gray:
                Debug.Log("<color=#555555>" + log + "</color>");
                break;
            default:
                Debug.Log(log);
                break;
        }

    }


}

/// <summary>
/// Log颜色枚举
/// </summary>
public enum LogColorEnum
{
    Red,
    Green,
    Blue,
    Yellow,
    White,
    Gray
}
