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

    // Pending attack data for Hold / LongPress trigger types
    private AttackActionData pendingLongPressAttack = null;
    private AttackActionData activeHoldAttack = null;
    private float holdAttackTimer = 0f;
    private int holdTickCount = 0;

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

        // 订阅输入释放回调，用于 Charge / Hold 的触发或取消
        if (inputHandler != null)
        {
            inputHandler.OnAttackCanceled += OnInputAttackCanceled;
            inputHandler.OnAttackStarted += OnInputAttackStarted;
        }
    }

    private void OnDestroy()
    {
        if (inputHandler != null)
        {
            inputHandler.OnAttackCanceled -= OnInputAttackCanceled;
            inputHandler.OnAttackStarted -= OnInputAttackStarted;
        }
    }

    private void Update()
    {
        if (IsAttacking())
        {
            UpdateAttackPhase(Time.deltaTime);
        }

        if (pendingLongPressAttack != null && inputHandler != null)
        {
            if (inputHandler.AttackHoldTime >= pendingLongPressAttack.longPressTimeThreshold)
            {
                BeginAttack(pendingLongPressAttack);
                pendingLongPressAttack = null;
                activeHoldAttack = null;
                hasBufferedAttack = false;
            }
        }

        if (activeHoldAttack != null && inputHandler != null && inputHandler.IsAttackPressed)
        {
            UpdateHoldAttack(Time.deltaTime);
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

    private void OnInputAttackStarted()
    {
        // 当收到新的按下事件，如果存在 pendingCharge 或 pendingHold，需要重置（防止残留）
        // 这里不清除 pendingHoldAttack，因为按下后可能继续持有以满足阈值

    }

    private void OnInputAttackCanceled()
    {
        if (activeHoldAttack != null)
        {
            EndHoldAttack();
            return;
        }

        if (pendingLongPressAttack != null)
        {
            pendingLongPressAttack = null;
            hasBufferedAttack = false;
        }
    }

    /// <summary>
    /// 尝试发起攻击。外部负责先判断 CanAttack() 等状态条件。技能消耗,cd是否满足,能量消耗等由此处的 BeginAttack 负责判定。
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

        if (selected == null)
            return;

        switch (selected.triggerType)
        {
            case AttackTriggerType.Tap:
                BeginAttack(selected);
                break;

            case AttackTriggerType.LongPress:
                if (inputHandler != null && inputHandler.AttackHoldTime >= selected.longPressTimeThreshold)
                {
                    BeginAttack(selected);
                }
                else
                {
                    pendingLongPressAttack = selected;
                }
                break;

            case AttackTriggerType.Hold:
                StartHoldAttack(selected);
                break;

            default:
                BeginAttack(selected);
                break;
        }
    }

    // 开始一次新的攻击生命周期
    private void BeginAttack(AttackActionData attackData)
    {
        if (attackData == null) return;

        if (!CanPerformAttack(attackData))
        {
            return;
        }

        ConsumeAttackResources(attackData);

        currentAttackActionData = attackData;
        currentAttackPhase = AttackPhase.WindUp;
        currentAttackTimer = 0f;
        currentAttackId = null;

        if (animHandler != null)
        {
            animHandler.SetActionAnimationParameter(currentAttackActionData);
            animHandler.SetAttackAnimationSpeed(currentAttackPhase, currentAttackActionData);
        }

        // 应用技能释放前的效果
        ApplySkillEffectsOnCast(attackData, gameObject);

        LogManager.Log($"[CharacterAttackController] 开始攻击: {attackData.acitonName}");
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

    private void StartHoldAttack(AttackActionData attackData)
    {
        if (attackData == null) return;

        if (!CanPerformAttack(attackData))
        {
            return;
        }

        ConsumeAttackResources(attackData);

        activeHoldAttack = attackData;
        holdAttackTimer = 0f;
        holdTickCount = 0;

        // 应用技能释放前的效果
        ApplySkillEffectsOnCast(attackData, gameObject);

        LogManager.Log($"[CharacterAttackController] 开始Hold攻击: {attackData.acitonName}");
        PerformHoldAttackTick();
        if (animHandler != null && activeHoldAttack.animationClip.isLooping)
        {
            animHandler.SetAttackAnimationSpeed(activeHoldAttack, activeHoldAttack.holdTickInterval);
            animHandler.SetActionAnimationParameter(activeHoldAttack);
        }
    }

    private void UpdateHoldAttack(float delta)
    {
        if (activeHoldAttack == null) return;

        holdAttackTimer += delta;

        if (holdAttackTimer >= activeHoldAttack.holdTickInterval)
        {
            holdAttackTimer = 0f;
            holdTickCount++;

            if (holdTickCount >= activeHoldAttack.maxHoldTicks)
            {
                LogManager.Log($"[CharacterAttackController] Hold攻击达到最大次数: {activeHoldAttack.maxHoldTicks}");
                EndHoldAttack();
                return;
            }

            PerformHoldAttackTick();
        }
    }

    private void PerformHoldAttackTick()
    {
        if (activeHoldAttack == null) return;

        bool facingRight = transform.localScale.x > 0f;

        if (AttackHitDetector.Instance != null)
        {
            string tickAttackId = AttackHitDetector.Instance.StartAttackDetection(
                activeHoldAttack,
                transform.position,
                facingRight,
                gameObject);
            // 立即进行一次命中检测,不走通用的前摇后摇逻辑
            AttackHitDetector.Instance.CheckHitForFrame(
                tickAttackId,
                activeHoldAttack,
                transform.position,
                facingRight,
                0f,
                gameObject);

            AttackHitDetector.Instance.EndAttackDetection(tickAttackId);
        }

        if (animHandler != null && activeHoldAttack.animationClip.isLooping == false)//动画不循环的时候,才需要每次执行攻击都播放一次攻击动画
        {
            animHandler.SetAttackAnimationSpeed(activeHoldAttack, activeHoldAttack.holdTickInterval);//设置攻击速度
            animHandler.SetActionAnimationParameter(activeHoldAttack);//设置攻击动画
        }

        LogManager.Log($"[CharacterAttackController] Hold攻击触发第{holdTickCount + 1}次");
    }

    private void EndHoldAttack()
    {
        if (activeHoldAttack == null) return;

        LogManager.Log($"[CharacterAttackController] Hold攻击结束，共触发{holdTickCount}次");

        // Hold攻击没有统一的attackId，所以传null
        // 如果需要对Hold攻击也支持受击方效果，需要改进实现方式
        ApplySkillEffectsOnComplete(activeHoldAttack, gameObject, null);

        activeHoldAttack = null;
        holdAttackTimer = 0f;
        holdTickCount = 0;
    }

    public void EndAttack()
    {
        if (currentAttackActionData == null || m_isEndingAttack) return;

        m_isEndingAttack = true;
        Debug.Log("[CharacterAttackController] 攻击结束");

        // 应用技能释放后的效果，传递attackId以便获取受击方信息
        ApplySkillEffectsOnComplete(currentAttackActionData, gameObject, currentAttackId);

        OnAttackEnded?.Invoke();

        m_isEndingAttack = false;

        if (!string.IsNullOrEmpty(currentAttackId) && AttackHitDetector.Instance != null)
        {
            AttackHitDetector.Instance.EndAttackDetection(currentAttackId);
            currentAttackId = null;
        }

        if (attackVisualizer != null)
        {
            attackVisualizer.ClearFrameData();
        }

        currentAttackActionData = null;
        currentAttackPhase = AttackPhase.WindUp;
        currentAttackTimer = 0f;
        currentComboIndex = 0;
        ConsumeBufferedAttack();

        pendingLongPressAttack = null;
        EndHoldAttack();
    }


    public void StartAttack(AttackActionData attackData)
    {
        if (attackData == null) return;

        ConsumeBufferedAttack();

        switch (attackData.triggerType)
        {
            case AttackTriggerType.Tap:
                BeginAttack(attackData);
                break;

            case AttackTriggerType.LongPress:
                if (inputHandler != null && inputHandler.AttackHoldTime >= attackData.longPressTimeThreshold)
                {
                    BeginAttack(attackData);
                }
                else
                {
                    pendingLongPressAttack = attackData;
                }
                break;

            case AttackTriggerType.Hold:
                StartHoldAttack(attackData);
                break;

            default:
                BeginAttack(attackData);
                break;
        }
    }

    bool CanPerformAttack(AttackActionData attackData)
    {
        if (attackData.skillData == null)
        {
            LogManager.LogWarning($"[CharacterAttackController] 攻击 {attackData.acitonName} 没有配置 SkillData");
            return true;
        }

        if (characterLogic == null || characterLogic.PlayerAttributes == null)
        {
            LogManager.LogError("[CharacterAttackController] characterLogic 或 PlayerAttributes 未初始化");
            return false;
        }

        if (characterLogic.PlayerAttributes.characterAtttibute.currentEnergy < attackData.skillData.EnergyCost)
        {
            LogManager.Log($"[CharacterAttackController] 能量不足，无法攻击。需要: {attackData.skillData.EnergyCost}, 当前: {characterLogic.PlayerAttributes.characterAtttibute.currentEnergy}");
            return false;
        }

        return true;
    }

    void ConsumeAttackResources(AttackActionData attackData)
    {
        if (attackData.skillData == null) return;

        if (characterLogic != null && characterLogic.PlayerAttributes != null)
        {
            float energyCost = attackData.skillData.EnergyCost;
            characterLogic.PlayerAttributes.characterAtttibute.ChangeEnergy(-energyCost);

            LogManager.Log($"[CharacterAttackController] 消耗能量: {energyCost}, 剩余: {characterLogic.PlayerAttributes.characterAtttibute.currentEnergy}");
        }
    }

    /// <summary>
    /// 应用技能释放前的效果
    /// 在技能开始执行时触发，通常用于自身增益、消耗buff等
    /// </summary>
    /// <param name="attackData">攻击动作数据</param>
    public static void ApplySkillEffectsOnCast(AttackActionData attackData, GameObject self)
    {
        if (attackData == null || attackData.skillData == null) return;
        if (attackData.skillData.effectsOnCast == null || attackData.skillData.effectsOnCast.Count == 0) return;

        foreach (var effect in attackData.skillData.effectsOnCast)
        {
            if (effect != null)
            {
                // 根据效果目标类型选择施加对象
                GameObject effectReceiver = (effect.effectTarget == EffectTarget.Attacker) ? self : null;

                if (effectReceiver == null)
                {
                    LogManager.LogWarning($"[CharacterAttackController] 技能释放前效果 {effect.effectName} 的目标类型为Target，但没有目标对象");
                    continue;
                }

                // 获取BuffSystem组件
                var receiverBuffSystem = effectReceiver.GetComponentInParent<BuffSystem>();
                if (receiverBuffSystem == null)
                {
                    LogManager.LogWarning($"[CharacterAttackController] 效果接收者 {effectReceiver.name} 没有BuffSystem组件，无法应用技能释放前效果 {effect.effectName}");
                    continue;
                }

                // 应用效果
                receiverBuffSystem.ApplyBuff(effect, self);
                LogManager.Log($"[CharacterAttackController] 应用技能释放前效果: {effect.effectName} 到 {effectReceiver.name}");
            }
        }
    }

    /// <summary>
    /// 应用技能释放后的效果
    /// 在技能完全结束时触发，通常用于清除debuff、额外奖励等
    /// 支持通过attackId获取受击方信息，用于对目标施加效果
    /// </summary>
    /// <param name="attackData">攻击动作数据</param>
    /// <param name="self">攻击者</param>
    /// <param name="attackId">攻击ID，用于从AttackHitDetector获取DamageInfo中的受击方信息</param>
    public static void ApplySkillEffectsOnComplete(AttackActionData attackData, GameObject self, string attackId = null)
    {
        if (attackData == null || attackData.skillData == null) return;
        if (attackData.skillData.effectsOnComplete == null || attackData.skillData.effectsOnComplete.Count == 0) return;

        // 从AttackHitDetector获取DamageInfo，以获得受击方信息
        GameObject target = null;
        if (!string.IsNullOrEmpty(attackId) && AttackHitDetector.Instance != null)
        {
            DamageInfo damageInfo = AttackHitDetector.Instance.GetAttackDamageInfo(attackId);
            if (damageInfo != null)
            {
                target = damageInfo.target;
            }
        }

        foreach (var effect in attackData.skillData.effectsOnComplete)
        {
            if (effect != null)
            {
                // 根据效果目标类型选择施加对象
                GameObject effectReceiver = (effect.effectTarget == EffectTarget.Attacker) ? self : target;

                if (effectReceiver == null)
                {
                    LogManager.LogWarning($"[CharacterAttackController] 技能释放后效果 {effect.effectName} 的目标类型为Target，但没有目标对象");
                    continue;
                }

                // 获取BuffSystem组件
                var receiverBuffSystem = effectReceiver.GetComponentInParent<BuffSystem>();
                if (receiverBuffSystem == null)
                {
                    LogManager.LogWarning($"[CharacterAttackController] 效果接收者 {effectReceiver.name} 没有BuffSystem组件，无法应用技能释放后效果 {effect.effectName}");
                    continue;
                }

                // 应用效果
                receiverBuffSystem.ApplyBuff(effect, self);
                LogManager.Log($"[CharacterAttackController] 应用技能释放后效果: {effect.effectName} 到 {effectReceiver.name}");
            }
        }
    }
}