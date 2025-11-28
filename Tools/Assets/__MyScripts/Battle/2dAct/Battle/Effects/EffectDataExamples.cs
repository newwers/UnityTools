using UnityEngine;

public class EffectDataExamples : MonoBehaviour
{
    public static EffectData CreateStunEffect(float duration)
    {
        var effect = ScriptableObject.CreateInstance<EffectData>();
        effect.effectName = "眩晕";
        effect.category = EffectCategory.Stun;
        effect.duration = duration;
        effect.isPermanent = false;
        effect.canStack = false;
        return effect;
    }

    public static EffectData CreateInvincibleEffect(float duration)
    {
        var effect = ScriptableObject.CreateInstance<EffectData>();
        effect.effectName = "无敌";
        effect.category = EffectCategory.Invincible;
        effect.duration = duration;
        effect.isPermanent = false;
        effect.canStack = false;
        return effect;
    }

    public static EffectData CreateSuperArmorEffect(float duration)
    {
        var effect = ScriptableObject.CreateInstance<EffectData>();
        effect.effectName = "霸体";
        effect.category = EffectCategory.SuperArmor;
        effect.duration = duration;
        effect.isPermanent = false;
        effect.canStack = false;
        return effect;
    }

    public static EffectData CreateDamageReductionEffect(float duration, float reductionPercent)
    {
        var effect = ScriptableObject.CreateInstance<EffectData>();
        effect.effectName = "减伤";
        effect.category = EffectCategory.DamageReduction;
        effect.duration = duration;
        effect.isPermanent = false;
        effect.canStack = true;
        effect.maxStacks = 5;
        effect.stackBehavior = StackBehavior.RefreshDuration;
        
        effect.parameters.Add(new EffectParameter 
        { 
            name = "reductionPercent", 
            value = reductionPercent,
            description = "伤害减免百分比"
        });
        
        return effect;
    }

    public static EffectData CreateDamageAmplificationEffect(float duration, float amplificationPercent)
    {
        var effect = ScriptableObject.CreateInstance<EffectData>();
        effect.effectName = "增伤";
        effect.category = EffectCategory.DamageAmplification;
        effect.duration = duration;
        effect.isPermanent = false;
        effect.canStack = true;
        effect.maxStacks = 5;
        effect.stackBehavior = StackBehavior.RefreshDuration;
        
        effect.parameters.Add(new EffectParameter 
        { 
            name = "amplificationPercent", 
            value = amplificationPercent,
            description = "伤害增幅百分比"
        });
        
        return effect;
    }

    public static EffectData CreateShieldEffect(float duration, float shieldAmount)
    {
        var effect = ScriptableObject.CreateInstance<EffectData>();
        effect.effectName = "护盾";
        effect.category = EffectCategory.Shield;
        effect.duration = duration;
        effect.isPermanent = false;
        effect.canStack = true;
        effect.maxStacks = 3;
        effect.stackBehavior = StackBehavior.IncreaseValue;
        
        effect.parameters.Add(new EffectParameter 
        { 
            name = "shieldAmount", 
            value = shieldAmount,
            description = "护盾值"
        });
        
        return effect;
    }

    public static EffectData CreateSlowEffect(float duration, float slowPercent)
    {
        var effect = ScriptableObject.CreateInstance<EffectData>();
        effect.effectName = "减速";
        effect.category = EffectCategory.Slow;
        effect.duration = duration;
        effect.isPermanent = false;
        effect.canStack = true;
        effect.maxStacks = 3;
        effect.stackBehavior = StackBehavior.RefreshDuration;
        
        effect.parameters.Add(new EffectParameter 
        { 
            name = "slowPercent", 
            value = slowPercent,
            description = "减速百分比"
        });
        
        return effect;
    }

    public static EffectData CreateHasteEffect(float duration, float hastePercent)
    {
        var effect = ScriptableObject.CreateInstance<EffectData>();
        effect.effectName = "加速";
        effect.category = EffectCategory.Haste;
        effect.duration = duration;
        effect.isPermanent = false;
        effect.canStack = true;
        effect.maxStacks = 3;
        effect.stackBehavior = StackBehavior.RefreshDuration;
        
        effect.parameters.Add(new EffectParameter 
        { 
            name = "hastePercent", 
            value = hastePercent,
            description = "加速百分比"
        });
        
        return effect;
    }

    public static EffectData CreateDamageReflectEffect(float duration, float reflectPercent)
    {
        var effect = ScriptableObject.CreateInstance<EffectData>();
        effect.effectName = "伤害反弹";
        effect.category = EffectCategory.DamageReflect;
        effect.duration = duration;
        effect.isPermanent = false;
        effect.canStack = false;
        
        effect.parameters.Add(new EffectParameter 
        { 
            name = "reflectPercent", 
            value = reflectPercent,
            description = "反弹伤害百分比"
        });
        
        return effect;
    }

