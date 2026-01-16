using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CharacterAttributes
{
    [Header("生命相关")]
    public float maxHealth = 100f;
    public float currentHealth = 100f;
    public float healthRegenRate = 1f;

    [Header("能量相关")]
    public float maxEnergy = 100f;
    public float currentEnergy = 100f;
    public float energyRegenRate = 10f;

    [Header("基础属性,力量,敏捷,移速速度")]
    /// <summary>
    /// 基础力量（不包含任何加成）
    /// 每100点力量增加100%伤害,1点力量增加1%伤害
    /// </summary>
    [SerializeField] private float baseStrength = 0f;
    /// <summary>
    /// 基础敏捷（不包含任何加成）
    /// </summary>
    [SerializeField] private float baseAgility = 0f;
    /// <summary>
    /// 基础移动速度（不包含任何加成）
    /// </summary>
    [SerializeField] private float baseMoveSpeed = 1f;

    /// <summary>
    /// 基础移动速度系数
    /// </summary>
    public float baseMoveSpeedMultiplier = 1f;


    [Header("暴击相关")]
    /// <summary>
    /// 基础暴击率（不包含任何加成）
    /// </summary>
    [SerializeField][Range(0, 100)] private float baseCritRate = 5f;
    /// <summary>
    /// 基础暴击倍率（不包含任何加成）
    /// </summary>
    [SerializeField] private float baseCritMultiplier = 1.5f;

    [Header("格挡相关")]
    public float maxBlockValue = 50f;
    public float currentBlockValue = 50f;
    public bool isBlocking = false;

    [Header("护盾相关")]
    public float maxShield = 0f;
    public float currentShield = 0f;

    [Header("闪避相关")]
    public int maxDodgeCount = 2;
    public int currentDodgeCount = 2;
    public float dodgeRegenInterval = 3f;
    private float dodgeRegenTimer = 0f;
    /// <summary>
    /// 闪避中
    /// </summary>
    public bool isDodging = false;

    [Header("震击相关")]
    public float maxStagger = 100f;
    public float currentStagger = 0f;
    public float staggerRecoveryRate = 20f;
    public float staggerStunThreshold = 100f;
    public bool isStaggered = false;

    [Header("攻击属性")]
    /// <summary>
    /// 基础附加伤害（不包含任何加成）
    /// </summary>
    public int baseAttackDamage = 0;

    [Header("战斗状态")]
    /// <summary>
    /// 霸体
    /// </summary>
    public bool hasSuperArmor = false;
    /// <summary>
    /// 无敌计数器，每标记一次无敌+1，取消无敌-1，当值>0时为无敌状态
    /// </summary>
    public int isInvincible = 0;
    /// <summary>
    /// 基础防御（不包含任何加成）
    /// </summary>
    public int baseDefense = 0;

    [Header("属性修改器")]
    /// <summary>
    /// 当前激活的所有属性修改器列表
    /// </summary>
    private List<AttributeModifierInstance> activeModifiers = new List<AttributeModifierInstance>();

    public event Action OnHealthChanged;
    public event Action OnEnergyChanged;
    public event Action OnShieldChanged;
    //public event Action OnBlockChanged;
    public event Action OnDodgeChanged;
    public event Action OnStaggerChanged;
    public event Action<CharacterBase> OnDeath;
    public event Action OnStaggerStun;

    #region 基础属性访问接口
    /// <summary>
    /// 获取基础力量（不包含buff加成）
    /// </summary>
    public float BaseStrength => baseStrength;

    /// <summary>
    /// 获取最终力量（包含所有buff加成）
    /// </summary>
    public float FinalStrength => GetModifiedValue(AttributeType.Strength, baseStrength);

    /// <summary>
    /// 获取基础敏捷（不包含buff加成）
    /// </summary>
    public float BaseAgility => baseAgility;

    /// <summary>
    /// 获取最终敏捷（包含所有buff加成）
    /// </summary>
    public float FinalAgility => GetModifiedValue(AttributeType.Agility, baseAgility);

    /// <summary>
    /// 获取基础移动速度（不包含buff加成）
    /// </summary>
    public float BaseMoveSpeed => baseMoveSpeed;

    /// <summary>
    /// 获取最终移动速度（包含所有buff加成）
    /// </summary>
    public float FinalMoveSpeed => GetModifiedValue(AttributeType.MoveSpeed, baseMoveSpeed);

    /// <summary>
    /// 获取基础暴击率（不包含buff加成）
    /// </summary>
    public float BaseCritRate => baseCritRate;

    /// <summary>
    /// 获取最终暴击率（包含所有buff加成）
    /// </summary>
    public float FinalCritRate => GetModifiedValue(AttributeType.CritRate, baseCritRate);

    /// <summary>
    /// 获取基础暴击倍率（不包含buff加成）
    /// </summary>
    public float BaseCritMultiplier => baseCritMultiplier;

    /// <summary>
    /// 获取最终暴击倍率（包含所有buff加成）
    /// </summary>
    public float FinalCritMultiplier => GetModifiedValue(AttributeType.CritMultiplier, baseCritMultiplier);


    /// <summary>
    /// 获取最终防御（包含所有buff加成）
    /// </summary>
    public int FinalDefense => (int)GetModifiedValue(AttributeType.Defense, baseDefense);

    /// <summary>
    /// 获取最终移动速度系数（包含所有buff加成）
    /// 用于外部直接获取计算后的速度倍率
    /// </summary>
    public float FinalMoveSpeedMultiplier => GetModifiedValue(AttributeType.MoveSpeedMultiplier, baseMoveSpeedMultiplier);

    /// <summary>
    /// 获取基础最大生命（不包含buff加成）
    /// </summary>
    public float BaseMaxHealth { get; private set; }

    /// <summary>
    /// 获取最终最大生命（包含所有buff加成）
    /// </summary>
    public float FinalMaxHealth => GetModifiedValue(AttributeType.MaxHealth, BaseMaxHealth);

    /// <summary>
    /// 获取基础最大能量（不包含buff加成）
    /// </summary>
    public float BaseMaxEnergy { get; private set; }

    /// <summary>
    /// 获取最终最大能量（包含所有buff加成）
    /// </summary>
    public float FinalMaxEnergy => GetModifiedValue(AttributeType.MaxEnergy, BaseMaxEnergy);

    /// <summary>
    /// 获取最终附加伤害（包含所有buff加成）
    /// 此值会在伤害计算时额外添加到总伤害中
    /// </summary>
    public float FinalAttackDamage => GetModifiedValue(AttributeType.Attack, baseAttackDamage);
    #endregion

    /// <summary>
    /// 初始化基础属性
    /// 在Awake或Start中调用，用于保存初始的基础属性值
    /// </summary>
    public void Initialize()
    {
        BaseMaxHealth = maxHealth;
        BaseMaxEnergy = maxEnergy;
    }

    /// <summary>
    /// 获取攻击速度倍率（基于最终敏捷计算）
    /// 每100点敏捷增加100%攻击速度，即每点敏捷增加1%攻击速度
    /// </summary>
    public float GetAttackSpeedMultiplier()
    {
        return 1f + (FinalAgility * 0.01f);
    }

    /// <summary>
    /// 获取施法速度（基于最终敏捷计算）
    /// </summary>
    public float GetCastSpeed()
    {
        return 1f + (FinalAgility * 0.015f);
    }

    public void UpdatePerFrame(float deltaTime)
    {
        UpdateRegeneration(deltaTime);
        //UpdateModifiers(deltaTime);
    }

    public void UpdateRegeneration(float deltaTime)
    {
        if (currentHealth < maxHealth && currentHealth > 0)
        {
            ChangeHealth(healthRegenRate * deltaTime);
        }

        if (currentEnergy < maxEnergy)
        {
            ChangeEnergy(energyRegenRate * deltaTime);
        }

        if (currentDodgeCount < maxDodgeCount)
        {
            dodgeRegenTimer += deltaTime;
            if (dodgeRegenTimer >= dodgeRegenInterval)
            {
                currentDodgeCount++;
                dodgeRegenTimer = 0f;
                OnDodgeChanged?.Invoke();
            }
        }

        if (currentStagger > 0)
        {
            currentStagger = Mathf.Max(0, currentStagger - staggerRecoveryRate * deltaTime);
            OnStaggerChanged?.Invoke();
        }
    }

    private void UpdateModifiers(float deltaTime)
    {
        //buff系统的效果,由buff系统移除
        for (int i = activeModifiers.Count - 1; i >= 0; i--)
        {
            activeModifiers[i].remainingDuration -= deltaTime;
            if (activeModifiers[i].remainingDuration <= 0)
            {
                RemoveModifier(activeModifiers[i]);
            }
        }
    }

    public void ChangeHealth(float amount, CharacterBase source = null)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        OnHealthChanged?.Invoke();

        if (currentHealth <= 0)
        {
            OnDeath?.Invoke(source);
        }
    }

    public void ChangeEnergy(float amount)
    {
        currentEnergy = Mathf.Clamp(currentEnergy + amount, 0, maxEnergy);
        OnEnergyChanged?.Invoke();
    }

    public void ChangeShield(float amount)
    {
        currentShield = Mathf.Clamp(currentShield + amount, 0, maxShield);
        OnShieldChanged?.Invoke();
    }

    public void AddStagger(float amount)
    {
        currentStagger = Mathf.Min(currentStagger + amount, maxStagger);
        OnStaggerChanged?.Invoke();

        if (currentStagger >= staggerStunThreshold)
        {
            isStaggered = true;
            OnStaggerStun?.Invoke();//todo:处理震击效果
        }
    }

    public bool ConsumeDodge()
    {
        if (currentDodgeCount > 0)
        {
            currentDodgeCount--;
            OnDodgeChanged?.Invoke();
            return true;
        }
        return false;
    }

    public bool HasEnoughEnergy(float cost)
    {
        return currentEnergy >= cost;
    }

    /// <summary>
    /// 添加无敌效果
    /// </summary>
    public void AddInvincibility()
    {
        isInvincible++;
    }

    /// <summary>
    /// 移除无敌效果
    /// </summary>
    public void RemoveInvincibility()
    {
        isInvincible = Mathf.Max(0, isInvincible - 1);
    }

    /// <summary>
    /// 检查是否处于无敌状态
    /// </summary>
    public bool IsInvincible()
    {
        return isInvincible > 0;
    }

    /// <summary>
    /// 添加属性修改器
    /// 用于添加Buff产生的属性加成效果
    /// </summary>
    /// <param name="modifier">要添加的修改器实例</param>
    /// </summary>
    public void AddModifier(AttributeModifierInstance modifier)
    {
        float oldMaxHealth = maxHealth;
        float oldMaxEnergy = maxEnergy;

        activeModifiers.Add(modifier);

        if (modifier.attributeType == AttributeType.MaxHealth)
        {
            // 更新最大生命值
            maxHealth = FinalMaxHealth;
            // 确保当前生命值不超过新的最大生命值
            // 不自动回复生命值，保持当前血量不变
            currentHealth = Mathf.Min(currentHealth, maxHealth);
            OnHealthChanged?.Invoke();
        }
        else if (modifier.attributeType == AttributeType.MaxEnergy)
        {
            maxEnergy = FinalMaxEnergy;
            OnEnergyChanged?.Invoke();
        }
    }

    /// <summary>
    /// 移除属性修改器
    /// 用于移除Buff时清除属性加成效果
    /// 注意：移除最大生命值时不会损失当前生命值，只会降低上限
    /// </summary>
    public void RemoveModifier(AttributeModifierInstance modifier)
    {
        float oldMaxHealth = maxHealth;
        float oldMaxEnergy = maxEnergy;

        activeModifiers.Remove(modifier);

        if (modifier.attributeType == AttributeType.MaxHealth)
        {
            float healthPercentage = oldMaxHealth > 0 ? currentHealth / oldMaxHealth : 1f;

            maxHealth = FinalMaxHealth;

            currentHealth = Mathf.Min(currentHealth, maxHealth);

            OnHealthChanged?.Invoke();
        }
        else if (modifier.attributeType == AttributeType.MaxEnergy)
        {
            maxEnergy = FinalMaxEnergy;
            currentEnergy = Mathf.Min(currentEnergy, maxEnergy);
            OnEnergyChanged?.Invoke();
        }
    }

    /// <summary>
    /// 获取经过修改器加成后的最终属性值
    /// 计算顺序：(基础值 + 固定值加成) * (1 + 百分比加成)
    /// </summary>
    /// <param name="attributeType">属性类型</param>
    /// <param name="baseValue">基础值</param>
    /// <returns>最终属性值</returns>
    public float GetModifiedValue(AttributeType attributeType, float baseValue)
    {
        float flatBonus = 0f;
        float percentBonus = 0f;

        // 累加所有该属性类型的修改器
        foreach (var modifier in activeModifiers)
        {
            if (modifier.attributeType == attributeType)
            {
                if (modifier.modifierType == ModifierType.Flat)
                {
                    flatBonus += modifier.value;
                }
                else if (modifier.modifierType == ModifierType.Percent)
                {
                    percentBonus += modifier.value;
                }
            }
        }

        // 计算最终值：(基础值 + 固定值) * (1 + 百分比/100)
        return (baseValue + flatBonus) * (1f + percentBonus / 100f);
    }

    /// <summary>
    /// 根据sourceId移除所有相关的修改器
    /// 用于移除某个特定Buff产生的所有属性加成
    /// </summary>
    /// <param name="sourceId">Buff来源ID</param>
    public void RemoveModifiersBySource(string sourceId)
    {
        // 从后往前遍历，避免删除时索引错误
        for (int i = activeModifiers.Count - 1; i >= 0; i--)
        {
            if (activeModifiers[i].sourceId == sourceId)
            {
                RemoveModifier(activeModifiers[i]);
            }
        }
    }

    /// <summary>
    /// 更新指定sourceId的修饰符层数
    /// 通过更新value值而不是删除再添加来避免血量重置问题
    /// </summary>
    public void UpdateModifierStacksBySource(string sourceId, int newStacks)
    {
        bool hasMaxHealthModifier = false;
        bool hasMaxEnergyModifier = false;

        foreach (var modifier in activeModifiers)
        {
            if (modifier.sourceId == sourceId)
            {
                float baseValue = modifier.value / modifier.stacks;
                modifier.value = baseValue * newStacks;
                modifier.stacks = newStacks;

                if (modifier.attributeType == AttributeType.MaxHealth)
                {
                    hasMaxHealthModifier = true;
                }
                else if (modifier.attributeType == AttributeType.MaxEnergy)
                {
                    hasMaxEnergyModifier = true;
                }
            }
        }

        if (hasMaxHealthModifier)
        {
            maxHealth = FinalMaxHealth;
            currentHealth = Mathf.Min(currentHealth, maxHealth);
            OnHealthChanged?.Invoke();
        }

        if (hasMaxEnergyModifier)
        {
            maxEnergy = FinalMaxEnergy;
            currentEnergy = Mathf.Min(currentEnergy, maxEnergy);
            OnEnergyChanged?.Invoke();
        }
    }
}

