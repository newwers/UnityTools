
// 对象池类
using System.Collections.Generic;
using UnityEngine;

public class SimpleGameObjectPool
{
    private List<GameObject> pool;
    private GameObject prefab;
    private int initialSize;

    public SimpleGameObjectPool(GameObject prefab, int initialSize)
    {
        this.prefab = prefab;
        this.initialSize = initialSize;
        pool = new List<GameObject>();

        for (int i = 0; i < initialSize; i++)
        {
            GameObject obj = GameObject.Instantiate(prefab);
            pool.Add(obj);
        }
    }

    public GameObject GetObject()
    {
        foreach (GameObject obj in pool)
        {
            if (!obj.activeSelf)
            {
                obj.SetActive(true);
                return obj;
            }
        }

        // 如果池中没有可用对象，创建一个新的
        GameObject newObj = GameObject.Instantiate(prefab);
        pool.Add(newObj);
        return newObj;
    }

    public void ReturnObject(GameObject obj)
    {
        obj.SetActive(false);
    }
}
