using UnityEngine;

public class BreakdownStateSOBase : ScriptableObject
{
    public virtual void Initialize(Character character) { }

    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void LogicUpdate() { }
    public virtual void PhysicsUpdate() { }
    public virtual void AnimationTriggerEvent(EAnimationTrigger animationTrigger) { }
    public virtual void ResetValues() { }
}
