using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum EGiftPackType { Balloon, Item }

[CreateAssetMenu(fileName = "NewGiftPack", menuName = "桌面气球配置/礼包/创建礼包配置")]
public class GiftPack : ScriptableObject
{
    [Header("礼包基础设置")]
    public string packName = "新礼包";
    public string description;
    public Sprite icon;
    public EGiftPackType packType;
    public int purchasePrice = 100; // 购买价格

    [Header("气球礼包内容")]
    public List<BalloonBaseData> balloonContents = new List<BalloonBaseData>();

    [Header("物品礼包内容")]
    public List<ItemConfigData> itemContents = new List<ItemConfigData>();

    // 随机获取礼包内容
    public ScriptableObject GetRandomContent()
    {
        switch (packType)
        {
            case EGiftPackType.Balloon:
                if (balloonContents.Count == 0) return null;
                return balloonContents[Random.Range(0, balloonContents.Count)];

            case EGiftPackType.Item:
                if (itemContents.Count == 0) return null;
                return itemContents[Random.Range(0, itemContents.Count)];

            default:
                return null;
        }
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(GiftPack))]
public class GiftPackEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 绘制默认的Inspector界面
        DrawDefaultInspector();

        // 获取当前编辑的GiftPack实例
        GiftPack giftPack = (GiftPack)target;

        // 显示礼包图标的预览
        ShowSpritePreview(giftPack.icon, "Gift Pack Icon Preview");

        // 显示气球礼包内容中每个气球的图标预览
        for (int i = 0; i < giftPack.balloonContents.Count; i++)
        {
            BalloonBaseData balloonData = giftPack.balloonContents[i];
            if (balloonData != null)
            {
                ShowSpritePreview(balloonData.icon, $"Balloon {i + 1} Icon Preview");
            }
        }

        // 显示物品礼包内容中每个物品的图标预览（假设ItemConfigData有icon字段）
        for (int i = 0; i < giftPack.itemContents.Count; i++)
        {
            ItemConfigData itemData = giftPack.itemContents[i];
            if (itemData != null && itemData.icon != null)
            {
                ShowSpritePreview(itemData.icon, $"Item {i + 1} Icon Preview");
            }
        }
    }

    private void ShowSpritePreview(Sprite sprite, string title)
    {
        if (sprite != null)
        {
            // 开始垂直布局
            EditorGUILayout.BeginVertical();

            // 绘制一个水平分隔线
            GUILayout.Space(10);
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            GUILayout.Space(5);

            // 绘制一个纹理预览窗口
            Rect previewRect = GUILayoutUtility.GetRect(sprite.rect.width, sprite.rect.height);

            // 计算原始宽高比
            float spriteRatio = sprite.rect.width / sprite.rect.height;
            float containerRatio = previewRect.width / previewRect.height;

            Vector2 drawSize;
            if (containerRatio > spriteRatio)
            {
                drawSize = new Vector2(previewRect.height * spriteRatio, previewRect.height);
            }
            else
            {
                drawSize = new Vector2(previewRect.width, previewRect.width / spriteRatio);
            }

            // 居中绘制位置
            Vector2 drawPos = new Vector2(
                previewRect.x + (previewRect.width - drawSize.x) / 2,
                previewRect.y + (previewRect.height - drawSize.y) / 2
            );

            // 计算UV坐标
            Rect spriteRect = new Rect(
                sprite.rect.x / sprite.texture.width,
                sprite.rect.y / sprite.texture.height,
                sprite.rect.width / sprite.texture.width,
                sprite.rect.height / sprite.texture.height
            );

            // 保持比例绘制Sprite
            GUI.DrawTextureWithTexCoords(
                new Rect(drawPos.x, drawPos.y, drawSize.x, drawSize.y),
                sprite.texture,
                spriteRect
            );

            // 结束垂直布局
            EditorGUILayout.EndVertical();
        }
    }
}
#endif