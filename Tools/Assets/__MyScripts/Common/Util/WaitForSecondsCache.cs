using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// WaitForSeconds缓存工具类
/// 用于减少频繁创建WaitForSeconds对象的性能开销
/// </summary>
public static class WaitForSecondsCache
{
    // 存储已缓存的WaitForSeconds对象
    private static readonly Dictionary<float, WaitForSeconds> _waitCache = new Dictionary<float, WaitForSeconds>();

    // 存储已缓存的WaitForSecondsRealtime对象（用于不受TimeScale影响的情况）
    private static readonly Dictionary<float, WaitForSecondsRealtime> _waitRealtimeCache = new Dictionary<float, WaitForSecondsRealtime>();

    // 默认的缓存配置
    private static readonly float[] _defaultWaitTimes = {
        0.016f,  // 大约60FPS的一帧
        0.033f,  // 大约30FPS的一帧
        0.05f,   // 20FPS的一帧
        0.1f,    // 0.1秒
        0.2f,    // 0.2秒
        0.25f,   // 0.25秒
        0.5f,    // 0.5秒
        1f,      // 1秒
        2f,      // 2秒
        3f,      // 3秒
        5f       // 5秒
    };

    /// <summary>
    /// 获取缓存的WaitForSeconds对象
    /// </summary>
    /// <param name="seconds">等待时间（秒）</param>
    /// <returns>缓存的WaitForSeconds对象</returns>
    public static WaitForSeconds WaitForSeconds(float seconds)
    {
        // 确保时间值为正数
        seconds = Mathf.Max(0, seconds);

        // 尝试从缓存中获取
        if (_waitCache.TryGetValue(seconds, out var waitForSeconds))
        {
            return waitForSeconds;
        }

        // 如果缓存中不存在，创建新的并缓存
        waitForSeconds = new WaitForSeconds(seconds);
        _waitCache[seconds] = waitForSeconds;

        return waitForSeconds;
    }

    /// <summary>
    /// 获取缓存的WaitForSecondsRealtime对象
    /// </summary>
    /// <param name="seconds">等待时间（秒）</param>
    /// <returns>缓存的WaitForSecondsRealtime对象</returns>
    public static WaitForSecondsRealtime GetWaitForSecondsRealtime(float seconds)
    {
        // 确保时间值为正数
        seconds = Mathf.Max(0, seconds);

        // 尝试从缓存中获取
        if (_waitRealtimeCache.TryGetValue(seconds, out var waitForSecondsRealtime))
        {
            return waitForSecondsRealtime;
        }

        // 如果缓存中不存在，创建新的并缓存
        waitForSecondsRealtime = new WaitForSecondsRealtime(seconds);
        _waitRealtimeCache[seconds] = waitForSecondsRealtime;

        return waitForSecondsRealtime;
    }

    /// <summary>
    /// 预缓存常用的WaitForSeconds时间
    /// 可以在游戏启动时调用以提高性能
    /// </summary>
    public static void PreCacheCommonTimes()
    {
        foreach (var time in _defaultWaitTimes)
        {
            WaitForSeconds(time);
            GetWaitForSecondsRealtime(time);
        }
    }

    /// <summary>
    /// 预缓存指定时间数组
    /// </summary>
    /// <param name="times">要预缓存的时间数组</param>
    public static void PreCacheTimes(float[] times)
    {
        if (times == null || times.Length == 0) return;

        foreach (var time in times)
        {
            if (time >= 0)
            {
                WaitForSeconds(time);
                GetWaitForSecondsRealtime(time);
            }
        }
    }

    /// <summary>
    /// 清理所有缓存
    /// 在场景切换或内存紧张时调用
    /// </summary>
    public static void ClearCache()
    {
        _waitCache.Clear();
        _waitRealtimeCache.Clear();
    }

    /// <summary>
    /// 清理指定时间之外的缓存
    /// </summary>
    /// <param name="timesToKeep">要保留的时间数组</param>
    public static void ClearCacheExcept(float[] timesToKeep)
    {
        if (timesToKeep == null || timesToKeep.Length == 0)
        {
            ClearCache();
            return;
        }

        var newWaitCache = new Dictionary<float, WaitForSeconds>();
        var newRealtimeCache = new Dictionary<float, WaitForSecondsRealtime>();

        foreach (var time in timesToKeep)
        {
            if (_waitCache.TryGetValue(time, out var waitForSeconds))
            {
                newWaitCache[time] = waitForSeconds;
            }

            if (_waitRealtimeCache.TryGetValue(time, out var waitForSecondsRealtime))
            {
                newRealtimeCache[time] = waitForSecondsRealtime;
            }
        }

        _waitCache.Clear();
        _waitRealtimeCache.Clear();

        foreach (var kvp in newWaitCache)
        {
            _waitCache[kvp.Key] = kvp.Value;
        }

        foreach (var kvp in newRealtimeCache)
        {
            _waitRealtimeCache[kvp.Key] = kvp.Value;
        }
    }

    /// <summary>
    /// 获取当前缓存数量
    /// </summary>
    /// <returns>返回缓存的WaitForSeconds和WaitForSecondsRealtime总数</returns>
    public static int GetCacheCount()
    {
        return _waitCache.Count + _waitRealtimeCache.Count;
    }

    /// <summary>
    /// 打印缓存信息（用于调试）
    /// </summary>
    public static void LogCacheInfo()
    {
        Debug.Log($"WaitForSeconds缓存数量: {_waitCache.Count}");
        Debug.Log($"WaitForSecondsRealtime缓存数量: {_waitRealtimeCache.Count}");
        Debug.Log($"总缓存数量: {GetCacheCount()}");

        if (_waitCache.Count > 0)
        {
            Debug.Log("缓存的WaitForSeconds时间:");
            foreach (var kvp in _waitCache)
            {
                Debug.Log($"  {kvp.Key:F3}秒");
            }
        }
    }
}