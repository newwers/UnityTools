using SteamSDK;
using UnityEngine;

[CreateAssetMenu(fileName = "UnlockOrnamentAchievementSO", menuName = "创建Steam成就SO/解锁X个摆件")]
public class UnlockOrnamentAchievementSO : SteamAchievementDataSO
{


    public override bool OnCheck()
    {
        if (achievementData.IsAchieved)
        {
            return false;
        }

        int count = GetUnlockedCount();
        if (requiredNum < 0)//收集所有摆件的成就，requiredNum为-1
        {
            if (count >= GameManager.Instance.ornamentTotalCount)
            {
                return true;
            }
        }
        else
        {
            if (count >= requiredNum)
            {
                OnTriggerAchievement();
                return true;
            }
        }



        return false;
    }

    public override float GetRunTimeValue()
    {
        return GetUnlockedCount();
    }

    /// <summary>
    /// 获取已解锁的摆件数量
    /// </summary>
    /// <returns>已解锁的摆件数量</returns>
    private int GetUnlockedCount()
    {
        try
        {
            int count = 0;
            var ornamentDatas = SaveManager.Instance.gameData.OrnamentDatasInLibrary;
            foreach (var data in ornamentDatas)
            {
                if (data.Value.isBought)
                {
                    count++;
                }
            }
            return count;
        }
        catch (System.Exception e)
        {
            // 如果GameManager不存在或方法未实现，记录错误并返回0
            Debug.LogWarning($"获取已解锁摆件数量失败: {e.Message}");
        }
        return 0;
    }
}
