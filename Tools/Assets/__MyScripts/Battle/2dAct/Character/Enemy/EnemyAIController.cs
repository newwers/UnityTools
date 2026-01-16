using Senses;
#if UNITY_EDITOR
#endif
using System.Collections;
using UnityEngine;

[DisallowMultipleComponent, RequireComponent(typeof(AttackHitVisualizer), typeof(PlayerAttributes), typeof(SenseSystemManager))]
public class EnemyAIController : CharacterBase
{
    [Header("AI配置")]
    [Tooltip("敌人配置数据")]
    public EnemyConfigData configData;

    [Header("感知系统")]
    [Tooltip("感知系统管理器")]
    public SenseSystemManager senseManager;

    [Header("受击设置")]
    [Tooltip("受击时的闪烁颜色")]
    public Color hitColor = Color.red;
    [Tooltip("受击闪烁持续时间")]
    public float hitFlashDuration = 0.1f;
    [Tooltip("受击特效预制体")]
    public GameObject hitEffectPrefab;
    [Tooltip("受击音效")]
    public AudioClip hitSound;

    [Header("动画设置")]
    [Tooltip("受伤动画触发器名称")]
    public string hurtTriggerName = "Hurt";
    [Tooltip("死亡动画触发器名称")]
    public string dieTriggerName = "Death";
    [Tooltip("闪避动画触发器名称")]
    public string dashTriggerName = "Dash";
    private readonly int stun = Animator.StringToHash("Stun");


    [Header("状态")]
    [SerializeField]
    private bool isStunned = false;

    public override bool IsDead => CurrentState == CharacterState.Death;
    public CharacterState CurrentAIState => CurrentState;

    public bool IsStunned => isStunned;
    public Vector3 PatrolCenter => transform.position;
    public bool IsFacingRight => isFacingRight;

    protected IAIStrategy aiStrategy;
    protected SpriteRenderer spriteRenderer;
    protected Animator animator;
    protected Color originalColor;

    private Vector3 spawnPosition;
    private Vector3 currentPatrolTarget;
    private float idleTimer;
    private float currentIdleTime;
    private float lastAttackTime;
    private float hitFlashTimer;
    private CharacterBase currentTarget_CharacterBase;
    private float stunTimer;

    // 闪避相关变量
    private float dodgeTimer;
    private float lastDodgeTime;
    protected Vector2 dodgeDirection;
    private bool isDodging;

    // 碰撞层相关变量
    private int EnemyLayer;
    private int PlayerLayer;
    private int ProjectileLayer;

    // 恢复技能相关变量
    private float lastRecoverySkillTime;

    private EnemyConfigData currentConfig;
    private int currentBossPhaseIndex = 0;
    private bool hasTriggeredPhaseChange = false;
    private bool isAttaking = false;


    protected override void Awake()
    {
        base.Awake();

        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        spawnPosition = transform.position;
    }

    private void Start()
    {
        if (configData == null)
        {
            LogManager.Log("[EnemyAIController] 未设置敌人配置数据！");
            enabled = false;
            return;
        }

        ApplyConfig(configData);


        PlayerAttributes.characterAtttibute.OnDeath += HandleDeath;
        InitializeAI();

        // 初始化感知系统
        InitializeSenseSystem();

        // 初始化碰撞层
        EnemyLayer = LayerMask.NameToLayer("Enemy");
        PlayerLayer = LayerMask.NameToLayer("Player");
        ProjectileLayer = LayerMask.NameToLayer("Projectile");

        ChangeState(CharacterState.Idle);
        if (animator)
        {
            animator.SetBool("Grounded", true);
        }
    }

    void OnDestroy()
    {
        // 取消订阅感知事件
        if (senseManager != null)
        {
            senseManager.OnSenseEvent -= HandleSenseEvent;
        }
    }

    /// <summary>
    /// 初始化感知系统
    /// </summary>
    private void InitializeSenseSystem()
    {
        // 如果感知系统管理器未赋值，则尝试获取或添加组件
        if (senseManager == null)
        {
            senseManager = GetComponent<SenseSystemManager>();
            if (senseManager == null)
            {
                senseManager = gameObject.AddComponent<SenseSystemManager>();
            }
        }

        // 订阅感知事件
        if (senseManager != null)
        {
            senseManager.OnSenseEvent += HandleSenseEvent;
        }
    }

