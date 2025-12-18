using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Normal AI Strategy", menuName = "Enemy System/AI Strategies/Normal")]
public class NormalAIStrategy : BaseAIStrategy
{
    private float lastPatrolTime;
    private bool isPatrolling = false;
    private Vector3 currentPatrolTarget;

    public override EnemyAIState DecideNextState()
    {
        if (!CanMakeDecision()) return controller.CurrentAIState;
        lastDecisionTime = Time.time;

        // 死亡优先
        if (controller.IsDead) return EnemyAIState.Death;

        // 硬直状态
        if (controller.IsStunned) return EnemyAIState.Stunned;

        // 检查目标
        GameObject target = controller.FindNearestPlayer();
        controller.SetCurrentTarget(target != null ? target.transform : null);

        // 有目标时的逻辑
        if (controller.CurrentTarget != null)
        {
            float distance = GetDistanceToTarget();

            if (distance <= config.attackRange && HasLineOfSightToTarget())
            {
                return EnemyAIState.Attack;
            }
            else if (distance <= config.detectRange)
            {
                return EnemyAIState.Chase;
            }
            else if (distance > config.loseTargetRange)
            {
                controller.SetCurrentTarget(null);
                return EnemyAIState.Patrol;
            }
        }

        // 无目标时的巡逻逻辑
        if (!isPatrolling || Time.time - lastPatrolTime > config.patrolDuration)
        {
            isPatrolling = !isPatrolling;
            lastPatrolTime = Time.time;
        }

        return isPatrolling ? EnemyAIState.Patrol : EnemyAIState.Idle;
    }

    public override AttackActionData SelectAttack()
    {
        if (config.attackActions == null || config.attackActions.Count == 0)
            return null;

        // 普通敌人随机选择攻击
        return config.attackActions[Random.Range(0, config.attackActions.Count)];
    }

    public override Vector3 GetPatrolTarget()
    {
        if (config.patrolArea.patrolType == PatrolAreaType.Circle)
        {
            Vector2 randomPoint = Random.insideUnitCircle * config.patrolArea.circleRadius;
            return controller.PatrolCenter + new Vector3(randomPoint.x, randomPoint.y, 0);
        }
        else if (config.patrolArea.patrolType == PatrolAreaType.Rectangle)
        {
            Vector3 randomPoint = new Vector3(
                Random.Range(-config.patrolArea.rectangleSize.x / 2, config.patrolArea.rectangleSize.x / 2),
                Random.Range(-config.patrolArea.rectangleSize.y / 2, config.patrolArea.rectangleSize.y / 2),
                0
            );
            return controller.PatrolCenter + randomPoint;
        }

        return controller.transform.position;
    }

    public override bool ShouldRetreat()
    {
        return false; // 普通敌人不撤退
    }

    public override bool ShouldUseSpecialAbility()
    {
        return false; // 普通敌人没有特殊能力
    }
}

// 精英敌人AI策略
[CreateAssetMenu(fileName = "Elite AI Strategy", menuName = "Enemy System/AI Strategies/Elite")]
public class EliteAIStrategy : NormalAIStrategy
{
    [Header("精英设置")]
    [Tooltip("特殊能力冷却时间")]
    [SerializeField] protected float specialAbilityCooldown = 10f;

    protected float lastSpecialAbilityTime;

    public override EnemyAIState DecideNextState()
    {
        if (!CanMakeDecision()) return controller.CurrentAIState;
        lastDecisionTime = Time.time;

        if (controller.IsDead) return EnemyAIState.Death;
        if (controller.IsStunned) return EnemyAIState.Stunned;

        GameObject target = controller.FindNearestPlayer();
        controller.SetCurrentTarget(target != null ? target.transform : null);

        if (controller.CurrentTarget != null)
        {
            float distance = GetDistanceToTarget();
            bool hasLOS = HasLineOfSightToTarget();

            // 精英敌人有更复杂的决策逻辑
            if (distance <= config.attackRange && hasLOS)
            {
                // 有机会使用特殊攻击
                if (ShouldUseSpecialAbility() && distance <= config.attackRange * 1.5f)
                {
                    return EnemyAIState.SpecialAttack;
                }
                return EnemyAIState.Attack;
            }
            else if (distance <= config.detectRange && hasLOS)
            {
                return EnemyAIState.Chase;
            }
            else if (distance > config.loseTargetRange)
            {
                controller.SetCurrentTarget(null);
            }
        }

        // 精英敌人更频繁巡逻
        return ShouldRetreat() ? EnemyAIState.Retreat : EnemyAIState.Patrol;
    }

