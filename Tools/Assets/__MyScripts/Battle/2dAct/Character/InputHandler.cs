using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// 输入命令类型
public enum InputCommandType
{
    Jump,
    Dash,
    Attack,
    Block,
    AssistAttack,//协助攻击
    SpecialAttack,//特殊攻击
    SpecialAttack2,//特殊攻击2
    HeavyAttack,      // 强力攻击
    DashAttack        // 冲刺攻击
}

// 输入命令结构
public struct InputCommand
{
    public InputCommandType Type;
    public float Time;

    public InputCommand(InputCommandType type, float time)
    {
        Type = type;
        Time = time;
    }
}
[DisallowMultipleComponent]

public class InputHandler : MonoBehaviour
{
    // 输入事件
    public System.Action<Vector2> OnMoveInput;
    public System.Action OnJumpInput;
    public System.Action OnJumpCanceledAction;
    public System.Action OnDashInput;
    public System.Action OnAttackStarted;
    public System.Action OnAttackPerformed;
    public System.Action OnAttackCanceled;
    public System.Action OnBlockStartedAction;
    public System.Action OnBlockCanceledAction;
    public System.Action OnAssistAttack;
    public System.Action OnSpecialAttack;
    public System.Action OnSpecialAttack2;
    /// <summary>
    /// 攻击输入事件
    /// </summary>
    public System.Action<float> OnInputAttackAction;


    // 添加冲刺攻击检测
    private float lastDashTime = 0f;
    private const float DASH_ATTACK_WINDOW = 0.5f;
    private bool isDashAttackWindowActive = false;

    // 预输入缓冲区
    private Stack<InputCommand> inputBuffer = new Stack<InputCommand>();

