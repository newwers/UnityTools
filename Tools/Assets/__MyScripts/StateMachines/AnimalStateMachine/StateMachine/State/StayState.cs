﻿
/*
 目前只有蝴蝶和瓢虫会有stay状态, 而且蝴蝶重写了Stay逻辑, 所以这个StayState只会被瓢虫使用
 */
using UnityEngine;

namespace StateMachineSystem
{
    public class StayState : StateBase
    {
        protected new StayStateSO stateConfig;

        private float stayStartTime;
        private float currentStayDuration;
        private bool stayTimeElapsed = false;
        private DragObjectController dragObjectController;

        public override StateType StateType { get { return StateType.Stay; } }

        public override void Initialize(StateMachine machine, StateSO config)
        {
            base.Initialize(machine, config);
            stateConfig = config as StayStateSO;
        }

        public override StateType GetNextState()
        {
            if (stayTimeElapsed)
            {
                // 时间到，50概率进入飞行状态
                int randomValue = Random.Range(0, 100);
                if (randomValue < 50)
                {
                    // 进入飞行状态
                    stateMachine.SetState(StateType.Interact_Enter);
                }
                else
                {
                    // 直接掉落，进入下落状态
                    stateMachine.SetState(StateType.Interact_Exit);
                }
            }
            return StateType.Stay;
        }

        public override void Enter()
        {
            base.Enter();

            // 重置所有动画参数
            stateMachine.ResetAllAnimatorBools();

            // 播放Stay动画
            if (stateConfig)
                stateMachine.SetAnimatorBool(stateConfig.animationParameterName, true);

            // 初始化停留时间
            stayStartTime = Time.time;
            if (stateConfig)
                currentStayDuration = Random.Range(stateConfig.minStayTime, stateConfig.maxStayTime);
            else
                currentStayDuration = 5f; // 默认停留时间
            stayTimeElapsed = false;

            // 检查当前碰撞物体是否有DragObjectController组件
            GameObject currentObject = stateMachine.GetCurrentCollidedObject();
            if (currentObject != null)
            {
                dragObjectController = currentObject.GetComponent<DragObjectController>();
                if (dragObjectController != null)
                {
                    // 添加OnBeginDrag事件监听
                    dragObjectController.OnBeginDrag += OnStayObjectBeginDragHandler;
                    LogManager.Log($"添加{currentObject.name}:OnBeginDrag事件监听");
                }
            }
        }

        public override void Exit()
        {
            base.Exit();

            // 重置Stay动画参数
            if (stateConfig)
            {
                stateMachine.SetAnimatorBool(stateConfig.animationParameterName, false);
            }

            // 取消OnBeginDrag事件监听
            if (dragObjectController != null)
            {
                LogManager.Log($"移除{dragObjectController.name}:OnBeginDrag事件监听");
                dragObjectController.OnBeginDrag -= OnStayObjectBeginDragHandler;
                dragObjectController = null;
            }
        }

        public override void Update()
        {
            base.Update();

            // 检查停留时间是否结束
            if (!stayTimeElapsed && Time.time - stayStartTime >= currentStayDuration)
            {
                stayTimeElapsed = true;
            }
        }

        // OnBeginDrag事件处理方法
        private void OnStayObjectBeginDragHandler()
        {
            // 切换到Interact_Enter状态
            LogManager.Log($"触发{dragObjectController.name}:OnBeginDragHandler事件!!!!");
            stateMachine.SetState(StateType.Interact_Enter);
        }


        /// <summary>
        /// 拖拽的时候如果碰撞到物体
        /// </summary>
        /// <param name="collision"></param>
        public void OnDraggingTriggerEnter2D(Collider2D collider)
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

            // 设置蝴蝶在物体中的标记
            if (stateMachine.Current != null)
            {
                stateMachine.Current.IsInObject = true;
                stateMachine.SetCurrentCollidedObject(collider.gameObject);
            }
        }

        public void OnDraggingTriggerExit2D(Collider2D collision)
        {
            // 当蝴蝶离开碰撞器时，重置在物体中的标记
            if (stateMachine.Current != null)
            {
                stateMachine.Current.IsInObject = false;
            }
        }
    }
}