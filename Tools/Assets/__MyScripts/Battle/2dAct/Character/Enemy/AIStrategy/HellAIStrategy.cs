using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 地狱级AI策略类，继承自BaseAIStrategy
/// 实现了高级AI行为，包括玩家位置预判、闪避、反击和组合攻击等功能
/// </summary>
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

    // 巡逻相关变量
    private float lastPatrolTime;
    private bool isPatrolling = false;
    private Vector3 currentPatrolTarget;

    // 组合攻击计数器
    private int currentComboCount = 0;

    // 闪避相关变量
    private float lastDodgeTime;
    private const float DODGE_COOLDOWN = 3f; // 闪避冷却时间（秒）

    private EnemyAIController enemyAIController;

    public override void Initialize(CharacterBase controller, EnemyConfigData config)
    {
        base.Initialize(controller, config);
        enemyAIController = controller as EnemyAIController;
    }

    /// <summary>
    /// 决定AI下一个状态
    /// 根据是否有目标分为两种决策逻辑
    /// </summary>
    /// <returns>AI下一个状态</returns>
    public override CharacterState DecideNextState()
    {
        // 检查是否可以进行决策（基于决策间隔）
        if (!CanMakeDecision()) return enemyAIController.CurrentAIState;
        lastDecisionTime = Time.time;

        // 死亡状态优先
        if (controller.IsDead) return CharacterState.Death;
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



        // 根据是否有目标执行不同的决策逻辑
        if (controller.CurrentTarget != null)
        {
            return DecideStateWithTarget();
        }

        return DecideStateWithoutTarget();
    }

    /// <summary>
    /// 有目标时的状态决策逻辑
    /// 包含闪避、攻击、追击等行为判断
    /// </summary>
    /// <returns>AI下一个状态</returns>
    private CharacterState DecideStateWithTarget()
    {
        // 获取当前距离和预测距离
        float distance = GetDistanceToTarget();
        Vector3 predictedPosition = PredictTargetPosition();
        float predictedDistance = Vector3.Distance(controller.transform.position, predictedPosition);
        bool hasLOS = HasLineOfSightToTarget();

        // 检查是否应该闪避
        if (ShouldDodge() && CanDodge())
        {
            PerformDodge();
            return CharacterState.Retreat;
        }

        // 攻击范围内且有视线
        if (distance <= config.attackRange && hasLOS)
        {
            // 检查是否应该反击
            if (ShouldCounterAttack())
            {
                return CharacterState.SpecialAttacking;
            }

            return CharacterState.Attacking;
        }
        // 预测位置在攻击范围内且有视线
        else if (predictedDistance <= config.attackRange * 1.2f && hasLOS)
        {
            return CharacterState.Chase;
        }
        // 检测范围内
        else if (hasLOS)
        {
            // 检查是否被侧翼攻击
            if (IsTargetFlanking())
            {
                return CharacterState.Retreat;
            }
            return CharacterState.Chase;
        }

        // 默认返回巡逻状态
        return CharacterState.Patrol;
    }

    /// <summary>
    /// 无目标时的状态决策逻辑
    /// 实现巡逻和待机的切换
    /// </summary>
    /// <returns>AI下一个状态</returns>
    private CharacterState DecideStateWithoutTarget()
    {
        // 检查是否需要切换巡逻/待机状态
        if (!isPatrolling || Time.time - lastPatrolTime > config.patrolDuration)
        {
            isPatrolling = !isPatrolling;
            lastPatrolTime = Time.time;
        }

        return isPatrolling ? CharacterState.Patrol : CharacterState.Idle;
    }

    /// <summary>
    /// 选择攻击方式
    /// 支持组合攻击和最优攻击选择
    /// </summary>
    /// <returns>选择的攻击数据</returns>
    public override AttackActionData SelectAttack()
    {
        // 检查是否有可用攻击
        if (config.attackActions == null || config.attackActions.Count == 0)
            return null;

        // 获取到目标的距离
        float distance = GetDistanceToTarget();
        // 获取合适的攻击列表
        List<AttackActionData> suitableAttacks = GetSuitableAttacks(distance);

        // 如果没有合适的攻击，返回第一个攻击
        if (suitableAttacks.Count == 0)
            return config.attackActions[0];

        // 处理组合攻击
        if (currentComboCount > 0)
        {
            currentComboCount--;
            return SelectComboAttack(suitableAttacks);
        }

        // 随机触发组合攻击
        if (Random.value < 0.4f)
        {
            currentComboCount = comboAttackCount - 1;
        }

        // 选择最优攻击
        return SelectOptimalAttack(suitableAttacks, distance);
    }

    /// <summary>
    /// 获取适合当前距离的攻击列表
    /// 根据敌人难度添加不同级别的攻击
    /// </summary>
    /// <param name="distance">到目标的距离</param>
    /// <returns>适合的攻击列表</returns>
    private List<AttackActionData> GetSuitableAttacks(float distance)
    {
        List<AttackActionData> suitable = new List<AttackActionData>();

        // 添加基础攻击
        foreach (var attack in config.attackActions)
        {
            if (attack != null)
            {
                suitable.Add(attack);
            }
        }

        // 对于精英和Boss级别敌人，添加精英攻击
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

    /// <summary>
    /// 选择组合攻击中的下一个攻击
    /// 选择前摇时间最短的攻击以保证连击流畅
    /// </summary>
    /// <param name="attacks">可用攻击列表</param>
    /// <returns>选择的攻击数据</returns>
    private AttackActionData SelectComboAttack(List<AttackActionData> attacks)
    {
        // 按前摇时间排序，选择最快的攻击
        attacks.Sort((a, b) => a.windUpTime.CompareTo(b.windUpTime));
        return attacks[0];
    }

    /// <summary>
    /// 选择最优攻击
    /// 考虑目标预测位置和攻击优先级
    /// </summary>
    /// <param name="attacks">可用攻击列表</param>
    /// <param name="distance">到目标的距离</param>
    /// <returns>选择的攻击数据</returns>
    private AttackActionData SelectOptimalAttack(List<AttackActionData> attacks, float distance)
    {
        // 预测目标位置
        Vector3 predictedPosition = PredictTargetPosition();

        // 按优先级排序，选择优先级最高的攻击
        attacks.Sort((a, b) => b.priority.CompareTo(a.priority));

        return attacks[0];
    }

    /// <summary>
    /// 获取巡逻目标点
    /// 根据巡逻区域类型生成随机巡逻点
    /// </summary>
    /// <returns>巡逻目标位置</returns>
    public override Vector3 GetPatrolTarget()
    {
        // 圆形巡逻区域
        if (config.patrolArea.patrolType == PatrolAreaType.Circle)
        {
            Vector2 randomPoint = Random.insideUnitCircle * config.patrolArea.circleRadius;
            return enemyAIController.PatrolCenter + new Vector3(randomPoint.x, randomPoint.y, 0);
        }
        // 矩形巡逻区域
        else if (config.patrolArea.patrolType == PatrolAreaType.Rectangle)
        {
            Vector3 randomPoint = new Vector3(
                Random.Range(-config.patrolArea.rectangleSize.x / 2, config.patrolArea.rectangleSize.x / 2),
                Random.Range(-config.patrolArea.rectangleSize.y / 2, config.patrolArea.rectangleSize.y / 2),
                0
            );
            return enemyAIController.PatrolCenter + randomPoint;
        }

        // 默认返回当前位置
        return controller.transform.position;
    }

    /// <summary>
    /// 判断是否应该撤退
    /// 基于生命值百分比判断
    /// </summary>
    /// <returns>是否应该撤退</returns>
    public override bool ShouldRetreat()
    {
        // 如果撤退阈值未设置，不撤退
        if (config.retreatHealthThreshold <= 0) return false;

        // 计算当前生命值百分比
        float healthPercent = controller.PlayerAttributes.characterAtttibute.currentHealth /
                            controller.PlayerAttributes.characterAtttibute.maxHealth;

        // 当生命值低于阈值时撤退
        return healthPercent <= config.retreatHealthThreshold;
    }

    /// <summary>
    /// 判断是否应该使用特殊能力
    /// 50%几率使用特殊能力
    /// </summary>
    /// <returns>是否应该使用特殊能力</returns>
    public override bool ShouldUseSpecialAbility()
    {
        return Random.value < 0.5f;
    }

    /// <summary>
    /// 预测目标位置
    /// 基于目标当前速度和预测时间计算
    /// </summary>
    /// <returns>预测的目标位置</returns>
    private Vector3 PredictTargetPosition()
    {
        // 如果没有目标，返回原点
        if (controller.CurrentTarget == null) return Vector3.zero;

        // 获取目标的刚体组件
        Rigidbody2D targetRb = controller.CurrentTarget.GetComponent<Rigidbody2D>();
        // 如果没有刚体，返回当前位置
        if (targetRb == null) return controller.CurrentTarget.position;

        // 基于当前速度和预测时间计算预测位置
        Vector3 currentVelocity = targetRb.linearVelocity;
        return controller.CurrentTarget.position + currentVelocity * predictionTime;
    }


    /// <summary>
    /// 判断是否可以闪避
    /// 基于闪避冷却时间
    /// </summary>
    /// <returns>是否可以闪避</returns>
    private bool CanDodge()
    {
        // 检查是否过了闪避冷却时间
        return Time.time - lastDodgeTime >= DODGE_COOLDOWN;
    }

    /// <summary>
    /// 执行闪避动作
    /// 更新闪避时间戳并记录日志
    /// </summary>
    private void PerformDodge()
    {
        // 更新闪避时间戳
        lastDodgeTime = Time.time;
        // 记录闪避日志
        LogManager.Log("[HellAI] 执行闪避!");
    }

    /// <summary>
    /// 判断是否应该反击
    /// 基于反击几率
    /// </summary>
    /// <returns>是否应该反击</returns>
    private bool ShouldCounterAttack()
    {
        // 基于概率判断是否反击
        return Random.value < counterAttackProbability;
    }

    /// <summary>
    /// 判断目标是否在侧翼
    /// 基于目标方向和角色朝向的点积计算
    /// </summary>
    /// <returns>目标是否在侧翼</returns>
    private bool IsTargetFlanking()
    {
        // 检查是否有目标
        if (controller.CurrentTarget == null) return false;

        // 计算到目标的方向向量
        Vector3 toTarget = (controller.CurrentTarget.position - controller.transform.position).normalized;
        // 计算角色正前方向量
        Vector3 forward = controller.transform.right * (enemyAIController.IsFacingRight ? 1 : -1);

        // 计算点积，判断目标是否在侧翼
        float dot = Vector3.Dot(toTarget, forward);

        // 点积小于0.3表示目标在侧翼
        return dot < 0.3f;
    }

    /// <summary>
    /// 判断是否应该闪避
    /// 地狱难度敌人有最高的闪避概率
    /// </summary>
    /// <returns>是否应该闪避</returns>
    public override bool ShouldDodge()
    {
        if (!controller.CanDash()) return false;

        // 地狱难度敌人有最高的闪避概率
        float dodgeProbability = config.dodgeProbability * 2.0f; // 地狱难度敌人闪避概率提升100%
        if (GameDifficultyManager.Instance != null)
        {
            dodgeProbability *= GameDifficultyManager.Instance.CurrentModifiers.dodgeProbabilityMultiplier;
        }

        // 随机判断是否闪避
        return Random.value < dodgeProbability;
    }


    /// <summary>
    /// 判断是否应该使用恢复技能
    /// 地狱难度敌人有最高的恢复技能使用概率
    /// </summary>
    /// <returns>是否应该使用恢复技能</returns>
    public override bool ShouldUseRecoverySkill()
    {
        if (!enemyAIController.CanUseRecoverySkill()) return false;

        // 地狱难度敌人有最高的恢复技能使用概率
        float recoveryProbability = config.recoverySkillProbability * 2.0f; // 地狱难度敌人恢复技能使用概率提升100%
        if (GameDifficultyManager.Instance != null)
        {
            recoveryProbability *= GameDifficultyManager.Instance.CurrentModifiers.recoverySkillProbabilityMultiplier;
        }

        // 随机判断是否使用恢复技能
        return Random.value < recoveryProbability;
    }
}
