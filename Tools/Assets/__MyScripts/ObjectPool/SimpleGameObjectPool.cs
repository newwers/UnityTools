
// 对象池类
using System.Collections.Generic;
using UnityEngine;

public class SimpleGameObjectPool<T> where T : UnityEngine.Object
{
    private List<T> pool;
    private T prefab;
    Transform m_Root;

    public SimpleGameObjectPool(T prefab, int initialSize, Transform root)
    {
        this.prefab = prefab;
        m_Root = root;
        pool = new List<T>();

        for (int i = 0; i < initialSize; i++)
        {
            T obj = Object.Instantiate(prefab, m_Root);
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
        }

        // 如果池中没有可用对象，创建一个新的
        T newObj = Object.Instantiate(prefab, m_Root);
        if (newObj is Component com)
        {
            com.gameObject.SetActive(true);
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
    }
}