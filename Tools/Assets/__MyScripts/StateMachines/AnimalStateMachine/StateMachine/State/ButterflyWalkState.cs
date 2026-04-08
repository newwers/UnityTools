﻿using UnityEngine;

namespace StateMachineSystem
{
    public class ButterflyWalkState : StateBase
    {
        protected new ButterflyWalkStateSO stateConfig;

        private Vector3 targetPosition;
        private float flightTime;
        private float sCurveFactor;
        private float sCurveOffset;
        private float groundHeight;
        private float flightDirection = 1; // 1为向右，-1为向左
        private float lastStayCheckTime;
        private GameObject currentCollidedObject;

        public override StateType StateType { get { return StateType.Walk; } }

        public override void Initialize(StateMachine machine, StateSO config)
        {
            base.Initialize(machine, config);
            stateConfig = config as ButterflyWalkStateSO;
        }

        public override StateType GetNextState()
        {
            return StateType.Walk;
        }

        public override void Enter()
        {
            base.Enter();

            // 重置所有动画参数
            stateMachine.ResetAllAnimatorBools();

            // 播放飞行动画
            stateMachine.SetAnimatorBool("Walk", true);

            // 初始化飞行相关字段
            flightTime = 0f;
            GenerateRandomTargetPosition();

            // 初始化S型曲线参数
            sCurveFactor = Random.Range(0.5f, 1.5f);
            sCurveOffset = Random.Range(0f, Mathf.PI * 2);


            // 限制高度，确保不低于地板
            groundHeight = GetGroundHeight();
        }

        public override void Exit()
        {
            base.Exit();

            // 重置飞行动画参数
            stateMachine.SetAnimatorBool("Walk", false);
        }

        public override void Update()
        {
            base.Update();

            // 更新飞行时间
            flightTime += Time.deltaTime;

            // 计算飞行轨迹
            CalculateFlightPath();

            // 处理X轴移动
            HandleXMovement();

            // 当蝴蝶在物体中时，每隔指定时间检查一次是否停留
            if (stateMachine.Current != null && stateMachine.Current.IsInObject)
            {
                if (Time.time - lastStayCheckTime >= stateConfig.stayCheckInterval)
                {
                    // 有指定概率切换到Stay状态
                    int randomValue = Random.Range(0, 100);
                    if (randomValue < stateConfig.WalkChangeToStayProbability)
                    {
                        // 将当前碰撞物体信息传递给StateMachine
                        stateMachine.SetCurrentCollidedObject(currentCollidedObject);
                        stateMachine.SetState(StateType.Stay);
                        if (stateMachine.Current != null)
                        {
                            stateMachine.Current.IsInObject = false;
                        }
                    }
                    lastStayCheckTime = Time.time;
                }
            }
        }

        private void CalculateFlightPath()
        {
            // 使用正弦函数和余弦函数的组合生成蝴蝶特有的S型曲线
            float heightOffset = Mathf.Sin(flightTime * stateConfig.flightFrequency + sCurveOffset) * stateConfig.maxFlightHeight;
            float sCurveEffect = Mathf.Sin(flightTime * stateConfig.flightFrequency * sCurveFactor) * stateConfig.maxFlightHeight * 0.5f;

            // 组合上下起伏和S型效果
            float totalHeightOffset = heightOffset + sCurveEffect;

            // 限制高度，确保不低于地板
            float minY = groundHeight + stateConfig.goundHeight; // 保持在地板上方一定距离

            // 更新位置
            Vector3 currentPosition = stateMachine.transform.position;
            Vector3 newPosition = currentPosition;

            // 计算目标高度，基于当前位置
            float targetY = currentPosition.y + totalHeightOffset * Time.deltaTime * stateConfig.flightFrequency;
            newPosition.y = Mathf.Max(targetY, minY);

            stateMachine.transform.position = newPosition;
        }

