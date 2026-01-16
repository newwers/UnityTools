using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 召唤物数据
/// 定义召唤物的基础属性和配置，与EnemyConfigData兼容
/// </summary>
[CreateAssetMenu(fileName = "New Summon", menuName = "Character System/Summon/SummonData")]
public class SummonData : ScriptableObject
{
    [Header("基础设置")]
    [Tooltip("召唤物名称")]
    public string summonName;

    [Tooltip("召唤物预制体")]
    public GameObject summonPrefab;

    [Tooltip("召唤物生命周期（秒），0表示永久存在")]
    public float lifetime = 30f;

    [Tooltip("召唤物AI策略")]
    public BaseAIStrategy aiStrategy;

    [Header("属性设置")]
    [Tooltip("召唤物基础生命值")]
    public float baseHealth = 100f;

    [Tooltip("召唤物基础攻击力")]
    public int baseAttack = 20;

    [Tooltip("召唤物基础防御")]
    public int baseDefense = 10;

    [Header("AI移动设置")]
    [Tooltip("召唤物移动速度")]
    public float moveSpeed = 3f;
    
    [Tooltip("巡逻速度")]
    public float patrolSpeed = 2f;
    
    [Tooltip("追击速度")]
    public float chaseSpeed = 4f;
    
    [Tooltip("巡逻时间")]
    public float patrolDuration = 2f;
    
    [Tooltip("空闲时间最小值")]
    public float idleTimeMin = 1f;
    
    [Tooltip("空闲时间最大值")]
    public float idleTimeMax = 3f;

    [Header("攻击设置")]
    [Tooltip("攻击范围")]
    public float attackRange = 2f;
    
    [Tooltip("攻击冷却时间")]
    public float attackCooldown = 2f;
    
    [Tooltip("丢失目标距离")]
    public float loseTargetDistance = 12f;
    
    [Tooltip("召唤物攻击动作数据")]
    public List<AttackActionData> attackActions = new List<AttackActionData>();

    [Tooltip("召唤物技能数据")]
    public SkillData[] skills;

    [Header("行为设置")]
    [Tooltip("是否跟随召唤者")]
    public bool followSummoner = true;

    [Tooltip("跟随距离")]
    public float followDistance = 5f;

    [Tooltip("警戒范围")]
    public float alertRange = 10f;

    [Tooltip("是否自动攻击敌人")]
    public bool autoAttack = true;
    
    [Tooltip("撤退生命阈值,百分比")]
    public float retreatHealthThreshold = 0;
    
    [Header("闪避设置")]
    [Tooltip("闪避生命阈值")]
    public float dodgeHealthThreshold = 0.3f;
    
    [Tooltip("闪避冷却时间")]
    public float dodgeCooldown = 2f;
    
    [Tooltip("闪避持续时间")]
    public float dodgeDuration = 0.5f;
    
    [Tooltip("闪避概率")]
    public float dodgeProbability = 0.5f;
    
    [Header("恢复技能设置")]
    [Tooltip("恢复技能生命阈值")]
    public float recoverySkillHealthThreshold = 0.2f;
    
    [Tooltip("恢复技能冷却时间")]
    public float recoverySkillCooldown = 10f;
    
    [Tooltip("恢复技能概率")]
    public float recoverySkillProbability = 0.5f;
    
    [Tooltip("恢复技能动作")]
    public List<AttackActionData> recoverySkillActions = new List<AttackActionData>();

    [Header("召唤物类型标签")]
    public List<string> summonTags = new List<string>();

    [Header("视觉效果")]
    [Tooltip("召唤出场特效")]
    public GameObject summonEffectPrefab;

    [Tooltip("召唤出场音效")]
    public AudioClip summonSound;

    [Tooltip("死亡特效")]
    public GameObject deathEffectPrefab;

    [Tooltip("死亡音效")]
    public AudioClip deathSound;
}