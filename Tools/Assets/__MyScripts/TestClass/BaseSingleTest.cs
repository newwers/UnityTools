using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseSingleTestManager : BaseSingleClass<BaseSingleTestManager>
{
    int a = 0;
    public void Test()
    {
        a += 1;
        Debug.Log("单例测试" + a);
    }

    public BaseSingleTestManager()
    {
        Debug.Log("单例构造函数测试" + a);
    }
}

public class BaseSingleTest : MonoBehaviour
{
    private void Start()
    {
        BaseSingleTestManager.Instance.Test();
        BaseSingleTestManager.Instance.Test();
    }
}
