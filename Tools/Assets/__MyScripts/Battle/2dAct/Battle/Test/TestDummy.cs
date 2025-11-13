using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestDummy : MonoBehaviour
{
    [Header("假人设置")]
    public int maxHealth = 100;
    public bool isInvincible = false; // 无敌模式，用于测试不会死亡
    public bool showDebugInfo = true;

    [Header("自动攻击设置")]
    public bool enableAutoAttack = false; // 是否启用自动攻击
    public float attackInterval = 2f; // 攻击间隔时间
    public ActionData attackData; // 攻击配置数据

    [Header("被击反馈")]
    public Color hitColor = Color.red;
    public float hitFlashDuration = 0.1f;
    public GameObject hitEffectPrefab;
    public AudioClip hitSound;

    [Header("硬直设置")]
    public bool isStunned = false;
    private float stunTimer = 0f;

    // 添加硬直状态事件
    public System.Action OnStunStart;
    public System.Action OnStunEnd;

    [Header("动画设置")]
    public string hurtTriggerName = "Hurt";
    public string dieTriggerName = "Die";
    public string resetTriggerName = "Reset";

    [Header("状态显示")]
    [SerializeField] private int currentHealth;
    [SerializeField] private bool isDead = false;

    // 组件引用
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private Color originalColor;

    // 被击计时器
    private float hitFlashTimer = 0f;

    // 自动攻击相关变量
    private float attackTimer = 0f;
    private List<GameObject> potentialTargets = new List<GameObject>(); // 潜在攻击目标

    // 事件
    public System.Action<int, int> OnHealthChanged; // 当前血量, 最大血量
    public System.Action OnDeath;
    public System.Action<Vector2, float> OnHit; // 击退方向, 击退力

    [Header("动作优先级")]
    [SerializeField] private int currentActionPriority = 0; // 当前执行动作的优先级

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        currentHealth = maxHealth;
        isDead = false;

        // 初始化自动攻击计时器
        attackTimer = attackInterval;
    }

    private void Start()
    {
        LogManager.Log($"[TestDummy] 假人初始化完成，血量: {currentHealth}/{maxHealth}");
    }

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



    private void OnGUI()
    {
        if (showDebugInfo)
        {
            // 在假人上方显示血量和状态
            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 2f);

            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.white;
            style.alignment = TextAnchor.MiddleCenter;
            style.fontSize = 12;

            string statusText = $"HP: {currentHealth}/{maxHealth}\n";
            statusText += isDead ? "状态: 死亡" : "状态: 存活";

            GUI.Label(new Rect(screenPos.x - 50, Screen.height - screenPos.y, 100, 50), statusText, style);
        }
    }

    #region Attack

    /// <summary>
    /// 执行自动攻击
    /// </summary>
    private void PerformAutoAttack()
    {
        if (attackData == null) return;
        currentActionPriority = 150;//攻击优先级设为150
        //LogManager.Log($"[TestDummy] 执行自动攻击: {attackData.attackName}");

        // 使用AttackHitDetector进行攻击检测
        StartCoroutine(ExecuteAttackWithHitDetector());
    }


    /// <summary>
    /// 使用AttackHitDetector执行攻击序列
    /// </summary>
    private IEnumerator ExecuteAttackWithHitDetector()
    {
        if (attackData == null) yield break;

        string attackId = AttackHitDetector.Instance.StartAttackDetection(
            attackData,
            transform.position,
            IsFacingRight(),
            gameObject
        );

        //LogManager.Log($"[TestDummy] 开始攻击检测，攻击ID: {attackId}");

        // 播放攻击动画
        if (animator != null && !string.IsNullOrEmpty(attackData.animationParameterName))
        {
            SetAttackAnimationParameter(attackData);
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
                gameObject
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

        // 结束攻击检测
        AttackHitDetector.Instance.EndAttackDetection(attackId);

#if UNITY_EDITOR
        ClearAttackPreview();
#endif

        // 清理动画参数
        if (animator != null && !string.IsNullOrEmpty(attackData.animationParameterName))
        {
            ClearAttackAnimationParameter(attackData);
        }

        currentActionPriority = 0; // 恢复默认优先级

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
    private int CalculateCurrentFrameIndex(ActionData attackData, float currentAttackTimer)
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
    private void SetAttackAnimationParameter(ActionData attackData)
    {
        if (animator == null || string.IsNullOrEmpty(attackData.animationParameterName)) return;

        switch (attackData.animationParameterType)
        {
            case ActionData.AnimationParameterType.Trigger:
                animator.SetTrigger(attackData.animationParameterName);
                break;
            case ActionData.AnimationParameterType.Bool:
                animator.SetBool(attackData.animationParameterName, attackData.animationBoolValue);
                break;
            case ActionData.AnimationParameterType.Int:
                animator.SetInteger(attackData.animationParameterName, attackData.animationIntValue);
                break;
            case ActionData.AnimationParameterType.Float:
                animator.SetFloat(attackData.animationParameterName, attackData.animationFloatValue);
                break;
        }
    }

    /// <summary>
    /// 清理攻击动画参数
    /// </summary>
    private void ClearAttackAnimationParameter(ActionData attackData)
    {
        if (animator == null || string.IsNullOrEmpty(attackData.animationParameterName)) return;

        switch (attackData.animationParameterType)
        {
            case ActionData.AnimationParameterType.Bool:
                animator.SetBool(attackData.animationParameterName, false);
                break;
            case ActionData.AnimationParameterType.Trigger:
                // Trigger不需要清理，它会自动重置
                break;
        }
    }

    /// <summary>
    /// 设置自动攻击配置
    /// </summary>
    public void SetAutoAttackConfig(ActionData newAttackData, float interval = 2f)
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
    /// 弹反后,由攻击施加硬直
    /// </summary>
    public void ApplyStun(float duration)
    {
        if (isStunned || isDead) return;

        currentActionPriority = 300; // 硬直优先级设为300
        isStunned = true;
        stunTimer = duration;

        LogManager.Log($"[TestDummy] 被硬直! 持续时间: {duration}秒");

        // 触发硬直动画
        if (animator != null)
        {
            animator.SetBool("Stun", true);
        }

        OnStunStart?.Invoke();

        // 停止当前动作
        if (enableAutoAttack)
        {
            StopAllCoroutines();
        }
    }

    private void RecoverFromStun()
    {
        isStunned = false;
        currentActionPriority = 0; // 恢复默认优先级

        LogManager.Log($"[TestDummy] 硬直结束");

        if (animator != null)
        {
            animator.SetBool("Stun", false);
        }

        OnStunEnd?.Invoke();

        // 恢复自动攻击
        if (enableAutoAttack)
        {
            attackTimer = attackInterval;
        }
    }

    /// <summary>
    /// 受到攻击
    /// </summary>
    /// <param name="damage">伤害值</param>
    /// <param name="knockbackDirection">击退方向</param>
    /// <param name="knockbackForce">击退力</param>
    /// <param name="hitPosition">命中位置</param>
    public void TakeDamage(ActionData attackData, AttackFrameData frameData, Vector2 knockbackDirection, float knockbackForce, Vector2 hitPosition)
    {
        if (isDead || isInvincible) return;

        // 计算伤害
        currentHealth = Mathf.Max(0, currentHealth - frameData.damage);

        LogManager.Log($"[TestDummy] 受到伤害: {frameData.damage}, 剩余血量: {currentHealth}/{maxHealth}");

        // 根据攻击优先级决定受击表现
        bool shouldPlayHitAnimation = ShouldPlayHitAnimation(attackData.priority);

        // 触发Hurt动画
        if (shouldPlayHitAnimation)
        {
            // 播放受击动画
            if (animator != null && !string.IsNullOrEmpty(hurtTriggerName))
            {
                animator.SetTrigger(hurtTriggerName);
            }
            LogManager.Log($"[TestDummy] 播放受击动画，当前动作优先级: {currentActionPriority}");
        }
        else
        {
            // 只播放闪白效果，不播放受击动画
            LogManager.Log($"[TestDummy] 只播放闪白效果，不打断当前动作");
        }

        // 触发事件
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnHit?.Invoke(knockbackDirection, knockbackForce);


        // 视觉反馈
        PlayHitFeedback(hitPosition);//闪白效果总是播放

        // 击退效果
        if (rb != null && knockbackForce > 0)
        {
            rb.AddForce(knockbackDirection.normalized * knockbackForce, ForceMode2D.Impulse);
        }

        // 检查死亡
        if (currentHealth <= 0)
        {
            Die();
        }


        // 检查是否造成眩晕
        if (frameData.causeStun)
        {
            ApplyStun(frameData.stunDuration);
        }
    }

    /// <summary>
    /// 判断是否应该播放受击动画
    /// </summary>
    private bool ShouldPlayHitAnimation(int priority)
    {
        // 这里需要从攻击数据获取优先级，暂时使用默认逻辑
        // 在实际攻击检测中，这个判断会在 ProcessHit 方法中完成
        return currentActionPriority < priority;
    }

    /// <summary>
    /// 受到攻击（简化版）
    /// </summary>
    public void TakeDamage(ActionData attackData, AttackFrameData frameData, Vector2 knockbackDirection, float knockbackForce)
    {
        TakeDamage(attackData, frameData, knockbackDirection, knockbackForce, transform.position);
    }

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

    private void Die()
    {
        isDead = true;

        LogManager.Log($"[TestDummy] 假人死亡");

        // 触发死亡动画
        if (animator != null && !string.IsNullOrEmpty(dieTriggerName))
        {
            animator.SetTrigger(dieTriggerName);
        }

        // 触发死亡事件
        OnDeath?.Invoke();

        // 死亡效果
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.gray;
        }

        // 禁用碰撞器
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
        }

        // 禁用物理
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
    }

    /// <summary>
    /// 重置假人状态
    /// </summary>
    public void ResetDummy()
    {
        currentHealth = maxHealth;
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

        LogManager.Log($"[TestDummy] 假人已重置，血量: {currentHealth}/{maxHealth}");
    }

    /// <summary>
    /// 设置无敌模式
    /// </summary>
    public void SetInvincible(bool invincible)
    {
        isInvincible = invincible;
        LogManager.Log($"[TestDummy] 无敌模式: {invincible}");
    }

    /// <summary>
    /// 直接设置血量（用于测试）
    /// </summary>
    public void SetHealth(int health)
    {
        currentHealth = Mathf.Clamp(health, 0, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0 && !isDead)
        {
            Die();
        }
    }

    // 在编辑器中可视化击退方向
    private void OnDrawGizmosSelected()
    {
        // 绘制生命值条
        Vector3 barPos = transform.position + Vector3.up * 1.5f;
        float healthPercent = (float)currentHealth / maxHealth;

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(barPos, new Vector3(1f, 0.2f, 0f));

        Gizmos.color = Color.green;
        Gizmos.DrawCube(barPos - new Vector3(0.5f * (1 - healthPercent), 0, 0),
                        new Vector3(healthPercent, 0.15f, 0f));

        // 绘制自动攻击范围
        //if (enableAutoAttack)
        //{
        //    Gizmos.color = Color.yellow;
        //    Gizmos.DrawWireSphere(transform.position, 5f); // 检测范围
        //}
    }
}