    public static EffectData CreateLifeStealEffect(float duration, float lifeStealPercent)
    {
        var effect = ScriptableObject.CreateInstance<EffectData>();
        effect.effectName = "生命偷取";
        effect.category = EffectCategory.LifeSteal;
        effect.duration = duration;
        effect.isPermanent = false;
        effect.canStack = false;
        
        effect.parameters.Add(new EffectParameter 
        { 
            name = "lifeStealPercent", 
            value = lifeStealPercent,
            description = "生命偷取百分比"
        });
        
        return effect;
    }

    public static EffectData CreateInstantHealEffect(float healAmount)
    {
        var effect = ScriptableObject.CreateInstance<EffectData>();
        effect.effectName = "立即回血";
        effect.category = EffectCategory.InstantHeal;
        effect.duration = 0f;
        effect.isPermanent = false;
        effect.canStack = false;
        
        effect.parameters.Add(new EffectParameter 
        { 
            name = "healAmount", 
            value = healAmount,
            description = "回复生命值"
        });
        
        return effect;
    }

    public static EffectData CreateInstantEnergyRestoreEffect(float energyAmount)
    {
        var effect = ScriptableObject.CreateInstance<EffectData>();
        effect.effectName = "立即回复能量";
        effect.category = EffectCategory.InstantEnergyRestore;
        effect.duration = 0f;
        effect.isPermanent = false;
        effect.canStack = false;
        
        effect.parameters.Add(new EffectParameter 
        { 
            name = "energyAmount", 
            value = energyAmount,
            description = "回复能量值"
        });
        
        return effect;
    }

    public static EffectData CreateEnergyRegenerationEffect(float duration, float energyPerSecond)
    {
        var effect = ScriptableObject.CreateInstance<EffectData>();
        effect.effectName = "持续回复能量";
        effect.category = EffectCategory.EnergyRegeneration;
        effect.duration = duration;
        effect.isPermanent = false;
        effect.canStack = true;
        effect.maxStacks = 3;
        effect.stackBehavior = StackBehavior.RefreshDuration;
        
        effect.parameters.Add(new EffectParameter 
        { 
            name = "energyPerSecond", 
            value = energyPerSecond,
            description = "每秒回复能量值"
        });
        
        return effect;
    }

    public static EffectData CreateStrengthBoostEffect(float duration, float strengthAmount = 0f, float strengthPercent = 0f)
    {
        var effect = ScriptableObject.CreateInstance<EffectData>();
        effect.effectName = "力量增强";
        effect.category = EffectCategory.StrengthBoost;
        effect.duration = duration;
        effect.isPermanent = false;
        effect.canStack = true;
        effect.maxStacks = 5;
        effect.stackBehavior = StackBehavior.RefreshDuration;
        
        if (strengthAmount > 0)
        {
            effect.parameters.Add(new EffectParameter 
            { 
                name = "strengthAmount", 
                value = strengthAmount,
                description = "力量增加固定值"
            });
        }
        
        if (strengthPercent > 0)
        {
            effect.parameters.Add(new EffectParameter 
            { 
                name = "strengthPercent", 
                value = strengthPercent,
                description = "力量增加百分比"
            });
        }
        
        return effect;
    }

    public static EffectData CreateSpeedBoostEffect(float duration, float speedAmount = 0f, float speedPercent = 0f)
    {
        var effect = ScriptableObject.CreateInstance<EffectData>();
        effect.effectName = "移速增强";
        effect.category = EffectCategory.SpeedBoost;
        effect.duration = duration;
        effect.isPermanent = false;
        effect.canStack = true;
        effect.maxStacks = 3;
        effect.stackBehavior = StackBehavior.RefreshDuration;
        
        if (speedAmount > 0)
        {
            effect.parameters.Add(new EffectParameter 
            { 
                name = "speedAmount", 
                value = speedAmount,
                description = "移速增加固定值"
            });
        }
        
        if (speedPercent > 0)
        {
            effect.parameters.Add(new EffectParameter 
            { 
                name = "speedPercent", 
                value = speedPercent,
                description = "移速增加百分比"
            });
        }
        
        return effect;
    }

    public static EffectData CreateAttackBoostEffect(float duration, float attackAmount = 0f, float attackPercent = 0f)
    {
        var effect = ScriptableObject.CreateInstance<EffectData>();
        effect.effectName = "攻击增强";
        effect.category = EffectCategory.AttackBoost;
        effect.duration = duration;
        effect.isPermanent = false;
        effect.canStack = true;
        effect.maxStacks = 5;
        effect.stackBehavior = StackBehavior.RefreshDuration;
        
        if (attackAmount > 0)
        {
            effect.parameters.Add(new EffectParameter 
            { 
                name = "attackAmount", 
                value = attackAmount,
                description = "攻击增加固定值"
            });
        }
        
        if (attackPercent > 0)
        {
            effect.parameters.Add(new EffectParameter 
            { 
                name = "attackPercent", 
                value = attackPercent,
                description = "攻击增加百分比"
            });
        }
        
        return effect;
    }

