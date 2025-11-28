using UnityEngine;

public class Example_Enemy : MonoBehaviour, IDamageable
{
    [Header("Enemy Stats")]
    public CharacterAttributes characterAttributes;
    public BuffSystem buffSystem;

    private bool isDead = false;
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

    /// <summary>
    /// 实现IDamageable接口的TakeDamage方法
    /// </summary>
    public void TakeDamage(DamageInfo damageInfo, AttackActionData attackActionData, AttackFrameData frameData, GameObject attacker)
    {
        if (isDead) return;

        if (buffSystem != null && characterAttributes.isInvincible)
        {
            LogManager.Log($"[Example_Enemy] 处于无敌状态，免疫伤害");
            return;
        }

        int damage = Mathf.RoundToInt(damageInfo.baseDamage);
        characterAttributes.currentHealth = Mathf.Max(0, characterAttributes.currentHealth - damage);

        LogManager.Log($"[Example_Enemy] 受到伤害: {damage}, 剩余血量: {characterAttributes.currentHealth}/{characterAttributes.maxHealth}");

        if (frameData != null)
        {
            // 不再使用frameData的眩晕设置，眩晕效果由EffectData控制
            PlayHurtReaction();

            // 应用击退：技能基础击退力 + 攻击帧附加击退力
            if (rb != null && attacker != null)
            {
                Vector2 knockbackDirection = (transform.position - attacker.transform.position).normalized;
                Vector2 finalKnockbackForce = frameData.knockbackForce;

                // 如果有技能数据，叠加技能的基础击退力
                if (damageInfo.skillData != null)
                {
                    finalKnockbackForce += damageInfo.skillData.knockbackForce;
                }

                rb.AddForce(knockbackDirection * finalKnockbackForce, ForceMode2D.Impulse);
            }
        }

        if (characterAttributes.currentHealth <= 0)
        {
            Die();
        }
    }

    private void ApplyStun(float duration)
    {
        LogManager.Log($"[Example_Enemy] 被硬直! 持续时间: {duration}秒");

        if (animator != null)
        {
            animator.SetBool("Stun", true);
        }

        Invoke(nameof(RecoverFromStun), duration);
    }

    private void RecoverFromStun()
    {
        LogManager.Log($"[Example_Enemy] 硬直结束");

        if (animator != null)
        {
            animator.SetBool("Stun", false);
        }
    }

    private void PlayHurtReaction()
    {
        if (animator != null)
        {
            animator.SetTrigger("Hurt");
        }
    }

    private void Die()
    {
        isDead = true;

        LogManager.Log($"[Example_Enemy] 敌人死亡");

        if (animator != null)
        {
            animator.SetTrigger("Die");
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

        Destroy(gameObject, 3f);
    }
}
