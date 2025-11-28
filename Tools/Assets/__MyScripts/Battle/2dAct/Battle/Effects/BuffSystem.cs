using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Buff系统管理类
/// 负责管理角色（或训练假人）身上的所有Buff/Debuff效果
/// 主要功能：
/// - 添加、移除、更新Buff
/// - 处理Buff堆叠和刷新逻辑
/// - 应用各种效果（眩晕、护盾、减速等）到CharacterAttributes和CharacterLogic
/// - 处理周期性效果（燃烧、中毒、流血、回复等）
/// </summary>
public class BuffSystem : MonoBehaviour
{
    /// <summary>
    /// 当前激活的所有Buff列表
    /// </summary>
    private List<ActiveBuff> activeBuffs = new List<ActiveBuff>();

    /// <summary>
    /// 角色属性引用，用于应用效果修改
    /// </summary>
    private CharacterAttributes attributes;

    /// <summary>
    /// 角色逻辑引用，用于控制角色状态（如眩晕、定身等）
    /// 对于TestDummy可以为null
    /// </summary>
    private CharacterLogic characterLogic;
    private IStunnable Stunnable;

    /// <summary>
    /// Buff添加事件，在新Buff被添加时触发
    /// </summary>
    public event Action<ActiveBuff> OnBuffAdded;

    /// <summary>
    /// Buff移除事件，在Buff被移除时触发
    /// </summary>
    public event Action<ActiveBuff> OnBuffRemoved;

    /// <summary>
    /// Buff刷新事件，在Buff持续时间被刷新时触发
    /// </summary>
    public event Action<ActiveBuff> OnBuffRefreshed;

    /// <summary>
    /// 初始化Buff系统
    /// 必须在使用前调用，通常在CharacterLogic.Awake()或TestDummy.Start()中初始化
    /// </summary>
    /// <param name="characterAttributes">角色属性引用</param>
    /// <param name="logic">角色逻辑引用（可选）。对CharacterLogic必须传入，对TestDummy可以传null</param>
    public void Init(CharacterAttributes characterAttributes, CharacterLogic logic = null, IStunnable stunnable = null)
    {
        attributes = characterAttributes;
        characterLogic = logic;
        Stunnable = stunnable;

        if (logic != null)
        {
            LogManager.Log($"[BuffSystem] 初始化完成（关联CharacterLogic）");
        }
        else
        {
            LogManager.Log($"[BuffSystem] 初始化完成（无CharacterLogic，可能是TestDummy）");
        }
    }

    /// <summary>
    /// 每帧更新Buff系统
    /// 处理Buff时间倒计时和周期性效果
    /// </summary>
    private void Update()
    {
        UpdateBuffs(Time.deltaTime);
        ApplyPeriodicEffects(Time.deltaTime);
    }

    /// <summary>
    /// 应用Buff到角色身上
    /// 这是外部调用的主要接口，由AttackHitDetector或其他系统调用
    /// </summary>
    /// <param name="effectData">要应用的效果数据</param>
    /// <param name="source">Buff来源对象（通常是攻击者）</param>
    public void ApplyBuff(EffectData effectData, GameObject source = null)
    {
        if (effectData == null)
        {
            LogManager.LogWarning("[BuffSystem] 尝试应用空的EffectData");
            return;
        }

        // 检查是否免疫此类效果（例如眩晕免疫、无敌免疫等）
        if (HasImmunity(effectData.category))
        {
            LogManager.Log($"[BuffSystem] {gameObject.name} 免疫 {effectData.effectName} 效果");
            return;
        }

        // 查找是否已存在相同效果
        var existingBuff = FindBuff(effectData);

        // 如果已存在且可堆叠，处理堆叠逻辑
        if (existingBuff != null && effectData.canStack)
        {
            HandleStackBehavior(existingBuff, effectData);
        }
        // 如果已存在但不可堆叠，只刷新持续时间
        else if (existingBuff != null && !effectData.canStack)
        {
            RefreshBuff(existingBuff, effectData);
        }
        // 添加新Buff
        else
        {
            AddNewBuff(effectData, source);
        }
    }

    /// <summary>
    /// 添加新的Buff
    /// 创建ActiveBuff实例并应用效果
    /// </summary>
    /// <param name="effectData">效果数据</param>
    /// <param name="source">Buff来源对象</param>
    private void AddNewBuff(EffectData effectData, GameObject source)
    {
        var newBuff = new ActiveBuff
        {
            data = effectData,
            remainingDuration = effectData.isPermanent ? -1f : effectData.duration,
            currentStacks = 1,
            source = source,
            accumulatedTime = 0f
        };

        activeBuffs.Add(newBuff);
        ApplyBuffEffect(newBuff);
        OnBuffAdded?.Invoke(newBuff);

        LogManager.Log($"[BuffSystem] {gameObject.name} 获得 {effectData.effectName} (持续: {effectData.duration}秒)");
    }

    /// <summary>
    /// 刷新已存在的Buff持续时间
    /// 用于不可堆叠的Buff重复应用时
    /// </summary>
    /// <param name="buff">要刷新的Buff</param>
    /// <param name="effectData">效果数据</param>
    private void RefreshBuff(ActiveBuff buff, EffectData effectData)
    {
        buff.remainingDuration = effectData.isPermanent ? -1f : effectData.duration;

        RefreshBuffEffect(buff);

        OnBuffRefreshed?.Invoke(buff);

        LogManager.Log($"[BuffSystem] {gameObject.name} 刷新 {effectData.effectName} 持续时间");
    }

