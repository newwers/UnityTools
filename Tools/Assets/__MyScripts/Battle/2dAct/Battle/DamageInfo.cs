using UnityEngine;

public class DamageInfo
{
    public float baseDamage;
    public float breakPower;
    public float staggerDamage;
    public bool isGuaranteedCrit;
    public float lifeStealPercent;
    
    // 攻击方信息
    public GameObject attacker;
    
    // 受击方信息（在命中检测时填充）
    public GameObject target;
    
    public SkillData skillData;
    public bool isMiss;
    public bool isBlocked;
    public bool isCritical;
    public float healthDamage;
    
    // 为了保持向后兼容性，source指向attacker
    public GameObject source
    {
        get => attacker;
        set => attacker = value;
    }
}