    protected virtual void Update()
    {
        if (IsDead) return;

        UpdateStun();
        UpdateHitFlash();
        CheckBossPhaseTransition();

        if (!isStunned)
        {
            UpdateAI();
        }
    }

    private void ApplyConfig(EnemyConfigData config)
    {
        currentConfig = config;

        if (config.attributes != null)
        {
            PlayerAttributes.characterAtttibute = config.attributes;
            PlayerAttributes.Initialize();

            if (GameDifficultyManager.Instance != null)
            {
                GameDifficultyManager.Instance.ApplyDifficultyToAttributes(PlayerAttributes.characterAtttibute);
                LogManager.Log($"[EnemyAIController] 应用游戏难度: {GameDifficultyManager.Instance.GetDifficultyName()}");
            }
        }
    }

    protected virtual void CheckBossPhaseTransition()
    {
        if (currentConfig.difficulty != EnemyDifficulty.Boss) return;
        if (currentConfig.bossPhases == null || currentConfig.bossPhases.Count == 0) return;
        if (hasTriggeredPhaseChange) return;

        float healthPercent = PlayerAttributes.characterAtttibute.currentHealth /
                             PlayerAttributes.characterAtttibute.maxHealth;

        for (int i = currentBossPhaseIndex; i < currentConfig.bossPhases.Count; i++)
        {
            BossPhaseConfig phase = currentConfig.bossPhases[i];

            if (healthPercent <= phase.healthPercentThreshold)
            {
                TransitionToBossPhase(phase, i);
                break;
            }
        }
    }

    private void TransitionToBossPhase(BossPhaseConfig phase, int phaseIndex)
    {
        if (phase.phaseConfig == null)
        {
            LogManager.Log($"[EnemyAIController] Boss阶段 {phase.phaseName} 未配置！");
            return;
        }

        hasTriggeredPhaseChange = true;
        currentBossPhaseIndex = phaseIndex + 1;

        LogManager.Log($"[EnemyAIController] Boss进入 {phase.phaseName}！");

        float currentHealthPercent = PlayerAttributes.characterAtttibute.currentHealth /
                                    PlayerAttributes.characterAtttibute.maxHealth;
        //todo:播放切换特效和音效
        ApplyConfig(phase.phaseConfig);

        PlayerAttributes.characterAtttibute.currentHealth = PlayerAttributes.characterAtttibute.maxHealth * currentHealthPercent;

        hasTriggeredPhaseChange = false;
    }

    private void UpdateStun()
    {
        if (isStunned)
        {
            stunTimer -= Time.deltaTime;
            if (stunTimer <= 0f)
            {
                RecoverFromStun();
            }
        }
    }

    private void UpdateHitFlash()
    {
        if (hitFlashTimer > 0)
        {
            hitFlashTimer -= Time.deltaTime;
            if (hitFlashTimer <= 0 && spriteRenderer != null)
            {
                spriteRenderer.color = originalColor;
            }
        }
    }

    private void UpdateAI()
    {
        var newState = aiStrategy.DecideNextState();
        if (newState != currentState)
        {
            ChangeState(newState);
        }

        switch (currentState)
        {
            case CharacterState.Idle:
                UpdateIdleState();
                break;
            case CharacterState.Patrol:
                UpdatePatrolState();
                break;
            case CharacterState.Chase:
                UpdateChaseState();
                break;
            case CharacterState.Attacking:
                UpdateAttackState();
                break;
            case CharacterState.SpecialAttacking:
                ExecuteSpecialAttackState();
                break;
            case CharacterState.Retreat:
                ExecuteRetreatState();
                break;
            case CharacterState.Dodging:
                UpdateDodgeState();
                break;
            case CharacterState.Stunned:
                //ExecuteStunnedState();//todo:执行眩晕
                break;
        }
    }

    private void UpdateIdleState()
    {
        idleTimer += Time.deltaTime;

        if (idleTimer >= currentIdleTime)
        {
            ChangeState(CharacterState.Patrol);
            return;
        }

        // 使用Move方法停止移动
        Move(Vector2.zero);

        if (animator != null)
        {
            animator.SetFloat("MoveX", 0);
            animator.SetFloat("animMoveSpeed", 1);
        }
    }

