using UnityEngine;

/// <summary>
/// 负责角色攻击生命周期：发起、阶段控制、命中检测、连招缓冲与结束。
/// 将攻击相关状态从 CharacterLogic 中剥离，提供简洁 API 给上层协调器调用。
/// </summary>
[DisallowMultipleComponent]
public class CharacterAttackController : MonoBehaviour
{
    // 发布攻击开始/结束事件（放在类成员区域）
    public System.Action<AttackActionData> OnAttackStarted;
    public System.Action OnAttackEnded;

    [Header("配置/引用")]
    public ActionManager actionManager;
    private InputHandler inputHandler;
    private CharacterAnimation animHandler;
    private AttackHitVisualizer attackVisualizer;
    private CharacterLogic characterLogic;

    // 防止 EndAttack 重入导致循环/递归触发
    private bool m_isEndingAttack = false;

    // 当前攻击状态（只允许外部读取）
    public AttackActionData currentAttackActionData { get; private set; }
    public AttackPhase currentAttackPhase { get; private set; } = AttackPhase.WindUp;
    public float currentAttackTimer { get; private set; }
    public int currentComboIndex { get; private set; }

    // 命中检测 id（用于 AttackHitDetector）
    private string currentAttackId;

    // 攻击缓冲标志（外部可读）
    public bool hasBufferedAttack { get; private set; }

    public void SetCharacterAnimation(CharacterAnimation characterAnimation)
    {
        animHandler = characterAnimation;
    }

    private void Awake()
    {
        characterLogic = GetComponent<CharacterLogic>();
        inputHandler = GetComponent<InputHandler>();
        attackVisualizer = GetComponent<AttackHitVisualizer>();

        if (actionManager == null)
            actionManager = characterLogic.actionManager;
    }

    private void Update()
    {
        // 攻击时更新攻击阶段计时逻辑
        if (IsAttacking())
        {
            UpdateAttackPhase(Time.deltaTime);
        }
    }

    // 外部调用判定是否处于攻击生命周期
    public bool IsAttacking()
    {
        return currentAttackActionData != null;
    }

    // 设置/消费缓冲输入
    public void BufferAttack()
    {
        hasBufferedAttack = true;
    }

    public void ConsumeBufferedAttack()
    {
        hasBufferedAttack = false;
    }

    /// <summary>
    /// 尝试发起攻击。外部负责先判断 CanAttack() 等条件。
    /// 参数由上层 CharacterLogic 提供（是否冲刺、是否在地面、是否处于弹反状态、是否允许弹反）。
    /// 面向方向使用 transform.localScale.x > 0 判断（保持与 CharacterLogic Flip 一致）。
    /// </summary>
    public void TryPerformAttack(bool isDashing, bool isGrounded, bool isParryState, bool canParry)
    {
        if (actionManager == null)
        {
            Debug.LogError("[CharacterAttackController] actionManager 未设置，无法执行攻击。");
            ConsumeBufferedAttack();
            return;
        }

        // 清除缓冲（调用者希望立即消费）
        ConsumeBufferedAttack();

        // 决定使用哪种攻击数据
        AttackActionData selected = null;

        if (isDashing && actionManager.dashAttack != null)
        {
            selected = actionManager.dashAttack;
        }
        else if (!isGrounded && actionManager.jumpAttack != null)
        {
            selected = actionManager.jumpAttack;
        }
        else if (isParryState && canParry && actionManager.parryAttack != null)
        {
            selected = actionManager.parryAttack;
        }
        else
        {
            var comboSequence = actionManager.GetComboSequence();
            if (comboSequence != null && comboSequence.Length > 0)
            {
                currentComboIndex = (currentComboIndex % comboSequence.Length) + 1;
                selected = comboSequence[currentComboIndex - 1];
            }
            else
            {
                Debug.LogError("[CharacterAttackController] 没有配置普通攻击序列!");
                return;
            }
        }

        BeginAttack(selected);
    }

    // 开始一次新的攻击生命周期
    private void BeginAttack(AttackActionData attackData)
    {
        if (attackData == null) return;

        currentAttackActionData = attackData;
        currentAttackPhase = AttackPhase.WindUp;
        currentAttackTimer = 0f;
        currentAttackId = null;

        // 触发动画参数（若有动画桥接）
        if (animHandler != null)
        {
            animHandler.SetActionAnimationParameter(currentAttackActionData);
            animHandler.SetAttackAnimationSpeed(currentAttackPhase, currentAttackActionData);
        }

        Debug.Log($"[CharacterAttackController] 开始攻击: {attackData.acitonName}");
        // 通知上层攻击开始
        OnAttackStarted?.Invoke(attackData);
    }

