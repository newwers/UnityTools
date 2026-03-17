using UnityEngine;

namespace StateMachineSystem
{
    public class IdleState : StateBase
    {
        protected new IdleStateSO stateConfig;

        public override StateType StateType { get { return StateType.Idle; } }

        public override void Initialize(StateMachine machine, StateSO config)
        {
            base.Initialize(machine, config);
            stateConfig = config as IdleStateSO;
        }

        public override StateType GetNextState()
        {
            int randomValue = Random.Range(0, 100);

            if (randomValue < stateConfig.idleToWalkProbability)
            {
                return StateType.Walk;
            }

            return StateType.Idle;
        }

        public override void Enter()
        {
            base.Enter();

            // 重置所有动画参数
            stateMachine.ResetAllAnimatorBools();

            // 播放Lie动画
            stateMachine.SetAnimatorBool("Idle", true);

            // 进入趴状态的逻辑
        }

        public override void Exit()
        {
            base.Exit();

            // 重置Lie动画参数
            stateMachine.SetAnimatorBool("Idle", false);

            // 退出趴状态的逻辑
        }
    }
}
