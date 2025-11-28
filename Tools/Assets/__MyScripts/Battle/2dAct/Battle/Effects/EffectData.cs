using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 效果分类枚举
/// 定义了游戏中所有可用的Buff/Debuff效果类型
/// </summary>
public enum EffectCategory
{
    [InspectorName("无")]
    /// <summary>
    /// 无效果 - 占位用，不产生任何实际效果
    /// </summary>
    None,

    [InspectorName("眩晕")]
    /// <summary>
    /// 眩晕效果 - 使目标完全无法行动，无法移动、攻击和使用技能
    /// 参数: duration（持续时间）
    /// </summary>
    Stun,

    [InspectorName("眩晕免疫")]
    /// <summary>
    /// 眩晕免疫 - 免疫所有眩晕效果
    /// </summary>
    StunImmunity,

    [InspectorName("无敌")]
    /// <summary>
    /// 无敌效果 - 免疫所有伤害和控制效果
    /// 参数: duration（持续时间）
    /// </summary>
    Invincible,

    [InspectorName("无敌免疫")]
    /// <summary>
    /// 无敌免疫 - 无法被赋予无敌状态（某些Boss专属）
    /// </summary>
    InvincibleImmunity,

    [InspectorName("霸体")]
    /// <summary>
    /// 霸体效果 - 攻击时不会被打断，仍然会受到伤害但不会被击退
    /// 参数: duration（持续时间）
    /// </summary>
    SuperArmor,

    [InspectorName("霸体免疫")]
    /// <summary>
    /// 霸体免疫 - 无法被赋予霸体状态
    /// </summary>
    SuperArmorImmunity,

    [InspectorName("击退")]
    /// <summary>
    /// 击退效果 - 强制移动目标到指定方向
    /// 参数: knockbackForce（击退力度）, knockbackDirection（击退方向）
    /// </summary>
    Knockback,

    [InspectorName("减速")]
    /// <summary>
    /// 减速效果 - 降低目标移动速度
    /// 参数: slowPercent（减速百分比，0-100）
    /// </summary>
    Slow,

    [InspectorName("加速")]
    /// <summary>
    /// 加速效果 - 提升目标移动速度
    /// 参数: hastePercent（加速百分比）
    /// </summary>
    Haste,

    [InspectorName("伤害减免")]
    /// <summary>
    /// 伤害减免 - 降低受到的伤害
    /// 参数: reductionPercent（减免百分比，0-100）
    /// 注意: 由DamageCalculator在伤害计算时处理
    /// </summary>
    DamageReduction,

    [InspectorName("伤害增幅")]
    /// <summary>
    /// 伤害增幅 - 提升造成的伤害
    /// 参数: amplificationPercent（增幅百分比）
    /// 注意: 由DamageCalculator在伤害计算时处理
    /// </summary>
    DamageAmplification,

    [InspectorName("伤害反弹")]
    /// <summary>
    /// 伤害反弹 - 将受到的伤害按比例反弹给攻击者
    /// 参数: reflectPercent（反弹百分比，0-100）
    /// 注意: 由DamageCalculator在伤害计算时处理
    /// </summary>
    DamageReflect,

    [InspectorName("生命偷取")]
    /// <summary>
    /// 生命偷取 - 造成伤害时恢复等比例生命值
    /// 参数: lifeStealPercent（偷取百分比，0-100）
    /// 注意: 由DamageCalculator在伤害计算时处理
    /// </summary>
    LifeSteal,

    [InspectorName("护盾")]
    /// <summary>
    /// 护盾效果 - 提供额外的生命护盾，吸收伤害
    /// 参数: shieldAmount（护盾值）
    /// </summary>
    Shield,

    [InspectorName("能量消耗")]
    /// <summary>
    /// 能量消耗 - 每秒持续消耗能量值
    /// 参数: drainPerSecond（每秒消耗量）
    /// </summary>
    EnergyDrain,

    [InspectorName("韧性伤害")]
    /// <summary>
    /// 韧性伤害 - 造成额外的韧性伤害，用于破坏敌人防御姿态
    /// 参数: staggerDamage（韧性伤害值）
    /// 注意: 由DamageCalculator在伤害计算时处理
    /// </summary>
    StaggerDamage,

