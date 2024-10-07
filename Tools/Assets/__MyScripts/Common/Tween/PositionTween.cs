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

    private float m_Duration = 1f;
    private Vector3 m_StartPos;
    private Vector3 m_EndPos;

    public List<Vector4> PosList;
    private int m_Index;

    public bool isLoop = true;

    [Header("是否开启动画播放")]
    public bool isEnable = false;

    public Action OnCompleteAction;

    float m_Timer;

    [Header("是否使用本地坐标")]
    public bool isLocalPos = true;
    [Header("是否使用锚点坐标")]
    public bool isAnchorsPos = false;
    [Header("是否使用相对坐标")]
    public bool isRelativePos = false;

    public int ID;

    [Header("一开始就播放动画")]
    public bool PlayOnAwake = false;

    RectTransform m_RectTransform;

    private void Awake()
    {
        m_RectTransform = transform as RectTransform;

        if (PlayOnAwake && PosList.Count > 1)
        {
            m_Index = 0;
            m_StartPos = PosList[m_Index];
            m_EndPos = PosList[m_Index+1];
            m_Duration = PosList[m_Index].w;
            isEnable = true;
            OnStart(m_StartPos, m_EndPos, m_Duration);
        }
    }

    public void OnStart(Vector3 startPos, Vector3 endPos, float duration)
    {
        m_StartPos = startPos;
        m_EndPos = endPos;
        this.m_Duration = duration;

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
        if (m_Timer >= m_Duration )
        {
            if (m_Index >= PosList.Count-1 && isLoop == false)
            {
                isEnable = false;
                m_Timer = 0f;
                OnComplete();
                return;
            }

            m_Index ++;
            m_StartPos = PosList[m_Index];
            m_EndPos = PosList[m_Index + 1];
            m_Duration = PosList[m_Index].w;

        }
        if (isLoop)
        {
            m_Timer %= m_Duration;
        }

        if (isRelativePos)
        {
            if (isAnchorsPos && m_RectTransform)//锚点坐标
            {
                m_RectTransform.anchoredPosition += Vector2.Lerp(m_StartPos, m_EndPos, m_Timer / m_Duration) ;
            }
            else if (isLocalPos)//本地坐标
            {
                transform.localPosition += Vector3.Lerp(m_StartPos, m_EndPos, m_Timer / m_Duration);
            }
            else//世界坐标
            {
                transform.position += Vector3.Lerp(m_StartPos, m_EndPos, m_Timer / m_Duration);
            }
        }
        else
        {
            if (isAnchorsPos && m_RectTransform)//锚点坐标
            {
                m_RectTransform.anchoredPosition = Vector2.Lerp(m_StartPos, m_EndPos, m_Timer / m_Duration);
            }
            else if (isLocalPos)//本地坐标
            {
                transform.localPosition = Vector3.Lerp(m_StartPos, m_EndPos, m_Timer / m_Duration);
            }
            else//世界坐标
            {
                transform.position = Vector3.Lerp(m_StartPos, m_EndPos, m_Timer / m_Duration);
            }

        }

        

    }

    private void OnDestroy()
    {
        PositionTweenController.OnRemove(this);
    }
}
