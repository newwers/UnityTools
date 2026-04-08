﻿﻿﻿using UnityEngine;

namespace StateMachineSystem
{
    public class LadybugFlyState : InteractIdleState
    {
        protected new LadybugFlyStateSO stateConfig;

        private float flyStartTime;
        private float currentFlyDuration;
        private bool flyTimeElapsed = false;

        private Vector3 targetPosition;
        private float flightTime;


        private float sCurveFactor;
        private float sCurveOffset;
        private float groundHeight;

        private float lastStayCheckTime;
        private GameObject currentCollidedObject;

        public override void Initialize(StateMachine machine, StateSO config)
        {
            base.Initialize(machine, config);
            stateConfig = config as LadybugFlyStateSO;
        }

        public override StateType GetNextState()
        {
            if (flyTimeElapsed)
            {
                return StateType.Interact_Exit;
            }
            return StateType.Interact_Idle;
        }


        public override void Enter()
        {
            base.Enter();

            flyStartTime = Time.time;
            currentFlyDuration = Random.Range(stateConfig.minFlyTime, stateConfig.maxFlyTime);
            flyTimeElapsed = false;

            flightTime = 0f;

            // 初始化X轴移动
            GenerateRandomTargetPosition();

            // 初始化S型曲线参数
            sCurveFactor = Random.Range(0.5f, 1.5f);
            sCurveOffset = Random.Range(0f, Mathf.PI * 2);

            // 限制高度，确保不低于地板
            groundHeight = GetGroundHeight();
        }

        public override void Update()
        {
            base.Update();

            // 检查飞行时间是否结束
            if (!flyTimeElapsed && Time.time - flyStartTime >= currentFlyDuration)
            {
                flyTimeElapsed = true;
            }

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
                    if (randomValue < stateConfig.FlyChangeToStayProbability)
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
            if (stateMachine.Current != null)
            {
                stateMachine.Current.IsInObject = false;
            }
        }

        private void CalculateFlightPath()
        {
            // 使用正弦函数和余弦函数的组合生成S型曲线
            float heightOffset = Mathf.Sin(flightTime * stateConfig.flightFrequency + sCurveOffset) * stateConfig.maxFlightHeight;
            float sCurveEffect = Mathf.Sin(flightTime * stateConfig.flightFrequency * sCurveFactor) * stateConfig.maxFlightHeight * 0.5f;

            // 组合上下起伏和S型效果
            float totalHeightOffset = heightOffset + sCurveEffect;

            // 限制高度，确保不低于地板
            float minY = groundHeight; // 保持在地板上方一定距离

            // 更新位置
            Vector3 currentPosition = stateMachine.transform.position;
            Vector3 newPosition = currentPosition;

            // 计算目标高度，基于当前位置而不是初始位置
            float targetY = currentPosition.y + totalHeightOffset * Time.deltaTime * stateConfig.flightFrequency;
            newPosition.y = Mathf.Max(targetY, minY);

            stateMachine.transform.position = newPosition;
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

            // 控制角色朝向
            SpriteRenderer spriteRenderer = stateMachine.GetSpriteRenderer();
            if (spriteRenderer != null)
            {
                // 计算移动方向
                float moveDirection = targetPosition.x - currentPosition.x;

                // 根据移动方向设置朝向
                if (moveDirection > 0)
                {
                    spriteRenderer.flipX = false;
                }
                else if (moveDirection < 0)
                {
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
#if UNITY_EDITOR
        public override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();
            if (stateMachine)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(targetPosition, 0.1f);
                Gizmos.DrawLine(stateMachine.transform.position, targetPosition);
            }
        }
#endif
    }
}