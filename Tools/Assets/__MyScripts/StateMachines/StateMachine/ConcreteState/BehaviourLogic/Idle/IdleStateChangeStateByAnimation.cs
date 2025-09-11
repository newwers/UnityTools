using System.Collections.Generic;
using UnityEngine;

public class IdleStateChangeStateByAnimation : IdleStateSOBase
{
    [Header("切换状态所需动画播放次数")]
    public Vector2Int PlayAnimationNum = new Vector2Int(1, 1);

    [Header("图片播放切换间隔")]
    public float frameRate = 0.18f; // 每帧切换的时间间隔


    [Header("脸")]
    public List<Sprite> faceSprites = new List<Sprite>();
    [Header("头发")]
    public List<Sprite> hairSprites = new List<Sprite>();
    [Header("手")]
    public List<Sprite> handSprites = new List<Sprite>();
    [Header("腿")]
    public List<Sprite> footSprites = new List<Sprite>();
    [Header("身体")]
    public List<Sprite> bodySprites = new List<Sprite>();
    [Header("椅子")]
    public List<Sprite> chairSprites = new List<Sprite>();
    [Header("椅子前扶手")]
    public List<Sprite> chairFrontSprites = new List<Sprite>();
    [Header("发饰")]
    public List<Sprite> hairAccessoriesSprites = new List<Sprite>();

    [Header("拖拽推力大小")]
    public float moveForce = 3f;



    [Header("拒绝拖拽概率")]
    public float RefusedRate = 0.6f;
    [Header("拒绝后冷却时间")]
    public float RefusedCooldown = 3f;

    [Header("金币增加间隔")]
    public Vector2 moneyIncreaseInterval = new Vector2(3, 6);
    [Header("金币增加间隔")]
    public float moneyIncreaseRate = 0.3f; // 每次增加金币的倍率

    private float nextSpriteChangeTime = 0f;
    private int currentFrameIndex = 0;
    private int currentLoopCount = 0;
    private int maxLoopCount = 0;
    private int maxFrameCount = 0;

    private bool m_bIsMoveLeft = false;

    protected Character characterController;


    private bool IsDragging = false;
    private bool m_IsRefused = false;
    private float refusedCooldownTimer = 0f;
    private bool m_HasMouseDownOnState = false;

    //金币计时器
    private float moneyIncreaseTimer;

    public override void Initialize(Character character)
    {
        base.Initialize(character);
        characterController = character;


        characterController.faceSprite.enabled = false;
        characterController.hairSprite.enabled = false;
        characterController.handSprite.enabled = false;
        characterController.footSprite.enabled = false;
        characterController.bodySprite.enabled = false;
        characterController.chairSprite.enabled = false;
        characterController.chairFrontSprite.enabled = false;
        characterController.hairAccessoriesSprite.enabled = false;

        //计算最大帧数
        maxFrameCount = Mathf.Max(faceSprites.Count, hairSprites.Count, handSprites.Count, footSprites.Count, bodySprites.Count, chairSprites.Count, chairFrontSprites.Count);
    }

    public override void Enter()
    {
        base.Enter();
        //设置循环次数
        maxLoopCount = Random.Range(PlayAnimationNum.x, PlayAnimationNum.y + 1);

        LogManager.Log($"进入休闲状态,循环次数:{maxLoopCount},最大帧数:{maxFrameCount},动画时长:{maxFrameCount * frameRate}");

        //播放索引归零
        currentFrameIndex = 0;

        //隐藏原有的SpriteRenderer
        characterController.SpriteRenderer.enabled = false;

        if (faceSprites.Count > 0)
        {
            characterController.SetSprite1(faceSprites[currentFrameIndex]);
        }
        if (hairSprites.Count > 0)
            characterController.SetSprite2(hairSprites[currentFrameIndex]);
        if (handSprites.Count > 0)
            characterController.SetSprite3(handSprites[currentFrameIndex]);
        if (footSprites.Count > 0)
            characterController.SetSprite4(footSprites[currentFrameIndex]);
        if (bodySprites.Count > 0)
            characterController.SetSprite5(bodySprites[currentFrameIndex]);
        if (chairSprites.Count > 0)
            characterController.SetSprite6(chairSprites[currentFrameIndex]);
        if (chairFrontSprites.Count > 0)
            characterController.SetSprite7(chairFrontSprites[currentFrameIndex]);
        if (hairAccessoriesSprites.Count > 0)
            characterController.SetSprite8(hairAccessoriesSprites[currentFrameIndex]);


        characterController.faceSprite.enabled = true;
        characterController.hairSprite.enabled = true;
        characterController.handSprite.enabled = true;
        characterController.footSprite.enabled = true;
        characterController.bodySprite.enabled = true;
        characterController.chairSprite.enabled = true;
        characterController.chairFrontSprite.enabled = true;
        characterController.hairAccessoriesSprite.enabled = true;

        nextSpriteChangeTime = Time.time + frameRate;


        //开始计时
        moneyIncreaseTimer = Time.time + GetRandomMoneyIncreaseInterval();


        //位移
        Move();
    }

