/*
角色控制
状态管理
攻击逻辑
 */
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static CharacterDatabase;

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



public class CharacterLogic : CharacterBase
{
    private const int JUMP_PROTECTION_FRAME_COUNT = 3; // 跳跃保护3帧

    private int EnemyLayer;
    private int PlayerLayer;

    // 逻辑事件
    public System.Action<PlayerState, PlayerState> OnStateChanged;
    public System.Action<int> OnAttackCombo;
    public System.Action OnJump;
    public System.Action OnLandAction;
    public System.Action<AttackActionData, AttackFrameData, CharacterBase> OnHurt;
    public System.Action OnDeath;
    // 添加格挡成功事件
    public System.Action OnBlockSuccess; // 格挡成功
    public System.Action<CharacterBase> OnParrySuccess; // 弹反成功（参数为被弹反的敌人）
    //硬直事件
    public System.Action OnStunned;

    // 多段跳跃相关
    [Header("多段跳跃状态")]
    [FieldReadOnly]
    [Tooltip("当前剩余空中跳跃次数")]
    public int currentAirJumpsRemaining = 0;

    [FieldReadOnly]
    [Tooltip("额外的跳跃次数（通过Buff获得）")]
    public int extraJumpCount = 0;



    [Header("逻辑状态")]
    public PlayerState CurrentState = PlayerState.Idle;

    [Header("角色切换")]
    [Tooltip("角色数据库")]
    public CharacterDatabase characterDatabase;


    public SkillCooldownManager SkillCooldownManager;

    private GameObject characterInstance;
    private readonly Dictionary<string, GameObject> characterInstanceCache = new Dictionary<string, GameObject>();

    // 添加强力攻击相关变量
    private bool isHeavyAttackCharging = false;
    private float heavyAttackChargeStartTime = 0f;


    // 攻击系统相关变量
    [Header("攻击调试信息")]
    private CharacterAttackController attackController;
    public CharacterAttackController AttackController => attackController;
    [FieldReadOnly]
    public ActionManager actionManager;
    [FieldReadOnly]
    public ActionData currentActionData;//当前行为数据
    public AttackActionData currentAttackActionData => attackController != null ? attackController.currentAttackActionData : null;//当前攻击数据

    public AttackPhase currentAttackPhase => attackController != null ? attackController.currentAttackPhase : AttackPhase.WindUp;
    public float currentAttackTimer => attackController != null ? attackController.currentAttackTimer : 0f;
    public int currentComboIndex => attackController != null ? attackController.currentComboIndex : 0;



    // 组件引用

    private InputHandler inputHandler;
    private CharacterAnimation animHandler;

    private int jumpProtectionFrames = 0;

    // 逻辑状态
    private bool m_IsGrounded;
    private float lastGroundedTime;
    private float lastDashTime;

    // 格挡状态
    /// <summary>
    /// 格挡中
    /// </summary>
    private bool isBlocking = false;
    private bool isParryWindowActive = false;
    private float parryWindowStartTime = 0f;
    private bool canParry = false; // 是否可以触发弹反

    private Coroutine m_RecoverFromHurt_Coroutine;
    private float stunTimer = 0f;


    private string currentCharacterName;
    private string specialAttackOriginalCharacterName;
    private bool isSpecialAttackActive = false;
    private Coroutine dashCoroutine;

    public InputHandler InputHandler
    {
        get { return inputHandler; }
    }
    public bool IsGrounded
    {
        get { return m_IsGrounded; }
    }

    public bool IsStunned
    {
        get { return CurrentState == PlayerState.Stunned; }
    }


    public override bool IsDead => CurrentState == PlayerState.Death || (PlayerAttributes != null && PlayerAttributes.IsDead);


    protected override void Awake()
    {
        base.Awake();

        EnemyLayer = LayerMask.NameToLayer("Enemy");
        PlayerLayer = LayerMask.NameToLayer("Player");

        inputHandler = GetComponent<InputHandler>();
        attackController = GetComponent<CharacterAttackController>();


        InitializeCharacter();

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
            inputHandler.OnInputAttackAction += HandleInputAttackAction;
            inputHandler.OnAssistAttack += HandleAssistAttack;
            inputHandler.OnSpecialAttack += HandleSpecialAttack;
            inputHandler.OnSpecialAttack2 += HandleSpecialAttack2;
            inputHandler.OnAttackCanceled += OnInputAttackCanceled;
        }

        if (attackController != null)
        {
            attackController.OnAttackStarted += HandleAttackControllerStarted;
            attackController.OnAttackEnded += HandleAttackControllerEnded;
        }
        if (PlayerAttributes)
        {
            PlayerAttributes.characterAtttibute.OnDeath += HandleDeath;
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
            inputHandler.OnInputAttackAction -= HandleInputAttackAction;
            inputHandler.OnAssistAttack -= HandleAssistAttack;
            inputHandler.OnSpecialAttack -= HandleSpecialAttack;
            inputHandler.OnSpecialAttack2 -= HandleSpecialAttack2;
            inputHandler.OnAttackCanceled -= OnInputAttackCanceled;
        }
        if (attackController != null)
        {
            attackController.OnAttackStarted -= HandleAttackControllerStarted;
            attackController.OnAttackEnded -= HandleAttackControllerEnded;
        }
        if (PlayerAttributes)
        {
            PlayerAttributes.characterAtttibute.OnDeath -= HandleDeath;
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
        HandleDownJumpInput();
    }