    // 攻击分段（前摇/命中/后摇）更新
    private void UpdateAttackPhase(float delta)
    {
        if (currentAttackActionData == null) return;

        currentAttackTimer += delta;

        switch (currentAttackPhase)
        {
            case AttackPhase.WindUp:
                if (currentAttackTimer >= currentAttackActionData.windUpTime)
                    EnterActivePhase();
                break;

            case AttackPhase.Active:
                // 每帧进行命中检测（依赖 AttackHitDetector）
                if (AttackHitDetector.Instance != null && !string.IsNullOrEmpty(currentAttackId))
                {
                    bool facingRight = transform.localScale.x > 0f;
                    AttackHitDetector.Instance.CheckHitForFrame(
                        currentAttackId,
                        currentAttackActionData,
                        transform.position,
                        facingRight,
                        currentAttackTimer,
                        gameObject);
                }

                if (currentAttackActionData != null && currentAttackTimer >= currentAttackActionData.windUpTime + currentAttackActionData.activeTime)
                    EnterRecoveryPhase();
                break;

            case AttackPhase.Recovery:
                if (currentAttackTimer >= currentAttackActionData.TotalDuration)
                {
                    EndAttack();
                }
                else
                {
                    // 恢复阶段：上层可调用 TryConsumeCombo() 触发连招
                }
                break;
        }
    }

    private void EnterActivePhase()
    {
        currentAttackPhase = AttackPhase.Active;

        // 启动命中检测（AttackHitDetector 会返回检测 id）
        if (AttackHitDetector.Instance != null && currentAttackActionData != null)
        {
            bool facingRight = transform.localScale.x > 0f;
            currentAttackId = AttackHitDetector.Instance.StartAttackDetection(
                currentAttackActionData,
                transform.position,
                facingRight,
                gameObject);
        }

        if (animHandler != null)
            animHandler.SetAttackAnimationSpeed(currentAttackPhase, currentAttackActionData);

        Debug.Log("[CharacterAttackController] 进入攻击激活阶段");
    }

    private void EnterRecoveryPhase()
    {
        currentAttackPhase = AttackPhase.Recovery;

        if (animHandler != null)
            animHandler.SetAttackAnimationSpeed(currentAttackPhase, currentAttackActionData);

        Debug.Log("[CharacterAttackController] 进入后摇阶段");
    }

    /// <summary>
    /// 结束当前攻击（外部也可直接调用，用于打断）
    /// </summary>
    public void EndAttack()
    {
        if (currentAttackActionData == null || m_isEndingAttack) return;

        m_isEndingAttack = true;
        Debug.Log("[CharacterAttackController] 攻击结束");

        // 通知上层攻击结束
        OnAttackEnded?.Invoke();

        // 允许下一次结束调用
        m_isEndingAttack = false;

        // 结束命中检测
        if (!string.IsNullOrEmpty(currentAttackId) && AttackHitDetector.Instance != null)
        {
            AttackHitDetector.Instance.EndAttackDetection(currentAttackId);
            currentAttackId = null;
        }


        // 清理可视化
        if (attackVisualizer != null)
        {
            attackVisualizer.ClearFrameData();
        }

        // 重置状态
        currentAttackActionData = null;
        currentAttackPhase = AttackPhase.WindUp;
        currentAttackTimer = 0f;
        currentComboIndex = 0;
        ConsumeBufferedAttack();
    }

    /// <summary>
    /// 在后摇或动画结束处由上层调用，尝试消费缓冲并发起连招。
    /// </summary>
    //public void TryConsumeCombo(bool isDashing, bool isGrounded, bool isParryState, bool canParry)
    //{
    //    if (!hasBufferedAttack || currentAttackActionData == null) return;

    //    if (currentAttackActionData.canCombo && currentAttackTimer >= currentAttackActionData.ComboStartTime)
    //    {
    //        // 清除缓冲并由上层提供的当前状态决定下一段攻击类型
    //        ConsumeBufferedAttack();
    //        TryPerformAttack(isDashing, isGrounded, isParryState, canParry);
    //    }
    //}

    // 公开方法，允许外部直接以指定的 AttackActionData 开始一次攻击
    public void StartAttack(AttackActionData attackData)
    {
        if (attackData == null) return;

        // 清除缓冲（调用者希望立即开始）
        ConsumeBufferedAttack();

        // 内部开始攻击（BeginAttack 已在同类中实现）
        BeginAttack(attackData);
    }
}