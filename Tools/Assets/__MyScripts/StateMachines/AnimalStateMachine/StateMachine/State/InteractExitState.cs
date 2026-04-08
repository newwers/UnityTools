﻿﻿using UnityEngine;

namespace StateMachineSystem
{
    public class InteractExitState : StateBase
    {
        protected new InteractExitStateSO stateConfig;

        private bool animationPlayed = false;

        public override StateType StateType { get { return StateType.Interact_Exit; } }

        public override void Initialize(StateMachine machine, StateSO config)
        {
            base.Initialize(machine, config);
            stateConfig = config as InteractExitStateSO;
        }

        public override StateType GetNextState()
        {
            if (animationPlayed)
            {
                // 随机决定切换到 Idle 或 Walk 状态
                int randomValue = Random.Range(0, 100);
                if (randomValue < stateConfig.exitToIdleProbability)
                {
                    return StateType.Idle;
                }
                else
                {
                    return StateType.Walk;
                }
            }
            return StateType.Interact_Exit;
        }

        public override void Enter()
        {
            base.Enter();

            // 重置所有动画参数
            stateMachine.ResetAllAnimatorBools();

            // 播放交互退出动画
            stateMachine.SetAnimatorBool(stateConfig.animationParameterName, true);

            // 标记动画开始播放
            animationPlayed = false;

            // 进入交互退出状态的逻辑
        }

        public override void Exit()
        {
            base.Exit();

            // 重置动画参数
            stateMachine.SetAnimatorBool(stateConfig.animationParameterName, false);

            // 退出交互退出状态的逻辑
        }

        public override void Update()
        {
            base.Update();

            // 检查动画是否播放完成
            if (!animationPlayed && stateMachine.animator != null)
            {
                AnimatorStateInfo stateInfo = stateMachine.animator.GetCurrentAnimatorStateInfo(0);
                if (stateInfo.IsName("Interact_Exit") && stateInfo.normalizedTime >= 1.0f)
                {
                    animationPlayed = true;
                }
            }
        }
    }
}