    /// <summary>
    /// 处理Buff堆叠行为
    /// </summary>
    private void HandleStackBehavior(ActiveBuff buff, EffectData effectData)
    {
        if (buff.currentStacks < effectData.maxStacks)
        {
            buff.currentStacks++;

            switch (effectData.stackBehavior)
            {
                case StackBehavior.RefreshDuration:
                    buff.remainingDuration = effectData.duration;
                    RefreshBuffEffect(buff);
                    break;
                case StackBehavior.AddDuration:
                    buff.remainingDuration += effectData.duration;
                    break;
                case StackBehavior.IncreaseValue:
                    break;
            }

            OnBuffRefreshed?.Invoke(buff);

            LogManager.Log($"[BuffSystem] {gameObject.name} 叠加 {effectData.effectName} (层数: {buff.currentStacks})");
        }
    }

    /// <summary>
    /// 更新所有激活的Buff
    /// 处理Buff持续时间倒计时，并移除过期的Buff
    /// 对于持续时间为0的立即生效Buff，会在应用后立即移除
    /// </summary>
    private void UpdateBuffs(float deltaTime)
    {
        for (int i = activeBuffs.Count - 1; i >= 0; i--)
        {
            var buff = activeBuffs[i];

            // 永久Buff（remainingDuration为-1）不需要倒计时
            if (buff.remainingDuration > 0)
            {
                buff.remainingDuration -= deltaTime;

                // 持续时间耗尽时移除Buff
                if (buff.remainingDuration <= 0)
                {
                    RemoveBuff(buff);
                }
            }
            // 对于持续时间为0的Buff（立即生效类型），直接移除
            else if (buff.remainingDuration == 0)
            {
                RemoveBuff(buff);
            }
        }
    }

    /// <summary>
    /// 移除指定的Buff
    /// </summary>
    /// <param name="buff">要移除的Buff</param>
    public void RemoveBuff(ActiveBuff buff)
    {
        RemoveBuffEffect(buff);
        activeBuffs.Remove(buff);
        OnBuffRemoved?.Invoke(buff);

        LogManager.Log($"[BuffSystem] {gameObject.name} 移除 {buff.data.effectName}");
    }

    /// <summary>
    /// 移除指定分类的所有Buff
    /// </summary>
    /// <param name="category">效果分类</param>
    public void RemoveBuffsByCategory(EffectCategory category)
    {
        var buffsToRemove = activeBuffs.FindAll(b => b.data.category == category);
        foreach (var buff in buffsToRemove)
        {
            RemoveBuff(buff);
        }
    }

    /// <summary>
    /// 查找指定效果数据的Buff
    /// </summary>
    private ActiveBuff FindBuff(EffectData effectData)
    {
        return activeBuffs.Find(b => b.data == effectData);
    }

    /// <summary>
    /// 检查是否拥有指定分类的Buff
    /// </summary>
    /// <param name="category">效果分类</param>
    /// <returns>是否拥有</returns>
    public bool HasBuff(EffectCategory category)
    {
        return activeBuffs.Exists(b => b.data.category == category);
    }

    /// <summary>
    /// 检查是否免疫某种效果
    /// </summary>
    private bool HasImmunity(EffectCategory category)
    {
        switch (category)
        {
            case EffectCategory.Stun:
                return HasBuff(EffectCategory.StunImmunity);
            case EffectCategory.Invincible:
                return HasBuff(EffectCategory.InvincibleImmunity);
            case EffectCategory.SuperArmor:
                return HasBuff(EffectCategory.SuperArmorImmunity);
            default:
                return false;
        }
    }

