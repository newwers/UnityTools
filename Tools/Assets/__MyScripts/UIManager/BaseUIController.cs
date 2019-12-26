using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseUIController : MonoBehaviour
{
    public BaseUIView View;

    /// <summary>
    /// 当创建完界面时,调用一次
    /// </summary>
    /// <param name="args">打开界面时,传递的参数</param>
    public virtual void OnCreated(object args)
    {
        if (View == null)
        {
            View = GetComponent<BaseUIView>();
        }

        RegisterNotification();
    }

    /// <summary>
    /// 在OnCreated后执行,用来进行消息事件的监听
    /// </summary>
    public virtual void RegisterNotification()
    {

    }

    /// <summary>
    /// 当界面显示时调用,如果第一次创建,那么在OnCreated后面调用
    /// </summary>
    /// <param name="args">打开界面时,传递的参数</param>
    public virtual void OnShow(object args)
    {

    }

    /// <summary>
    /// 当界面被关闭隐藏时调用
    /// </summary>
    public virtual void OnHide()
    {

    }

    /// <summary>
    /// 当界面被销毁时调用
    /// </summary>
    public virtual void OnHideAndDestroy()
    {

    }


}
