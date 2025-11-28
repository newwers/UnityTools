using System;
using UnityEngine;

[DisallowMultipleComponent]
public class CharacterAnimation : MonoBehaviour
{
    [Header("动画设置")]
    public float animationBaseSpeed = 1.5f;
    public float dashAnimationLength = 0.643f;

    // 被击计时器
    private float hitFlashTimer = 0f;
    private Color originalColor;
    public Color hitColor = Color.red;
    public float hitFlashDuration = 0.1f;

    private Animator animator;
    private CharacterLogic logic;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    // 动画参数名称
    private readonly int animMoveSpeed = Animator.StringToHash("animMoveSpeed");
    private readonly int moveX = Animator.StringToHash("MoveX");
    private readonly int grounded = Animator.StringToHash("Grounded");
    private readonly int airSpeedY = Animator.StringToHash("AirSpeedY");
    private readonly int jump = Animator.StringToHash("Jump");
    private readonly int block = Animator.StringToHash("Block");
    private readonly int blockSuccess = Animator.StringToHash("BlockSuccess");
    private readonly int idleBlock = Animator.StringToHash("IdleBlock");
    private readonly int hurt = Animator.StringToHash("Hurt");
    private readonly int death = Animator.StringToHash("Death");
    private readonly int stun = Animator.StringToHash("Stun");
    private readonly int roll = Animator.StringToHash("Roll");
    private readonly int animRollSpeed = Animator.StringToHash("animRollSpeed");

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();


        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    void SubEvent()
    {
        if (logic)
        {
            // 订阅逻辑事件
            logic.OnStateChanged += OnStateChanged;
            logic.OnJump += OnJump;
            logic.OnLandAction += OnLand;
            logic.OnDeath += OnDeath;
            logic.OnBlockSuccess += OnBlockSuccess;
        }
    }

    void UnSubEvent()
    {
        if (logic != null)
        {
            logic.OnStateChanged -= OnStateChanged;
            logic.OnJump -= OnJump;
            logic.OnLandAction -= OnLand;
            logic.OnDeath -= OnDeath;
            logic.OnBlockSuccess -= OnBlockSuccess;
        }

    }

    private void OnEnable()
    {
        //logic.OnStunned += OnStunned;
    }


    private void OnDisable()
    {
        UnSubEvent();
        //logic.OnStunned -= OnStunned;
    }

    private void Update()
    {
        // 处理被击闪白效果
        if (hitFlashTimer > 0)
        {
            hitFlashTimer -= Time.deltaTime;
            if (hitFlashTimer <= 0 && spriteRenderer != null)
            {
                spriteRenderer.color = originalColor;
            }

        }
        UpdateAnimationParameters();
    }


    public void SetCharacterLogic(CharacterLogic characterLogic, Rigidbody2D rigidbody2D)
    {
        logic = characterLogic;
        rb = rigidbody2D;

        SubEvent();
    }

    private void UpdateAnimationParameters()
    {
        if (rb == null)
        {

            return;
        }
        // 移动速度参数
        float actualMoveSpeed = new Vector2(rb.linearVelocity.x, 0).magnitude;
        float animMoveSpeedValue = actualMoveSpeed / animationBaseSpeed;
        animator.SetFloat(animMoveSpeed, animMoveSpeedValue);

        // 水平移动输入
        float moveInputX = 0;
        if (logic && logic.InputHandler)
        {
            moveInputX = logic.InputHandler.MoveInput.x;
        }
        animator.SetFloat(moveX, Mathf.Abs(moveInputX));

        // 地面检测
        animator.SetBool(grounded, logic.IsGrounded);

        // 垂直速度
        animator.SetFloat(airSpeedY, rb.linearVelocity.y);
    }

    #region 动画事件处理
    private void OnStateChanged(PlayerState previousState, PlayerState newState)
    {
        // 根据状态切换动画
        if (previousState == newState)
        {
            return;
        }

        if (previousState == PlayerState.Stunned)//眩晕结束
        {
            OnStunnedEnd();
        }

        switch (newState)
        {
            case PlayerState.Dashing:
                animator.SetFloat(animRollSpeed, dashAnimationLength / logic.actionManager.dashAction.dashDuration);
                //animator.SetTrigger(roll);
                break;

            //case PlayerState.Blocking:
            //    animator.SetTrigger(block);
            //    animator.SetBool(idleBlock, true);
            //    break;

            case PlayerState.Stunned://眩晕状态
                OnStunned();
                return;//眩晕状态不处理后续动画参数设置,否则会覆盖眩晕动画,眩晕没有对应的ActionData,后面看看要不要统一
        }

        SetActionAnimationParameter(logic.currentActionData);
    }

    /// <summary>
    /// 硬直
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    private void OnStunned()
    {
        animator.SetBool(stun, true);
    }