        private void HandleXMovement()
        {
            // 向目标位置移动
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

            flightDirection = targetPosition.x - currentPosition.x;

            // 控制角色朝向
            SpriteRenderer spriteRenderer = stateMachine.GetSpriteRenderer();
            if (spriteRenderer != null)
            {
                // 根据飞行方向设置朝向
                if (flightDirection > 0)
                {
                    // 向右移动，设置flipX为false
                    spriteRenderer.flipX = false;
                }
                else if (flightDirection < 0)
                {
                    // 向左移动，设置flipX为true
                    spriteRenderer.flipX = true;
                }
            }
        }

        private void GenerateRandomTargetPosition()
        {
            // 检查是否启用屏幕边界限制
            bool useScreenBounds = stateMachine != null && stateMachine.blackboard != null && stateMachine.blackboard.isRandomWalkWithinScreenBounds;

            if (useScreenBounds)
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
                    UseConfiguredRange();
                }
            }
            else
            {
                // 不使用屏幕边界限制，使用配置的移动范围
                UseConfiguredRange();
            }
        }

        private void UseConfiguredRange()
        {
            // 尝试使用MainGround的范围
            if (WoodBox.HasInstance && WoodBox.Instance.MainGround != null)
            {
                Vector2 groundRange = WoodBox.Instance.MainGround.GetGroundXRange();
                float randomX = Random.Range(groundRange.x, groundRange.y);
                targetPosition = new Vector3(randomX, stateMachine.transform.position.y, stateMachine.transform.position.z);
            }
            else
            {
                // 如果没有MainGround，使用配置的移动范围
                float randomX = Random.Range(-stateConfig.moveRangeX / 2, stateConfig.moveRangeX / 2);
                targetPosition = new Vector3(randomX, stateMachine.transform.position.y, stateMachine.transform.position.z);
            }
        }

        public void OnTriggerEnter2D(Collider2D collider)
        {
            // 忽略自己的碰撞器
            if (collider.gameObject == stateMachine.gameObject)
            {
                return;
            }

            // 忽略AnimalIgnoreCollider
            if (collider.CompareTag("AnimalIgnoreCollider"))
            {
                return;
            }

            //忽略MainGround层
            if (collider.gameObject.layer == LayerMask.NameToLayer("MainGround"))
                return;

            //忽略没有DragObjectController组件的物体
            if (collider.GetComponent<DragObjectController>() == null)
                return;

            // 记录当前碰撞的物体
            currentCollidedObject = collider.gameObject;

            // 设置蝴蝶在物体中的标记
            if (stateMachine.Current != null)
            {
                stateMachine.Current.IsInObject = true;
            }
            lastStayCheckTime = Time.time;
        }


        public void OnTriggerExit2D(Collider2D collision)
        {
            // 当蝴蝶离开碰撞器时，重置在物体中的标记
            if (stateMachine.Current != null)
            {
                stateMachine.Current.IsInObject = false;
            }
        }

        private float GetGroundHeight()
        {
            // 尝试获取MainGround的高度
            if (WoodBox.HasInstance && WoodBox.Instance.MainGround != null)
            {
                return WoodBox.Instance.MainGround.GetGroundHeight();
            }
            // 如果没有MainGround，返回一个默认值
            return -2f;
        }

        // 处理点击调换朝向
        public void ToggleFlightDirection()
        {

            for (int i = 0; i < 5; i++)//循环5次随机位置,取反方向
            {
                // 更新目标位置，确保新的朝向有目标
                GenerateRandomTargetPosition();
                var newFlightDirection = targetPosition.x - stateMachine.transform.position.x;
                bool isSameSign = flightDirection * newFlightDirection > 0;
                if (isSameSign == false)
                {
                    break;
                }
            }

        }
#if UNITY_EDITOR
        // 在场景中绘制飞行目标点和轨迹
        public override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();

            if (stateMachine != null)
            {
                // 绘制目标点
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(targetPosition, 0.2f);
                Gizmos.DrawLine(stateMachine.transform.position, targetPosition);

            }
        }
#endif
    }
}