    /// <summary>
    /// 刷新Buff效果
    /// 用于某些需要在刷新时重新应用效果的Buff类型
    /// 主要处理需要重置状态或刷新特效的效果
    /// </summary>
    private void RefreshBuffEffect(ActiveBuff buff)
    {
        switch (buff.data.category)
        {
            // 控制类效果 - 需要刷新控制状态
            case EffectCategory.Stun:
                ApplyStunEffect(buff);
                break;

            case EffectCategory.Freeze:
                RefreshFreezeEffect(buff);
                break;

            case EffectCategory.Root:
                ApplyRootEffect(buff);
                break;

            case EffectCategory.Silence:
                ApplySilenceEffect(buff);
                break;

            // 属性加成类效果 - 刷新时重新计算数值
            case EffectCategory.Shield:
                // 护盾刷新时不需要额外操作，堆叠逻辑已处理
                break;

            case EffectCategory.Slow:
                // 减速效果刷新时不需要额外操作
                break;

            case EffectCategory.Haste:
                // 加速效果刷新时不需要额外操作
                break;

            case EffectCategory.SuperArmor:
                ApplySuperArmorEffect(buff);
                break;

            case EffectCategory.Invincible:
                ApplyInvincibleEffect(buff);
                break;

            // 属性提升类效果 - 刷新时不需要重新应用
            case EffectCategory.StrengthBoost:
            case EffectCategory.SpeedBoost:
            case EffectCategory.AttackBoost:
            case EffectCategory.DefenseBoost:
            case EffectCategory.CritRateBoost:
            case EffectCategory.CritDamageBoost:
            case EffectCategory.MaxHealthBoost:
            case EffectCategory.MaxEnergyBoost:
                // 属性提升在应用时已生效，刷新时不需要额外操作
                break;

            // 伤害修正类效果 - 通过DamageCalculator处理
            case EffectCategory.DamageReduction:
            case EffectCategory.DamageAmplification:
            case EffectCategory.DamageReflect:
            case EffectCategory.LifeSteal:
            case EffectCategory.GuaranteedCrit:
            case EffectCategory.Blind:
                // 这些效果在伤害计算时查询，不需要刷新操作
                break;

            // 周期性效果 - 在Update中持续处理
            case EffectCategory.Burn:
            case EffectCategory.Poison:
            case EffectCategory.Bleed:
            case EffectCategory.ConditionalHeal:
            case EffectCategory.EnergyDrain:
            case EffectCategory.EnergyRegeneration:
                // 周期性效果会在ApplyPeriodicEffects中持续生效，刷新时不需要额外操作
                break;

            // 立即生效类效果 - 不需要刷新
            case EffectCategory.InstantHeal:
            case EffectCategory.InstantEnergyRestore:
            case EffectCategory.Knockback:
            case EffectCategory.StaggerDamage:
                // 立即生效的效果在应用时已完成，刷新时不需要额外操作
                break;

            // 免疫类效果 - 刷新时不需要额外操作
            case EffectCategory.StunImmunity:
            case EffectCategory.InvincibleImmunity:
            case EffectCategory.SuperArmorImmunity:
                // 免疫效果通过HasBuff查询，不需要刷新操作
                break;

            default:
                break;
        }
    }

    /// <summary>
    /// 应用Buff效果到角色
    /// 根据Buff分类执行对应的效果逻辑
    /// </summary>
    private void ApplyBuffEffect(ActiveBuff buff)
    {
        if (attributes == null)
        {
            LogManager.LogError($"[BuffSystem] CharacterAttributes 未初始化！");
            return;
        }

        switch (buff.data.category)
        {
            case EffectCategory.Stun:
                // 眩晕效果 - 同步到CharacterLogic
                ApplyStunEffect(buff);
                break;

            case EffectCategory.Shield:
                // 护盾效果 - 增加护盾值
                ApplyShieldEffect(buff);
                break;

            case EffectCategory.Slow:
                // 减速效果 - 降低移动速度
                ApplySlowEffect(buff);
                break;

            case EffectCategory.Haste:
                // 加速效果 - 提升移动速度
                ApplyHasteEffect(buff);
                break;

            case EffectCategory.DamageReduction:
                // 伤害减免 - 通过DamageCalculator处理，这里不需要额外操作
                break;

            case EffectCategory.DamageAmplification:
                // 伤害增幅 - 通过DamageCalculator处理，这里不需要额外操作
                break;

            case EffectCategory.SuperArmor:
                // 霸体效果 - 标记为霸体状态
                ApplySuperArmorEffect(buff);
                break;

            case EffectCategory.Invincible:
                // 无敌效果 - 标记为无敌状态
                ApplyInvincibleEffect(buff);
                break;

            case EffectCategory.Root:
                // 定身效果 - 同步到CharacterLogic
                ApplyRootEffect(buff);
                break;

            case EffectCategory.Freeze:
                // 冰冻效果 - 同步到CharacterLogic
                ApplyFreezeEffect(buff);
                break;

            case EffectCategory.Silence:
                // 沉默效果 - 禁止使用技能
                ApplySilenceEffect(buff);
                break;

            case EffectCategory.InstantHeal:
                // 立即回血
                ApplyInstantHealEffect(buff);
                break;

            case EffectCategory.InstantEnergyRestore:
                // 立即回复能量
                ApplyInstantEnergyRestoreEffect(buff);
                break;

            case EffectCategory.StrengthBoost:
                // 增加力量
                ApplyStrengthBoostEffect(buff);
                break;

            case EffectCategory.SpeedBoost:
                // 增加移速
                ApplySpeedBoostEffect(buff);
                break;

            case EffectCategory.AttackBoost:
                // 增加攻击
                ApplyAttackBoostEffect(buff);
                break;

            case EffectCategory.DefenseBoost:
                // 增加防御
                ApplyDefenseBoostEffect(buff);
                break;

            case EffectCategory.CritRateBoost:
                // 增加暴击率
                ApplyCritRateBoostEffect(buff);
                break;

            case EffectCategory.CritDamageBoost:
                // 增加暴击伤害
                ApplyCritDamageBoostEffect(buff);
                break;

            case EffectCategory.MaxHealthBoost:
                // 增加生命上限
                ApplyMaxHealthBoostEffect(buff);
                break;

            case EffectCategory.MaxEnergyBoost:
                // 增加能量上限
                ApplyMaxEnergyBoostEffect(buff);
                break;

            default:
                // 其他效果类型的通用处理
                break;
        }
    }

