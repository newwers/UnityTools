using System.Collections.Generic;
using UnityEngine;

public enum DetectRangeType
{
    [InspectorName("圆形")]
    Circle,
    [InspectorName("矩形")]
    Rectangle
}

[CreateAssetMenu(fileName = "New Enemy Config", menuName = "Character System/Enemy Config Data")]
public class EnemyConfigData : ScriptableObject
{
    [Header("基础设置")]
    public string enemyName;
    public EnemyDifficulty difficulty = EnemyDifficulty.Normal;

    [Header("AI策略")]
    [Tooltip("AI策略配置,留空则根据难度自动创建")]
    public BaseAIStrategy aiStrategy;

    [Header("角色属性")]
    public CharacterAttributes attributes;

    [Header("巡逻设置")]
    public PatrolArea patrolArea = new PatrolArea();
    public float idleTimeMin = 1f;
    public float idleTimeMax = 3f;
    public float patrolSpeed = 2f;
    public float patrolDuration = 2f;// 巡逻时间

    //[Header("检测设置(暂时没用,用视觉检测代替,后面可以将视觉检测参数提取到配置中)")]
    //public DetectRangeType detectRangeType = DetectRangeType.Circle;
    //[Tooltip("圆形检测时的半径")]
    //public float detectRange = 8f;
    //[Tooltip("矩形检测时的宽度")]
    //public float detectWidth = 10f;
    //[Tooltip("矩形检测时的高度")]
    //public float detectHeight = 6f;
    //public float loseTargetRange = 12f;//丢失目标距离 丢失目标距离其实就是视觉距离
    //public LayerMask targetLayers;

    [Header("追击设置")]
    public float chaseSpeed = 4f;
    public float loseTargetDistance = 12f;
    /// <summary>
    /// 撤退生命阈值,百分比
    /// </summary>
    public float retreatHealthThreshold = 0; // 低于该生命值百分比时可以撤退，0表示不撤退

    [Header("攻击设置")]
    public List<AttackActionData> attackActions = new List<AttackActionData>();
    public float attackRange = 2f;
    public float attackCooldown = 2f;

    [Header("精英特殊设置")]
    [Tooltip("精英敌人额外的攻击方式")]
    public List<AttackActionData> eliteAttackActions = new List<AttackActionData>();

    [Header("Boss设置")]
    [Tooltip("Boss的攻击配置列表")]
    public List<AttackActionData> bossAttackActions = new List<AttackActionData>();
    [Tooltip("Boss的阶段配置列表")]
    public List<BossPhaseConfig> bossPhases = new List<BossPhaseConfig>();

    [Header("闪避设置")]
    [Tooltip("触发闪避的生命值阈值")]
    [Range(0f, 1f)]
    public float dodgeHealthThreshold = 0.3f;
    [Tooltip("闪避冷却时间")]
    public float dodgeCooldown = 3f;
    [Tooltip("闪避持续时间")]
    public float dodgeDuration = 0.3f;
    [Tooltip("基础闪避概率")]
    [Range(0f, 1f)]
    public float dodgeProbability = 0.3f;

    [Header("恢复技能设置")]
    [Tooltip("触发恢复技能的生命值阈值")]
    [Range(0f, 1f)]
    public float recoverySkillHealthThreshold = 0.4f;
    [Tooltip("恢复技能冷却时间")]
    public float recoverySkillCooldown = 10f;
    [Tooltip("基础恢复技能使用概率")]
    [Range(0f, 1f)]
    public float recoverySkillProbability = 0.5f;
    [Tooltip("恢复技能的动作数据列表")]
    public List<AttackActionData> recoverySkillActions = new List<AttackActionData>();

    [Header("视觉效果")]
    public GameObject deathEffect;
    public AudioClip deathSound;

    public List<AttackActionData> GetAllAttacks()
    {
        List<AttackActionData> allAttacks = new List<AttackActionData>(attackActions);

        if (difficulty == EnemyDifficulty.Elite || difficulty == EnemyDifficulty.Boss)
        {
            allAttacks.AddRange(eliteAttackActions);
        }

        return allAttacks;
    }

    public AttackActionData GetRandomAttack()
    {
        List<AttackActionData> availableAttacks = GetAllAttacks();

        if (availableAttacks.Count == 0)
            return null;

        return availableAttacks[Random.Range(0, availableAttacks.Count)];
    }
}

[System.Serializable]
public class BossPhaseConfig
{
    [Tooltip("阶段名称（用于调试）")]
    public string phaseName = "阶段2";

    [Header("阶段触发条件")]
    [Tooltip("当生命值百分比低于此值时触发阶段切换")]
    [Range(0f, 1f)]
    public float healthPercentThreshold = 0.5f;

    [Header("阶段切换效果")]
    public GameObject phaseTransitionEffect;
    public AudioClip phaseTransitionSound;

    [Header("阶段配置")]
    [Tooltip("切换到的敌人配置")]
    public EnemyConfigData phaseConfig;

}
