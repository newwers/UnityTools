using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 高级打击停顿系统，通过控制 Time.timeScale 实现多层级的时间缩放效果
/// </summary>
public class AdvancedHitStop : BaseMonoSingleClass<AdvancedHitStop>
{
    /// <summary>
    /// 时间缩放修改类型
    /// </summary>
    public enum TimeScaleModifyType
    {
        /// <summary>直接设置时间缩放值</summary>
        Direct,
        /// <summary>在原有时间缩放基础上叠加</summary>
        Additive
    }

    /// <summary>
    /// 打击停顿请求数据
    /// </summary>
    [System.Serializable]
    public class HitStopRequest
    {
        /// <summary>持续时间（秒）</summary>
        public float duration;
        /// <summary>时间缩放值</summary>
        public float timeScale;
        /// <summary>优先级（数值越大优先级越高）</summary>
        public float priority;
        /// <summary>时间缩放修改类型</summary>
        public TimeScaleModifyType modifyType;
        /// <summary>开始时间（使用真实时间）</summary>
        public float startTime;
        
        /// <summary>结束时间（使用真实时间）</summary>
        public float EndTime => startTime + duration;
        
        /// <summary>
        /// 根据修改类型获取有效的时间缩放值
        /// </summary>
        public float GetEffectiveTimeScale(float baseTimeScale)
        {
            return modifyType == TimeScaleModifyType.Direct ? timeScale : baseTimeScale + timeScale;
        }
    }

    private const float DEFAULT_TIME_SCALE = 1f;
    
    private List<HitStopRequest> activeRequests = new List<HitStopRequest>();
    private Coroutine updateCoroutine;
    private float previousTimeScale = DEFAULT_TIME_SCALE;

    /// <summary>
    /// 触发打击停顿效果
    /// </summary>
    /// <param name="duration">持续时间（秒）</param>
    /// <param name="timeScale">时间缩放值（0 为完全停止，1 为正常速度）</param>
    /// <param name="priority">优先级（数值越大优先级越高，高优先级会覆盖低优先级）</param>
    /// <param name="modifyType">时间缩放修改类型（Direct 为直接设置，Additive 为叠加）</param>
    public void TriggerHitStop(float duration, float timeScale, float priority = 1f, TimeScaleModifyType modifyType = TimeScaleModifyType.Direct)
    {
        HitStopRequest newRequest = new HitStopRequest
        {
            duration = duration,
            timeScale = timeScale,
            priority = priority,
            modifyType = modifyType,
            startTime = Time.realtimeSinceStartup
        };

        HitStopRequest currentHighestPriority = GetHighestPriorityRequest();

        if (currentHighestPriority != null)
        {
            if (priority > currentHighestPriority.priority)
            {
                RemoveRequest(currentHighestPriority);
                AddRequest(newRequest);
            }
            else if (priority < currentHighestPriority.priority)
            {
                return;
            }
            else
            {
                AddRequest(newRequest);
            }
        }
        else
        {
            previousTimeScale = Time.timeScale;
            AddRequest(newRequest);
        }

        if (updateCoroutine == null)
        {
            updateCoroutine = StartCoroutine(UpdateTimeScale());
        }
    }

    /// <summary>
    /// 添加请求到活动列表并按优先级排序
    /// </summary>
    private void AddRequest(HitStopRequest request)
    {
        activeRequests.Add(request);
        activeRequests.Sort((a, b) => b.priority.CompareTo(a.priority));
    }

    /// <summary>
    /// 从活动列表中移除指定请求
    /// </summary>
    private void RemoveRequest(HitStopRequest request)
    {
        activeRequests.Remove(request);
    }

    /// <summary>
    /// 获取当前优先级最高的请求
    /// </summary>
    private HitStopRequest GetHighestPriorityRequest()
    {
        return activeRequests.Count > 0 ? activeRequests[0] : null;
    }

    /// <summary>
    /// 每帧更新时间缩放值，根据当前优先级最高的请求应用效果
    /// </summary>
    private IEnumerator UpdateTimeScale()
    {
        while (activeRequests.Count > 0)
        {
            float currentTime = Time.realtimeSinceStartup;
            
            for (int i = activeRequests.Count - 1; i >= 0; i--)
            {
                if (currentTime >= activeRequests[i].EndTime)
                {
                    activeRequests.RemoveAt(i);
                }
            }

            if (activeRequests.Count > 0)
            {
                HitStopRequest highestPriority = activeRequests[0];
                float targetTimeScale = highestPriority.GetEffectiveTimeScale(previousTimeScale);
                Time.timeScale = targetTimeScale;
            }
            else
            {
                Time.timeScale = previousTimeScale;
            }

            yield return null;
        }

        Time.timeScale = previousTimeScale;
        updateCoroutine = null;
    }

    private void OnDestroy()
    {
        if (updateCoroutine != null)
        {
            StopCoroutine(updateCoroutine);
            updateCoroutine = null;
        }
        
        Time.timeScale = DEFAULT_TIME_SCALE;
        activeRequests.Clear();
    }
}