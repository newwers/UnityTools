using UnityEngine;

namespace SteamSDK.SO
{
    [CreateAssetMenu(fileName = "LibraryMushroomCountAchievementSO", menuName = "创建Steam成就SO/库存内有X个蘑菇", order = 2)]
    public class LibraryMushroomCountAchievementSO : SteamAchievementDataSO
    {
        public override bool OnCheck()
        {
            if (achievementData.IsAchieved)
            {
                return false;
            }
            
            // 更新库存蘑菇数量
            GameManager.Instance.GetCurrentLibraryMushroomNums();
            
            // 检查库存蘑菇数量是否达到要求
            int count = GameManager.Instance.currentLibraryMushroomTotalNums;
            if (count >= requiredNum)
            {
                OnTriggerAchievement();
                return true;
            }
            
            return false;
        }
        
        public override float GetRunTimeValue()
        {
            // 更新库存蘑菇数量
            GameManager.Instance.GetCurrentLibraryMushroomNums();
            
            // 返回当前库存蘑菇数量
            return GameManager.Instance.currentLibraryMushroomTotalNums;
        }
    }
}