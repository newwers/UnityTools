/*
 * ════════════════════════════════════════════════════════════════════════════
 * 新敌人实现示例 - 如何创建支持攻击系统的可受击对象
 * ════════════════════════════════════════════════════════════════════════════
 * 
 * 此文件展示了三种不同复杂度的敌人实现方式，供参考。
 * 
 * ════════════════════════════════════════════════════════════════════════════
 */

using UnityEngine;

// ═══════════════════════════════════════════════════════════════════════════
// 示例1: 最简单的敌人（普通小怪）
// ═══════════════════════════════════════════════════════════════════════════
public class SimpleEnemy : MonoBehaviour, IDamageable
{
    [Header("基础属性")]
    public int maxHealth = 100;
    private int currentHealth;
    private bool isDead = false;

    public bool IsDead => isDead;
    public Transform Transform => transform;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(DamageInfo damageInfo, AttackActionData attackActionData, AttackFrameData frameData, GameObject attacker)
    {
        if (isDead) return;

        int damage = Mathf.RoundToInt(damageInfo.baseDamage);
        currentHealth -= damage;

        LogManager.Log($"[SimpleEnemy] 受到 {damage} 点伤害，剩余血量: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        GetComponent<Collider2D>().enabled = false;
        Destroy(gameObject, 1f);
    }
}

// ═══════════════════════════════════════════════════════════════════════════
// 示例2: 带反馈的敌人（精英怪）
// ═══════════════════════════════════════════════════════════════════════════
public class EliteEnemy : MonoBehaviour, IDamageable
{
    [Header("属性")]
    public int maxHealth = 300;
    private int currentHealth;
    private bool isDead = false;
    private bool isStunned = false;

    [Header("组件")]
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    public bool IsDead => isDead;
    public Transform Transform => transform;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(DamageInfo damageInfo, AttackActionData attackActionData, AttackFrameData frameData, GameObject attacker)
    {
        if (isDead) return;

        int damage = Mathf.RoundToInt(damageInfo.baseDamage);
        currentHealth -= damage;

        LogManager.Log($"[EliteEnemy] 受到 {damage} 点伤害，剩余血量: {currentHealth}/{maxHealth}");

        if (frameData != null)
        {
            //if (frameData.causeStun)
            //{
            //    ApplyStun(frameData.stunDuration);
            //}
            //else
            //{
            //    animator?.SetTrigger("Hurt");
            //}

            if (rb != null && attacker != null)
            {
                Vector2 knockbackDir = (transform.position - attacker.transform.position).normalized;
                rb.AddForce(knockbackDir * frameData.knockbackForce, ForceMode2D.Impulse);
            }

            if (frameData.hitEffect != null)
            {
                Instantiate(frameData.hitEffect, transform.position, Quaternion.identity);
            }
        }

        StartCoroutine(FlashWhite());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void ApplyStun(float duration)
    {
        isStunned = true;
        animator?.SetBool("Stunned", true);
        Invoke(nameof(RecoverFromStun), duration);
    }

    private void RecoverFromStun()
    {
        isStunned = false;
        animator?.SetBool("Stunned", false);
    }

    private System.Collections.IEnumerator FlashWhite()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = Color.red;
        }
    }

    private void Die()
    {
        isDead = true;
        animator?.SetTrigger("Die");
        GetComponent<Collider2D>().enabled = false;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
        Destroy(gameObject, 3f);
    }
}

// ═══════════════════════════════════════════════════════════════════════════
// 示例3: 完整属性系统的敌人（Boss）
// ═══════════════════════════════════════════════════════════════════════════
public class BossEnemy : MonoBehaviour, IDamageable
{
    [Header("属性系统")]
    public CharacterAttributes characterAttributes;
    public BuffSystem buffSystem;

    private bool isDead = false;
    private bool isStunned = false;

    [Header("组件")]
    private Rigidbody2D rb;
    private Animator animator;