    private void OnStunnedEnd()
    {
        animator.SetBool(stun, false);
    }

    private void OnBlockSuccess()
    {
        // 格挡成功动画处理
        animator.SetTrigger(blockSuccess);
    }

    private void OnJump()
    {
        animator.SetTrigger(jump);
    }

    private void OnLand()
    {
        // 落地动画处理
    }

    public void OnHurt(bool shouldPlayHitAnimation)
    {
        if (shouldPlayHitAnimation)
        {
            animator.SetTrigger(hurt);
        }

        // 视觉反馈
        PlayHitFeedback();//闪白效果总是播放
    }

    private void PlayHitFeedback()
    {
        // 颜色闪白
        if (spriteRenderer != null)
        {
            spriteRenderer.color = hitColor;
            hitFlashTimer = hitFlashDuration;
        }
    }

    private void OnDeath()
    {
        //animator.SetTrigger(death);//动画在OnStateChanged中进行控制播放
    }
    #endregion

    #region 动画状态查询
    public bool CanInterruptCurrentAnimation()
    {
        if (!animator.IsInTransition(0))
        {
            AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(0);
            return currentState.normalizedTime > 0.7f; // 动画播放70%后可以中断
        }
        return true;
    }

    public bool IsCurrentAnimationFinished(string animationName)
    {
        AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(0);
        return currentState.IsName(animationName) && currentState.normalizedTime >= 1.0f;
    }
    #endregion

    public void SetAttackAnimationSpeed(AttackActionData attackData, float time)
    {
        //根据给定时间设置动画速度
        if (attackData != null && attackData.animationClip != null)
        {
            float actualLength = attackData.animationClip.length;
            float speedMultiplier = actualLength / time; //动画播放速度 = 原始播放时间 / 期望播放时间
            LogManager.Log($"[CharacterAnimation] 设置攻击动画速度: {speedMultiplier} for {attackData.acitonName}");
            animator.SetFloat("AttackSpeed", speedMultiplier);
        }
    }
    // 添加攻击动画速度控制
    public void SetAttackAnimationSpeed(AttackPhase phase, AttackActionData attackData)
    {
        if (attackData != null && attackData.animationClip != null)
        {
            // 根据攻击数据调整动画速度
            float totalFrames = attackData.windUpFrames + attackData.activeFrames + attackData.recoveryFrames;
            float desiredLength = attackData.TotalDuration;
            float actualLength = attackData.animationClip.length;

            switch (phase)
            {
                case AttackPhase.WindUp:
                    totalFrames = attackData.windUpFrames;
                    desiredLength = attackData.windUpTime;
                    actualLength = attackData.ActualWindUpFrames;
                    break;
                case AttackPhase.Active:
                    totalFrames = attackData.activeFrames;
                    desiredLength = attackData.activeTime;
                    actualLength = attackData.ActualActiveFrames;
                    break;
                case AttackPhase.Recovery:
                    totalFrames = attackData.recoveryFrames;
                    desiredLength = attackData.recoveryTime;
                    actualLength = attackData.ActualRecoveryFrames;
                    break;
                default:
                    break;
            }

            //float speedMultiplier = totalFrames / (desiredLength * attackData.frameRate);
            float speedMultiplier = 1 / (desiredLength / (actualLength / attackData.frameRate)); //动画播放速度 = 期望播放时间 / 原始播放时间    原始播放时间 = 原始帧数 ÷ 原始帧率  最后还要取倒数因为Unity的Animator速度是乘法关系
            LogManager.Log($"[CharacterAnimation] 设置攻击动画速度: {speedMultiplier} ,phase{phase} for {attackData.acitonName}");
            animator.SetFloat("AttackSpeed", speedMultiplier);
        }
    }

    /// <summary>
    /// 根据动作数据设置动画参数
    /// 目前只有切换状态的时候触发一次,对于持续性例如移动动作,在update中负责动画调用
    /// </summary>
    /// <param name="actionData"></param>
    public void SetActionAnimationParameter(ActionData actionData)
    {
        if (actionData == null || actionData.animationParameters == null)
            return;

        foreach (var param in actionData.animationParameters)
        {
            //LogManager.Log($"[CharacterAnimation] 设置动画参数: {param.parameterName}, 类型: {param.type}");
            switch (param.type)
            {
                case ActionData.AnimationParameterType.Trigger:
                    animator.SetTrigger(param.parameterName);
                    break;
                case ActionData.AnimationParameterType.Bool:
                    animator.SetBool(param.parameterName, param.animationBoolValue);
                    break;
                case ActionData.AnimationParameterType.Int:
                    animator.SetInteger(param.parameterName, param.animationIntValue);
                    break;
                case ActionData.AnimationParameterType.Float:
                    animator.SetFloat(param.parameterName, param.animationFloatValue);
                    break;
            }
        }


    }

}