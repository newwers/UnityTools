using UnityEngine;

[CreateAssetMenu(fileName = "New Jump Action Data", menuName = "Character System/Character Action/Jump Action Data")]
public class JumpActionData : ActionData
{
    [Header("跳跃设置")]
    public float jumpForce = 10f;
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;
    public float coyoteTime = 0.1f;

    [Header("多段跳跃设置")]
    [Tooltip("基础最大跳跃次数，1表示只能跳一次（单跳），2表示可以二段跳")]
    public int baseMaxAirJumps = 0;
    
    [Tooltip("每次空中跳跃的力度系数，相对于jumpForce的百分比")]
    [Range(0f, 1.5f)]
    public float airJumpForceMultiplier = 0.85f;

    [Header("地面检测")]
    public LayerMask groundLayer;
    public Vector2 groundCheckSize = new Vector2(1f, 0.1f);
    public Vector2 groundCheckOffset = new Vector2(0f, -0.05f);
}
