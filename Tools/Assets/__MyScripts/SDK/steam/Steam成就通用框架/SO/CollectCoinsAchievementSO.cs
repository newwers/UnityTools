using SteamSDK;
using UnityEngine;

[CreateAssetMenu(fileName = "CollectCoinsAchievementSO", menuName = "创建Steam成就SO/收集X个孢子")]
public class CollectCoinsAchievementSO : SteamAchievementDataSO
{

    public override bool OnCheck()
    {
        if (achievementData.IsAchieved)
        {
            return false;
        }

        // 检测是否解锁
        int count = GetCoins();
        if (count >= requiredNum)
        {
            OnTriggerAchievement();
            return true;
        }

        return false;
    }

    override public float GetRunTimeValue()
    {
        return GetCoins();//todo:这边要获取累计的,而不是当前的
    }

    public int GetCoins()
    {
        return SaveManager.Instance.gameData.totalCoinsCollected;
    }
}