using System;
using System.Collections;
using UnityEngine;

// CharacterState.cs
/// <summary>
/// 角色状态
/// </summary>
public enum CharacterState
{
    Working,//工作
    /// <summary>
    /// 休闲
    /// </summary>
    Leisure,
    /// <summary>
    /// 崩溃
    /// </summary>
    Breakdown,
    Sleeping,//睡觉
    /// <summary>
    /// 洗澡
    /// </summary>
    Showering,
    GoingOut//外出
}
/// <summary>
/// 休闲动作
/// </summary>
public enum LeisureActivity
{
    /// <summary>
    /// 喝咖啡
    /// </summary>
    DrinkingCoffee,
    /// <summary>
    /// 玩手机
    /// </summary>
    PlayingPhone,
    /// <summary>
    /// 阅读
    /// </summary>
    ReadingBook,
    /// <summary>
    /// 发呆
    /// </summary>
    Daydreaming
}
/// <summary>
/// 表情
/// </summary>
public enum Expression
{
    /// <summary>
    /// 快乐
    /// </summary>
    Happy,
    /// <summary>
    /// 悲伤
    /// </summary>
    Sad,
    /// <summary>
    /// 无语
    /// </summary>
    Speechless,
    /// <summary>
    /// 中立
    /// </summary>
    Neutral
}

public class Character : MonoBehaviour, IMoveable
{
    [Header("属性")]
    public float StressLevel = 0f;//当前压力值
    public float MaxStressLevel = 100f;//最大压力值

    public SpriteRenderer SpriteRenderer;
    public SpriteRenderer EmojiSpriteRenderer;
    public Transform DeskTransform;//工作桌位置

    #region Test

    public SpriteRenderer faceSprite;
    public SpriteRenderer hairSprite;
    public SpriteRenderer handSprite;
    public SpriteRenderer footSprite;
    public SpriteRenderer bodySprite;
    public SpriteRenderer chairSprite;
    public SpriteRenderer chairFrontSprite;
    public SpriteRenderer hairAccessoriesSprite;

    #endregion

    public MouseEvents MouseEvents;
    public Rigidbody2D RB { get; set; }
    public bool IsFacingRight { get; set; } = true;

    #region State Machine

    public StateMachine stateMachine;
    public IdleState idleState;
    public BreakdownState breakdownState;
    public WorkState workingState;

    #endregion

    #region State ScriptableObject Logic

    public WorkLogic pWorkLogic;
    public WorkLogic pWorkLogicInstance
    {
        get;
        set;
    }

    public BreakdownLogic pBreakdownLogic;
    public BreakdownLogic pBreakdownLogicInstance
    {
        get;
        set;
    }

    public IdleStateEmptyLogic pIdleStateEmptyLogic;
    public IdleStateEmptyLogic pIdleStateEmptyLogicInstance
    {
        get;
        set;
    }

    public IdleStateEatFoodLogic pIdleStateEatFoodLogic;
    public IdleStateEatFoodLogic pIdleStateEatFoodLogicInstance
    {
        get;
        set;
    }

    #endregion

    public EmojiLogic EmojiLogic;
    [NonSerialized]
    public EmojiLogic m_PEmojiLogicInstance;
    private Coroutine m_DelayHideEmojiCoroutine;


    private AddMoneyLogic m_pAddMoneyLogic;

    private void Awake()
    {
        RB = GetComponent<Rigidbody2D>();
        stateMachine = new StateMachine();
        idleState = new IdleState(stateMachine, this);
        breakdownState = new BreakdownState(stateMachine, this);
        workingState = new WorkState(stateMachine, this);

        m_pAddMoneyLogic = new AddMoneyLogic();
        m_pAddMoneyLogic = new AddMoneyLogic();
        m_pAddMoneyLogic.moneyCriticalMultiplier = pWorkLogic.moneyCriticalMultiplier;
        m_pAddMoneyLogic.moneyCriticalRate = pWorkLogic.moneyCriticalRate;
        m_pAddMoneyLogic.moneyIncreaseAmount = pWorkLogic.moneyIncreaseAmount;

        pWorkLogicInstance = Instantiate(pWorkLogic);//实例化工作逻辑脚本ableObject
        pBreakdownLogicInstance = Instantiate(pBreakdownLogic);//实例化崩溃逻辑脚本ableObject
        pIdleStateEmptyLogicInstance = Instantiate(pIdleStateEmptyLogic);

        pIdleStateEatFoodLogicInstance = Instantiate(pIdleStateEatFoodLogic);

        m_PEmojiLogicInstance = Instantiate(EmojiLogic);


        MouseEvents.OnMouseClickHander.AddListener(OnMouseClick);
        MouseEvents.OnMouseDownHander.AddListener(OnMouseDownEvent);
        MouseEvents.OnMouseUpHander.AddListener(OnMouseUpEvent);
        MouseEvents.OnMouseDragHander.AddListener(OnMouseDragEvent);


    }