    /// <summary>
    /// 移除Buff效果
    /// 恢复Buff对角色造成的影响
    /// </summary>
    private void RemoveBuffEffect(ActiveBuff buff)
    {
        if (attributes == null)
        {
            LogManager.LogError($"[BuffSystem] CharacterAttributes 未初始化！");
            return;
        }

        switch (buff.data.category)
        {
            case EffectCategory.Stun:
                // 移除眩晕效果
                RemoveStunEffect(buff);
                break;

            case EffectCategory.Shield:
                // 移除护盾效果
                RemoveShieldEffect(buff);
                break;

            case EffectCategory.Slow:
                // 移除减速效果
                RemoveSlowEffect(buff);
                break;

            case EffectCategory.Haste:
                // 移除加速效果
                RemoveHasteEffect(buff);
                break;

            case EffectCategory.SuperArmor:
                // 移除霸体效果
                RemoveSuperArmorEffect(buff);
                break;

            case EffectCategory.Invincible:
                // 移除无敌效果
                RemoveInvincibleEffect(buff);
                break;

            case EffectCategory.Root:
                // 移除定身效果
                RemoveRootEffect(buff);
                break;

            case EffectCategory.Freeze:
                // 移除冰冻效果
                RemoveFreezeEffect(buff);
                break;

            case EffectCategory.Silence:
                // 移除沉默效果
                RemoveSilenceEffect(buff);
                break;

            case EffectCategory.StrengthBoost:
                // 移除力量增加
                RemoveStrengthBoostEffect(buff);
                break;

            case EffectCategory.SpeedBoost:
                // 移除移速增加
                RemoveSpeedBoostEffect(buff);
                break;

            case EffectCategory.AttackBoost:
                // 移除攻击增加
                RemoveAttackBoostEffect(buff);
                break;

            case EffectCategory.DefenseBoost:
                // 移除防御增加
                RemoveDefenseBoostEffect(buff);
                break;

            case EffectCategory.CritRateBoost:
                // 移除暴击率增加
                RemoveCritRateBoostEffect(buff);
                break;

            case EffectCategory.CritDamageBoost:
                // 移除暴击伤害增加
                RemoveCritDamageBoostEffect(buff);
                break;

            case EffectCategory.MaxHealthBoost:
                // 移除生命上限增加
                RemoveMaxHealthBoostEffect(buff);
                break;

            case EffectCategory.MaxEnergyBoost:
                // 移除能量上限增加
                RemoveMaxEnergyBoostEffect(buff);
                break;

            default:
                break;
        }
    }

    #region 具体效果实现

    /// <summary>
    /// 应用眩晕效果
    /// 通过CharacterLogic使角色进入眩晕状态，完全无法行动
    /// </summary>
    private void ApplyStunEffect(ActiveBuff buff)
    {
        if (Stunnable != null)
        {
            float duration = buff.data.duration;
            Stunnable.ApplyStun(duration);

            LogManager.Log($"[BuffSystem] 施加眩晕效果: {duration}秒");
        }
        else
        {
            LogManager.LogWarning($"[BuffSystem] 无法施加眩晕效果: Stunnable未配置");
        }
    }

    /// <summary>
    /// 移除眩晕效果
    /// 眩晕效果由CharacterLogic的计时器自动恢复，这里不需要额外操作
    /// </summary>
    private void RemoveStunEffect(ActiveBuff buff)
    {
        // 眩晕效果由CharacterLogic的计时器自动恢复
        // 这里不需要额外操作
    }

    /// <summary>
    /// 应用护盾效果
    /// 增加角色的最大护盾值和当前护盾值
    /// </summary>
    private void ApplyShieldEffect(ActiveBuff buff)
    {
        float shieldAmount = buff.data.GetParameterValue("shieldAmount") * buff.currentStacks;
        attributes.maxShield += shieldAmount;
        attributes.currentShield += shieldAmount;
        LogManager.Log($"[BuffSystem] 获得护盾: {shieldAmount}，当前护盾: {attributes.currentShield}/{attributes.maxShield}");
    }

    /// <summary>
    /// 移除护盾效果
    /// 减少最大护盾值，当前护盾值不超过新的最大值
    /// </summary>
    private void RemoveShieldEffect(ActiveBuff buff)
    {
        float shieldAmount = buff.data.GetParameterValue("shieldAmount") * buff.currentStacks;
        attributes.maxShield -= shieldAmount;
        attributes.currentShield = Mathf.Min(attributes.currentShield, attributes.maxShield);
        LogManager.Log($"[BuffSystem] 移除护盾: {shieldAmount}，当前护盾: {attributes.currentShield}/{attributes.maxShield}");
    }

    /// <summary>
    /// 应用减速效果
    /// 通过降低移动速度修正系数来实现减速
    /// </summary>
    private void ApplySlowEffect(ActiveBuff buff)
    {
        float slowPercent = buff.data.GetParameterValue("slowPercent");
        // 减速通过速度修正系数实现，降低移动速度
        attributes.moveSpeedMultiplier *= (1f - slowPercent / 100f);
        LogManager.Log($"[BuffSystem] 施加减速: {slowPercent}%，当前速度倍率: {attributes.moveSpeedMultiplier:F2}");
    }

    /// <summary>
    /// 移除减速效果
    /// 恢复移动速度修正系数
    /// </summary>
    private void RemoveSlowEffect(ActiveBuff buff)
    {
        float slowPercent = buff.data.GetParameterValue("slowPercent");
        attributes.moveSpeedMultiplier /= (1f - slowPercent / 100f);
        LogManager.Log($"[BuffSystem] 移除减速，当前速度倍率: {attributes.moveSpeedMultiplier:F2}");
    }

