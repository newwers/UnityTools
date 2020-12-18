using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestTimeSystem : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        TimeManager.Instance.AddPerSecondUpdateDelegate(PerSecondUpdate);
        Test();
    }

    private void PerSecondUpdate()
    {
        
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
            data.date = new DateTime(TimeManager.Instance.Now.Year, TimeManager.Instance.Now.Month, TimeManager.Instance.Now.Day, hour, minute, second);
        }
        else if (isWeekday)
        {
            //需要添加的天数 = 目标星期数 - 当前星期数
            int addDays = weekday - TimeManager.Instance.Now.DayOfWeek;
            if (addDays < 0)//如果小于0,就+7天
            {
                addDays += 7;
            }
            data.date = TimeManager.Instance.Now.AddDays(addDays);
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
            else if (isWeekday)
            {
                data.date = data.date.AddDays(7);
            }
        };


        TimeManager.Instance.AddTimeEvent(data);
    }
}
