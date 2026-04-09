using UnityEngine;

namespace SteamSDK.SO
{
    [CreateAssetMenu(fileName = "LegendaryMushroomEffectAchievementSO", menuName = "创建Steam成就SO/触发传说级蘑菇特效", order = 2)]
    public class LegendaryMushroomEffectAchievementSO : SteamAchievementDataSO
    {
        private const string PREFS_KEY = "LegendaryMushroomEffectCount";

        public override bool OnCheck()
        {
            if (achievementData.IsAchieved)
            {
                return false;
            }

            int triggerCount = StorageSystem.LoadIntFromPlayerPrefs(PREFS_KEY);
            if (triggerCount >= requiredNum)
            {
                OnTriggerAchievement();
                return true;
            }

            return false;
        }

        public override float GetRunTimeValue()
        {
            return StorageSystem.LoadIntFromPlayerPrefs(PREFS_KEY, 0);
        }

        /// <summary>
        /// 记录传说级蘑菇特效触发
        /// </summary>
        public static void RecordLegendaryMushroomEffect()
        {
            int currentCount = StorageSystem.LoadIntFromPlayerPrefs(PREFS_KEY, 0);
            StorageSystem.SaveIntToPlayerPrefs(PREFS_KEY, currentCount + 1);
        }
    }
}