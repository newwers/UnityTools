﻿using UnityEngine;

namespace StateMachineSystem
{
    public class FlyStartState : InteractEnterState
    {
        protected new FlyStartStateSO stateConfig;

        private float startTime;
        private Vector3 initialPosition;
        private int flightDirection;
        private float sCurveFactor;
        private float sCurveOffset;
        private float groundHeight;

        public override void Initialize(StateMachine machine, StateSO config)
        {
            base.Initialize(machine, config);
            stateConfig = config as FlyStartStateSO;
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

            startTime = Time.time;
            initialPosition = stateMachine.transform.position;

            // 保存飞行起始位置到Blackboard，供FlyEndState使用
            if (stateMachine.blackboard != null)
            {
                stateMachine.blackboard.flyInitialPosition = initialPosition;
            }

            // 保存初始飞行方向
            SpriteRenderer spriteRenderer = stateMachine.GetSpriteRenderer();
            if (spriteRenderer != null)
            {
                // 如果flipX为true，说明朝左，否则朝右
                flightDirection = spriteRenderer.flipX ? -1 : 1;
            }
            else
            {
                flightDirection = 1; // 默认朝右
            }

            // 初始化S型曲线参数
            sCurveFactor = Random.Range(0.5f, 1.5f);
            sCurveOffset = Random.Range(0f, Mathf.PI * 2);

            // 限制高度，确保不低于地板
            groundHeight = GetGroundHeight();
        }

        public override void Update()
        {
            base.Update();

            Fly2();
        }

        void Fly2()
        {
            // 计算飞行起始轨迹
            float elapsedTime = Time.time - startTime;
            float progress = Mathf.Clamp01(elapsedTime / stateConfig.animationDuration);

            // 计算向前移动的距离
            float forwardDistance = progress * stateConfig.forwardDistance;

            // 使用正弦函数和余弦函数的组合生成S型曲线
            float heightOffset = Mathf.Sin(progress * Mathf.PI) * stateConfig.maxHeight;
            float sCurveEffect = Mathf.Sin(progress * Mathf.PI * 2 * sCurveFactor + sCurveOffset) * stateConfig.maxHeight * 0.3f;

            // 组合上下起伏和S型效果
            float totalHeightOffset = heightOffset + sCurveEffect;


            float minY = groundHeight; // 保持在地板上方一定距离

            // 更新位置
            Vector3 newPosition = initialPosition;
            newPosition.x += flightDirection * forwardDistance;
            newPosition.y = Mathf.Max(initialPosition.y + totalHeightOffset, minY);
            stateMachine.transform.position = newPosition;
        }

        void Fly1()
        {
            // 计算飞行起始轨迹
            float elapsedTime = Time.time - startTime;
            float progress = Mathf.Clamp01(elapsedTime / stateConfig.animationDuration);

            // 计算向前移动的距离
            float forwardDistance = progress * stateConfig.forwardDistance;

            // 使用正弦曲线计算高度变化（只取正值部分）
            float sinValue = Mathf.Sin(progress * Mathf.PI); // 0到π的正弦值，范围0到1到0
            float heightOffset = sinValue * stateConfig.maxHeight;

            // 更新位置
            Vector3 newPosition = initialPosition;
            newPosition.x += flightDirection * forwardDistance;
            newPosition.y += heightOffset;
            //newPosition.y = Mathf.Max(initialPosition.y + totalHeightOffset, minY);
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
    }
}