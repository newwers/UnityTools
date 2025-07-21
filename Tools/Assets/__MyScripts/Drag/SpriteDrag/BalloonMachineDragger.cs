using UnityEngine;

public class BalloonMachineDragger : BaseDrag
{
    public Collider2D btnCollider;

    protected override void OnMouseDown()
    {
        base.OnMouseDown();

        //重新计算鼠标位置
        if (btnCollider.OverlapPoint(mouseWorldPos))
        {
            isDragging = false; // 如果点击在按钮上，则不允许拖拽
        }

    }
}
