/********************************************************************
生成日期:	1:11:2020 13:16
类    名: 	AddNewItemAni
作    者:	zdq
描    述:	控制物品缩放动画
*********************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TopGame.UI
{
    public class ScaleTween : MonoBehaviour
    {
        public AnimationCurve ScaleCurve;
        public float PlayDelay;
        public int index;

        bool m_IsEnable = false;
        float m_DelayTimer = 0f;
        float m_PlayTimer = 0f;
        bool m_IsCallOnce = false;
        public void DisableOnFinish()
        {
            m_IsCallOnce = true;
        }

        private void Update()
        {
            if (m_IsEnable == false)
            {
                return;
            }
            m_DelayTimer += Time.deltaTime;
            if (m_DelayTimer >= PlayDelay)
            {
                transform.localScale = new Vector3(ScaleCurve.Evaluate(m_PlayTimer), ScaleCurve.Evaluate(m_PlayTimer), ScaleCurve.Evaluate(m_PlayTimer));
                m_PlayTimer += Time.deltaTime;
                if (m_PlayTimer >= ScaleCurve.keys[ScaleCurve.keys.Length -1].time)
                {
                    OnTweenCompleted();
                }
            }
        }

        void OnTweenCompleted()
        {
            Stop();


        }

        [ContextMenu("测试播放")]
        public void Play()
        {
            if (m_IsCallOnce) return;
            m_DelayTimer = 0f;
            m_PlayTimer = 0f;
            m_IsEnable = true;

            if (index > 0)
            {
                if (ScaleCurve.keys.Length > 3)
                {
                    PlayDelay = (index - 1) * ScaleCurve.keys[ScaleCurve.keys.Length - 2].time;
                }
            }
            transform.localScale = new Vector3(ScaleCurve.Evaluate(0), ScaleCurve.Evaluate(0), ScaleCurve.Evaluate(0));
        }

        public void Stop()
        {
            Clear();
        }

        void Clear()
        {
            m_IsEnable = false;
            PlayDelay = 0f;
            m_DelayTimer = 0f;
            m_PlayTimer = 0f;
            index = 0;
        }
    }
}
