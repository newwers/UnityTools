/// <summary>
/// 伤害信息(不包括最终伤害计算)
/// </summary>
public class DamageInfo
{
    public float baseDamage;
    public float breakPower;
    public float staggerDamage;
    public bool isGuaranteedCrit;
    public float lifeStealPercent;

    // 攻击方信息
    public CharacterBase attacker;

    // 索敌目标（技能释放前获取的目标）
    public CharacterBase targetedEnemy;

    // 受击方信息（在命中检测时填充）
    public CharacterBase target;

    public SkillData skillData;
    public bool isMiss;
    public bool isBlocked;
    public bool isCritical;
    public float healthDamage;


}
