using UnityEngine;

public class IdleStateSOBase : ScriptableObject
{
    public virtual void Initialize(Character character) { }

    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void LogicUpdate() { }
    public virtual void PhysicsUpdate() { }
    public virtual void AnimationTriggerEvent(EAnimationTrigger animationTrigger) { }
    public virtual void ResetValues() { }

    public virtual void OnMouseDownEvent()
    {
    }

    public virtual void OnMouseUpEvent()
    {
    }

    public virtual void OnMouseDragEvent()
    {

    }

    //是否达到条件
    public virtual bool IsConditionMet()
    {
        return true;
    }
    /// <summary>
    /// 满足条件后,如何切换状态函数
    /// </summary>
    protected virtual void ChangeStateEvent()
    {

    }
}
