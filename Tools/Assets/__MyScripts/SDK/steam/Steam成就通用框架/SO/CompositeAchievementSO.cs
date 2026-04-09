using UnityEngine;

namespace SteamSDK.SO
{
    [CreateAssetMenu(fileName = "CompositeAchievementSO", menuName = "创建Steam成就SO/组合成就", order = 2)]
    public class CompositeAchievementSO : SteamAchievementDataSO
    {
        [Tooltip("第一个需要完成的成就")]
        public SteamAchievementDataSO firstAchievement;
        
        [Tooltip("第二个需要完成的成就")]
        public SteamAchievementDataSO secondAchievement;
        
        public override bool OnCheck()
        {
            if (achievementData.IsAchieved)
            {
                return false;
            }
            
            // 检查两个成就是否都已完成
            bool isFirstAchieved = firstAchievement != null && firstAchievement.AchievementData.IsAchieved;
            bool isSecondAchieved = secondAchievement != null && secondAchievement.AchievementData.IsAchieved;
            
            if (isFirstAchieved && isSecondAchieved)
            {
                OnTriggerAchievement();
                return true;
            }
            
            return false;
        }
        
        public override float GetRunTimeValue()
        {
            // 计算两个成就的完成情况，返回0-2之间的值
            int completedCount = 0;
            if (firstAchievement != null && firstAchievement.AchievementData.IsAchieved)
            {
                completedCount++;
            }
            if (secondAchievement != null && secondAchievement.AchievementData.IsAchieved)
            {
                completedCount++;
            }
            return completedCount;
        }
    }
}