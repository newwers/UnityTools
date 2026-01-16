using Senses;
using UnityEngine;

/// <summary>
/// 召唤物控制器
/// 继承自EnemyAIController，复用敌人AI逻辑，仅修改索敌目标
/// </summary>
[DisallowMultipleComponent, RequireComponent(typeof(AttackHitVisualizer), typeof(PlayerAttributes), typeof(BuffSystem)), RequireComponent(typeof(SenseSystemManager))]
public class SummonController : EnemyAIController
{
    /// <summary>
    /// 召唤物数据
    /// </summary>
    private SummonData summonData;

    /// <summary>
    /// 召唤者
    /// </summary>
    private CharacterBase summoner;

    /// <summary>
    /// 生命周期计时器
    /// </summary>
    private float lifetimeTimer = 0f;


    /// <summary>
    /// 召唤物数据属性
    /// </summary>
    public SummonData SummonData => summonData;

    /// <summary>
    /// 召唤者属性
    /// </summary>
    public CharacterBase Summoner => summoner;

    /// <summary>
    /// 生命周期是否结束
    /// </summary>
    public bool IsLifetimeEnded => summonData != null && summonData.lifetime > 0 && lifetimeTimer >= summonData.lifetime;

    protected override void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        //spawnPosition = transform.position;
    }

    /// <summary>
    /// 初始化召唤物
    /// </summary>
    /// <param name="data">召唤物数据</param>
    /// <param name="summoner">召唤者</param>
    public void Initialize(SummonData data, CharacterBase summoner)
    {
        this.summonData = data;
        this.summoner = summoner;
        this.lifetimeTimer = 0f;

        // 获取动画控制器
        animator = GetComponent<Animator>();

        // 初始化属性
        if (PlayerAttributes != null)
        {
            PlayerAttributes.characterAtttibute.maxHealth = data.baseHealth;
            PlayerAttributes.characterAtttibute.currentHealth = data.baseHealth;
            PlayerAttributes.characterAtttibute.baseAttackDamage = data.baseAttack;
            PlayerAttributes.characterAtttibute.baseDefense = data.baseDefense;
        }

        // 应用召唤物配置到EnemyAIController
        ApplySummonConfigToAI();

        Debug.Log($"[SummonController] {data.summonName} 已初始化，召唤者: {summoner.name}");
    }

    /// <summary>
    /// 将召唤物配置应用到AI控制器
    /// </summary>
    private void ApplySummonConfigToAI()
    {
        // 初始化感知系统
        if (senseManager == null)
        {
            senseManager = GetComponent<SenseSystemManager>();
        }

        // 订阅感知事件
        if (senseManager != null)
        {
            senseManager.OnSenseEvent += HandleSenseEvent;
        }

        // 创建或获取EnemyConfigData
        if (configData == null)
        {
            configData = ScriptableObject.CreateInstance<EnemyConfigData>();
        }

        // 将召唤物配置应用到EnemyConfigData
        configData.aiStrategy = summonData.aiStrategy;
        configData.attackRange = summonData.attackRange;
        configData.chaseSpeed = summonData.chaseSpeed;
        configData.patrolSpeed = summonData.patrolSpeed;
        configData.loseTargetDistance = summonData.loseTargetDistance;
        configData.attackCooldown = summonData.attackCooldown;
        configData.idleTimeMin = summonData.idleTimeMin;
        configData.idleTimeMax = summonData.idleTimeMax;
        configData.patrolDuration = summonData.patrolDuration;
        configData.retreatHealthThreshold = summonData.retreatHealthThreshold;
        configData.dodgeHealthThreshold = summonData.dodgeHealthThreshold;
        configData.dodgeCooldown = summonData.dodgeCooldown;
        configData.dodgeDuration = summonData.dodgeDuration;
        configData.dodgeProbability = summonData.dodgeProbability;
        configData.recoverySkillHealthThreshold = summonData.recoverySkillHealthThreshold;
        configData.recoverySkillCooldown = summonData.recoverySkillCooldown;
        configData.recoverySkillProbability = summonData.recoverySkillProbability;
        configData.recoverySkillActions = summonData.recoverySkillActions;

        // 将召唤物的攻击动作列表转换为EnemyConfigData的格式
        configData.attackActions.Clear();
        foreach (var action in summonData.attackActions)
        {
            if (action != null)
            {
                configData.attackActions.Add(action);
            }
        }

        // 应用AI配置
        ApplyAIConfig(configData);
    }

    protected override void Update()
    {
        // 调用父类的Update逻辑，处理AI行为
        base.Update();

        // 更新生命周期计时器
        if (summonData != null && summonData.lifetime > 0)
        {
            lifetimeTimer += Time.deltaTime;
            if (IsLifetimeEnded)
            {
                Die();
            }
        }
    }

    protected override void CheckBossPhaseTransition()
    {
        //召唤物目前没有阶段转换逻辑，留空
    }

    /// <summary>
    /// 重写HandleSenseEvent，修改索敌逻辑，使其攻击敌人而非玩家
    /// </summary>
    /// <param name="senseEvent">感知事件数据</param>
    protected override void HandleVisionEvent(SenseEvent senseEvent)
    {
        // 设置当前目标为检测到的敌人
        if (senseEvent.detectedObject != null)
        {
            if (senseEvent.detectedObject.layer == LayerMask.NameToLayer("Enemy")) // 检测到敌人作为目标
            {
                SetCurrentTarget(senseEvent.detectedObject.transform);

                // 如果当前状态不是追击或攻击，则切换到追击状态
                if (CurrentState != CharacterState.Chase && CurrentState != CharacterState.Attacking && CurrentState != CharacterState.Death)
                {
                    ChangeState(CharacterState.Chase);
                }
            }
            if (senseEvent.detectedObject.layer == LayerMask.NameToLayer("Projectile")) // 检测到投掷物
            {
                if (aiStrategy.ShouldDodge())
                {
                    dodgeDirection = (senseEvent.detectedObject.transform.position - transform.position).normalized;
                    PerformDash(dodgeDirection);
                }
            }
        }
    }

    /// <summary>
    /// 重写CanAttack，添加生命周期检查
    /// </summary>
    public override bool CanAttack()
    {
        return base.CanAttack() && !IsLifetimeEnded;
    }

    /// <summary>
    /// 重写TakeDamage，添加死亡回收逻辑
    /// </summary>
    public override void TakeDamage(DamageInfo damageInfo, AttackActionData actionData, AttackFrameData frameData, CharacterBase attacker)
    {
        base.TakeDamage(damageInfo, actionData, frameData, attacker);

        // 如果死亡，回收召唤物
        if (IsDead)
        {
            Die();
        }
    }

    /// <summary>
    /// 死亡处理
    /// </summary>
    private void Die()
    {
        // 播放死亡动画
        if (animator != null)
        {
            animator.SetTrigger("Death");
        }

        // 延迟回收召唤物
        StartCoroutine(ReturnToPoolAfterDelay(2f));
    }

    /// <summary>
    /// 延迟回收召唤物协程
    /// </summary>
    /// <param name="delay">延迟时间</param>
    private System.Collections.IEnumerator ReturnToPoolAfterDelay(float delay)
    {
        yield return WaitForSecondsCache.WaitForSeconds(delay);

        // 回收召唤物到对象池
        SummonManager.Instance.ReturnSummon(this);
    }


    protected override void OnDrawGizmosSelected()
    {
        // 绘制攻击范围和警戒范围
        if (summonData != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, summonData.attackRange);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, summonData.alertRange);
        }
        // 绘制父类的Gizmos
        else
        {
            base.OnDrawGizmosSelected();
        }
    }
}