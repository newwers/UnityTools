using Steamworks;
using UnityEngine;

namespace SteamSDK
{
    [CreateAssetMenu(fileName = "SteamAchievementDataSO", menuName = "创建Steam成就SO", order = 2)]
    public class SteamAchievementDataSO : ScriptableObject
    {
        [SerializeField]
        protected SteamAchievementData achievementData = new SteamAchievementData();
        public int requiredNum = 0;


        //private void OnEnable()
        //{
        //    LogManager.Log($"成就: {achievementData.AchievementId}OnEnable");//在游戏启动后会运行
        //}
        public SteamAchievementData AchievementData
        {
            get { return achievementData; }
            set { achievementData = value; }
        }
        /// <summary>
        /// 获取当前成就的运行时值，子类可以重写此方法以提供特定的逻辑来计算成就条件的当前值，例如当前游戏时间、已完成的任务数量等
        /// </summary>
        /// <returns></returns>
        public virtual float GetRunTimeValue()
        {
            return 0;
        }

        public float GetValue()
        {
            if (achievementData.ValueType == ValueType.Int)
            {
                return achievementData.IntValue;
            }
            else if (achievementData.ValueType == ValueType.Float)
            {
                return achievementData.FloatValue;
            }
            return 0;
        }

        public virtual void SetValue(float value)
        {
            if (achievementData.ValueType == ValueType.Int)
            {
                if (achievementData.IntValue < (int)value)
                {
                    achievementData.IntValue = (int)value;
                }
            }
            else if (achievementData.ValueType == ValueType.Float)
            {
                if (achievementData.FloatValue < value)
                {
                    achievementData.FloatValue = value;
                }
            }
        }

        /// <summary>
        /// 检查成就条件
        /// </summary>
        /// <returns>在满足条件时返回true</returns>
        public virtual bool OnCheck()
        {
            if (achievementData.IsAchieved)
            {
                return false;
            }

            return false;

            //由子类实现具体的条件检查逻辑，当前暂时没有具体的条件检查逻辑
        }

        /// <summary>
        /// 当用户统计数据接收时调用
        /// </summary>
        public virtual void OnUserStatsReceived(bool isAchieved)
        {
            achievementData.IsAchieved = isAchieved;

            if (string.IsNullOrEmpty(achievementData.AssociatedVariableName))
            {
                return;
            }

            if (achievementData.ValueType == SteamSDK.ValueType.Int)
            {
                int value;
                SteamUserStats.GetStat(achievementData.AssociatedVariableName, out value);
                achievementData.IntValue = value;
            }
            else if (achievementData.ValueType == SteamSDK.ValueType.Float)
            {
                float floatValue;
                SteamUserStats.GetStat(achievementData.AssociatedVariableName, out floatValue);
                achievementData.FloatValue = floatValue;
            }
        }

        /// <summary>
        /// 当存储统计数据时调用
        /// </summary>
        public virtual void OnStoreStats()
        {
            SetValue(GetRunTimeValue());

            if (string.IsNullOrEmpty(achievementData.AssociatedVariableName))
            {
                return;
            }

            if (achievementData.ValueType == SteamSDK.ValueType.Int)
            {
                SteamUserStats.SetStat(achievementData.AssociatedVariableName, achievementData.IntValue);
            }
            else if (achievementData.ValueType == SteamSDK.ValueType.Float)
            {
                SteamUserStats.SetStat(achievementData.AssociatedVariableName, achievementData.FloatValue);
            }
        }

        /// <summary>
        /// 当触发成就时调用
        /// </summary>
        public virtual void OnTriggerAchievement()
        {
            achievementData.IsAchieved = true;
            OnStoreStats();
            SteamUserStats.SetAchievement(achievementData.AchievementId);//通知steam 成就达成
        }

        internal void OnResetAchievement()
        {
            achievementData.IsAchieved = false;
        }
    }
}
