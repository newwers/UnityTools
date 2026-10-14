using UnityEngine;

namespace SteamSDK.SO
{
    [CreateAssetMenu(fileName = "WateringCountAchievementSO", menuName = "创建Steam成就SO/进行了X次灌溉", order = 2)]
    public class WateringCountAchievementSO : SteamAchievementDataSO
    {
        private const string PREFS_KEY = "WateringCount";
        
        public override bool OnCheck()
        {
            if (achievementData.IsAchieved)
            {
                return false;
            }
            
            // 检查浇水的次数是否达到要求
            int count = StorageSystem.LoadIntFromPlayerPrefs(PREFS_KEY, 0);
            if (count >= requiredNum)
            {
                OnTriggerAchievement();
                return true;
            }
            
            return false;
        }
        
        public override float GetRunTimeValue()
        {
            // 返回当前浇水的次数
            return StorageSystem.LoadIntFromPlayerPrefs(PREFS_KEY, 0);
        }
        
        /// <summary>
        /// 记录浇水的操作
        /// </summary>
        public static void RecordWatering()
        {
            int currentCount = StorageSystem.LoadIntFromPlayerPrefs(PREFS_KEY, 0);
            StorageSystem.SaveIntToPlayerPrefs(PREFS_KEY, currentCount + 1);
        }

        public override void OnReset()
        {
            StorageSystem.DeleteKeyFromPlayerPrefs(PREFS_KEY);
        }

        public override void OnUserStatsReceived(bool isAchieved)
        {
            base.OnUserStatsReceived(isAchieved);

            if (string.IsNullOrEmpty(achievementData.AssociatedVariableName))
            {
                return;
            }

            if (achievementData.IsAchieved == false)
            {
                // 同步本地数据以反映成就统计的状态
                if (achievementData.ValueType == SteamSDK.ValueType.Int)
                {
                    StorageSystem.SaveIntToPlayerPrefs(PREFS_KEY, (int)GetValue());
                }
                else
                {
                    if (achievementData.ValueType == SteamSDK.ValueType.Float)
                    {
                        StorageSystem.SaveFloatToPlayerPrefs(PREFS_KEY, GetValue());
                    }
                }
            }
        }
    }
}