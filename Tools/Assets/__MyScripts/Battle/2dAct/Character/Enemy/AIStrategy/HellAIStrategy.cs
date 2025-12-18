using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Hell AI Strategy", menuName = "Enemy System/AI Strategies/Hell")]
public class HellAIStrategy : BaseAIStrategy
{
    [Header("地狱AI设置")]
    [Tooltip("预判玩家位置时间(秒)")]
    [SerializeField] private float predictionTime = 0.5f;
    
    [Tooltip("组合攻击数量")]
    [SerializeField] private int comboAttackCount = 2;
    
    [Tooltip("闪避几率")]
    [Range(0f, 1f)]
    [SerializeField] private float dodgeProbability = 0.3f;
    
    [Tooltip("反击几率")]
    [Range(0f, 1f)]
    [SerializeField] private float counterAttackProbability = 0.4f;
    
    private float lastPatrolTime;
    private bool isPatrolling = false;
    private Vector3 currentPatrolTarget;
    private int currentComboCount = 0;
    private float lastDodgeTime;
    private const float DODGE_COOLDOWN = 3f;

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
            return DecideStateWithTarget();
        }

        return DecideStateWithoutTarget();
    }

    private EnemyAIState DecideStateWithTarget()
    {
        float distance = GetDistanceToTarget();
        Vector3 predictedPosition = PredictTargetPosition();
        float predictedDistance = Vector3.Distance(controller.transform.position, predictedPosition);
        bool hasLOS = HasLineOfSightToTarget();

        if (ShouldDodge() && CanDodge())
        {
            PerformDodge();
            return EnemyAIState.Retreat;
        }

        if (distance <= config.attackRange && hasLOS)
        {
            if (ShouldCounterAttack())
            {
                return EnemyAIState.SpecialAttack;
            }
            
            return EnemyAIState.Attack;
        }
        else if (predictedDistance <= config.attackRange * 1.2f && hasLOS)
        {
            return EnemyAIState.Chase;
        }
        else if (distance <= config.detectRange)
        {
            if (IsTargetFlanking())
            {
                return EnemyAIState.Retreat;
            }
            return EnemyAIState.Chase;
        }
        else if (distance > config.loseTargetRange)
        {
            controller.SetCurrentTarget(null);
            return EnemyAIState.Patrol;
        }

        return EnemyAIState.Patrol;
    }

    private EnemyAIState DecideStateWithoutTarget()
    {
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

        float distance = GetDistanceToTarget();
        List<AttackActionData> suitableAttacks = GetSuitableAttacks(distance);

        if (suitableAttacks.Count == 0)
            return config.attackActions[0];

        if (currentComboCount > 0)
        {
            currentComboCount--;
            return SelectComboAttack(suitableAttacks);
        }

        if (Random.value < 0.4f)
        {
            currentComboCount = comboAttackCount - 1;
        }

        return SelectOptimalAttack(suitableAttacks, distance);
    }

    private List<AttackActionData> GetSuitableAttacks(float distance)
    {
        List<AttackActionData> suitable = new List<AttackActionData>();
        
        foreach (var attack in config.attackActions)
        {
            if (attack != null)
            {
                suitable.Add(attack);
            }
        }

        if (config.difficulty == EnemyDifficulty.Elite || config.difficulty == EnemyDifficulty.Boss)
        {
            foreach (var attack in config.eliteAttackActions)
            {
                if (attack != null)
                {
                    suitable.Add(attack);
                }
            }
        }

        return suitable;
    }

    private AttackActionData SelectComboAttack(List<AttackActionData> attacks)
    {
        attacks.Sort((a, b) => a.windUpTime.CompareTo(b.windUpTime));
        return attacks[0];
    }

    private AttackActionData SelectOptimalAttack(List<AttackActionData> attacks, float distance)
    {
        Vector3 predictedPosition = PredictTargetPosition();
        
        attacks.Sort((a, b) => b.priority.CompareTo(a.priority));
        
        return attacks[0];
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
        if (config.retreatHealthThreshold <= 0) return false;

        float healthPercent = controller.PlayerAttributes.characterAtttibute.currentHealth / 
                            controller.PlayerAttributes.characterAtttibute.maxHealth;
        
        return healthPercent <= config.retreatHealthThreshold;
    }

    public override bool ShouldUseSpecialAbility()
    {
        return Random.value < 0.5f;
    }

    private Vector3 PredictTargetPosition()
    {
        if (controller.CurrentTarget == null) return Vector3.zero;

        Rigidbody2D targetRb = controller.CurrentTarget.GetComponent<Rigidbody2D>();
        if (targetRb == null) return controller.CurrentTarget.position;

        Vector3 currentVelocity = targetRb.linearVelocity;
        return controller.CurrentTarget.position + currentVelocity * predictionTime;
    }

    private bool ShouldDodge()
    {
        if (controller.CurrentTarget == null) return false;

        CharacterBase targetCharacter = controller.CurrentTarget.GetComponent<CharacterBase>();
        if (targetCharacter == null) return false;

        return Random.value < dodgeProbability;
    }

    private bool CanDodge()
    {
        return Time.time - lastDodgeTime >= DODGE_COOLDOWN;
    }

    private void PerformDodge()
    {
        lastDodgeTime = Time.time;
        LogManager.Log("[HellAI] 执行闪避!");
    }

    private bool ShouldCounterAttack()
    {
        return Random.value < counterAttackProbability;
    }

    private bool IsTargetFlanking()
    {
        if (controller.CurrentTarget == null) return false;

        Vector3 toTarget = (controller.CurrentTarget.position - controller.transform.position).normalized;
        Vector3 forward = controller.transform.right * (controller.IsFacingRight ? 1 : -1);

        float dot = Vector3.Dot(toTarget, forward);
        
        return dot < 0.3f;
    }
}
