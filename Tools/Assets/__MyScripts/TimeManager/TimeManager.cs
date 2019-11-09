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
    /// <summary>
    /// 每秒计时器
    /// </summary>
    private float m_PerSecondTimer;

    public double m_CurrentTimeStamp
    {
        get {
            return new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
        }
    }

    /// <summary>
    /// 每秒更新一次函数
    /// </summary>
    public List<Action> m_PerSecondUpdateFunc = new List<Action>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        LogManager.Log("DateTime.UtcNow=" + DateTime.UtcNow);
        LogManager.Log("m_CurrentTimeStamp=" + m_CurrentTimeStamp);
    }

    private void Update()
    {
        m_PerSecondTimer += Time.deltaTime;
        if (m_PerSecondTimer >= 1f)
        {
            m_PerSecondTimer -= 1f;
            foreach (var item in m_PerSecondUpdateFunc)
            {
                item();
            }
        }
    }
}
