/// <summary>
/// 崩溃状态
/// 进入后切换奔溃动画
/// 持续一段时间后切换到休闲状态?
/// 角色无法被操作和交互?
/// </summary>
public class BreakdownState : State
{


    public BreakdownState(StateMachine stateMachine, Character characterController) : base(stateMachine, characterController)
    {

    }

    public override void AmationTriggerEvent(EAnimationTrigger animationTrigger)
    {
        base.AmationTriggerEvent(animationTrigger);
    }

    public override void Enter()
    {
        base.Enter();
        characterController.pBreakdownLogicInstance.Enter();

        LogManager.Log($"进入奔溃状态");
    }

    public override void Exit()
    {
        base.Exit();
        characterController.pBreakdownLogicInstance.Exit();
        LogManager.Log($"退出奔溃状态");
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        characterController.pBreakdownLogicInstance.LogicUpdate();

    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        characterController.pBreakdownLogicInstance.PhysicsUpdate();
    }

    public override void OnInteract(EmojiLogic emojiLogic)
    {
        base.OnInteract(emojiLogic);

        emojiLogic.OnShowBreakdownEmoji();

    }
}
