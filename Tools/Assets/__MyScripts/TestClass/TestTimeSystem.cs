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
        TimeEventData data = new TimeEventData();
        data.showTips = "111";
        data.hour = 15;
        data.minute = 53;
        data.second = 0;
        data.nextTriggerTimeSpan = 60;


        TimeManager.Instance.AddTimeEvent(data);
    }
}
