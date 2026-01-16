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
[DisallowMultipleComponent]
[RequireComponent(typeof(PlayerAttributes))]
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
    private CharacterBase characterBase;
    public CharacterBase CharacterBase => characterBase;

    /// <summary>
    /// 角色逻辑引用，用于控制角色状态（如眩晕、定身等）
    /// 对于TestDummy可以为null
    /// </summary>
    private Rigidbody2D rd;
    private IStunnable Stunnable;

    /// <summary>
    /// 顿帧效果的原始时间缩放值
    /// 用于在移除顿帧效果时恢复到之前的时间缩放
    /// </summary>
    private float originalTimeScale = 1f;

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
    public void Init(CharacterBase characterBase, CharacterAttributes characterAttributes, Rigidbody2D rigidbody2D, IStunnable stunnable = null)
    {
        this.characterBase = characterBase;
        attributes = characterAttributes;
        rd = rigidbody2D;
        Stunnable = stunnable;

    }

    /// <summary>
    /// 每帧更新Buff系统
    /// 处理Buff时间倒计时和周期性效果
    /// </summary>
    private void Update()
    {
        // 对于顿帧效果，使用真实时间（不受Time.timeScale影响）
        // 对于其他效果，使用缩放时间
        UpdateBuffs(Time.deltaTime, Time.unscaledDeltaTime);
        ApplyPeriodicEffects(Time.deltaTime);
    }

    /// <summary>
    /// 应用Buff到角色身上
    /// 这是外部调用的主要接口，由AttackHitDetector或其他系统调用
    /// </summary>
    /// <param name="effectData">要应用的效果数据</param>
    /// <param name="source">Buff来源对象（通常是攻击者）</param>
    /// <param name="target">Buff目标对象（通常是受击者或技能释放时的索敌目标）</param>
    public void ApplyBuff(EffectData effectData, CharacterBase source = null, CharacterBase target = null)
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
            AddNewBuff(effectData, source, target);
        }
    }

    /// <summary>
    /// 添加新的Buff
    /// 创建ActiveBuff实例并应用效果
    /// </summary>
    /// <param name="effectData">效果数据</param>
    /// <param name="source">Buff来源对象</param>
    /// <param name="target">Buff目标对象</param>
    private void AddNewBuff(EffectData effectData, CharacterBase source, CharacterBase target = null)
    {
        var newBuff = new ActiveBuff
        {
            data = effectData,
            remainingDuration = effectData.isPermanent ? -1f : effectData.duration,
            currentStacks = effectData.initialStacks, // 使用初始层数
            source = source,
            source_CharacterAttributes = source.PlayerAttributes.characterAtttibute,
            source_BuffSystem = source.BuffSystem,
            target = target,
            accumulatedTime = 0f,
            isPermanent = effectData.isPermanent
        };

        activeBuffs.Add(newBuff);
        ApplyBuffEffect(newBuff);
        OnBuffAdded?.Invoke(newBuff);

        LogManager.Log($"[BuffSystem] {gameObject.name} 获得 {effectData.effectName} (持续: {effectData.duration}秒, 层数: {newBuff.currentStacks})");
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
    /// 处理Buff的堆叠行为
    /// 根据配置的堆叠行为类型，执行不同的堆叠逻辑
    /// </summary>
    private void HandleStackBehavior(ActiveBuff buff, EffectData effectData)
    {
        if (buff.currentStacks < effectData.maxStacks)
        {
            int oldStacks = buff.currentStacks;
            int actualAddedStacks = Mathf.Min(effectData.stacksPerApplication, effectData.maxStacks - oldStacks);
            int newStacks = oldStacks + actualAddedStacks;

            switch (effectData.stackBehavior)
            {
                case StackBehavior.RefreshDuration:
                    buff.remainingDuration = effectData.duration;
                    buff.currentStacks = newStacks;
                    RefreshBuffEffect(buff);
                    break;

                case StackBehavior.AddDuration:
                    buff.remainingDuration += effectData.duration * actualAddedStacks;
                    buff.currentStacks = newStacks;
                    break;

                case StackBehavior.IncreaseValue:
                    buff.remainingDuration = effectData.duration;

                    if (ShouldUpdateModifierDirectly(effectData.category))
                    {
                        buff.currentStacks = newStacks;
                        attributes.UpdateModifierStacksBySource(GetBuffSourceId(buff), newStacks);
                    }
                    else
                    {
                        RemoveBuffEffect(buff);
                        buff.currentStacks = newStacks;
                        ApplyBuffEffect(buff);
                    }
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
    /// <param name="deltaTime">缩放后的时间增量，用于大多数效果</param>
    /// <param name="unscaledDeltaTime">未缩放的时间增量，用于顿帧等特殊效果</param>
    private void UpdateBuffs(float deltaTime, float unscaledDeltaTime)
    {
        for (int i = activeBuffs.Count - 1; i >= 0; i--)
        {
            var buff = activeBuffs[i];

            // 永久Buff（remainingDuration为-1）不需要倒计时
            if (buff.remainingDuration > 0)
            {
                // 顿帧效果使用真实时间（不受Time.timeScale影响）
                // 其他效果使用缩放时间
                float timeToSubtract = (buff.data.category == EffectCategory.HitStop)
                    ? unscaledDeltaTime
                    : deltaTime;

                buff.remainingDuration -= timeToSubtract;

                // 持续时间耗尽时移除Buff
                if (buff.remainingDuration <= 0)
                {
                    HandleExpiredBuff(buff);
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
    /// 处理过期的Buff
    /// 如果层数大于1，则减少层数并重置持续时间；否则移除整个Buff
    /// </summary>
    private void HandleExpiredBuff(ActiveBuff buff)
    {
        if (buff.currentStacks > 1)
        {
            int oldStacks = buff.currentStacks;
            int newStacks = oldStacks - 1;
            buff.currentStacks = newStacks;
            buff.remainingDuration = buff.data.duration;

            if (ShouldUpdateModifierDirectly(buff.data.category))
            {
                attributes.UpdateModifierStacksBySource(GetBuffSourceId(buff), newStacks);
            }
            else
            {
                RemoveBuffEffect(buff);
                ApplyBuffEffect(buff);
            }

            LogManager.Log($"[BuffSystem] {buff.data.effectName} 时间到期，减少一层 (剩余层数: {buff.currentStacks})");
        }
        else
        {
            RemoveBuff(buff);
        }
    }

    private bool ShouldUpdateModifierDirectly(EffectCategory category)
    {
        return category == EffectCategory.MaxHealthBoost ||
               category == EffectCategory.MaxEnergyBoost ||
               category == EffectCategory.StrengthBoost ||
               category == EffectCategory.AgilityBoost ||
               category == EffectCategory.SpeedBoost ||
               category == EffectCategory.AttackBoost ||
               category == EffectCategory.DefenseBoost ||
               category == EffectCategory.CritRateBoost ||
               category == EffectCategory.CritDamageBoost;
    }

    /// <summary>
    /// 移除指定的Buff
    /// </summary>
    /// <param name="buff">要移除的Buff</param>
    public void RemoveBuff(ActiveBuff buff)
    {
        if (buff.isPermanent)
        {
            //LogManager.LogWarning($"[BuffSystem] {gameObject.name} 尝试移除永久Buff {buff.data.effectName}，操作被阻止");
            return;
        }

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
        var buffsToRemove = activeBuffs.FindAll(b => b.data.category == category && !b.isPermanent);
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

            //case EffectCategory.Freeze:
            //    RefreshFreezeEffect(buff);
            //    break;

            case EffectCategory.Root:
                ApplyRootEffect(buff);
                break;

            //case EffectCategory.Silence:
            //    ApplySilenceEffect(buff);
            //    break;

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
            case EffectCategory.AgilityBoost:
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
                //case EffectCategory.Blind:
                // 这些效果在伤害计算时查询，不需要刷新操作
                break;

            // 周期性效果 - 在Update中持续处理
            //case EffectCategory.Burn:
            //case EffectCategory.Poison:
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
                // 立即生效的效果在应用时已完成，刷新时不需要额外操作
                break;

            // 免疫类效果 - 刷新时不需要额外操作
            case EffectCategory.StunImmunity:
            case EffectCategory.InvincibleImmunity:
            case EffectCategory.SuperArmorImmunity:
                // 免疫效果通过HasBuff查询，不需要刷新操作
                break;

            // 顿帧效果 - 立即生效类型，不需要刷新
            case EffectCategory.HitStop:
                // 顿帧是立即生效且持续时间很短的效果，通常不会被刷新
                // 如果需要刷新，重新应用即可
                ApplyHitStopEffect(buff);
                break;

            // 镜头抖动效果 - 立即生效类型
            case EffectCategory.CameraShake:
                // 镜头抖动每次刷新都重新触发，根据优先级决定是否执行
                ApplyCameraShakeEffect(buff);
                break;

            case EffectCategory.ExtraJump:
                // 额外跳跃次数效果不需要刷新
                break;

            case EffectCategory.Teleport:
                // 瞬移效果是立即生效类型，刷新时重新触发
                ApplyTeleportEffect(buff);
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
                // 眩晕效果 -
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
            case EffectCategory.Knockback:
                // 击退效果 - 立即应用击退
                ApplyKnockbackEffect(buff);
                break;
            case EffectCategory.Invincible:
                // 无敌效果 - 标记为无敌状态
                ApplyInvincibleEffect(buff);
                break;

            case EffectCategory.Root:
                // 定身效果 -
                ApplyRootEffect(buff);
                break;

            //case EffectCategory.Freeze:
            //    // 冰冻效果 -
            //    ApplyFreezeEffect(buff);
            //    break;

            //case EffectCategory.Silence:
            //    // 沉默效果 - 禁止使用技能
            //    ApplySilenceEffect(buff);
            //    break;

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
            case EffectCategory.AgilityBoost:
                // 增加敏捷
                ApplyAgilityBoostEffect(buff);
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

            case EffectCategory.HitStop:
                // 顿帧效果 - 短暂降低时间流速
                ApplyHitStopEffect(buff);
                break;

            case EffectCategory.CameraShake:
                // 镜头抖动效果 - 使镜头产生震动
                ApplyCameraShakeEffect(buff);
                break;

            case EffectCategory.ExtraJump:
                // 额外跳跃次数效果 - 通过CharacterLogic处理
                ApplyExtraJumpEffect(buff);
                break;

            case EffectCategory.Teleport:
                // 瞬移效果 - 立即移动到目标附近
                ApplyTeleportEffect(buff);
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

            //case EffectCategory.Freeze:
            //    // 移除冰冻效果
            //    RemoveFreezeEffect(buff);
            //    break;

            //case EffectCategory.Silence:
            //    // 移除沉默效果
            //    RemoveSilenceEffect(buff);
            //    break;

            case EffectCategory.StrengthBoost:
                // 移除力量增加
                RemoveStrengthBoostEffect(buff);
                break;
            case EffectCategory.AgilityBoost:
                // 移除敏捷增加
                RemoveAgilityBoostEffect(buff);
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

            case EffectCategory.HitStop:
                // 移除顿帧效果
                RemoveHitStopEffect(buff);
                break;

            case EffectCategory.ExtraJump:
                // 移除额外跳跃次数效果
                RemoveExtraJumpEffect(buff);
                break;

            case EffectCategory.Teleport:
                // 瞬移是立即生效类型，不需要移除操作
                break;

            default:
                break;
        }
    }


    #region 具体效果实现

    /// <summary>
    /// 应用眩晕效果
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
    /// </summary>
    private void RemoveStunEffect(ActiveBuff buff)
    {
        // 这里不需要额外操作
    }

    /// <summary>
    /// 应用护盾效果
    /// 增加角色的最大护盾值和当前护盾值
    /// </summary>
    private void ApplyShieldEffect(ActiveBuff buff)
    {
        float shieldAmount = buff.data.GetParameterValue("value") * buff.currentStacks;
        attributes.maxShield += shieldAmount;
        attributes.ChangeShield(shieldAmount);
        LogManager.Log($"[BuffSystem] 获得护盾: {shieldAmount}，当前护盾: {attributes.currentShield}/{attributes.maxShield}");
    }

    /// <summary>
    /// 移除护盾效果
    /// 减少最大护盾值，当前护盾值不超过新的最大值
    /// </summary>
    private void RemoveShieldEffect(ActiveBuff buff)
    {
        float shieldAmount = buff.data.GetParameterValue("value") * buff.currentStacks;
        attributes.maxShield -= shieldAmount;
        attributes.ChangeShield(0);
        LogManager.Log($"[BuffSystem] 移除护盾: {shieldAmount}，当前护盾: {attributes.currentShield}/{attributes.maxShield}");
    }

    /// <summary>
    /// 应用减速效果
    /// 通过activeModifiers系统降低移动速度系数
    /// </summary>
    private void ApplySlowEffect(ActiveBuff buff)
    {
        float slowPercent = buff.data.GetParameterValue("percent") * buff.currentStacks;

        AttributeModifierInstance modifier = new AttributeModifierInstance
        {
            attributeType = AttributeType.MoveSpeedMultiplier,
            modifierType = ModifierType.Percent,
            value = -slowPercent,
            remainingDuration = -1,
            sourceId = GetBuffSourceId(buff),
            stacks = buff.currentStacks
        };

        attributes.AddModifier(modifier);
        LogManager.Log($"[BuffSystem] 施加减速: {slowPercent}%，当前速度倍率: {attributes.FinalMoveSpeedMultiplier:F2}");
    }

    /// <summary>
    /// 移除减速效果
    /// 通过移除对应的modifier恢复移动速度系数
    /// </summary>
    private void RemoveSlowEffect(ActiveBuff buff)
    {
        attributes.RemoveModifiersBySource(GetBuffSourceId(buff));
        LogManager.Log($"[BuffSystem] 移除减速，当前速度倍率: {attributes.FinalMoveSpeedMultiplier:F2}");
    }

    /// <summary>
    /// 应用加速效果
    /// 通过activeModifiers系统提升移动速度系数
    /// </summary>
    private void ApplyHasteEffect(ActiveBuff buff)
    {
        float hastePercent = buff.data.GetParameterValue("percent") * buff.currentStacks;

        AttributeModifierInstance modifier = new AttributeModifierInstance
        {
            attributeType = AttributeType.MoveSpeedMultiplier,
            modifierType = ModifierType.Percent,
            value = hastePercent,
            remainingDuration = -1,
            sourceId = GetBuffSourceId(buff),
            stacks = buff.currentStacks
        };

        attributes.AddModifier(modifier);
        LogManager.Log($"[BuffSystem] 施加加速: {hastePercent}%，当前速度倍率: {attributes.FinalMoveSpeedMultiplier:F2}");
    }

    /// <summary>
    /// 移除加速效果
    /// 通过移除对应的modifier恢复移动速度系数
    /// </summary>
    private void RemoveHasteEffect(ActiveBuff buff)
    {
        attributes.RemoveModifiersBySource(GetBuffSourceId(buff));
        LogManager.Log($"[BuffSystem] 移除加速，当前速度倍率: {attributes.FinalMoveSpeedMultiplier:F2}");
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
        attributes.hasSuperArmor = false;
        LogManager.Log($"[BuffSystem] 移除霸体");
    }

    /// <summary>
    /// 应用击退效果
    /// </summary>
    /// <param name="buff"></param>
    private void ApplyKnockbackEffect(ActiveBuff buff)
    {
        if (buff.data.parameters.Count > 0)
        {
            float knockbackDistance = buff.data.GetParameterValue("distance");
            float forceX = buff.data.GetParameterValue("forceX");
            float forceY = buff.data.GetParameterValue("forceY");
            Vector3 knockbackDirection = (transform.position - buff.source.transform.position).normalized;
            if (forceX != 0 || forceY != 0)
            {
                Vector2 force = new Vector2(forceX, forceY);
                rd.AddForce(force * knockbackDirection, ForceMode2D.Impulse);
                LogManager.Log($"[BuffSystem] 施加击退效果: 力量 ({forceX}, {forceY})，方向 {knockbackDirection}");
            }
            if (knockbackDistance != 0)
            {
                transform.position += knockbackDirection * knockbackDistance;
                LogManager.Log($"[BuffSystem] 施加击退效果: 距离 {knockbackDistance}，方向 {knockbackDirection}");
            }
        }
    }

    /// <summary>
    /// 应用无敌效果
    /// 设置角色属性标记，使其免疫所有伤害和控制效果
    /// </summary>
    private void ApplyInvincibleEffect(ActiveBuff buff)
    {
        attributes.AddInvincibility();
        LogManager.Log($"[BuffSystem] 获得无敌，免疫所有伤害 (计数: {attributes.isInvincible})");
    }

    /// <summary>
    /// 移除无敌效果
    /// </summary>
    private void RemoveInvincibleEffect(ActiveBuff buff)
    {
        attributes.RemoveInvincibility();
        LogManager.Log($"[BuffSystem] 移除无敌 (计数: {attributes.isInvincible})");
    }

    /// <summary>
    /// 应用定身效果
    /// 限制角色移动但允许攻击，通过activeModifiers将移动速度系数设为-100%
    /// </summary>
    private void ApplyRootEffect(ActiveBuff buff)
    {
        AttributeModifierInstance modifier = new AttributeModifierInstance
        {
            attributeType = AttributeType.MoveSpeedMultiplier,
            modifierType = ModifierType.Percent,
            value = -100f,
            remainingDuration = -1,
            sourceId = GetBuffSourceId(buff),
            stacks = 1
        };

        attributes.AddModifier(modifier);
        LogManager.Log($"[BuffSystem] 施加定身效果，无法移动但可以攻击");
    }

    /// <summary>
    /// 移除定身效果
    /// 通过移除对应的modifier恢复移动速度
    /// </summary>
    private void RemoveRootEffect(ActiveBuff buff)
    {
        attributes.RemoveModifiersBySource(GetBuffSourceId(buff));
        LogManager.Log($"[BuffSystem] 移除定身效果，恢复速度倍率: {attributes.FinalMoveSpeedMultiplier:F2}");
    }

    /// <summary>
    /// 应用冰冻效果
    /// 类似眩晕，但还会通过activeModifiers降低防御力
    /// </summary>
    private void ApplyFreezeEffect(ActiveBuff buff)
    {
        if (Stunnable != null)
        {
            // 冰冻效果类似眩晕
            float duration = buff.data.duration;
            Stunnable.ApplyStun(duration);

            // 通过activeModifiers降低防御
            float defenseReduction = buff.data.GetParameterValue("value");
            if (defenseReduction != 0)
            {
                var modifier = new AttributeModifierInstance
                {
                    attributeType = AttributeType.Defense,
                    modifierType = ModifierType.Flat,
                    value = -defenseReduction, // 负值表示降低
                    remainingDuration = buff.remainingDuration,
                    sourceId = GetBuffSourceId(buff),
                    stacks = buff.currentStacks
                };
                attributes.AddModifier(modifier);
            }

            LogManager.Log($"[BuffSystem] 施加冰冻效果: {duration}秒, 防御降低: {defenseReduction}");
        }
        else
        {
            LogManager.LogWarning($"[BuffSystem] 无法施加冰冻效果: Stunnable未配置");
        }
    }

    /// <summary>
    /// 移除冰冻效果
    /// 通过activeModifiers恢复防御力
    /// </summary>
    private void RemoveFreezeEffect(ActiveBuff buff)
    {
        // 通过sourceId移除所有相关的修改器（包括防御降低）
        attributes.RemoveModifiersBySource(GetBuffSourceId(buff));

        LogManager.Log($"[BuffSystem] 移除冰冻效果，最终防御: {attributes.FinalDefense}");
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
    /// <summary>
    /// 应用立即回复生命效果
    /// </summary>
    /// <param name="buff"></param>
    private void ApplyInstantHealEffect(ActiveBuff buff)
    {
        float healAmount = buff.data.GetParameterValue("value") * buff.currentStacks;
        attributes.ChangeHealth(healAmount, buff.source);
        DamageDisplayHelper.ShowHealOnCharacter(healAmount, transform);
        LogManager.Log($"[BuffSystem] 立即回复生命: {healAmount}，当前生命: {attributes.currentHealth}/{attributes.maxHealth}");
    }

    private void ApplyInstantEnergyRestoreEffect(ActiveBuff buff)
    {
        float energyAmount = buff.data.GetParameterValue("value") * buff.currentStacks;
        attributes.ChangeEnergy(energyAmount);
        LogManager.Log($"[BuffSystem] 立即回复能量: {energyAmount}，当前能量: {attributes.currentEnergy}/{attributes.maxEnergy}");
    }

    /// <summary>
    /// 应用力量增加效果（通过activeModifiers实现）
    /// </summary>
    private void ApplyStrengthBoostEffect(ActiveBuff buff)
    {
        float strengthAmount = buff.data.GetParameterValue("value");
        float strengthPercent = buff.data.GetParameterValue("percent");

        // 通过activeModifiers添加固定值加成
        if (strengthAmount != 0)
        {
            var modifier = new AttributeModifierInstance
            {
                attributeType = AttributeType.Strength,
                modifierType = ModifierType.Flat,
                value = strengthAmount * buff.currentStacks,
                remainingDuration = buff.remainingDuration,
                sourceId = GetBuffSourceId(buff),
                stacks = buff.currentStacks
            };
            attributes.AddModifier(modifier);
        }

        // 通过activeModifiers添加百分比加成
        if (strengthPercent != 0)
        {
            var modifier = new AttributeModifierInstance
            {
                attributeType = AttributeType.Strength,
                modifierType = ModifierType.Percent,
                value = strengthPercent * buff.currentStacks,
                remainingDuration = buff.remainingDuration,
                sourceId = GetBuffSourceId(buff),
                stacks = buff.currentStacks
            };
            attributes.AddModifier(modifier);
        }

        LogManager.Log($"[BuffSystem] 增加力量，最终力量: {attributes.FinalStrength}");
    }

    /// <summary>
    /// 移除力量增加效果（通过activeModifiers实现）
    /// </summary>
    private void RemoveStrengthBoostEffect(ActiveBuff buff)
    {
        // 通过sourceId移除所有相关的修改器
        attributes.RemoveModifiersBySource(GetBuffSourceId(buff));

        LogManager.Log($"[BuffSystem] 移除力量增加，最终力量: {attributes.FinalStrength}");
    }
    /// <summary>
    /// 应用敏捷增加效果（通过activeModifiers实现）
    /// </summary>
    private void ApplyAgilityBoostEffect(ActiveBuff buff)
    {
        float agilityAmount = buff.data.GetParameterValue("value");
        float agilityPercent = buff.data.GetParameterValue("percent");

        // 通过activeModifiers添加固定值加成
        if (agilityAmount != 0)
        {
            var modifier = new AttributeModifierInstance
            {
                attributeType = AttributeType.Agility,
                modifierType = ModifierType.Flat,
                value = agilityAmount * buff.currentStacks,
                remainingDuration = buff.remainingDuration,
                sourceId = GetBuffSourceId(buff),
                stacks = buff.currentStacks
            };
            attributes.AddModifier(modifier);
        }

        // 通过activeModifiers添加百分比加成
        if (agilityPercent != 0)
        {
            var modifier = new AttributeModifierInstance
            {
                attributeType = AttributeType.Agility,
                modifierType = ModifierType.Percent,
                value = agilityPercent * buff.currentStacks,
                remainingDuration = buff.remainingDuration,
                sourceId = GetBuffSourceId(buff),
                stacks = buff.currentStacks
            };
            attributes.AddModifier(modifier);
        }

        LogManager.Log($"[BuffSystem] 增加敏捷，最终敏捷: {attributes.FinalAgility}");
    }

    /// <summary>
    /// 移除敏捷增加效果（通过activeModifiers实现）
    /// </summary>
    private void RemoveAgilityBoostEffect(ActiveBuff buff)
    {
        // 通过sourceId移除所有相关的修改器
        attributes.RemoveModifiersBySource(GetBuffSourceId(buff));

        LogManager.Log($"[BuffSystem] 移除敏捷增加，最终敏捷: {attributes.FinalAgility}");
    }
    /// <summary>
    /// 应用移速增加效果（通过activeModifiers实现）
    /// </summary>
    private void ApplySpeedBoostEffect(ActiveBuff buff)
    {
        float speedAmount = buff.data.GetParameterValue("value");
        float speedPercent = buff.data.GetParameterValue("percent");

        // 通过activeModifiers添加固定值加成
        if (speedAmount != 0)
        {
            var modifier = new AttributeModifierInstance
            {
                attributeType = AttributeType.MoveSpeed,
                modifierType = ModifierType.Flat,
                value = speedAmount * buff.currentStacks,
                remainingDuration = buff.remainingDuration,
                sourceId = GetBuffSourceId(buff),
                stacks = buff.currentStacks
            };
            attributes.AddModifier(modifier);
        }

        // 通过activeModifiers添加百分比加成
        if (speedPercent != 0)
        {
            var modifier = new AttributeModifierInstance
            {
                attributeType = AttributeType.MoveSpeed,
                modifierType = ModifierType.Percent,
                value = speedPercent * buff.currentStacks,
                remainingDuration = buff.remainingDuration,
                sourceId = GetBuffSourceId(buff),
                stacks = buff.currentStacks
            };
            attributes.AddModifier(modifier);
        }

        LogManager.Log($"[BuffSystem] 增加移速，最终移速: {attributes.FinalMoveSpeed}");
    }

    /// <summary>
    /// 移除移速增加效果（通过activeModifiers实现）
    /// </summary>
    private void RemoveSpeedBoostEffect(ActiveBuff buff)
    {
        // 通过sourceId移除所有相关的修改器
        attributes.RemoveModifiersBySource(GetBuffSourceId(buff));

        LogManager.Log($"[BuffSystem] 移除移速增加，最终移速: {attributes.FinalMoveSpeed}");
    }

    private void ApplyAttackBoostEffect(ActiveBuff buff)
    {
        float attackAmount = buff.data.GetParameterValue("value") * buff.currentStacks;
        float attackPercent = buff.data.GetParameterValue("percent") * buff.currentStacks;

        if (attackAmount != 0)
        {
            var modifier = new AttributeModifierInstance
            {
                attributeType = AttributeType.Attack,
                modifierType = ModifierType.Flat,
                value = attackAmount * buff.currentStacks,
                remainingDuration = buff.remainingDuration,
                sourceId = GetBuffSourceId(buff),
                stacks = buff.currentStacks
            };
            attributes.AddModifier(modifier);

        }
        if (attackPercent != 0)
        {
            var modifier = new AttributeModifierInstance
            {
                attributeType = AttributeType.Attack,
                modifierType = ModifierType.Percent,
                value = attackPercent * buff.currentStacks,
                remainingDuration = buff.remainingDuration,
                sourceId = GetBuffSourceId(buff),
                stacks = buff.currentStacks
            };
            attributes.AddModifier(modifier);
        }

        LogManager.Log($"[BuffSystem] 增加攻击: 固定值={attackAmount}, 百分比={attackPercent}%");
    }

    private void RemoveAttackBoostEffect(ActiveBuff buff)
    {
        attributes.RemoveModifiersBySource(GetBuffSourceId(buff));
        LogManager.Log($"[BuffSystem] 移除攻击增加");
    }

    /// <summary>
    /// 应用防御增加效果（通过activeModifiers实现）
    /// </summary>
    private void ApplyDefenseBoostEffect(ActiveBuff buff)
    {
        float defenseAmount = buff.data.GetParameterValue("value");
        float defensePercent = buff.data.GetParameterValue("percent");

        // 通过activeModifiers添加固定值加成
        if (defenseAmount != 0)
        {
            var modifier = new AttributeModifierInstance
            {
                attributeType = AttributeType.Defense,
                modifierType = ModifierType.Flat,
                value = defenseAmount * buff.currentStacks,
                remainingDuration = buff.remainingDuration,
                sourceId = GetBuffSourceId(buff),
                stacks = buff.currentStacks
            };
            attributes.AddModifier(modifier);
        }

        // 通过activeModifiers添加百分比加成
        if (defensePercent != 0)
        {
            var modifier = new AttributeModifierInstance
            {
                attributeType = AttributeType.Defense,
                modifierType = ModifierType.Percent,
                value = defensePercent * buff.currentStacks,
                remainingDuration = buff.remainingDuration,
                sourceId = GetBuffSourceId(buff),
                stacks = buff.currentStacks
            };
            attributes.AddModifier(modifier);
        }

        LogManager.Log($"[BuffSystem] 增加防御，最终防御: {attributes.FinalDefense}");
    }

    /// <summary>
    /// 移除防御增加效果（通过activeModifiers实现）
    /// </summary>
    private void RemoveDefenseBoostEffect(ActiveBuff buff)
    {
        // 通过sourceId移除所有相关的修改器
        attributes.RemoveModifiersBySource(GetBuffSourceId(buff));

        LogManager.Log($"[BuffSystem] 移除防御增加，最终防御: {attributes.FinalDefense}");
    }

    /// <summary>
    /// 应用暴击率增加效果（通过activeModifiers实现）
    /// </summary>
    private void ApplyCritRateBoostEffect(ActiveBuff buff)
    {
        float critRatePercent = buff.data.GetParameterValue("percent");

        // 通过activeModifiers添加百分比加成（暴击率一般使用固定值形式，但用Flat类型）
        if (critRatePercent != 0)
        {
            var modifier = new AttributeModifierInstance
            {
                attributeType = AttributeType.CritRate,
                modifierType = ModifierType.Flat,
                value = critRatePercent * buff.currentStacks,
                remainingDuration = buff.remainingDuration,
                sourceId = GetBuffSourceId(buff),
                stacks = buff.currentStacks
            };
            attributes.AddModifier(modifier);
        }

        LogManager.Log($"[BuffSystem] 增加暴击率: {critRatePercent * buff.currentStacks}%，最终暴击率: {attributes.FinalCritRate}%");
    }

    /// <summary>
    /// 移除暴击率增加效果（通过activeModifiers实现）
    /// </summary>
    private void RemoveCritRateBoostEffect(ActiveBuff buff)
    {
        // 通过sourceId移除所有相关的修改器
        attributes.RemoveModifiersBySource(GetBuffSourceId(buff));

        LogManager.Log($"[BuffSystem] 移除暴击率增加，最终暴击率: {attributes.FinalCritRate}%");
    }

    /// <summary>
    /// 应用暴击伤害增加效果（通过activeModifiers实现）
    /// </summary>
    private void ApplyCritDamageBoostEffect(ActiveBuff buff)
    {
        float critDamagePercent = buff.data.GetParameterValue("percent");

        // 通过activeModifiers添加百分比加成（转换为倍率的增量）
        if (critDamagePercent != 0)
        {
            var modifier = new AttributeModifierInstance
            {
                attributeType = AttributeType.CritMultiplier,
                modifierType = ModifierType.Flat,
                value = (critDamagePercent / 100f) * buff.currentStacks,
                remainingDuration = buff.remainingDuration,
                sourceId = GetBuffSourceId(buff),
                stacks = buff.currentStacks
            };
            attributes.AddModifier(modifier);
        }

        LogManager.Log($"[BuffSystem] 增加暴击伤害: {critDamagePercent * buff.currentStacks}%，最终暴击倍率: {attributes.FinalCritMultiplier}x");
    }

    /// <summary>
    /// 移除暴击伤害增加效果（通过activeModifiers实现）
    /// </summary>
    private void RemoveCritDamageBoostEffect(ActiveBuff buff)
    {
        // 通过sourceId移除所有相关的修改器
        attributes.RemoveModifiersBySource(GetBuffSourceId(buff));

        LogManager.Log($"[BuffSystem] 移除暴击伤害增加，最终暴击倍率: {attributes.FinalCritMultiplier}x");
    }

    /// <summary>
    /// 应用最大生命值增加效果（通过activeModifiers实现）
    /// </summary>
    private void ApplyMaxHealthBoostEffect(ActiveBuff buff)
    {
        float maxHealthAmount = buff.data.GetParameterValue("value");
        float maxHealthPercent = buff.data.GetParameterValue("percent");

        // 通过activeModifiers添加固定值加成
        if (maxHealthAmount != 0)
        {
            var modifier = new AttributeModifierInstance
            {
                attributeType = AttributeType.MaxHealth,
                modifierType = ModifierType.Flat,
                value = maxHealthAmount * buff.currentStacks,
                remainingDuration = buff.remainingDuration,
                sourceId = GetBuffSourceId(buff),
                stacks = buff.currentStacks
            };
            attributes.AddModifier(modifier);
        }

        // 通过activeModifiers添加百分比加成
        if (maxHealthPercent != 0)
        {
            var modifier = new AttributeModifierInstance
            {
                attributeType = AttributeType.MaxHealth,
                modifierType = ModifierType.Percent,
                value = maxHealthPercent * buff.currentStacks,
                remainingDuration = buff.remainingDuration,
                sourceId = GetBuffSourceId(buff),
                stacks = buff.currentStacks
            };
            attributes.AddModifier(modifier);
        }

        LogManager.Log($"[BuffSystem] 增加生命上限，最终生命上限: {attributes.FinalMaxHealth}");
    }

    /// <summary>
    /// 移除最大生命值增加效果（通过activeModifiers实现）
    /// </summary>
    private void RemoveMaxHealthBoostEffect(ActiveBuff buff)
    {
        // 通过sourceId移除所有相关的修改器
        attributes.RemoveModifiersBySource(GetBuffSourceId(buff));

        LogManager.Log($"[BuffSystem] 移除生命上限增加，最终生命上限: {attributes.FinalMaxHealth}");
    }

    /// <summary>
    /// 应用最大能量增加效果（通过activeModifiers实现）
    /// </summary>
    private void ApplyMaxEnergyBoostEffect(ActiveBuff buff)
    {
        float maxEnergyAmount = buff.data.GetParameterValue("value");
        float maxEnergyPercent = buff.data.GetParameterValue("percent");

        // 通过activeModifiers添加固定值加成
        if (maxEnergyAmount > 0)
        {
            var modifier = new AttributeModifierInstance
            {
                attributeType = AttributeType.MaxEnergy,
                modifierType = ModifierType.Flat,
                value = maxEnergyAmount * buff.currentStacks,
                remainingDuration = buff.remainingDuration,
                sourceId = GetBuffSourceId(buff),
                stacks = buff.currentStacks
            };
            attributes.AddModifier(modifier);
        }

        // 通过activeModifiers添加百分比加成
        if (maxEnergyPercent > 0)
        {
            var modifier = new AttributeModifierInstance
            {
                attributeType = AttributeType.MaxEnergy,
                modifierType = ModifierType.Percent,
                value = maxEnergyPercent * buff.currentStacks,
                remainingDuration = buff.remainingDuration,
                sourceId = GetBuffSourceId(buff),
                stacks = buff.currentStacks
            };
            attributes.AddModifier(modifier);
        }

        LogManager.Log($"[BuffSystem] 增加能量上限，最终能量上限: {attributes.FinalMaxEnergy}");
    }

    /// <summary>
    /// 移除最大能量增加效果（通过activeModifiers实现）
    /// </summary>
    private void RemoveMaxEnergyBoostEffect(ActiveBuff buff)
    {
        // 通过sourceId移除所有相关的修改器
        attributes.RemoveModifiersBySource(GetBuffSourceId(buff));

        LogManager.Log($"[BuffSystem] 移除能量上限增加，最终能量上限: {attributes.FinalMaxEnergy}");
    }

    /// <summary>
    /// 应用顿帧效果
    /// 通过降低Time.timeScale来制造打击感
    /// 注意: 这会影响整个游戏的时间流速，包括物理、动画等所有基于时间的系统
    /// </summary>
    private void ApplyHitStopEffect(ActiveBuff buff)
    {
        // 获取时间缩放参数（默认为0.1，表示时间流速降低到10%）
        float timeScale = buff.data.GetParameterValue("timeScale");
        float priority = buff.data.GetParameterValue("priority");

        // 如果没有设置timeScale参数，使用默认值0.1
        if (timeScale == 0)
        {
            timeScale = 0.1f;
        }

        AdvancedHitStop.Instance.TriggerHitStop(buff.data.duration, timeScale, priority);

        LogManager.Log($"[BuffSystem] 应用顿帧效果，时间缩放: {timeScale}, 持续时间: {buff.data.duration}秒（真实时间）");
    }

    /// <summary>
    /// 移除顿帧效果
    /// 恢复Time.timeScale到原始值
    /// </summary>
    private void RemoveHitStopEffect(ActiveBuff buff)
    {
        // 恢复时间缩放到原始值
        Time.timeScale = originalTimeScale;

        LogManager.Log($"[BuffSystem] 移除顿帧效果，恢复时间缩放: {originalTimeScale}");
    }

    /// <summary>
    /// 应用镜头抖动效果
    /// 通过CinemachineImpulseSource触发镜头震动
    /// </summary>
    private void ApplyCameraShakeEffect(ActiveBuff buff)
    {
        float force = buff.data.GetParameterValue("force");
        if (force == 0)
        {
            force = 1.0f;
        }

        float priority = buff.data.GetParameterValue("priority");

        float velocityX = buff.data.GetParameterValue("velocityX");
        float velocityY = buff.data.GetParameterValue("velocityY");
        float velocityZ = buff.data.GetParameterValue("velocityZ");

        float duration = buff.data.duration > 0 ? buff.data.duration : 0.2f;

        if (velocityX != 0 || velocityY != 0 || velocityZ != 0)
        {
            Vector3 velocity = new Vector3(velocityX, velocityY, velocityZ);
            CameraShakeManager.Instance.TriggerCameraShake(force, velocity, priority, duration);
            LogManager.Log($"[BuffSystem] 应用镜头抖动效果，力度: {force}, 方向: {velocity}, 优先级: {priority}");
        }
        else
        {
            CameraShakeManager.Instance.TriggerCameraShake(force, priority, duration);
            LogManager.Log($"[BuffSystem] 应用镜头抖动效果，力度: {force}, 优先级: {priority}");
        }
    }

    /// <summary>
    /// 应用额外跳跃次数效果
    /// 增加角色的空中跳跃次数
    /// </summary>
    private void ApplyExtraJumpEffect(ActiveBuff buff)
    {
        int extraJumpCount = Mathf.RoundToInt(buff.data.GetParameterValue("value")) * buff.currentStacks;

        if (characterBase is CharacterLogic characterLogic)
        {
            characterLogic.AddExtraJumps(extraJumpCount);
            LogManager.Log($"[BuffSystem] 增加额外跳跃次数: {extraJumpCount}");
        }
        else
        {
            LogManager.LogWarning($"[BuffSystem] 无法应用额外跳跃效果: 目标不是CharacterLogic类型");
        }
    }

    /// <summary>
    /// 移除额外跳跃次数效果
    /// 减少角色的空中跳跃次数
    /// </summary>
    private void RemoveExtraJumpEffect(ActiveBuff buff)
    {
        int extraJumpCount = Mathf.RoundToInt(buff.data.GetParameterValue("value")) * buff.currentStacks;

        if (characterBase is CharacterLogic characterLogic)
        {
            characterLogic.RemoveExtraJumps(extraJumpCount);
            LogManager.Log($"[BuffSystem] 移除额外跳跃次数: {extraJumpCount}");
        }
    }

    /// <summary>
    /// 应用瞬移效果
    /// 瞬间移动到目标身后或身前
    /// 瞬移第一种情况,攻击者给自己施加瞬移Buff,则瞬移到target身后或身前  source是攻击者,target是目标
    /// 第二种,攻击者给命中者施加瞬移Buff,则命中者瞬移到攻击者身后或身前  source是攻击者,target是命中者
    /// </summary>
    private void ApplyTeleportEffect(ActiveBuff buff)
    {
        if (buff.source == null)
        {
            LogManager.LogWarning($"[BuffSystem] 无法应用瞬移效果: 没有目标对象");
            return;
        }

        float offsetX = buff.data.GetParameterValue("offsetX");
        float offsetY = buff.data.GetParameterValue("offsetY");

        if (offsetX == 0)
        {
            offsetX = 2f;
        }

        Transform targetTransform = null;
        bool targetFacingRight = true;

        if (buff.data.effectTarget == EffectTarget.Attacker)//攻击者给自己施加瞬移Buff
        {
            if (buff.target)
            {
                targetTransform = buff.target.transform;
                targetFacingRight = buff.target.isFacingRight;
            }
            else
            {
                targetTransform = buff.source.transform;
                targetFacingRight = buff.source.isFacingRight;
            }
        }
        else//攻击者给命中者施加瞬移Buff
        {
            targetTransform = buff.source.transform;
            targetFacingRight = buff.source.isFacingRight;
        }

        if (targetTransform == null)
        {
            LogManager.LogWarning($"[BuffSystem] 无法应用瞬移效果: 目标对象不存在");
            return;
        }

        Vector3 targetPosition = targetTransform.position;

        float actualOffsetX = targetFacingRight ? -offsetX : offsetX;

        Vector3 teleportPosition = new Vector3(
            targetPosition.x + actualOffsetX,
            targetPosition.y + offsetY,
            targetPosition.z
        );

        if (buff.data.effectTarget == EffectTarget.Attacker)//攻击者给自己施加瞬移Buff
        {
            buff.source.transform.position = teleportPosition;

            if (buff.source.rb != null)
            {
                buff.source.rb.linearVelocity = Vector2.zero;
            }

            LogManager.Log($"[BuffSystem] 攻击者瞬移到目标位置: {teleportPosition}");
        }
        else//攻击者给命中者施加瞬移Buff
        {
            if (buff.target != null)
            {
                buff.target.transform.position = teleportPosition;//命中者瞬移到攻击者身后或身前位置
                if (buff.target.rb != null)
                {
                    buff.target.rb.linearVelocity = Vector2.zero;
                }
            }
            else//容错处理,如果没有命中者,则改为自己瞬移
            {
                characterBase.transform.position = teleportPosition;

                if (rd != null)
                {
                    rd.linearVelocity = Vector2.zero;
                }
            }



            LogManager.Log($"[BuffSystem] 被击中者瞬移到目标位置: {teleportPosition}");
        }
    }

    #endregion

    #region 辅助方法

    /// <summary>
    /// 生成Buff的唯一来源ID
    /// 用于在属性修改器中标识Buff来源，方便移除时定位
    /// </summary>
    /// <param name="buff">Buff实例</param>
    /// <returns>唯一的来源ID字符串</returns>
    private string GetBuffSourceId(ActiveBuff buff)
    {
        // 使用Buff数据的名称和实例的哈希码生成唯一ID
        return $"{buff.data.effectName}_{buff.GetHashCode()}";
    }

    #endregion

    /// <summary>
    /// 应用周期性效果（如持续伤害、持续回复等）
    /// </summary>
    private void ApplyPeriodicEffects(float deltaTime)
    {
        for (int i = activeBuffs.Count - 1; i >= 0; i--)
        {
            // 检查索引是否有效（防止在循环过程中列表被修改）
            if (i >= activeBuffs.Count) continue;

            var buff = activeBuffs[i];

            if (buff == null || !activeBuffs.Contains(buff)) continue;

            // 检查角色是否已经死亡
            if (attributes != null && attributes.currentHealth <= 0)
                break;

            switch (buff.data.category)
            {
                //case EffectCategory.Burn:
                //    // 燃烧效果 - 每秒造成伤害
                //    ApplyBurnDamage(buff, deltaTime);
                //    break;

                //case EffectCategory.Poison:
                //    // 中毒效果 - 每秒造成伤害
                //    ApplyPoisonDamage(buff, deltaTime);
                //    break;

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
    /// 应用Buff伤害
    /// 根据伤害计算类型，使用DamageCalculator计算伤害或直接扣除生命值
    /// </summary>
    private void ApplyBuffDamage(ActiveBuff buff, float damageValue, string damageTypeName)
    {
        if (buff.data.damageCalcType == EDamageCalcType.Fixed)
        {
            attributes.ChangeHealth(-damageValue, buff.source);
            DamageTextManager.Instance.ShowDamageText(damageValue, DamageTextType.Normal, transform.position);
            LogManager.Log($"[BuffSystem] {damageTypeName}伤害(固定): {damageValue:F1}，剩余生命: {attributes.currentHealth}");
        }
        else
        {
            CharacterAttributes sourceAttributes = null;
            BuffSystem sourceBuffSystem = null;

            if (buff.source != null)
            {
                sourceAttributes = buff.source_CharacterAttributes;
                sourceBuffSystem = buff.source_BuffSystem;
            }

            var damageInfo = new DamageInfo
            {
                baseDamage = damageValue
            };

            var damageResult = DamageCalculator.CalculateDamage(damageInfo, buff.source, characterBase);

            if (!damageResult.isMiss)
            {
                attributes.ChangeHealth(-damageResult.healthDamage, buff.source);
                DamageDisplayHelper.ShowDamageOnCharacter(damageResult, transform);
                LogManager.Log($"[BuffSystem] {damageTypeName}伤害(普通): {damageResult.finalDamage:F1}(实际生命伤害: {damageResult.healthDamage:F1})，剩余生命: {attributes.currentHealth}");
            }
        }
    }

    /// <summary>
    /// 应用燃烧伤害
    /// 每秒造成持续伤害，伤害会随堆叠层数增加
    /// </summary>
    private void ApplyBurnDamage(ActiveBuff buff, float deltaTime)
    {
        float damagePerTick = buff.data.GetParameterValue("value") * buff.currentStacks;
        buff.accumulatedTime += deltaTime;

        float tickInterval = buff.data.tickInterval > 0 ? buff.data.tickInterval : 1f;
        if (buff.accumulatedTime >= tickInterval)
        {
            ApplyBuffDamage(buff, damagePerTick, "燃烧");
            buff.accumulatedTime -= tickInterval;
        }
    }

    /// <summary>
    /// 应用中毒伤害
    /// 每秒造成持续伤害，伤害会随堆叠层数增加
    /// </summary>
    private void ApplyPoisonDamage(ActiveBuff buff, float deltaTime)
    {
        float damagePerTick = buff.data.GetParameterValue("value") * buff.currentStacks;
        buff.accumulatedTime += deltaTime;

        float tickInterval = buff.data.tickInterval > 0 ? buff.data.tickInterval : 1f;
        if (buff.accumulatedTime >= tickInterval)
        {
            ApplyBuffDamage(buff, damagePerTick, "中毒");
            buff.accumulatedTime -= tickInterval;
        }
    }

    /// <summary>
    /// 应用流血伤害
    /// 每秒造成持续伤害，伤害会随堆叠层数增加
    /// </summary>
    private void ApplyBleedDamage(ActiveBuff buff, float deltaTime)
    {
        float damagePerTick = buff.data.GetParameterValue("value") * buff.currentStacks;
        buff.accumulatedTime += deltaTime;

        float tickInterval = buff.data.tickInterval > 0 ? buff.data.tickInterval : 1f;
        if (buff.accumulatedTime >= tickInterval)
        {
            ApplyBuffDamage(buff, damagePerTick, "流血");
            buff.accumulatedTime -= tickInterval;
        }
    }

    /// <summary>
    /// 应用条件回复
    /// 每秒回复生命值
    /// </summary>
    private void ApplyConditionalHeal(ActiveBuff buff, float deltaTime)
    {
        float healPerTick = buff.data.GetParameterValue("value") * buff.currentStacks;
        buff.accumulatedTime += deltaTime;

        float tickInterval = buff.data.tickInterval > 0 ? buff.data.tickInterval : 1f;
        if (buff.accumulatedTime >= tickInterval)
        {
            attributes.ChangeHealth(healPerTick, buff.source);
            DamageDisplayHelper.ShowHealOnCharacter(healPerTick, transform);
            buff.accumulatedTime -= tickInterval;
            LogManager.Log($"[BuffSystem] 回复生命: {healPerTick}，当前生命: {attributes.currentHealth}");
        }
    }

    /// <summary>
    /// 应用能量消耗
    /// 每秒消耗能量值（需要能量系统支持）
    /// </summary>
    private void ApplyEnergyDrain(ActiveBuff buff, float deltaTime)
    {
        float drainPerTick = buff.data.GetParameterValue("value") * buff.currentStacks;
        buff.accumulatedTime += deltaTime;

        float tickInterval = buff.data.tickInterval > 0 ? buff.data.tickInterval : 1f;
        if (buff.accumulatedTime >= tickInterval)
        {
            attributes.ChangeEnergy(-drainPerTick);
            buff.accumulatedTime -= tickInterval;
            LogManager.Log($"[BuffSystem] 能量消耗: {drainPerTick:F1}");
        }
    }

    private void ApplyEnergyRegeneration(ActiveBuff buff, float deltaTime)
    {
        float energyPerTick = buff.data.GetParameterValue("value") * buff.currentStacks;
        buff.accumulatedTime += deltaTime;

        float tickInterval = buff.data.tickInterval > 0 ? buff.data.tickInterval : 1f;
        if (buff.accumulatedTime >= tickInterval)
        {
            attributes.ChangeEnergy(energyPerTick);
            buff.accumulatedTime -= tickInterval;
            LogManager.Log($"[BuffSystem] 回复能量: {energyPerTick}，当前能量: {attributes.currentEnergy}/{attributes.maxEnergy}");
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
    public CharacterBase source;            // Buff来源
    public CharacterAttributes source_CharacterAttributes;
    public BuffSystem source_BuffSystem;
    public CharacterBase target;            // Buff目标
    public float accumulatedTime;        // 累积时间（用于周期性效果）
    public bool isPermanent;             // 是否为永久效果
}