    public override void Exit()
    {
        base.Exit();

        characterController.faceSprite.enabled = false;
        characterController.hairSprite.enabled = false;
        characterController.handSprite.enabled = false;
        characterController.footSprite.enabled = false;
        characterController.bodySprite.enabled = false;
        characterController.chairSprite.enabled = false;
        characterController.chairFrontSprite.enabled = false;
        characterController.hairAccessoriesSprite.enabled = false;



        IsDragging = false;
        m_IsRefused = false;
        refusedCooldownTimer = 0f;
        m_HasMouseDownOnState = false;
        moneyIncreaseTimer = 0f;
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        // 更新Sprite播放
        if (Time.time >= nextSpriteChangeTime)
        {
            PlayNextSprite();
            nextSpriteChangeTime = Time.time + frameRate;
        }



        if (moneyIncreaseTimer < Time.time)
        {
            moneyIncreaseTimer = Time.time + GetRandomMoneyIncreaseInterval();
            AddMoney();
        }

    }

    private void PlayNextSprite()
    {
        currentFrameIndex++;

        if (faceSprites.Count > 0)
            characterController.SetSprite1(faceSprites[Mathf.Clamp(currentFrameIndex, 0, faceSprites.Count - 1)]);
        if (hairSprites.Count > 0)
            characterController.SetSprite2(hairSprites[Mathf.Clamp(currentFrameIndex, 0, hairSprites.Count - 1)]);
        if (handSprites.Count > 0)
            characterController.SetSprite3(handSprites[Mathf.Clamp(currentFrameIndex, 0, handSprites.Count - 1)]);
        if (footSprites.Count > 0)
            characterController.SetSprite4(footSprites[Mathf.Clamp(currentFrameIndex, 0, footSprites.Count - 1)]);
        if (bodySprites.Count > 0)
            characterController.SetSprite5(bodySprites[Mathf.Clamp(currentFrameIndex, 0, bodySprites.Count - 1)]);
        if (chairSprites.Count > 0)
            characterController.SetSprite6(chairSprites[Mathf.Clamp(currentFrameIndex, 0, chairSprites.Count - 1)]);
        if (chairFrontSprites.Count > 0)
            characterController.SetSprite7(chairFrontSprites[Mathf.Clamp(currentFrameIndex, 0, chairFrontSprites.Count - 1)]);
        if (hairAccessoriesSprites.Count > 0)
            characterController.SetSprite8(hairAccessoriesSprites[Mathf.Clamp(currentFrameIndex, 0, hairAccessoriesSprites.Count - 1)]);

        //是否动画循环?,目前是播放到最后一帧停止
        if (currentFrameIndex >= maxFrameCount)
        {
            currentLoopCount++;
            if (IsConditionMet())
            {
                ChangeStateEvent();
            }
            else
            {
                //重置播放索引
                currentFrameIndex = 0;
            }
        }
    }

    //是否达到条件
    public override bool IsConditionMet()
    {
        return currentLoopCount >= maxLoopCount;
    }
    /// <summary>
    /// 满足条件后,如何切换状态函数
    /// </summary>
    protected override void ChangeStateEvent()
    {
        //动画播放完毕,切换到工作状态
        characterController.stateMachine.ChangeState(characterController.workingState);
    }

