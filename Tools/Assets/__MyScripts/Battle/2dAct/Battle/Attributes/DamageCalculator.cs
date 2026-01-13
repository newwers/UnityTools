using UnityEngine;

public class DamageCalculator
{
    public static DamageResult CalculateDamage(
        DamageInfo damageInfo,
        CharacterBase attackerCharacterBase,
        CharacterBase targetCharacterBase)
    {
        CharacterAttributes attackerAttribute = attackerCharacterBase.PlayerAttributes.characterAtttibute;
        BuffSystem attackerBuffs = attackerCharacterBase.BuffSystem;
        CharacterAttributes targetAttribute = targetCharacterBase.PlayerAttributes.characterAtttibute;
        BuffSystem targetBuffs = targetCharacterBase.BuffSystem;

        var result = new DamageResult();

        if (targetBuffs != null && (targetBuffs.HasBuff(EffectCategory.Invincible) || targetAttribute.isDodging))
        {
            result.isMiss = true;
            LogManager.Log("[DamageCalculator] 攻击被闪避/无敌状态免疫");
            return result;
        }

        float baseDamage = damageInfo.baseDamage;

        // 添加附加伤害
        baseDamage += attackerAttribute.FinalAttackDamage;

        // 使用最终力量（包含所有buff加成）计算伤害
        baseDamage *= (1f + attackerAttribute.FinalStrength / 100f);

        bool isCrit = CheckCritical(attackerAttribute, damageInfo, attackerBuffs);
        if (isCrit)
        {
            // 使用最终暴击倍率（包含所有buff加成）
            baseDamage *= attackerAttribute.FinalCritMultiplier;
            result.isCritical = true;
        }

        float damageReduction = CalculateDamageReduction(targetBuffs);
        float damageAmplification = CalculateDamageAmplification(attackerBuffs);

        baseDamage *= (1f - damageReduction / 100f);
        baseDamage *= (1f + damageAmplification / 100f);

        //格挡
        if (targetAttribute.isBlocking)
        {
            if (targetAttribute.currentBlockValue >= damageInfo.breakPower)
            {
                result.isBlocked = true;
                result.finalDamage = 0f;
                LogManager.Log($"[DamageCalculator] 攻击被格挡 (格挡值: {targetAttribute.currentBlockValue}, 破防力: {damageInfo.breakPower})");
                return result;
            }
            else
            {
                LogManager.Log($"[DamageCalculator] 破防成功 (格挡值: {targetAttribute.currentBlockValue}, 破防力: {damageInfo.breakPower})");
            }
        }

        //防御计算
        baseDamage = CalculateDamageDefense(baseDamage, targetAttribute.FinalDefense);

        float remainingDamage = baseDamage;
        //护盾
        if (targetAttribute.currentShield > 0)
        {
            float shieldDamage = Mathf.Min(targetAttribute.currentShield, remainingDamage);
            targetAttribute.ChangeShield(-shieldDamage);
            remainingDamage -= shieldDamage;
            result.shieldDamage = shieldDamage;
            LogManager.Log($"[DamageCalculator] 护盾吸收伤害: {shieldDamage}, 剩余护盾: {targetAttribute.currentShield}");
        }


        result.healthDamage = remainingDamage;
        result.finalDamage = baseDamage;
        //震击值
        if (damageInfo.staggerDamage > 0)
        {
            targetAttribute.AddStagger(damageInfo.staggerDamage);
            LogManager.Log($"[DamageCalculator] 造成震击: {damageInfo.staggerDamage}, 当前震击值: {targetAttribute.currentStagger}");
        }

        HandleDamageReflect(attackerCharacterBase, targetCharacterBase, result.finalDamage);

        HandleLifeSteal(attackerAttribute, damageInfo, result.finalDamage, attackerBuffs);

        return result;
    }

