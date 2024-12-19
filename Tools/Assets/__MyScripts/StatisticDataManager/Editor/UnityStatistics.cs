using Newtonsoft.Json;
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
            Debug.Log("静态构造函数 static UnityStatistics");

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
            Debug.Log("析构函数 ~UnityStatistics m_data:" + m_data);
            OnDestroy();

        }

        public void Init()
        {
            Debug.Log("file path:" + StatisticDataManager.SaveFilePath);
            //读取数据
            if (m_data == null)
            {
                LoadDatas();
            }

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
            Debug.Log("保存统计数据! UnityStatisticsData");
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
            //Debug.Log("当前运行unity次数:" + m_data.runCount + ",运行时间:" + m_data.runTime);
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
        /// unity 关闭的时候记录运行时间
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
                Debug.LogError("本次运行数据获取异常!请检查");
            }
            
            Debug.Log("本次运行了unity :" + time +" 秒!");
            SaveDatas();
            OnExitUnityAction?.Invoke();

            EditorApplication.playModeStateChanged -= playModeStateChanged;//退出unity的时候,取消事件注册,防止多次注册
        }


        /// <summary>
        /// 监听运行unity
        /// </summary>
        public void OnRunUnity()
        {
            Debug.Log("开始运行unity!");
            StatisticDataManager.InitData();//统计只有编辑器下进行,所以由这边进行初始化,其他地方使用,要注意初始化调用顺序
            AddData();

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