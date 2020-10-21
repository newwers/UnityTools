using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionTween : MonoBehaviour
{
    public float Duration = 1f;
    public Vector3 StartPos;
    public Vector3 EndPos;

    public bool isLoop = true;

    public bool isEnable = false;

    public Action OnCompleteAction;

    float m_Timer;

    public void OnStart(Vector3 startPos,Vector3 endPos,float duration)
    {
        StartPos = startPos;
        EndPos = endPos;
        this.Duration = duration;

        isEnable = true;
        m_Timer = 0f;
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
            
            transform.localPosition = Vector3.Lerp(StartPos, EndPos, m_Timer / Duration);
        }
    }
}