    private void UpdatePatrolState()
    {
        float distanceToTarget = Vector3.Distance(transform.position, currentPatrolTarget);

        if (distanceToTarget < 0.5f)
        {
            ChangeState(CharacterState.Idle);
            return;
        }

        Vector3 direction = (currentPatrolTarget - transform.position).normalized;

        // 使用Move方法处理巡逻移动
        Move(new Vector2(direction.x, 0));

        if (animator != null)
        {
            animator.SetFloat("MoveX", 1);
            animator.SetFloat("animMoveSpeed", Mathf.Abs(currentConfig.patrolSpeed));
        }
    }

    private void UpdateChaseState()
    {
        if (currentTarget == null)
        {
            ChangeState(CharacterState.Idle);
            return;
        }

        float distanceToTarget = Vector3.Distance(transform.position, currentTarget.transform.position);

        if (distanceToTarget > currentConfig.loseTargetDistance)
        {
            SetCurrentTarget(null);
            ChangeState(CharacterState.Idle);
            return;
        }

        if (distanceToTarget <= currentConfig.attackRange)
        {
            ChangeState(CharacterState.Attacking);
            return;
        }

        Vector3 direction = (currentTarget.transform.position - transform.position).normalized;

        // 使用Move方法处理追击移动
        Move(new Vector2(direction.x, 0));

        if (animator != null)
        {
            animator.SetFloat("MoveX", 1);
            animator.SetFloat("animMoveSpeed", Mathf.Abs(currentConfig.chaseSpeed));
        }
    }

    private void UpdateAttackState()
    {
        if (currentTarget == null)
        {
            ChangeState(CharacterState.Idle);
            return;
        }

        float distanceToTarget = Vector3.Distance(transform.position, currentTarget.transform.position);

        if (distanceToTarget > currentConfig.attackRange * 1.5f)
        {
            ChangeState(CharacterState.Chase);
            return;
        }

        // 使用Move方法停止移动
        Move(Vector2.zero);

        Vector3 direction = (currentTarget.transform.position - transform.position).normalized;
        if ((direction.x > 0 && !isFacingRight) || (direction.x < 0 && isFacingRight))
        {
            Flip();
        }

        if (Time.time - lastAttackTime >= GetEffectiveAttackCooldown())
        {
            PerformAttack();
        }

        if (animator != null)
        {
            animator.SetFloat("MoveX", 0);
            animator.SetFloat("animMoveSpeed", 1);
        }
    }


    public override void PerformAttack()
    {
        var attackData = aiStrategy.SelectAttack();
        if (attackData == null) return;

        PerformAttack(attackData);
    }

    public override void PerformAttack(AttackActionData attackData)
    {
        if (attackData == null) return;

        lastAttackTime = Time.time;

        // 攻击实现
        StartCoroutine(ExecuteAttackWithHitDetector(attackData));
    }

    public override bool CanAttack()
    {
        return !IsDead && !isStunned && currentTarget != null;
    }

    #region IMoveable 接口重写
    public override bool CanMove()
    {
        // 死亡或硬直状态下不能移动
        if (IsDead || isStunned)
            return false;

        return true;
    }

    public override void Move(Vector2 direction)
    {
        if (!CanMove())
            return;

        // 根据当前状态执行不同的移动速度
        float moveSpeed = 0f;
        switch (currentState)
        {
            case CharacterState.Patrol:
                moveSpeed = currentConfig.patrolSpeed;
                break;
            case CharacterState.Chase:
                moveSpeed = currentConfig.chaseSpeed;
                break;
            case CharacterState.Retreat:
                moveSpeed = currentConfig.patrolSpeed;
                break;
            case CharacterState.Dodging:
                moveSpeed = currentConfig.chaseSpeed * 5f;
                break;
            default:
                moveSpeed = 0f;
                break;
        }

        // 应用移动速度
        rb.linearVelocity = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);

