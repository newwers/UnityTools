using UnityEngine;

namespace SteamSDK.SO
{
    [CreateAssetMenu(fileName = "PlayTimeAchievementSO", menuName = "创建Steam成就SO/游玩时长", order = 2)]
    public class PlayTimeAchievementSO : SteamAchievementDataSO
    {
        [Header("游玩时长配置")]
        [Tooltip("达成成就所需的游玩时长（秒）")]
        public int requiredPlayTimeSeconds = 100; // 单位秒

        public override bool OnCheck()
        {
            if (achievementData.IsAchieved)
            {
                return false;
            }

            // 检测是否达到了所需的游玩时长
            float totalPlayTimeSeconds = GetTotalPlayTimeSeconds();
            achievementData.FloatValue = totalPlayTimeSeconds; // 更新当前游玩时长到成就数据中
            if (totalPlayTimeSeconds >= requiredPlayTimeSeconds)
            {
                OnTriggerAchievement();
                return true;
            }

            return false;
        }

        /// <summary>
        /// 获取总游玩时长（秒）
        /// </summary>
        /// <returns>总游玩时长（秒）</returns>
        private float GetTotalPlayTimeSeconds()
        {
            // 尝试通过GameManager获取总游玩时长
            try
            {
                if (GameManager.Instance != null)
                {
                    // 假设GameManager中有获取总游玩时长的方法
                    // 这里需要根据实际项目中的游玩时长计算逻辑进行调整
                    return GameManager.Instance.GetCurrentGameTime();
                }
            }
            catch (System.Exception e)
            {
                // 如果GameManager不存在或方法未实现，记录错误并返回0
                Debug.LogWarning($"获取总游玩时长失败: {e.Message}");
            }
            return 0;
        }
    }
}