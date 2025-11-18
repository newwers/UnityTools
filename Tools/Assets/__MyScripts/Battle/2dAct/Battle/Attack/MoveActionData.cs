using UnityEngine;
[CreateAssetMenu(fileName = "New Move Action Data", menuName = "Character System/Move Action Data")]
public class MoveActionData : ActionData
{
    [Header("移动设置")]
    public float moveSpeed = 3f;
    public float acceleration = 15f;
    public float deceleration = 20f;
}