    void Move()
    {
        bool hasDir = characterController.stateMachine.Context.WorkStateChangeToIdleStateDirector.magnitude > 0;
        if (hasDir)//如果有方向,则直接使用该方向移动,并清空该方向
        {
            characterController.SetMove(characterController.stateMachine.Context.WorkStateChangeToIdleStateDirector);
            characterController.stateMachine.Context.WorkStateChangeToIdleStateDirector = Vector2.zero;
            return;
        }

        // 计算力的方向：水平方向 × 推力大小（Y轴为0，不产生上下力）
        Vector2 dir = m_bIsMoveLeft ? Vector2.left : Vector2.right;
        Vector2 forceDirection = dir * moveForce;

        // 给刚体添加力（ForceMode2D.Force：基于质量的力，符合物理规律）

        characterController.SetMove(forceDirection);

        m_bIsMoveLeft = !m_bIsMoveLeft;

        // 可选：使用Impulse模式（瞬时冲量，不受质量影响，适合跳跃/快速启动）
        // rb2d.AddForce(forceDirection, ForceMode2D.Impulse);
    }

    public void AddMoney()
    {
        LogManager.Log($"休闲状态下增加金币");
        characterController.AddMoney(moneyIncreaseRate);
    }

    float GetRandomMoneyIncreaseInterval()
    {
        return Random.Range(moneyIncreaseInterval.x, moneyIncreaseInterval.y);
    }

    public override void OnMouseDownEvent()
    {
        base.OnMouseDownEvent();

        LogManager.Log($"休闲状态下鼠标按下");
        m_HasMouseDownOnState = true;
    }

    public override void OnMouseUpEvent()
    {
        base.OnMouseUpEvent();

        IsDragging = false;
        m_IsRefused = false;
        m_HasMouseDownOnState = false;
        LogManager.Log($"休闲状态下鼠标抬起");//如果在拖动过程中切换状态,那么这边代码触发不了
    }

    public override void OnMouseDragEvent()
    {
        base.OnMouseDragEvent();

        if (m_HasMouseDownOnState == false)//确保是从当前状态下按下鼠标开始拖动的
        {
            return;
        }

        // 检查是否在冷却时间内
        if (Time.time < refusedCooldownTimer)
        {
            // 显示拒绝表情
            characterController.m_PEmojiLogicInstance.ShowRefuseEmoji();
            LogManager.Log($"休闲状态下拖动，冷却中，拒绝休息");
            return;
        }

        if (IsDragging == false)
        {
            IsDragging = true;

            //主角在工作时，按住左键向左或右拖动一段距离，即可让主角按鼠标滑动方向弹开去休息；30%概率受到拒绝（弹出“勿扰”、“嘘声”等心情气泡）\
            float value = Random.value;
            m_IsRefused = value < RefusedRate;
            LogManager.Log($"休闲状态下拖动,随机值:{value},是否拒绝:{m_IsRefused}");
            if (m_IsRefused)
            {
                //显示拒绝表情
                characterController.m_PEmojiLogicInstance.ShowRefuseEmoji();
                refusedCooldownTimer = Time.time + RefusedCooldown;
                LogManager.Log($"休闲状态下拖动，拒绝休息");
                return;
            }
        }

        if (IsDragging && m_IsRefused)
        {
            //显示拒绝表情
            characterController.m_PEmojiLogicInstance.ShowRefuseEmoji();
            return;//如果拒绝休息,则不处理拖动逻辑
        }

        //计算左右拖拽方向
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 direction = mousePosition - characterController.transform.position;
        direction.y = 0;
        direction.z = 0;
        direction.Normalize();

        //计算电脑在角色的方向
        var deskDir = characterController.DeskTransform.position - characterController.transform.position;
        deskDir.y = 0;
        deskDir.z = 0;
        deskDir.Normalize();

        //判断两个方向是否相同
        float dot = Vector3.Dot(direction, deskDir);
        if (dot <= 0)
        {
            //相同方向,则拒绝
            LogManager.Log($"休闲状态下拖动，方向不相同桌子");
            return;
        }

        characterController.stateMachine.ChangeState(characterController.workingState);

        LogManager.Log($"休闲状态下鼠标拖拽,切换到工作状态,方向:" + direction);
    }
}
