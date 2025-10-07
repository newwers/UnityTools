using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum EItemSize
{
    Small,      // 小型物品
    //Medium,     // 中型物品
    Large       // 大型物品
}

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

    public EItemSize itemSize = EItemSize.Small; // 物品大小



    [Tooltip("默认图标(取自图标列表第一张)")]
    public Sprite icon
    {
        get { return icons.Count > 0 ? icons[0] : null; }
    }


    [Tooltip("图标列表(不同颜色)")]
    public List<Sprite> icons = new List<Sprite>();

    public ItemController prefab;

    [Tooltip("爆炸特效")]
    public GameObject popEffect;        // 爆炸特效


    [Header("重量设置")]
    [Range(0f, 5000f)]
    public float mass = 5.0f; // 质量大小
    [Space(10)]
    [Header("漂浮点上限")]
    public int floatPointsLimit = 100;
    [Header("每秒积累的漂浮点")]
    public float floatPointsPerSecond = 10f;
    [Header("每几秒积累漂浮点")]
    public float floatPointsAddInterval = 2f;
    [Space(10)]
    [Header("从礼盒弹出时力大小")]
    public Vector2 ejectForce = new Vector2(20, 40); // 从礼盒弹出时力大小


    [Header("购买价格")]
    public int basePrice = 10;

    [Header("爆炸价格")]
    public int explodePrice = 5;

    [Header("售卖价格")]
    public int sellPrice = 5; // 售卖价格


    // 在现有变量声明处添加以下参数
    [Header("碰撞音效设置")]
    public AudioClip collisionAudio; // 拖拽音效到这里
    public float minImpactForce = 1f; // 最小触发力度阈值
    public float maxImpactForce = 10f; // 最大音量对应的力度
    public float volumeMultiplier = 1f; // 音量乘数



    [Header("生成配置")]
    [Tooltip("是否可在游戏中生成")]
    public bool canBeGenerated = true;


    public bool isUnLock//物体的解锁状态从GameManager获取
    {
        get
        {
            return GameManager.Instance.GetItemUnlockData(id).isUnLock;
        }
    }

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


    // 获取随机图标
    public Tuple<int, Sprite> GetRandomIcon()
    {
        if (icons == null || icons.Count == 0)
            return Tuple.Create(-1, (Sprite)null);

        int randomIndex = UnityEngine.Random.Range(0, icons.Count);
        return Tuple.Create(randomIndex, icons[randomIndex]);
    }

    public Sprite GetIcon(int index)
    {
        if (index < 0 || index >= icons.Count)
        {
            Debug.LogWarning($"Index {index} is out of range for icons list.");
            return null;
        }
        return icons[index];
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(ItemConfigData))]
[CanEditMultipleObjects]
public class ItemConfigDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 绘制默认的Inspector界面
        DrawDefaultInspector();

        // 获取当前编辑的ItemConfigData实例
        ItemConfigData itemData = (ItemConfigData)target;

        // 显示图标预览
        if (itemData.icons != null)
        {
            for (int i = 0; i < itemData.icons.Count; i++)
            {
                ShowSpritePreview(itemData.icons[i], "预览图");
            }

        }


        // 显示随机图标预览按钮
        if (GUILayout.Button("预览随机图标"))
        {
            Sprite randomIcon = itemData.GetRandomIcon().Item2;
            ShowSpritePreview(randomIcon, "Random Icon Preview");
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
        else
        {
            EditorGUILayout.LabelField($"{title}: 无图标");
        }
    }
}
#endif