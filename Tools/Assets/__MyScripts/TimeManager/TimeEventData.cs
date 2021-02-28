using System;
using UnityEngine;

[System.Serializable]
public class TimeEventData :IComparable
{
    public int id;

    DateTime m_date;
    /// <summary>
    /// 设置的日期,用DateTime就可以表示
    /// </summary>
    public DateTime date
    {
        get
        {
            if (m_date.Hour != hour || m_date.Minute != minute || m_date.Second != second)
            {
                m_date = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, hour, minute, second);
            }
            return m_date;
        }
    }
    /// <summary>
    /// 显示的提示语句
    /// </summary>
    public string showTips;
    /// <summary>
    /// 播放的音频
    /// </summary>
    public AudioClip playAudio;
    /// <summary>
    /// 当时间到后,执行的函数
    /// </summary>
    public Action<TimeEventData> OnTriggerAction;
    /// <summary>
    /// 下次触发间隔
    /// </summary>
    public int nextTriggerTimeSpan;

    public int hour;
    public int minute;
    public int second;

    public void AddSecond(int second)
    {
        int hour = second / (60 * 60);
        int minute = (second % (60/60)) / 60;

        this.second += second % 60;
        if (this.second > 59)
        {
            this.minute++;
        }
        this.second %= 60;

        this.minute += minute;
        if (this.minute > 59)
        {
            this.hour++;
        }
        this.minute %= 60;

        this.hour += hour;
        this.hour %= 24;

    }

    public int CompareTo(object obj)
    {
        if (obj == null) return 1;

        TimeEventData other = obj as TimeEventData;
        if (other != null)
        {
            if (this.hour != other.hour)
            {
                return this.hour - other.hour;
            }
            else if (this.minute != other.minute)
            {
                return this.minute - other.minute;
            }
            else if (this.second != other.second)
            {
                return this.second - other.second;
            }
            return 0;
        }
        else
            throw new ArgumentException("Object is not a TimeEventData");
    }
}
[System.Serializable]
public class TimeEventSaveData
{
    public TimeEventData[] datas;
}
