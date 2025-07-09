/*
对象池类
物体隐藏默认回收
同一类型不同数据,在拿到缓存对象时需要重置,
同一个父类的,每个子类一个对象池,否则无法区分
 */

using System.Collections.Generic;
using UnityEngine;

public class SimpleGameObjectPool<T> where T : UnityEngine.Object
{
    private List<T> pool;
    private T prefab;
    Transform m_Root;

    public SimpleGameObjectPool(T prefab, Transform root, int initialSize = 0)
    {
        this.prefab = prefab;
        m_Root = root;
        pool = new List<T>();

        for (int i = 0; i < initialSize; i++)
        {
            T obj = Object.Instantiate(prefab, m_Root);
            if (obj is Component com)
            {
                com.gameObject.SetActive(false);
            }
            if (obj is GameObject go)
            {
                go.SetActive(false);
            }
            pool.Add(obj);
        }
    }

    public T GetObject()
    {
        foreach (T obj in pool)
        {
            if (obj is Component gameObj && !gameObj.gameObject.activeSelf)
            {
                gameObj.gameObject.SetActive(true);
                return obj;
            }
            else if (obj is GameObject go && !go.activeSelf)
            {
                go.SetActive(true);
                return obj;
            }
        }

        // 如果池中没有可用对象，创建一个新的
        T newObj = Object.Instantiate(prefab, m_Root);
        if (newObj is Component com)
        {
            com.gameObject.SetActive(true);
        }
        else if (newObj is GameObject go && !go.activeSelf)
        {
            go.SetActive(true);
        }
        pool.Add(newObj);
        return newObj;
    }

    public void ReturnObject(T obj)
    {
        if (obj is Component com)
        {
            com.gameObject.SetActive(false);
        }
        else if (obj is GameObject go)
        {
            go.SetActive(false);
        }

    }
}