using System;
using UnityEngine;

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
    private readonly int attack = Animator.StringToHash("Attack");
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
        logic = GetComponent<CharacterLogic>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    private void OnEnable()
    {
        // 订阅逻辑事件
        logic.OnStateChanged += OnStateChanged;
        logic.OnJump += OnJump;
        logic.OnLandAction += OnLand;
        logic.OnDeath += OnDeath;
        logic.OnBlockSuccess += OnBlockSuccess;
        //logic.OnStunned += OnStunned;
    }


    private void OnDisable()
    {
        logic.OnStateChanged -= OnStateChanged;
        logic.OnJump -= OnJump;
        logic.OnLandAction -= OnLand;
        logic.OnDeath -= OnDeath;
        logic.OnBlockSuccess -= OnBlockSuccess;
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

    private void UpdateAnimationParameters()
    {
        // 移动速度参数
        float actualMoveSpeed = new Vector2(rb.linearVelocity.x, 0).magnitude;
        float animMoveSpeedValue = actualMoveSpeed / animationBaseSpeed;
        animator.SetFloat(animMoveSpeed, animMoveSpeedValue);

        // 水平移动输入
        animator.SetFloat(moveX, Mathf.Abs(logic.GetComponent<InputHandler>().MoveInput.x));

        // 地面检测
        animator.SetBool(grounded, logic.GetComponent<CharacterLogic>().IsGrounded);

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
                animator.SetFloat(animRollSpeed, dashAnimationLength / logic.dashDuration);
                animator.SetTrigger(roll);
                break;

            case PlayerState.Blocking:
                animator.SetTrigger(block);
                animator.SetBool(idleBlock, true);
                break;

            case PlayerState.Idle:
                animator.SetBool(idleBlock, false);
                break;
            case PlayerState.Stunned:
                OnStunned();
                break;
        }
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
        animator.SetBool("noBlood", false);
        animator.SetTrigger(death);
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


    // 添加攻击动画速度控制
    public void SetAttackAnimationSpeed(AttackPhase phase, ActionData attackData)
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

    // 添加新的动画参数设置方法
    public void SetAttackAnimationParameter(ActionData attackData)
    {
        if (attackData == null || string.IsNullOrEmpty(attackData.animationParameterName))
            return;

        switch (attackData.animationParameterType)
        {
            case ActionData.AnimationParameterType.Trigger:
                animator.SetTrigger(attackData.animationParameterName);
                break;
            case ActionData.AnimationParameterType.Bool:
                animator.SetBool(attackData.animationParameterName, attackData.animationBoolValue);
                break;
            case ActionData.AnimationParameterType.Int:
                animator.SetInteger(attackData.animationParameterName, attackData.animationIntValue);
                break;
            case ActionData.AnimationParameterType.Float:
                animator.SetFloat(attackData.animationParameterName, attackData.animationFloatValue);
                break;
        }

        LogManager.Log($"[CharacterAnimation] 设置动画参数: {attackData.animationParameterName}, 类型: {attackData.animationParameterType}");
    }

    // 清理动画参数（在攻击结束时调用）
    public void ClearAttackAnimationParameter(ActionData attackData)
    {
        if (attackData == null || string.IsNullOrEmpty(attackData.animationParameterName))
            return;

        switch (attackData.animationParameterType)
        {
            case ActionData.AnimationParameterType.Bool:
                animator.SetBool(attackData.animationParameterName, false);
                break;
            case ActionData.AnimationParameterType.Trigger:
                // Trigger不需要清理，它会自动重置
                break;
                // Int和Float类型通常不需要特别清理
        }
    }

}