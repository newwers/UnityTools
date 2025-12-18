using System.Collections;
using UnityEngine;

/// <summary>
/// 训练假人组件
/// 用于测试攻击系统、Buff系统和伤害计算
/// 提供了完整的受击反馈、状态管理和自动攻击功能
/// </summary>
public class TestDummy : CharacterBase
{

    /// <summary>
    /// 硬直开始事件
    /// </summary>
    public System.Action OnStunStart;

    /// <summary>
    /// 硬直结束事件
    /// </summary>
    public System.Action OnStunEnd;

    /// <summary>
    /// 生命值改变事件 (当前血量, 最大血量)
    /// </summary>
    public System.Action<float, float> OnHealthChanged;

    /// <summary>
    /// 死亡事件
    /// </summary>
    public System.Action OnDeath;

    /// <summary>
    /// 受击事件 (击退方向, 击退力)
    /// </summary>
    public System.Action<Vector2, Vector2> OnHit;



    [Tooltip("无敌模式，启用后不会死亡（用于测试）")]
    public bool isInvincible = false;

    [Tooltip("是否在屏幕上显示调试信息")]
    public bool showDebugInfo = true;

    [Header("自动攻击设置")]
    [Tooltip("是否启用自动攻击功能")]
    public bool enableAutoAttack = false;

    [Tooltip("自动攻击的间隔时间（秒）")]
    public float attackInterval = 2f;

    [Tooltip("自动攻击使用的攻击数据")]
    public AttackActionData attackData;

    [Header("被击反馈")]
    [Tooltip("受击时的闪烁颜色")]
    public Color hitColor = Color.red;

    [Tooltip("受击闪烁持续时间（秒）")]
    public float hitFlashDuration = 0.1f;

    [Tooltip("受击特效预制体")]
    public GameObject hitEffectPrefab;

    [Tooltip("受击音效")]
    public AudioClip hitSound;

    [Header("硬直设置")]
    [Tooltip("当前是否处于硬直状态")]
    public bool isStunned = false;

    /// <summary>
    /// 硬直剩余时间
    /// </summary>
    private float stunTimer = 0f;


    [Header("动画设置")]
    [Tooltip("受伤动画触发器名称")]
    public string hurtTriggerName = "Hurt";

    [Tooltip("死亡动画触发器名称")]
    public string dieTriggerName = "Die";

    [Tooltip("重置动画触发器名称")]
    public string resetTriggerName = "Reset";

    [Tooltip("硬直动画布尔参数名称")]
    public string stunBoolName = "Stun";

    [Header("状态显示")]
    [SerializeField]
    [Tooltip("是否已死亡")]
    private bool isDead = false;

    public override bool IsDead => isDead;

    // 组件引用
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private Color originalColor;

    /// <summary>
    /// 受击闪烁计时器
    /// </summary>
    private float hitFlashTimer = 0f;

    // 自动攻击相关变量
    private float attackTimer = 0f;



    /// <summary>
    /// 初始化组件引用和默认状态
    /// </summary>
    protected override void Awake()
    {
        base.Awake();

        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        isDead = false;

        // 初始化自动攻击计时器
        attackTimer = attackInterval;

        PlayerAttributes.characterAtttibute.OnDeath += CharacterAtttibute_OnDeath;
    }

    private void CharacterAtttibute_OnDeath(CharacterBase obj)
    {
        Die();
    }

    /// <summary>
    /// 初始化BuffSystem
    /// </summary>
    private void Start()
    {
        // 初始化BuffSystem，假人没有CharacterLogic，传入null
        if (BuffSystem != null && PlayerAttributes != null)
        {
            BuffSystem.Init(this, PlayerAttributes.characterAtttibute, rb, this);
            LogManager.Log($"[TestDummy] BuffSystem初始化完成");
        }

        LogManager.Log($"[TestDummy] 假人初始化完成，血量: {PlayerAttributes.characterAtttibute.maxHealth}");
    }

