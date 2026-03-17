
// 状态机上下文，用于在状态间传递数据
using UnityEngine;

public class StateMachineContext
{
    /// <summary>
    /// 休闲状态切换到工作状态时的目标方向
    /// </summary>
    public Vector2 WorkStateChangeToIdleStateDirector;
    /// <summary>
    ///  是否切换到休闲动画状态
    /// </summary>
    public bool IsChangeToIdleStateByAnimation = false;
}

public class StateMachine
{
    public State CurrentState { get; private set; }
    public StateMachineContext Context { get; private set; }

    public void Initialize(State startingState, StateMachineContext context)
    {
        Context = context ?? new StateMachineContext();
        CurrentState = startingState ?? throw new System.ArgumentNullException(nameof(startingState));
        CurrentState.Enter();
    }

    public void ChangeState(State newState)
    {
        if (newState == null) throw new System.ArgumentNullException(nameof(newState));
        CurrentState.Exit();
        CurrentState = newState;
        CurrentState.Enter();
    }
}
