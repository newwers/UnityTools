using Senses;
#if UNITY_EDITOR
#endif
using System.Collections;
using UnityEngine;

public enum EnemyAIState
{
    [InspectorName("待机")]
    Idle,           // 待机
    [InspectorName("巡逻")]
    Patrol,         // 巡逻
    [InspectorName("追击")]
    Chase,          // 追击
    [InspectorName("攻击")]
    Attack,         // 攻击
    [InspectorName("特殊攻击")]
    SpecialAttack,  // 特殊攻击
    [InspectorName("撤退")]
    Retreat,        // 撤退
    [InspectorName("硬直")]
    Stunned,        // 硬直
    [InspectorName("受伤")]
    Hurt,           //受伤
    [InspectorName("死亡")]
    Death            // 死亡
}

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
    private readonly int stun = Animator.StringToHash("Stun");


    [Header("状态")]
    [SerializeField]
    private EnemyAIState currentState = EnemyAIState.Idle;
    [SerializeField]
    private bool isDead = false;
    [SerializeField]
    private bool isStunned = false;

    public override bool IsDead => isDead;
    public EnemyAIState CurrentAIState => currentState;
    public Transform CurrentTarget => currentTarget;
    public bool IsStunned => isStunned;
    public Vector3 PatrolCenter => transform.position;
    public bool IsFacingRight => isFacingRight;

    private IAIStrategy aiStrategy;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private Color originalColor;

    private Vector3 spawnPosition;
    private Vector3 currentPatrolTarget;
    private float idleTimer;
    private float currentIdleTime;
    private float lastAttackTime;
    private float hitFlashTimer;
    private Transform currentTarget;
    private float stunTimer;

    private EnemyConfigData currentConfig;
    private int currentBossPhaseIndex = 0;
    private bool hasTriggeredPhaseChange = false;


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

        ChangeState(EnemyAIState.Idle);
        if (animator)
        {
            animator.SetBool("Grounded", true);
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

    private void Update()
    {
        if (isDead) return;

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
            PlayerAttributes.characterAtttibute = new CharacterAttributes
            {
                maxHealth = config.attributes.maxHealth,
                currentHealth = config.attributes.maxHealth,
                healthRegenRate = config.attributes.healthRegenRate,
                maxEnergy = config.attributes.maxEnergy,
                currentEnergy = config.attributes.maxEnergy,
                energyRegenRate = config.attributes.energyRegenRate
            };
            PlayerAttributes.Initialize();

            if (GameDifficultyManager.Instance != null)
            {
                GameDifficultyManager.Instance.ApplyDifficultyToAttributes(PlayerAttributes.characterAtttibute);
                LogManager.Log($"[EnemyAIController] 应用游戏难度: {GameDifficultyManager.Instance.GetDifficultyName()}");
            }
        }
    }

    private void CheckBossPhaseTransition()
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
            case EnemyAIState.Idle:
                UpdateIdleState();
                break;
            case EnemyAIState.Patrol:
                UpdatePatrolState();
                break;
            case EnemyAIState.Chase:
                UpdateChaseState();
                break;
            case EnemyAIState.Attack:
                UpdateAttackState();
                break;
            case EnemyAIState.SpecialAttack:
                ExecuteSpecialAttackState();
                break;
            case EnemyAIState.Retreat:
                ExecuteRetreatState();
                break;
            case EnemyAIState.Stunned:
                //ExecuteStunnedState();//todo:执行眩晕
                break;
        }
    }

    private void UpdateIdleState()
    {
        //GameObject target = FindNearestTarget();
        //if (target != null)
        //{
        //    currentTarget = target.transform;
        //    ChangeState(EnemyAIState.Chase);
        //    return;
        //}

        idleTimer += Time.deltaTime;

        if (idleTimer >= currentIdleTime)
        {
            ChangeState(EnemyAIState.Patrol);
            return;
        }

        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        if (animator != null)
        {
            animator.SetFloat("MoveX", 0);
            animator.SetFloat("animMoveSpeed", 1);
        }
    }

    private void UpdatePatrolState()
    {
        //GameObject target = FindNearestTarget();
        //if (target != null)
        //{
        //    currentTarget = target.transform;
        //    ChangeState(EnemyAIState.Chase);
        //    return;
        //}

        float distanceToTarget = Vector3.Distance(transform.position, currentPatrolTarget);

        if (distanceToTarget < 0.5f)
        {
            ChangeState(EnemyAIState.Idle);
            return;
        }

        Vector3 direction = (currentPatrolTarget - transform.position).normalized;

        if ((direction.x > 0 && !isFacingRight) || (direction.x < 0 && isFacingRight))
        {
            Flip();
        }

        rb.linearVelocity = new Vector2(direction.x * currentConfig.patrolSpeed, rb.linearVelocity.y);

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
            ChangeState(EnemyAIState.Idle);
            return;
        }

        float distanceToTarget = Vector3.Distance(transform.position, currentTarget.transform.position);

        if (distanceToTarget > currentConfig.loseTargetDistance)
        {
            currentTarget = null;
            ChangeState(EnemyAIState.Idle);
            return;
        }

        if (distanceToTarget <= currentConfig.attackRange)
        {
            ChangeState(EnemyAIState.Attack);
            return;
        }

        Vector3 direction = (currentTarget.transform.position - transform.position).normalized;
        if ((direction.x > 0 && !isFacingRight) || (direction.x < 0 && isFacingRight))
        {
            Flip();
        }

        rb.linearVelocity = new Vector2(direction.x * currentConfig.chaseSpeed, rb.linearVelocity.y);

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
            ChangeState(EnemyAIState.Idle);
            return;
        }

        float distanceToTarget = Vector3.Distance(transform.position, currentTarget.transform.position);

        if (distanceToTarget > currentConfig.attackRange * 1.5f)
        {
            ChangeState(EnemyAIState.Chase);
            return;
        }

        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

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

    private void PerformAttack()
    {
        var attackData = aiStrategy.SelectAttack();
        if (attackData == null) return;

        lastAttackTime = Time.time;
        StartCoroutine(ExecuteAttackWithHitDetector(attackData));
    }

    private IEnumerator ExecuteAttackWithHitDetector(AttackActionData attackActionData)
    {
        if (attackActionData == null) yield break;

        CharacterBase targetedEnemy = currentTarget != null ? currentTarget.GetComponent<CharacterBase>() : null;
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

    }

    private void SetActionAnimationParameter(ActionData actionData)
    {
        CharacterAnimation.SetActionAnimationParameter(animator, actionData);
    }

    public override void TakeDamage(DamageInfo damageInfo, AttackActionData attackActionData, AttackFrameData frameData, CharacterBase attacker)
    {
        base.TakeDamage(damageInfo, attackActionData, frameData, attacker);
        if (isDead) return;

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
        return currentState != EnemyAIState.Death || currentState != EnemyAIState.Stunned;
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
        if (isDead) return;

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

        ChangeState(EnemyAIState.Stunned);
        OnStunned();
    }

    private void RecoverFromStun()
    {
        isStunned = false;

        LogManager.Log($"[EnemyAIController] 硬直结束");

        OnStunnedEnd();
        ChangeState(EnemyAIState.Idle);
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
        if (isDead) return;

        isDead = true;

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

        ChangeState(EnemyAIState.Death);
    }

    private GameObject FindNearestTarget()
    {
        Collider2D[] colliders = GetTargetsInDetectRange();
        GameObject nearest = null;
        float minDistance = float.MaxValue;

        foreach (var col in colliders)
        {
            float distance = Vector3.Distance(transform.position, col.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = col.gameObject;
            }
        }

        return nearest;
    }

    private Collider2D[] GetTargetsInDetectRange()
    {
        if (currentConfig.detectRangeType == DetectRangeType.Circle)
        {
            float effectiveDetectRange = GetEffectiveDetectRange();
            return Physics2D.OverlapCircleAll(transform.position, effectiveDetectRange, currentConfig.targetLayers);
        }
        else
        {
            float effectiveWidth = GetEffectiveDetectWidth();
            float effectiveHeight = GetEffectiveDetectHeight();
            Vector2 boxSize = new Vector2(effectiveWidth, effectiveHeight);
            return Physics2D.OverlapBoxAll(transform.position, boxSize, 0f, currentConfig.targetLayers);
        }
    }

    private float GetEffectiveDetectRange()
    {
        float baseRange = currentConfig.detectRange;
        if (GameDifficultyManager.Instance != null)
        {
            return baseRange * GameDifficultyManager.Instance.GetDetectRangeMultiplier();
        }
        return baseRange;
    }

    private float GetEffectiveDetectWidth()
    {
        float baseWidth = currentConfig.detectWidth;
        if (GameDifficultyManager.Instance != null)
        {
            return baseWidth * GameDifficultyManager.Instance.GetDetectRangeMultiplier();
        }
        return baseWidth;
    }

    private float GetEffectiveDetectHeight()
    {
        float baseHeight = currentConfig.detectHeight;
        if (GameDifficultyManager.Instance != null)
        {
            return baseHeight * GameDifficultyManager.Instance.GetDetectRangeMultiplier();
        }
        return baseHeight;
    }

    public bool IsTargetInDetectRange(Vector3 targetPosition)
    {
        if (currentConfig.detectRangeType == DetectRangeType.Circle)
        {
            float effectiveRange = GetEffectiveDetectRange();
            return Vector3.Distance(transform.position, targetPosition) <= effectiveRange;
        }
        else
        {
            float halfWidth = GetEffectiveDetectWidth() / 2f;
            float halfHeight = GetEffectiveDetectHeight() / 2f;
            Vector3 offset = targetPosition - transform.position;
            return Mathf.Abs(offset.x) <= halfWidth && Mathf.Abs(offset.y) <= halfHeight;
        }
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

    private void ChangeState(EnemyAIState newState)
    {
        if (currentState == newState) return;

        OnStateExit(currentState);

        EnemyAIState previousState = currentState;
        currentState = newState;

        OnStateEnter(newState);

        LogManager.Log($"[EnemyAIController] 状态切换: {previousState} -> {newState}");
    }

    private void OnStateEnter(EnemyAIState state)
    {
        switch (state)
        {
            case EnemyAIState.Idle:
                currentIdleTime = Random.Range(currentConfig.idleTimeMin, currentConfig.idleTimeMax);
                idleTimer = 0f;
                break;
            case EnemyAIState.Patrol:
                currentPatrolTarget = currentConfig.patrolArea.GetRandomPointInArea(spawnPosition);
                break;
        }
    }

    private void OnStateExit(EnemyAIState state)
    {
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private void OnDrawGizmosSelected()
    {
        if (currentConfig == null) return;

        // 绘制传统检测范围
        Gizmos.color = Color.yellow;
        if (currentConfig.detectRangeType == DetectRangeType.Circle)
        {
            Gizmos.DrawWireSphere(transform.position, GetEffectiveDetectRange());
        }
        else
        {
            Vector3 boxSize = new Vector3(GetEffectiveDetectWidth(), GetEffectiveDetectHeight(), 0f);
            Gizmos.DrawWireCube(transform.position, boxSize);
        }


        // 绘制攻击范围
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, currentConfig.attackRange);

        // 绘制巡逻区域
        Vector3 center = Application.isPlaying ? spawnPosition : transform.position;
        currentConfig.patrolArea.DrawGizmos(center, Color.green);

        // 绘制目标连接线
        //if (currentTarget != null)
        //{
        //    Gizmos.color = Color.green;
        //    Gizmos.DrawLine(transform.position, currentTarget.position);
        //    //在线中间显示由EnemyAIController指向目标的文本
        //    Vector3 midPoint = (transform.position + currentTarget.position) / 2;
        //    Handles.Label(midPoint, "EnemyAIController的目标");
        //}
    }

    public GameObject FindNearestPlayer()
    {
        Collider2D[] colliders;
        if (configData.detectRangeType == DetectRangeType.Circle)
        {
            colliders = Physics2D.OverlapCircleAll(transform.position, configData.detectRange, configData.targetLayers);
        }
        else
        {
            Vector2 boxSize = new Vector2(configData.detectWidth, configData.detectHeight);
            colliders = Physics2D.OverlapBoxAll(transform.position, boxSize, 0f, configData.targetLayers);
        }

        GameObject nearest = null;
        float minDistance = float.MaxValue;

        foreach (var col in colliders)
        {
            float distance = Vector3.Distance(transform.position, col.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = col.gameObject;
            }
        }

        return nearest;
    }

    // 公共方法
    public void SetCurrentTarget(Transform target)
    {
        currentTarget = target;
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
    private void HandleSenseEvent(SenseEvent senseEvent)
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
    private void HandleVisionEvent(SenseEvent senseEvent)
    {
        // 设置当前目标为检测到的对象
        if (senseEvent.detectedObject != null)
        {
            currentTarget = senseEvent.detectedObject.transform;
            LogManager.Log($"[EnemyAIController] 视觉检测到: {senseEvent.detectedObject.name}, 强度: {senseEvent.intensity}");

            // 如果当前状态不是追击或攻击，则切换到追击状态
            if (currentState != EnemyAIState.Chase && currentState != EnemyAIState.Attack && currentState != EnemyAIState.Death)
            {
                ChangeState(EnemyAIState.Chase);
            }
        }
    }

    private void ExecutePatrolState()
    {
    }

    private void ExecuteSpecialAttackState()
    {
        if (currentTarget == null)
        {
            ChangeState(EnemyAIState.Idle);
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
        Vector3 retreatPosition = transform.position + retreatDirection * configData.patrolArea.circleRadius;//todo:这边距离后面调

        MoveToPosition(retreatPosition, configData.patrolSpeed);
    }

    private void MoveToPosition(Vector3 targetPosition, float speed)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        rb.linearVelocity = new Vector2(direction.x * speed, rb.linearVelocity.y);

        // 面向移动方向
        if (direction.x > 0 && !isFacingRight) Flip();
        else if (direction.x < 0 && isFacingRight) Flip();
    }
}
