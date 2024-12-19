using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace zdq
{
    /// <summary>
    /// 统计数据存档管理
    /// </summary>
    public class StatisticDataManager 
    {

        /// <summary>
        /// 单次运行信息
        /// </summary>
        [System.Serializable]
        public class SingleStatisticsData
        {
            /// <summary>
            /// 开始运行时间点
            /// </summary>
            public string fromTime;
            /// <summary>
            /// 结束运行时间点
            /// </summary>
            public string toTime;
            /// <summary>
            /// 运行时长(秒)
            /// </summary>
            public int runTime;

            /// <summary>
            /// 日期转字符串格式
            /// </summary>
            /// <param name="dateTime"></param>
            /// <returns></returns>
            public string ConvertDateTimeFormat(DateTime dateTime)
            {
                return dateTime.ToString("yyyy-MM-dd HH:mm:ss");
            }


        }

        /// <summary>
        /// 单天数据记录结构
        /// </summary>
        [System.Serializable]
        public class UnityStatisticsData
        {
            /// <summary>
            /// unity 启动次数
            /// </summary>
            public int runCount;
            /// <summary>
            /// 启动时长(单位秒)
            /// </summary>
            public int runTimeLength;

            public int year;
            public int month;
            public int day;

            public List<SingleStatisticsData> recordList;

            /// <summary>
            /// 获取本次运行的记录数据
            /// </summary>
            /// <returns></returns>
            public SingleStatisticsData GetThisRunData()
            {
                if (recordList == null)//容错处理,防止有未创建的情况
                {
                    recordList = new List<SingleStatisticsData>();
                }

                if (recordList.Count == 0)
                {
                    SingleStatisticsData singleStatisticsData = new SingleStatisticsData();
                    singleStatisticsData.fromTime = singleStatisticsData.ConvertDateTimeFormat(DateTime.Now);
                    recordList.Add(singleStatisticsData);
                    return singleStatisticsData;
                }

                return recordList[recordList.Count-1];
            }
            /// <summary>
            /// 添加本次运行时候的数据
            /// </summary>
            public void AddThisRunData()
            {
                if (recordList == null)//容错处理,防止有未创建的情况
                {
                    recordList = new List<SingleStatisticsData>();
                }

                SingleStatisticsData singleStatisticsData = new SingleStatisticsData();
                singleStatisticsData.fromTime = singleStatisticsData.ConvertDateTimeFormat(DateTime.Now);
                recordList.Add(singleStatisticsData);
            }
        }
        /// <summary>
        /// 所有数据记录结构
        /// </summary>
        [System.Serializable]
        public class UnityStatisticsSaveData
        {
            /// <summary>
            /// unity 启动次数
            /// </summary>
            public List<UnityStatisticsData> datas;
        }

        public static readonly string SaveFileName = "UnityStatistics.json";
        public static readonly string SaveFileRoot = Application.dataPath + "/../zdq/";
        public static readonly string SaveFilePath = SaveFileRoot + SaveFileName;



        public static Action OnFirstRunUnityAction;
        public static Action OnRunUnityAction;

        static UnityStatisticsSaveData m_pSaveData = null;

        public static string GetJsonData()
        {
            if (Z.FileTool.FileTools.ExistFile(SaveFilePath) == false)
            {
                return null;
            }
            var json = Z.FileTool.FileTools.ReadFile(SaveFilePath, System.Text.Encoding.UTF8);
            return json;
        }

        public static UnityStatisticsSaveData GetStatisticData()
        {
            if (m_pSaveData != null)
            {
                return m_pSaveData;
            }

            var json = GetJsonData();
            if (json == null)
            {
                return null;
            }

            m_pSaveData = Newtonsoft.Json.JsonConvert.DeserializeObject<UnityStatisticsSaveData>(json);
            return m_pSaveData;
        }

        public static void SaveData(string json)
        {
            Z.FileTool.FileTools.CreateDirectory(SaveFileRoot);
            Z.FileTool.FileTools.WriteFile(SaveFilePath, json, System.Text.Encoding.UTF8);
        }

        public static void SaveData(UnityStatisticsSaveData data)
        {
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(data, Formatting.Indented);
            SaveData(json);
        }

        public static void InitData()
        {
            DateTime dateTime = DateTime.Now;

            bool isExit = false;

            var data = GetStatisticData();

            //检查当天是否有数据存在
            for (int i = 0; i < data.datas.Count; i++)
            {
                var item = data.datas[i];
                if (item.year == dateTime.Year && item.month == dateTime.Month && item.day == dateTime.Day)
                {
                    isExit = true;

                    item.AddThisRunData();
                    data.datas[i] = item;
                    OnRunUnityAction?.Invoke();
                    SaveData(m_pSaveData);
                    break;
                }
            }

            //当天没有数据情况
            if (isExit == false)
            {
                var item = new UnityStatisticsData();
                item.year = dateTime.Year;
                item.month = dateTime.Month;
                item.day = dateTime.Day;
                item.runCount = 0;

                item.recordList = new List<SingleStatisticsData>();
                SingleStatisticsData singleStatisticsData = new SingleStatisticsData();
                singleStatisticsData.fromTime = singleStatisticsData.ConvertDateTimeFormat(dateTime);
                item.recordList.Add(singleStatisticsData);

                data.datas.Add(item);

                OnFirstRunUnityAction?.Invoke();

                SaveData(m_pSaveData);
            }

        }

        /// <summary>
        /// 获取当前数据
        /// </summary>
        /// <returns></returns>
        public static UnityStatisticsData GetTodayData()
        {
            var data = GetStatisticData();
            DateTime dateTime = DateTime.Now;

            for (int i = 0; i < data.datas.Count; i++)
            {
                var item = data.datas[i];
                if (item.year == dateTime.Year && item.month == dateTime.Month && item.day == dateTime.Day)
                {
                    return item;
                }
            }

            Debug.LogError("获取不到当天数据,请确保执行了 InitData 函数!");

            return null;
        }
    }
}