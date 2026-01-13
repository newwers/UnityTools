using UnityEngine;
using Senses;

namespace Senses.Examples
{
    public class AIWithSensesExample : MonoBehaviour
    {
        [Header("感知系统")]
        public SenseSystemManager senseManager;

        [Header("AI行为配置")]
        [Tooltip("巡逻速度")]
        public float patrolSpeed = 2f;
        [Tooltip("追逐速度")]
        public float chaseSpeed = 4f;
        [Tooltip("攻击范围")]
        public float attackRange = 1.5f;
        [Tooltip("失去目标距离")]
        public float loseTargetDistance = 20f;

        private enum AIState
        {
            Patrol,
            Chase,
            Attack,
            Investigate
        }

        private AIState currentState = AIState.Patrol;
        private GameObject currentTarget = null;
        private Vector3 patrolDirection = Vector3.right;
        private float patrolTimer = 0f;
        private float patrolDuration = 3f;

        private void Start()
        {
            if (senseManager == null)
            {
                senseManager = GetComponent<SenseSystemManager>();
                if (senseManager == null)
                {
                    senseManager = gameObject.AddComponent<SenseSystemManager>();
                }
            }

            senseManager.OnSenseEvent += HandleSenseEvent;
        }

        private void Update()
        {
            switch (currentState)
            {
                case AIState.Patrol:
                    UpdatePatrolState();
                    break;
                case AIState.Chase:
                    UpdateChaseState();
                    break;
                case AIState.Attack:
                    UpdateAttackState();
                    break;
                case AIState.Investigate:
                    UpdateInvestigateState();
                    break;
            }
        }

        private void UpdatePatrolState()
        {
            // 巡逻移动
            transform.Translate(patrolDirection * patrolSpeed * Time.deltaTime);

            patrolTimer += Time.deltaTime;
            if (patrolTimer >= patrolDuration)
            {
                patrolDirection = -patrolDirection;
                patrolTimer = 0f;
            }

            // 检查是否有感知事件
            if (currentTarget != null)
            {
                currentState = AIState.Chase;
            }
        }

        private void UpdateChaseState()
        {
            if (currentTarget == null)
            {
                currentState = AIState.Patrol;
                return;
            }

            float distanceToTarget = Vector3.Distance(transform.position, currentTarget.transform.position);

            if (distanceToTarget > loseTargetDistance)
            {
                currentTarget = null;
                currentState = AIState.Patrol;
                return;
            }

            if (distanceToTarget <= attackRange)
            {
                currentState = AIState.Attack;
                return;
            }

            // 追逐目标
            Vector3 direction = (currentTarget.transform.position - transform.position).normalized;
            transform.Translate(direction * chaseSpeed * Time.deltaTime);
            transform.right = direction;
        }

        private void UpdateAttackState()
        {
            if (currentTarget == null)
            {
                currentState = AIState.Patrol;
                return;
            }

            float distanceToTarget = Vector3.Distance(transform.position, currentTarget.transform.position);

            if (distanceToTarget > attackRange * 1.5f)
            {
                currentState = AIState.Chase;
                return;
            }

            if (distanceToTarget > loseTargetDistance)
            {
                currentTarget = null;
                currentState = AIState.Patrol;
                return;
            }

            // 面向目标
            Vector3 direction = (currentTarget.transform.position - transform.position).normalized;
            transform.right = direction;

            // 这里可以添加攻击逻辑
            Debug.Log($"攻击目标: {currentTarget.name}");
        }

        private void UpdateInvestigateState()
        {
            // 调查声音来源的逻辑
            // 这里可以添加更复杂的调查行为
            Debug.Log("调查声音来源");
            currentState = AIState.Patrol;
        }

        private void HandleSenseEvent(SenseEvent senseEvent)
        {
            switch (senseEvent.senseType)
            {
                case SenseType.Vision:
                    HandleVisionEvent(senseEvent);
                    break;
                case SenseType.Hearing:
                    HandleHearingEvent(senseEvent);
                    break;
            }
        }

        private void HandleVisionEvent(SenseEvent senseEvent)
        {
            currentTarget = senseEvent.detectedObject;
            Debug.Log($"视觉检测到: {senseEvent.detectedObject.name}, 强度: {senseEvent.intensity}");
        }

        private void HandleHearingEvent(SenseEvent senseEvent)
        {
            if (currentState == AIState.Patrol)
            {
                // 只有在巡逻状态才会调查声音
                Debug.Log($"听到声音: {senseEvent.eventTag}, 强度: {senseEvent.intensity}");
                // 这里可以添加调查声音来源的逻辑
            }
        }

        public void EnableVision(bool enable)
        {
            if (senseManager != null)
            {
                senseManager.SetVisionEnabled(enable);
            }
        }

        public void EnableHearing(bool enable)
        {
            if (senseManager != null)
            {
                senseManager.SetHearingEnabled(enable);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, loseTargetDistance);

            if (currentTarget != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, currentTarget.transform.position);
            }
        }
    }
}