    //实现下+跳跃输入,执行跳下平台
    private void HandleDownJumpInput()
    {
        if ((CurrentState == PlayerState.Idle || CurrentState == PlayerState.Blocking) && inputHandler != null && inputHandler.MoveInput.y < -0.1f && inputHandler.IsJumpPressed)
        {
            //实现跳下功能
            // 检测脚下是否是一-way平台
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 10f, actionManager.jumpAction.groundLayer);
            if (hit.collider != null && hit.collider.TryGetComponent<PlatformEffector2D>(out _))
            {
                // 临时忽略与平台的碰撞
                //rb.AddForce(Vector2.down * 5f, ForceMode2D.Impulse); // 向下施加一个小力，帮助角色穿过平台
                StartCoroutine(DisableCollisionTemporarily(hit.collider));
            }
        }
    }

    private IEnumerator DisableCollisionTemporarily(Collider2D platformCollider)
    {
        Physics2D.IgnoreCollision(boxCollider2D, platformCollider, true);
        Vector3 halfHeight = new Vector3(0, boxCollider2D.size.y / 2, 0);
        RaycastHit2D hit = Physics2D.Raycast(transform.position + halfHeight, Vector2.down, 10f, actionManager.jumpAction.groundLayer);
        while (hit.collider != null && hit.collider.TryGetComponent<PlatformEffector2D>(out _))//todo:往下跳的时候,持续检测,直到不在平台上为止,这边可能比较费性能,后续可以优化
        {
            hit = Physics2D.Raycast(transform.position + halfHeight, Vector2.down, 10f, actionManager.jumpAction.groundLayer);
            yield return null;
            yield return null;//等两帧再检测
        }
        Physics2D.IgnoreCollision(boxCollider2D, platformCollider, false);
    }

    /// <summary>
    /// 特殊攻击：根据 specialAttack 绑定的 ActionData 临时加载对应角色执行攻击
    /// </summary>
    private void HandleSpecialAttack()
    {
        var specialAttackData = actionManager != null ? actionManager.specialAttack : null;
        ExecuteSpecialAttack(specialAttackData, "特殊攻击");
    }

    private void HandleSpecialAttack2()
    {
        var specialAttackData = actionManager != null ? actionManager.specialAttack2 : null;
        ExecuteSpecialAttack(specialAttackData, "特殊攻击2");
    }

    private void ExecuteSpecialAttack(AttackActionData specialAttackData, string label)
    {
        if (specialAttackData == null)
        {
            LogManager.Log($"[CharacterLogic] 当前角色未配置{label}，执行普通攻击逻辑");
            HandleAttackStarted();
            return;
        }

        if (characterDatabase == null)
        {
            LogManager.LogError($"[CharacterLogic] 未配置角色数据库，无法执行{label}");
            HandleAttackStarted();
            return;
        }

        if (isSpecialAttackActive)
        {
            LogManager.Log($"[CharacterLogic] {label}未结束，忽略新的输入");
            return;
        }

        if (CanAttack() == false)//过滤无法攻击的状态
        {
            return;
        }


        if (SkillCooldownManager != null && SkillCooldownManager.IsOnCooldown(specialAttackData))
        {
            float remaining = SkillCooldownManager.GetRemainingCooldown(specialAttackData);
            LogManager.Log($"[CharacterLogic] {label}冷却中，剩余时间: {remaining:F2}秒");
            return;
        }

        var specialAttackToExecute = specialAttackData;

        specialAttackOriginalCharacterName = currentCharacterName;

        string targetCharacterName = specialAttackData.character != null ? specialAttackData.character.name : null;
        if (string.IsNullOrEmpty(targetCharacterName) || !LoadCharacterPrefab(targetCharacterName))
        {
            LogManager.LogError($"[CharacterLogic] {label}角色加载失败，执行普通攻击");
            specialAttackOriginalCharacterName = null;
            HandleAttackStarted();
            return;
        }

        isSpecialAttackActive = true;

        if (attackController != null && specialAttackToExecute != null)
        {
            attackController.StartAttack(specialAttackToExecute);
        }
    }
    /// <summary>
    /// 角色切换
    /// </summary>
    private void HandleAssistAttack()
    {
        if (!HasAlternateCharacter())
        {
            LogManager.Log("[CharacterLogic] 无可切换的协助角色");
            return;
        }

        if (isSpecialAttackActive)
        {
            LogManager.Log("[CharacterLogic] 特殊攻击执行中，暂不允许切换角色");
            return;
        }

        if (IsStunned)
        {
            LogManager.Log("[CharacterLogic] 眩晕中，暂不允许切换角色");
            return;
        }
        //todo:后面加一个切换角色是否cd中


        string nextCharacterName = GetNextCharacterName();
        TrySwitchCharacter(nextCharacterName);
    }

    private bool HasAlternateCharacter()
    {
        return characterDatabase != null &&
               characterDatabase.characters != null &&
               characterDatabase.characters.Count > 1;
    }

    private string GetNextCharacterName()
    {
        if (characterDatabase == null || characterDatabase.characters == null || characterDatabase.characters.Count == 0)
        {
            return currentCharacterName;
        }

        if (!HasAlternateCharacter())
        {
            return currentCharacterName;
        }

        int currentIndex = GetCurrentCharacterIndex();
        if (currentIndex < 0)
        {
            currentIndex = 0;
        }

        int nextIndex = (currentIndex + 1) % characterDatabase.GetCharacterCount();
        return characterDatabase.characters[nextIndex].characterName;
    }

    private int GetCurrentCharacterIndex()
    {
        if (characterDatabase == null || characterDatabase.characters == null || characterDatabase.characters.Count == 0)
        {
            return -1;
        }

        if (string.IsNullOrEmpty(currentCharacterName))
        {
            return -1;
        }

        return characterDatabase.characters.FindIndex(c => c.characterName == currentCharacterName);
    }

    private bool TrySwitchCharacter(string targetCharacterName)
    {
        if (characterDatabase == null || characterDatabase.characters == null || characterDatabase.characters.Count == 0)
        {
            LogManager.LogError("[CharacterLogic] 未配置角色数据库，无法切换角色");
            return false;
        }

        if (string.IsNullOrEmpty(targetCharacterName))
        {
            targetCharacterName = characterDatabase.characters[0].characterName;
        }

        if (string.Equals(targetCharacterName, currentCharacterName) && characterInstance != null)
        {
            return true;
        }

        if (!LoadCharacterPrefab(targetCharacterName))
        {
            LogManager.LogError($"[CharacterLogic] 无法切换到角色: {targetCharacterName}");
            return false;
        }

        return true;
    }

    private void RestoreSpecialAttackCharacter()
    {
        if (!isSpecialAttackActive)
            return;

        if (!string.IsNullOrEmpty(specialAttackOriginalCharacterName) &&
            !string.Equals(specialAttackOriginalCharacterName, currentCharacterName))
        {
            if (!TrySwitchCharacter(specialAttackOriginalCharacterName))
            {
                LogManager.LogError("[CharacterLogic] 恢复特殊攻击前的角色失败");
            }
        }

        specialAttackOriginalCharacterName = null;
        isSpecialAttackActive = false;
    }

    private void UpdateTimers()
    {
        if (CurrentState == PlayerState.Jumping && rb.linearVelocity.y < 0)
        {
            ChangeState(PlayerState.Falling);
        }

        if (CurrentState == PlayerState.Stunned && stunTimer > 0)
        {
            stunTimer -= Time.deltaTime;
            if (stunTimer <= 0)
            {
                RecoverFromStun();
            }
        }
    }


    /// <summary>
    /// 旧版受伤处理器（用于兼容旧代码）
    /// </summary>
    private void OnHurtHandler(AttackActionData attackData, AttackFrameData frameData, CharacterBase attacker)
    {
        if (IsDead) return; // 已死亡则忽略受伤事件


        // 根据攻击优先级决定受击表现
        // 切换到硬直状态
        bool shouldPlayHitAnimation = ShouldPlayHitAnimation(attackData != null ? attackData.priority : 0);
        animHandler?.OnHurt(shouldPlayHitAnimation);
        if (shouldPlayHitAnimation)//只有播放受击动画时才切换状态
        {
            ChangeState(PlayerState.Hurt);//todo:打断攻击时,如何中断伤害?
            //根据受伤时间,切换回之前的状态
            if (m_RecoverFromHurt_Coroutine != null)
            {
                StopCoroutine(m_RecoverFromHurt_Coroutine);
            }
            m_RecoverFromHurt_Coroutine = StartCoroutine(RecoverFromHurt(actionManager.hurtAction.animationClipLength)); // 假设受伤后0.2秒恢复
        }

        // 应用击退：技能基础击退力 + 攻击帧附加击退力
        if (rb != null && attacker != null && frameData != null)
        {
            Vector2 knockbackDirection = (transform.position - attacker.transform.position).normalized;
            Vector2 finalKnockbackForce = frameData.knockbackForce;

            // 如果是攻击行为数据，叠加技能的基础击退力
            AttackActionData attackActionData = attackData as AttackActionData;
            if (attackActionData != null && attackActionData.skillData != null)
            {
                finalKnockbackForce += attackActionData.skillData.knockbackForce;
            }

            rb.AddForce(knockbackDirection * finalKnockbackForce, ForceMode2D.Impulse);
        }

        // 播放命中特效
        if (frameData != null && frameData.hitEffect != null)
        {
            Instantiate(frameData.hitEffect, transform.position, Quaternion.identity);
        }

        OnHurt?.Invoke(attackData, frameData, attacker);
    }

    public void AddForce(Vector2 force)
    {
        if (rb)
        {
            rb.AddForce(force, ForceMode2D.Impulse);
        }
    }

    private void InitializeCharacter()
    {
        if (characterDatabase == null || characterDatabase.characters == null || characterDatabase.characters.Count == 0)
        {
            LogManager.LogError("[CharacterLogic] 角色数据库为空，无法初始化角色");
            return;
        }

        var initialEntry = characterDatabase.characters[0];
        if (!LoadCharacterPrefab(initialEntry))
        {
            LogManager.LogError("[CharacterLogic] 初始化角色失败");
        }
    }

    private bool LoadCharacterPrefab(int index)
    {
        var data = characterDatabase != null ? characterDatabase.GetCharacterPrefab(index) : null;
        return LoadCharacterPrefab(data);
    }

    private bool LoadCharacterPrefab(string characterName)
    {
        if (characterDatabase == null || string.IsNullOrEmpty(characterName))
        {
            return false;
        }

        var entry = characterDatabase.characters?.Find(c => c.characterName == characterName);
        return LoadCharacterPrefab(entry);
    }

    private bool LoadCharacterPrefab(CharacterEntry data)
    {
        if (characterDatabase == null || characterDatabase.characters == null || characterDatabase.characters.Count == 0)
        {
            LogManager.LogError("[CharacterLogic] 角色数据库为空，无法加载角色");
            return false;
        }

        if (data == null)
        {
            LogManager.LogError("[CharacterLogic] 无法加载角色预制体: 数据库中未找到对应预制体");
            return false;
        }

        string characterKey = data.characterName;
        if (string.IsNullOrEmpty(characterKey))
        {
            LogManager.LogError("[CharacterLogic] 角色名称为空，无法缓存实例");
            return false;
        }

        if (characterInstance != null && !string.IsNullOrEmpty(currentCharacterName))
        {
            CacheCurrentCharacterInstance(currentCharacterName, characterInstance);
            characterInstance.SetActive(false);
        }

        GameObject nextInstance = GetOrCreateCharacterInstance(characterKey, data);
        if (nextInstance == null)
        {
            LogManager.LogError("[CharacterLogic] 角色缓存实例无效");
            return false;
        }

        characterInstance = nextInstance;
        characterInstance.transform.localPosition = data.posOffset;
        characterInstance.SetActive(true);
        currentCharacterName = characterKey;

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
        return true;
    }

    private void CacheCurrentCharacterInstance(string characterKey, GameObject instance)
    {
        if (instance == null || string.IsNullOrEmpty(characterKey)) return;

        characterInstanceCache[characterKey] = instance;
    }

    private GameObject GetOrCreateCharacterInstance(string characterKey, CharacterEntry data)
    {
        if (!string.IsNullOrEmpty(characterKey) && characterInstanceCache.TryGetValue(characterKey, out var cachedInstance) && cachedInstance != null)
        {
            return cachedInstance;
        }

        var instance = Instantiate(data.characterPrefab, transform.position + data.posOffset, transform.rotation, transform);
        if (!string.IsNullOrEmpty(characterKey))
        {
            characterInstanceCache[characterKey] = instance;
        }
        return instance;
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

        // 切换时打断所有攻击/格挡/弹反/输入缓存状态
        BreakAttack();
        attackController?.ConsumeBufferedAttack();

        SetIsBlockState(false);
        isParryWindowActive = false;
        canParry = false;
        isHeavyAttackCharging = false;
        currentActionData = actionManager != null ? (ActionData)actionManager.idleAction : null;

        if (refreshStateImmediately)
        {
            ForceRefreshState();
        }

        LogManager.Log($"[CharacterLogic] 已应用新的 ActionManager: {newActionManager.name}");
    }


    IEnumerator RecoverFromHurt(float duration)
    {
        yield return WaitForSecondsCache.WaitForSeconds(duration);
        m_RecoverFromHurt_Coroutine = null;
        if (CurrentState == PlayerState.Hurt)
        {
            RefreshState();
        }
    }

    private bool ShouldPlayHitAnimation(int priority)
    {
        return DamageCalculator.ShouldPlayHitAnimation(priority, PlayerAttributes.characterAtttibute);//非霸体时才播放受击动画
    }

    /// <summary>
    /// 处理格挡成功
    /// </summary>
    private void HandleBlockSuccess(ActionData attackData, AttackFrameData frameData, CharacterBase attacker)
    {
        LogManager.Log($"[CharacterLogic] 格挡成功! 来自 {attacker.name} 的攻击");

        // 触发格挡成功事件
        OnBlockSuccess?.Invoke();

        // 轻微击退效果：(技能基础击退力 + 攻击帧附加击退力) * 0.3
        if (rb != null && frameData != null)
        {
            Vector2 knockbackDirection = (transform.position - attacker.transform.position).normalized;
            Vector2 finalKnockbackForce = frameData.knockbackForce;

            // 如果是攻击行为数据，叠加技能的基础击退力
            AttackActionData attackActionData = attackData as AttackActionData;
            if (attackActionData != null && attackActionData.skillData != null)
            {
                finalKnockbackForce += attackActionData.skillData.knockbackForce;
            }

            rb.AddForce(knockbackDirection * finalKnockbackForce * 0.3f, ForceMode2D.Impulse);
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

        OnParrySuccess?.Invoke(null);//todo:若需要给订阅者（UI/效果）知道被反弹的敌人，应该在触发时携带该攻击来源（需要在触发点保存 attacker）
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
        // 落地时重置空中跳跃次数
        ResetAirJumps();

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
        float finalDashSpeed = GetFinalDashSpeed();
        float dashDirection = isFacingRight ? 1f : -1f;
        rb.linearVelocity = new Vector2(dashDirection * finalDashSpeed, rb.linearVelocity.y);
    }

    private void HandleBlockState()
    {
        float moveInputX = 0f;
        if (inputHandler)
        {
            moveInputX = inputHandler.MoveInput.x;
        }

        float finalMoveSpeed = GetFinalMoveSpeed();
        rb.linearVelocity = new Vector2(moveInputX * finalMoveSpeed * 0.3f, rb.linearVelocity.y);
    }

    private void HandleAttackState()
    {
        if (CurrentState == PlayerState.JumpAttacking)
        {
            // 跳跃攻击时使用空中移动逻辑
            HandleAirMovement();
            HandleAttackMovement();//使用攻击移动逻辑
        }
        else if (currentAttackActionData != null && currentAttackActionData.enableMovement && currentAttackPhase == AttackPhase.Active)
        {
            // 攻击状态下可以轻微移动
            HandleAttackMovement();
        }
        else
        {
            // 攻击时限制移动
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
        else if (actionManager != null && (attackData == actionManager.jumpAttack || attackData == actionManager.downAttack))//普通跳跃攻击和下落攻击都是属于跳跃攻击
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
    }

    // 处理攻击子系统结束的回调
    private void HandleAttackControllerEnded(bool isBreak)
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

        if (isSpecialAttackActive)
        {
            RestoreSpecialAttackCharacter();
        }
    }


    /// <summary>
    /// 弹反成功（由动画事件调用）
    /// </summary>
    public void OnParryAttackEnd()
    {
        LogManager.Log($"[CharacterLogic] 弹反成功!");

        // 切换到攻击状态或返回格挡状态(这个体验不好,弹反后进入待机状态)
        //if (inputHandler && inputHandler.IsBlockPressed)
        //{
        //    ChangeState(PlayerState.Blocking);
        //}
        //else
        {
            RefreshState();
        }
    }

    /// <summary>
    /// 施加硬直效果
    /// 由BuffSystem或攻击系统调用，使角色进入硬直状态
    /// 硬直状态下角色完全无法行动，优先级最高
    /// 支持刷新眩晕时间：如果角色已经处于眩晕状态，会更新眩晕时间为新的持续时间
    /// </summary>
    /// <param name="duration">硬直持续时间（秒）</param>
    public override void ApplyStun(float duration)
    {
        base.ApplyStun(duration);
        if (CurrentState == PlayerState.Stunned)
        {
            stunTimer = duration;
            LogManager.Log($"[CharacterLogic] 刷新硬直时间: {duration}秒");
            return;
        }

        stunTimer = duration;

        LogManager.Log($"[CharacterLogic] 施加硬直时间: {duration}秒");

        ChangeState(PlayerState.Stunned);

        OnStunned?.Invoke();
    }

    /// <summary>
    /// 从硬直状态恢复
    /// 由UpdateTimers调用，当眩晕计时器归零时自动恢复到正常状态
    /// </summary>
    private void RecoverFromStun()
    {
        if (CurrentState == PlayerState.Stunned)
        {
            LogManager.Log($"[CharacterLogic] 从硬直状态恢复");
            RefreshState();
        }
    }

    /// <summary>
    /// 攻击被打断时调用函数
    /// </summary>
    private void BreakAttack()
    {
        // 由攻击子系统处理打断和清理
        attackController?.EndAttack(true);

        // 保持上层状态一致性
        currentActionData = actionManager != null ? (ActionData)actionManager.idleAction : null;
    }

    private void HandleAttackMovement()
    {
        if (currentAttackActionData == null || !currentAttackActionData.enableMovement) return;

        if (currentAttackActionData.activeTime <= 0)
        {
            LogManager.LogWarning("[CharacterLogic] 当前攻击动作数据的 activeTime 小于等于0，无法计算攻击移动");
            return;
        }
        float phaseProgress = (currentAttackTimer - currentAttackActionData.windUpTime) / currentAttackActionData.activeTime;
        float movementFactor = currentAttackActionData.movementCurve.Evaluate(phaseProgress);

        float movementDirection = isFacingRight ? 1f : -1f;
        Vector2 movement = new Vector2(movementDirection * currentAttackActionData.movementSpeed.x * movementFactor, currentAttackActionData.movementSpeed.y);
        if (currentAttackActionData.IsAccumulateForce)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x + movement.x, rb.linearVelocity.y + movement.y);//攻击的时候附带攻击移动
        }
        else
        {
            rb.linearVelocity = new Vector2(movement.x, movement.y);//攻击的时候附带攻击移动
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
        // 攻击过程中不允许改变朝向，避免攻击动作被输入打断
        //格挡中不允许改变朝向
        //眩晕状态不允许改变朝向
        if (!IsAttacking() && !isBlocking && !IsStunned)
        {
            if (input.x > 0 && !isFacingRight)
                Flip();
            else if (input.x < 0 && isFacingRight)
                Flip();
        }
    }

    private void HandleJumpInput()
    {
        // 跳跃优先级最高，可以打断除不可打断状态外的所有状态
        if (CanJump())
        {
            // 如果当前正在冲刺，先结束冲刺
            if (CurrentState == PlayerState.Dashing)
            {
                if (dashCoroutine != null)
                {
                    StopCoroutine(dashCoroutine);
                    dashCoroutine = null;
                }

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
        if (CanDash())
        {
            if (CurrentState == PlayerState.Jumping || CurrentState == PlayerState.Falling)
            {
                ChangeState(PlayerState.Dashing);
            }

            PerformDash();
        }

    }
    /// <summary>
    /// 玩家攻击输入处理
    /// </summary>
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
            // 攻击时取消格挡状态
            if (isBlocking)
            {
                SetIsBlockState(false);
                isParryWindowActive = false;
                canParry = false;
                LogManager.Log($"[CharacterLogic] 攻击时取消格挡状态");
            }

            // 检查是否在跳跃状态且按下了下键（下+攻击键组合）
            bool isInAir = CurrentState == PlayerState.Jumping || CurrentState == PlayerState.Falling;
            bool isDownPressed = inputHandler != null && inputHandler.MoveInput.y < -0.1f;

            if (isInAir && isDownPressed && actionManager != null && actionManager.downAttack != null)
            {
                // 执行下落攻击
                LogManager.Log($"[CharacterLogic] 检测到跳跃状态下+攻击键组合，执行下落攻击");
                attackController?.StartAttack(actionManager.downAttack);
                inputHandler?.ConsumeInput(InputCommandType.Attack);
            }
            else
            {
                // 检查是否按下了上键（上+攻击键组合）
                bool isUpPressed = inputHandler != null && inputHandler.MoveInput.y > 0.1f;

                if (isUpPressed && actionManager != null && actionManager.upAttack != null)
                {
                    // 执行上攻击
                    LogManager.Log($"[CharacterLogic] 检测到上+攻击键组合，执行上攻击");
                    attackController?.StartAttack(actionManager.upAttack);
                    inputHandler?.ConsumeInput(InputCommandType.Attack);
                }
                else
                {
                    // 由攻击子系统选择并开始合适的攻击类型（普通攻击）
                    attackController?.TryPerformAttack(
                        CurrentState == PlayerState.Dashing,
                        m_IsGrounded,
                        CurrentState == PlayerState.Parrying,
                        canParry);
                    // 消耗基础攻击输入
                    inputHandler?.ConsumeInput(InputCommandType.Attack);
                }
            }
        }
        else
        {
            LogManager.Log($"[CharacterLogic] 当前无法攻击，缓冲攻击输入");
            attackController?.BufferAttack();
        }
    }


    /// <summary>
    /// 松开攻击输入时调用
    /// </summary>
    private void OnInputAttackCanceled()
    {
        // 重置充能标志
        heavyAttackChargeStartTime = 0f;
        isHeavyAttackCharging = false;
    }


    /// <summary>
    /// 执行攻击动作
    /// </summary>
    private void PerformAttack()
    {
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
        PlayerAttributes.characterAtttibute.isBlocking = state;
    }
    /// <summary>
    /// 普攻按住相应处理
    /// </summary>
    /// <param name="pressTime"></param>
    private void HandleInputAttackAction(float pressTime)
    {
        // 强力攻击检测（基于当前 actionManager 中配置的 heavyAttack 阈值）
        if (!isHeavyAttackCharging && actionManager != null && actionManager.heavyAttack != null && inputHandler.IsAttackPressed)
        {
            if (pressTime >= actionManager.heavyAttack.longPressTimeThreshold)
            {
                // 将重攻击加入缓冲并通知订阅者
                //inputHandler?.ConsumeInput(InputCommandType.Attack);

                if (currentAttackActionData != null && (currentAttackActionData.windUpTime + currentAttackActionData.activeTime) > actionManager.heavyAttack.longPressTimeThreshold)
                {
                    LogManager.LogWarning($"[CharacterLogic] 警告: 当前攻击动作的预备时间{(currentAttackActionData.windUpTime + currentAttackActionData.activeTime)},大于重攻击按键阈值{actionManager.heavyAttack.longPressTimeThreshold}，可能导致普通攻击无法正确触发");
                }
                LogManager.Log($"[CharacterLogic] 触发重攻击");
                HandleHeavyAttackStarted();
            }
        }
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

    /// <summary>
    /// 重强攻击
    /// </summary>
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

    /// <summary>
    /// 获取当前总的最大空中跳跃次数（基础 + 额外）
    /// </summary>
    private int GetTotalMaxAirJumps()
    {
        int baseJumps = actionManager != null && actionManager.jumpAction != null
            ? actionManager.jumpAction.baseMaxAirJumps
            : 0;
        return baseJumps + extraJumpCount;
    }

    /// <summary>
    /// 重置空中跳跃次数
    /// 在落地时调用
    /// </summary>
    private void ResetAirJumps()
    {
        currentAirJumpsRemaining = GetTotalMaxAirJumps();
        LogManager.Log($"[CharacterLogic] 重置空中跳跃次数: {currentAirJumpsRemaining}");
    }

    /// <summary>
    /// 消耗一次空中跳跃机会
    /// </summary>
    private void ConsumeAirJump()
    {
        if (currentAirJumpsRemaining > 0)
        {
            currentAirJumpsRemaining--;
            LogManager.Log($"[CharacterLogic] 消耗一次空中跳跃，剩余: {currentAirJumpsRemaining}");
        }
    }

    /// <summary>
    /// 增加额外跳跃次数（由Buff系统调用）
    /// </summary>
    public void AddExtraJumps(int count)
    {
        extraJumpCount += count;
        currentAirJumpsRemaining += count;
        LogManager.Log($"[CharacterLogic] 增加额外跳跃次数 +{count}，总额外跳跃: {extraJumpCount}，当前剩余: {currentAirJumpsRemaining}");
    }

    /// <summary>
    /// 移除额外跳跃次数（由Buff系统调用）
    /// </summary>
    public void RemoveExtraJumps(int count)
    {
        extraJumpCount = Mathf.Max(0, extraJumpCount - count);
        currentAirJumpsRemaining = Mathf.Max(0, currentAirJumpsRemaining - count);
        LogManager.Log($"[CharacterLogic] 移除额外跳跃次数 -{count}，总额外跳跃: {extraJumpCount}，当前剩余: {currentAirJumpsRemaining}");
    }

    private bool CanJump()
    {
        bool coyoteTimeValid = Time.time - lastGroundedTime <= actionManager.jumpAction.coyoteTime;
        bool isGroundedOrCoyote = m_IsGrounded || coyoteTimeValid;
        bool isDownPressed = inputHandler.MoveInput.y < 0;

        // 在地面或土狼时间内可以跳跃
        if (isGroundedOrCoyote && !isDownPressed && CanInterruptForJump())
        {
            return true;
        }

        // 空中且有剩余跳跃次数可以跳跃
        if (!m_IsGrounded && currentAirJumpsRemaining > 0 && !isDownPressed && CanInterruptForJump())
        {
            return true;
        }

        return false;
    }

    private bool CanDash()
    {
        if (Time.time - lastDashTime < actionManager.dashAction.dashCooldown)
        {
            return false;
        }

        if (!CanInterruptForDash())
        {
            return false;
        }

        if (PlayerAttributes != null && PlayerAttributes.characterAtttibute != null)
        {
            if (actionManager.dashAction.consumeDodgeCount)
            {
                return PlayerAttributes.characterAtttibute.currentDodgeCount > 0;
            }
        }

        return true;
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
            case PlayerState.Blocking://格挡状态可以打断,直接攻击
                return true;
            default:
                return true;
        }
    }
    /// <summary>
    /// 是否能够发起冲刺攻击
    /// </summary>
    /// <returns></returns>
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


    public bool IsAttacking()
    {
        return CurrentState == PlayerState.Attacking ||
               CurrentState == PlayerState.HeavyAttacking ||
               CurrentState == PlayerState.DashAttacking ||
               CurrentState == PlayerState.JumpAttacking ||
               CurrentState == PlayerState.Parrying ||//弹反也算攻击状态
               CurrentState == PlayerState.SpecialAttacking ||
               CurrentState == PlayerState.AssistAttacking;
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

                // 清理 InputHandler 的缓冲
                inputHandler?.ConsumeInput(InputCommandType.Attack);
            }
        }
    }

    #endregion

    #region 动作执行
    private void HandleMovement(Vector2 input)
    {
        float finalMoveSpeed = GetFinalMoveSpeed();
        float targetSpeed = input.x * finalMoveSpeed;
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

        float finalAirMoveSpeed = GetFinalAirMoveSpeed();
        float targetSpeed = moveInputX * finalAirMoveSpeed;
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
        bool isAirJump = !m_IsGrounded;

        // 计算跳跃力度
        float jumpPower = actionManager.jumpAction.jumpForce;
        if (isAirJump && actionManager.jumpAction.baseMaxAirJumps > 0)
        {
            // 空中跳跃使用力度系数
            jumpPower *= actionManager.jumpAction.airJumpForceMultiplier;
        }

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpPower);
        ChangeState(PlayerState.Jumping);

        // 如果是空中跳跃，消耗一次跳跃次数
        if (isAirJump)
        {
            ConsumeAirJump();
        }

        OnJump?.Invoke();

        // 设置跳跃保护帧
        jumpProtectionFrames = JUMP_PROTECTION_FRAME_COUNT;

        LogManager.Log($"[CharacterLogic] 执行{(isAirJump ? "空中" : "地面")}跳跃，打断当前状态");
    }

    private void PerformDash()
    {
        ChangeState(PlayerState.Dashing);
        lastDashTime = Time.time;

        if (inputHandler != null)
        {
            inputHandler.OnDashStarted();
        }

        if (actionManager.dashAction.consumeDodgeCount)
        {
            if (PlayerAttributes.characterAtttibute.ConsumeDodge())
            {
                PlayerAttributes.characterAtttibute.isDodging = true;
                LogManager.Log($"[CharacterLogic] 闪避消耗成功，剩余次数: {PlayerAttributes.characterAtttibute.currentDodgeCount}/{PlayerAttributes.characterAtttibute.maxDodgeCount}");
            }
        }

        // 冲刺时忽略角色层和Enemy层之间的碰撞
        Physics2D.IgnoreLayerCollision(PlayerLayer, EnemyLayer, true);

        // 保存协程引用以便后续 Stop
        if (dashCoroutine != null)
        {
            StopCoroutine(dashCoroutine);
            dashCoroutine = null;
        }

        dashCoroutine = StartCoroutine(EndDashAfterTime());
        StartCoroutine(HandleDodgeInvincibility());


        LogManager.Log($"[CharacterLogic] 执行冲刺，打断当前状态");
    }

    /// <summary>
    /// 获取最终移动速度（考虑属性加成）
    /// </summary>
    private float GetFinalMoveSpeed()
    {
        if (PlayerAttributes == null || PlayerAttributes.characterAtttibute == null)
        {
            return actionManager != null ? actionManager.moveAction.moveSpeed : 5f;
        }

        float baseSpeed = actionManager != null ? actionManager.moveAction.moveSpeed : 5f;
        float attributeSpeed = PlayerAttributes.characterAtttibute.FinalMoveSpeed;
        float multiplier = PlayerAttributes.characterAtttibute.FinalMoveSpeedMultiplier;

        // 计算最终速度：基础速度 + 属性速度加成，再乘以系数
        return (baseSpeed + attributeSpeed) * multiplier;
    }

    /// <summary>
    /// 获取空中移动速度（考虑属性加成）
    /// </summary>
    private float GetFinalAirMoveSpeed()
    {
        return GetFinalMoveSpeed() * 0.8f; // 空中移动速度为地面的80%
    }

    /// <summary>
    /// 获取冲刺速度（考虑属性加成）
    /// </summary>
    private float GetFinalDashSpeed()
    {
        if (PlayerAttributes == null || PlayerAttributes.characterAtttibute == null)
        {
            return actionManager != null ? actionManager.dashAction.dashSpeed : 10f;
        }

        float baseDashSpeed = actionManager != null ? actionManager.dashAction.dashSpeed : 10f;
        float attributeSpeed = PlayerAttributes.characterAtttibute.FinalMoveSpeed;
        float multiplier = PlayerAttributes.characterAtttibute.FinalMoveSpeedMultiplier;

        // 冲刺速度也受移动属性影响
        return (baseDashSpeed + attributeSpeed * 0.5f) * multiplier;//todo:这边是伤害计算公式,后面考虑优化
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
                if (PlayerAttributes != null && PlayerAttributes.characterAtttibute != null)
                {
                    PlayerAttributes.characterAtttibute.isDodging = false;
                }
                Physics2D.IgnoreLayerCollision(PlayerLayer, EnemyLayer, false);
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
                // 攻击类状态使用 currentAttackActionData
                currentActionData = currentAttackActionData != null ? (ActionData)currentAttackActionData : null;
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

            case PlayerState.Falling:
            case PlayerState.Jumping:
                if (newState == PlayerState.Jumping)//跳跃或者下落的时候可以切换到跳跃
                {
                    return true;
                }
                return false;//跳跃的时候不能切换到格挡,否则下落攻击会变成格挡

            case PlayerState.Attacking:
            case PlayerState.HeavyAttacking:
            case PlayerState.DashAttacking:
            case PlayerState.JumpAttacking:
                return animHandler.CanInterruptCurrentAnimation();

            default:
                return true;
        }
    }


    #endregion

    #region 协程
    private IEnumerator EndDashAfterTime()
    {
        yield return WaitForSecondsCache.WaitForSeconds(actionManager.dashAction.dashDuration);
        if (CurrentState == PlayerState.Dashing)
        {
            RefreshState();
        }
    }

    private IEnumerator HandleDodgeInvincibility()
    {
        if (PlayerAttributes == null || PlayerAttributes.characterAtttibute == null || actionManager == null)
        {
            yield break;
        }

        yield return WaitForSecondsCache.WaitForSeconds(actionManager.dashAction.invincibleStartTime);

        if (PlayerAttributes.characterAtttibute.isDodging)
        {
            PlayerAttributes.characterAtttibute.AddInvincibility();
            LogManager.Log($"[CharacterLogic] 闪避无敌开始 (计数: {PlayerAttributes.characterAtttibute.isInvincible})");

            yield return WaitForSecondsCache.WaitForSeconds(actionManager.dashAction.invincibleDuration);

            PlayerAttributes.characterAtttibute.RemoveInvincibility();
            LogManager.Log($"[CharacterLogic] 闪避无敌结束 (计数: {PlayerAttributes.characterAtttibute.isInvincible})");
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
        yield return WaitForSecondsCache.WaitForSeconds(actionManager.blockAction.parryWindow);

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
#if UNITY_EDITOR
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
#endif

    #region 生命与死亡实现

    public override void TakeDamage(DamageInfo damageInfo, AttackActionData attackActionData, AttackFrameData frameData, CharacterBase attacker)
    {
        base.TakeDamage(damageInfo, attackActionData, frameData, attacker);
        if (IsDead || PlayerAttributes.characterAtttibute.IsInvincible()) return;


        // 检查是否在格挡状态,并且在弹反窗口内
        if (isBlocking)//格挡状态•	TryParry()
        {
            LogManager.Log($"当前在格挡状态");
            if (CanBlockAttack(attacker.transform.position))//检测格挡攻击方向
            {
                HandleBlockSuccess(attackActionData, frameData, attacker);
                LogManager.Log($"格挡成功");
            }
        }

        ApplyDamageWithCalculation(damageInfo, attackActionData, frameData, attacker);
    }



    /// <summary>
    /// 应用带属性计算的伤害（攻击者是CharacterLogic时使用）
    /// </summary>
    private void ApplyDamageWithCalculation(DamageInfo damageInfo, AttackActionData attackActionData, AttackFrameData frameData, CharacterBase attacker)
    {
        if (PlayerAttributes == null)
        {
            LogManager.LogWarning($"[CharacterLogic] 目标没有PlayerAttributes组件");
            return;
        }

        var attackerAttributes = damageInfo.attacker.PlayerAttributes.characterAtttibute;
        var targetAttributes = PlayerAttributes.characterAtttibute;
        var attackerBuffSystem = damageInfo.attacker.BuffSystem;
        var targetBuffSystem = BuffSystem;

        if (attackerAttributes == null)
        {
            return;
        }

        DamageResult result = DamageCalculator.CalculateDamage(damageInfo, attacker, this);
        DamageDisplayHelper.ShowDamageOnCharacter(result, transform);
        if (result.isMiss)
        {
            LogManager.Log("[CharacterLogic] 攻击未命中（闪避/无敌）");
            return;
        }

        if (!result.isBlocked && result.healthDamage > 0)
        {
            targetAttributes.ChangeHealth(-result.healthDamage, damageInfo.attacker);
            LogManager.Log($"[CharacterLogic] 造成伤害: {result.healthDamage}{(result.isCritical ? " (暴击!)" : "")}");

            bool died = targetAttributes.currentHealth <= 0;
            if (died)
            {
                //HandleDeath(attacker);//由ChangeHealth中调用死亡处理
                return;
            }
        }

        OnHurtHandler(attackActionData, frameData, attacker);
    }


    /// <summary>
    /// 处理死亡流程
    /// </summary>
    private void HandleDeath(CharacterBase killer = null)
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
        ChangeState(PlayerState.Death);
        attackController.EndAttack(true);

        // 禁用输入并停止物理运动
        if (inputHandler != null)
            inputHandler.enabled = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.simulated = false;
        }

        //清除buff
        if (BuffSystem != null)
        {
            BuffSystem.ClearAllBuffs();
        }

        // 广播死亡事件（动画会监听并播放死亡动画）
        OnDeath?.Invoke();
    }

    #endregion
}