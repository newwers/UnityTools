/********************************************************************
生成日期:	17:9:2021   14:31
类    名: 	NumTween
作    者:	zdq
描    述:	数字跳动动画组件
*********************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TopGame.UI
{
    public class NumTween : MonoBehaviour
    {
        public Text text;

        public AnimationCurve timeCurve;

        float m_TweenTime = 0;

        int m_BeginValue = 0;
        int m_EndValue = 0;

        bool m_bEnable = false;
        bool m_bUpNumber = true;

        float m_Timer = 0;

        private void Start()
        {
            if (!text)
            {
                text = GetComponent<Text>();
            }
        }
        //------------------------------------------------------
        public void Play(int begin,int end)
        {
            if (begin == end)
            {
                return;
            }
            m_BeginValue = begin;
            m_EndValue = end;
            m_bUpNumber = begin < end;
            m_Timer = 0;
            m_bEnable = true;
            m_TweenTime = 1f;
            if (timeCurve != null && timeCurve.keys.Length > 0)
            {
                m_TweenTime = timeCurve[timeCurve.keys.Length - 1].time;
            }
        }
        //------------------------------------------------------
        public void Stop()
        {
            m_BeginValue = 0;
            m_EndValue = 0;
            m_bEnable = false;
            m_Timer = 0;
        }
        //------------------------------------------------------
        private void Update()
        {
            if (!m_bEnable || !text || timeCurve == null )
            {
                return;
            }

            if (m_Timer >= m_TweenTime)
            {
                if (text)
                {
                    text.text = m_EndValue.ToString();
                }
                Stop();
                return;
            }
            int value = m_BeginValue;
            float per = timeCurve.Evaluate(m_Timer);

            if (m_bUpNumber)
            {
                int dif = m_EndValue - m_BeginValue;
                int val = Mathf.FloorToInt(dif * per);
                value += val;
                //Debug.Log("value:" + value + ",val:" + val + ",per:" + per);
            }
            else
            {
                int dif = m_BeginValue - m_EndValue;
                int val = Mathf.FloorToInt(dif * per);
                value -= val;
            }
            
            if (text)
            {
                text.text = value.ToString();
                
            }

            m_Timer+= Time.unscaledDeltaTime;
        }
        //------------------------------------------------------
#if UNITY_EDITOR
        [ContextMenu("测试")]
        public void Test()
        {
            Play(Random.Range(0, 1000), Random.Range(0, 10000));
        }
#endif
    }
#if UNITY_EDITOR
    [CanEditMultipleObjects]
    [CustomEditor(typeof(NumTween))]
    class NumTweenEditor : Editor
    {
        private SerializedProperty m_timeCurve;
        private SerializedProperty m_text;

        void OnEnable()
        {
            if (target is NumTween)
            {
                NumTween text = (NumTween)target;
            }
            m_timeCurve = serializedObject.FindProperty("timeCurve");
            m_text = serializedObject.FindProperty("text");
        }
        //------------------------------------------------------
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            //EditorGUILayout.PropertyField(m_timeCurve);
            //EditorGUILayout.PropertyField(m_text);

            DrawToolBtns();

            serializedObject.ApplyModifiedProperties();
        }
        //------------------------------------------------------
        void DrawToolBtns()
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("测试"))
            {
                if (target is NumTween)
                {
                    var text = target as NumTween;
                    Undo.RecordObject(text, "NumTween");
                    text.Test();
                }
            }

            EditorGUILayout.EndHorizontal();
        }
    }
#endif
}
