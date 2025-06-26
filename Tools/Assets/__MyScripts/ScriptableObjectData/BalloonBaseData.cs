using System;
using System.Collections.Generic;
using UnityEngine;


#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "BalloonBaseData", menuName = "桌面气球配置/气球/创建气球数据配置")]
public class BalloonBaseData : ScriptableObject
{
    [Serializable]
    public class BalloonLevelConfig
    {
        public int level; // 气球等级
        public List<Sprite> sprites; // 气球图标列表
        public int NeedExp; // 升级所需经验
    }


    [FieldReadOnly]
    public string id;


    [Header("是否可在游戏中生成")]
    public bool canBeGenerated = true;

    [Header("基础属性")]
    [Tooltip("气球品质")]
    public EQuality quality = EQuality.Common;

    [Tooltip("气球图标")]
    public Sprite icon;
    [Tooltip("气球预制体")]
    public BalloonController prefab;        // 气球预制体
    [Tooltip("爆炸特效")]
    public GameObject popEffect;        // 爆炸特效

    [Header("气球描述")]
    [Tooltip("气球名称")]
    public string balloonName = "New Balloon";

    [TextArea(1, 3)]
    [Tooltip("气球描述")]
    public string description;

    public List<BalloonLevelConfig> levelDataList = new List<BalloonLevelConfig>(); // 气球等级数据列表


    [Header("购买价格")]
    public int basePrice = 10;

    [Header("爆炸掉落金币")]
    public int baseExplodePrice = 5;



    [Header("重量设置")]
    [Tooltip("升力大小")]
    [Range(0f, 200f)]
    public float liftForce = 20.0f; // 升力大小
    public float stayTimeAtTop = 300f; // 5分钟 = 300秒




    public int maxLevel
    {
        get
        {
            if (levelDataList == null || levelDataList.Count == 0)
            {
                Debug.LogWarning("气球等级数据列表为空或未设置");
                return 1; // 默认返回1级
            }
            return levelDataList[levelDataList.Count - 1].level; // 返回最后一个等级
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

    public Sprite GetIconIndexSprite(int level, int iconIndex)
    {
        if (levelDataList == null || levelDataList.Count == 0)
        {
            Debug.LogWarning("气球等级数据列表为空或未设置");
            return null;
        }

        foreach (var config in levelDataList)
        {
            if (config.level == level && config.sprites != null && config.sprites.Count > 0)
            {
                return config.sprites[iconIndex];
            }
        }
        Debug.LogWarning($"未找到等级为 {level} 的气球图标索引:{iconIndex}，使用默认图标");
        return null; // 如果没有找到对应等级的图标，返回null
    }



    public int GetRandomIconIndex(int level)
    {
        foreach (var config in levelDataList)
        {
            if (config.level == level && config.sprites != null && config.sprites.Count > 0)
            {
                return UnityEngine.Random.Range(0, config.sprites.Count);
            }
        }

        Debug.LogWarning($"未找到等级为 {level} 的气球图标，无法获取随机索引");
        return 0;
    }

    public int GetExpForNextLevel(int level)
    {
        if (levelDataList == null || levelDataList.Count == 0)
        {
            Debug.LogWarning("气球等级数据列表为空或未设置");
            return 0;
        }
        foreach (var config in levelDataList)
        {
            if (config.level == level)
            {
                return config.NeedExp;
            }
        }
        Debug.LogWarning($"未找到等级为 {level} 的气球经验数据");
        return 0; // 如果没有找到对应等级，返回0
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(BalloonBaseData))]
public class BalloonBaseDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 绘制默认的Inspector界面
        DrawDefaultInspector();

        // 获取当前编辑的BalloonBaseData实例
        BalloonBaseData balloonData = (BalloonBaseData)target;

        // 显示主图标预览
        ShowSpritePreview(balloonData.icon, "Main Icon Preview");

        // 显示每个等级的图标预览
        for (int i = 0; i < balloonData.levelDataList.Count; i++)
        {
            BalloonBaseData.BalloonLevelConfig levelConfig = balloonData.levelDataList[i];
            for (int j = 0; j < levelConfig.sprites.Count; j++)
            {
                Sprite sprite = levelConfig.sprites[j];
                string previewTitle = $"Level {levelConfig.level} Sprite {j + 1} Preview";
                ShowSpritePreview(sprite, previewTitle);
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

            // // 计算UV坐标
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