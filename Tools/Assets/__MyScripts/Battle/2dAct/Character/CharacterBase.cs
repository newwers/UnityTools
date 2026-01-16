using UnityEngine;

/// <summary>
/// 统一角色状态枚举
/// </summary>
public enum CharacterState
{
    // 共有状态
    Idle,           // 待机
    Attacking,      // 攻击
    SpecialAttacking, // 特殊攻击
    Dodging,        // 闪避
    Stunned,        // 硬直
    Hurt,           // 受伤
    Death,          // 死亡

    // 玩家特有状态
    Running,        // 移动
    Jumping,        // 跳跃
    Falling,        // 下落
    JumpAttacking,  // 跳跃攻击
    HeavyAttacking, // 重攻击
    AssistAttacking,// 协助攻击
    DashAttacking,  // 冲刺攻击
    Blocking,       // 格挡
    Parrying,       // 弹反
    Down,           // 倒地
    GettingUp,      // 爬起

    // 敌人特有状态
    Patrol,         // 巡逻
    Chase,          // 追击
    Retreat         // 撤退
}

/// <summary>
/// 角色状态管理接口
/// </summary>
public interface ICharacterState
{
    CharacterState CurrentState { get; }
    event System.Action<CharacterState, CharacterState> OnStateChanged;
    void ChangeState(CharacterState newState);
}

public interface IAttackable
{
    bool IsAttacking();
    void PerformAttack();
    void PerformAttack(AttackActionData attackData);
    bool CanAttack();
}

public interface IDodgeable
{
    bool CanDash();
    void PerformDash(Vector2 direction);
    bool IsDodging();
}

/// <summary>
/// 移动接口
/// </summary>
public interface IMoveable
{
    bool CanMove();
    void Move(Vector2 direction);
    bool IsMoving();
}

/// <summary>
/// 跳跃接口
/// </summary>
public interface IJumpable
{
    bool CanJump();
    void PerformJump();
    bool IsJumping();
    bool CanDoubleJump();
    void PerformDoubleJump();
}

/// <summary>
/// 格挡接口
/// </summary>
public interface IBlockable
{
    /// <summary>
    /// 检查是否可以格挡
    /// </summary>
    bool CanBlock();
    
    /// <summary>
    /// 执行格挡
    /// </summary>
    void PerformBlock();
    
    /// <summary>
    /// 检查是否正在格挡
    /// </summary>
    bool IsBlocking();
    
    /// <summary>
    /// 处理格挡时受到的伤害
    /// </summary>
    void BlockDamage(DamageInfo damageInfo, AttackActionData actionData, AttackFrameData frameData, CharacterBase attacker);
    
    /// <summary>
    /// 检查是否可以弹反
    /// </summary>
    bool CanParry();
    
    /// <summary>
    /// 执行弹反
    /// </summary>
    void PerformParry();
    
    /// <summary>
    /// 检查是否正在弹反
    /// </summary>
    bool IsParrying();
}

