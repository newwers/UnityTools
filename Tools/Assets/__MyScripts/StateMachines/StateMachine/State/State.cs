using System;

/// <summary>
/// 动画触发器事件类型
/// </summary>
public enum EAnimationTrigger
{
    EnterState,
    ExitState,
    Action1,
    Action2,
    Action3
}

public class State
{
    protected StateMachine stateMachine;
    protected Character characterController;

    public State(StateMachine stateMachine, Character characterController)
    {
        this.stateMachine = stateMachine ?? throw new ArgumentNullException(nameof(stateMachine));
        this.characterController = characterController ?? throw new ArgumentNullException(nameof(characterController));
    }

    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void LogicUpdate() { }
    public virtual void PhysicsUpdate() { }
    /// <summary>
    /// 动画触发器事件
    /// </summary>
    /// <param name="animationTrigger"></param>
    public virtual void AmationTriggerEvent(EAnimationTrigger animationTrigger) { }
    /// <summary>
    /// 鼠标点击交互
    /// </summary>
    /// <param name="emojiLogic"></param>
    public virtual void OnInteract(EmojiLogic emojiLogic)
    {

    }


    public virtual void OnMouseDownEvent()
    {
    }

    public virtual void OnMouseUpEvent()
    {
    }

    public virtual void OnMouseDragEvent()
    {

    }
}