/// <summary>
/// 属性修改器实例
/// 用于记录Buff对角色属性的加成效果
/// </summary>
[Serializable]
public class AttributeModifierInstance
{
    /// <summary>
    /// 属性类型
    /// </summary>
    public AttributeType attributeType;

    /// <summary>
    /// 修改器类型（固定值/百分比）
    /// </summary>
    public ModifierType modifierType;

    /// <summary>
    /// 修改值（固定值为具体数值，百分比为0-100的数值）
    /// </summary>
    public float value;

    /// <summary>
    /// 剩余持续时间（-1表示永久）
    /// </summary>
    public float remainingDuration;

    /// <summary>
    /// 修改器来源ID（通常为Buff的唯一标识）
    /// 用于在移除Buff时定位并移除对应的修改器
    /// </summary>
    public string sourceId;

    /// <summary>
    /// Buff的当前层数
    /// 用于支持可堆叠的Buff效果
    /// </summary>
    public int stacks;
}



public enum ModifierType
{
    [InspectorName("固定值")]
    Flat,   // 固定值
    [InspectorName("百分比")]
    Percent // 百分比
}

public enum AttributeType
{
    None,
    Health,//血量
    Energy,//能量
    Strength,//力量
    Agility,//敏捷
    MoveSpeed,//移动速度
    MoveSpeedMultiplier,//移动速度系数
    CritRate,//暴击率
    CritMultiplier,//暴击伤害
    BlockValue,//格挡值
    DodgePoints,//闪避点数
    AttackSpeed,//攻击速度
    DamageReduction,//伤害减免
    DamageAmplification,//伤害增幅
    Attack,
    Defense,
    MaxHealth,
    MaxEnergy
}
