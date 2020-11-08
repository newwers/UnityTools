/********************************************************************
生成日期:	
类    名: 	PositionTween
作    者:	zdq
描    述:	位置平滑过渡
*********************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionTweenController
{
    static Dictionary<int, PositionTween> ms_Tweens = new Dictionary<int, PositionTween>();
    public static void OnAdd(PositionTween tween)
    {
        if (tween == null) return;
        ms_Tweens[tween.ID] = tween;
    }
    public static void OnRemove(PositionTween tween)
    {
        if (tween == null) return;
        if (ms_Tweens.ContainsKey(tween.ID))
        {
            ms_Tweens.Remove(tween.ID);
        }
    }
    public static PositionTween Find(int id)
    {
        PositionTween tween;
        if (ms_Tweens.TryGetValue(id, out tween))
            return tween;
        return null;
    }
    public static bool IsPlaying(int id)
    {
        PositionTween tween;
        if (ms_Tweens.TryGetValue(id, out tween))
        {
            return tween.isEnable;
        }
        return false;
    }
}


public class PositionTween : MonoBehaviour
{
    public float Duration = 1f;
    public Vector3 StartPos;
    public Vector3 EndPos;

    public bool isLoop = true;

    [Header("是否开启动画播放")]
    public bool isEnable = false;

    public Action OnCompleteAction;

    float m_Timer;

    [Header("是否使用本地坐标")]
    public bool isLocalPos = true;

    public int ID;

    [Header("一开始就播放动画")]
    public bool PlayOnAwake = false;

    private void Awake()
    {
        if (PlayOnAwake)
        {
            isEnable = true;
            OnStart(StartPos, EndPos, Duration);
        }
    }

    public void OnStart(Vector3 startPos, Vector3 endPos, float duration)
    {
        StartPos = startPos;
        EndPos = endPos;
        this.Duration = duration;

        isEnable = true;
        m_Timer = 0f;

        PositionTweenController.OnAdd(this);
    }
    public void Stop()
    {
        isEnable = false;
        m_Timer = 0f;
    }
    public void OnComplete()
    {
        OnCompleteAction?.Invoke();
    }

    // Update is called once per frame
    void Update()
    {
        if (isEnable == false)
        {
            return;
        }
        m_Timer += Time.deltaTime;
        if (m_Timer >= Duration && isLoop == false)
        {
            isEnable = false;
            m_Timer = 0f;
            OnComplete();
            return;
        }
        if (isLoop)
        {
            if (m_Timer == Duration)
            {

            }
            else
            {
                m_Timer %= Duration;
            }
        }

        if (isLocalPos)
        {
            transform.localPosition = Vector3.Lerp(StartPos, EndPos, m_Timer / Duration);
        }
        else
        {
            transform.position = Vector3.Lerp(StartPos, EndPos, m_Timer / Duration);
        }

    }

    private void OnDestroy()
    {
        PositionTweenController.OnRemove(this);
    }
}
