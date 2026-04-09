using UnityEngine;

namespace SteamSDK.SO
{
    [CreateAssetMenu(fileName = "OpenMoundCountAchievementSO", menuName = "创建Steam成就SO/打开了X个土包", order = 2)]
    public class OpenMoundCountAchievementSO : SteamAchievementDataSO
    {
        private const string PREFS_KEY = "OpenMoundCount";
        
        public override bool OnCheck()
        {
            if (achievementData.IsAchieved)
            {
                return false;
            }
            
            // 检查打开土包的次数是否达到要求
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
            // 返回当前打开土包的次数
            return StorageSystem.LoadIntFromPlayerPrefs(PREFS_KEY, 0);
        }
        
        /// <summary>
        /// 记录打开土包的操作
        /// </summary>
        public static void RecordOpenMound()
        {
            int currentCount = StorageSystem.LoadIntFromPlayerPrefs(PREFS_KEY, 0);
            StorageSystem.SaveIntToPlayerPrefs(PREFS_KEY, currentCount + 1);
        }
    }
}