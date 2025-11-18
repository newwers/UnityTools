using UnityEngine;
[CreateAssetMenu(fileName = "New Block Action Data", menuName = "Character System/Block Action Data")]
public class BlockActionData : ActionData
{
    // 格挡和弹反相关字段
    [Header("格挡设置")]
    public float blockDamageReduction = 0.7f; // 格挡伤害减免
    public float parryDamageMultiplier = 1.5f; // 弹反伤害加成
    public float parryWindow = 0.2f; // 弹反输入窗口
    public float parryStunDuration = 1.0f; // 弹反成功时敌人的硬直时间
}
