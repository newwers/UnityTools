using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 消息中心
/// 由于消息中心全局共享一个,那么可以定义成静态吗?
/// 消息中心核心方法就是消息的订阅(或者说注册)和消息的公布(或者说通知)
/// 还有一个消息注册的移除功能
/// 独立模块,不需要继承 MonoBehaviour
/// 观察者模式的优点:解耦,解耦的意思就是例如一个按钮的点击过程,负责UI部分的只处理按钮的点击,负责逻辑部分的在收到
/// 按钮点击的消息后,进行对应逻辑的处理,按钮只需要负责将消息发送出去,不需要管逻辑的处理,
/// 使用时注意事项:
/// 1.在使用观察者模式时,通常需要定义一个作为区分的标识,例如枚举类型,而枚举类型实际上也是对应一个数值
/// 如果定义的数值重复了,那么就会有问题
/// 2.在注册消息时,需要注意不要重复注册了,每注册一次,在通知时都会调用一次,这边其实应该消息中心在注册时,会进行判断,重复类型的注册就过滤掉
/// 3.在一个事件生命周期结束时,记得将注册的消息进行释放,不释放的话,会占用内存(其实也占用不大?因为只是存放了消息对应的Action函数?)
/// 4.消息的注册都是需要比消息通知先执行,这样在通知的时候才有对应的类型
/// </summary>
public static class Notification  {

    /// <summary>
    /// 缓存所有的注册消息
    /// </summary>
    private static Dictionary<string, List<Action<object>>> mNotificationList = new Dictionary<string, List<Action<object>>>();

    #region 注释
    // 消息的注册
    // 首先消息的注册,在注册的时候就需要指定调用时要执行的函数
    // 同时,为了让通知时,知道是哪个消息,所以还需要一个类型
    // 类型经过考虑,我们用字符串作为类型
    // 传递函数当作参数
    // Action:该方法不具有参数并且不返回值。
    // Action<T>:该方法只有一个参数并且不返回值.
    // Action<T1,T2>:该方法具有两个参数且不返回值。
    // 以此类推,可以有多个参数,但是都没有返回值
    // Func<TResult>:封装一个不具有参数但却返回 TResult 参数指定的类型值的方法
    // Func<T,TResult>:封装一个具有一个参数并返回 TResult 参数指定的类型值的方法
    // Func<T1,T2,TResult>:封装一个具有两个参数并返回 TResult 参数指定的类型值的方法
    // Func的特点就是一定有返回值,并且参数的最后一个是返回类型,前面都是传递的参数
    #endregion
    
    /// <summary>
    /// 消息注册
    /// </summary>
    /// <param name="type">消息的类型,通常用函数名设置</param>
    /// <param name="func">当消息接收到后,执行的函数,函数带有一个object类型的参数作为可传递的参数,函数必须是没有返回值</param>
    public static void Subscribe(string type, Action<object> func)
    {

        //注册时,应该有一个dictionary进行缓存
        //添加时,应该先做个是否存在的判断,防止重复添加
        if (!mNotificationList.ContainsKey(type))
        {
            List<Action<object>> actions = new List<Action<object>>();
            actions.Add(func);
            mNotificationList.Add(type, actions);
        }
        else//多个消息订阅同一条数据
        {
            mNotificationList[type].Add(func);
        }
        


        //Debug.Log("本身方法:" + new System.Diagnostics.StackTrace().GetFrame(0));//本身方法:Subscribe at offset 6 in file:line:column <filename unknown>:0:0
        //Debug.Log("调用方法:" + new System.Diagnostics.StackTrace().GetFrame(1).GetMethod().Name);//调用方法:Awake
        //Debug.Log("调用类名:" + new System.Diagnostics.StackTrace().GetFrame(1).GetMethod().ReflectedType.Name); //调用类名:NotificationTest
        //Debug.Log("传递的类型:" + type);
    }

    /// <summary>
    /// 消息的通知,消息通知
    /// 默认传递参数可以不用设置
    /// </summary>
    public static void Publish(string type,object arg = null)
    {
        if (mNotificationList.ContainsKey(type))
        {
            foreach (var item in mNotificationList[type])
            {
                item.Invoke(arg);
            }
        }
        else
        {
            //Debug.LogError("缺少对应类型:" + type);
        }
    }
    /// <summary>
    /// 消息的移除
    /// </summary>
    public static void Unsubscribe(string type)
    {
        if (mNotificationList.ContainsKey(type))
        {
            mNotificationList.Remove(type);
        }
    }


}
