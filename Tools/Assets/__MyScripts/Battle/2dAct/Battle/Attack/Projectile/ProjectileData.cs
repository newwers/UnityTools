using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 投掷物数据配置（ScriptableObject）
/// 用于配置投掷物的预制体、物理属性、命中检测层级、携带伤害和效果等
/// 该SO用于在AttackFrameData中引用，以在指定攻击帧发射投掷物
/// </summary>
[CreateAssetMenu(fileName = "New Projectile", menuName = "Character System/Projectile/Projectile Data")]
public class ProjectileData : ScriptableObject
{
    [Header("预制体（发射时会实例化或从对象池中取出）")]
    [Tooltip("投掷物的预制体，预制体应包含可视/碰撞组件并可以挂上ProjectileController（可选）")]
    public GameObject prefab;


    [Header("碰撞与命中")]
    [Tooltip("投掷物的命中目标层级掩码")]
    public LayerMask hitLayers;


    [Header("物理与运动")]
    [Tooltip("初始发射速度（单位：单位/秒）")]
    public float initialSpeed = 10f; // 初始速度


    public RigidbodyType2D bodyType = RigidbodyType2D.Kinematic;

    [Tooltip("重力缩放，仅在2D物理中生效")]
    public float gravityScale = 0;

    [Tooltip("初始方向偏移（单位向量）范围0到1之间,会乘上当前角色方向")]
    public Vector2 directionOffset = new Vector2(1, 0);

    [Tooltip("线性阻尼")]
    public float linearDamping;
    [Tooltip("角阻尼")]
    public float angularDamping;

    [Tooltip("是否将碰撞器设置为触发器（Trigger）设置为trigger后,没有物理效果模拟了")]
    public bool isTrigger = false;

    [Tooltip("投掷物存活时间（秒），超过则自动回收）0则持续到碰撞为止")]
    public float lifetime = 5;


    [Tooltip("最大命中目标数量，0=无限")]
    public int maxHitTargets = 1;

    [Header("伤害与效果")]
    [Tooltip("投掷物自身携带的附加伤害（会与SkillData配合使用）")]
    public int damage = 1;

    [Tooltip("投掷物携带的击退力（在命中时施加给目标）")]
    public Vector2 knockbackForce = Vector2.zero;

    [Tooltip("可选：投掷物携带的SkillData，用于扩展技能相关行为")]
    public SkillData skillData;

    [Tooltip("投掷物命中时施加的效果列表（例如眩晕、减速等）")]
    public List<EffectData> effects = new List<EffectData>();

    [Header("视觉/声音")]
    [Tooltip("命中时播放的命中特效（可选）")]
    public GameObject hitVfxPrefab;

    [Tooltip("命中时播放的音效（可选）")]
    public AudioClip hitSfx;

    // 编辑器层面的小校验
    private void OnValidate()
    {
        if (initialSpeed < 0) initialSpeed = 0f;
        if (gravityScale < 0) gravityScale = 0f;
        if (lifetime < 0) lifetime = 0f;
        if (maxHitTargets < 0) maxHitTargets = 0;
    }
}