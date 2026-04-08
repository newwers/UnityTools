using UnityEngine;

namespace StateMachineSystem
{
    public class ButterflyStayState : StateBase
    {
        protected new ButterflyStayStateSO stateConfig;

        private float stayStartTime;
        private float currentStayDuration;
        private bool stayTimeElapsed = false;
        private float lastAnimationUpdateTime;
        private float currentAnimationUpdateInterval;
        private float lastNormalizedTime;
        private DragObjectController dragObjectController;

        public override StateType StateType { get { return StateType.Stay; } }

        public override void Initialize(StateMachine machine, StateSO config)
        {
            base.Initialize(machine, config);
            stateConfig = config as ButterflyStayStateSO;
        }

        public override StateType GetNextState()
        {
            if (stayTimeElapsed)
            {
                // 时间到，进入飞行状态
                return StateType.Walk;
            }
            return StateType.Stay;
        }

        public override void Enter()
        {
            base.Enter();

            // 重置所有动画参数
            stateMachine.ResetAllAnimatorBools();

            // 播放停留动画
            stateMachine.SetAnimatorBool("Stay", true);

            // 初始化停留时间
            stayStartTime = Time.time;
            currentStayDuration = Random.Range(stateConfig.minStayTime, stateConfig.maxStayTime);
            stayTimeElapsed = false;

            // 初始化动画更新时间
            lastAnimationUpdateTime = Time.time;
            currentAnimationUpdateInterval = Random.Range(stateConfig.minAnimationUpdateTime, stateConfig.maxAnimationUpdateTime);

            // 初始化动画 normalizedTime
            lastNormalizedTime = 0f;

            // 检查当前碰撞物体是否有DragObjectController组件
            GameObject currentObject = stateMachine.GetCurrentCollidedObject();
            if (currentObject != null)
            {
                dragObjectController = currentObject.GetComponent<DragObjectController>();
                if (dragObjectController != null)
                {
                    // 添加OnBeginDrag事件监听
                    dragObjectController.OnBeginDrag += OnStayObjectBeginDragHandler;
                }
            }
        }

        public override void Exit()
        {
            base.Exit();

            // 重置Stay动画参数
            stateMachine.SetAnimatorBool("Stay", false);

            // 取消OnBeginDrag事件监听
            if (dragObjectController != null)
            {
                dragObjectController.OnBeginDrag -= OnStayObjectBeginDragHandler;
                dragObjectController = null;
            }
        }

        public override void Update()
        {
            base.Update();

            // 检查动画是否播放完成一遍
            if (stateMachine.animator != null)
            {
                AnimatorStateInfo stateInfo = stateMachine.animator.GetCurrentAnimatorStateInfo(0);
                float currentNormalizedTime = stateInfo.normalizedTime;

                // 当动画从小于1变为大于等于1时，说明播放了一遍
                if (lastNormalizedTime < 1 && currentNormalizedTime >= 1)
                {
                    // 随机设置新的动画播放速度
                    float randomSpeed = Random.Range(stateConfig.minAnimationSpeed, stateConfig.maxAnimationSpeed);
                    //stateMachine.animator.speed = randomSpeed;
                    stateMachine.animator.SetFloat("AnimatorSpeed", randomSpeed);
                }

                // 更新 lastNormalizedTime
                lastNormalizedTime = currentNormalizedTime;
            }

            // 检查停留时间是否结束
            if (!stayTimeElapsed && Time.time - stayStartTime >= currentStayDuration)
            {
                stayTimeElapsed = true;
            }
        }
#if UNITY_EDITOR
        public override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();

            if (stateMachine != null)
            {
                // 计算已经停留的时间
                float elapsedTime = Time.time - stayStartTime;
                // 计算剩余停留时间
                float remainingTime = Mathf.Max(0, currentStayDuration - elapsedTime);

                // 获取当前动画播放速度
                float animationSpeed = 1f;
                if (stateMachine.animator != null)
                {
                    animationSpeed = stateMachine.animator.GetFloat("AnimatorSpeed");
                }

                // 设置文字颜色
                Gizmos.color = Color.white;

                // 计算显示位置（在蝴蝶上方）
                Vector3 displayPosition = stateMachine.transform.position + new Vector3(0, 1f, 0);

                // 绘制停留时间信息
                string stayTimeText = string.Format("Stay Time: {0:F1}s / {1:F1}s", elapsedTime, currentStayDuration);
                UnityEditor.Handles.Label(displayPosition, stayTimeText);

                // 绘制播放速度信息
                string speedText = string.Format("Anim Speed: {0:F2}x", animationSpeed);
                UnityEditor.Handles.Label(displayPosition + new Vector3(0, -0.3f, 0), speedText);
            }
        }

#endif

        // OnBeginDrag事件处理方法
        private void OnStayObjectBeginDragHandler()
        {
            // 切换到Interact_Enter状态
            stateMachine.SetState(StateType.Walk);
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