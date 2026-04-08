using UnityEngine;

namespace StateMachineSystem
{
    public class InteractEnterState : StateBase
    {
        protected new InteractEnterStateSO stateConfig;

        protected bool animationPlayed = false;

        public override StateType StateType { get { return StateType.Interact_Enter; } }

        public override void Initialize(StateMachine machine, StateSO config)
        {
            base.Initialize(machine, config);
            stateConfig = config as InteractEnterStateSO;
        }

        public override StateType GetNextState()
        {
            if (animationPlayed)
            {
                return StateType.Interact_Idle;
            }
            return StateType.Interact_Enter;
        }

        public override void Enter()
        {
            base.Enter();

            // 重置所有动画参数
            stateMachine.ResetAllAnimatorBools();

            // 播放交互进入动画
            stateMachine.SetAnimatorBool(stateConfig.animationParameterName, true);

            // 标记动画开始播放
            animationPlayed = false;

            // 进入交互进入状态的逻辑
        }

        public override void Exit()
        {
            base.Exit();

            // 重置动画参数
            stateMachine.SetAnimatorBool(stateConfig.animationParameterName, false);

            // 退出交互进入状态的逻辑
        }

        public override void Update()
        {
            base.Update();

            // 检查动画是否播放完成
            // 这里简化处理，实际项目中可能需要通过动画事件或状态机来检测动画结束
            // 假设动画播放时间为2秒，2秒后标记为播放完成
            if (!animationPlayed && stateMachine.animator != null)
            {
                AnimatorStateInfo stateInfo = stateMachine.animator.GetCurrentAnimatorStateInfo(0);
                if (stateInfo.IsName("Interact_Enter") && stateInfo.normalizedTime >= 1.0f)
                {
                    animationPlayed = true;
                }
            }
        }
    }
}