    public static EffectData CreateDefenseBoostEffect(float duration, float defenseAmount = 0f, float defensePercent = 0f)
    {
        var effect = ScriptableObject.CreateInstance<EffectData>();
        effect.effectName = "防御增强";
        effect.category = EffectCategory.DefenseBoost;
        effect.duration = duration;
        effect.isPermanent = false;
        effect.canStack = true;
        effect.maxStacks = 5;
        effect.stackBehavior = StackBehavior.RefreshDuration;
        
        if (defenseAmount > 0)
        {
            effect.parameters.Add(new EffectParameter 
            { 
                name = "defenseAmount", 
                value = defenseAmount,
                description = "防御增加固定值"
            });
        }
        
        if (defensePercent > 0)
        {
            effect.parameters.Add(new EffectParameter 
            { 
                name = "defensePercent", 
                value = defensePercent,
                description = "防御增加百分比"
            });
        }
        
        return effect;
    }

    public static EffectData CreateCritRateBoostEffect(float duration, float critRatePercent)
    {
        var effect = ScriptableObject.CreateInstance<EffectData>();
        effect.effectName = "暴击率增强";
        effect.category = EffectCategory.CritRateBoost;
        effect.duration = duration;
        effect.isPermanent = false;
        effect.canStack = true;
        effect.maxStacks = 3;
        effect.stackBehavior = StackBehavior.RefreshDuration;
        
        effect.parameters.Add(new EffectParameter 
        { 
            name = "critRatePercent", 
            value = critRatePercent,
            description = "暴击率增加百分比"
        });
        
        return effect;
    }

    public static EffectData CreateCritDamageBoostEffect(float duration, float critDamagePercent)
    {
        var effect = ScriptableObject.CreateInstance<EffectData>();
        effect.effectName = "暴击伤害增强";
        effect.category = EffectCategory.CritDamageBoost;
        effect.duration = duration;
        effect.isPermanent = false;
        effect.canStack = true;
        effect.maxStacks = 3;
        effect.stackBehavior = StackBehavior.RefreshDuration;
        
        effect.parameters.Add(new EffectParameter 
        { 
            name = "critDamagePercent", 
            value = critDamagePercent,
            description = "暴击伤害增加百分比"
        });
        
        return effect;
    }

    public static EffectData CreateMaxHealthBoostEffect(float duration, float maxHealthAmount = 0f, float maxHealthPercent = 0f)
    {
        var effect = ScriptableObject.CreateInstance<EffectData>();
        effect.effectName = "生命上限增强";
        effect.category = EffectCategory.MaxHealthBoost;
        effect.duration = duration;
        effect.isPermanent = false;
        effect.canStack = true;
        effect.maxStacks = 5;
        effect.stackBehavior = StackBehavior.RefreshDuration;
        
        if (maxHealthAmount > 0)
        {
            effect.parameters.Add(new EffectParameter 
            { 
                name = "maxHealthAmount", 
                value = maxHealthAmount,
                description = "生命上限增加固定值"
            });
        }
        
        if (maxHealthPercent > 0)
        {
            effect.parameters.Add(new EffectParameter 
            { 
                name = "maxHealthPercent", 
                value = maxHealthPercent,
                description = "生命上限增加百分比"
            });
        }
        
        return effect;
    }

    public static EffectData CreateMaxEnergyBoostEffect(float duration, float maxEnergyAmount = 0f, float maxEnergyPercent = 0f)
    {
        var effect = ScriptableObject.CreateInstance<EffectData>();
        effect.effectName = "能量上限增强";
        effect.category = EffectCategory.MaxEnergyBoost;
        effect.duration = duration;
        effect.isPermanent = false;
        effect.canStack = true;
        effect.maxStacks = 5;
        effect.stackBehavior = StackBehavior.RefreshDuration;
        
        if (maxEnergyAmount > 0)
        {
            effect.parameters.Add(new EffectParameter 
            { 
                name = "maxEnergyAmount", 
                value = maxEnergyAmount,
                description = "能量上限增加固定值"
            });
        }
        
        if (maxEnergyPercent > 0)
        {
            effect.parameters.Add(new EffectParameter 
            { 
                name = "maxEnergyPercent", 
                value = maxEnergyPercent,
                description = "能量上限增加百分比"
            });
        }
        
        return effect;
    }
}
