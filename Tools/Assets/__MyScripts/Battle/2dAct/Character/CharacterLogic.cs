/*
角色控制
状态管理
攻击逻辑
 */
using System.Collections;
using System.Linq;
using UnityEngine;

public enum PlayerState
{
    Idle,           // 待机
    Running,        // 移动
    Jumping,        // 跳跃
    Falling,        // 下落
    Attacking,      // 普通攻击
    JumpAttacking,  // 跳跃攻击
    HeavyAttacking, // 重攻击
    AssistAttacking,// 协助攻击
    SpecialAttacking,// 特殊攻击
    Dashing,        // 冲刺
    DashAttacking,  // 冲刺攻击
    Blocking,       // 格挡
    Parrying,       // 弹反
    Down,           // 倒地
    GettingUp,      // 爬起
    Stunned         // 硬直
}


public enum AttackPhase
{
    WindUp,     // 前摇阶段
    Active,     // 攻击中阶段,只有在攻击阶段才进行命中检测
    Recovery    // 后摇阶段
}

public class CharacterLogic : MonoBehaviour
{

    // 逻辑事件
    public System.Action<PlayerState, PlayerState> OnStateChanged;
    public System.Action<int> OnAttackCombo;
    public System.Action OnJump;
    public System.Action OnLandAction;
    public System.Action<ActionData, AttackFrameData, GameObject> OnHurt;
    public System.Action OnDeath;
    // 添加格挡成功事件
    public System.Action OnBlockSuccess; // 格挡成功
    public System.Action<GameObject> OnParrySuccess; // 弹反成功（参数为被弹反的敌人）
    //硬直事件
    public System.Action OnStunned;


    [Header("逻辑状态")]
    public PlayerState CurrentState = PlayerState.Idle;
    public AttackPhase currentAttackPhase = AttackPhase.WindUp;

    [Header("移动设置")]
    public float moveSpeed = 8f;
    public float acceleration = 15f;
    public float deceleration = 20f;

    [Header("跳跃设置")]
    public float jumpForce = 16f;
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;
    public float coyoteTime = 0.1f;

    [Header("地面检测")]
    public LayerMask groundLayer;
    public Vector2 groundCheckSize = new Vector2(0.5f, 0.1f);
    public Vector2 groundCheckOffset = new Vector2(0f, -0.5f);

