using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QueueTask 
{
    public TaskState state = TaskState.UnBegin;

    Action<QueueTask> m_ExcuteEvent;

    Action<QueueTask> m_ComponentEvent;

    public QueueTask(Action<QueueTask> m_ExcuteEvent, Action<QueueTask> m_ComponentEvent)
    {
        state = TaskState.UnBegin;
        this.m_ExcuteEvent = m_ExcuteEvent;
        this.m_ComponentEvent = m_ComponentEvent;
    }

    /// <summary>
    /// 执行任务时的函数
    /// </summary>
    public void Excute()
    {
        state = TaskState.Running;
        if (m_ExcuteEvent == null)
        {
            return;
        }
        m_ExcuteEvent(this);
    }
    /// <summary>
    /// 当任务执行完时,需要用户自己手动调用
    /// </summary>
    public void OnComplete()
    {
        state = TaskState.Completed;

        if (m_ComponentEvent != null)
        {
            m_ComponentEvent(this);
        }
        

        QueueManager.Instance.StartNextQueue();
    }
}

public enum TaskState
{
    /// <summary>
    /// 未开始
    /// </summary>
    UnBegin,
    /// <summary>
    /// 进行中
    /// </summary>
    Running,
    /// <summary>
    /// 已完成
    /// </summary>
    Completed,
}