    public bool IsDead => isDead;
    public Transform Transform => transform;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        if (buffSystem != null && characterAttributes != null)
        {
            buffSystem.Init(characterAttributes, null);
        }
    }

    public void TakeDamage(DamageInfo damageInfo, AttackActionData attackActionData, AttackFrameData frameData, GameObject attacker)
    {
        if (isDead) return;

        if (buffSystem != null && characterAttributes.isInvincible)
        {
            LogManager.Log("[BossEnemy] 无敌状态，免疫伤害");
            return;
        }

        CharacterLogic attackerLogic = attacker.GetComponent<CharacterLogic>();

        if (attackerLogic != null && attackerLogic.PlayerAttributes != null)
        {
            ApplyDamageWithCalculation(attackerLogic, damageInfo, frameData, attacker);
        }
        else
        {
            ApplySimpleDamage(damageInfo, frameData, attacker);
        }
    }

    private void ApplyDamageWithCalculation(CharacterLogic attackerLogic, DamageInfo damageInfo, AttackFrameData frameData, GameObject attacker)
    {
        if (damageInfo.skillData == null)
        {
            ApplySimpleDamage(damageInfo, frameData, attacker);
            return;
        }

        var attackerAttributes = attackerLogic.PlayerAttributes.characterAtttibute;
        var targetAttributes = characterAttributes;
        var attackerBuffSystem = attackerLogic.buffSystem;
        var targetBuffSystem = buffSystem;

        DamageResult result = DamageCalculator.CalculateDamage(
            damageInfo,
            attackerAttributes,
            targetAttributes,
            attackerBuffSystem,
            targetBuffSystem
        );

        if (result.isMiss)
        {
            LogManager.Log("[BossEnemy] 闪避成功");
            return;
        }

        if (result.isBlocked)
        {
            LogManager.Log("[BossEnemy] 格挡成功");
            PlayBlockEffect();
            return;
        }

        if (result.healthDamage > 0)
        {
            targetAttributes.ChangeHealth(-result.healthDamage);

            string damageText = $"[BossEnemy] 受到 {result.healthDamage} 点伤害";
            if (result.isCritical)
                damageText += " (暴击!)";
            LogManager.Log(damageText);

            if (targetAttributes.currentHealth <= 0)
            {
                Die();
                return;
            }
        }

        ApplyHitReaction(frameData, attacker);
    }

    private void ApplySimpleDamage(DamageInfo damageInfo, AttackFrameData frameData, GameObject attacker)
    {
        int damage = Mathf.RoundToInt(damageInfo.baseDamage);
        characterAttributes.currentHealth -= damage;

        LogManager.Log($"[BossEnemy] 受到 {damage} 点伤害");

        if (characterAttributes.currentHealth <= 0)
        {
            Die();
            return;
        }

        ApplyHitReaction(frameData, attacker);
    }

    private void ApplyHitReaction(AttackFrameData frameData, GameObject attacker)
    {
        if (frameData == null) return;

        //if (frameData.causeStun)
        //{
        //    ApplyStun(frameData.stunDuration);
        //}
        //else
        //{
        //    animator?.SetTrigger("Hurt");
        //}

        if (rb != null && attacker != null)
        {
            Vector2 knockbackDir = (transform.position - attacker.transform.position).normalized;
            rb.AddForce(knockbackDir * frameData.knockbackForce, ForceMode2D.Impulse);
        }

        if (frameData.hitEffect != null)
        {
            Instantiate(frameData.hitEffect, transform.position, Quaternion.identity);
        }
    }

    private void ApplyStun(float duration)
    {
        isStunned = true;
        animator?.SetBool("Stunned", true);
        Invoke(nameof(RecoverFromStun), duration);
    }

    private void RecoverFromStun()
    {
        isStunned = false;
        animator?.SetBool("Stunned", false);
    }

    private void PlayBlockEffect()
    {
        // 播放格挡特效
    }

    private void Die()
    {
        isDead = true;
        animator?.SetTrigger("Die");
        GetComponent<Collider2D>().enabled = false;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;

        // Boss死亡特效和奖励
        Destroy(gameObject, 5f);
    }
}

/*
 * ════════════════════════════════════════════════════════════════════════════
 * 使用建议
 * ════════════════════════════════════════════════════════════════════════════
 * 
 * 1. 普通小怪 → 使用 SimpleEnemy
 *    - 只需基础的生命值和死亡逻辑
 *    - 不需要复杂的受击反馈
 * 
 * 2. 精英怪物 → 使用 EliteEnemy
 *    - 需要击退、硬直等受击反馈
 *    - 需要动画和特效支持
 *    - 不需要完整的属性系统
 * 
 * 3. Boss敌人 → 使用 BossEnemy
 *    - 需要完整的属性系统（力量、敏捷等）
 *    - 需要支持Buff系统
 *    - 需要复杂的伤害计算（暴击、格挡、闪避等）
 * 
 * ════════════════════════════════════════════════════════════════════════════
 */
