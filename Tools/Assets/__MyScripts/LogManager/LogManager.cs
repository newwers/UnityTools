using System;
using System.Collections.Generic;
using System.Text;
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

/// <summary>
/// Log除了可以管理是否打印外,还可以将打印写到硬盘中
/// </summary>
public class LogManager :MonoBehaviour {


    public static LogLevel LogType = LogLevel.All;

    public static LogManager Instance;

    public LogLevel m_LogLevel = LogLevel.All;

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
        }
        m_LogLevel = LogType;
    }
    /// <summary>
    /// 当Inspector面板属性发生改变,
    /// </summary>
    private void OnValidate()
    {
        if (m_LogLevel != LogType)
        {
            LogType = m_LogLevel;
            Debug.Log("当Inspector面板属性发生改变,LogType=" + LogType);
        }
        if (WriteLogFrequency != WriteLogFrequencyStatic)
        {
            WriteLogFrequencyStatic = WriteLogFrequency;
            Debug.Log("设置打印次数,WriteLogFrequencyStatic=" + WriteLogFrequencyStatic);
        }
    }

    private void OnDestroy()
    {
        if (Instance != null)
        {
            Instance = null;
        }
    }


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
        Debug.Log(log);
        m_WriteLogCount++;
        
        m_StringBuilder.Append(DateTime.Now.ToString());
        m_StringBuilder.Append(" Log == ");
        m_StringBuilder.AppendLine(log);
        if (m_WriteLogCount >= WriteLogFrequencyStatic)
        {
            WriteLog();
            m_WriteLogCount = 0;
        }
    }

    private static void ErrorLog(string log)
    {
        Debug.LogError(log);
        m_WriteLogCount++;
        
        m_StringBuilder.Append(DateTime.Now.ToString());
        m_StringBuilder.Append(" ErrorLog == ");
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
        Tools.FileTool.FileTools.WriteFile(Application.streamingAssetsPath + "/Log.txt", log, Encoding.UTF8, true);
        m_StringBuilder.Clear();
    }


}
