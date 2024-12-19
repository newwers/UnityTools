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


        StatisticDataManager.UnityStatisticsSaveData m_data;
        
        

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
            Debug.Log("file path:" + StatisticDataManager.SaveFilePath);
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
            if (Z.FileTool.FileTools.ExistFile(StatisticDataManager.SaveFilePath) == false)
            {
                m_data = new StatisticDataManager.UnityStatisticsSaveData();
                m_data.datas = new List<StatisticDataManager.UnityStatisticsData>();
                return;
            }
            m_data = StatisticDataManager.GetStatisticData();
        }

        void SaveDatas()
        {
            m_Dirty = 0;
            if (m_data == null)
            {
                return;
            }

            string json = JsonConvert.SerializeObject(m_data, Formatting.Indented);
            StatisticDataManager.SaveData(json);
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
            var data = StatisticDataManager.GetTodayData();
            int time = (int)Time.realtimeSinceStartup;
            data.runTimeLength += time;
            var singleStatisticsData = data.GetThisRunData();
            if (singleStatisticsData != null)
            {
                singleStatisticsData.runTime = time;
                singleStatisticsData.toTime = singleStatisticsData.ConvertDateTimeFormat(DateTime.Now);
            }
            else
            {
                Debug.LogError("�����������ݻ�ȡ�쳣!����");
            }
            
            Debug.Log("����������unity :" + time +" ��!");
            SaveDatas();
            OnExitUnityAction?.Invoke();

            EditorApplication.playModeStateChanged -= playModeStateChanged;//�˳�unity��ʱ��,ȡ���¼�ע��,��ֹ���ע��
        }


        /// <summary>
        /// ��������unity
        /// </summary>
        public void OnRunUnity()
        {
            Debug.Log("��ʼ����unity!");
            StatisticDataManager.InitData();//ͳ��ֻ�б༭���½���,��������߽��г�ʼ��,�����ط�ʹ��,Ҫע���ʼ������˳��
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

            var data = StatisticDataManager.GetTodayData();
            if (data != null)
            {
                data.runCount++;
            }
        }

    }
}