using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace zdq
{
    /// <summary>
    /// 统计在 unity 中进行的操作脚本
    /// </summary>
    public class UnityStatistics
    {
        [System.Serializable]
        class UnityStatisticsData
        {
            /// <summary>
            /// unity 启动次数
            /// </summary>
            public int runCount;
            /// <summary>
            /// unity 启动时间(单位分)
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
        /// 标记几次操作自动保存
        /// </summary>
        const int SaveDirtyCount = 5;
        int m_Dirty = 0;

        /// <summary>
        /// 私有化构造函数,禁止new
        /// </summary>
        UnityStatistics() 
        {
            //Init();
            Debug.Log("UnityStatistics() m_data:" + m_data);
        }

        /// <summary>
        /// 静态构造函数,第一次调用该静态对象的时候触发
        /// </summary>
        static UnityStatistics()
        {
            Debug.Log("static UnityStatistics");

        }

        /// <summary>
        /// 析构函数
        /// 主要用于在垃圾回收器回收类实例时执行一些必要的清理操作
        /// 析构函数只能在类中定义，不能用于结构体；
        //  一个类中只能定义一个析构函数；
        //  析构函数不能继承或重载；
        //  析构函数没有返回值；
        //  析构函数是自动调用的，不能手动调用；
        //  析构函数不能使用访问权限修饰符修饰，也不能包含参数。

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
            //读取数据
            LoadDatas();

            //监听启动
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
        /// 展示保存的数据
        /// </summary>
        public void ShowDatas()
        {
            if (m_data == null)
            {
                return;
            }
            Debug.Log("当前运行unity次数:" + m_data.runCount + ",运行时间:" + m_data.runTime);
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
        /// 监听运行unity
        /// </summary>
        public void OnRunUnity()
        {
            m_data.runCount++;

            m_Dirty++;
        }

        /// <summary>
        /// 当前unity 处于焦点状态时,记录运行时间
        /// </summary>
        public void OnUnityFocus()
        {
            //m_data.runTime

            m_Dirty++;
        }

    }
}