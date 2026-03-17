
using UnityEngine;
/// <summary>
/// 喝咖啡、玩手机、看书、发呆只是播放动画不一样,逻辑是一样的
/// </summary>
[CreateAssetMenu(fileName = "IdleStateEmptyLogic", menuName = "状态机/休闲/无动作逻辑")]
public class IdleStateEmptyLogic : IdleStateChangeStateByTime
{

    //[Header("休闲状态的图标")]
    //public Sprite IdleStateIcon; // 

    public override void Initialize(Character character)
    {
        base.Initialize(character);
    }

    public override void Enter()
    {
        base.Enter();

        //播放对应动画
        //characterController.SetSprite(IdleStateIcon);
    }



    public override void Exit()
    {
        base.Exit();
    }


    public override void LogicUpdate()
    {
        base.LogicUpdate();
    }



    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
    }

    public override void ResetValues()
    {
        base.ResetValues();
    }


    public override void AnimationTriggerEvent(EAnimationTrigger animationTrigger)
    {
        base.AnimationTriggerEvent(animationTrigger);
    }

    protected override void ChangeStateEvent()
    {
        //各50%概率：A-回去工作；B-进行随机一种休闲活动
        bool toWork = Random.value < 0.5f;
        LogManager.Log("空闲待机状态,是否切换到工作状态:" + toWork);
        if (toWork)
        {
            characterController.stateMachine.ChangeState(characterController.workingState);
        }
        else
        {
            characterController.stateMachine.Context.IsChangeToIdleStateByAnimation = true;
            characterController.stateMachine.ChangeState(characterController.idleState);
        }
    }
}
