using UnityEngine;

public interface IDamageable
{
    void TakeDamage(DamageInfo damageInfo, AttackActionData actionData, AttackFrameData frameData, GameObject attacker);

    bool IsDead { get; }

    Transform Transform { get; }
}

public interface IStunnable
{
    void ApplyStun(float stunDuration);
}