    /// <summary>
    /// 每帧更新
    /// 处理硬直计时、受击闪烁效果和自动攻击
    /// </summary>
    private void Update()
    {
        // 更新硬直计时器
        if (isStunned)
        {
            stunTimer -= Time.deltaTime;
            if (stunTimer <= 0f)
            {
                RecoverFromStun();
            }
        }

        // 处理被击闪白效果
        if (hitFlashTimer > 0)
        {
            hitFlashTimer -= Time.deltaTime;
            if (hitFlashTimer <= 0 && spriteRenderer != null)
            {
                spriteRenderer.color = originalColor;
            }
        }

        // 自动攻击逻辑
        if (enableAutoAttack && !isDead && attackData != null && !isStunned)
        {
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0f)
            {
                PerformAutoAttack();
                attackTimer = attackInterval;
            }
        }
    }

    #region Attack

    /// <summary>
    /// 执行自动攻击
    /// 启动攻击检测流程
    /// </summary>
    private void PerformAutoAttack()
    {
        if (attackData == null) return;

        //LogManager.Log($"[TestDummy] 执行自动攻击: {attackData.attackName}");

        // 使用统一的攻击流程
        StartCoroutine(ExecuteAttackWithHitDetector());
    }


    /// <summary>
    /// 使用AttackHitDetector执行攻击序列
    /// 协调攻击的前摇、攻击中、后摇各个阶段
    /// 使用统一的攻击流程，与CharacterAttackController保持一致
    /// </summary>
    private IEnumerator ExecuteAttackWithHitDetector()
    {
        if (attackData == null) yield break;

        // 技能释放前效果
        CharacterAttackController.ApplySkillEffectsOnCast(attackData, this);

        // 开始攻击检测，获取attackId
        string attackId = AttackHitDetector.Instance.StartAttackDetection(
            attackData,
            transform.position,
            IsFacingRight(),
            this
        );

        //LogManager.Log($"[TestDummy] 开始攻击检测，攻击ID: {attackId}");

        // 播放攻击动画
        if (animator != null && attackData.animationParameters != null)
        {
            SetActionAnimationParameter(attackData);
        }

        // 前摇阶段
        float attackTimer = 0f;
        while (attackTimer < attackData.windUpTime)
        {
            attackTimer += Time.deltaTime;

            // 在前摇阶段也可以绘制攻击框预览
#if UNITY_EDITOR
            DrawAttackPreview(attackTimer);
#endif

            yield return null;
        }

        // 攻击中阶段
        float activeTimer = 0f;
        while (activeTimer < attackData.activeTime)
        {
            activeTimer += Time.deltaTime;
            attackTimer += Time.deltaTime;

            // 使用AttackHitDetector检测攻击命中
            AttackHitDetector.Instance.CheckHitForFrame(
                attackId,
                attackData,
                transform.position,
                IsFacingRight(),
                attackTimer,
                this
            );

#if UNITY_EDITOR
            DrawAttackPreview(attackTimer);
#endif

            yield return null;
        }

        // 后摇阶段
        float recoveryTimer = 0f;
        while (recoveryTimer < attackData.recoveryTime)
        {
            recoveryTimer += Time.deltaTime;
            attackTimer += Time.deltaTime;
            yield return null;
        }

        // 技能释放后的效果，传递attackId以获取受击方信息
        CharacterAttackController.ApplySkillEffectsOnComplete(attackData, this, attackId);

        // 结束攻击检测
        AttackHitDetector.Instance.EndAttackDetection(attackId);

#if UNITY_EDITOR
        ClearAttackPreview();
#endif


        //LogManager.Log($"[TestDummy] 攻击完成");
    }

    /// <summary>
    /// 判断假人是否面向右侧
    /// </summary>
    private bool IsFacingRight()
    {
        // 简单的判断方法，可以根据实际需要调整
        return transform.localScale.x >= 0;
    }

#if UNITY_EDITOR
    /// <summary>
    /// 绘制攻击预览（编辑器模式下）
    /// </summary>
    private void DrawAttackPreview(float currentAttackTimer)
    {
        var visualizer = GetComponent<AttackHitVisualizer>();
        if (visualizer == null) return;

        int currentFrameIndex = CalculateCurrentFrameIndex(attackData, currentAttackTimer);
        var frameData = attackData?.GetFrameData(currentFrameIndex);

        if (frameData != null && frameData.isAttackFrame)
        {
            Vector2 detectionPosition = CalculateHitboxPosition(
                frameData,
                transform.position,
                IsFacingRight()
            );

            visualizer.SetCurrentFrameData(
                null, // TestDummy没有CharacterLogic组件
                frameData,
                detectionPosition,
                IsFacingRight()
            );
        }
    }

    /// <summary>
    /// 清除攻击预览
    /// </summary>
    private void ClearAttackPreview()
    {
        var visualizer = GetComponent<AttackHitVisualizer>();
        if (visualizer != null)
        {
            visualizer.ClearFrameData();
        }
    }
