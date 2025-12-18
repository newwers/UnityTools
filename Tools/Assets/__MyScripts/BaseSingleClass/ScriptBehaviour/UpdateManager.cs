using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Update观察者接口（需要执行Update逻辑的对象需实现此接口）
/// </summary>
public interface IUpdateObserver
{
    /// <summary>
    /// 被观察的Update方法（替代MonoBehaviour的Update）
    /// </summary>
    void ObservedUpdate();
}

/// <summary>
/// Update统一管理器（观察者模式核心，挂载到Unity场景中的任意物体上即可）
/// 目的: 减少MonoBehaviour的数量，优化Update调用性能
/// </summary>
public class UpdateManager : MonoBehaviour
{
    // 已注册的观察者列表（正式执行Update的对象）
    private static List<IUpdateObserver> _observers = new List<IUpdateObserver>();
    // 待处理的观察者列表（避免遍历期间修改正式列表）
    private static List<IUpdateObserver> _pendingObservers = new List<IUpdateObserver>();
    private static int _currentIndex;

    /// <summary>
    /// Unity原生Update（每帧触发所有观察者的Update逻辑）
    /// </summary>
    private void Update()
    {
        for (_currentIndex = _observers.Count - 1; _currentIndex >= 0; _currentIndex--)
        {
            _observers[_currentIndex].ObservedUpdate();
        }

        _observers.AddRange(_pendingObservers);// 批量添加待处理的观察者（遍历结束后统一处理）
        _pendingObservers.Clear();
    }

    /// <summary>
    /// 注册观察者（外部调用，添加需要执行Update的对象）
    /// </summary>
    /// <param name="observer">实现了IUpdateObserver的对象</param>
    public static void RegisterObserver(IUpdateObserver observer)
    {
        _pendingObservers.Add(observer);
    }

    /// <summary>
    /// 取消注册观察者（外部调用，移除不需要执行Update的对象）
    /// </summary>
    /// <param name="observer">要移除的观察者对象</param>
    public static void UnregisterObserver(IUpdateObserver observer)
    {
        _observers.Remove(observer);
        _currentIndex--;
    }
}