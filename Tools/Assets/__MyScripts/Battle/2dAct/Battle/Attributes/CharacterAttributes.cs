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

    [Header("基础属性")]
    /// <summary>
    /// 力量
    /// 每100点力量增加100%伤害,1点力量增加1%伤害
    /// </summary>
    public float strength = 10f;
    /// <summary>
    /// 敏捷
    /// </summary>
    public float agility = 10f;
    /// <summary>
    /// 移动速度
    /// </summary>
    public float moveSpeed = 5f;

    [Header("暴击相关")]
    [Range(0, 100)] public float critRate = 5f;
    public float critMultiplier = 2f;

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
    public bool isDodging = false;

    [Header("震击相关")]
    public float maxStagger = 100f;
    public float currentStagger = 0f;
    public float staggerRecoveryRate = 20f;
    public float staggerStunThreshold = 100f;
    public bool isStaggered = false;

    [Header("战斗状态")]
    public float moveSpeedMultiplier = 1f;
    public bool hasSuperArmor = false;
    public bool isInvincible = false;
    public int defense = 0;

    [Header("属性修改器")]
    private List<AttributeModifierInstance> activeModifiers = new List<AttributeModifierInstance>();

    public event Action OnHealthChanged;
    public event Action OnEnergyChanged;
    public event Action OnShieldChanged;
    public event Action OnBlockChanged;
    public event Action OnDodgeChanged;
    public event Action OnStaggerChanged;
    public event Action OnDeath;
    public event Action OnStaggerStun;

    public float GetAttackSpeed()
    {
        return 1f + (agility * 0.02f);
    }

    public float GetCastSpeed()
    {
        return 1f + (agility * 0.015f);
    }

    public void UpdatePerFrame(float deltaTime)
    {
        UpdateRegeneration(deltaTime);
        UpdateModifiers(deltaTime);
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
        for (int i = activeModifiers.Count - 1; i >= 0; i--)
        {
            activeModifiers[i].remainingDuration -= deltaTime;
            if (activeModifiers[i].remainingDuration <= 0)
            {
                RemoveModifier(activeModifiers[i]);
            }
        }
    }

    public void ChangeHealth(float amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        OnHealthChanged?.Invoke();

        if (currentHealth <= 0)
        {
            OnDeath?.Invoke();
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
            OnStaggerStun?.Invoke();
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

    public void AddModifier(AttributeModifierInstance modifier)
    {
        activeModifiers.Add(modifier);
    }

    public void RemoveModifier(AttributeModifierInstance modifier)
    {
        activeModifiers.Remove(modifier);
    }

    public float GetModifiedValue(AttributeType attributeType, float baseValue)
    {
        float flatBonus = 0f;
        float percentBonus = 0f;

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

        return (baseValue + flatBonus) * (1f + percentBonus / 100f);
    }
}

[Serializable]
public class AttributeModifierInstance
{
    public AttributeType attributeType;
    public ModifierType modifierType;
    public float value;
    public float remainingDuration;
    public string sourceId;
}