#endif

    /// <summary>
    /// 计算当前帧索引
    /// </summary>
    private int CalculateCurrentFrameIndex(AttackActionData attackData, float currentAttackTimer)
    {
        if (attackData == null) return 0;

        float activeElapsed = currentAttackTimer - attackData.windUpTime;
        activeElapsed = Mathf.Clamp(activeElapsed, 0f, attackData.activeTime);

        int totalActiveFrames = attackData.ActualActiveFrames;
        if (totalActiveFrames <= 0) return 0;

        float phaseProgress = activeElapsed / attackData.activeTime;
        int frameIndex = Mathf.FloorToInt(phaseProgress * totalActiveFrames);
        frameIndex = Mathf.Clamp(frameIndex, 0, totalActiveFrames - 1);

        int actualFrameIndex = attackData.ActualWindUpFrames + frameIndex;
        return actualFrameIndex;
    }

    /// <summary>
    /// 计算攻击框位置
    /// </summary>
    private Vector2 CalculateHitboxPosition(AttackFrameData frameData, Vector2 characterPosition, bool facingRight)
    {
        return characterPosition +
            new Vector2(frameData.hitboxOffset.x * (facingRight ? 1 : -1),
                       frameData.hitboxOffset.y);
    }

    /// <summary>
    /// 设置攻击动画参数
    /// </summary>
    public void SetActionAnimationParameter(ActionData actionData)
    {
        CharacterAnimation.SetActionAnimationParameter(animator, actionData);
    }


    /// <summary>
    /// 设置自动攻击配置
    /// </summary>
    public void SetAutoAttackConfig(AttackActionData newAttackData, float interval = 2f)
    {
        attackData = newAttackData;
        attackInterval = interval;
        attackTimer = attackInterval; // 重置计时器

        LogManager.Log($"[TestDummy] 设置自动攻击配置: {attackData.acitonName}, 间隔: {attackInterval}秒");
    }

    /// <summary>
    /// 启用/禁用自动攻击
    /// </summary>
    public void SetAutoAttackEnabled(bool enabled)
    {
        enableAutoAttack = enabled;
        attackTimer = attackInterval; // 重置计时器

        LogManager.Log($"[TestDummy] 自动攻击: {(enabled ? "启用" : "禁用")}");
    }

    #endregion

    /// <summary>
    /// 施加硬直效果
    /// 由攻击系统调用，使假人进入硬直状态，无法行动
    /// 支持刷新眩晕时间：如果假人已经处于眩晕状态，会更新眩晕时间为新的持续时间
    /// </summary>
    /// <param name="duration">硬直持续时间（秒）</param>
    public override void ApplyStun(float duration)
    {
        if (isDead) return;

        if (isStunned)
        {
            stunTimer = duration;
            LogManager.Log($"[TestDummy] 刷新硬直时间: {duration}秒");
            return;
        }

        isStunned = true;
        stunTimer = duration;

        LogManager.Log($"[TestDummy] 被硬直! 持续时间: {duration}秒");

        if (animator != null && !string.IsNullOrEmpty(stunBoolName))
        {
            animator.SetBool(stunBoolName, true);
        }

        OnStunStart?.Invoke();

        if (enableAutoAttack)
        {
            StopAllCoroutines();
        }
    }

    /// <summary>
    /// 从硬直状态恢复
    /// </summary>
    private void RecoverFromStun()
    {
        isStunned = false;

        LogManager.Log($"[TestDummy] 硬直结束");

        if (animator != null && !string.IsNullOrEmpty(stunBoolName))
        {
            animator.SetBool(stunBoolName, false);
        }

        OnStunEnd?.Invoke();

        // 恢复自动攻击
        if (enableAutoAttack)
        {
            attackTimer = attackInterval;
        }
    }

    /// <summary>
    /// 实现IDamageable接口的TakeDamage方法
    /// 统一处理来自攻击系统的伤害
    /// </summary>
    /// <param name="damageInfo">伤害信息</param>
    /// <param name="frameData">攻击帧数据</param>
    /// <param name="attacker">攻击者</param>
    /// <summary>
    /// 实现IDamageable接口的TakeDamage方法
    /// 用于接收来自新攻击系统的伤害
    /// </summary>
    public override void TakeDamage(DamageInfo damageInfo, AttackActionData attackActionData, AttackFrameData frameData, CharacterBase attacker)
    {
        base.TakeDamage(damageInfo, attackActionData, frameData, attacker);
        if (isDead || isInvincible) return;

        if (BuffSystem != null && PlayerAttributes.characterAtttibute.IsInvincible())
        {
            LogManager.Log($"[TestDummy] 处于无敌状态（Buff效果，计数: {PlayerAttributes.characterAtttibute.isInvincible}），免疫伤害");
            return;
        }
        Vector2 knockbackDirection = (transform.position - attacker.transform.position).normalized;

        // 计算最终击退力：技能基础击退力 + 攻击帧附加击退力
        Vector2 finalKnockbackForce = frameData.knockbackForce;
        if (damageInfo.skillData != null)
        {
            finalKnockbackForce += damageInfo.skillData.knockbackForce;
        }

        TakeDamage(damageInfo, attackActionData, frameData, knockbackDirection, finalKnockbackForce, transform.position);
    }

    /// <summary>
    /// 受到攻击伤害
    /// 由AttackHitDetector在检测到命中时调用
    /// 处理伤害计算、受击反馈、硬直判定、击退效果等
    /// 注意：此方法不处理Buff应用，Buff由AttackHitDetector通过BuffSystem直接应用
    /// </summary>
    /// <param name="attackData">攻击动作数据</param>
    /// <param name="frameData">攻击帧数据，包含伤害、硬直等信息</param>
    /// <param name="knockbackDirection">击退方向</param>
    /// <param name="knockbackForce">击退力</param>
    /// <param name="hitPosition">命中位置，用于播放特效</param>
    public void TakeDamage(DamageInfo damageInfo, AttackActionData attackActionData, AttackFrameData frameData, Vector2 knockbackDirection, Vector2 knockbackForce, Vector2 hitPosition)
    {
        var attacker = damageInfo.attacker;
        // 检查无敌和死亡状态
        if (isDead || isInvincible) return;

        // 检查BuffSystem的无敌状态（由Buff效果产生的无敌）
        if (BuffSystem != null && PlayerAttributes.characterAtttibute.IsInvincible())
        {
            LogManager.Log($"[TestDummy] 处于无敌状态（Buff效果，计数: {PlayerAttributes.characterAtttibute.isInvincible}），免疫伤害");
            return;
        }

        // 应用伤害到生命值
        ApplyDamageWithCalculation(attacker, damageInfo, attackActionData, frameData);


        // 检查是否造成眩晕
        if (isStunned)
        {
            LogManager.Log($"[TestDummy] 已处于硬直状态，忽略新的受击硬直");
        }
        else
        {
            // 根据攻击优先级决定受击表现
            bool shouldPlayHitAnimation = ShouldPlayHitAnimation(attackData.priority);

            // 触发Hurt动画
            if (shouldPlayHitAnimation)
            {
                // 播放受击动画，打断当前动作
                if (animator != null && !string.IsNullOrEmpty(hurtTriggerName))
                {
                    animator.SetTrigger(hurtTriggerName);
                }
                LogManager.Log($"[TestDummy] 播放受击动画");
            }
            else
            {
                // 只播放闪白效果，不打断当前动作
                LogManager.Log($"[TestDummy] 攻击优先级不足，只播放闪白效果，不打断当前动作");
            }
        }

        // 触发事件
        OnHealthChanged?.Invoke(PlayerAttributes.characterAtttibute.currentHealth, PlayerAttributes.characterAtttibute.maxHealth);
        OnHit?.Invoke(knockbackDirection, knockbackForce);

        // 视觉反馈（闪白效果总是播放）
        PlayHitFeedback(hitPosition);

        // 击退效果
        if (rb != null && knockbackForce.magnitude > 0)
        {
            rb.AddForce(knockbackDirection.normalized * knockbackForce, ForceMode2D.Impulse);
        }

        // 检查死亡
        if (PlayerAttributes.characterAtttibute.currentHealth <= 0)
        {
            Die();
        }
    }


    /// <summary>
    /// 应用带属性计算的伤害（攻击者是CharacterLogic时使用）
    /// </summary>
    private void ApplyDamageWithCalculation(CharacterBase attacker, DamageInfo damageInfo, AttackActionData attackActionData, AttackFrameData frameData)
    {
        var attackerAttributes = attacker.PlayerAttributes.characterAtttibute;
        var targetAttributes = PlayerAttributes.characterAtttibute;
        var attackerBuffSystem = attacker.BuffSystem;
        var targetBuffSystem = BuffSystem;

        if (attackerAttributes == null)
        {
            return;
        }

        DamageResult result = DamageCalculator.CalculateDamage(damageInfo, attacker, this);
        DamageDisplayHelper.ShowDamageOnCharacter(result, transform);

        if (result.isMiss)
        {
            LogManager.Log("[TestDummy] 攻击未命中（闪避/无敌）");
            return;
        }

        if (!result.isBlocked && result.healthDamage > 0)
        {
            PlayerAttributes.characterAtttibute.ChangeHealth(-Mathf.RoundToInt(result.healthDamage), damageInfo.attacker);
            LogManager.Log($"[TestDummy] 造成伤害: {result.healthDamage}{(result.isCritical ? " (暴击" : "")},剩余血量: {PlayerAttributes.characterAtttibute.currentHealth}/{PlayerAttributes.characterAtttibute.maxHealth}");

            bool died = PlayerAttributes.characterAtttibute.currentHealth <= 0;
            if (died)
            {
                Die();
                return;
            }
        }
        else if (result.isBlocked)
        {
            LogManager.Log("[TestDummy] 攻击被格挡");
            // 可以在这里添加格挡特效或音效
            return;
        }
    }

    /// <summary>
    /// 判断是否应该播放受击动画
    /// </summary>
    /// <param name="priority">攻击的优先级</param>
    /// <returns>如果应该播放受击动画返回true</returns>
    private bool ShouldPlayHitAnimation(int priority)
    {
        return DamageCalculator.ShouldPlayHitAnimation(priority, PlayerAttributes.characterAtttibute);
    }



    /// <summary>
    /// 播放受击视觉反馈效果
    /// 包括颜色闪烁、命中特效和音效
    /// </summary>
    /// <param name="hitPosition">命中位置</param>
    private void PlayHitFeedback(Vector2 hitPosition)
    {
        // 颜色闪白
        if (spriteRenderer != null)
        {
            spriteRenderer.color = hitColor;
            hitFlashTimer = hitFlashDuration;
        }

        // 播放命中特效
        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, hitPosition, Quaternion.identity);
        }

        // 播放音效
        if (hitSound != null)
        {
            // AudioSource.PlayClipAtPoint(hitSound, hitPosition);
        }
    }

    /// <summary>
    /// 假人死亡处理
    /// 触发死亡动画、事件、禁用碰撞器和物理
    /// </summary>
    private void Die()
    {
        if (isDead) return;
        isDead = true;

        LogManager.Log($"[TestDummy] 假人死亡");

        // 触发死亡动画
        if (animator != null && !string.IsNullOrEmpty(dieTriggerName))
        {
            animator.SetTrigger(dieTriggerName);
        }

        // 触发死亡事件
        OnDeath?.Invoke();

        // 死亡视觉效果
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.gray;
        }

        // 禁用碰撞器
        if (boxCollider2D != null)
        {
            boxCollider2D.enabled = false;
        }

        // 禁用物理
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        //清除buff
        if (BuffSystem != null)
        {
            BuffSystem.ClearAllBuffs();
        }
    }

    /// <summary>
    /// 重置假人状态
    /// 将假人恢复到初始状态，用于重复测试
    /// </summary>
    public void ResetDummy()
    {
        PlayerAttributes.characterAtttibute.currentHealth = PlayerAttributes.characterAtttibute.maxHealth;
        isDead = false;

        // 清除硬直状态
        isStunned = false;
        stunTimer = 0f;

        // 重置动画状态
        if (animator != null)
        {
            if (!string.IsNullOrEmpty(resetTriggerName))
            {
                animator.SetTrigger(resetTriggerName);
            }
            else
            {
                // 如果没有重置触发器，则重置所有参数
                animator.Rebind();
                animator.Update(0f);
            }
        }

        // 恢复碰撞器
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = true;
        }

        // 恢复物理
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
        }

        // 恢复颜色
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }

        LogManager.Log($"[TestDummy] 假人已重置，血量: {PlayerAttributes.characterAtttibute.currentHealth}/{PlayerAttributes.characterAtttibute.maxHealth}");
    }

    /// <summary>
    /// 设置无敌模式
    /// 用于测试，开启后假人不会受到伤害
    /// </summary>
    /// <param name="invincible">是否无敌</param>
    public void SetInvincible(bool invincible)
    {
        isInvincible = invincible;
        LogManager.Log($"[TestDummy] 无敌模式: {invincible}");
    }

    /// <summary>
    /// 直接设置血量（用于测试）
    /// </summary>
    /// <param name="health">目标血量值</param>
    public void SetHealth(int health)
    {
        PlayerAttributes.characterAtttibute.currentHealth = Mathf.Clamp(health, 0, PlayerAttributes.characterAtttibute.maxHealth);
        OnHealthChanged?.Invoke(PlayerAttributes.characterAtttibute.currentHealth, PlayerAttributes.characterAtttibute.maxHealth);

        if (PlayerAttributes.characterAtttibute.currentHealth <= 0 && !isDead)
        {
            Die();
        }
    }
}