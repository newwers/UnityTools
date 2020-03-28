using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 主要用来解决非继承自MonoBehaviour脚本,无法有Update这种调用函数的问题
/// 通过将非继承自MonoBehaviour的代码传递到这里进行mono生命周期的调用,来解决非mono脚本代码调用问题
/// </summary>
public class MonoManager : BaseSingleClass<MonoManager>
{
    MonoController monoController;

    public MonoManager()
    {
        GameObject monoManager = new GameObject("MonoManager");
        monoController = monoManager.AddComponent<MonoController>();
        GameObject.DontDestroyOnLoad(monoManager);
    }

    /// <summary>
    /// 往Update事件里面添加函数
    /// </summary>
    /// <param name="action"></param>
    public void AddUpdateEvent(UnityAction action)
    {
        monoController.AddUpdateEvent(action);
    }
    /// <summary>
    /// 移除Update事件里面函数
    /// </summary>
    /// <param name="action"></param>
    public void RemoteUpdateEvent(UnityAction action)
    {
        monoController.RemoteUpdateEvent(action);
    }
    /// <summary>
    /// 开启一个携程
    /// </summary>
    /// <param name="routine"></param>
    public void StartCoroutine(IEnumerator routine)
    {
        monoController.StartCoroutine(routine);
    }
    /// <summary>
    /// 停止一个携程
    /// </summary>
    /// <param name="routine"></param>
    public void StopCoroutine(IEnumerator routine)
    {
        monoController.StopCoroutine(routine);
    }
    /// <summary>
    /// 停止所有携程
    /// </summary>
    public void StopAllCoroutines()
    {
        monoController.StopAllCoroutines();
    }
}

/// <summary>
/// 继承自MonoBehaviour用来执行Mono生命周期函数使用
/// </summary>
public class MonoController : MonoBehaviour
{
    /// <summary>
    /// 存放传递过来在Update里面执行的函数
    /// </summary>
    private event UnityAction UnityUpdateEvent;

    /// <summary>
    /// 添加函数到Update事件中
    /// </summary>
    /// <param name="action"></param>
    public void AddUpdateEvent(UnityAction action)
    {
        UnityUpdateEvent += action;
    }
    /// <summary>
    /// 从Update事件中移除函数
    /// </summary>
    /// <param name="action"></param>
    public void RemoteUpdateEvent(UnityAction action)
    {
        UnityUpdateEvent -= action;
    }


    private void Update()
    {
        if (UnityUpdateEvent != null)
        {
            UnityUpdateEvent.Invoke();
        }
    }
}
