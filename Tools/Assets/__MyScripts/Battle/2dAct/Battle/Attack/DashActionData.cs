using UnityEngine;
[CreateAssetMenu(fileName = "New Dash Action Data", menuName = "Character System/Dash Action Data")]
public class DashActionData : ActionData
{
    [Header("冲刺设置")]
    public float dashSpeed = 10f;
    public float dashDuration = 0.3f;
    public float dashCooldown = 0.5f;
}
