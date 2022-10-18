using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace zdq
{
    /// <summary>
    /// ͳ���� unity �н��еĲ����ű�
    /// </summary>
    public class UnityStatistics
    {
        [System.Serializable]
        class UnityStatisticsData
        {
            /// <summary>
            /// unity ��������
            /// </summary>
            public int runCount;
            /// <summary>
            /// unity ����ʱ��(��λ��)
            /// </summary>
            public long runTime;
        }

        static UnityStatistics m_Instance = null;
        public static UnityStatistics Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    m_Instance = new UnityStatistics();
                }
                return m_Instance;
            }
        }

        UnityStatisticsData m_data;
        string m_SaveFilePath;
        string m_SaveFileRoot;
        string m_SaveFileName;

        /// <summary>
        /// ��Ǽ��β����Զ�����
        /// </summary>
        const int SaveDirtyCount = 5;
        int m_Dirty = 0;

        /// <summary>
        /// ˽�л����캯��,��ֹnew
        /// </summary>
        UnityStatistics() 
        {
            //Init();
            Debug.Log("UnityStatistics() m_data:" + m_data);
        }

        /// <summary>
        /// ��̬���캯��,��һ�ε��øþ�̬�����ʱ�򴥷�
        /// </summary>
        static UnityStatistics()
        {
            Debug.Log("static UnityStatistics");

        }

        /// <summary>
        /// ��������
        /// ��Ҫ����������������������ʵ��ʱִ��һЩ��Ҫ���������
        /// ��������ֻ�������ж��壬�������ڽṹ�壻
        //  һ������ֻ�ܶ���һ������������
        //  �����������ܼ̳л����أ�
        //  ��������û�з���ֵ��
        //  �����������Զ����õģ������ֶ����ã�
        //  ������������ʹ�÷���Ȩ�����η����Σ�Ҳ���ܰ���������

        /// </summary>
        ~UnityStatistics()
        {
            Debug.Log("~UnityStatistics m_data:" + m_data);
            OnDestroy();

        }

        public void Init()
        {
            m_SaveFileName = "UnityStatistics.json";
            m_SaveFileRoot = Application.dataPath + "/../zdq/";
            m_SaveFilePath = m_SaveFileRoot + m_SaveFileName;
            Debug.Log("file path:" + m_SaveFilePath);
            //��ȡ����
            LoadDatas();

            //��������
            EditorApplication.playModeStateChanged += playModeStateChanged;
        }


        public void Update()
        {
            if (m_Dirty >= SaveDirtyCount)
            {
                SaveDatas();
            }
        }

        public void OnDestroy()
        {
            SaveDatas();
            m_data = null;
            m_Dirty = 0;

            EditorApplication.playModeStateChanged -= playModeStateChanged;
        }

        private void LoadDatas()
        {
            if (Tools.FileTool.FileTools.ExistFile(m_SaveFilePath) == false)
            {
                m_data = new UnityStatisticsData();
                return;
            }
            m_data = JsonUtility.FromJson<UnityStatisticsData>(m_SaveFilePath);
        }

        void SaveDatas()
        {
            m_Dirty = 0;
            if (m_data == null)
            {
                return;
            }

            Tools.FileTool.FileTools.CreateDirectory(m_SaveFileRoot);
            string json = JsonUtility.ToJson(m_data);
            Tools.FileTool.FileTools.WriteFile(m_SaveFilePath,json,System.Text.Encoding.UTF8);
            Debug.Log("Save UnityStatisticsData");
        }

        /// <summary>
        /// չʾ���������
        /// </summary>
        public void ShowDatas()
        {
            if (m_data == null)
            {
                return;
            }
            Debug.Log("��ǰ����unity����:" + m_data.runCount + ",����ʱ��:" + m_data.runTime);
        }


        private void playModeStateChanged(PlayModeStateChange state)
        {
            Debug.Log("state:" + state);
            switch (state)
            {
                case PlayModeStateChange.EnteredEditMode:
                    break;
                case PlayModeStateChange.ExitingEditMode:
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    OnRunUnity();
                    break;
                case PlayModeStateChange.ExitingPlayMode:
                    break;
                default:
                    break;
            }
        }


        /// <summary>
        /// ��������unity
        /// </summary>
        public void OnRunUnity()
        {
            m_data.runCount++;

            m_Dirty++;
        }

        /// <summary>
        /// ��ǰunity ���ڽ���״̬ʱ,��¼����ʱ��
        /// </summary>
        public void OnUnityFocus()
        {
            //m_data.runTime

            m_Dirty++;
        }

    }
}