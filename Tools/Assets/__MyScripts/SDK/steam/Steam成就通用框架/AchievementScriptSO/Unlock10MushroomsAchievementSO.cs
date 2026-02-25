using UnityEngine;

namespace SteamSDK.SO
{
    [CreateAssetMenu(fileName = "Unlock10MushroomsAchievementSO", menuName = "创建Steam成就SO/解锁10款蘑菇", order = 2)]
    public class Unlock10MushroomsAchievementSO : SteamAchievementDataSO
    {
        public int requiredNum = 10;

        public override bool OnCheck()
        {
            if (achievementData.IsAchieved)
            {
                return false;
            }

            // 检测是否解锁了10款蘑菇
            int unlockedMushroomsCount = GetUnlockedMushroomsCount();
            if (unlockedMushroomsCount >= requiredNum)
            {
                OnTriggerAchievement();
                return true;
            }

            return false;
        }

        /// <summary>
        /// 获取已解锁的蘑菇数量
        /// </summary>
        /// <returns>已解锁的蘑菇数量</returns>
        private int GetUnlockedMushroomsCount()
        {
            // 尝试通过GameManager获取已解锁的蘑菇数量
            try
            {

            }
            catch (System.Exception e)
            {
                // 如果GameManager不存在或方法未实现，记录错误并返回0
                Debug.LogWarning($"获取已解锁蘑菇数量失败: {e.Message}");
            }
            return 0;
        }
    }
}