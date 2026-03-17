using UnityEngine;

[CreateAssetMenu(fileName = "WorkLogic", menuName = "状态机/工作/工作逻辑")]
public class WorkLogic : WorkStateSOBase
{
    [Header("拒绝拖拽概率")]
    public float RefusedRate = 0.3f;
    [Header("拒绝后冷却时间")]
    public float RefusedCooldown = 3f;
    [Header("检测状态切换间隔")]
    public Vector2 CheckChangeStateInterval = new Vector2(10, 10);
    [Header("每次检测切换状态的概率")]
    public float ChangeStateRate = 0.2f;
    [Header("金币增加间隔")]
    public Vector2 moneyIncreaseInterval = new Vector2(3, 6);

    [Header("金币增加量")]
    public Vector2Int moneyIncreaseAmount = new Vector2Int(2, 5);

    [Header("获得金币暴击率")]
    public float moneyCriticalRate = 0.05f;
    [Header("获得金币暴击倍数")]
    public float moneyCriticalMultiplier = 2f;

    //[Header("压力增加量")]
    //public float stressIncreaseAmount = 5f;

    public Sprite stateIcon;

    public float moveForce = 3f; // 推力大小

    //金币计时器
    private float moneyIncreaseTimer;
    //切换状态计时器
    private float changeStateTimer;

    private Character characterController;

    private bool IsDragging = false;
    private bool m_IsRefused = false;
    private float refusedCooldownTimer = 0f;
    private bool m_HasMouseDownOnState = false;


    public override void Initialize(Character character)
    {
        base.Initialize(character);
        characterController = character;

    }
    public override void AnimationTriggerEvent(EAnimationTrigger animationTrigger)
    {
        base.AnimationTriggerEvent(animationTrigger);
    }

    public override void Enter()
    {
        base.Enter();
        //播放动画
        characterController.SetSprite(stateIcon);

        //开始计时
        moneyIncreaseTimer = Time.time + GetRandomMoneyIncreaseInterval();
        changeStateTimer = Time.time + Random.Range(CheckChangeStateInterval.x, CheckChangeStateInterval.y);

        characterController.transform.localPosition = new Vector3(0, characterController.transform.localPosition.y, characterController.transform.localPosition.z);

        LogManager.Log($"进入工作状态,时长:" + (changeStateTimer - Time.time));
    }

    public override void Exit()
    {
        base.Exit();
        IsDragging = false;
        m_IsRefused = false;
        refusedCooldownTimer = 0;
        m_HasMouseDownOnState = false;
    }


    public override void LogicUpdate()
    {
        base.LogicUpdate();
        if (moneyIncreaseTimer < Time.time)
        {

            moneyIncreaseTimer = Time.time + GetRandomMoneyIncreaseInterval();
            AddMoney();
            //characterController.AddStress(stressIncreaseAmount);
        }

        if (changeStateTimer < Time.time)
        {
            changeStateTimer = Time.time + Random.Range(CheckChangeStateInterval.x, CheckChangeStateInterval.y);
            float value = Random.value;
            if (value < ChangeStateRate)
            {
                LogManager.Log($"工作状态下,切换到休闲状态,随机值:{value}");
                characterController.stateMachine.ChangeState(characterController.idleState);
            }
            else
            {
                LogManager.Log($"工作状态下,继续工作,随机值:{value}");
            }
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
    }

    public override void ResetValues()
    {
        base.ResetValues();
    }

    public void AddMoney()
    {
        LogManager.Log($"工作状态添加金币");
        characterController.AddMoney(1f);
    }

    float GetRandomMoneyIncreaseInterval()
    {
        return Random.Range(moneyIncreaseInterval.x, moneyIncreaseInterval.y);
    }


    public override void OnMouseDownEvent()
    {
        base.OnMouseDownEvent();

        LogManager.Log($"工作状态下鼠标按下");
        m_HasMouseDownOnState = true;

    }

    public override void OnMouseUpEvent()
    {
        base.OnMouseUpEvent();

        IsDragging = false;
        m_IsRefused = false;
        m_HasMouseDownOnState = false;
        LogManager.Log($"工作状态下鼠标抬起");//如果在拖动过程中切换状态,那么这边代码触发不了
    }

    public override void OnMouseDragEvent()
    {
        base.OnMouseDragEvent();

        if (m_HasMouseDownOnState == false)//确保是从工作状态下按下鼠标开始拖动的
        {
            return;
        }

        // 检查是否在冷却时间内
        if (Time.time < refusedCooldownTimer)
        {
            // 显示拒绝表情
            characterController.m_PEmojiLogicInstance.ShowRefuseEmoji();
            LogManager.Log($"工作状态下拖动，冷却中，拒绝休息");
            return;
        }

        if (IsDragging == false)
        {
            IsDragging = true;

            //主角在工作时，按住左键向左或右拖动一段距离，即可让主角按鼠标滑动方向弹开去休息；30%概率受到拒绝（弹出“勿扰”、“嘘声”等心情气泡）\
            float value = Random.value;
            m_IsRefused = value < RefusedRate;
            LogManager.Log($"工作状态下拖动,随机值:{value},是否拒绝:{m_IsRefused}");
            if (m_IsRefused)
            {
                //显示拒绝表情
                characterController.m_PEmojiLogicInstance.ShowRefuseEmoji();
                refusedCooldownTimer = Time.time + RefusedCooldown;
                LogManager.Log($"工作状态下拖动，拒绝休息");
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
        //todo:当移动停止后,或者3秒后,切换到休闲状态
        characterController.stateMachine.Context.WorkStateChangeToIdleStateDirector = direction * moveForce;
        characterController.stateMachine.ChangeState(characterController.idleState);

        LogManager.Log($"工作状态下鼠标拖拽,方向:" + direction);
    }
}
