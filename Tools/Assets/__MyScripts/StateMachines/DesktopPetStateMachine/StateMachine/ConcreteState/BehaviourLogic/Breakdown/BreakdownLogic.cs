
using UnityEngine;

[CreateAssetMenu(fileName = "BreakdownLogic", menuName = "状态机/崩溃/崩溃逻辑")]
public class BreakdownLogic : BreakdownStateSOBase
{
    public float breakdownDuration = 5f; // 崩溃状态持续时间
    public Sprite BreakdownStateIcon; // 崩溃状态的图标
    private float breakdownTimer; // 计时器
    private Character characterController;

    public override void AnimationTriggerEvent(EAnimationTrigger animationTrigger)
    {
        base.AnimationTriggerEvent(animationTrigger);
    }

    public override void Enter()
    {
        base.Enter();
        // 切换到崩溃动画
        characterController.SetSprite(BreakdownStateIcon);
        breakdownTimer = Time.time + breakdownDuration;
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Initialize(Character character)
    {
        base.Initialize(character);
        characterController = character;
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        if (Time.time >= breakdownTimer)
        {
            // 时间到，切换到休闲状态
            characterController.stateMachine.ChangeState(characterController.idleState);
            characterController.AddStress(-characterController.MaxStressLevel);//重置压力值
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
}
