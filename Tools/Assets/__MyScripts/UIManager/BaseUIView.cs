using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 一个界面的基类
/// 界面第一次被创建出来的时候
/// 界面打开时候
/// 界面关闭隐藏时
/// 界面被销毁的时候
/// 判断界面是否打开状态
/// 每个界面都有一个唯一界面ID
/// </summary>
public class BaseUIView : MonoBehaviour
{
    /// <summary>
    /// 每个界面都有一个唯一界面ID
    /// </summary>
    public int UIInstanceID = 0;
    /// <summary>
    /// 界面显示状态
    /// </summary>
    public bool isShowState = false;

    /// <summary>
    /// 当创建完界面时,调用一次
    /// </summary>
    /// <param name="args">打开界面时,传递的参数</param>
    public virtual void OnCreated(object args)
    {

    }
    /// <summary>
    /// 当界面显示时调用,如果第一次创建,那么在OnCreated后面调用
    /// </summary>
    /// <param name="args">打开界面时,传递的参数</param>
    public virtual void OnShow(object args)
    {
        this.gameObject.SetActive(true);//在每次打开界面的时候保证界面是显示激活状态
        isShowState = true;
    }
    /// <summary>
    /// 当界面被关闭隐藏时调用
    /// </summary>
    public virtual void OnHide()
    {
        isShowState = false;
    }

    /// <summary>
    /// 当界面被销毁时调用
    /// </summary>
    public virtual void OnHideAndDestroy()
    {
        isShowState = false;
    }
}
