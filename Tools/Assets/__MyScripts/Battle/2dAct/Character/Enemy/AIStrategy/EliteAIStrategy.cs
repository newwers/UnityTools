using System.Collections.Generic;
using UnityEngine;

// 精英敌人AI策略
[CreateAssetMenu(fileName = "Elite AI Strategy", menuName = "AI Strategies/Elite")]
public class EliteAIStrategy : NormalAIStrategy
{
    [Header("精英设置")]
    [Tooltip("特殊能力冷却时间")]
    [SerializeField] protected float specialAbilityCooldown = 10f;

    protected float lastSpecialAbilityTime;

    public override CharacterState DecideNextState()
    {
        if (!CanMakeDecision()) return enemyAIController.CurrentAIState;
        lastDecisionTime = Time.time;

        if (enemyAIController.IsDead) return CharacterState.Death;
        if (enemyAIController.IsStunned) return CharacterState.Stunned;

        if (enemyAIController.IsDodging())//闪避中
        {
            return CharacterState.Dodging;
        }

        // 检查是否应该使用恢复技能
        if (ShouldUseRecoverySkill())
        {
            enemyAIController.PerformRecoverySkill();
            return CharacterState.Idle; // 恢复技能后进入待机状态
        }



        if (controller.CurrentTarget != null)
        {
            // 检查是否应该闪避
            if (ShouldDodge())
            {
                // 计算闪避方向（远离目标）
                Vector2 dodgeDirection = (controller.transform.position - controller.CurrentTarget.position).normalized;
                controller.PerformDash(dodgeDirection);
                return CharacterState.Dodging;
            }

            float distance = GetDistanceToTarget();
            bool hasLOS = HasLineOfSightToTarget();

            // 精英敌人有更复杂的决策逻辑
            if (distance <= config.attackRange && hasLOS)
            {
                // 有机会使用特殊攻击
                if (ShouldUseSpecialAbility() && distance <= config.attackRange * 1.5f)
                {
                    return CharacterState.SpecialAttacking;
                }
                return CharacterState.Attacking;
            }
            else if (hasLOS)
            {
                return CharacterState.Chase;
            }
        }

        // 精英敌人更频繁巡逻
        return ShouldRetreat() ? CharacterState.Retreat : CharacterState.Patrol;
    }

    public override AttackActionData SelectAttack()
    {
        // 先检查是否有精英攻击配置
        if (config.eliteAttackActions != null && config.eliteAttackActions.Count > 0)
        {
            // 精英敌人根据距离选择攻击
            float distance = GetDistanceToTarget();

            var suitableAttacks = new List<AttackActionData>();
            foreach (var attack in config.eliteAttackActions)
            {
                // 根据攻击范围选择合适的攻击
                if (attack != null && distance <= GetAttackRange(attack))
                {
                    suitableAttacks.Add(attack);
                }
            }

            if (suitableAttacks.Count > 0)
            {
                return suitableAttacks[Random.Range(0, suitableAttacks.Count)];
            }

            return config.eliteAttackActions[0]; // 默认第一个攻击
        }

        // 如果没有精英攻击配置，从普通攻击列表中获取
        return base.SelectAttack();
    }

    public override bool ShouldRetreat()
    {
        // 精英敌人在低血量时撤退
        if (config.retreatHealthThreshold <= 0) return false;

        float healthPercent = controller.PlayerAttributes.characterAtttibute.currentHealth / controller.PlayerAttributes.characterAtttibute.maxHealth;
        return healthPercent <= config.retreatHealthThreshold;
    }

    public override bool ShouldUseSpecialAbility()
    {
        if (Time.time - lastSpecialAbilityTime < specialAbilityCooldown)
            return false;

        // 30%几率使用特殊能力
        if (Random.value < 0.3f)
        {
            lastSpecialAbilityTime = Time.time;
            return true;
        }

        return false;
    }

    public override bool ShouldDodge()
    {
        if (!controller.CanDash()) return false;

        // 精英敌人有更高的闪避概率
        float dodgeProbability = config.dodgeProbability * 1.2f; // 精英敌人闪避概率提升20%
        if (GameDifficultyManager.Instance != null)
        {
            dodgeProbability *= GameDifficultyManager.Instance.CurrentModifiers.dodgeProbabilityMultiplier;
        }

        // 随机判断是否闪避
        return Random.value < dodgeProbability;
    }

    public override bool ShouldUseRecoverySkill()
    {
        if (!enemyAIController.CanUseRecoverySkill()) return false;

        // 精英敌人有更高的恢复技能使用概率
        float recoveryProbability = config.recoverySkillProbability * 1.3f; // 精英敌人恢复技能使用概率提升30%
        if (GameDifficultyManager.Instance != null)
        {
            recoveryProbability *= GameDifficultyManager.Instance.CurrentModifiers.recoverySkillProbabilityMultiplier;
        }

        // 随机判断是否使用恢复技能
        return Random.value < recoveryProbability;
    }

    public float GetAttackRange(AttackActionData attack)
    {
        // 计算攻击的有效范围
        float maxRange = 0f;
        foreach (var frame in attack.frameData)
        {
            float frameRange = frame.hitboxOffset.x + (frame.hitboxType == HitboxType.Circle ?
                frame.hitboxRadius : frame.hitboxSize.x);
            maxRange = Mathf.Max(maxRange, frameRange);
        }
        return maxRange;
    }
}
