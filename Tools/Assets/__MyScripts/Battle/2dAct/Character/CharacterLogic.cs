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
    Stunned,         // 硬直
    Hurt,         // 受伤
    Death,         // 死亡
}


public enum AttackPhase
{
    WindUp,     // 前摇阶段
    Active,     // 攻击中阶段,只有在攻击阶段才进行命中攻击帧检测
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



    [Header("角色切换")]
    [Tooltip("角色数据库")]
    public CharacterDatabase characterDatabase;
    private GameObject characterInstance;

    // 添加强力攻击相关变量
    private bool isHeavyAttackCharging = false;
    private float heavyAttackChargeStartTime = 0f;


    // 攻击系统相关变量
    [Header("攻击调试信息")]
    [FieldReadOnly]
    public ActionManager actionManager;
    [SerializeField]
    private CharacterAttackController attackController;
    [FieldReadOnly]
    public ActionData currentActionData;//当前行为数据
    public AttackActionData currentAttackActionData => attackController != null ? attackController.currentAttackActionData : null;//当前攻击数据

    public AttackPhase currentAttackPhase => attackController != null ? attackController.currentAttackPhase : AttackPhase.WindUp;
    public float currentAttackTimer => attackController != null ? attackController.currentAttackTimer : 0f;
    public int currentComboIndex => attackController != null ? attackController.currentComboIndex : 0;




    [FieldReadOnly]
    public bool isFacingRight = true;
    [FieldReadOnly]
    public bool hasBufferedAttack; // 是否有缓冲的攻击输入

    // 组件引用
    private Rigidbody2D rb;
    private InputHandler inputHandler;
    private CharacterAnimation animHandler;

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
    private Coroutine m_RecoverFromHurt_Coroutine;
    public const int StunPriority = 300; // 硬直优先级最高
    public const int BlockPriority = 200; // 格挡优先级
    public const int ParryPriority = 250; // 弹反优先级
    public const int DashPriority = 150; // 冲刺优先级
    public const int HurtPriority = 100; // 受伤优先级
    public const int IdlePriority = 0; // 待机优先级



    public InputHandler InputHandler
    {
        get { return inputHandler; }
    }
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

    [Header("生命属性")]
    public int maxHealth = 100;
    [FieldReadOnly]
    public int currentHealth = 100;
    private int currentCharacterIndex;

    public bool IsDead => CurrentState == PlayerState.Death || currentHealth <= 0;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        inputHandler = GetComponent<InputHandler>();
        attackController = GetComponent<CharacterAttackController>();
        currentCharacterIndex = 0;
        LoadCharacterPrefab(currentCharacterIndex);

        // 初始化生命值
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        if (currentHealth == 0)
            currentHealth = maxHealth;

