using UnityEngine;
public enum AttributeType
{
    None,
    Health,//血量
    Energy,//能量
    Strength,//力量
    Agility,//敏捷
    MoveSpeed,//移动速度
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

public enum ModifierType
{
    Flat,   // 固定值
    Percent // 百分比
}

/// <summary>
/// 玩家属性组件，负责管理血量等数值
/// </summary>
[DisallowMultipleComponent]
public class PlayerAttributes : MonoBehaviour
{
    public CharacterAttributes characterAtttibute;

    /// <summary>
    /// 是否死亡（仅根据生命值判断）
    /// </summary>
    public bool IsDead => characterAtttibute.currentHealth <= 0;

    private void Update()
    {
        characterAtttibute.UpdateRegeneration(Time.deltaTime);
    }

    /// <summary>
    /// 初始化属性（用于开局或重生）
    /// </summary>
    public void Initialize()
    {
        characterAtttibute.currentHealth = Mathf.Clamp(characterAtttibute.currentHealth, 0, characterAtttibute.maxHealth);
        if (characterAtttibute.currentHealth == 0)
        {
            characterAtttibute.currentHealth = characterAtttibute.maxHealth;
        }
    }

    /// <summary>
    /// 扣血，返回是否死亡
    /// </summary>
    public bool TakeDamage(int damage)
    {
        if (IsDead) return true;

        float prevHp = characterAtttibute.currentHealth;
        characterAtttibute.currentHealth = Mathf.Max(0, characterAtttibute.currentHealth - damage);
        LogManager.Log($"[PlayerAttributes] 受伤: {damage} HP {prevHp} -> {characterAtttibute.currentHealth}");

        return IsDead;
    }

    /// <summary>
    /// 回复生命
    /// </summary>
    public void Heal(int amount)
    {
        if (IsDead) return;

        float prev = characterAtttibute.currentHealth;
        characterAtttibute.currentHealth = Mathf.Clamp(characterAtttibute.currentHealth + amount, 0, characterAtttibute.maxHealth);
        LogManager.Log($"[PlayerAttributes] 回复: {amount} HP {prev} -> {characterAtttibute.currentHealth}");
    }



}



