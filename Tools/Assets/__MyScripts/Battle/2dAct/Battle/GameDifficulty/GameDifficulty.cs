using UnityEngine;

public enum GameDifficulty
{
    [InspectorName("普通")]
    Normal,
    [InspectorName("困难")]
    Hard,
    [InspectorName("地狱")]
    Hell
}

[System.Serializable]
public class DifficultyModifiers
{
    [Header("属性倍率")]
    [Tooltip("生命值倍率")]
    public float healthMultiplier = 1f;
    
    [Tooltip("攻击力倍率")]
    public float attackMultiplier = 1f;
    
    [Tooltip("移动速度倍率")]
    public float moveSpeedMultiplier = 1f;
    
    [Tooltip("攻击速度倍率(冷却时间倍率)")]
    public float attackSpeedMultiplier = 1f;
    
    [Tooltip("防御力倍率")]
    public float defenseMultiplier = 1f;
    
    [Header("AI设置")]
    [Tooltip("检测范围倍率")]
    public float detectRangeMultiplier = 1f;
    
    [Tooltip("AI决策速度倍率")]
    public float aiDecisionSpeedMultiplier = 1f;
    
    [Tooltip("使用AI策略")]
    public bool useEnhancedAI = false;
    
    [Header("经验与掉落")]
    [Tooltip("经验值倍率")]
    public float expMultiplier = 1f;
    
    [Tooltip("掉落倍率")]
    public float lootMultiplier = 1f;
}
