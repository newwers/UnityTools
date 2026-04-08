using UnityEngine;

namespace StateMachineSystem
{
    public class InteractIdleState : StateBase
    {
        protected new InteractIdleStateSO stateConfig;

        private float idleStartTime;
        private float currentIdleDuration;
        private bool idleTimeElapsed = false;

        public override StateType StateType { get { return StateType.Interact_Idle; } }

        public override void Initialize(StateMachine machine, StateSO config)
        {
            base.Initialize(machine, config);
            stateConfig = config as InteractIdleStateSO;
        }

        public override StateType GetNextState()
        {
            if (idleTimeElapsed && !stateMachine.Current.IsDragging && stateMachine.Current.IsInGround)//时间到,并且不在拖拽状态中,并且在地面,目前只有蜗牛会触发
            {
                return StateType.Interact_Exit;
            }
            return StateType.Interact_Idle;
        }

        public override void Enter()
        {
            base.Enter();

            // 重置所有动画参数
            stateMachine.ResetAllAnimatorBools();

            // 播放交互 idle 动画
            stateMachine.SetAnimatorBool(stateConfig.animationParameterName, true);

            // 记录开始时间
            idleStartTime = Time.time;

            // 随机生成 idle 时间
            currentIdleDuration = Random.Range(stateConfig.minIdleTime, stateConfig.maxIdleTime);

            // 标记时间未结束
            idleTimeElapsed = false;

            // 进入交互 idle 状态的逻辑
        }

        public override void Exit()
        {
            base.Exit();

            // 重置动画参数
            stateMachine.SetAnimatorBool(stateConfig.animationParameterName, false);

            // 退出交互 idle 状态的逻辑
        }

        public override void Update()
        {
            base.Update();

            // 检查 idle 时间是否结束
            if (!idleTimeElapsed && Time.time - idleStartTime >= currentIdleDuration)
            {
                idleTimeElapsed = true;
            }
        }
        
#if UNITY_EDITOR        
        public override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();

            if (stateMachine != null && stateConfig != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(stateMachine.transform.position + new Vector3(0, stateMachine.Current.animalData.collisionCheckRadius / 2, 0), stateMachine.Current.animalData.collisionCheckRadius);
            }
        }
#endif  
    }
  
}