        // 初始化当前行为数据（优先使用 ActionManager 中的 idleAction）
        currentActionData = actionManager != null ? (ActionData)actionManager.idleAction : null;
    }




    private void OnEnable()
    {
        // 订阅输入事件
        if (inputHandler)
        {
            inputHandler.OnMoveInput += HandleMoveInput;
            inputHandler.OnJumpInput += HandleJumpInput;
            inputHandler.OnJumpCanceledAction += HandleJumpCanceled;
            inputHandler.OnDashInput += HandleDashInput;
            inputHandler.OnAttackStarted += HandleAttackStarted;
            inputHandler.OnBlockStartedAction += HandleBlockStarted;
            inputHandler.OnBlockCanceledAction += HandleBlockCanceled;
            inputHandler.OnHeavyAttackStarted += HandleHeavyAttackStarted;
            inputHandler.OnAssistAttack += HandleAssistAttack;
        }
        OnHurt += OnHurtHandler;

        if (attackController != null)
        {
            attackController.OnAttackStarted += HandleAttackControllerStarted;
            attackController.OnAttackEnded += HandleAttackControllerEnded;
        }
    }


    private void OnDisable()
    {
        // 订阅输入事件
        if (inputHandler)
        {
            inputHandler.OnMoveInput -= HandleMoveInput;
            inputHandler.OnJumpInput -= HandleJumpInput;
            inputHandler.OnJumpCanceledAction -= HandleJumpCanceled;
            inputHandler.OnDashInput -= HandleDashInput;
            inputHandler.OnAttackStarted -= HandleAttackStarted;
            inputHandler.OnBlockStartedAction -= HandleBlockStarted;
            inputHandler.OnBlockCanceledAction -= HandleBlockCanceled;
            inputHandler.OnHeavyAttackStarted -= HandleHeavyAttackStarted;
            inputHandler.OnAssistAttack -= HandleAssistAttack;
        }
        OnHurt -= OnHurtHandler;
        if (attackController != null)
        {
            attackController.OnAttackStarted -= HandleAttackControllerStarted;
            attackController.OnAttackEnded -= HandleAttackControllerEnded;
        }
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
    }


    private void HandleAssistAttack()
    {
        if (characterDatabase != null && characterDatabase.characters != null && characterDatabase.characters.Count > 1)
        {
            if (characterInstance != null)//销毁旧角色
            {
                Destroy(characterInstance);
            }
        }
        //循环切换角色
        currentCharacterIndex = (currentCharacterIndex + 1) % characterDatabase.GetCharacterCount();
        LoadCharacterPrefab(currentCharacterIndex);
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
        if (IsDead) return; // 已死亡则忽略受伤事件

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

        // 先应用伤害
        bool died = TakeDamage(frameData != null ? frameData.damage : 0, attacker);

        if (died)
        {
            // 如果死亡，立即处理死亡流程并返回（不会继续执行受击或眩晕）
            HandleDeath(attacker);
            return;
        }

        // 检查是否造成眩晕
        if (frameData != null && frameData.causeStun)
        {
            ApplyStun(frameData.stunDuration);
        }
        else
        {
            // 根据攻击优先级决定受击表现
            // 切换到硬直状态
            bool shouldPlayHitAnimation = ShouldPlayHitAnimation(attackData != null ? attackData.priority : 0);
            animHandler?.OnHurt(shouldPlayHitAnimation);
            if (shouldPlayHitAnimation)//只有播放受击动画时才切换状态
            {
                ChangeState(PlayerState.Hurt);
                //根据受伤时间,切换回之前的状态
                if (m_RecoverFromHurt_Coroutine != null)
                {
                    StopCoroutine(m_RecoverFromHurt_Coroutine);
                }
                m_RecoverFromHurt_Coroutine = StartCoroutine(RecoverFromHurt(actionManager.hurtAction.animationClipLength)); // 假设受伤后0.2秒恢复
            }
        }


        // 应用击退
        if (rb != null && attacker != null && frameData != null)
        {
            Vector2 knockbackDirection = (transform.position - attacker.transform.position).normalized;
            rb.AddForce(knockbackDirection * frameData.knockbackForce, ForceMode2D.Impulse);
        }

        // 播放命中特效
        if (frameData != null && frameData.hitEffect != null)
        {
            Instantiate(frameData.hitEffect, transform.position, Quaternion.identity);
        }
    }

    void LoadCharacterPrefab(int index)
    {
        if (characterDatabase != null && characterDatabase.characters != null && characterDatabase.characters.Count > 0)
        {
            var data = characterDatabase.GetCharacterPrefab(index);
            if (data != null)
            {
                characterInstance = Instantiate(data.characterPrefab, transform.position, transform.rotation, transform);
                animHandler = characterInstance.GetComponent<CharacterAnimation>();
                if (animHandler != null)
                {
                    animHandler.SetCharacterLogic(this, rb);
                }

                if (attackController != null)
                {
                    attackController.SetCharacterAnimation(animHandler);
                    attackController.actionManager = data.actionManager;
                }
                ApplyActionManager(data.actionManager);
            }
            else
            {
                LogManager.LogError("[CharacterLogic] 无法加载角色预制体: 数据库中未找到对应预制体");
            }
        }
    }

    /// <summary>
    /// 运行时切换 ActionManager（用于角色切换）
    /// </summary>
    /// <param name="newActionManager">目标 ActionManager 资产</param>
    /// <param name="refreshStateImmediately">是否立刻刷新状态</param>
    public void ApplyActionManager(ActionManager newActionManager, bool refreshStateImmediately = true)
    {
        if (newActionManager == null)
        {
            LogManager.LogError("[CharacterLogic] 无法应用空的 ActionManager");
            return;
        }

        actionManager = newActionManager;

        // 切换时打断所有攻击/格挡/弹反状态
        BreakAttack();
        hasBufferedAttack = false;
        SetIsBlockState(false);
        isParryWindowActive = false;
        canParry = false;
        isHeavyAttackCharging = false;
        currentActionPriority = IdlePriority;
        currentActionData = actionManager != null ? (ActionData)actionManager.idleAction : null;

        if (refreshStateImmediately)
        {
            ForceRefreshState();
        }

        LogManager.Log($"[CharacterLogic] 已应用新的 ActionManager: {newActionManager.name}");
    }


    IEnumerator RecoverFromHurt(float duration)
    {
        yield return new WaitForSeconds(duration);
        m_RecoverFromHurt_Coroutine = null;
        if (CurrentState == PlayerState.Hurt)
        {
            RefreshState();
        }
    }

    private bool ShouldPlayHitAnimation(int priority)
    {
        return priority > HurtPriority && currentActionPriority < priority;//只有当攻击优先级高于硬直优先级且当前动作优先级低于攻击优先级时才播放受击动画
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
        if (CurrentState == PlayerState.Death)
        {
            return false;
        }
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
                return currentAttackActionData == null;

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
        if (inputHandler && Mathf.Abs(inputHandler.MoveInput.x) > 0.1f)
            return PlayerState.Running;
        else
            return PlayerState.Idle;
    }

    private void CheckGround()
    {
        Vector2 checkPosition = (Vector2)transform.position + actionManager.jumpAction.groundCheckOffset;
        bool wasGrounded = m_IsGrounded;
        m_IsGrounded = Physics2D.OverlapBox(checkPosition, actionManager.jumpAction.groundCheckSize, 0f, actionManager.jumpAction.groundLayer);

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
        else if (!m_IsGrounded && wasGrounded)// 离开地面时
        {
            lastGroundedTime = Time.time;
            OnLeaveGround();
            // 离开地面时刷新状态
            RefreshState();
        }
    }

    /// <summary>
    /// 离开地面时
    /// </summary>
    private void OnLeaveGround()
    {

    }
    /// <summary>
    /// 落地时
    /// </summary>
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
        if (inputHandler != null && Mathf.Abs(inputHandler.MoveInput.x) > 0.1f)
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
        rb.linearVelocity = new Vector2(dashDirection * actionManager.dashAction.dashSpeed, rb.linearVelocity.y);
    }

    private void HandleBlockState()
    {
        float moveInputX = 0f;
        if (inputHandler)
        {
            moveInputX = inputHandler.MoveInput.x;
        }
        rb.linearVelocity = new Vector2(moveInputX * actionManager.moveAction.moveSpeed * 0.3f, rb.linearVelocity.y);
    }

    private void HandleAttackState()
    {
        if (CurrentState == PlayerState.JumpAttacking)
        {
            // 跳跃攻击时使用空中移动逻辑
            HandleAirMovement();
            //HandleJumpPhysics(); // 保持跳跃物理效果
        }
        else if (currentAttackActionData != null && currentAttackActionData.enableMovement && currentAttackPhase == AttackPhase.Active)
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

    // 处理攻击子系统开始的回调（放在类的私有方法区域）
    private void HandleAttackControllerStarted(AttackActionData attackData)
    {
        if (attackData == null) return;

        // 根据所选攻击数据设置对应的状态（保持与原语义一致）
        if (actionManager != null && attackData == actionManager.heavyAttack)
        {
            ChangeState(PlayerState.HeavyAttacking);
        }
        else if (actionManager != null && attackData == actionManager.dashAttack)
        {
            ChangeState(PlayerState.DashAttacking);
        }
        else if (actionManager != null && attackData == actionManager.jumpAttack)
        {
            ChangeState(PlayerState.JumpAttacking);
        }
        else if (actionManager != null && attackData == actionManager.parryAttack)
        {
            ChangeState(PlayerState.Parrying);
        }
        else
        {
            ChangeState(PlayerState.Attacking);
        }

        // 与原逻辑保持一致，让 currentActionData 指向当前攻击数据
        currentActionData = attackData != null ? (ActionData)attackData : null;

        // 记录最近一次攻击时间
        lastAttackTime = Time.time;
    }

    // 处理攻击子系统结束的回调
    private void HandleAttackControllerEnded()
    {
        // 如果刚结束的是弹反攻击，走弹反结束路径；否则刷新状态
        if (CurrentState == PlayerState.Parrying)
        {
            OnParryAttackEnd();
        }
        else
        {
            // 攻击结束回归合适状态
            RefreshState();
        }
    }


    /// <summary>
    /// 弹反成功（由动画事件调用）
    /// </summary>
    public void OnParryAttackEnd()
    {
        LogManager.Log($"[CharacterLogic] 弹反成功!");

        // 切换到攻击状态或返回格挡状态
        if (inputHandler && inputHandler.IsBlockPressed)
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
        // 由攻击子系统处理打断和清理
        attackController?.EndAttack();

        // 保持上层状态一致性
        currentActionData = actionManager != null ? (ActionData)actionManager.idleAction : null;
    }

    private void HandleAttackMovement()
    {
        if (currentAttackActionData == null || !currentAttackActionData.enableMovement) return;

        float phaseProgress = (currentAttackTimer - currentAttackActionData.windUpTime) / currentAttackActionData.activeTime;
        float movementFactor = currentAttackActionData.movementCurve.Evaluate(phaseProgress);

        float movementDirection = isFacingRight ? 1f : -1f;
        Vector2 movement = new Vector2(movementDirection * currentAttackActionData.movementSpeed * movementFactor, 0f);

        rb.linearVelocity = new Vector2(movement.x, rb.linearVelocity.y);
    }

    private void CheckComboInput()
    {
        if (hasBufferedAttack && currentAttackActionData != null && currentAttackActionData.canCombo && currentAttackTimer >= currentAttackActionData.ComboStartTime)
        {
            LogManager.Log($"[CharacterLogic] 后摇阶段检测到连招输入，执行连招");
            PerformComboAttack();
            hasBufferedAttack = false;
        }
    }

    private void PerformComboAttack()
    {
        // 连招不检查 CanAttack()，因为已经在攻击状态中
        if (currentAttackActionData != null && currentAttackActionData.canCombo)
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
            // 攻击中则缓冲，由攻击子系统管理缓冲
            attackController?.BufferAttack();
        }
        else if (CanAttack())
        {
            // 由攻击子系统选择并开始合适的攻击类型
            attackController?.TryPerformAttack(
                CurrentState == PlayerState.Dashing,
                m_IsGrounded,
                CurrentState == PlayerState.Parrying,
                canParry);
            // 消耗基础攻击输入
            inputHandler?.ConsumeInput(InputCommandType.Attack);
        }
        else
        {
            LogManager.Log($"[CharacterLogic] 当前无法攻击，缓冲攻击输入");
            attackController?.BufferAttack();
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

    // 替换现有的 PerformHeavyAttack 实现为委托到 attackController（带回退逻辑）
    private void PerformHeavyAttack()
    {
        if (actionManager == null || actionManager.heavyAttack == null)
        {
            LogManager.LogError($"[CharacterLogic] 强力攻击未配置!");
            isHeavyAttackCharging = false;
            heavyAttackChargeStartTime = 0f;
            return;
        }

        LogManager.Log($"[CharacterLogic] 执行强力攻击");

        // 优先使用攻击子系统
        if (attackController != null)
        {
            // 让攻击子系统启动指定的强力攻击数据
            attackController.StartAttack(actionManager.heavyAttack);

            // 重置充能标志
            heavyAttackChargeStartTime = 0f;
            isHeavyAttackCharging = false;

            return;
        }

    }


    #endregion

    #region 预输入检查
    private void CheckBufferedInputs()
    {
        // 只检查攻击预输入
        if (CanAttack() && inputHandler != null && inputHandler.TryGetBufferedInput(InputCommandType.Attack, out var attackCmd))
        {
            LogManager.Log($"[CharacterLogic] 从缓冲区获取到攻击输入: {attackCmd}");
            PerformAttack();
        }
    }
    #endregion

    #region 条件检查方法
    private bool CanJump()
    {
        bool coyoteTimeValid = Time.time - lastGroundedTime <= actionManager.jumpAction.coyoteTime;
        return (m_IsGrounded || coyoteTimeValid) && CanInterruptForJump();
    }

    private bool CanDash()
    {
        return Time.time - lastDashTime >= actionManager.dashAction.dashCooldown && CanInterruptForDash();
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
                return currentAttackActionData == null || currentAttackActionData.canCancel;

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
                return currentAttackActionData == null || currentAttackActionData.canCancel;

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
                return actionManager != null && actionManager.jumpAttack != null;

            case PlayerState.Attacking:
            case PlayerState.HeavyAttacking:
            case PlayerState.DashAttacking:
            case PlayerState.JumpAttacking:
                // 在后摇阶段且允许连招时可以中断
                if (currentAttackPhase == AttackPhase.Recovery &&
                    currentAttackActionData != null &&
                    currentAttackActionData.canCombo)
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

    private void ProcessAttackBuffer()
    {
        // 如果攻击子系统有缓冲并且当前允许发起攻击，则请求攻击子系统发起攻击
        if (attackController != null && attackController.hasBufferedAttack)
        {
            if (CanAttack())
            {
                attackController.TryPerformAttack(
                    CurrentState == PlayerState.Dashing,
                    m_IsGrounded,
                    CurrentState == PlayerState.Parrying,
                    canParry);

                // 清理 InputHandler 的缓冲（如原来）
                inputHandler?.ConsumeInput(InputCommandType.Attack);
            }
        }
    }

    #endregion

    #region 动作执行
    private void HandleMovement(Vector2 input)
    {
        float targetSpeed = input.x * actionManager.moveAction.moveSpeed;
        float speedDiff = targetSpeed - rb.linearVelocity.x;
        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? actionManager.moveAction.acceleration : actionManager.moveAction.deceleration;
        float movement = Mathf.Pow(Mathf.Abs(speedDiff) * accelRate, 0.9f) * Mathf.Sign(speedDiff);

        rb.AddForce(movement * Vector2.right);
    }

    private void HandleAirMovement()
    {
        float moveInputX = 0f;
        if (inputHandler)
        {
            moveInputX = inputHandler.MoveInput.x;
        }

        float targetSpeed = moveInputX * actionManager.moveAction.moveSpeed * 0.8f;
        float speedDiff = targetSpeed - rb.linearVelocity.x;
        float movement = speedDiff * actionManager.moveAction.acceleration * 0.5f;
        rb.AddForce(movement * Vector2.right);
    }

    private void HandleJumpPhysics()
    {
        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (actionManager.jumpAction.fallMultiplier - 1) * Time.deltaTime;
        }
        else if (rb.linearVelocity.y > 0 && inputHandler && !inputHandler.IsBlockPressed)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (actionManager.jumpAction.lowJumpMultiplier - 1) * Time.deltaTime;
        }
    }

    private void PerformJump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, actionManager.jumpAction.jumpForce);
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
        // 旧实现已迁移到 attackController，保留兼容调用路径
        if (attackController == null)
        {
            LogManager.LogError($"[CharacterLogic] attackController 未挂载!");
            return;
        }

        // 让攻击子系统选择并开始攻击
        attackController.TryPerformAttack(
            CurrentState == PlayerState.Dashing,
            m_IsGrounded,
            CurrentState == PlayerState.Parrying,
            canParry);

        // 同步 currentActionData 指向攻击子系统（保持原先外部读取习惯）
        currentActionData = currentAttackActionData != null ? (ActionData)currentAttackActionData : null;

        // 消耗主攻击输入
        inputHandler?.ConsumeInput(InputCommandType.Attack);

        LogManager.Log($"[CharacterLogic] 委托执行攻击: {currentAttackActionData?.acitonName}， 连招段数: {currentComboIndex}");
    }

    #endregion

    #region 状态管理
    public void ChangeState(PlayerState newState)
    {
        if (CurrentState == newState) return;

        // 如果已经死亡，禁止任何状态切换（除非需要在死亡时做特殊处理）
        if (CurrentState == PlayerState.Death)
            return;

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

        // 进入新状态 — 设置 currentActionData 为对应 ActionManager 中的行为（若存在）
        switch (newState)
        {
            case PlayerState.Attacking:
            case PlayerState.HeavyAttacking:
            case PlayerState.DashAttacking:
            case PlayerState.JumpAttacking:
            case PlayerState.Parrying:
                lastAttackTime = Time.time;
                // 攻击类状态使用 currentAttackActionData（若有）
                //currentActionData = currentAttackActionData != null ? (ActionData)currentAttackActionData : null;
                break;

            case PlayerState.Idle:
                currentActionData = actionManager != null ? (ActionData)actionManager.idleAction : null;
                break;

            case PlayerState.Running:
                currentActionData = actionManager != null ? (ActionData)actionManager.moveAction : null;
                break;

            case PlayerState.Dashing:
                currentActionData = actionManager != null ? (ActionData)actionManager.dashAction : null;
                break;

            case PlayerState.Jumping:
                currentActionData = actionManager != null ? (ActionData)actionManager.jumpAction : null;
                break;

            case PlayerState.Falling:
                currentActionData = actionManager != null ? actionManager.fallAction : null;
                break;

            case PlayerState.Blocking:
                currentActionData = actionManager != null ? (ActionData)actionManager.blockAction : null;
                break;

            case PlayerState.Hurt:
                currentActionData = actionManager != null ? actionManager.hurtAction : null;
                break;

            case PlayerState.Death:
                currentActionData = actionManager != null ? actionManager.deathAction : null;
                break;

            default:
                currentActionData = actionManager != null ? (ActionData)actionManager.idleAction : null;
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
                return currentActionPriority = currentAttackActionData.priority;
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
        yield return new WaitForSeconds(actionManager.dashAction.dashDuration);
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
        yield return new WaitForSeconds(actionManager.blockAction.parryWindow);

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


    private void OnDrawGizmosSelected()
    {
        // 设置Gizmos颜色
        Gizmos.color = m_IsGrounded ? Color.green : Color.red;

        if (actionManager != null && actionManager.jumpAction != null)
        {
            // 计算检测区域的位置
            Vector2 checkPosition = (Vector2)transform.position + actionManager.jumpAction.groundCheckOffset;

            // 绘制地面检测区域
            Gizmos.DrawWireCube(checkPosition, actionManager.jumpAction.groundCheckSize);

            // 绘制检测区域的填充（半透明）
            Gizmos.color = m_IsGrounded ? new Color(0, 1, 0, 0.3f) : new Color(1, 0, 0, 0.3f);
            Gizmos.DrawCube(checkPosition, actionManager.jumpAction.groundCheckSize);

            // 绘制从角色中心到检测区域的连线
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, checkPosition);

            // 在检测区域显示文本标签
            GUIStyle style = new GUIStyle();
            style.normal.textColor = m_IsGrounded ? Color.green : Color.red;
            UnityEditor.Handles.Label(checkPosition + Vector2.up * 0.2f,
                $"Ground Check\nSize: {actionManager.jumpAction.groundCheckSize}\nGrounded: {m_IsGrounded}", style);
        }


    }

    private void OnDrawGizmos()
    {
        // 非选中状态下也显示，但颜色更淡
        if (!UnityEditor.Selection.Contains(gameObject))
        {
            Gizmos.color = m_IsGrounded ? new Color(0, 1, 0, 0.1f) : new Color(1, 0, 0, 0.1f);
            if (actionManager != null && actionManager.jumpAction != null)
            {
                Vector2 checkPosition = (Vector2)transform.position + actionManager.jumpAction.groundCheckOffset;
                Gizmos.DrawWireCube(checkPosition, actionManager.jumpAction.groundCheckSize);
            }

        }
    }


    #region 生命与死亡实现

    /// <summary>
    /// 扣血，返回是否死亡
    /// </summary>
    public bool TakeDamage(int damage, GameObject attacker = null)
    {
        if (IsDead) return true;

        int prevHp = currentHealth;
        currentHealth = Mathf.Max(0, currentHealth - damage);
        LogManager.Log($"[CharacterLogic] 受伤: {damage} HP {prevHp} -> {currentHealth}");

        return currentHealth <= 0;
    }

    /// <summary>
    /// 回复生命
    /// </summary>
    public void Heal(int amount)
    {
        if (IsDead) return;
        int prev = currentHealth;
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        LogManager.Log($"[CharacterLogic] 回复: {amount} HP {prev} -> {currentHealth}");
    }

    /// <summary>
    /// 处理死亡流程
    /// </summary>
    private void HandleDeath(GameObject killer = null)
    {
        if (IsDead && CurrentState == PlayerState.Death) return;

        LogManager.Log($"[CharacterLogic] 死亡: 被 {(killer != null ? killer.name : "未知")} 击杀");

        // 停止正在进行的伤害恢复协程
        if (m_RecoverFromHurt_Coroutine != null)
        {
            StopCoroutine(m_RecoverFromHurt_Coroutine);
            m_RecoverFromHurt_Coroutine = null;
        }

        // 结束攻击，清理检测
        BreakAttack();

        // 设置为死亡状态，禁止后续状态切换
        currentActionPriority = int.MaxValue / 2;
        ChangeState(PlayerState.Death);
        attackController.EndAttack();

        // 禁用输入并停止物理运动
        if (inputHandler != null)
            inputHandler.enabled = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.simulated = false;
        }

        // 广播死亡事件（动画会监听并播放死亡动画）
        OnDeath?.Invoke();
    }

    #endregion
}