    /// <summary>
    /// 应用加速效果
    /// 通过提升移动速度修正系数来实现加速
    /// </summary>
    private void ApplyHasteEffect(ActiveBuff buff)
    {
        float hastePercent = buff.data.GetParameterValue("hastePercent");
        attributes.moveSpeedMultiplier *= (1f + hastePercent / 100f);
        LogManager.Log($"[BuffSystem] 施加加速: {hastePercent}%，当前速度倍率: {attributes.moveSpeedMultiplier:F2}");
    }

    /// <summary>
    /// 移除加速效果
    /// 恢复移动速度修正系数
    /// </summary>
    private void RemoveHasteEffect(ActiveBuff buff)
    {
        float hastePercent = buff.data.GetParameterValue("hastePercent");
        attributes.moveSpeedMultiplier /= (1f + hastePercent / 100f);
        LogManager.Log($"[BuffSystem] 移除加速，当前速度倍率: {attributes.moveSpeedMultiplier:F2}");
    }

    /// <summary>
    /// 应用霸体效果
    /// 设置角色属性标记，使其在攻击时不会被打断
    /// </summary>
    private void ApplySuperArmorEffect(ActiveBuff buff)
    {
        attributes.hasSuperArmor = true;
        LogManager.Log($"[BuffSystem] 获得霸体，角色攻击时将不会被打断");
    }

    /// <summary>
    /// 移除霸体效果
    /// 检查是否还有其他霸体Buff，如果没有则移除标记
    /// </summary>
    private void RemoveSuperArmorEffect(ActiveBuff buff)
    {
        // 检查是否还有其他霸体Buff
        if (!HasBuff(EffectCategory.SuperArmor))
        {
            attributes.hasSuperArmor = false;
            LogManager.Log($"[BuffSystem] 移除霸体");
        }
    }

    /// <summary>
    /// 应用无敌效果
    /// 设置角色属性标记，使其免疫所有伤害和控制效果
    /// </summary>
    private void ApplyInvincibleEffect(ActiveBuff buff)
    {
        attributes.isInvincible = true;
        LogManager.Log($"[BuffSystem] 获得无敌，免疫所有伤害");
    }

    /// <summary>
    /// 移除无敌效果
    /// </summary>
    private void RemoveInvincibleEffect(ActiveBuff buff)
    {
        attributes.isInvincible = false;
        LogManager.Log($"[BuffSystem] 移除无敌");
    }

    /// <summary>
    /// 应用定身效果
    /// 限制角色移动但允许攻击，通过将移动速度设为0实现
    /// </summary>
    private void ApplyRootEffect(ActiveBuff buff)
    {
        if (characterLogic != null)
        {
            // 定身效果会限制移动但允许攻击
            // 保存原始速度用于恢复
            float originalSpeed = attributes.moveSpeedMultiplier;
            buff.data.parameters.Add(new EffectParameter
            {
                name = "originalSpeed",
                value = originalSpeed
            });
            attributes.moveSpeedMultiplier = 0f;
            LogManager.Log($"[BuffSystem] 施加定身效果，无法移动但可以攻击");
        }
        else
        {
            LogManager.LogWarning($"[BuffSystem] 无法施加定身效果: CharacterLogic未配置");
        }
    }

    /// <summary>
    /// 移除定身效果
    /// 恢复原始移动速度
    /// </summary>
    private void RemoveRootEffect(ActiveBuff buff)
    {
        float originalSpeed = buff.data.GetParameterValue("originalSpeed");
        if (originalSpeed > 0)
        {
            attributes.moveSpeedMultiplier = originalSpeed;
        }
        else
        {
            attributes.moveSpeedMultiplier = 1f;
        }
        LogManager.Log($"[BuffSystem] 移除定身效果，恢复速度倍率: {attributes.moveSpeedMultiplier:F2}");
    }

    /// <summary>
    /// 应用冰冻效果
    /// 类似眩晕，但还会降低防御力
    /// </summary>
    private void ApplyFreezeEffect(ActiveBuff buff)
    {
        if (Stunnable != null)
        {
            // 冰冻效果类似眩晕，但还会降低防御
            float duration = buff.data.duration;
            Stunnable.ApplyStun(duration);

            float defenseReduction = buff.data.GetParameterValue("defenseReduction");
            attributes.defense -= (int)defenseReduction;
            LogManager.Log($"[BuffSystem] 施加冰冻效果: {duration}秒, 防御降低: {defenseReduction}");
        }
        else
        {
            LogManager.LogWarning($"[BuffSystem] 无法施加冰冻效果: Stunnable未配置");
        }
    }

    /// <summary>
    /// 移除冰冻效果
    /// 恢复防御力
    /// </summary>
    private void RemoveFreezeEffect(ActiveBuff buff)
    {
        float defenseReduction = buff.data.GetParameterValue("defenseReduction");
        attributes.defense += (int)defenseReduction;
        LogManager.Log($"[BuffSystem] 移除冰冻效果，恢复防御: {defenseReduction}");
    }

    /// <summary>
    /// 刷新冰冻效果
    /// 刷新眩晕时间
    /// </summary>
    private void RefreshFreezeEffect(ActiveBuff buff)
    {
        if (Stunnable != null)
        {
            float duration = buff.data.duration;
            Stunnable.ApplyStun(duration);
            LogManager.Log($"[BuffSystem] 刷新冰冻效果眩晕时间: {duration}秒");
        }
    }

