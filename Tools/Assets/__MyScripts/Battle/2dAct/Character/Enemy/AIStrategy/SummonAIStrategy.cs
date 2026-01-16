using UnityEngine;

/// <summary>
/// 召唤物AI策略
/// 专为召唤物设计的AI行为，支持跟随、攻击、保护等模式
/// </summary>
[CreateAssetMenu(fileName = "Summon AI Strategy", menuName = "Enemy System/AI Strategies/Summon")]
public class SummonAIStrategy : BaseAIStrategy
{
    [Header("召唤物AI设置")]
    [Tooltip("跟随距离阈值")]
    [SerializeField] private float followDistanceThreshold = 5f;

    [Tooltip("攻击距离阈值")]
    [SerializeField] private float attackDistanceThreshold = 2f;

    [Tooltip("保护范围")]
    [SerializeField] private float protectRange = 3f;

    [Tooltip("巡逻半径")]
    [SerializeField] private float patrolRadius = 5f;

    [Tooltip("是否优先攻击召唤者的目标")]
    [SerializeField] private bool prioritizeSummonerTarget = true;

    [Tooltip("是否在召唤者受到攻击时保护")]
    [SerializeField] private bool protectSummonerWhenAttacked = true;

    private SummonController summonController;
    public override void Initialize(CharacterBase controller, EnemyConfigData config)
    {
        base.Initialize(controller, config);
        summonController = controller as SummonController;
    }

    /// <summary>
    /// 决策下一个状态
    /// </summary>
    /// <returns>AI状态</returns>
    public override CharacterState DecideNextState()
    {
        if (controller == null)
        {
            return CharacterState.Idle;
        }

        // 如果是召唤物，获取召唤者信息
        SummonController summonController = controller as SummonController;
        if (summonController == null)
        {
            return CharacterState.Idle;
        }

        CharacterBase summoner = summonController.Summoner;
        if (summoner == null)
        {
            return CharacterState.Idle;
        }

        // 检查召唤者是否在跟随距离内
        float distanceToSummoner = Vector3.Distance(controller.transform.position, summoner.transform.position);

        // 检查是否有目标敌人
        Transform targetEnemy = controller.CurrentTarget;

        // 优先攻击召唤者的目标
        if (prioritizeSummonerTarget)
        {
            // 如果召唤者是玩家，尝试获取玩家的攻击目标
            CharacterLogic playerLogic = summoner as CharacterLogic;
            if (playerLogic != null)
            {
                // 这里需要根据实际情况获取玩家的攻击目标，可能需要扩展CharacterLogic
                // 暂时使用现有目标
            }
        }

        // 如果有目标敌人，且在攻击范围内，进入攻击状态
        if (targetEnemy != null && Vector3.Distance(controller.transform.position, targetEnemy.position) <= attackDistanceThreshold)
        {
            return CharacterState.Attacking;
        }

        // 如果有目标敌人，且在警戒范围内，进入追击状态
        //if (targetEnemy != null && Vector3.Distance(controller.transform.position, targetEnemy.position) <= summonController.senseManager.detectionRange)
        //{
        //    return CharacterState.Chase;
        //}

        // 如果距离召唤者太远，进入追击状态（跟随）
        if (distanceToSummoner > followDistanceThreshold)
        {
            return CharacterState.Chase;
        }

        // 否则，进入巡逻状态
        return CharacterState.Patrol;
    }

    /// <summary>
    /// 选择攻击动作
    /// </summary>
    /// <returns>攻击动作数据</returns>
    public override AttackActionData SelectAttack()
    {
        if (controller == null)
        {
            return null;
        }

        // 如果是召唤物，获取召唤物数据
        SummonController summonController = controller as SummonController;
        if (summonController == null || summonController.SummonData == null)
        {
            return null;
        }

        SummonData summonData = summonController.SummonData;
        if (summonData.attackActions == null || summonData.attackActions.Count == 0)
        {
            return null;
        }

        // 简单实现：随机选择一个攻击动作
        int randomIndex = Random.Range(0, summonData.attackActions.Count);
        return summonData.attackActions[randomIndex];
    }

    /// <summary>
    /// 获取巡逻目标点
    /// </summary>
    /// <returns>巡逻目标点</returns>
    public override Vector3 GetPatrolTarget()
    {
        if (controller == null)
        {
            return controller.transform.position;
        }

        // 如果是召唤物，围绕召唤者巡逻
        SummonController summonController = controller as SummonController;
        if (summonController != null && summonController.Summoner != null)
        {
            // 在召唤者周围随机生成巡逻点
            Vector2 randomOffset = Random.insideUnitCircle * patrolRadius;
            return summonController.Summoner.transform.position + new Vector3(randomOffset.x, randomOffset.y, 0);
        }

        // 否则，在当前位置周围巡逻
        Vector2 randomOffset2 = Random.insideUnitCircle * patrolRadius;
        return controller.transform.position + new Vector3(randomOffset2.x, randomOffset2.y, 0);
    }

    /// <summary>
    /// 是否应该撤退
    /// </summary>
    /// <returns>是否应该撤退</returns>
    public override bool ShouldRetreat()
    {
        // 召唤物通常不撤退，除非生命值过低
        if (controller == null || controller.PlayerAttributes == null)
        {
            return false;
        }

        // 简单实现：当生命值低于20%时撤退
        float healthPercent = controller.PlayerAttributes.characterAtttibute.currentHealth / controller.PlayerAttributes.characterAtttibute.maxHealth;
        return healthPercent < 0.2f;
    }

    /// <summary>
    /// 是否应该使用特殊能力
    /// </summary>
    /// <returns>是否应该使用特殊能力</returns>
    public override bool ShouldUseSpecialAbility()
    {
        // 召唤物默认不使用特殊能力，除非有特殊配置
        return false;
    }

    /// <summary>
    /// 是否应该闪避
    /// </summary>
    /// <returns>是否应该闪避</returns>
    public override bool ShouldDodge()
    {
        // 召唤物默认不闪避
        return false;
    }

    /// <summary>
    /// 是否应该使用恢复技能
    /// </summary>
    /// <returns>是否应该使用恢复技能</returns>
    public override bool ShouldUseRecoverySkill()
    {
        // 简单实现：当生命值低于50%时使用恢复技能
        if (controller == null || controller.PlayerAttributes == null)
        {
            return false;
        }

        float healthPercent = controller.PlayerAttributes.characterAtttibute.currentHealth / controller.PlayerAttributes.characterAtttibute.maxHealth;
        return healthPercent < 0.5f;
    }
}