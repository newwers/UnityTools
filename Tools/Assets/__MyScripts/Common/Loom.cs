using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Linq;


//我们只需要关系两个函数：RunAsync(Action)和QueueOnMainThread(Action, [optional] float time) 就可以轻松实现一个函数的两段代码在C#线程和Unity的主线程中交叉运行。
//原理也很简单：用线程池去运行RunAsync(Action)的函数，在Update中运行QueueOnMainThread(Acition, [optional] float time)传入的函数。
public class Loom : MonoBehaviour
{
    public static int maxThreads = 8;//定义最大线程数
    static int numThreads;//同时运行几个线程的数量
    private static Loom _current;//静态的实例本身
    //private int _count;//什么的数量?      没用到该变量                            *************************************
    static bool initialized;//标记是否实例化
    /// <summary>
    /// 当没有将次脚本附到游戏对象上时
    /// </summary>
    public static Loom Current//返回当前类的实例化对象
    {
        get
        {
            Initialize();//初始化
            return _current;//初始化后_current就表示脚本的对象,返回一个实例化后的对象
        }
    }
    //在脚本整个生命周期内它仅被调用一次.Awake在所有对象被初始化之后调用.Awake总是在Start之前被调用。它不能用来执行协同程序。
    void Awake()
    {
        _current = this;//当前的对象为这个类  这里的this相当于new Loom()
        initialized = true;//将是否实例化标记设置为true            在awake唤醒的时候就将initialized设置为true,那么下面实例化的方法不就永远执行不了了?
    }
    
    //实例化的方法
    static void Initialize()
    {
        if (!initialized)//如果第一次实例化,不懂这句话能不能执行?或许这句话的意思是如果没有将脚本附到游戏组件上,那么调用实例化的方法可以自动生成一个游戏对象,这就说得通
        {
            if (!Application.isPlaying)//返回是否处在Unity的播放模式中,如果没运行就返回
                return;
            initialized = true;//初始化标记设置为true
            var g = new GameObject("Loom");//新建一个游戏对象名字为Loom
            _current = g.AddComponent<Loom>();//给新建的游戏对象附加脚本组件,并将对象返回给当前对象的实例
        }
    }
    private List<Action> _actions = new List<Action>();//定义泛型数组,类型是Action(无返回值的委托)    猜测用来储存线程?******************
    //一个结构,用来存放线程的,方法的名字是延时排列项目
    public struct DelayedQueueItem
    {
        public float time;//延长的时间
        public Action action;//执行的委托
    }
    private List<DelayedQueueItem> _delayed = new List<DelayedQueueItem>();//一个存放线程延时委托的列表(在这里要分清数组,ArrayList和List的区别)
    List<DelayedQueueItem> _currentDelayed = new List<DelayedQueueItem>();//当前延时线程列表
    /// <summary>
    /// 排列主线程?
    /// </summary>
    /// <param name="action">委托的方法</param>
    public static void QueueOnMainThread(Action action)
    {
        QueueOnMainThread(action, 0f);//调用两个参数的重载方法
    }
    /// <summary>
    /// 将委托排列到主线程中
    /// </summary>
    /// <param name="action">委托的方法</param>
    /// <param name="time">延迟的时间</param>
    public static void QueueOnMainThread(Action action, float time)
    {
        if (time != 0)//如果传入的时间不为零
        {
            lock (Current._delayed)//锁定当前对象中的延时的类型为DelayedQueueItem的List泛型
            {
                Current._delayed.Add(new DelayedQueueItem { time = Time.time + time, action = action });//向泛型里面添加新的对象,时间为从游戏开始的时间到现在的时间加上传入的时间,委托加上传入的委托
            }
        }
        else//传入的时间参数为零
        {
            lock (Current._actions)//锁定当前对象中的类型为Action的List泛型,
            {
                Current._actions.Add(action);//向List泛型里面添加委托
            }
        }
    }
    /// <summary>
    /// 用多线程运行委托的方法
    /// </summary>
    /// <param name="a">方法委托</param>
    /// <returns>返回的值为null?那为什么还要返回?</returns>
    public static Thread RunAsync(Action a)
    {
        Initialize();//初始化,将_current变量赋值
        while (numThreads >= maxThreads)//当运行的多线程的数量大于等于最大线程数量的时候
        {
            Thread.Sleep(1);//线程停止1毫秒
        }
        Interlocked.Increment(ref numThreads);//不懂?数量递增?*****************************************
        ThreadPool.QueueUserWorkItem(RunAction, a);//不懂?将方法排入线程队列中?****************************************
        return null;
    }
    /// <summary>
    /// 运行委托
    /// </summary>
    /// <param name="action">传入的委托</param>
    private static void RunAction(object action)
    {
        try
        {
            ((Action)action)();//类型转换,拆箱?
        }
        catch//不报错
        {
        }
        finally
        {
            Interlocked.Decrement(ref numThreads);//线程运行数量递减?*********************
        }
    }
    /// <summary>
    /// 当物体被销毁时它将被调用，并且可用于任意清理代码。当脚本编译完成之后被重加载时，OnDisable将被调用
    /// </summary>
    void OnDisable()
    {
        if (_current == this)//如果实例化的对象等于当前的对象
        {
            _current = null;//清空
        }
    }
    List<Action> _currentActions = new List<Action>();//这个类型为Action的List数组是干嘛用的?当前委托?***************
    // Update is called once per frame
    void Update()
    {
        lock (_actions)//锁住类型为Action的List数组
        {
            _currentActions.Clear();//清空当前List数组的元素
            _currentActions.AddRange(_actions);//向当前委托数组_currentActions添加委托的方法  在这里要搞清楚_currentActions,_actions,_current,_currentDelayed,_delayed,Current,这几个变量的作用和意义
            _actions.Clear();//清空_actions数组的元素********************_actions的作用?
        }
        foreach (var a in _currentActions)//遍历_currentActions    _currentActions的作用?
        {
            a();//运行List里面的所有委托的方法
        }
        lock (_delayed)//锁住_delayed变量 _delayed什么作用?
        {
            _currentDelayed.Clear();//清空_currentDelayed元素
            _currentDelayed.AddRange(_delayed.Where(d => d.time <= Time.time));//向_currentDelayed里面添加元素(当_delayed里面储存的时间的变量小鱼从运行到现在的时间则添加)?**************************
            foreach (var item in _currentDelayed)//遍历_currentDelayed
                _delayed.Remove(item);//从_delayed移除元素
        }
        foreach (var delayed in _currentDelayed)
        {
            delayed.action();//遍历_currentDelayed运行里面的所有委托方法
        }
    }
}