    /// <summary>
    /// 应用沉默效果
    /// 禁止使用技能，通过标记实现，技能系统会检查此标记
    /// </summary>
    private void ApplySilenceEffect(ActiveBuff buff)
    {
        // 沉默效果可以通过标记实现
        // 技能系统会检查HasBuff(EffectCategory.Silence)来判断是否可以使用技能
        LogManager.Log($"[BuffSystem] 施加沉默效果，无法使用技能");
    }

    /// <summary>
    /// 移除沉默效果
    /// </summary>
    private void RemoveSilenceEffect(ActiveBuff buff)
    {
        LogManager.Log($"[BuffSystem] 移除沉默效果");
    }

    private void ApplyInstantHealEffect(ActiveBuff buff)
    {
        float healAmount = buff.data.GetParameterValue("healAmount");
        attributes.currentHealth = Mathf.Min(attributes.maxHealth, attributes.currentHealth + healAmount);
        LogManager.Log($"[BuffSystem] 立即回复生命: {healAmount}，当前生命: {attributes.currentHealth}/{attributes.maxHealth}");
    }

    private void ApplyInstantEnergyRestoreEffect(ActiveBuff buff)
    {
        float energyAmount = buff.data.GetParameterValue("energyAmount");
        attributes.currentEnergy = Mathf.Min(attributes.maxEnergy, attributes.currentEnergy + energyAmount);
        LogManager.Log($"[BuffSystem] 立即回复能量: {energyAmount}，当前能量: {attributes.currentEnergy}/{attributes.maxEnergy}");
    }

    private void ApplyStrengthBoostEffect(ActiveBuff buff)
    {
        float strengthAmount = buff.data.GetParameterValue("strengthAmount");
        float strengthPercent = buff.data.GetParameterValue("strengthPercent");

        if (strengthPercent > 0)
        {
            attributes.strength *= (1f + strengthPercent / 100f);
        }
        else
        {
            attributes.strength += strengthAmount;
        }
        LogManager.Log($"[BuffSystem] 增加力量，当前力量: {attributes.strength}");
    }

    private void RemoveStrengthBoostEffect(ActiveBuff buff)
    {
        float strengthAmount = buff.data.GetParameterValue("strengthAmount");
        float strengthPercent = buff.data.GetParameterValue("strengthPercent");

        if (strengthPercent > 0)
        {
            attributes.strength /= (1f + strengthPercent / 100f);
        }
        else
        {
            attributes.strength -= strengthAmount;
        }
        LogManager.Log($"[BuffSystem] 移除力量增加，当前力量: {attributes.strength}");
    }

    private void ApplySpeedBoostEffect(ActiveBuff buff)
    {
        float speedAmount = buff.data.GetParameterValue("speedAmount");
        float speedPercent = buff.data.GetParameterValue("speedPercent");

        if (speedPercent > 0)
        {
            attributes.moveSpeed *= (1f + speedPercent / 100f);
        }
        else
        {
            attributes.moveSpeed += speedAmount;
        }
        LogManager.Log($"[BuffSystem] 增加移速，当前移速: {attributes.moveSpeed}");
    }

    private void RemoveSpeedBoostEffect(ActiveBuff buff)
    {
        float speedAmount = buff.data.GetParameterValue("speedAmount");
        float speedPercent = buff.data.GetParameterValue("speedPercent");

        if (speedPercent > 0)
        {
            attributes.moveSpeed /= (1f + speedPercent / 100f);
        }
        else
        {
            attributes.moveSpeed -= speedAmount;
        }
        LogManager.Log($"[BuffSystem] 移除移速增加，当前移速: {attributes.moveSpeed}");
    }

    private void ApplyAttackBoostEffect(ActiveBuff buff)
    {
        float attackAmount = buff.data.GetParameterValue("attackAmount");
        float attackPercent = buff.data.GetParameterValue("attackPercent");

        LogManager.Log($"[BuffSystem] 增加攻击: 固定值={attackAmount}, 百分比={attackPercent}%");
    }

    private void RemoveAttackBoostEffect(ActiveBuff buff)
    {
        LogManager.Log($"[BuffSystem] 移除攻击增加");
    }

    private void ApplyDefenseBoostEffect(ActiveBuff buff)
    {
        float defenseAmount = buff.data.GetParameterValue("defenseAmount");
        float defensePercent = buff.data.GetParameterValue("defensePercent");

        if (defensePercent > 0)
        {
            attributes.defense = (int)(attributes.defense * (1f + defensePercent / 100f));
        }
        else
        {
            attributes.defense += (int)defenseAmount;
        }
        LogManager.Log($"[BuffSystem] 增加防御，当前防御: {attributes.defense}");
    }

    private void RemoveDefenseBoostEffect(ActiveBuff buff)
    {
        float defenseAmount = buff.data.GetParameterValue("defenseAmount");
        float defensePercent = buff.data.GetParameterValue("defensePercent");

        if (defensePercent > 0)
        {
            attributes.defense = (int)(attributes.defense / (1f + defensePercent / 100f));
        }
        else
        {
            attributes.defense -= (int)defenseAmount;
        }
        LogManager.Log($"[BuffSystem] 移除防御增加，当前防御: {attributes.defense}");
    }

