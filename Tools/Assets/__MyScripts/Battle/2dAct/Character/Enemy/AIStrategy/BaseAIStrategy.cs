using UnityEngine;

public interface IAIStrategy
{
    void Initialize(EnemyAIController controller, EnemyConfigData config);
    EnemyAIState DecideNextState();
    AttackActionData SelectAttack();
    Vector3 GetPatrolTarget();
    bool ShouldRetreat();
    bool ShouldUseSpecialAbility();
}

public abstract class BaseAIStrategy : ScriptableObject, IAIStrategy
{
    [Header("决策设置")]
    [Tooltip("AI决策更新间隔(秒)")]
    [SerializeField] protected float decisionInterval = 0.5f;

    protected EnemyAIController controller;
    protected EnemyConfigData config;
    protected float lastDecisionTime;

    public virtual void Initialize(EnemyAIController controller, EnemyConfigData config)
    {
        this.controller = controller;
        this.config = config;
        
        if (GameDifficultyManager.Instance != null)
        {
            float speedMultiplier = GameDifficultyManager.Instance.GetAIDecisionSpeedMultiplier();
            decisionInterval /= speedMultiplier;
        }
    }

    public abstract EnemyAIState DecideNextState();
    public abstract AttackActionData SelectAttack();
    public abstract Vector3 GetPatrolTarget();
    public abstract bool ShouldRetreat();
    public abstract bool ShouldUseSpecialAbility();

    protected bool CanMakeDecision()
    {
        return Time.time - lastDecisionTime >= decisionInterval;
    }

    protected bool HasLineOfSightToTarget()
    {
        if (controller.CurrentTarget == null) return false;

        Vector3 direction = controller.CurrentTarget.position - controller.transform.position;
        RaycastHit2D hit = Physics2D.Raycast(
            controller.transform.position,
            direction.normalized,
            direction.magnitude,
            config.targetLayers
        );

        return hit.collider == null || hit.collider.transform == controller.CurrentTarget;
    }

    protected float GetDistanceToTarget()
    {
        if (controller.CurrentTarget == null) return float.MaxValue;
        return Vector3.Distance(controller.transform.position, controller.CurrentTarget.position);
    }
}
