using UnityEngine;
[CreateAssetMenu(fileName = "New Jump Action Data", menuName = "Character System/Jump Action Data")]
public class JumpActionData : ActionData
{
    [Header("跳跃设置")]
    public float jumpForce = 10f;
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;
    public float coyoteTime = 0.1f;

    [Header("地面检测")]
    public LayerMask groundLayer;
    public Vector2 groundCheckSize = new Vector2(1f, 0.1f);
    public Vector2 groundCheckOffset = new Vector2(0f, -0.05f);
}
