/*
只有瓢虫才用到这个状态,当瓢虫在空中飞行结束后，进入这个状态进行下落处理。没有水平位移
*/
using UnityEngine;

namespace StateMachineSystem
{
    public class FlyEndState : InteractExitState
    {
        protected new FlyEndStateSO stateConfig;

        private Vector3 initialPosition;
        private Vector3 startPosition;
        private bool isFalling = true;
        private bool hasLanded = false;
        private float fallStartTime;
        private GameObject currentPerchObject;
        private GameObject previousPerchObject;
        private float collisionDetectedTime;
        private bool collisionDetected = false;
        private bool isCollidingWithGround = false;

        public override void Initialize(StateMachine machine, StateSO config)
        {
            base.Initialize(machine, config);
            stateConfig = config as FlyEndStateSO;
        }

        public override StateType GetNextState()
        {
            if (hasLanded)
            {
                // 从Ladybug的IsGround标记获取是否在地板上
                Ladybug ladybug = stateMachine.Current as Ladybug;
                bool isOnGround = ladybug != null && ladybug.IsGround;

                // 如果降落到非地面碰撞器，进入Stay状态
                if (!isOnGround)
                {
                    return StateType.Stay;
                }
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

            startPosition = stateMachine.transform.position;

            // 从Blackboard获取FlyStartState的initialPosition
            if (stateMachine.blackboard != null)
            {
                initialPosition = stateMachine.blackboard.flyInitialPosition;
            }
            else
            {
                // 如果没有获取到，使用当前位置作为默认值
                initialPosition = new Vector3(startPosition.x, stateMachine.transform.position.y, startPosition.z);
            }

            isFalling = true;
            hasLanded = false;
            fallStartTime = Time.time;
            collisionDetected = false;
            isCollidingWithGround = false;
            // 保存上一次的栖息物体
            previousPerchObject = currentPerchObject;
            // 重置当前栖息物体
            currentPerchObject = null;
        }

        public override void Update()
        {
            base.Update();

            if (isFalling && !hasLanded)
            {
                HandleFalling();
            }
        }

        private void HandleFalling()
        {
            // 计算下落速度
            float fallDuration = Time.time - fallStartTime;
            float fallSpeed = stateConfig.fallSpeed * fallDuration;

            // 更新位置
            Vector3 currentPosition = stateMachine.transform.position;
            Vector3 newPosition = currentPosition;
            newPosition.y -= fallSpeed * Time.deltaTime;

            // 检查是否碰到碰撞器
            if (CheckCollision(newPosition, out bool isOnGround))
            {
                Ladybug ladybug = stateMachine.Current as Ladybug;

                // 如果是地板，立即停止下落
                if (isOnGround)
                {
                    hasLanded = true;
                    isFalling = false;
                    //LogManager.Log("瓢虫在地板上立刻停止降落");
                    collisionDetected = false; // 不需要等待0.1秒，直接停止
                }
                // 如果不是地板，标记碰撞检测到并记录时间
                else if (!collisionDetected)
                {
                    collisionDetected = true;
                    collisionDetectedTime = Time.time;
                    //LogManager.Log($"瓢虫降落碰撞到: {currentPerchObject.name}，开始计时");
                }
            }

            // 检查碰撞检测后是否经过了0.1秒
            if (collisionDetected && Time.time - collisionDetectedTime >= stateConfig.collisionCheckDelayTime)
            {
                hasLanded = true;
                isFalling = false;
            }

            // 如果下落时间超过60秒，强制落到地板上
            if (fallDuration > 60f)
            {
                // 获取地板高度
                float groundHeight = GetGroundHeight();
                
                // 设置位置到地板高度
                newPosition.y = groundHeight;
                stateMachine.transform.position = newPosition;
                
                // 标记已经落在地板上
                hasLanded = true;
                isFalling = false;
                isCollidingWithGround = true;
            }

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

        private bool CheckCollision(Vector3 position, out bool isOnGround)
        {
            isOnGround = false;
            // 检测下落位置是否有碰撞器
            Collider2D[] colliders = Physics2D.OverlapCircleAll(position + new Vector3(0, stateConfig.collisionCheckRadius / 2, 0), stateConfig.collisionCheckRadius);

            foreach (Collider2D collider in colliders)
            {
                // 忽略自己的碰撞器
                if (collider.gameObject == stateMachine.gameObject)
                {
                    continue;
                }

                // 如果上一个状态是 Stay，那么只检测 MainGround 层
                if (stateMachine.PreviousStateType == StateType.Stay)
                {
                    if (collider.gameObject.layer == LayerMask.NameToLayer("MainGround"))
                    {
                        // 记录当前栖息的碰撞器物体
                        currentPerchObject = collider.gameObject;
                        isOnGround = true;
                        return true;
                    }
                }
                else
                {

                    // 检查是否是蘑菇或摆件等碰撞器
                    if (IsValidCollision(collider))
                    {
                        //LogManager.Log($"瓢虫降落碰撞到: {collider.gameObject.name}");
                        // 记录当前栖息的碰撞器物体
                        currentPerchObject = collider.gameObject;
                        return true;
                    }
                }
            }

            return false;
        }

        private bool IsValidCollision(Collider2D collider)
        {
            // 检查碰撞器是否是蘑菇、摆件等
            // 这里需要根据实际的游戏对象标签或组件来判断
            // 暂时使用标签来判断，实际项目中可能需要更复杂的逻辑
            if (collider.CompareTag("AnimalIgnoreCollider"))
            {
                return false; // 忽略这个碰撞器
            }

            // 检查是否是地板（MainGround层），地板不能忽略
            if (collider.gameObject.layer == LayerMask.NameToLayer("MainGround"))
            {
                return true; // 地板不能忽略
            }

            return true;
        }
#if UNITY_EDITOR
        // 在场景中绘制检测半径
        public override void OnDrawGizmosSelected()
        {
            if (stateMachine != null && stateConfig != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(stateMachine.transform.position + new Vector3(0, stateConfig.collisionCheckRadius / 2, 0), stateConfig.collisionCheckRadius);
            }
        }
#endif        
    }
}