    // 当前输入状态
    public Vector2 MoveInput { get; private set; }
    /// <summary>
    /// 攻击按键持续时间
    /// 多个攻击按钮都共享 AttackHoldTime，若需要对不同技能分别计时（例如协助技能有独立蓄力进度），可改为为每个按键维持独立计时器（比如 attackHoldTime, assistHoldTime, specialHoldTime）。
    /// </summary>
    public float AttackHoldTime { get; private set; }
    public bool IsBlockPressed { get; private set; }
    public bool IsJumpPressed { get; private set; }
    public bool IsAttackPressed { get; private set; }

    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction dashAction;
    private InputAction blockAction;
    private InputAction attackAction;
    private InputAction assistAction;
    private InputAction specialAction;
    private InputAction specialAction2;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();

        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];
        dashAction = playerInput.actions["Dash"];
        blockAction = playerInput.actions["Block"];
        attackAction = playerInput.actions["Attack"];
        assistAction = playerInput.actions["AssistAttack"];
        specialAction = playerInput.actions["SpecialAttack"];
        specialAction2 = playerInput.actions.FindAction("SpecialAttack2", throwIfNotFound: false);

        //LogManager.Log($"[InputHandler] 初始化完成，缓冲区大小: {inputBuffer.Count}");
    }

    private void OnEnable()
    {
        jumpAction.started += OnJumpStarted;
        jumpAction.canceled += OnJumpCanceled;
        dashAction.started += OnDashStarted;
        blockAction.started += OnBlockStarted;
        blockAction.canceled += OnBlockCanceled;
        attackAction.started += OnAttackStartedCallback;
        attackAction.performed += OnAttackPerformedCallback;
        attackAction.canceled += OnAttackCanceledCallback;

        // assist / special also treated as attack-like inputs for hold/charge handling
        assistAction.started += OnAssistStartedCallback;
        assistAction.performed += OnAssistPerformedCallback;
        assistAction.canceled += OnAssistCanceledCallback;

        specialAction.started += OnSpecialStartedCallback;
        specialAction.performed += OnSpecialPerformedCallback;
        specialAction.canceled += OnSpecialCanceledCallback;

        if (specialAction2 != null)
        {
            specialAction2.started += OnSpecial2StartedCallback;
            specialAction2.performed += OnSpecial2PerformedCallback;
            specialAction2.canceled += OnSpecial2CanceledCallback;
        }
        else
        {
            LogManager.LogWarning("[InputHandler] 未在输入资产中找到 SpecialAttack2");
        }
        LogManager.Log($"[InputHandler] 输入事件已订阅");
    }

    private void OnDisable()
    {
        jumpAction.started -= OnJumpStarted;
        jumpAction.canceled -= OnJumpCanceled;
        dashAction.started -= OnDashStarted;
        blockAction.started -= OnBlockStarted;
        blockAction.canceled -= OnBlockCanceled;
        attackAction.started -= OnAttackStartedCallback;
        attackAction.performed -= OnAttackPerformedCallback;
        attackAction.canceled -= OnAttackCanceledCallback;

        assistAction.started -= OnAssistStartedCallback;
        assistAction.performed -= OnAssistPerformedCallback;
        assistAction.canceled -= OnAssistCanceledCallback;

        specialAction.started -= OnSpecialStartedCallback;
        specialAction.performed -= OnSpecialPerformedCallback;
        specialAction.canceled -= OnSpecialCanceledCallback;

        if (specialAction2 != null)
        {
            specialAction2.started -= OnSpecial2StartedCallback;
            specialAction2.performed -= OnSpecial2PerformedCallback;
            specialAction2.canceled -= OnSpecial2CanceledCallback;
        }

        LogManager.Log($"[InputHandler] 输入事件已取消订阅");
    }

    private void Update()
    {
        // 更新移动输入
        MoveInput = moveAction.ReadValue<Vector2>();
        OnMoveInput?.Invoke(MoveInput);

        // 更新攻击按键时间：任意攻击类按键（主攻/协助/特殊）被按住时开始累加
        if (IsAnyAttackActionPressed())
        {
            AttackHoldTime += Time.deltaTime;
            OnInputAttackAction?.Invoke(AttackHoldTime);
        }

        // 更新冲刺攻击窗口
        if (isDashAttackWindowActive && Time.time - lastDashTime > DASH_ATTACK_WINDOW)
        {
            isDashAttackWindowActive = false;
            LogManager.Log($"[InputHandler] 冲刺攻击窗口关闭");
        }
    }

    private bool IsAnyAttackActionPressed()
    {
        if (attackAction != null && attackAction.IsPressed()) return true;
        if (assistAction != null && assistAction.IsPressed()) return true;
        if (specialAction != null && specialAction.IsPressed()) return true;
        if (specialAction2 != null && specialAction2.IsPressed()) return true;
        return false;
    }

    #region 输入回调方法
    private void OnJumpStarted(InputAction.CallbackContext context)
    {
        //AddToBuffer(new InputCommand(InputCommandType.Jump, Time.time));// 不再添加到缓冲区，直接触发事件
        IsJumpPressed = true;
        OnJumpInput?.Invoke();
    }

    private void OnJumpCanceled(InputAction.CallbackContext context)
    {
        IsJumpPressed = false;
        OnJumpCanceledAction?.Invoke();
    }

    /// <summary>
    /// 冲刺操作输入
    /// </summary>
    /// <param name="context"></param>
    private void OnDashStarted(InputAction.CallbackContext context)
    {
        //AddToBuffer(new InputCommand(InputCommandType.Dash, Time.time));// 不再添加到缓冲区，直接触发事件
        OnDashInput?.Invoke();
    }

    /// <summary>
    /// 执行冲击逻辑时调用此方法，标记冲刺开始时间
    /// // 添加冲刺开始标记方法
    /// </summary>
    public void OnDashStarted()
    {
        lastDashTime = Time.time;
        isDashAttackWindowActive = true;
        LogManager.Log($"[InputHandler] 冲刺开始，开启冲刺攻击窗口");
    }

    // 添加冲刺攻击检查方法
    public bool IsInDashAttackWindow()
    {
        return isDashAttackWindowActive && Time.time - lastDashTime <= DASH_ATTACK_WINDOW;
    }


    private void OnAttackStartedCallback(InputAction.CallbackContext context)
    {
        // 开始计时
        AttackHoldTime = 0f;
        IsAttackPressed = true;

        // 先检查冲刺攻击窗口
        if (isDashAttackWindowActive && Time.time - lastDashTime <= DASH_ATTACK_WINDOW)
        {
            LogManager.Log($"[InputHandler] 检测到冲刺攻击输入");
            // 冲刺攻击由CharacterLogic处理，这里只标记
            AddToBuffer(new InputCommand(InputCommandType.DashAttack, Time.time));
        }
        else
        {
            AddToBuffer(new InputCommand(InputCommandType.Attack, Time.time));
        }

        // 统一通知攻击开始（供 CharacterAttackController 订阅）
        OnAttackStarted?.Invoke();
    }

    private void OnAttackPerformedCallback(InputAction.CallbackContext context)
    {
        // performed 不重置持有时间；这是按键被确认的阶段
        OnAttackPerformed?.Invoke();
    }

    private void OnAttackCanceledCallback(InputAction.CallbackContext context)
    {
        // 取消时触发取消事件并重置计时
        AttackHoldTime = 0f;
        IsAttackPressed = false;
        OnAttackCanceled?.Invoke();
    }

    private void OnBlockStarted(InputAction.CallbackContext context)
    {
        IsBlockPressed = true;
        AddToBuffer(new InputCommand(InputCommandType.Block, Time.time));
        OnBlockStartedAction?.Invoke();
    }

    private void OnBlockCanceled(InputAction.CallbackContext context)
    {
        IsBlockPressed = false;
        OnBlockCanceledAction?.Invoke();
    }

    // Assist handlers (treated as attack-like for hold/charge)
    private void OnAssistStartedCallback(InputAction.CallbackContext context)
    {
        // reset hold timer for new press
        AttackHoldTime = 0f;

        AddToBuffer(new InputCommand(InputCommandType.AssistAttack, Time.time));
        OnAssistAttack?.Invoke();
        //OnAttackStarted?.Invoke();
    }
    private void OnAssistPerformedCallback(InputAction.CallbackContext context)
    {
        OnAttackPerformed?.Invoke();
    }
    private void OnAssistCanceledCallback(InputAction.CallbackContext context)
    {
        AttackHoldTime = 0f;
        //OnAttackCanceled?.Invoke();
    }

    // Special handlers
    private void OnSpecialStartedCallback(InputAction.CallbackContext context)
    {
        AttackHoldTime = 0f;

        AddToBuffer(new InputCommand(InputCommandType.SpecialAttack, Time.time));
        OnSpecialAttack?.Invoke();
        //OnAttackStarted?.Invoke();
    }
    private void OnSpecialPerformedCallback(InputAction.CallbackContext context)
    {
        OnAttackPerformed?.Invoke();
    }
    private void OnSpecialCanceledCallback(InputAction.CallbackContext context)
    {
        AttackHoldTime = 0f;
        //OnAttackCanceled?.Invoke();
    }

    // Special2 handlers
    private void OnSpecial2StartedCallback(InputAction.CallbackContext context)
    {
        AttackHoldTime = 0f;

        AddToBuffer(new InputCommand(InputCommandType.SpecialAttack2, Time.time));
        OnSpecialAttack2?.Invoke();
        //OnAttackStarted?.Invoke();
    }
    private void OnSpecial2PerformedCallback(InputAction.CallbackContext context)
    {
        OnAttackPerformed?.Invoke();
    }
    private void OnSpecial2CanceledCallback(InputAction.CallbackContext context)
    {
        AttackHoldTime = 0f;
        //OnAttackCanceled?.Invoke();
    }

    private void OnAssistAttackCallback(InputAction.CallbackContext context)
    {
        AddToBuffer(new InputCommand(InputCommandType.AssistAttack, Time.time));
        OnAssistAttack?.Invoke();
    }

    private void OnSpecialAttackCallback(InputAction.CallbackContext context)
    {
        AddToBuffer(new InputCommand(InputCommandType.SpecialAttack, Time.time));
        OnSpecialAttack?.Invoke();
    }

    private void OnSpecialAttack2Callback(InputAction.CallbackContext context)
    {
        AddToBuffer(new InputCommand(InputCommandType.SpecialAttack2, Time.time));
        OnSpecialAttack2?.Invoke();
    }
    #endregion

    #region 预输入系统
    public bool TryGetBufferedInput(InputCommandType type, out InputCommand command)
    {
        command = default;

        // 只检查栈顶（最后一个）缓存操作
        if (inputBuffer.Count > 0)
        {
            var topCommand = inputBuffer.Peek();
            if (topCommand.Type == type)
            {
                command = topCommand;
                LogManager.Log($"[InputHandler] 找到有效的 {type} 输入: {command}");
                return true;
            }

            // 特殊处理：冲刺攻击可以覆盖普通攻击
            if (type == InputCommandType.DashAttack && topCommand.Type == InputCommandType.Attack)
            {
                if (IsInDashAttackWindow())
                {
                    command = new InputCommand(InputCommandType.DashAttack, Time.time);
                    LogManager.Log($"[InputHandler] 将普通攻击转换为冲刺攻击");
                    return true;
                }
            }
        }

        return false;
    }

    public void ConsumeInput(InputCommandType type)
    {
        // 只移除栈顶的指定类型输入
        if (inputBuffer.Count > 0 && inputBuffer.Peek().Type == type)
        {
            var consumed = inputBuffer.Pop();
            LogManager.Log($"[InputHandler] 消耗栈顶输入: {consumed}");
            inputBuffer.Clear(); // 每次消耗缓存区后,清空缓冲区，防止重复使用旧输入
        }
    }

    private void AddToBuffer(InputCommand command)
    {
        inputBuffer.Push(command);
        //LogManager.Log($"[InputHandler] 添加到缓冲区: {command}，当前缓冲区大小: {inputBuffer.Count}");
    }

    #endregion
}