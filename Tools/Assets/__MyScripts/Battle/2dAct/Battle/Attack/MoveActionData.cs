using UnityEngine;
[CreateAssetMenu(fileName = "New Move Action Data", menuName = "Character System/Character Action/Move Action Data")]
public class MoveActionData : ActionData
{
    /// <summary>
    /// 基础移动速度
    /// 计算最终速度：基础速度 + 属性速度加成，再乘以系数
    /// </summary>
    [Header("移动设置")]
    public float moveSpeed = 3f;
    /// <summary>
    /// 加速度
    /// </summary>
    public float acceleration = 15f;
    /// <summary>
    /// 减速率
    /// </summary>
    public float deceleration = 20f;
}