    /// <summary>
    /// 检查是否触发暴击
    /// </summary>
    private static bool CheckCritical(CharacterAttributes attacker, DamageInfo damageInfo, BuffSystem attackerBuffs)
    {
        if (damageInfo.isGuaranteedCrit)
        {
            return true;
        }

        if (attackerBuffs != null && attackerBuffs.HasBuff(EffectCategory.GuaranteedCrit))
        {
            return true;
        }

        // 使用最终暴击率（包含所有buff加成）
        float roll = Random.Range(0f, 100f);
        return roll < attacker.FinalCritRate;
    }
    /// <summary>
    /// 伤害减免计算
    /// </summary>
    /// <param name="targetBuffs"></param>
    /// <returns></returns>
    private static float CalculateDamageReduction(BuffSystem targetBuffs)
    {
        float reduction = 0f;

        if (targetBuffs != null)
        {
            foreach (var buff in targetBuffs.GetActiveBuffs())
            {
                if (buff.data.category == EffectCategory.DamageReduction)
                {
                    reduction += buff.data.GetParameterValue("percent") * buff.currentStacks;
                }
            }
        }

        return reduction;
    }
    /// <summary>
    /// 伤害增幅计算
    /// </summary>
    /// <param name="attackerBuffs"></param>
    /// <returns></returns>
    private static float CalculateDamageAmplification(BuffSystem attackerBuffs)
    {
        float amplification = 0f;

        if (attackerBuffs != null)
        {
            foreach (var buff in attackerBuffs.GetActiveBuffs())
            {
                if (buff.data.category == EffectCategory.DamageAmplification)
                {
                    amplification += buff.data.GetParameterValue("percent") * buff.currentStacks;
                }
            }
        }

        return amplification;
    }
    /// <summary>
    /// 伤害防御计算
    /// 防御高于攻击力时,强制伤害为1点,否则有多少防御力就抵消多少伤害
    /// </summary>
    /// <param name="attackerBuffs"></param>
    /// <returns></returns>
    private static float CalculateDamageDefense(float damage, float finalDefense)
    {
        float finalValue = damage;

        finalValue = Mathf.Max(damage - finalDefense, 1);

        return finalValue;
    }
    /// <summary>
    /// 伤害反射
    /// </summary>
    /// <param name="attacker"></param>
    /// <param name="targetBuffs"></param>
    /// <param name="damage"></param>
    private static void HandleDamageReflect(CharacterBase attackerCharacterBase,
        CharacterBase targetCharacterBase, float damage)
    {
        CharacterAttributes attacker = attackerCharacterBase.PlayerAttributes.characterAtttibute; ;

        BuffSystem attackerBuffs = attackerCharacterBase.BuffSystem;

        BuffSystem targetBuffs = targetCharacterBase.BuffSystem;

        if (targetBuffs == null) return;

        foreach (var buff in targetBuffs.GetActiveBuffs())
        {
            if (buff.data.category == EffectCategory.DamageReflect)
            {
                float reflectPercent = buff.data.GetParameterValue("percent");
                float reflectValue = buff.data.GetParameterValue("value");
                float reflectDamage = 0;
                if (reflectValue > 0)//先按固定值反弹
                {
                    reflectDamage = reflectValue;
                }
                if (reflectPercent > 0)//再按百分比反弹
                {
                    reflectDamage = damage * (reflectPercent / 100f);
                }

                attacker.ChangeHealth(-reflectDamage, targetCharacterBase);
                DamageDisplayHelper.ShowDamageOnCharacter(new DamageResult { finalDamage = reflectDamage }, attackerBuffs.transform);
                LogManager.Log($"[DamageCalculator] 伤害反弹: {reflectDamage} ({reflectPercent}%)");
            }
        }
    }
    /// <summary>
    /// 生命偷取
    /// </summary>
    /// <param name="attacker"></param>
    /// <param name="damageInfo"></param>
    /// <param name="damageDealt"></param>
    /// <param name="attackerBuffs"></param>
    private static void HandleLifeSteal(CharacterAttributes attacker, DamageInfo damageInfo, float damageDealt, BuffSystem attackerBuffs)
    {
        float totalLifeSteal = damageInfo.lifeStealPercent;

        if (attackerBuffs != null)
        {
            foreach (var buff in attackerBuffs.GetActiveBuffs())
            {
                if (buff.data.category == EffectCategory.LifeSteal)
                {
                    totalLifeSteal += buff.data.GetParameterValue("percent");
                }
            }
        }

        if (totalLifeSteal > 0)
        {
            float healAmount = damageDealt * (totalLifeSteal / 100f);
            attacker.ChangeHealth(healAmount, damageInfo.attacker);
            DamageDisplayHelper.ShowHealOnCharacter(healAmount, damageInfo.attacker.transform);
            LogManager.Log($"[DamageCalculator] 生命偷取: {healAmount} ({totalLifeSteal}%)");
        }
    }
    /// <summary>
    /// 判断是否应该播放受击动画
    /// </summary>
    /// <param name="priority">攻击的优先级</param>
    /// <returns>如果应该播放受击动画返回true</returns>
    public static bool ShouldPlayHitAnimation(int priority, CharacterAttributes characterAttributes)
    {
        return characterAttributes.hasSuperArmor == false;//非霸体时才播放受击动画
        //return priority > HurtPriority && currentActionPriority < priority;//只有当攻击优先级高于硬直优先级且当前动作优先级低于攻击优先级时才播放受击动画
    }
}

/// <summary>
/// 伤害结果
/// </summary>
public class DamageResult
{
    /// <summary>
    /// 本次攻击造成总伤害(不计算护盾)
    /// </summary>
    public float finalDamage;
    /// <summary>
    /// 本次攻击实际扣除血量伤害
    /// </summary>
    public float healthDamage;
    /// <summary>
    /// 本次护盾吸收伤害
    /// </summary>
    public float shieldDamage;
    public bool isCritical;
    public bool isBlocked;
    public bool isMiss;
}
