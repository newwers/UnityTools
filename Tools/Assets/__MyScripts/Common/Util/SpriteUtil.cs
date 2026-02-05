using System.Collections.Generic;
using UnityEngine;

public static class SpriteUtil
{
    public static Texture2D GetSpriteTexture(Sprite sprite)
    {
        if (sprite == null)
            return null;

        // 如果sprite的纹理已经是单独的了，可能不需要提取，但通常图集里的sprite.texture返回整个图集
        // 获取sprite在原始纹理中的像素区域
        Rect rect = sprite.rect;
        Texture2D originalTexture = sprite.texture;

        // 创建一个新的纹理，大小为sprite的宽高
        Texture2D newTexture = new Texture2D((int)rect.width, (int)rect.height);

        // 获取原始纹理中sprite区域的像素
        Color[] pixels = originalTexture.GetPixels((int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height);

        // 设置到新纹理
        newTexture.SetPixels(pixels);
        newTexture.Apply();

        return newTexture;
    }

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
    /// <param name="sprite"></param>
    /// <param name="boxCollider"></param>
    public static void AdjustBoxCollider(SpriteRenderer sprite, BoxCollider2D boxCollider)
    {
        if (boxCollider == null || sprite == null)
            return;

        // 获取精灵的原始边界
        Bounds spriteLocalBounds = sprite.sprite.bounds;

        // 将本地边界转换到世界空间，然后再转换回碰撞器所在的本地空间
        Transform colliderTransform = boxCollider.transform;

        // 计算世界空间中的边界
        Vector3 worldSize = Vector3.Scale(spriteLocalBounds.size, sprite.transform.lossyScale);
        Vector3 worldCenter = sprite.transform.TransformPoint(spriteLocalBounds.center);

        // 转换到碰撞器的本地空间
        Vector3 localSize = colliderTransform.InverseTransformVector(worldSize);
        Vector3 localCenter = colliderTransform.InverseTransformPoint(worldCenter);

        boxCollider.size = new Vector2(Mathf.Abs(localSize.x), Mathf.Abs(localSize.y));
        boxCollider.offset = new Vector2(localCenter.x, localCenter.y);
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

    public static void AdjustCircleCollider(SpriteRenderer sprite, CircleCollider2D circleCollider)
    {
        if (circleCollider == null)
            return;

        Bounds spriteBounds = sprite.bounds;

        // 使用边界大小的平均值作为半径基础
        float baseRadius = Mathf.Max(spriteBounds.size.x, spriteBounds.size.y) * 0.5f;
        circleCollider.radius = baseRadius /** circleRadiusMultiplier*/;
        circleCollider.offset = new Vector2(
            spriteBounds.center.x,
            spriteBounds.center.y
        );
    }
}