    private void ApplyCritRateBoostEffect(ActiveBuff buff)
    {
        float critRatePercent = buff.data.GetParameterValue("critRatePercent");
        attributes.critRate += critRatePercent;
        LogManager.Log($"[BuffSystem] 增加暴击率: {critRatePercent}%，当前暴击率: {attributes.critRate}%");
    }

    private void RemoveCritRateBoostEffect(ActiveBuff buff)
    {
        float critRatePercent = buff.data.GetParameterValue("critRatePercent");
        attributes.critRate -= critRatePercent;
        LogManager.Log($"[BuffSystem] 移除暴击率增加，当前暴击率: {attributes.critRate}%");
    }

    private void ApplyCritDamageBoostEffect(ActiveBuff buff)
    {
        float critDamagePercent = buff.data.GetParameterValue("critDamagePercent");
        attributes.critMultiplier += (critDamagePercent / 100f);
        LogManager.Log($"[BuffSystem] 增加暴击伤害: {critDamagePercent}%，当前暴击倍率: {attributes.critMultiplier}x");
    }

    private void RemoveCritDamageBoostEffect(ActiveBuff buff)
    {
        float critDamagePercent = buff.data.GetParameterValue("critDamagePercent");
        attributes.critMultiplier -= (critDamagePercent / 100f);
        LogManager.Log($"[BuffSystem] 移除暴击伤害增加，当前暴击倍率: {attributes.critMultiplier}x");
    }

    private void ApplyMaxHealthBoostEffect(ActiveBuff buff)
    {
        float maxHealthAmount = buff.data.GetParameterValue("maxHealthAmount");
        float maxHealthPercent = buff.data.GetParameterValue("maxHealthPercent");

        if (maxHealthPercent > 0)
        {
            attributes.maxHealth *= (1f + maxHealthPercent / 100f);
        }
        else
        {
            attributes.maxHealth += maxHealthAmount;
        }
        LogManager.Log($"[BuffSystem] 增加生命上限，当前生命上限: {attributes.maxHealth}");
    }

    private void RemoveMaxHealthBoostEffect(ActiveBuff buff)
    {
        float maxHealthAmount = buff.data.GetParameterValue("maxHealthAmount");
        float maxHealthPercent = buff.data.GetParameterValue("maxHealthPercent");

        if (maxHealthPercent > 0)
        {
            attributes.maxHealth /= (1f + maxHealthPercent / 100f);
        }
        else
        {
            attributes.maxHealth -= maxHealthAmount;
        }

        attributes.currentHealth = Mathf.Min(attributes.currentHealth, attributes.maxHealth);
        LogManager.Log($"[BuffSystem] 移除生命上限增加，当前生命上限: {attributes.maxHealth}");
    }

    private void ApplyMaxEnergyBoostEffect(ActiveBuff buff)
    {
        float maxEnergyAmount = buff.data.GetParameterValue("maxEnergyAmount");
        float maxEnergyPercent = buff.data.GetParameterValue("maxEnergyPercent");

        if (maxEnergyPercent > 0)
        {
            attributes.maxEnergy *= (1f + maxEnergyPercent / 100f);
        }
        else
        {
            attributes.maxEnergy += maxEnergyAmount;
        }
        LogManager.Log($"[BuffSystem] 增加能量上限，当前能量上限: {attributes.maxEnergy}");
    }

    private void RemoveMaxEnergyBoostEffect(ActiveBuff buff)
    {
        float maxEnergyAmount = buff.data.GetParameterValue("maxEnergyAmount");
        float maxEnergyPercent = buff.data.GetParameterValue("maxEnergyPercent");

        if (maxEnergyPercent > 0)
        {
            attributes.maxEnergy /= (1f + maxEnergyPercent / 100f);
        }
        else
        {
            attributes.maxEnergy -= maxEnergyAmount;
        }

        attributes.currentEnergy = Mathf.Min(attributes.currentEnergy, attributes.maxEnergy);
        LogManager.Log($"[BuffSystem] 移除能量上限增加，当前能量上限: {attributes.maxEnergy}");
    }

    #endregion

    /// <summary>
    /// 应用周期性效果（如持续伤害、持续回复等）
    /// </summary>
    private void ApplyPeriodicEffects(float deltaTime)
    {
        foreach (var buff in activeBuffs)
        {
            switch (buff.data.category)
            {
                case EffectCategory.Burn:
                    // 燃烧效果 - 每秒造成伤害
                    ApplyBurnDamage(buff, deltaTime);
                    break;

                case EffectCategory.Poison:
                    // 中毒效果 - 每秒造成伤害
                    ApplyPoisonDamage(buff, deltaTime);
                    break;

                case EffectCategory.Bleed:
                    // 流血效果 - 每秒造成伤害
                    ApplyBleedDamage(buff, deltaTime);
                    break;

                case EffectCategory.ConditionalHeal:
                    // 条件回复 - 每秒回复生命
                    ApplyConditionalHeal(buff, deltaTime);
                    break;

                case EffectCategory.EnergyDrain:
                    // 能量消耗 - 每秒消耗能量
                    ApplyEnergyDrain(buff, deltaTime);
                    break;

                case EffectCategory.EnergyRegeneration:
                    // 持续回复能量 - 每秒回复能量
                    ApplyEnergyRegeneration(buff, deltaTime);
                    break;
            }
        }
    }

    #region 周期性效果实现

