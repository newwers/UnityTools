using UnityEngine;

[DisallowMultipleComponent, RequireComponent(typeof(AttackHitVisualizer), typeof(PlayerAttributes), typeof(BuffSystem))]
public class CharacterBase : MonoBehaviour, IDamageable, IStunnable
{
    [Header("角色属性")]
    [Tooltip("角色属性组件")]
    public PlayerAttributes PlayerAttributes;

    [Tooltip("Buff系统组件")]
    public BuffSystem BuffSystem;

    public Rigidbody2D rb;
    public BoxCollider2D boxCollider2D;


    [FieldReadOnly]
    public bool isFacingRight = true;

    public virtual bool IsDead => false;

    public Transform Transform => transform;
    #region Unity函数


    private void Reset()
    {
        //自动获取组件
        PlayerAttributes = GetComponent<PlayerAttributes>();
        BuffSystem = GetComponent<BuffSystem>();
        rb = GetComponent<Rigidbody2D>();
        boxCollider2D = GetComponent<BoxCollider2D>();
    }

    protected virtual void Awake()
    {
        PlayerAttributes?.Initialize();
        BuffSystem.Init(this, PlayerAttributes.characterAtttibute, rb, this);
    }

    #endregion
    #region 接口函数

    /// <summary>
    /// 应用眩晕
    /// </summary>
    /// <param name="stunDuration"></param>
    public virtual void ApplyStun(float stunDuration)
    {

    }
    /// <summary>
    /// 收到伤害
    /// </summary>
    /// <param name="damageInfo"></param>
    /// <param name="actionData"></param>
    /// <param name="frameData"></param>
    /// <param name="attacker"></param>
    public virtual void TakeDamage(DamageInfo damageInfo, AttackActionData actionData, AttackFrameData frameData, CharacterBase attacker)
    {

    }
    #endregion

}
