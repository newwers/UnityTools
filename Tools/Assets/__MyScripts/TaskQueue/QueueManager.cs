using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QueueManager : MonoBehaviour
{

    public static QueueManager Instance;

    /// <summary>
    /// 存放执行的任务队列
    /// </summary>
    public List<QueueTask> m_QueueDic = new List<QueueTask>();

    #region 初始化单例

    

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void OnDestroy()
    {
        if (Instance != null)
        {
            Instance = null;
        }
    }
    #endregion

    /// <summary>
    /// 启动队列
    /// </summary>
    public void StartQueue()
    {
        if (m_QueueDic.Count <= 0)
        {
            Debug.Log("队列没有任务,队列结束");
            return;
        }
        m_QueueDic[0].Excute();
        
        Debug.Log("启动队列");
    }

    /// <summary>
    /// 开始下一个队列
    /// </summary>
    public void StartNextQueue()
    {
        m_QueueDic.Remove(m_QueueDic[0]);
        if (m_QueueDic.Count <= 0)
        {
            Debug.Log("队列没有任务,任务结束");
            return;
        }
        m_QueueDic[0].Excute();
    }



}