    [InspectorName("必定暴击")]
    /// <summary>
    /// 必定暴击 - 所有攻击必定暴击
    /// 参数: duration（持续时间）
    /// 注意: 由DamageCalculator在伤害计算时处理
    /// </summary>
    GuaranteedCrit,

    [InspectorName("持续回复")]
    /// <summary>
    /// 条件回复 - 每秒持续回复生命值
    /// 参数: healPerSecond（每秒回复量）
    /// </summary>
    ConditionalHeal,

    [InspectorName("定身")]
    /// <summary>
    /// 定身效果 - 无法移动但可以攻击和使用技能
    /// 参数: duration（持续时间）
    /// </summary>
    Root,

    [InspectorName("沉默")]
    /// <summary>
    /// 沉默效果 - 无法使用技能，但可以移动和普通攻击
    /// 参数: duration（持续时间）
    /// </summary>
    Silence,

    [InspectorName("致盲")]
    /// <summary>
    /// 致盲效果 - 大幅降低命中率
    /// 参数: duration（持续时间）, missChance（miss几率增加百分比）
    /// 注意: 由DamageCalculator在伤害计算时处理
    /// </summary>
    Blind,

    [InspectorName("燃烧")]
    /// <summary>
    /// 燃烧效果 - 每秒持续造成火焰伤害
    /// 参数: damagePerSecond（每秒伤害）
    /// </summary>
    Burn,

    [InspectorName("中毒")]
    /// <summary>
    /// 中毒效果 - 每秒持续造成毒素伤害
    /// 参数: damagePerSecond（每秒伤害）
    /// </summary>
    Poison,

    [InspectorName("冰冻")]
    /// <summary>
    /// 冰冻效果 - 完全无法行动且防御力降低（类似眩晕但附带防御削弱）
    /// 参数: duration（持续时间）, defenseReduction（防御降低值）
    /// </summary>
    Freeze,

    [InspectorName("流血")]
    /// <summary>
    /// 流血效果 - 每秒持续造成物理伤害
    /// 参数: damagePerSecond（每秒伤害）
    /// </summary>
    Bleed,

    [InspectorName("立即回血")]
    /// <summary>
    /// 立即回血 - 瞬间恢复一定量生命值
    /// 参数: healAmount（回复量）
    /// </summary>
    InstantHeal,

    [InspectorName("立即回复能量")]
    /// <summary>
    /// 立即回复能量 - 瞬间恢复一定量能量值
    /// 参数: energyAmount（回复量）
    /// </summary>
    InstantEnergyRestore,

    [InspectorName("持续回复能量")]
    /// <summary>
    /// 持续回复能量 - 每秒持续回复能量值
    /// 参数: energyPerSecond（每秒回复量）
    /// </summary>
    EnergyRegeneration,

    [InspectorName("增加力量")]
    /// <summary>
    /// 增加力量 - 提升力量属性
    /// 参数: strengthAmount（力量增加值）或 strengthPercent（力量增加百分比）
    /// </summary>
    StrengthBoost,

    [InspectorName("增加移速")]
    /// <summary>
    /// 增加移速 - 提升移动速度
    /// 参数: speedAmount（移速增加值）或 speedPercent（移速增加百分比）
    /// </summary>
    SpeedBoost,

    [InspectorName("增加攻击")]
    /// <summary>
    /// 增加攻击 - 提升攻击力
    /// 参数: attackAmount（攻击增加值）或 attackPercent（攻击增加百分比）
    /// </summary>
    AttackBoost,

    [InspectorName("增加防御")]
    /// <summary>
    /// 增加防御 - 提升防御力
    /// 参数: defenseAmount（防御增加值）或 defensePercent（防御增加百分比）
    /// </summary>
    DefenseBoost,

    [InspectorName("增加暴击率")]
    /// <summary>
    /// 增加暴击率 - 提升暴击几率
    /// 参数: critRatePercent（暴击率增加百分比）
    /// </summary>
    CritRateBoost,

    [InspectorName("增加暴击伤害")]
    /// <summary>
    /// 增加暴击伤害 - 提升暴击伤害倍率
    /// 参数: critDamagePercent（暴击伤害增加百分比）
    /// </summary>
    CritDamageBoost,

    [InspectorName("增加生命上限")]
    /// <summary>
    /// 增加生命上限 - 提升最大生命值
    /// 参数: maxHealthAmount（生命上限增加值）或 maxHealthPercent（生命上限增加百分比）
    /// </summary>
    MaxHealthBoost,

