using UnityEngine;

namespace SteamSDK.SO
{
    [CreateAssetMenu(fileName = "UnlockMushroomsAchievementSO", menuName = "创建Steam成就SO/解锁X款蘑菇", order = 2)]
    public class UnlockMushroomsAchievementSO : SteamAchievementDataSO
    {
        public override bool OnCheck()
        {
            if (achievementData.IsAchieved)
            {
                return false;
            }

            // 检测是否解锁了10款蘑菇
            int unlockedMushroomsCount = GetUnlockedMushroomsCount();
            if (requiredNum < 0)//收集所有蘑菇的成就，requiredNum为-1
            {
                if (unlockedMushroomsCount >= GameManager.Instance.mushroomTotalCount)
                {
                    return true;
                }
            }
            else
            {
                if (unlockedMushroomsCount >= requiredNum)
                {
                    OnTriggerAchievement();
                    return true;
                }
            }



            return false;
        }

        public override float GetRunTimeValue()
        {
            return GetUnlockedMushroomsCount();
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
                return GameManager.Instance.currentUnlockedMushroom;
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