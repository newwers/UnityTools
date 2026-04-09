using UnityEngine;

namespace SteamSDK.SO
{
    [CreateAssetMenu(fileName = "OrnamentMushroomCountAchievementSO", menuName = "创建Steam成就SO/在摆件上放置3颗蘑菇", order = 2)]
    public class OrnamentMushroomCountAchievementSO : SteamAchievementDataSO
    {
        public override bool OnCheck()
        {
            if (achievementData.IsAchieved)
            {
                return false;
            }

            // 检查所有摆件上的蘑菇数量
            if (HasOrnamentWithThreeMushrooms())
            {
                OnTriggerAchievement();
                return true;
            }

            return false;
        }

        public override float GetRunTimeValue()
        {
            // 返回最大的摆件蘑菇数量
            return GetMaxMushroomsOnOrnament();
        }

        /// <summary>
        /// 检查是否有任何摆件上有3颗或更多蘑菇
        /// </summary>
        /// <returns>是否有摆件上有3颗或更多蘑菇</returns>
        private bool HasOrnamentWithThreeMushrooms()
        {
            try
            {
                foreach (var ornament in Garden.Instance.currentOrnaments.Values)
                {
                    if (ornament != null && ornament.mushRoomsOnOrnament != null && ornament.mushRoomsOnOrnament.Count >= requiredNum)
                    {
                        return true;
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"检查摆件蘑菇数量失败: {e.Message}");
            }
            return false;
        }

        /// <summary>
        /// 获取所有摆件上蘑菇数量的最大值
        /// </summary>
        /// <returns>最大的摆件蘑菇数量</returns>
        private float GetMaxMushroomsOnOrnament()
        {
            int maxCount = 0;
            try
            {
                foreach (var ornament in Garden.Instance.currentOrnaments.Values)
                {
                    if (ornament != null && ornament.mushRoomsOnOrnament != null && ornament.mushRoomsOnOrnament.Count > maxCount)
                    {
                        maxCount = ornament.mushRoomsOnOrnament.Count;
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"获取摆件蘑菇数量失败: {e.Message}");
            }
            return maxCount;
        }
    }
}