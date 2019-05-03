using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

/// <summary>
/// 检查代码执行时间
/// </summary>
public static class CheckCodeExecuteTime  {

	private static Stopwatch stopwatch = new Stopwatch();

    public static void StartCheck()
    {
        stopwatch.Start(); //  开始监视代码运行时间
    }

    public static void EndCheck()
    {
        stopwatch.Stop(); //  停止监视
        TimeSpan timespan = stopwatch.Elapsed; //  获取当前实例测量得出的总时间
        double milliseconds = timespan.TotalMilliseconds;  //  总毫秒数
        UnityEngine.Debug.Log("<color=#ff0000>代码执行时间:" + milliseconds + "毫秒</color>");
    }
}