    /// <summary>
    /// 应用燃烧伤害
    /// 每秒造成持续伤害，伤害会随堆叠层数增加
    /// </summary>
    private void ApplyBurnDamage(ActiveBuff buff, float deltaTime)
    {
        float damagePerSecond = buff.data.GetParameterValue("damagePerSecond") * buff.currentStacks;
        buff.accumulatedTime += deltaTime;

        // 每1秒结算一次伤害
        if (buff.accumulatedTime >= 1f)
        {
            attributes.currentHealth = Mathf.Max(0, attributes.currentHealth - (int)damagePerSecond);
            buff.accumulatedTime -= 1f;
            LogManager.Log($"[BuffSystem] 燃烧伤害: {(int)damagePerSecond}，剩余生命: {attributes.currentHealth}");
        }
    }

    /// <summary>
    /// 应用中毒伤害
    /// 每秒造成持续伤害，伤害会随堆叠层数增加
    /// </summary>
    private void ApplyPoisonDamage(ActiveBuff buff, float deltaTime)
    {
        float damagePerSecond = buff.data.GetParameterValue("damagePerSecond") * buff.currentStacks;
        buff.accumulatedTime += deltaTime;

        // 每1秒结算一次伤害
        if (buff.accumulatedTime >= 1f)
        {
            attributes.currentHealth = Mathf.Max(0, attributes.currentHealth - (int)damagePerSecond);
            buff.accumulatedTime -= 1f;
            LogManager.Log($"[BuffSystem] 中毒伤害: {(int)damagePerSecond}，剩余生命: {attributes.currentHealth}");
        }
    }

    /// <summary>
    /// 应用流血伤害
    /// 每秒造成持续伤害，伤害会随堆叠层数增加
    /// </summary>
    private void ApplyBleedDamage(ActiveBuff buff, float deltaTime)
    {
        float damagePerSecond = buff.data.GetParameterValue("damagePerSecond") * buff.currentStacks;
        buff.accumulatedTime += deltaTime;

        // 每1秒结算一次伤害
        if (buff.accumulatedTime >= 1f)
        {
            attributes.currentHealth = Mathf.Max(0, attributes.currentHealth - (int)damagePerSecond);
            buff.accumulatedTime -= 1f;
            LogManager.Log($"[BuffSystem] 流血伤害: {(int)damagePerSecond}，剩余生命: {attributes.currentHealth}");
        }
    }

    /// <summary>
    /// 应用条件回复
    /// 每秒回复生命值
    /// </summary>
    private void ApplyConditionalHeal(ActiveBuff buff, float deltaTime)
    {
        float healPerSecond = buff.data.GetParameterValue("healPerSecond");
        buff.accumulatedTime += deltaTime;

        // 每1秒结算一次回复
        if (buff.accumulatedTime >= 1f)
        {
            attributes.currentHealth = Mathf.Min(attributes.maxHealth, attributes.currentHealth + (int)healPerSecond);
            buff.accumulatedTime -= 1f;
            LogManager.Log($"[BuffSystem] 回复生命: {(int)healPerSecond}，当前生命: {attributes.currentHealth}");
        }
    }

    /// <summary>
    /// 应用能量消耗
    /// 每秒消耗能量值（需要能量系统支持）
    /// </summary>
    private void ApplyEnergyDrain(ActiveBuff buff, float deltaTime)
    {
        float drainPerSecond = buff.data.GetParameterValue("drainPerSecond");
        buff.accumulatedTime += deltaTime;

        // 每1秒结算一次能量消耗
        if (buff.accumulatedTime >= 1f)
        {
            // 这里需要能量系统支持
            attributes.currentEnergy -= drainPerSecond;
            buff.accumulatedTime -= 1f;
            LogManager.Log($"[BuffSystem] 能量消耗: {(int)drainPerSecond}");
        }
    }

    private void ApplyEnergyRegeneration(ActiveBuff buff, float deltaTime)
    {
        float energyPerSecond = buff.data.GetParameterValue("energyPerSecond");
        buff.accumulatedTime += deltaTime;

        // 每1秒结算一次能量回复
        if (buff.accumulatedTime >= 1f)
        {
            attributes.currentEnergy = Mathf.Min(attributes.maxEnergy, attributes.currentEnergy + energyPerSecond);
            buff.accumulatedTime -= 1f;
            LogManager.Log($"[BuffSystem] 回复能量: {energyPerSecond}，当前能量: {attributes.currentEnergy}/{attributes.maxEnergy}");
        }
    }

    #endregion

    /// <summary>
    /// 获取所有激活的Buff列表
    /// </summary>
    /// <returns>Buff列表副本</returns>
    public List<ActiveBuff> GetActiveBuffs()
    {
        return new List<ActiveBuff>(activeBuffs);
    }

    /// <summary>
    /// 清除所有Buff
    /// </summary>
    public void ClearAllBuffs()
    {
        for (int i = activeBuffs.Count - 1; i >= 0; i--)
        {
            RemoveBuff(activeBuffs[i]);
        }
    }
}

/// <summary>
/// 激活的Buff实例
/// 保存Buff的运行时数据
/// </summary>
[Serializable]
public class ActiveBuff
{
    public EffectData data;              // Buff数据
    public float remainingDuration;      // 剩余持续时间
    public int currentStacks;            // 当前堆叠层数
    public GameObject source;            // Buff来源
    public float accumulatedTime;        // 累积时间（用于周期性效果）
}