    private void Start()
    {
        pWorkLogicInstance.Initialize(this);//初始化工作逻辑脚本ableObject
        pBreakdownLogicInstance.Initialize(this);
        pIdleStateEmptyLogicInstance.Initialize(this);
        pIdleStateEatFoodLogicInstance.Initialize(this);
        m_PEmojiLogicInstance.Init(this);

        stateMachine.Initialize(workingState, new StateMachineContext());//设置初始状态

        SetEmojiEnable(false);
    }

    private void Update()
    {
        stateMachine.CurrentState.LogicUpdate();
        m_PEmojiLogicInstance.OnUpdate();
    }

    private void FixedUpdate()
    {
        stateMachine.CurrentState.PhysicsUpdate();
    }



    private void AnimationTriggerEvent(EAnimationTrigger animationTrigger)
    {
        stateMachine.CurrentState.AmationTriggerEvent(animationTrigger);
    }

    public void AddMoney(float rate = 1f)
    {
        m_pAddMoneyLogic.AddMoney(rate);
    }

    public void AddStress(float value)
    {
        StressLevel += value;
        if (StressLevel >= MaxStressLevel)
        {
            stateMachine.ChangeState(breakdownState);//切换到崩溃状态
        }
    }

    public void SetSprite(Sprite sprite)
    {
        SpriteRenderer.enabled = true;
        SpriteRenderer.sprite = sprite;
    }

    public void SetSpriteFlipX(bool isleft)
    {
        //SpriteRenderer.flipX = isleft;//多层级下flipX无法支持正确反转
        SpriteRenderer.transform.localScale = new Vector3(isleft ? -1 : 1, 1, 1);
    }

    #region Test


    public void SetSprite1(Sprite sprite)
    {
        faceSprite.sprite = sprite;
    }

    public void SetSprite2(Sprite sprite)
    {
        hairSprite.sprite = sprite;
    }

    public void SetSprite3(Sprite sprite)
    {
        handSprite.sprite = sprite;
    }

    public void SetSprite4(Sprite sprite)
    {
        footSprite.sprite = sprite;
    }

    public void SetSprite5(Sprite sprite)
    {
        bodySprite.sprite = sprite;
    }

    public void SetSprite6(Sprite sprite)
    {
        chairSprite.sprite = sprite;
    }
    public void SetSprite7(Sprite sprite)
    {
        chairFrontSprite.sprite = sprite;
    }

    public void SetSprite8(Sprite sprite)
    {
        hairAccessoriesSprite.sprite = sprite;
    }
    #endregion
    public void SetEmojiSprite(Sprite sprite)
    {
        EmojiSpriteRenderer.sprite = sprite;
        SetEmojiEnable(true);

        if (m_DelayHideEmojiCoroutine != null)
        {
            StopCoroutine(m_DelayHideEmojiCoroutine);
            m_DelayHideEmojiCoroutine = null;
        }
        m_DelayHideEmojiCoroutine = StartCoroutine(DelayHideEmoji());
    }

    IEnumerator DelayHideEmoji()
    {
        yield return new WaitForSeconds(2f);
        SetEmojiEnable(false);
        m_DelayHideEmojiCoroutine = null;
        m_PEmojiLogicInstance.OnEmojiComplete();
    }

    void SetEmojiEnable(bool enable)
    {
        EmojiSpriteRenderer.enabled = enable;
    }


    #region Mouse Events


    private void OnMouseClick()
    {
        m_PEmojiLogicInstance.OnInteract();
    }


    private void OnMouseDragEvent()
    {
        stateMachine.CurrentState.OnMouseDragEvent();
    }

    private void OnMouseUpEvent()
    {
        stateMachine.CurrentState.OnMouseUpEvent();
    }

    private void OnMouseDownEvent()
    {
        stateMachine.CurrentState.OnMouseDownEvent();
    }

    #endregion

    #region IMovebale

    public void SetMove(Vector2 dir)
    {
        RB.AddForce(dir, ForceMode2D.Impulse);

        SetSpriteFlipX(dir.x < 0);
    }


    public void CheckForLeftFacing(Vector2 velocity)
    {
        if (IsFacingRight && velocity.x < 0)
        {
            Vector3 ratator = new Vector3(transform.rotation.x, 180, transform.rotation.z);
            transform.rotation = Quaternion.Euler(ratator);
            IsFacingRight = false;
        }
        else if (!IsFacingRight && velocity.x > 0)
        {
            Vector3 ratator = new Vector3(transform.rotation.x, 0, transform.rotation.z);
            transform.rotation = Quaternion.Euler(ratator);
            IsFacingRight = true;
        }
    }

    public void Move(Vector2 velocity)
    {
        RB.linearVelocity = velocity;

        CheckForLeftFacing(velocity);//todo: 这里可能会有问题
    }
    #endregion
}