    [InspectorName("增加能量上限")]
    /// <summary>
    /// 增加能量上限 - 提升最大能量值
    /// 参数: maxEnergyAmount（能量上限增加值）或 maxEnergyPercent（能量上限增加百分比）
    /// </summary>
    MaxEnergyBoost,
}

/// <summary>
/// 效果影响目标枚举
/// 定义效果应该施加给谁
/// </summary>
public enum EffectTarget
{
    /// <summary>
    /// 被击中者 - 效果施加给受击目标（默认行为）
    /// </summary>
    [InspectorName("被击中者")]
    Target,

    /// <summary>
    /// 攻击者 - 效果施加给发起攻击的角色
    /// 例如：攻击时回复自身能量、生命偷取等
    /// </summary>
    [InspectorName("攻击者")]
    Attacker
}

/// <summary>
/// 效果数据ScriptableObject
/// 定义各种Buff/Debuff的基础属性和参数
/// </summary>
[CreateAssetMenu(fileName = "New Effect", menuName = "Character System/Effect/Effect Data")]
public class EffectData : ScriptableObject
{
    [Header("基础信息")]
    [Tooltip("效果显示名称")]
    public string effectName;

    [Tooltip("效果分类，决定效果的行为类型")]
    public EffectCategory category;

    [Tooltip("效果图标，用于UI显示")]
    public Sprite icon;

    [Tooltip("效果影响目标：被击中者还是攻击者")]
    public EffectTarget effectTarget = EffectTarget.Target;

    [Tooltip("是否需要命中才触发：true=必须命中目标才生效，false=无论命中与否都生效")]
    public bool requireHit = true;

    [Header("持续时间")]
    [Tooltip("效果持续时间（秒）")]
    public float duration = 0f;

    [Tooltip("是否为永久效果，永久效果不会自动消失")]
    public bool isPermanent = false;

    [Header("效果参数")]
    [Tooltip("效果的自定义参数列表，用于配置效果的具体数值")]
    public List<EffectParameter> parameters = new List<EffectParameter>();

    [Header("视觉效果")]
    [Tooltip("效果特效预制体")]
    public GameObject vfxPrefab;

    [Tooltip("效果音效")]
    public AudioClip sfxClip;

    [Tooltip("UI着色颜色")]
    public Color uiTintColor = Color.white;

    [Header("堆叠设置")]
    [Tooltip("是否允许堆叠")]
    public bool canStack = false;

    [Tooltip("最大堆叠层数")]
    public int maxStacks = 1;

    [Tooltip("堆叠行为类型")]
    public StackBehavior stackBehavior = StackBehavior.RefreshDuration;

    /// <summary>
    /// 根据参数名获取参数值
    /// </summary>
    /// <param name="parameterName">参数名称</param>
    /// <returns>参数值，如果找不到则返回0</returns>
    public float GetParameterValue(string parameterName)
    {
        var param = parameters.Find(p => p.name == parameterName);
        return param != null ? param.value : 0f;
    }
}

/// <summary>
/// Buff堆叠行为枚举
/// 定义了当Buff重复添加时的处理方式
/// </summary>
public enum StackBehavior
{
    /// <summary>
    /// 刷新持续时间 - 每次叠加时重置持续时间到初始值
    /// 例如: 5秒的Buff在第3秒时再次触发，持续时间重置为5秒
    /// </summary>
    RefreshDuration,

    /// <summary>
    /// 增加持续时间 - 每次叠加时累加持续时间
    /// 例如: 5秒的Buff在第3秒时再次触发，总持续时间变为8秒（剩余2秒+新增5秒）
    /// </summary>
    AddDuration,

    /// <summary>
    /// 增加数值 - 每次叠加时增加效果强度，持续时间不变
    /// 例如: +10%伤害的Buff叠加2层变为+20%伤害
    /// </summary>
    IncreaseValue
}

/// <summary>
/// 效果参数类
/// 用于定义效果的具体数值参数
/// </summary>
[Serializable]
public class EffectParameter
{
    [Tooltip("参数名称，用于代码中查找")]
    public string name;

    [Tooltip("参数数值")]
    public float value;

    [TextArea]
    [Tooltip("参数说明")]
    public string description;
}
