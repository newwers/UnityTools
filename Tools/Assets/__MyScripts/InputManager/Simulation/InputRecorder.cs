using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace InputSimulation
{
    public class InputRecorder 
    {
        [System.Serializable]
        public struct Info
        {
            public double time;
            public EMouseButton button;
            public MouseHook.POINT point;
            public bool isDown;

            public override string ToString()
            {
                return $"button:{button},point:{point},isdown:{isDown},time:{time}";
            }
        }

        public enum EMouseButton
        {
            Left=0,
            Right=1
        }

        public static event Action OnStopPlay;
        public static event Action OnStartRecord;
        public static event Action OnEndRecord;

        static InputRecorder m_sInstance = null;

        List<Info> m_vRecordInfo = new List<Info>();
        bool m_bPlay = false;
        int m_nPlayIndex = 0;
        double m_nRecordTime;
        double m_nTimer = 0;

        public InputRecorder()
        {
            m_sInstance = this;
            Debug.Log("InputRecorder 构造函数调用");
        }

        public void StartRecord()
        {
            MouseHook.ButtonClick += MouseHook_ButtonDown;
            m_vRecordInfo.Clear();
            m_bPlay = false;
            m_nPlayIndex = 0;
            m_nRecordTime = 0;
            m_nTimer = 0;
            OnStartRecord?.Invoke();
        }

        public void EndRecord()
        {
            MouseHook.ButtonClick -= MouseHook_ButtonDown;
            m_bPlay = false;
            m_nPlayIndex = 0;
            m_nRecordTime = 0;
            m_nTimer = 0;
            if (m_vRecordInfo.Count > 1)//移除点击停止时的点击
            {
                m_vRecordInfo.RemoveAt(m_vRecordInfo.Count - 1);
                m_vRecordInfo.RemoveAt(m_vRecordInfo.Count - 1);
            }
            OnEndRecord?.Invoke();
        }

        private void MouseHook_ButtonDown(int button,bool isDown, MouseHook.POINT point)
        {
            if (m_vRecordInfo.Count > 0)
            {
                m_nRecordTime = EditorApplication.timeSinceStartup - m_nRecordTime;
            }
            else
            {
                m_nRecordTime = 0;
            }
            AddInfo(button, isDown, point);

            m_nRecordTime = EditorApplication.timeSinceStartup;
        }

        public void AddInfo(int button,bool isDown, MouseHook.POINT point)
        {
            Debug.Log("m_nRecordTime:" + m_nRecordTime);
            Info info = new Info();
            info.time = m_nRecordTime;
            info.button = (EMouseButton)button;
            info.point = point;
            info.isDown = isDown;
            m_vRecordInfo.Add(info);
        }

        public void Play()
        {
            if (m_vRecordInfo.Count == 0)
            {
                return;
            }
            m_nPlayIndex = 0;
            m_nPlayIndex = 0;
            m_nRecordTime = 0;
            m_nTimer = 0;
            m_bPlay = true;
            UnityEngine.Debug.Log("InputRecorder Play");
        }

        public void Stop()
        {
            m_bPlay = false;
            m_nPlayIndex = 0;
            m_nPlayIndex = 0;
            m_nRecordTime = 0;
            m_nTimer = 0;
            UnityEngine.Debug.Log("InputRecorder Stop");
            OnStopPlay?.Invoke();
        }

        public static void SStop()
        {
            if (m_sInstance != null)
            {
                m_sInstance.Stop();
            }
        }

        public static void SStartRecord()
        {
            if (m_sInstance != null)
            {
                m_sInstance.StartRecord();
            }
        }

        public static void SEndRecord()
        {
            if (m_sInstance != null)
            {
                m_sInstance.EndRecord();
            }
        }

        public void Update()
        {
            if (!m_bPlay || m_vRecordInfo.Count == 0 || m_nPlayIndex>= m_vRecordInfo.Count)
            {
                return;
            }

            var info = m_vRecordInfo[m_nPlayIndex];
            if (m_nTimer == 0)
            {
                m_nTimer = info.time + EditorApplication.timeSinceStartup;//先等待再执行
            }
            

            if (EditorApplication.timeSinceStartup < m_nTimer)
            {
                return;
            }

            ExcudeInfo(info, m_nPlayIndex);

            m_nPlayIndex++;

            if (m_nPlayIndex >= m_vRecordInfo.Count)
            {
                Stop();
            }

            m_nTimer = 0;
        }

        void ExcudeInfo(Info info,int index)
        {
            Debug.Log($"index:{index}  info: {info.ToString()}");
            MouseHook.MoveTo(info.point.X, info.point.Y);
            switch (info.button)
            {
                case EMouseButton.Left:
                    if (info.isDown)
                    {
                        MouseHook.LeftClickDown();
                    }
                    else
                    {
                        MouseHook.LeftClickUp();
                    }
                    break;
                case EMouseButton.Right:
                    if (info.isDown)
                    {
                        MouseHook.RightClickDown();
                    }
                    else
                    {
                        MouseHook.RightClickUp();
                    }
                    break;
            }
        }
        //------------------------------------------------------
        public List<Info> GetInfos()
        {
            return m_vRecordInfo;
        }
        //------------------------------------------------------
        public void Load(List<Info> infos)
        {
            EndRecord();
            Stop();

            //根据本地文件,加载数据到内存中
            m_vRecordInfo = infos;
        }
    }
}