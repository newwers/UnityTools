using System;
using System.Threading;
using UnityEngine;
using System.Diagnostics;
using System.Runtime.InteropServices;

/// <summary>
/// 单实例管理器
/// 用于确保游戏只能运行一个实例
/// </summary>
/// <remarks>
/// 该类使用互斥锁（Mutex）来实现跨进程的互斥
/// 当尝试启动第二个游戏实例时，新实例会自动关闭，同时激活已有实例
/// </remarks>
public class SingleInstanceManager
{
    /// <summary>
    /// 互斥锁实例
    /// </summary>
    private static Mutex _mutex;
    
    /// <summary>
    /// 互斥锁名称，应确保唯一
    /// </summary>
    /// <remarks>
    /// 建议使用游戏名称加上唯一标识符
    /// 避免与其他应用程序冲突
    /// </remarks>
    private const string MUTEX_NAME = "MushRoomGameSingleInstanceMutex_8F7E6D5C4B3A2Z1";

    /// <summary>
    /// 设置窗口为前台窗口
    /// </summary>
    /// <param name="hWnd">窗口句柄</param>
    /// <returns>操作是否成功</returns>
    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    /// <summary>
    /// 显示窗口
    /// </summary>
    /// <param name="hWnd">窗口句柄</param>
    /// <param name="nCmdShow">显示方式</param>
    /// <returns>操作是否成功</returns>
    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    /// <summary>
    /// 恢复窗口状态
    /// </summary>
    private const int SW_RESTORE = 9;

    /// <summary>
    /// 初始化单实例管理器
    /// </summary>
    /// <remarks>
    /// 应在游戏启动的最早阶段调用此方法
    /// 建议在 GameManager.Awake() 方法的最开始调用
    /// </remarks>
    public static void Init()
    {
        bool createdNew;
        _mutex = new Mutex(true, MUTEX_NAME, out createdNew);

        if (!createdNew)
        {
            // 已有实例在运行，激活已有实例
            ActivateExistingInstance();
            // 关闭当前实例
            Application.Quit();
        }
    }

    /// <summary>
    /// 激活已有实例
    /// </summary>
    /// <remarks>
    /// 查找并激活已运行的游戏实例
    /// 将其窗口显示在前台
    /// </remarks>
    private static void ActivateExistingInstance()
    {
        Process currentProcess = Process.GetCurrentProcess();
        foreach (Process process in Process.GetProcessesByName(currentProcess.ProcessName))
        {
            if (process.Id != currentProcess.Id)
            {
                // 激活已有实例的窗口
                IntPtr hWnd = process.MainWindowHandle;
                if (hWnd != IntPtr.Zero)
                {
                    ShowWindow(hWnd, SW_RESTORE);
                    SetForegroundWindow(hWnd);
                }
                break;
            }
        }
    }

    /// <summary>
    /// 释放互斥锁
    /// </summary>
    /// <remarks>
    /// 应在游戏退出时调用此方法
    /// 建议在 GameManager.OnApplicationQuit() 方法中调用
    /// </remarks>
    public static void Release()
    {
        if (_mutex != null)
        {
            _mutex.ReleaseMutex();
            _mutex.Dispose();
            _mutex = null;
        }
    }
}

/* 使用说明：
 * 1. 在 GameManager.cs 的 Awake() 方法中添加以下代码：
 *    SingleInstanceManager.Init();
 *    确保此调用在所有其他初始化代码之前执行
 *
 * 2. 在 GameManager.cs 的 OnApplicationQuit() 方法中添加以下代码：
 *    SingleInstanceManager.Release();
 *    确保在游戏退出时释放互斥锁
 *
 * 3. 功能说明：
 *    - 当尝试启动第二个游戏实例时，新实例会自动检测到已有实例
 *    - 新实例会激活已有实例的窗口并将其显示在前台
 *    - 新实例会自动退出，确保游戏始终只有一个实例在运行
 *    - 当游戏正常退出时，互斥锁会被释放
 *    - 当游戏崩溃时，操作系统会自动释放互斥锁
 */