    public override AttackActionData SelectAttack()
    {
        if (config.eliteAttackActions == null || config.eliteAttackActions.Count == 0)
            return null;

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

// Boss AI策略
[CreateAssetMenu(fileName = "Boss AI Strategy", menuName = "Enemy System/AI Strategies/Boss")]
public class BossAIStrategy : EliteAIStrategy
{
    private int currentPhase = 0;
    private bool phaseTransitioning = false;

    public override void Initialize(EnemyAIController controller, EnemyConfigData config)
    {
        base.Initialize(controller, config);
        currentPhase = 0;
    }

    public override EnemyAIState DecideNextState()
    {
        if (phaseTransitioning) return EnemyAIState.Idle;

        // 检查阶段转换
        CheckPhaseTransition();

        return base.DecideNextState();
    }

    public override AttackActionData SelectAttack()
    {
        if (config.bossAttackActions == null || config.bossAttackActions.Count == 0)
            return null;

        // Boss根据阶段选择攻击
        var phaseAttacks = GetCurrentPhaseAttacks();
        if (phaseAttacks.Count == 0) return base.SelectAttack();

        // Boss有更智能的攻击选择
        float distance = GetDistanceToTarget();
        var suitableAttacks = new List<AttackActionData>();

        foreach (var attack in phaseAttacks)
        {
            if (attack != null && distance <= GetAttackRange(attack) * 1.2f)
            {
                suitableAttacks.Add(attack);
            }
        }

        if (suitableAttacks.Count > 0)
        {
            // 根据攻击冷却和优先级选择
            suitableAttacks.Sort((a, b) => b.priority.CompareTo(a.priority));
            return suitableAttacks[0];
        }

        return base.SelectAttack();
    }

    public override bool ShouldUseSpecialAbility()
    {
        // Boss更频繁使用特殊能力
        if (Time.time - lastSpecialAbilityTime < specialAbilityCooldown * 0.7f)
            return false;

        // 根据阶段调整几率
        float probability = 0.4f + (currentPhase * 0.2f);
        if (Random.value < probability)
        {
            lastSpecialAbilityTime = Time.time;
            return true;
        }

        return false;
    }

    private void CheckPhaseTransition()
    {
        if (config.bossPhases == null || config.bossPhases.Count == 0)
            return;

        float healthPercent = controller.PlayerAttributes.characterAtttibute.currentHealth / controller.PlayerAttributes.characterAtttibute.maxHealth;

        for (int i = currentPhase; i < config.bossPhases.Count; i++)
        {
            if (healthPercent <= config.bossPhases[i].healthPercentThreshold)
            {
                if (i > currentPhase)
                {
                    StartPhaseTransition(i);
                }
                break;
            }
        }
    }

    private void StartPhaseTransition(int newPhase)
    {
        phaseTransitioning = true;
        currentPhase = newPhase;

        // 播放阶段转换效果
        if (config.bossPhases[newPhase].phaseTransitionEffect != null)
        {
            //Instantiate(config.bossPhases[newPhase].phaseTransitionEffect,controller.transform.position, Quaternion.identity);
        }

        // 应用新阶段的配置
        var phaseConfig = config.bossPhases[newPhase].phaseConfig;
        if (phaseConfig != null)
        {
            controller.ApplyAIConfig(phaseConfig);
        }

        // 阶段转换完成
        phaseTransitioning = false;
    }

    private List<AttackActionData> GetCurrentPhaseAttacks()
    {
        //return config.bossAttackActions;
        var attacks = new List<AttackActionData>();

        // 加上基础攻击
        attacks.AddRange(config.attackActions);
        attacks.AddRange(config.eliteAttackActions);
        attacks.AddRange(config.bossAttackActions);

        return attacks;
    }
}