    [Header("冲刺设置")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 0.5f;

    [Header("攻击系统")]
    public ActionManager attackManager;
    public float attackResetTime = 1.0f;

    // 格挡和弹反相关字段
    [Header("格挡设置")]
    public float blockDamageReduction = 0.7f; // 格挡伤害减免
    public float parryDamageMultiplier = 1.5f; // 弹反伤害加成
    public float parryWindow = 0.2f; // 弹反输入窗口
    public float parryStunDuration = 1.0f; // 弹反成功时敌人的硬直时间



    // 添加强力攻击相关变量
    [Header("强力攻击设置")]
    public float heavyAttackHoldTime = 0.5f;
    private bool isHeavyAttackCharging = false;
    private float heavyAttackChargeStartTime = 0f;


    // 攻击系统相关变量
    [Header("攻击调试信息")]
    public ActionData currentAttackData;
    public float currentAttackTimer = 0f;
    public int currentComboIndex = 0;



    [FieldReadOnly]
    public bool isFacingRight = true;
    [FieldReadOnly]
    public bool hasBufferedAttack; // 是否有缓冲的攻击输入

    // 组件引用
    private Rigidbody2D rb;
    private InputHandler inputHandler;
    private CharacterAnimation animHandler;

    // 添加攻击检测相关字段
    private string currentAttackId;
    private AttackHitVisualizer attackVisualizer;

    private int jumpProtectionFrames = 0;
    private const int JUMP_PROTECTION_FRAME_COUNT = 3; // 跳跃保护3帧

    // 逻辑状态
    private bool m_IsGrounded;
    private float lastGroundedTime;
    private float lastDashTime;
    private float lastAttackTime;

    // 格挡状态
    /// <summary>
    /// 格挡中
    /// </summary>
    private bool isBlocking = false;
    private bool isParryWindowActive = false;
    private float parryWindowStartTime = 0f;
    private bool canParry = false; // 是否可以触发弹反

    [Header("动作优先级")]
    [SerializeField] private int currentActionPriority = 0; // 当前执行动作的优先级
    public const int StunPriority = 300; // 硬直优先级最高
    public const int BlockPriority = 200; // 格挡优先级
    public const int ParryPriority = 250; // 弹反优先级
    public const int DashPriority = 150; // 冲刺优先级
    public const int IdlePriority = 0; // 待机优先级




    public bool IsGrounded
    {
        get { return m_IsGrounded; }
    }

    public bool IsAttacking()
    {
        return CurrentState == PlayerState.Attacking ||
               CurrentState == PlayerState.HeavyAttacking ||
               CurrentState == PlayerState.DashAttacking ||
               CurrentState == PlayerState.JumpAttacking ||
               CurrentState == PlayerState.Parrying;//弹反也算攻击状态
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        inputHandler = GetComponent<InputHandler>();
        animHandler = GetComponent<CharacterAnimation>();
        attackVisualizer = GetComponent<AttackHitVisualizer>();
    }

    private void OnEnable()
    {
        // 订阅输入事件
        inputHandler.OnMoveInput += HandleMoveInput;
        inputHandler.OnJumpInput += HandleJumpInput;
        inputHandler.OnJumpCanceledAction += HandleJumpCanceled;
        inputHandler.OnDashInput += HandleDashInput;
        inputHandler.OnAttackStarted += HandleAttackStarted;
        inputHandler.OnBlockStartedAction += HandleBlockStarted;
        inputHandler.OnBlockCanceledAction += HandleBlockCanceled;
        inputHandler.OnHeavyAttackStarted += HandleHeavyAttackStarted;
        OnHurt += OnHurtHandler;
    }


    private void OnDisable()
    {
        // 订阅输入事件
        inputHandler.OnMoveInput -= HandleMoveInput;
        inputHandler.OnJumpInput -= HandleJumpInput;
        inputHandler.OnJumpCanceledAction -= HandleJumpCanceled;
        inputHandler.OnDashInput -= HandleDashInput;
        inputHandler.OnAttackStarted -= HandleAttackStarted;
        inputHandler.OnBlockStartedAction -= HandleBlockStarted;
        inputHandler.OnBlockCanceledAction -= HandleBlockCanceled;
        inputHandler.OnHeavyAttackStarted -= HandleHeavyAttackStarted;
        OnHurt -= OnHurtHandler;
    }

    private void Update()
    {
        UpdateTimers();
        CheckGround();

        // 更新跳跃保护帧
        if (jumpProtectionFrames > 0)
            jumpProtectionFrames--;

        HandleStateMachine();
        ProcessAttackBuffer();

        // 更新攻击阶段
        if (IsAttacking())
        {
            UpdateAttackPhase();
        }
    }

    private void UpdateTimers()
    {
        // 跳跃状态更新
        if (CurrentState == PlayerState.Jumping && rb.linearVelocity.y < 0)
        {
            ChangeState(PlayerState.Falling);
        }
    }


    private void OnHurtHandler(ActionData attackData, AttackFrameData frameData, GameObject attacker)
    {
        // 检查是否在格挡状态,并且在弹反窗口内
        if (isBlocking)//格挡状态
        {
            LogManager.Log($"当前在格挡状态");
            if (CanBlockAttack(attacker.transform.position))//检测格挡攻击方向
            {
                HandleBlockSuccess(frameData, attacker);
                //if (isParryWindowActive)//是否在弹反窗口内
                //{
                //    //完美格挡
                //}
                //else
                //{
                //    //普通格挡
                //}
            }
            return;
        }

        // 检查是否造成眩晕
        if (frameData.causeStun)
        {
            ApplyStun(frameData.stunDuration);
        }
        else
        {
            // 根据攻击优先级决定受击表现
            bool shouldPlayHitAnimation = ShouldPlayHitAnimation(attackData.priority);
            animHandler.OnHurt(shouldPlayHitAnimation);
        }


        // 应用伤害和击退
        if (rb != null)
        {
            Vector2 knockbackDirection = (transform.position - attacker.transform.position).normalized;
            rb.AddForce(knockbackDirection * frameData.knockbackForce, ForceMode2D.Impulse);
        }

        // 播放命中特效
        if (frameData.hitEffect != null)
        {
            Instantiate(frameData.hitEffect, transform.position, Quaternion.identity);
        }
    }

    private bool ShouldPlayHitAnimation(int priority)
    {
        // 这里需要从攻击数据获取优先级，暂时使用默认逻辑
        // 在实际攻击检测中，这个判断会在 ProcessHit 方法中完成
        return currentActionPriority < priority;
    }

    /// <summary>
    /// 处理格挡成功
    /// </summary>
    private void HandleBlockSuccess(AttackFrameData frameData, GameObject attacker)
    {
        LogManager.Log($"[CharacterLogic] 格挡成功! 来自 {attacker.name} 的攻击");

        // 触发格挡成功事件
        OnBlockSuccess?.Invoke();

        // 轻微击退效果
        if (rb != null)
        {
            Vector2 knockbackDirection = (transform.position - attacker.transform.position).normalized;
            rb.AddForce(knockbackDirection * frameData.knockbackForce * 0.3f, ForceMode2D.Impulse);
        }

        // 播放格挡特效
        if (frameData.hitEffect != null)
        {
            Instantiate(frameData.hitEffect, transform.position, Quaternion.identity);
        }

        // 激活弹反窗口
        ActivateParryWindow();
    }

    /// <summary>
    /// 尝试触发弹反
    /// </summary>
    private void TryParry()
    {
        if (!canParry || !isParryWindowActive) return;

        LogManager.Log($"[CharacterLogic] 触发弹反!");
        SetIsBlockState(false);//弹反时,取消格挡状态

        // 切换到弹反状态
        ChangeState(PlayerState.Parrying);

        PerformAttack();//执行弹反攻击

        // 这里可以添加弹反的特殊效果，比如时间减缓、屏幕特效等

        canParry = false; // 防止重复触发

        OnParrySuccess?.Invoke(null);
    }

    /// <summary>
    /// 强制刷新状态（忽略状态检查）
    /// </summary>
    public void ForceRefreshState()
    {
        PlayerState newState = DetermineAppropriateState();
        ChangeState(newState);
        LogManager.Log($"[CharacterLogic] 强制刷新状态到: {newState}");
    }

    /// <summary>
    /// 安全刷新状态，只在特定状态下刷新
    /// characterLogic.SafeRefreshState(PlayerState.Idle, PlayerState.Running);
    /// </summary>
    public void SafeRefreshState(params PlayerState[] allowedStates)
    {
        if (allowedStates.Contains(CurrentState) || allowedStates.Length == 0)
        {
            RefreshState();
        }
    }

    // 添加统一的刷新状态函数
    /// <summary>
    /// 根据当前条件自动刷新到合适的状态
    /// </summary>
    public void RefreshState()
    {
        // 如果当前状态是不可自动刷新的状态，则保持原状态
        if (!CanStateBeRefreshed())
            return;

        PlayerState newState = DetermineAppropriateState();

        if (CurrentState != newState)
        {
            ChangeState(newState);
            LogManager.Log($"[CharacterLogic] 自动刷新状态到: {newState}");
        }
    }

    /// <summary>
    /// 判断当前状态是否可以被自动刷新
    /// </summary>
    private bool CanStateBeRefreshed()
    {
        return true;
        switch (CurrentState)
        {
            case PlayerState.Down:
            case PlayerState.GettingUp:
            case PlayerState.Stunned:
            case PlayerState.Dashing:
            case PlayerState.Blocking:
            case PlayerState.Parrying:
                // 这些状态需要特定条件才能退出，不能自动刷新
                return false;

            // 攻击状态在结束后应该可以被刷新
            case PlayerState.Attacking:
            case PlayerState.HeavyAttacking:
            case PlayerState.DashAttacking:
            case PlayerState.JumpAttacking:
            case PlayerState.SpecialAttacking:
                // 只有在攻击结束后（没有当前攻击数据）才可以刷新
                return currentAttackData == null;

            default:
                return true;
        }
    }

    /// <summary>
    /// 根据当前条件确定最合适的状态
    /// </summary>
    private PlayerState DetermineAppropriateState()
    {
        // 优先检查地面状态
        if (!m_IsGrounded)
        {
            // 空中状态
            if (rb.linearVelocity.y > 0.1f)
                return PlayerState.Jumping;
            else
                return PlayerState.Falling;
        }

        // 地面状态
        if (Mathf.Abs(inputHandler.MoveInput.x) > 0.1f)
            return PlayerState.Running;
        else
            return PlayerState.Idle;
    }

    private void CheckGround()
    {
        Vector2 checkPosition = (Vector2)transform.position + groundCheckOffset;
        bool wasGrounded = m_IsGrounded;
        m_IsGrounded = Physics2D.OverlapBox(checkPosition, groundCheckSize, 0f, groundLayer);

        // 跳跃保护帧内不检测落地
        if (jumpProtectionFrames > 0)
        {
            m_IsGrounded = false;
        }

        if (m_IsGrounded && !wasGrounded)//落地时
        {
            OnLand();

            // 保护帧内不切换状态
            if (jumpProtectionFrames == 0)
            {
                // 使用统一的状态刷新函数
                RefreshState();
            }
        }
        else if (!m_IsGrounded && wasGrounded)
        {
            lastGroundedTime = Time.time;

            // 离开地面时刷新状态
            RefreshState();
        }
    }

    private void OnLand()
    {
        //if (CurrentState == PlayerState.JumpAttacking)//在空中攻击落地时,直接结束攻击
        //{
        //    //EndAttack();//切换状态时会调用breakAttack,就会清除攻击数据
        //}
        OnLandAction?.Invoke();
    }

    private void HandleStateMachine()
    {
        CheckBufferedInputs();// 检查预输入

        switch (CurrentState)
        {
            case PlayerState.Idle:
            case PlayerState.Running:
                HandleGroundedState();
                break;

            case PlayerState.Jumping:
            case PlayerState.Falling:
                HandleAirborneState();
                break;

            case PlayerState.Dashing:
                HandleDashState();
                break;

            case PlayerState.Blocking:
                HandleBlockState();
                break;

            case PlayerState.Attacking:
            case PlayerState.HeavyAttacking:
            case PlayerState.DashAttacking:
            case PlayerState.JumpAttacking:
                HandleAttackState();
                break;
        }
    }

    #region 状态处理
    private void HandleGroundedState()
    {
        // 状态转换
        if (Mathf.Abs(inputHandler.MoveInput.x) > 0.1f)
        {
            ChangeState(PlayerState.Running);
        }
        else
        {
            ChangeState(PlayerState.Idle);
        }
    }

    private void HandleAirborneState()
    {
        HandleAirMovement();
        HandleJumpPhysics();

        // 落地检测
        if (m_IsGrounded)
        {
            ChangeState(PlayerState.Idle);
        }
    }

    private void HandleDashState()
    {
        float dashDirection = isFacingRight ? 1f : -1f;
        rb.linearVelocity = new Vector2(dashDirection * dashSpeed, rb.linearVelocity.y);
    }

    private void HandleBlockState()
    {
        rb.linearVelocity = new Vector2(inputHandler.MoveInput.x * moveSpeed * 0.3f, rb.linearVelocity.y);
    }

    private void HandleAttackState()
    {
        if (CurrentState == PlayerState.JumpAttacking)
        {
            // 跳跃攻击时使用空中移动逻辑
            HandleAirMovement();
            //HandleJumpPhysics(); // 保持跳跃物理效果
        }
        else if (currentAttackData != null && currentAttackData.enableMovement && currentAttackPhase == AttackPhase.Active)
        {
            // 攻击状态下可以轻微移动
            HandleAttackMovement();
        }
        else
        {
            // 限制移动
            rb.linearVelocity = new Vector2(rb.linearVelocity.x * 0.9f, rb.linearVelocity.y);
        }
    }
    #endregion

    #region 攻击阶段系统
    private void UpdateAttackPhase()
    {
        if (currentAttackData == null) return;

        currentAttackTimer += Time.deltaTime;

        switch (currentAttackPhase)
        {
            case AttackPhase.WindUp:
                if (currentAttackTimer >= currentAttackData.windUpTime)
                {
                    EnterActivePhase();
                }
                break;

            case AttackPhase.Active:
                // 每帧进行攻击检测
                if (AttackHitDetector.Instance != null && !string.IsNullOrEmpty(currentAttackId))
                {
                    AttackHitDetector.Instance.CheckHitForFrame(
                        currentAttackId, currentAttackData, transform.position,
                        isFacingRight, currentAttackTimer, gameObject);
                }


                if (currentAttackTimer >= currentAttackData.windUpTime + currentAttackData.activeTime)
                {
                    EnterRecoveryPhase();
                }
                break;

            case AttackPhase.Recovery:
                if (currentAttackTimer >= currentAttackData.TotalDuration)
                {
                    EndAttack();
                }
                else
                {
                    CheckComboInput();
                }
                break;
        }
    }

    //private int CalculateCurrentFrameIndex()
    //{
    //    if (currentAttackData == null) return 0;

    //    float activeElapsed = currentAttackTimer - currentAttackData.windUpTime;
    //    activeElapsed = Mathf.Clamp(activeElapsed, 0f, currentAttackData.activeTime);

    //    int totalActiveFrames = currentAttackData.ActualActiveFrames;
    //    if (totalActiveFrames <= 0) return 0;

    //    float phaseProgress = activeElapsed / currentAttackData.activeTime;
    //    int frameIndex = Mathf.FloorToInt(phaseProgress * totalActiveFrames);
    //    frameIndex = Mathf.Clamp(frameIndex, 0, totalActiveFrames - 1);

    //    int actualFrameIndex = currentAttackData.ActualWindUpFrames + frameIndex;
    //    return actualFrameIndex;
    //}

    private void EnterActivePhase()
    {
        currentAttackPhase = AttackPhase.Active;

        // 开始攻击检测
        currentAttackId = AttackHitDetector.Instance.StartAttackDetection(
            currentAttackData, transform.position, isFacingRight, gameObject);


        // 设置动画速度
        if (animHandler != null)
        {
            animHandler.SetAttackAnimationSpeed(currentAttackPhase, currentAttackData);
        }

        LogManager.Log($"[CharacterLogic] 进入攻击中阶段");
    }

    private void EnterRecoveryPhase()
    {
        currentAttackPhase = AttackPhase.Recovery;
        // 设置动画速度
        if (animHandler != null)
        {
            animHandler.SetAttackAnimationSpeed(currentAttackPhase, currentAttackData);
        }
        LogManager.Log($"[CharacterLogic] 进入后摇阶段");
    }
    /// <summary>
    /// 攻击结束后调用函数
    /// </summary>
    private void EndAttack()
    {
        LogManager.Log($"[CharacterLogic] 攻击结束");

        if (CurrentState == PlayerState.Parrying)//弹反攻击结束
        {
            OnParryAttackEnd();
        }

        // 结束攻击检测
        if (!string.IsNullOrEmpty(currentAttackId))
        {
            AttackHitDetector.Instance.EndAttackDetection(currentAttackId);
            currentAttackId = null;
        }

        // 清理动画参数
        if (animHandler != null && currentAttackData != null)
        {
            animHandler.ClearAttackAnimationParameter(currentAttackData);
        }

        // 清除可视化
        if (attackVisualizer != null)
        {
            attackVisualizer.ClearFrameData();
        }

        // 清除连招输入
        hasBufferedAttack = false;
        currentAttackData = null;
        currentComboIndex = 0;

        // 强制刷新状态
        ForceRefreshState();
    }

    /// <summary>
    /// 弹反成功（由动画事件调用）
    /// </summary>
    public void OnParryAttackEnd()
    {
        LogManager.Log($"[CharacterLogic] 弹反成功!");

        // 切换到攻击状态或返回格挡状态
        if (inputHandler.IsBlockPressed)
        {
            ChangeState(PlayerState.Blocking);
        }
        else
        {
            RefreshState();
        }
    }

    /// <summary>
    /// 施加硬直
    /// </summary>
    public void ApplyStun(float duration)
    {
        if (CurrentState == PlayerState.Stunned) return;

        currentActionPriority = 300; // 硬直优先级最高

        LogManager.Log($"[CharacterLogic] 施加 硬直时间: {duration}秒");

        // 切换到硬直状态
        ChangeState(PlayerState.Stunned);

        //播放硬直动画
        OnStunned?.Invoke();

        // 设置硬直计时器
        StartCoroutine(RecoverFromStun(duration));
    }

    private IEnumerator RecoverFromStun(float duration)
    {
        yield return new WaitForSeconds(duration);

        if (CurrentState == PlayerState.Stunned)
        {
            RefreshState();
        }
    }

    /// <summary>
    /// 攻击被打断时调用函数
    /// </summary>
    private void BreakAttack()
    {
        // 清理动画参数
        if (animHandler != null && currentAttackData != null)
        {
            animHandler.ClearAttackAnimationParameter(currentAttackData);
        }

        // 清除可视化
        if (attackVisualizer != null)
        {
            attackVisualizer.ClearFrameData();
        }

        currentAttackData = null;
        currentAttackPhase = AttackPhase.WindUp;
        currentAttackTimer = 0f;
        currentComboIndex = 0;
        hasBufferedAttack = false;

        // 使用统一的状态刷新函数
        //RefreshState();//这边会死循环和ChangeState中调用的冲突
    }

    private void HandleAttackMovement()
    {
        if (currentAttackData == null || !currentAttackData.enableMovement) return;

        float phaseProgress = (currentAttackTimer - currentAttackData.windUpTime) / currentAttackData.activeTime;
        float movementFactor = currentAttackData.movementCurve.Evaluate(phaseProgress);

        float movementDirection = isFacingRight ? 1f : -1f;
        Vector2 movement = new Vector2(movementDirection * currentAttackData.movementSpeed * movementFactor, 0f);

        rb.linearVelocity = new Vector2(movement.x, rb.linearVelocity.y);
    }

    private void CheckComboInput()
    {
        if (hasBufferedAttack && currentAttackData != null && currentAttackData.canCombo && currentAttackTimer >= currentAttackData.ComboStartTime)
        {
            LogManager.Log($"[CharacterLogic] 后摇阶段检测到连招输入，执行连招");
            PerformComboAttack();
            hasBufferedAttack = false;
        }
    }

    private void PerformComboAttack()
    {
        // 连招不检查 CanAttack()，因为已经在攻击状态中
        if (currentAttackData != null && currentAttackData.canCombo)
        {
            PerformAttack();
        }
        else
        {
            LogManager.Log($"[CharacterLogic] 当前攻击不允许连招");
        }
    }



    #endregion

    #region 输入处理
    private void HandleMoveInput(Vector2 input)
    {
        if (CurrentState == PlayerState.Idle || CurrentState == PlayerState.Running)
        {
            HandleMovement(input);
        }
        else if (CurrentState == PlayerState.Jumping || CurrentState == PlayerState.Falling)
        {
            HandleAirMovement();
        }

        // 转向处理
        if (input.x > 0 && !isFacingRight)
            Flip();
        else if (input.x < 0 && isFacingRight)
            Flip();
    }

    private void HandleJumpInput()
    {
        // 跳跃优先级最高，可以打断除不可打断状态外的所有状态
        if (CanJump())
        {
            // 如果当前正在冲刺，先结束冲刺
            if (CurrentState == PlayerState.Dashing)
            {
                StopCoroutine(EndDashAfterTime());
                ChangeState(PlayerState.Jumping);
            }

            PerformJump();
        }
    }

    private void HandleJumpCanceled()
    {
        // 短按跳跃处理
    }

    private void HandleDashInput()
    {
        // 冲刺优先级第二高，可以打断攻击和跳跃
        if (CanDash())
        {
            // 如果当前正在跳跃，先结束跳跃
            if (CurrentState == PlayerState.Jumping || CurrentState == PlayerState.Falling)
            {
                // 跳跃中可以立即切换到冲刺
                ChangeState(PlayerState.Dashing);
            }

            PerformDash();
        }
    }

    private void HandleAttackStarted()
    {
        LogManager.Log($"[CharacterLogic] 收到攻击输入，检查是否可以攻击");

        // 检查是否在弹反窗口内
        if (isParryWindowActive && canParry)
        {
            TryParry();
            return;
        }

        if (IsAttacking())
        {
            BufferAttackInput();
        }
        else if (CanAttack())
        {
            PerformAttack();
        }
        else
        {
            // 如果不能立即攻击，则缓冲攻击输入
            LogManager.Log($"[CharacterLogic] 当前无法攻击，缓冲攻击输入");
            BufferAttackInput();
        }
    }

    private void HandleBlockStarted()
    {
        if (CanBlock())
        {
            SetIsBlockState(true);
            LogManager.Log($"切换到格挡状态");
            ChangeState(PlayerState.Blocking);
        }
    }

    private void HandleBlockCanceled()
    {
        LogManager.Log($"取消格挡状态");
        SetIsBlockState(false);
        if (CurrentState == PlayerState.Blocking)//格挡状态下取消
        {
            isParryWindowActive = false;
            canParry = false;
            RefreshState();
        }

        if (CurrentState == PlayerState.Parrying)//弹反状态下取消
        {

        }
    }

    private void SetIsBlockState(bool state)
    {
        isBlocking = state;
    }

    /// <summary>
    /// 重攻击开始输入处理
    /// </summary>
    private void HandleHeavyAttackStarted()
    {
        if (CanHeavyAttack())
        {
            isHeavyAttackCharging = true;
            heavyAttackChargeStartTime = Time.time;
            PerformHeavyAttack();
            LogManager.Log($"[CharacterLogic] 开始强力攻击");
        }
    }

    private bool CanHeavyAttack()
    {
        // 强力攻击只能在特定状态下使用
        switch (CurrentState)
        {
            case PlayerState.Idle:
            case PlayerState.Running:
            case PlayerState.Attacking:
                return true;

            default:
                return false;
        }
    }

    private void PerformHeavyAttack()
    {
        if (attackManager == null || attackManager.heavyAttack == null)
        {
            LogManager.LogError($"[CharacterLogic] 强力攻击未配置!");
            return;
        }

        LogManager.Log($"[CharacterLogic] 执行强力攻击");

        // 设置攻击数据
        currentAttackData = attackManager.heavyAttack;
        currentAttackPhase = AttackPhase.WindUp;
        currentAttackTimer = 0f;
        currentComboIndex = 0;

        // 根据当前状态设置适当的攻击状态
        if (!m_IsGrounded)
        {
            ChangeState(PlayerState.JumpAttacking);
        }
        else
        {
            ChangeState(PlayerState.HeavyAttacking);
        }

        // 触发动画
        if (animHandler != null)
        {
            animHandler.SetAttackAnimationParameter(currentAttackData);
            animHandler.SetAttackAnimationSpeed(currentAttackPhase, currentAttackData);
        }

        heavyAttackChargeStartTime = 0f;
        isHeavyAttackCharging = false;
    }


    #endregion

    #region 预输入检查
    private void CheckBufferedInputs()
    {
        // 只检查攻击预输入
        if (CanAttack() && inputHandler.TryGetBufferedInput(InputCommandType.Attack, out var attackCmd))
        {
            LogManager.Log($"[CharacterLogic] 从缓冲区获取到攻击输入: {attackCmd}");
            PerformAttack();
        }
    }
    #endregion

    #region 条件检查方法
    private bool CanJump()
    {
        bool coyoteTimeValid = Time.time - lastGroundedTime <= coyoteTime;
        return (m_IsGrounded || coyoteTimeValid) && CanInterruptForJump();
    }

    private bool CanDash()
    {
        return Time.time - lastDashTime >= dashCooldown && CanInterruptForDash();
    }

    private bool CanAttack()
    {
        return CanInterruptForAttack();
    }
    /// <summary>
    /// 格挡状态检测
    /// </summary>
    /// <returns></returns>
    private bool CanBlock()
    {
        return CanChangeState(PlayerState.Blocking);
    }

    /// <summary>
    /// 检查是否可以格挡此次攻击
    /// 格挡逻辑检测
    /// </summary>
    private bool CanBlockAttack(Vector3 attackerPosition)
    {
        if (!isBlocking) return false;

        // 检查攻击方向：只格挡来自前方的攻击
        Vector3 attackDirection = (attackerPosition - transform.position).normalized;
        float dotProduct = Vector3.Dot(attackDirection, isFacingRight ? Vector3.right : Vector3.left);

        return dotProduct > 0; // 攻击来自前方
    }


    // 跳跃打断检查：可以打断除不可打断状态外的所有状态
    private bool CanInterruptForJump()
    {
        switch (CurrentState)
        {
            case PlayerState.Down:
            case PlayerState.GettingUp:
            case PlayerState.Stunned:
                return false;

            case PlayerState.Dashing: // 跳跃可以打断冲刺
                return true;

            case PlayerState.Attacking:
            case PlayerState.HeavyAttacking:
            case PlayerState.AssistAttacking:
            case PlayerState.SpecialAttacking:
            case PlayerState.DashAttacking:
            case PlayerState.JumpAttacking:
                // 攻击状态可以直接被跳跃打断
                return currentAttackData == null || currentAttackData.canCancel;

            default:
                return CanChangeState(PlayerState.Jumping);
        }
    }

    // 冲刺打断检查：可以打断攻击和跳跃
    private bool CanInterruptForDash()
    {
        switch (CurrentState)
        {
            case PlayerState.Down:
            case PlayerState.GettingUp:
            case PlayerState.Stunned:
                return false;

            case PlayerState.Jumping:
            case PlayerState.Falling: // 冲刺可以打断跳跃
                return true;

            case PlayerState.Attacking:
            case PlayerState.HeavyAttacking:
            case PlayerState.AssistAttacking:
            case PlayerState.SpecialAttacking:
            case PlayerState.DashAttacking:
            case PlayerState.JumpAttacking:
                // 攻击状态可以直接被冲刺打断
                return currentAttackData == null || currentAttackData.canCancel;

            default:
                return CanChangeState(PlayerState.Dashing);
        }
    }

    // 攻击打断检查：优先级最低，只能在特定状态下执行
    private bool CanInterruptForAttack()
    {
        switch (CurrentState)
        {
            case PlayerState.Down:
            case PlayerState.GettingUp:
            case PlayerState.Stunned:
                //case PlayerState.Dashing: // 冲刺攻击能打断冲刺
                return false;

            case PlayerState.Jumping: // 跳跃中允许攻击
            case PlayerState.Falling:
                return attackManager != null && attackManager.jumpAttack != null;

            case PlayerState.Attacking:
            case PlayerState.HeavyAttacking:
            case PlayerState.DashAttacking:
            case PlayerState.JumpAttacking:
                // 在后摇阶段且允许连招时可以中断
                if (currentAttackPhase == AttackPhase.Recovery &&
                    currentAttackData != null &&
                    currentAttackData.canCombo)
                {
                    return true;
                }
                // 其他阶段需要动画可中断
                //return animHandler.CanInterruptCurrentAnimation();
                return false; // 攻击状态下不允许被新攻击打断
            default:
                return true;
        }
    }

    private bool CanDashAttack()
    {
        return CanInterruptForDashAttack();
    }

    private bool CanInterruptForDashAttack()
    {
        // 冲刺攻击优先级较高，可以打断大多数状态
        switch (CurrentState)
        {
            case PlayerState.Down:
            case PlayerState.GettingUp:
            case PlayerState.Stunned:
                return false;

            default:
                return true;
        }
    }


    #endregion

    #region 攻击预输入系统
    private void BufferAttackInput()
    {
        // 设置攻击缓冲标志
        hasBufferedAttack = true;

        LogManager.Log($"[CharacterLogic] 缓冲攻击输入");
    }

    private void ProcessAttackBuffer()
    {
        // 检查是否有缓冲的攻击输入且仍在有效时间内
        if (hasBufferedAttack)
        {
            // 如果当前可以攻击，则执行攻击
            if (CanAttack())
            {
                PerformAttack();
                hasBufferedAttack = false; // 清除缓冲标志
            }
        }
    }

    // 在动画结束时检查是否有缓冲的攻击输入
    public void CheckForBufferedAttackOnAnimationEnd()
    {
        if (hasBufferedAttack && CanAttack())
        {
            PerformAttack();
            hasBufferedAttack = false;
        }
    }
    #endregion

    #region 动作执行
    private void HandleMovement(Vector2 input)
    {
        float targetSpeed = input.x * moveSpeed;
        float speedDiff = targetSpeed - rb.linearVelocity.x;
        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;
        float movement = Mathf.Pow(Mathf.Abs(speedDiff) * accelRate, 0.9f) * Mathf.Sign(speedDiff);

        rb.AddForce(movement * Vector2.right);
    }

    private void HandleAirMovement()
    {
        float targetSpeed = inputHandler.MoveInput.x * moveSpeed * 0.8f;
        float speedDiff = targetSpeed - rb.linearVelocity.x;
        float movement = speedDiff * acceleration * 0.5f;
        rb.AddForce(movement * Vector2.right);
    }

    private void HandleJumpPhysics()
    {
        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }
        else if (rb.linearVelocity.y > 0 && !inputHandler.IsBlockPressed)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        }
    }

    private void PerformJump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        ChangeState(PlayerState.Jumping);
        OnJump?.Invoke();

        // 设置跳跃保护帧
        jumpProtectionFrames = JUMP_PROTECTION_FRAME_COUNT;

        LogManager.Log($"[CharacterLogic] 执行跳跃，打断当前状态");
    }

    private void PerformDash()
    {
        ChangeState(PlayerState.Dashing);
        lastDashTime = Time.time;

        // 通知InputHandler冲刺开始
        if (inputHandler != null)
        {
            inputHandler.OnDashStarted();
        }

        StartCoroutine(EndDashAfterTime());

        LogManager.Log($"[CharacterLogic] 执行冲刺，打断当前状态");
    }

    /// <summary>
    /// 执行攻击动作
    /// </summary>
    private void PerformAttack()
    {
        if (attackManager == null)
        {
            LogManager.LogError($"[CharacterLogic] AttackManager 未设置!");
            return;
        }

        hasBufferedAttack = false;
        inputHandler.ConsumeInput(InputCommandType.Attack);

        // 根据当前状态选择攻击数据
        if (CurrentState == PlayerState.Dashing && attackManager.dashAttack != null)// 冲刺攻击优先级最高
        {
            currentAttackData = attackManager.dashAttack;
            ChangeState(PlayerState.DashAttacking);
            // 消耗冲刺攻击输入
            inputHandler.ConsumeInput(InputCommandType.DashAttack);
        }
        else if (!m_IsGrounded && attackManager.jumpAttack != null)//跳跃攻击
        {
            currentAttackData = attackManager.jumpAttack;
            ChangeState(PlayerState.JumpAttacking);
        }
        else if (CurrentState == PlayerState.Parrying && canParry)//弹反攻击
        {
            currentAttackData = attackManager.parryAttack;
            LogManager.Log("[CharacterLogic] 执行弹反攻击");
        }
        //else if (attackManager.specialAttack != null && inputHandler.MoveInput.y < -0.5f)//特殊攻击
        //{
        //    currentAttackData = attackManager.specialAttack;
        //    ChangeState(PlayerState.SpecialAttacking);
        //}
        else
        {
            // 普通攻击连招 - 使用新的动画参数系统
            var comboSequence = attackManager.GetComboSequence();
            if (comboSequence != null && comboSequence.Length > 0)
            {
                currentComboIndex = (currentComboIndex % comboSequence.Length) + 1;
                currentAttackData = comboSequence[currentComboIndex - 1];
                ChangeState(PlayerState.Attacking);
            }
            else
            {
                LogManager.LogError($"[CharacterLogic] 没有配置普通攻击序列!");
                return;
            }
        }

        // 重置攻击状态
        currentAttackPhase = AttackPhase.WindUp;
        currentAttackTimer = 0f;

        LogManager.Log($"[CharacterLogic] 执行攻击: {currentAttackData.acitonName}, 连招段数: {currentComboIndex}");

        // 使用新的动画参数系统触发动画
        if (animHandler != null)
        {
            animHandler.SetAttackAnimationParameter(currentAttackData);
            animHandler.SetAttackAnimationSpeed(currentAttackPhase, currentAttackData);
        }
    }

    #endregion

    #region 状态管理
    public void ChangeState(PlayerState newState)
    {
        if (CurrentState == newState) return;

        // 退出当前状态
        switch (CurrentState)
        {
            case PlayerState.Dashing:
                rb.linearVelocity = new Vector2(rb.linearVelocity.x * 0.5f, rb.linearVelocity.y);
                break;
            case PlayerState.Attacking:
            case PlayerState.HeavyAttacking:
            case PlayerState.DashAttacking:
            case PlayerState.JumpAttacking:
            case PlayerState.Parrying:
                // 退出攻击状态时重置攻击数据
                if (newState != PlayerState.Attacking &&
                    newState != PlayerState.HeavyAttacking &&
                    newState != PlayerState.DashAttacking &&
                    newState != PlayerState.JumpAttacking &&
                    newState != PlayerState.Parrying)
                {
                    BreakAttack();


                }
                break;
        }

        // 进入新状态
        switch (newState)
        {
            case PlayerState.Attacking:
            case PlayerState.HeavyAttacking:
            case PlayerState.DashAttacking:
            case PlayerState.JumpAttacking:
            case PlayerState.Parrying:
                lastAttackTime = Time.time;
                break;
        }

        PlayerState previousState = CurrentState;
        CurrentState = newState;
        OnStateChanged?.Invoke(previousState, newState);

        LogManager.Log($"[CharacterLogic] 状态切换: {previousState} -> {newState}");
    }

    private bool CanChangeState(PlayerState newState)
    {
        switch (CurrentState)
        {
            case PlayerState.Down:
            case PlayerState.GettingUp:
            case PlayerState.Stunned:
                return false;

            case PlayerState.Attacking:
            case PlayerState.HeavyAttacking:
            case PlayerState.DashAttacking:
            case PlayerState.JumpAttacking:
                // 这里可以添加更精确的动画帧检查
                return animHandler.CanInterruptCurrentAnimation();

            default:
                return true;
        }
    }

    /// <summary>
    /// 获取状态的优先级
    /// </summary>
    private int GetStatePriority(PlayerState state)
    {
        switch (state)
        {
            case PlayerState.Idle:
            case PlayerState.Running:
            case PlayerState.Jumping:
            case PlayerState.Falling:
            case PlayerState.Dashing:
                return IdlePriority;
            case PlayerState.Attacking:
            case PlayerState.HeavyAttacking:
            case PlayerState.DashAttacking:
            case PlayerState.JumpAttacking:
                return currentActionPriority = currentAttackData.priority;
            case PlayerState.Blocking:
                return BlockPriority; // 格挡优先级高于受击动画
            case PlayerState.Parrying:
                return ParryPriority; // 弹反优先级更高
            case PlayerState.Stunned:
                return StunPriority; // 硬直状态优先级最高
            default:
                return 0;
        }
    }

    #endregion

    #region 协程
    private IEnumerator EndDashAfterTime()
    {
        yield return new WaitForSeconds(dashDuration);
        if (CurrentState == PlayerState.Dashing)
        {
            // 使用统一的状态刷新函数
            RefreshState();
        }
    }

    /// <summary>
    /// 激活弹反输入窗口
    /// </summary>
    private void ActivateParryWindow()
    {
        isParryWindowActive = true;
        canParry = true;
        parryWindowStartTime = Time.time;

        LogManager.Log($"[CharacterLogic] 弹反窗口激活");

        // 设置定时器关闭弹反窗口
        StartCoroutine(DeactivateParryWindowAfterTime());
    }

    private IEnumerator DeactivateParryWindowAfterTime()
    {
        yield return new WaitForSeconds(parryWindow);

        if (isParryWindowActive)
        {
            isParryWindowActive = false;
            canParry = false;
            LogManager.Log($"[CharacterLogic] 弹反窗口关闭");
        }
    }

    #endregion

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    #region 公共方法
    public void OnAttackAnimationEnd()
    {
    }

    public void OnHurtAnimationEnd()
    {
        // 使用统一的状态刷新函数
        RefreshState();
    }

    #endregion

    private void OnDrawGizmosSelected()
    {
        // 设置Gizmos颜色
        Gizmos.color = m_IsGrounded ? Color.green : Color.red;

        // 计算检测区域的位置
        Vector2 checkPosition = (Vector2)transform.position + groundCheckOffset;

        // 绘制地面检测区域
        Gizmos.DrawWireCube(checkPosition, groundCheckSize);

        // 绘制检测区域的填充（半透明）
        Gizmos.color = m_IsGrounded ? new Color(0, 1, 0, 0.3f) : new Color(1, 0, 0, 0.3f);
        Gizmos.DrawCube(checkPosition, groundCheckSize);

        // 绘制从角色中心到检测区域的连线
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, checkPosition);

        // 在检测区域显示文本标签
        GUIStyle style = new GUIStyle();
        style.normal.textColor = m_IsGrounded ? Color.green : Color.red;
        UnityEditor.Handles.Label(checkPosition + Vector2.up * 0.2f,
            $"Ground Check\nSize: {groundCheckSize}\nGrounded: {m_IsGrounded}", style);
    }

    private void OnDrawGizmos()
    {
        // 非选中状态下也显示，但颜色更淡
        if (!UnityEditor.Selection.Contains(gameObject))
        {
            Gizmos.color = m_IsGrounded ? new Color(0, 1, 0, 0.1f) : new Color(1, 0, 0, 0.1f);

            Vector2 checkPosition = (Vector2)transform.position + groundCheckOffset;
            Gizmos.DrawWireCube(checkPosition, groundCheckSize);
        }
    }
}