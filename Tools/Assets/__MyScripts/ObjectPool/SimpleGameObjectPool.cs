/*
对象池类
物体隐藏默认回收
同一类型不同数据,在拿到缓存对象时需要重置,
同一个父类的,每个子类一个对象池,否则无法区分

使用方式:
- 池中对象需要在取出/回收时重置自身状态时,实现 IPoolable 接口
- GetObject 激活对象后回调 IPoolable.OnGetFromPool()
- ReturnObject 隐藏对象前回调 IPoolable.OnReturnToPool()
 */

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 对象池中对象实现该接口,用于在每次取出与回收时处理自身状态。
/// </summary>
public interface IPoolable
{
    /// <summary>从池中取出、激活后调用。用于重置状态(计时器、速度、数据等)。</summary>
    void OnGetFromPool();

    /// <summary>回收到池、隐藏前调用。用于停止运动、清理临时状态等。</summary>
    void OnReturnToPool();
}

public class SimpleGameObjectPool<T> where T : UnityEngine.Object
{
    private readonly T prefab;
    private readonly Transform m_Root;
    private readonly Stack<T> pool = new Stack<T>();   // 只存"可用"对象，取出/回收均为 O(1)

    public SimpleGameObjectPool(T prefab, Transform root, int initialSize = 0)
    {
        this.prefab = prefab;
        m_Root = root;

        for (int i = 0; i < initialSize; i++)
        {
            T obj = CreateObject();
            SetObjectActive(obj, false);
            pool.Push(obj);
        }
    }

    /// <summary>取出一个可用对象：无可用则新建，并回调 IPoolable.OnGetFromPool。</summary>
    public T GetObject()
    {
        T obj = pool.Count > 0 ? pool.Pop() : CreateObject();
        SetObjectActive(obj, true);

        if (obj is IPoolable poolable)
            poolable.OnGetFromPool();

        return obj;
    }

    /// <summary>回收对象：回调 IPoolable.OnReturnToPool 后隐藏入池。</summary>
    public void ReturnObject(T obj)
    {
        if (obj == null) return;

        // 已处于隐藏状态说明此前已回收过，避免同一对象重复入池
        if (!IsObjectActive(obj)) return;

        if (obj is IPoolable poolable)
            poolable.OnReturnToPool();

        SetObjectActive(obj, false);
        pool.Push(obj);
    }

    private T CreateObject()
    {
        return Object.Instantiate(prefab, m_Root);
    }

    private static bool IsObjectActive(T obj)
    {
        if (obj is Component com) return com.gameObject.activeSelf;
        if (obj is GameObject go) return go.activeSelf;
        return true;
    }

    private static void SetObjectActive(T obj, bool active)
    {
        if (obj is Component com)
            com.gameObject.SetActive(active);
        else if (obj is GameObject go)
            go.SetActive(active);
    }
}
