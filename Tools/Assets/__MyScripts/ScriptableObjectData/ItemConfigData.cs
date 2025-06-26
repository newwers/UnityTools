using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "ItemData", menuName = "桌面气球配置/物品/创建物体数据配置")]
public class ItemConfigData : ScriptableObject
{
    [FieldReadOnly]
    public string id;

    [Header("描述")]
    [Tooltip("名称")]
    public string Name = "New Item";

    [TextArea(1, 3)]
    [Tooltip("描述")]
    public string description;


    [Tooltip("品质")]
    public EQuality quality = EQuality.Common;


    [Tooltip("图标")]
    public Sprite icon;

    public ItemController prefab; // 物体预制体引用

    [Tooltip("爆炸特效")]
    public GameObject popEffect;        // 爆炸特效


    [Header("重量设置")]
    [Range(0f, 200f)]
    public float mass = 5.0f; // 质量大小
    [Header("最大浮力点数")]
    public int maxFloatPoint = 10; // 最大浮力点数
    [Header("增加1点浮力点数所需时间间隔/秒")]
    public float FloatPointAddInterval = 10f;//增加1点浮力点数所需时间间隔/秒


    [Header("购买价格")]
    public int basePrice = 10;

    [Header("爆炸价格")]
    public int explodePrice = 5;

    [Header("售卖价格")]
    public int sellPrice = 5; // 售卖价格

    [HideInInspector]
    public bool isNoValue = false; // 是否不需要价值


    [Header("生成配置")]
    [Tooltip("是否可在游戏中生成")]
    public bool canBeGenerated = true;

    [Header("是否解锁气球")]
    [NonSerialized]
    public bool isUnLock = false; // 是否解锁

#if UNITY_EDITOR
    private void OnValidate()
    {
        // 检查是否有重复的 ID
        ItemConfigData[] allConfigs = Resources.FindObjectsOfTypeAll<ItemConfigData>();
        foreach (ItemConfigData config in allConfigs)
        {
            if (config != this && config.id == this.id)
            {
                // 如果有重复，生成新的 ID
                this.id = Guid.NewGuid().ToString();
                UnityEditor.EditorUtility.SetDirty(this);
                UnityEditor.AssetDatabase.SaveAssets();
                break;
            }
        }

        if (string.IsNullOrEmpty(id))
        {
            // 自动生成唯一 ID
            id = Guid.NewGuid().ToString();
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssets();
        }
    }

#endif

    // 计算最终爆炸价格
    public int GetFinalExplodePrice()
    {
        return basePrice + explodePrice;
    }

    public void SetUnLock()
    {
        isUnLock = true;
    }
}


#if UNITY_EDITOR

[CustomEditor(typeof(ItemConfigData))]
public class ItemConfigDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 绘制默认的Inspector界面
        DrawDefaultInspector();

        // 获取当前编辑的ItemConfigData实例
        ItemConfigData itemData = (ItemConfigData)target;

        // 显示图标预览
        ShowSpritePreview(itemData.icon, "Item Icon Preview");
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