using UnityEngine;

[CreateAssetMenu(fileName = "New Dash Action Data", menuName = "Character System/Character Action/Dash Action Data")]
public class DashActionData : ActionData
{
    [Header("冲刺设置")]
    public float dashSpeed = 10f;
    public float dashDuration = 0.3f;
    public float dashCooldown = 0.5f;

    [Header("闪避设置")]
    [Tooltip("是否消耗闪避次数")]
    public bool consumeDodgeCount = true;
    [Tooltip("闪避无敌开始时间(相对于闪避开始的时间)")]
    public float invincibleStartTime = 0f;
    [Tooltip("闪避无敌持续时间")]
    public float invincibleDuration = 0.3f;
}
