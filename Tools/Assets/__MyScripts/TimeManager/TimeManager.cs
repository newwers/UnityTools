using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 时间管理,提供游戏全局时间相关函数操作
/// 需要定时操作的,可以将函数注册到这里进行调用
/// 再判断时间满足触发条件后,触发对应事件
/// 然后检测触发重复类型,
/// </summary>
public class TimeManager
{
    public static TimeManager Instance;
    static int m_TimeEventIDIndex = 0;
    public int TimeEventIDIndex
    {
        get
        {
            m_TimeEventIDIndex++;
            return m_TimeEventIDIndex;
        }
    }
    /// <summary>
    /// 每秒计时器
    /// </summary>
    private float m_PerSecondTimer;

    /// <summary>
    /// 当前时间戳
    /// </summary>
    public double m_CurrentTimeStamp
    {
        get {
            return new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
        }
    }

    public DateTime Now
    {
        get
        {
            return DateTime.Now;
        }
    }

    private List<TimeEventData> m_TimeEvents = new List<TimeEventData>();

    
    /// <summary>
    /// 这里的委托和用List<Action>实现函数的调用,又什么区别呢?
    /// </summary>
    public delegate void PerSecondUpdateDelegate();
    /// <summary>
    /// 每秒更新一次函数
    /// </summary>
    private PerSecondUpdateDelegate PerSecondUpdateCallBack;

    static TimeManager()
    {
        if (Instance == null)
        {
            Instance = new TimeManager();

            //LogManager.Log("DateTime.UtcNow=" + DateTime.UtcNow);
            //LogManager.Log("m_CurrentTimeStamp=" + m_CurrentTimeStamp);
        }
    }

    public void Init()
    {
        //var datas = Tool.ReadTimeEvents();
        //SetTimeEvents(datas);
        //读取配置由UITime里面读取,并添加委托事件
    }

    public void Update(float time)
    {
        m_PerSecondTimer += time;
        if (m_PerSecondTimer >= 1f)
        {
            m_PerSecondTimer -= 1f;
            OnPerSecondUpdate();
            if (PerSecondUpdateCallBack != null)
            {
                PerSecondUpdateCallBack.Invoke();
            }
        }
    }

    private void OnPerSecondUpdate()
    {
        for (int i = m_TimeEvents.Count -1; i >= 0; i--)
        {
            if (CheckDateTimeEqual(m_TimeEvents[i].date, Now))
            {
                OnTriggerTimeEvent(m_TimeEvents[i]);
                CheckTriggerRepeat(m_TimeEvents[i]);
            }
        }
    }

    bool CheckDateTimeEqual(DateTime time1, DateTime time2)
    {
        //Debug.Log("time1:" + (long)time1.TimeOfDay.TotalSeconds + ",time2:" + (long)time2.TimeOfDay.TotalSeconds + ",equal:" + ((long)time1.TimeOfDay.TotalSeconds == (long)time2.TimeOfDay.TotalSeconds));
        return (long)time1.TimeOfDay.TotalSeconds == (long)time2.TimeOfDay.TotalSeconds;
    }

    /// <summary>
    /// 添加每秒调用函数
    /// </summary>
    public void AddPerSecondUpdateDelegate(PerSecondUpdateDelegate func)
    {
        PerSecondUpdateCallBack += func;
    }

    /// <summary>
    /// 移除每秒调用函数
    /// </summary>
    public void RemotePerSecondUpdateDelegate(PerSecondUpdateDelegate func)
    {
        PerSecondUpdateCallBack -= func;
    }

    public void AddTimeEvent(TimeEventData data)
    {
        //todo:判断id重复
        m_TimeEvents.Add(data);
    }

    public void RemoveTimeEvent(TimeEventData data)
    {
        m_TimeEvents.Remove(data);
    }

    public void SetTimeEvent(TimeEventData data)
    {
        for (int i = 0; i < m_TimeEvents.Count; i++)
        {
            if (m_TimeEvents[i].id == data.id)
            {
                m_TimeEvents[i] = data;
                break;
            }
        }
    }

    public TimeEventData GetTimeEvent(int id)
    {
        for (int i = 0; i < m_TimeEvents.Count; i++)
        {
            if (m_TimeEvents[i].id == id)
            {
                return m_TimeEvents[i];
            }
        }
        return null;
    }

    void OnTriggerTimeEvent(TimeEventData data)
    {
        data.OnTriggerAction?.Invoke(data);
    }

    /// <summary>
    /// 检测是否重复触发
    /// </summary>
    /// <param name="data"></param>
    void CheckTriggerRepeat(TimeEventData data)
    {
        if (data == null)
        {
            return;
        }
        if (data.nextTriggerTimeSpan > 0)
        {
            data.AddSecond(data.nextTriggerTimeSpan);
            Debug.Log("设置的时间:" + data.date.ToString());
        }
    }

    public void Clear()
    {
        m_TimeEvents.Clear();
    }

    void Test()
    {
        //Calendar 日历类
        //TimeSpan 时间间隔类
        TimeEventData data = new TimeEventData();

        var Now = TimeManager.Instance.Now;
        data.id = TimeManager.Instance.TimeEventIDIndex;
        data.nextTriggerTimeSpan = 5;
        data.hour = data.date.Hour;
        data.minute = data.date.Minute;
        Debug.Log("设置的时间:" + data.date.ToString());
        //data.OnTriggerAction = () =>
        //{
        //    //SetTipsText(tips);
        //};

        data.showTips = "111";

        AddTimeEvent(data);
    }

    public TimeEventSaveData GetSaveData()
    {
        TimeEventSaveData datas = new TimeEventSaveData();
        datas.datas = m_TimeEvents.ToArray();
        return datas;
    }

    public void SetTimeEvents(TimeEventSaveData datas)
    {
        if (datas == null)
        {
            return;
        }
        m_TimeEvents.Clear();
        foreach (var item in datas.datas)
        {
            m_TimeEvents.Add(item);
        }
    }

}
