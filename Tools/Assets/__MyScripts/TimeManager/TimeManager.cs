using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 时间管理,提供游戏全局时间相关函数操作
/// 需要定时操作的,可以将函数注册到这里进行调用
/// </summary>
public class TimeManager : MonoBehaviour
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

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        //LogManager.Log("DateTime.UtcNow=" + DateTime.UtcNow);
        //LogManager.Log("m_CurrentTimeStamp=" + m_CurrentTimeStamp);
    }

    private void Update()
    {
        m_PerSecondTimer += Time.deltaTime;
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
                m_TimeEvents[i].OnTriggerAction?.Invoke();
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
        m_TimeEvents.Add(data);
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

    public void RemoveTimeEvent(TimeEventData data)
    {
        m_TimeEvents.Remove(data);
    }

    public void Clear()
    {
        m_TimeEvents.Clear();
    }

    void Test()
    {
        //Calendar 日历类
        //TimeSpan 时间间隔类
        bool isEveryDay = true;
        bool isWeekday = false;
        int year = 2020;
        int month = 12;
        int day = 17;
        int hour = 10;
        int minute = 30;
        int second = 0;
        DayOfWeek weekday = DayOfWeek.Friday;
        TimeEventData data = new TimeEventData();
        if (isEveryDay)
        {
            data.date = new DateTime(Now.Year, Now.Month, Now.Day, hour, minute, second);
        }
        else if (isWeekday)
        {
            //需要添加的天数 = 目标星期数 - 当前星期数
            int addDays = weekday - Now.DayOfWeek;
            if (addDays < 0)//如果小于0,就+7天
            {
                addDays += 7;
            }
            data.date = Now.AddDays(addDays);
            data.weekday = weekday;
        }
        else
        {
            data.date = new DateTime(year, month, day, hour, minute, second);
        }


        data.isEveryDay = isEveryDay;
        data.showTips = "111";

        data.OnTriggerAction = () =>
        {
            if (isEveryDay)
            {
                data.date = data.date.AddDays(1);
            }
            else if(isWeekday)
            {
                data.date = data.date.AddDays(7);
            }
        };


        AddTimeEvent(data);
    }

}