        // 转向处理
        if ((direction.x > 0 && !isFacingRight) || (direction.x < 0 && isFacingRight))
        {
            Flip();
        }
    }

    public override bool IsMoving()
    {
        // 根据刚体速度判断是否正在移动
        return Mathf.Abs(rb.linearVelocity.x) > 0.1f;
    }
    #endregion IMoveable 接口重写

    private IEnumerator ExecuteAttackWithHitDetector(AttackActionData attackActionData)
    {
        if (attackActionData == null) yield break;

        isAttaking = true;

        CharacterBase targetedEnemy = currentTarget_CharacterBase;
        CharacterAttackController.ApplySkillEffectsOnCast(attackActionData, this, targetedEnemy);

        string attackId = AttackHitDetector.Instance.StartAttackDetection(
            attackActionData,
            transform.position,
            isFacingRight,
            this,
            targetedEnemy
        );

        if (animator != null && attackActionData.animationParameters != null)
        {
            SetActionAnimationParameter(attackActionData);
        }

        float attackTimer = 0f;
        while (attackTimer < attackActionData.windUpTime)
        {
            attackTimer += Time.deltaTime;
            yield return null;
        }

        float activeTimer = 0f;
        while (activeTimer < attackActionData.activeTime)
        {
            activeTimer += Time.deltaTime;
            attackTimer += Time.deltaTime;

            AttackHitDetector.Instance.CheckHitForFrame(
                attackId,
                attackActionData,
                transform.position,
                isFacingRight,
                attackTimer,
                this
            );

            yield return null;
        }

        float recoveryTimer = 0f;
        while (recoveryTimer < attackActionData.recoveryTime)
        {
            recoveryTimer += Time.deltaTime;
            attackTimer += Time.deltaTime;
            yield return null;
        }

        CharacterAttackController.ApplySkillEffectsOnComplete(attackActionData, this, attackId);

        AttackHitDetector.Instance.EndAttackDetection(attackId);

        isAttaking = false;

        // 攻击结束后自动恢复到合适的状态
        if (CurrentState == CharacterState.Attacking || CurrentState == CharacterState.SpecialAttacking)
        {
            if (currentTarget != null)
            {
                float distanceToTarget = Vector3.Distance(transform.position, currentTarget.transform.position);
                if (distanceToTarget <= currentConfig.attackRange)
                {
                    // 仍在攻击范围内，保持攻击状态
                }
                else if (distanceToTarget <= currentConfig.loseTargetDistance)
                {
                    ChangeState(CharacterState.Chase);
                }
                else
                {
                    ChangeState(CharacterState.Idle);
                }
            }
            else
            {
                ChangeState(CharacterState.Idle);
            }
        }
    }

    private void SetActionAnimationParameter(ActionData actionData)
    {
        CharacterAnimation.SetActionAnimationParameter(animator, actionData);
    }

    public override void TakeDamage(DamageInfo damageInfo, AttackActionData attackActionData, AttackFrameData frameData, CharacterBase attacker)
    {
        base.TakeDamage(damageInfo, attackActionData, frameData, attacker);
        if (IsDead) return;

        if (BuffSystem != null && PlayerAttributes.characterAtttibute.IsInvincible())
        {
            LogManager.Log($"[EnemyAIController] 处于无敌状态，免疫伤害");
            return;
        }

        Vector2 knockbackDirection = (transform.position - attacker.transform.position).normalized;

        Vector2 finalKnockbackForce = frameData.knockbackForce;
        if (damageInfo.skillData != null)
        {
            finalKnockbackForce += damageInfo.skillData.knockbackForce;
        }

        ApplyDamageWithCalculation(damageInfo, attackActionData, frameData, attacker);

        if (!isStunned)
        {
            bool shouldPlayHitAnimation = ShouldPlayHitAnimation(attackActionData.priority);

            if (shouldPlayHitAnimation)
            {
                if (animator != null && !string.IsNullOrEmpty(hurtTriggerName))
                {
                    animator.SetTrigger(hurtTriggerName);
                }
            }
        }

        PlayHitFeedback(transform.position);

        if (rb != null && finalKnockbackForce.magnitude > 0)
        {
            rb.AddForce(knockbackDirection.normalized * finalKnockbackForce, ForceMode2D.Impulse);
        }
    }

    private void ApplyDamageWithCalculation(DamageInfo damageInfo, AttackActionData attackActionData, AttackFrameData frameData, CharacterBase attacker)
    {
        var attackerAttributes = damageInfo.attacker.PlayerAttributes?.characterAtttibute;

        if (attackerAttributes == null)
        {
            return;
        }

        DamageResult result = DamageCalculator.CalculateDamage(damageInfo, damageInfo.attacker, this);

        DamageDisplayHelper.ShowDamageOnCharacter(result, transform);

        if (result.isMiss)
        {
            LogManager.Log("[EnemyAIController] 攻击未命中");
            return;
        }

        if (!result.isBlocked && result.healthDamage > 0)
        {
            PlayerAttributes.characterAtttibute.ChangeHealth(-result.healthDamage, damageInfo.attacker);
            LogManager.Log($"[EnemyAIController] 受到伤害: {result.healthDamage}{(result.isCritical ? " (暴击!)" : "")}, 剩余血量: {PlayerAttributes.characterAtttibute.currentHealth}/{PlayerAttributes.characterAtttibute.maxHealth}");
        }
        else if (result.isBlocked)
        {
            LogManager.Log("[EnemyAIController] 攻击被格挡");
        }
    }

    private bool ShouldPlayHitAnimation(int priority)
    {
        return CurrentState != CharacterState.Death && CurrentState != CharacterState.Stunned;
    }

    private void PlayHitFeedback(Vector2 hitPosition)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = hitColor;
            hitFlashTimer = hitFlashDuration;
        }

        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, hitPosition, Quaternion.identity);
        }

        if (hitSound != null)
        {
        }
    }

    public override void ApplyStun(float duration)
    {
        if (IsDead) return;

        if (isStunned)
        {
            stunTimer = duration;
            LogManager.Log($"[EnemyAIController] 刷新硬直时间: {duration}秒");
            return;
        }

        isStunned = true;
        stunTimer = duration;

        LogManager.Log($"[EnemyAIController] 被硬直! 持续时间: {duration}秒");

        StopAllCoroutines();
        rb.linearVelocity = Vector2.zero;

        ChangeState(CharacterState.Stunned);
        OnStunned();
    }

    private void RecoverFromStun()
    {
        isStunned = false;

        LogManager.Log($"[EnemyAIController] 硬直结束");

        OnStunnedEnd();
        ChangeState(CharacterState.Idle);
    }

    private void OnStunned()
    {
        animator.SetBool(stun, true);
    }

    private void OnStunnedEnd()
    {
        animator.SetBool(stun, false);
    }

    private void HandleDeath(CharacterBase killer = null)
    {
        if (IsDead) return;

        LogManager.Log($"[EnemyAIController] 敌人死亡");

        StopAllCoroutines();

        if (animator != null && !string.IsNullOrEmpty(dieTriggerName))
        {
            animator.SetTrigger(dieTriggerName);
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.gray;
        }

        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        if (BuffSystem != null)
        {
            BuffSystem.ClearAllBuffs();
        }

        ChangeState(CharacterState.Death);
    }


    private float GetEffectiveAttackCooldown()
    {
        float baseCooldown = currentConfig.attackCooldown;
        if (GameDifficultyManager.Instance != null)
        {
            return baseCooldown * GameDifficultyManager.Instance.GetAttackCooldownMultiplier();
        }
        return baseCooldown;
    }

    public override void ChangeState(CharacterState newState)
    {
        if (CurrentState == newState) return;

        OnStateExit(CurrentState);

        CharacterState previousState = CurrentState;
        base.ChangeState(newState);

        // 触发状态变更事件
        //OnStateChanged?.Invoke(previousState, newState);
        LogManager.Log($"[EnemyAIController] 状态切换: {previousState} -> {CurrentState}");
    }

    protected override void OnStateEnter(CharacterState state)
    {
        switch (state)
        {
            case CharacterState.Idle:
                currentIdleTime = Random.Range(currentConfig.idleTimeMin, currentConfig.idleTimeMax);
                idleTimer = 0f;
                break;
            case CharacterState.Patrol:
                currentPatrolTarget = currentConfig.patrolArea.GetRandomPointInArea(spawnPosition);
                break;
            case CharacterState.Dodging:
                animator.SetTrigger(dashTriggerName);
                animator.SetFloat("animRollSpeed", 0.643f / currentConfig.dodgeDuration);//闪避速度倍数计算,后面这边可以将动画时长提取成配置
                break;
            case CharacterState.Attacking:
            case CharacterState.SpecialAttacking:
                // 攻击状态进入时的处理
                break;
            case CharacterState.Stunned:
                // 硬直状态进入时的处理
                break;
        }
    }

    protected override void OnStateExit(CharacterState state)
    {
        switch (state)
        {
            case CharacterState.Dodging:
                // 闪避状态退出时的处理
                break;
            case CharacterState.Attacking:
            case CharacterState.SpecialAttacking:
                // 攻击状态退出时的处理
                break;
        }
    }

    /// <summary>
    /// 强制刷新状态到合适的状态
    /// </summary>
    public void ForceRefreshState()
    {
        if (IsDead)
        {
            ChangeState(CharacterState.Death);
            return;
        }

        if (isStunned)
        {
            ChangeState(CharacterState.Stunned);
            return;
        }

        if (currentTarget != null)
        {
            float distanceToTarget = Vector3.Distance(transform.position, currentTarget.transform.position);
            if (distanceToTarget <= currentConfig.attackRange)
            {
                ChangeState(CharacterState.Attacking);
            }
            else if (distanceToTarget <= currentConfig.loseTargetDistance)
            {
                ChangeState(CharacterState.Chase);
            }
            else
            {
                ChangeState(CharacterState.Idle);
            }
        }
        else
        {
            ChangeState(CharacterState.Idle);
        }
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    protected virtual void OnDrawGizmosSelected()
    {
        if (currentConfig == null) return;

        // 绘制攻击范围
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, currentConfig.attackRange);

        // 绘制巡逻区域
        Vector3 center = Application.isPlaying ? spawnPosition : transform.position;
        currentConfig.patrolArea.DrawGizmos(center, Color.green);

    }



    // 公共方法
    public void SetCurrentTarget(Transform target)
    {
        currentTarget = target;
        currentTarget_CharacterBase = target != null ? target.GetComponent<CharacterBase>() : null;
    }

    /// <summary>
    /// 启用或禁用视觉感知
    /// </summary>
    /// <param name="enable">是否启用视觉感知</param>
    public void EnableVision(bool enable)
    {
        if (senseManager != null)
        {
            senseManager.SetVisionEnabled(enable);
            LogManager.Log($"[EnemyAIController] 视觉感知已{(enable ? "启用" : "禁用")}");
        }
    }

    public void ApplyAIConfig(EnemyConfigData newConfig)
    {
        configData = newConfig;
        aiStrategy = GetOrCreateStrategy(newConfig);
        aiStrategy.Initialize(this, newConfig);
    }

    private IAIStrategy GetOrCreateStrategy(EnemyConfigData config)
    {
        if (config.aiStrategy != null)
        {
            return Instantiate(config.aiStrategy);
        }

        return CreateDefaultStrategyForDifficulty(config.difficulty);
    }

    private IAIStrategy CreateDefaultStrategyForDifficulty(EnemyDifficulty difficulty)
    {
        BaseAIStrategy strategy = null;

        bool useEnhancedAI = GameDifficultyManager.Instance != null &&
                            GameDifficultyManager.Instance.ShouldUseEnhancedAI();

        if (useEnhancedAI)
        {
            strategy = ScriptableObject.CreateInstance<HellAIStrategy>();
            LogManager.Log($"[EnemyAIController] 使用地狱AI策略");
        }
        else
        {
            switch (difficulty)
            {
                case EnemyDifficulty.Elite:
                    strategy = ScriptableObject.CreateInstance<EliteAIStrategy>();
                    break;
                case EnemyDifficulty.Boss:
                    strategy = ScriptableObject.CreateInstance<BossAIStrategy>();
                    break;
                default:
                    strategy = ScriptableObject.CreateInstance<NormalAIStrategy>();
                    break;
            }
        }

        return strategy;
    }

    private void InitializeAI()
    {
        if (configData == null)
        {
            Debug.LogError("EnemyController: AI配置缺失!");
            return;
        }

        aiStrategy = GetOrCreateStrategy(configData);
        aiStrategy.Initialize(this, configData);
    }

    /// <summary>
    /// 处理感知事件
    /// </summary>
    /// <param name="senseEvent">感知事件数据</param>
    protected virtual void HandleSenseEvent(SenseEvent senseEvent)
    {
        // 只处理视觉感知事件
        if (senseEvent.senseType == SenseType.Vision)
        {
            HandleVisionEvent(senseEvent);
        }
    }

    /// <summary>
    /// 处理视觉感知事件
    /// </summary>
    /// <param name="senseEvent">视觉感知事件数据</param>
    protected virtual void HandleVisionEvent(SenseEvent senseEvent)
    {
        // 设置当前目标为检测到的对象
        if (senseEvent.detectedObject != null)
        {
            if (senseEvent.detectedObject.layer == PlayerLayer)//检测到玩家
            {
                SetCurrentTarget(senseEvent.detectedObject.transform);

                // 如果当前状态不是追击或攻击，则切换到追击状态
                if (CurrentState != CharacterState.Chase && CurrentState != CharacterState.Attacking && CurrentState != CharacterState.Death)
                {
                    ChangeState(CharacterState.Chase);
                }
            }
            if (senseEvent.detectedObject.layer == ProjectileLayer)//检测到投掷物
            {
                if (aiStrategy.ShouldDodge())
                {
                    dodgeDirection = (senseEvent.detectedObject.transform.position - transform.position).normalized;
                    PerformDash(dodgeDirection);
                }
            }

            //LogManager.Log($"[EnemyAIController] 视觉检测到: {senseEvent.detectedObject.name}, 强度: {senseEvent.intensity}");

        }
    }


    private void ExecuteSpecialAttackState()
    {
        if (currentTarget == null)
        {
            ChangeState(CharacterState.Idle);
            return;
        }

        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        Vector3 direction = (currentTarget.transform.position - transform.position).normalized;
        if ((direction.x > 0 && !isFacingRight) || (direction.x < 0 && isFacingRight))
        {
            Flip();
        }

        if (Time.time - lastAttackTime >= currentConfig.attackCooldown)
        {
            PerformSpecialAttack();
        }
    }

    private void PerformSpecialAttack()
    {
        var attackData = aiStrategy.SelectAttack();
        if (attackData == null) return;

        lastAttackTime = Time.time;
        StartCoroutine(ExecuteAttackWithHitDetector(attackData));
    }

    private void ExecuteRetreatState()
    {
        if (currentTarget == null) return;

        // 远离目标移动
        Vector3 retreatDirection = (transform.position - currentTarget.position).normalized;

        // 使用Move方法处理撤退移动
        Move(new Vector2(retreatDirection.x, 0));
    }

    private void UpdateDodgeState()
    {
        dodgeTimer += Time.deltaTime;

        // 执行闪避移动
        if (dodgeTimer < currentConfig.dodgeDuration) // 闪避动作持续时间
        {
            // 使用Move方法处理闪避移动
            Move(dodgeDirection);
        }
        else
        {
            // 闪避结束，使用Move方法停止移动
            Move(Vector2.zero);

            // 退出闪避状态，根据实际情况切换到合适的状态
            if (currentTarget != null)
            {
                float distanceToTarget = Vector3.Distance(transform.position, currentTarget.transform.position);
                if (distanceToTarget <= currentConfig.attackRange)
                {
                    ChangeState(CharacterState.Attacking);
                }
                else if (distanceToTarget <= currentConfig.loseTargetDistance)
                {
                    ChangeState(CharacterState.Chase);
                }
                else
                {
                    ChangeState(CharacterState.Idle);
                }
            }
            else
            {
                ChangeState(CharacterState.Idle);
            }
        }

        //if (animator != null)
        //{
        //    animator.SetFloat("MoveX", Mathf.Abs(dodgeDirection.x));
        //    //animator.SetFloat("animMoveSpeed", Mathf.Abs(currentConfig.chaseSpeed * 2f));
        //}
    }

    private void MoveToPosition(Vector3 targetPosition, float speed)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;

        // 使用Move方法处理移动
        Move(new Vector2(direction.x, 0));
    }


    /// <summary>
    /// 检查是否可以闪避
    /// 状态检测
    /// 冷却检测
    /// 生命条件检测
    /// 
    /// </summary>
    /// <returns>是否可以闪避</returns>
    public override bool CanDash()
    {
        if (IsDead || isStunned || isAttaking || isDodging) return false;
        if (CurrentState == CharacterState.Hurt || CurrentState == CharacterState.Retreat) return false;

        // 检查冷却时间
        if (Time.time - lastDodgeTime < currentConfig.dodgeCooldown)
        {
            return false;
        }

        // 检查生命值阈值
        float healthPercent = PlayerAttributes.characterAtttibute.currentHealth / PlayerAttributes.characterAtttibute.maxHealth;
        if (healthPercent > currentConfig.dodgeHealthThreshold)
        {
            return false;
        }

        //if (CurrentTarget == null)//当前没有目标不能闪避
        //{
        //    return false;
        //}

        // 检查是否有投掷物在身前
        if (ProjectileManager.Instance.HasProjectileInFront(this))
        {
            return true;
        }

        if (currentTarget_CharacterBase && (currentTarget_CharacterBase.IsDead || currentTarget_CharacterBase.IsAttacking() == false))//目标死了或者没攻击不能闪避
        {
            return false;
        }

        return true;
    }


    public override void PerformDash(Vector2 direction)
    {
        if (!CanDash()) return;

        lastDodgeTime = Time.time;
        dodgeTimer = 0f;
        dodgeDirection = direction.normalized;
        isDodging = true;

        // 记录闪避日志
        LogManager.Log($"[EnemyAIController] 执行闪避! 方向: {dodgeDirection}");

        // 闪避时忽略敌人层和玩家层之间的碰撞
        Physics2D.IgnoreLayerCollision(EnemyLayer, PlayerLayer, true);
        Physics2D.IgnoreLayerCollision(EnemyLayer, ProjectileLayer, true);

        // 启动闪避无敌帧协程
        StartCoroutine(HandleDodgeInvincibility());

        // 切换到闪避状态
        ChangeState(CharacterState.Dodging);
    }

    public override bool IsDodging()
    {
        return isDodging;
    }

    /// <summary>
    /// 执行恢复技能
    /// </summary>
    public void PerformRecoverySkill()
    {
        lastRecoverySkillTime = Time.time;

        // 播放恢复技能动画
        if (currentConfig.recoverySkillActions != null && currentConfig.recoverySkillActions.Count > 0)
        {
            var recoveryAction = currentConfig.recoverySkillActions[Random.Range(0, currentConfig.recoverySkillActions.Count)];
            if (recoveryAction != null && animator != null && recoveryAction.animationParameters != null)
            {
                PerformAttack(recoveryAction);
            }
        }
    }

    /// <summary>
    /// 检查是否可以使用恢复技能
    /// </summary>
    /// <returns>是否可以使用恢复技能</returns>
    public bool CanUseRecoverySkill()
    {
        if (IsDead || isStunned) return false;

        // 检查冷却时间
        if (Time.time - lastRecoverySkillTime < currentConfig.recoverySkillCooldown)
        {
            return false;
        }

        // 检查生命值阈值
        float healthPercent = PlayerAttributes.characterAtttibute.currentHealth / PlayerAttributes.characterAtttibute.maxHealth;
        if (healthPercent > currentConfig.recoverySkillHealthThreshold)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// 处理闪避无敌帧
    /// </summary>
    private IEnumerator HandleDodgeInvincibility()
    {
        // 设置无敌状态
        if (PlayerAttributes != null && PlayerAttributes.characterAtttibute != null)
        {
            PlayerAttributes.characterAtttibute.isDodging = true;
        }

        // 闪避无敌持续时间
        yield return WaitForSecondsCache.WaitForSeconds(currentConfig.dodgeDuration);

        // 结束无敌状态
        if (PlayerAttributes != null && PlayerAttributes.characterAtttibute != null)
        {
            PlayerAttributes.characterAtttibute.isDodging = false;
        }

        isDodging = false;

        // 恢复碰撞
        Physics2D.IgnoreLayerCollision(EnemyLayer, PlayerLayer, false);
        Physics2D.IgnoreLayerCollision(EnemyLayer, ProjectileLayer, false);

        LogManager.Log($"[EnemyAIController] 闪避无敌帧结束");
    }


    public override bool IsAttacking()
    {
        return isAttaking;
    }
}
