using UnityEngine;

public interface IMoveable
{
    Rigidbody2D RB { get; set; }
    bool IsFacingRight { get; set; }
    void Move(Vector2 velocity);
    void CheckForLeftFacing(Vector2 velocity);//检测面向左边
}
