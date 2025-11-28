using UnityEngine;

public class DamageCalculator
{
    public static DamageResult CalculateDamage(
        DamageInfo damageInfo,
        CharacterAttributes attacker,
        CharacterAttributes target,
        BuffSystem attackerBuffs = null,
        BuffSystem targetBuffs = null)
    {
        var result = new DamageResult();

        if (targetBuffs != null && (targetBuffs.HasBuff(EffectCategory.Invincible) || target.isDodging))
        {
            result.isMiss = true;
            LogManager.Log("[DamageCalculator] 攻击被闪避/无敌状态免疫");
            return result;
        }

        float baseDamage = damageInfo.baseDamage;

        baseDamage *= (1f + attacker.strength / 100f);// 力量增加伤害,1点力量增加1%伤害

        bool isCrit = CheckCritical(attacker, damageInfo, attackerBuffs);
        if (isCrit)
        {
            baseDamage *= attacker.critMultiplier;
            result.isCritical = true;
        }

        float damageReduction = CalculateDamageReduction(targetBuffs);
        float damageAmplification = CalculateDamageAmplification(attackerBuffs);

        baseDamage *= (1f - damageReduction / 100f);
        baseDamage *= (1f + damageAmplification / 100f);

        if (target.isBlocking)
        {
            if (target.currentBlockValue >= damageInfo.breakPower)
            {
                result.isBlocked = true;
                result.finalDamage = 0f;
                LogManager.Log($"[DamageCalculator] 攻击被格挡 (格挡值: {target.currentBlockValue}, 破防力: {damageInfo.breakPower})");
                return result;
            }
            else
            {
                LogManager.Log($"[DamageCalculator] 破防成功 (格挡值: {target.currentBlockValue}, 破防力: {damageInfo.breakPower})");
            }
        }

        float remainingDamage = baseDamage;

        if (target.currentShield > 0)
        {
            float shieldDamage = Mathf.Min(target.currentShield, remainingDamage);
            target.ChangeShield(-shieldDamage);
            remainingDamage -= shieldDamage;
            result.shieldDamage = shieldDamage;
            LogManager.Log($"[DamageCalculator] 护盾吸收伤害: {shieldDamage}, 剩余护盾: {target.currentShield}");
        }

        result.healthDamage = remainingDamage;
        result.finalDamage = baseDamage;

        if (damageInfo.staggerDamage > 0)
        {
            target.AddStagger(damageInfo.staggerDamage);
            LogManager.Log($"[DamageCalculator] 造成震击伤害: {damageInfo.staggerDamage}, 当前震击值: {target.currentStagger}");
        }

        HandleDamageReflect(attacker, targetBuffs, result.finalDamage);

        HandleLifeSteal(attacker, damageInfo, result.finalDamage, attackerBuffs);

        return result;
    }

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

        float roll = Random.Range(0f, 100f);
        return roll < attacker.critRate;
    }

    private static float CalculateDamageReduction(BuffSystem targetBuffs)
    {
        float reduction = 0f;

        if (targetBuffs != null)
        {
            foreach (var buff in targetBuffs.GetActiveBuffs())
            {
                if (buff.data.category == EffectCategory.DamageReduction)
                {
                    reduction += buff.data.GetParameterValue("reductionPercent") * buff.currentStacks;
                }
            }
        }

        return reduction;
    }

    private static float CalculateDamageAmplification(BuffSystem attackerBuffs)
    {
        float amplification = 0f;

        if (attackerBuffs != null)
        {
            foreach (var buff in attackerBuffs.GetActiveBuffs())
            {
                if (buff.data.category == EffectCategory.DamageAmplification)
                {
                    amplification += buff.data.GetParameterValue("amplificationPercent") * buff.currentStacks;
                }
            }
        }

        return amplification;
    }

    private static void HandleDamageReflect(CharacterAttributes attacker, BuffSystem targetBuffs, float damage)
    {
        if (targetBuffs == null) return;

        foreach (var buff in targetBuffs.GetActiveBuffs())
        {
            if (buff.data.category == EffectCategory.DamageReflect)
            {
                float reflectPercent = buff.data.GetParameterValue("reflectPercent");
                float reflectDamage = damage * (reflectPercent / 100f);
                attacker.ChangeHealth(-reflectDamage);
                LogManager.Log($"[DamageCalculator] 伤害反弹: {reflectDamage} ({reflectPercent}%)");
            }
        }
    }

    private static void HandleLifeSteal(CharacterAttributes attacker, DamageInfo damageInfo, float damageDealt, BuffSystem attackerBuffs)
    {
        float totalLifeSteal = damageInfo.lifeStealPercent;

        if (attackerBuffs != null)
        {
            foreach (var buff in attackerBuffs.GetActiveBuffs())
            {
                if (buff.data.category == EffectCategory.LifeSteal)
                {
                    totalLifeSteal += buff.data.GetParameterValue("lifeStealPercent");
                }
            }
        }

        if (totalLifeSteal > 0)
        {
            float healAmount = damageDealt * (totalLifeSteal / 100f);
            attacker.ChangeHealth(healAmount);
            LogManager.Log($"[DamageCalculator] 生命偷取: {healAmount} ({totalLifeSteal}%)");
        }
    }
}


public class DamageResult
{
    public float finalDamage;
    public float healthDamage;
    public float shieldDamage;
    public bool isCritical;
    public bool isBlocked;
    public bool isMiss;
}
