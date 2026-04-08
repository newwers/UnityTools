using UnityEngine;

namespace StateMachineSystem
{
    public class WalkState : StateBase
    {
        protected new WalkStateSO stateConfig;

        private Vector3 targetPosition;

        public override StateType StateType { get { return StateType.Walk; } }

        public override void Initialize(StateMachine machine, StateSO config)
        {
            base.Initialize(machine, config);
            stateConfig = config as WalkStateSO;
        }

        public override StateType GetNextState()
        {
            int randomValue = Random.Range(0, 100);

            if (randomValue < stateConfig.walkToIdleProbability)
            {
                return StateType.Idle;
            }

            return StateType.Walk;
        }

        public override void Enter()
        {
            base.Enter();

            // 重置所有动画参数
            stateMachine.ResetAllAnimatorBools();

            // 播放Walk动画
            stateMachine.SetAnimatorBool("Walk", true);

            // 初始化移动相关字段
            GenerateRandomTargetPosition();

            // 进入行走状态的逻辑
        }

        private void GenerateRandomTargetPosition()
        {
            // 计算屏幕边界
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                // 获取屏幕左右边界
                float screenLeft = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, 0)).x;
                float screenRight = mainCamera.ViewportToWorldPoint(new Vector3(1, 0, 0)).x;

                // 计算移动范围
                float minX = screenLeft + 1f;
                float maxX = screenRight - 1f;

                // 生成随机目标位置
                float randomX = Random.Range(minX, maxX);
                targetPosition = new Vector3(randomX, stateMachine.transform.position.y, stateMachine.transform.position.z);
            }
            else
            {
                // 如果没有相机，使用配置的移动范围
                float randomX = Random.Range(-stateConfig.moveRangeX / 2, stateConfig.moveRangeX / 2);
                targetPosition = new Vector3(randomX, stateMachine.transform.position.y, stateMachine.transform.position.z);
            }
        }

        public override void Exit()
        {
            base.Exit();

            // 重置Walk动画参数
            stateMachine.SetAnimatorBool("Walk", false);

            // 退出行走状态的逻辑
        }

        public override void Update()
        {
            base.Update();

            // 向目标位置移动
            if (stateMachine != null && stateMachine.gameObject != null)
            {
                Vector3 currentPosition = stateMachine.transform.position;
                stateMachine.transform.position = Vector3.MoveTowards(
                    currentPosition,
                    targetPosition,
                    stateConfig.moveSpeed * Time.deltaTime
                );

                // 检查是否到达目标位置附近
                float distanceToTarget = Vector3.Distance(currentPosition, targetPosition);
                if (distanceToTarget <= stateConfig.targetReachDistance)
                {
                    // 到达目标位置附近，生成新的目标位置
                    GenerateRandomTargetPosition();
                }

                // 控制角色朝向
                SpriteRenderer spriteRenderer = stateMachine.GetSpriteRenderer();
                if (spriteRenderer != null)
                {
                    // 计算移动方向
                    float moveDirection = targetPosition.x - currentPosition.x;

                    // 根据移动方向设置朝向
                    if (moveDirection > 0)
                    {
                        // 向右移动，设置flipX为false
                        spriteRenderer.flipX = false;
                    }
                    else if (moveDirection < 0)
                    {
                        // 向左移动，设置flipX为true（保持默认朝向）
                        spriteRenderer.flipX = true;
                    }
                }
            }
        }
    }
}
