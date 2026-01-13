using UnityEngine;



/// <summary>
/// 投掷物控制器
/// 负责运动、碰撞检测、命中处理和回收
/// 兼容当前伤害体系：通过AttackHitDetector.CreateDamageInfo创建DamageInfo并调用目标的TakeDamage
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class ProjectileController : MonoBehaviour
{
    // 投掷物配置数据
    public ProjectileData data;
    [FieldReadOnly]
    // 发射者引用（用于填充DamageInfo.attacker）
    public CharacterBase owner;

    // 当前已命中目标计数
    private int hitCount = 0;

    // 运动组件
    public Rigidbody2D rb;
    public Collider2D coll;
    public SpriteRenderer spriteRenderer;

    // 回收计时
    private float lifeTimer = 0f;


    private void OnEnable()
    {
        lifeTimer = 0f;
        hitCount = 0;
    }

    private void Update()
    {
        lifeTimer += Time.deltaTime;
        if (data != null && data.lifetime > 0 && lifeTimer >= data.lifetime)
        {
            ProjectileManager.Instance.ReturnProjectile(this);
        }
    }

    // 由管理器设置初速度并激活
    public void Launch(Vector2 direction, CharacterBase owner, ProjectileData projectileData)
    {
        gameObject.SetActive(true);

        spriteRenderer.flipX = direction.x < 0;

        this.owner = owner;
        this.data = projectileData;

        coll.isTrigger = projectileData.isTrigger;
        rb.gravityScale = projectileData.gravityScale;
        rb.bodyType = projectileData.bodyType;
        rb.linearDamping = projectileData.linearDamping;
        rb.angularDamping = projectileData.angularDamping;
        direction = new Vector2(direction.x * projectileData.directionOffset.x, projectileData.directionOffset.y);//左右方向上乘以偏移,上下方向上直接取偏移值
        var initialVelocity = direction.normalized * projectileData.initialSpeed;
        rb.AddForce(initialVelocity, ForceMode2D.Impulse);

        lifeTimer = 0f;
        hitCount = 0;

    }

    private void HandleHit(GameObject other)
    {
        if (owner == null || data == null || owner.gameObject == other) return;

        // 如果碰撞到Wall层，直接回收投掷物
        if (other.layer == LayerMask.NameToLayer("Wall"))
        {
            ProjectileManager.Instance.ReturnProjectile(this);
            return;
        }

        if (((1 << other.layer) & data.hitLayers) == 0) return;

        CharacterBase target = other.GetComponent<CharacterBase>();
        if (target == null) return;

        // 创建DamageInfo（借用AttackHitDetector的CreateDamageInfo逻辑）todo:这边的逻辑是否需要优化
        AttackActionData dummyAction = ScriptableObject.CreateInstance<AttackActionData>();
        dummyAction.skillData = data.skillData;

        AttackFrameData dummyFrame = new AttackFrameData();
        dummyFrame.damage = data.damage;
        dummyFrame.knockbackForce = data.knockbackForce;
        dummyFrame.effects = data.effects;

        DamageInfo dmg = AttackHitDetector.CreateDamageInfo(dummyAction, dummyFrame, owner, target);

        // 填充目标
        dmg.target = target;

        // 应用效果（投掷物命中时施加的效果）
        // 使用AttackHitDetector的ApplyFrameEffects方法通过反射调用（因为它是私有）, 为避免反射，这里直接应用到目标/攻击者的BuffSystem
        if (dummyFrame.effects != null && dummyFrame.effects.Count > 0)
        {
            foreach (var eff in dummyFrame.effects)
            {
                if (eff == null) continue;
                var receiver = (eff.effectTarget == EffectTarget.Attacker) ? owner.BuffSystem : target.BuffSystem;
                if (receiver != null)
                {
                    receiver.ApplyBuff(eff, owner, target);
                }
            }
        }

        // 调用受击逻辑
        target.TakeDamage(dmg, dummyAction, dummyFrame, owner);

        hitCount++;

        // 达到最大命中次数则回收
        if (data.maxHitTargets > 0 && hitCount >= data.maxHitTargets)
        {
            ProjectileManager.Instance.ReturnProjectile(this);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        HandleHit(other.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleHit(collision.gameObject);
    }
}
