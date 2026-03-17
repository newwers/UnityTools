/// <summary>
/// 工作状态
/// 角色在电脑前工作
/// 每隔一段时间增加压力值,压力值高到一定程度会进入崩溃状态
/// 每隔一段时间增加金币
/// </summary>
public class WorkState : State
{

    public WorkState(StateMachine stateMachine, Character characterController) : base(stateMachine, characterController)
    {

    }

    public override void AmationTriggerEvent(EAnimationTrigger animationTrigger)
    {
        base.AmationTriggerEvent(animationTrigger);
        characterController.pWorkLogicInstance.AnimationTriggerEvent(animationTrigger);
    }

    public override void Enter()
    {
        base.Enter();
        LogManager.Log($"进入工作状态");
        characterController.pWorkLogicInstance.Enter();
    }

    public override void Exit()
    {
        base.Exit();
        LogManager.Log($"退出工作状态");
        characterController.pWorkLogicInstance.Exit();
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        characterController.pWorkLogicInstance.LogicUpdate();

    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        characterController.pWorkLogicInstance.PhysicsUpdate();
    }


    public override void OnInteract(EmojiLogic emojiLogic)
    {
        base.OnInteract(emojiLogic);

        emojiLogic.OnShowWorkEmoji();

    }

    public override void OnMouseDownEvent()
    {
        base.OnMouseDownEvent();
        characterController.pWorkLogicInstance.OnMouseDownEvent();
    }

    public override void OnMouseUpEvent()
    {
        base.OnMouseUpEvent();
        characterController.pWorkLogicInstance.OnMouseUpEvent();
    }

    public override void OnMouseDragEvent()
    {
        base.OnMouseDragEvent();
        characterController.pWorkLogicInstance.OnMouseDragEvent();
    }

}
