using System.Collections.Generic;
using UnityEngine;

public static class SpriteUtil
{
    /// <summary>
    /// 设置spriteRenderer中使用SpriteProgressshader的材质的进度值
    /// </summary>
    /// <param name="sprite"></param>
    /// <param name="progress"></param>
    public static void SetSpriteProgress(SpriteRenderer sprite, float progress)
    {
        if (sprite == null) return;

        // 获取当前属性块
        MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
        sprite.GetPropertyBlock(propertyBlock);

        // 设置新颜色
        propertyBlock.SetFloat("_MaskProgress", Mathf.Clamp01(progress));

        // 应用属性块
        sprite.SetPropertyBlock(propertyBlock);
    }

    /// <summary>
    /// 忽略两个碰撞
    /// </summary>
    /// <param name="selfCollider"></param>
    /// <param name="otherCollider"></param>
    /// <param name="isIgnore"></param>
    public static void IgnoreCollision(Collider2D selfCollider, Collider2D otherCollider, bool isIgnore)
    {
        Physics2D.IgnoreCollision(selfCollider, otherCollider, isIgnore);
    }

    /// <summary>
    /// 根据SpriteRenderer调整碰撞体
    /// </summary>
    /// <param name="spriteBounds">SpriteRenderer.sprite.bound</param>
    /// <param name="boxCollider"></param>
    public static void AdjustBoxCollider(Bounds spriteBounds, BoxCollider2D boxCollider)
    {
        if (boxCollider == null)
            return;
        boxCollider.size = spriteBounds.size;
        boxCollider.offset = spriteBounds.center;

        //boxCollider.size = new Vector2(
        //    spriteBounds.size.x,
        //    spriteBounds.size.y
        //);
        //boxCollider.offset = new Vector2(
        //    spriteBounds.center.x,
        //    spriteBounds.center.y
        //);
    }

    public static void AdjustPolygonCollider(SpriteRenderer targetSpriteRenderer, PolygonCollider2D polygonCollider)
    {
        if (polygonCollider == null)
            return;

        polygonCollider.pathCount = targetSpriteRenderer.sprite.GetPhysicsShapeCount();

        List<Vector2> path = new List<Vector2>();
        for (int i = 0; i < polygonCollider.pathCount; i++)
        {
            path.Clear();
            targetSpriteRenderer.sprite.GetPhysicsShape(i, path);
            polygonCollider.SetPath(i, path.ToArray());
        }
    }

    public static void AdjustCircleCollider(Bounds spriteBounds, CircleCollider2D circleCollider)
    {
        if (circleCollider == null)
            return;

        // 使用边界大小的平均值作为半径基础
        float baseRadius = Mathf.Max(spriteBounds.size.x, spriteBounds.size.y) * 0.5f;
        circleCollider.radius = baseRadius /** circleRadiusMultiplier*/;
        circleCollider.offset = new Vector2(
            spriteBounds.center.x,
            spriteBounds.center.y
        );
    }
}
