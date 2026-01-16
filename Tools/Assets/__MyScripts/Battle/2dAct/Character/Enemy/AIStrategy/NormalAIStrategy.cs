using UnityEngine;

[CreateAssetMenu(fileName = "Normal AI Strategy", menuName = "AI Strategies/Normal")]
public class NormalAIStrategy : BaseAIStrategy
{
    private float lastPatrolTime;
    /// <summary>
    /// 巡逻状态
    /// todo:闪避时是否在巡逻?
    /// </summary>
    private bool isPatrolling = false;
    private Vector3 currentPatrolTarget;

    protected EnemyAIController enemyAIController;

    public override void Initialize(CharacterBase controller, EnemyConfigData config)
    {
        base.Initialize(controller, config);

        enemyAIController = controller as EnemyAIController;
    }

    public override CharacterState DecideNextState()
    {
        if (!CanMakeDecision()) return enemyAIController.CurrentAIState;
        lastDecisionTime = Time.time;

        // 死亡优先
        if (enemyAIController.IsDead) return CharacterState.Death;

        // 硬直状态
        if (enemyAIController.IsStunned) return CharacterState.Stunned;

        if (controller.IsDodging())//闪避中
        {
            return CharacterState.Dodging;
        }

        // 检查是否应该使用恢复技能
        if (ShouldUseRecoverySkill())
        {
            enemyAIController.PerformRecoverySkill();
            return CharacterState.Idle; // 恢复技能后进入待机状态
        }



        // 有目标时的逻辑
        if (controller.CurrentTarget != null)
        {
            float distance = GetDistanceToTarget();

            // 检查是否应该闪避
            if (ShouldDodge())
            {
                // 计算闪避方向（远离目标）
                Vector2 dodgeDirection = (controller.transform.position - controller.CurrentTarget.position).normalized;
                controller.PerformDash(dodgeDirection);
                return CharacterState.Dodging;
            }

            if (distance <= config.attackRange && HasLineOfSightToTarget())
            {
                return CharacterState.Attacking;
            }
            else if (HasLineOfSightToTarget())
            {
                return CharacterState.Chase;
            }
        }

        // 无目标时的巡逻逻辑
        if (!isPatrolling || Time.time - lastPatrolTime > config.patrolDuration)
        {
            isPatrolling = !isPatrolling;
            lastPatrolTime = Time.time;
        }

        return isPatrolling ? CharacterState.Patrol : CharacterState.Idle;
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
            return enemyAIController.PatrolCenter + new Vector3(randomPoint.x, randomPoint.y, 0);
        }
        else if (config.patrolArea.patrolType == PatrolAreaType.Rectangle)
        {
            Vector3 randomPoint = new Vector3(
                Random.Range(-config.patrolArea.rectangleSize.x / 2, config.patrolArea.rectangleSize.x / 2),
                Random.Range(-config.patrolArea.rectangleSize.y / 2, config.patrolArea.rectangleSize.y / 2),
                0
            );
            return enemyAIController.PatrolCenter + randomPoint;
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

    public override bool ShouldDodge()
    {
        if (!controller.CanDash()) return false;

        // 基于难度参数的动态闪避概率计算
        float dodgeProbability = config.dodgeProbability;
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

        // 基于难度参数的恢复技能使用概率计算
        float recoveryProbability = config.recoverySkillProbability;
        if (GameDifficultyManager.Instance != null)
        {
            recoveryProbability *= GameDifficultyManager.Instance.CurrentModifiers.recoverySkillProbabilityMultiplier;
        }

        // 随机判断是否使用恢复技能
        return Random.value < recoveryProbability;
    }
}



