using UnityEngine;

public interface IDamageable
{
    void TakeDamage(DamageInfo damageInfo, AttackActionData actionData, AttackFrameData frameData, CharacterBase attacker);

    bool IsDead { get; }

    Transform transform { get; }
}

public interface IStunnable
{
    void ApplyStun(float stunDuration);
}
