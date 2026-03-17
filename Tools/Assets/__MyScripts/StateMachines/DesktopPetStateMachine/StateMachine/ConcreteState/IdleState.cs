
using UnityEngine;

/// <summary>
/// 休闲状态
/// 进入该状态后,角色会随机选择一种休闲活动进行
/// 随机一个休闲动作 喝咖啡、玩手机、看书、发呆
/// 播放对应动作的动画
/// </summary>
public class IdleState : State
{
    private IdleStateSOBase currentState;
    public IdleState(StateMachine stateMachine, Character characterController) : base(stateMachine, characterController)
    {
    }

    public override void AmationTriggerEvent(EAnimationTrigger animationTrigger)
    {
        base.AmationTriggerEvent(animationTrigger);
        currentState.AnimationTriggerEvent(animationTrigger);
    }

    public override void Enter()
    {
        base.Enter();
        // 随机选择一种休闲活动,然后更新状态逻辑实例
        bool isChangeToEat = stateMachine.Context.IsChangeToIdleStateByAnimation;
        if (isChangeToEat)
        {
            LogManager.Log("切换到IdleState:直接指定有动作的空闲逻辑");
            currentState = characterController.pIdleStateEatFoodLogicInstance;
        }
        else
        {
            if (Random.value < 0.4)//每次进入休闲时，40%没动作、60%有动作
            {
                LogManager.Log("IdleState:选择没动作的空闲逻辑");
                currentState = characterController.pIdleStateEmptyLogicInstance;
            }
            else
            {
                LogManager.Log("IdleState:选择有动作的空闲逻辑");
                currentState = characterController.pIdleStateEatFoodLogicInstance;
            }
        }

        stateMachine.Context.IsChangeToIdleStateByAnimation = false;//重置
        currentState.Enter();

        LogManager.Log($"进入待机状态");
    }

    public override void Exit()
    {
        base.Exit();
        currentState.Exit();
        LogManager.Log($"退出待机状态");
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        currentState.LogicUpdate();

    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        currentState.PhysicsUpdate();
    }

    public override void OnInteract(EmojiLogic emojiLogic)
    {
        base.OnInteract(emojiLogic);

        emojiLogic.OnShowIdleEmoji();

    }

    public override void OnMouseDownEvent()
    {
        base.OnMouseDownEvent();
        currentState.OnMouseDownEvent();
    }

    public override void OnMouseUpEvent()
    {
        base.OnMouseUpEvent();
        currentState.OnMouseUpEvent();
    }

    public override void OnMouseDragEvent()
    {
        base.OnMouseDragEvent();
        currentState.OnMouseDragEvent();
    }
}
