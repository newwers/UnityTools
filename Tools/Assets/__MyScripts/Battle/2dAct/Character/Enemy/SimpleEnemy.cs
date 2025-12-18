using UnityEngine;

public class SimpleEnemy : CharacterBase
{
    [Header("受击设置")]
    [Tooltip("受击时的闪烁颜色")]
    public Color hitColor = Color.red;
    [Tooltip("受击闪烁持续时间")]
    public float hitFlashDuration = 0.1f;
    [Tooltip("受击特效预制体")]
    public GameObject hitEffectPrefab;

    [Header("动画设置")]
    [Tooltip("受伤动画触发器名称")]
    public string hurtTriggerName = "Hurt";
    [Tooltip("死亡动画触发器名称")]
    public string dieTriggerName = "Death";
    private readonly int stun = Animator.StringToHash("Stun");

    [Header("状态")]
    [SerializeField]
    private bool isDead = false;
    [SerializeField]
    private bool isStunned = false;

    public override bool IsDead => isDead;

    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private Color originalColor;
    private float hitFlashTimer;
    private float stunTimer;

    private const int IDLE_PRIORITY = 0;
    private const int HURT_PRIORITY = 100;
    private const int STUN_PRIORITY = 300;
    private int currentActionPriority = IDLE_PRIORITY;

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    private void Start()
    {
        PlayerAttributes.characterAtttibute.OnDeath += HandleDeath;

        if (BuffSystem != null && PlayerAttributes != null)
        {
            BuffSystem.Init(this, PlayerAttributes.characterAtttibute, rb, this);
        }
    }

    private void Update()
    {
        if (isDead) return;

        if (isStunned)
        {
            stunTimer -= Time.deltaTime;
            if (stunTimer <= 0f)
            {
                RecoverFromStun();
            }
        }

        if (hitFlashTimer > 0)
        {
            hitFlashTimer -= Time.deltaTime;
            if (hitFlashTimer <= 0 && spriteRenderer != null)
            {
                spriteRenderer.color = originalColor;
            }
        }
    }

    public override void TakeDamage(DamageInfo damageInfo, AttackActionData attackActionData, AttackFrameData frameData, CharacterBase attacker)
    {
        if (isDead) return;

        if (BuffSystem != null && PlayerAttributes.characterAtttibute.IsInvincible())
        {
            LogManager.Log($"[SimpleEnemy] 处于无敌状态（计数: {PlayerAttributes.characterAtttibute.isInvincible}），免疫伤害");
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
        var targetAttributes = PlayerAttributes.characterAtttibute;
        var attackerBuffSystem = damageInfo.attacker.BuffSystem;
        var targetBuffSystem = BuffSystem;

        if (attackerAttributes == null)
        {
            return;
        }

        DamageResult result = DamageCalculator.CalculateDamage(damageInfo, damageInfo.attacker, this);

        DamageDisplayHelper.ShowDamageOnCharacter(result, transform);

        if (result.isMiss)
        {
            LogManager.Log("[SimpleEnemy] 攻击未命中（闪避/无敌）");
            return;
        }

        if (!result.isBlocked && result.healthDamage > 0)
        {
            PlayerAttributes.characterAtttibute.ChangeHealth(-result.healthDamage, damageInfo.attacker);
            LogManager.Log($"[SimpleEnemy] 受到伤害: {result.healthDamage}{(result.isCritical ? " (暴击!)" : "")}, 剩余血量: {PlayerAttributes.characterAtttibute.currentHealth}/{PlayerAttributes.characterAtttibute.maxHealth}");
        }
        else if (result.isBlocked)
        {
            LogManager.Log("[SimpleEnemy] 攻击被格挡");
        }
    }

    private bool ShouldPlayHitAnimation(int priority)
    {
        return priority > HURT_PRIORITY && currentActionPriority < priority;
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
    }

    public override void ApplyStun(float duration)
    {
        if (isDead) return;

        if (isStunned)
        {
            stunTimer = duration;
            LogManager.Log($"[SimpleEnemy] 刷新硬直时间: {duration}秒");
            return;
        }

        currentActionPriority = STUN_PRIORITY;
        isStunned = true;
        stunTimer = duration;

        LogManager.Log($"[SimpleEnemy] 被硬直! 持续时间: {duration}秒");

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
        OnStunned();
    }

    private void RecoverFromStun()
    {
        isStunned = false;
        currentActionPriority = IDLE_PRIORITY;

        LogManager.Log($"[SimpleEnemy] 硬直结束");
        OnStunnedEnd();
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

        LogManager.Log($"[SimpleEnemy] 敌人死亡");

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
    }
}