[DisallowMultipleComponent, RequireComponent(typeof(AttackHitVisualizer), typeof(PlayerAttributes), typeof(BuffSystem))]
public class CharacterBase : MonoBehaviour, IDamageable, IStunnable, IAttackable, IDodgeable, IMoveable, IJumpable, IBlockable, ICharacterState
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
    protected Transform currentTarget;

    [Header("状态管理")]
    [SerializeField] protected CharacterState currentState = CharacterState.Idle;


    // 状态管理实现
    public CharacterState CurrentState => currentState;
    public event System.Action<CharacterState, CharacterState> OnStateChanged;


    public virtual bool IsDead => currentState == CharacterState.Death;

    public Transform CurrentTarget => currentTarget;
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

    #region 状态管理实现

    /// <summary>
    /// 切换角色状态
    /// </summary>
    /// <param name="newState">新状态</param>
    public virtual void ChangeState(CharacterState newState)
    {
        if (currentState == newState) return;

        OnStateExit(currentState);
        CharacterState previousState = currentState;
        currentState = newState;
        OnStateEnter(newState);

        OnStateChanged?.Invoke(previousState, newState);
    }

    /// <summary>
    /// 状态进入时调用
    /// </summary>
    /// <param name="state">进入的状态</param>
    protected virtual void OnStateEnter(CharacterState state)
    {
        // 子类可以重写此方法来处理特定状态的进入逻辑
    }

    /// <summary>
    /// 状态退出时调用
    /// </summary>
    /// <param name="state">退出的状态</param>
    protected virtual void OnStateExit(CharacterState state)
    {
        // 子类可以重写此方法来处理特定状态的退出逻辑
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

    #region IAttackable 接口实现
    public virtual bool IsAttacking()
    {
        return false;
    }

    public virtual void PerformAttack()
    {
    }

    public virtual void PerformAttack(AttackActionData attackData)
    {
    }

    public virtual bool CanAttack()
    {
        return !IsDead;
    }
    #endregion

    #region IDodgeable 接口实现
    public virtual bool CanDash()
    {
        return !IsDead;
    }

    public virtual void PerformDash(Vector2 direction)
    {
    }

    public virtual bool IsDodging()
    {
        return false;
    }
    #endregion

    #region IMoveable 接口实现
    /// <summary>
    /// 检查是否可以移动
    /// </summary>
    /// <returns></returns>
    public virtual bool CanMove()
    {
        return !IsDead;
    }

    /// <summary>
    /// 执行移动
    /// </summary>
    /// <param name="direction"></param>
    public virtual void Move(Vector2 direction)
    {
        // 基础移动实现，子类可以重写
    }

    /// <summary>
    /// 检查是否正在移动
    /// </summary>
    /// <returns></returns>
    public virtual bool IsMoving()
    {
        return false;
    }
    #endregion IMoveable 接口实现

    #region IJumpable 接口实现
    /// <summary>
    /// 检查是否可以跳跃
    /// </summary>
    /// <returns></returns>
    public virtual bool CanJump()
    {
        return !IsDead;
    }

    /// <summary>
    /// 执行跳跃
    /// </summary>
    public virtual void PerformJump()
    {
        // 基础跳跃实现，子类可以重写
    }

    /// <summary>
    /// 检查是否正在跳跃
    /// </summary>
    /// <returns></returns>
    public virtual bool IsJumping()
    {
        return false;
    }

    /// <summary>
    /// 检查是否可以二段跳
    /// </summary>
    /// <returns></returns>
    public virtual bool CanDoubleJump()
    {
        return false;
    }

    /// <summary>
    /// 执行二段跳
    /// </summary>
    public virtual void PerformDoubleJump()
    {
        // 基础二段跳实现，子类可以重写
    }
    #endregion IJumpable 接口实现

    #region IBlockable 接口实现
    /// <summary>
    /// 检查是否可以格挡
    /// </summary>
    /// <returns>如果可以格挡返回true，否则返回false</returns>
    public virtual bool CanBlock()
    {
        return !IsDead && currentState != CharacterState.Attacking && currentState != CharacterState.Dodging && currentState != CharacterState.Stunned && currentState != CharacterState.Hurt;
    }
    
    /// <summary>
    /// 执行格挡
    /// </summary>
    public virtual void PerformBlock()
    {
        if (CanBlock())
        {
            ChangeState(CharacterState.Blocking);
        }
    }
    
    /// <summary>
    /// 检查是否正在格挡
    /// </summary>
    /// <returns>如果正在格挡返回true，否则返回false</returns>
    public virtual bool IsBlocking()
    {
        return currentState == CharacterState.Blocking;
    }
    
    /// <summary>
    /// 处理格挡时受到的伤害
    /// </summary>
    /// <param name="damageInfo">伤害信息</param>
    /// <param name="actionData">攻击动作数据</param>
    /// <param name="frameData">攻击帧数据</param>
    /// <param name="attacker">攻击者</param>
    public virtual void BlockDamage(DamageInfo damageInfo, AttackActionData actionData, AttackFrameData frameData, CharacterBase attacker)
    {
        // 基础格挡伤害处理，子类可以重写以实现不同的格挡效果
        // 默认实现：减少伤害或完全格挡
    }
    
    /// <summary>
    /// 检查是否可以弹反
    /// </summary>
    /// <returns>如果可以弹反返回true，否则返回false</returns>
    public virtual bool CanParry()
    {
        return !IsDead && currentState != CharacterState.Attacking && currentState != CharacterState.Dodging && currentState != CharacterState.Stunned && currentState != CharacterState.Hurt;
    }
    
    /// <summary>
    /// 执行弹反
    /// </summary>
    public virtual void PerformParry()
    {
        if (CanParry())
        {
            ChangeState(CharacterState.Parrying);
        }
    }
    
    /// <summary>
    /// 检查是否正在弹反
    /// </summary>
    /// <returns>如果正在弹反返回true，否则返回false</returns>
    public virtual bool IsParrying()
    {
        return currentState == CharacterState.Parrying;
    }
    #endregion IBlockable 接口实现

}
