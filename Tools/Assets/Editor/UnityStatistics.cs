using Newtonsoft.Json;
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
            /// ����ʱ��(��λ��?)
            /// </summary>
            public int runTimeLength;

            public int year;
            public int month;
            public int day;
        }
        [System.Serializable]
        class UnityStatisticsSaveData
        {
            /// <summary>
            /// unity ��������
            /// </summary>
            public List<UnityStatisticsData> datas;
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

        public static Action OnFirstRunUnityAction;
        public static Action OnRunUnityAction;
        public static Action OnExitUnityAction;


        UnityStatisticsSaveData m_data;
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
            Debug.Log("��̬���캯�� static UnityStatistics");

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
            Debug.Log("�������� ~UnityStatistics m_data:" + m_data);
            OnDestroy();

        }

        public void Init()
        {
            m_SaveFileName = "UnityStatistics.json";
            m_SaveFileRoot = Application.dataPath + "/../zdq/";
            m_SaveFilePath = m_SaveFileRoot + m_SaveFileName;
            Debug.Log("file path:" + m_SaveFilePath);
            //��ȡ����
            if (m_data == null)
            {
                LoadDatas();
            }

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
                m_data = new  UnityStatisticsSaveData();
                m_data.datas = new List<UnityStatisticsData>();
                return;
            }
            var json = Tools.FileTool.FileTools.ReadFile(m_SaveFilePath,System.Text.Encoding.UTF8);
            m_data = JsonConvert.DeserializeObject<UnityStatisticsSaveData>(json);
        }

        void SaveDatas()
        {
            m_Dirty = 0;
            if (m_data == null)
            {
                return;
            }

            Tools.FileTool.FileTools.CreateDirectory(m_SaveFileRoot);
            string json = JsonConvert.SerializeObject(m_data, Formatting.Indented);
            Tools.FileTool.FileTools.WriteFile(m_SaveFilePath,json,System.Text.Encoding.UTF8);
            Debug.Log("����ͳ������! UnityStatisticsData");
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
            //Debug.Log("��ǰ����unity����:" + m_data.runCount + ",����ʱ��:" + m_data.runTime);
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
                    OnExitUnity();
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// unity �رյ�ʱ���¼����ʱ��
        /// </summary>
        private void OnExitUnity()
        {
            var data = GetTodayData();
            int time = (int)Time.realtimeSinceStartup;
            data.runTimeLength += time;
            Debug.Log("����������unity :" + time +" ��!");
            OnExitUnityAction?.Invoke();
        }


        /// <summary>
        /// ��������unity
        /// </summary>
        public void OnRunUnity()
        {
            Debug.Log("��ʼ����unity!");
            AddData();

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

        void AddData()
        {
            if (m_data == null || m_data.datas == null)
            {
                Debug.LogError("m_data is null !");
                return;
            }

            var data = GetTodayData();
            if (data != null)
            {
                data.runCount++;
            }
        }

        UnityStatisticsData GetTodayData()
        {
            DateTime dateTime = DateTime.Now;

            bool isExit = false;

            for (int i = 0; i < m_data.datas.Count; i++)
            {
                var item = m_data.datas[i];
                if (item.year == dateTime.Year && item.month == dateTime.Month && item.day == dateTime.Day)
                {
                    isExit = true;
                    OnRunUnityAction?.Invoke();
                    return item;
                }
            }

            if (isExit == false)
            {
                var item = new UnityStatisticsData();
                item.year = dateTime.Year;
                item.month = dateTime.Month;
                item.day = dateTime.Day;
                item.runCount = 0;

                m_data.datas.Add(item);

                OnFirstRunUnityAction?.Invoke();
                return item;
            }

            return null;
        }
    }
}