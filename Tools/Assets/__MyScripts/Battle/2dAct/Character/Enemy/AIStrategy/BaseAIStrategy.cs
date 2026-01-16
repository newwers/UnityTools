using UnityEngine;

public interface IAIStrategy
{
    void Initialize(CharacterBase characterBase, EnemyConfigData config);
    /// <summary>
    /// 决策下一个状态
    /// </summary>
    /// <returns></returns>
    CharacterState DecideNextState();
    AttackActionData SelectAttack();
    /// <summary>
    /// 获取巡逻目标点
    /// </summary>
    /// <returns></returns>
    Vector3 GetPatrolTarget();
    /// <summary>
    /// 是否应该撤退
    /// </summary>
    /// <returns></returns>
    bool ShouldRetreat();
    /// <summary>
    /// 是否应该使用特殊能力
    /// </summary>
    /// <returns></returns>
    bool ShouldUseSpecialAbility();

    /// <summary>
    /// 是否应该闪避
    /// </summary>
    /// <returns></returns>
    bool ShouldDodge();

    /// <summary>
    /// 是否应该使用恢复技能
    /// </summary>
    /// <returns></returns>
    bool ShouldUseRecoverySkill();
}

public abstract class BaseAIStrategy : ScriptableObject, IAIStrategy
{
    [Header("决策设置")]
    [Tooltip("AI决策更新间隔(秒)")]
    [SerializeField] protected float decisionInterval = 0.5f;

    protected CharacterBase controller;
    protected EnemyConfigData config;
    protected float lastDecisionTime;

    public virtual void Initialize(CharacterBase controller, EnemyConfigData config)
    {
        this.controller = controller;
        this.config = config;

        if (GameDifficultyManager.Instance != null)
        {
            float speedMultiplier = GameDifficultyManager.Instance.GetAIDecisionSpeedMultiplier();
            decisionInterval /= speedMultiplier;
        }
    }

    public abstract CharacterState DecideNextState();
    public abstract AttackActionData SelectAttack();
    public abstract Vector3 GetPatrolTarget();
    public abstract bool ShouldRetreat();
    public abstract bool ShouldUseSpecialAbility();
    public abstract bool ShouldDodge();
    public abstract bool ShouldUseRecoverySkill();
    /// <summary>
    /// 是否可以做出决策
    /// </summary>
    /// <returns></returns>
    protected bool CanMakeDecision()
    {
        return Time.time - lastDecisionTime >= decisionInterval;
    }
    /// <summary>
    /// 是否有视线看到目标
    /// </summary>
    /// <returns></returns>
    protected bool HasLineOfSightToTarget()
    {
        return controller.CurrentTarget != null;
    }

    protected float GetDistanceToTarget()
    {
        if (controller.CurrentTarget == null) return float.MaxValue;
        return Vector3.Distance(controller.transform.position, controller.CurrentTarget.position);
    }
}
