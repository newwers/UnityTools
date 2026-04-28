using UnityEngine;

namespace Z.BehaviourTree.AI
{
    public abstract class AINodeBase : ActionNode
    {
        protected EnemyAIController controller;
        protected EnemyConfigData config;

        protected override void OnStart()
        {
            InitializeReferences();
        }

        protected override void OnStop() { }

        private void InitializeReferences()
        {
            controller = GetBlackboardValue<EnemyAIController>("controller");
            config = GetBlackboardValue<EnemyConfigData>("config");
            
            if (controller == null)
            {
                Debug.LogError("[AINodeBase] 无法从黑板获取 EnemyAIController");
            }
            
            if (config == null)
            {
                Debug.LogError("[AINodeBase] 无法从黑板获取 EnemyConfigData");
            }

            UpdateBlackboardReferences();
        }

        protected void UpdateBlackboardReferences()
        {
            if (blackboard != null && controller != null)
            {
                blackboard.SetValue("controller", controller);
                blackboard.SetValue("config", config);
            }
        }

        protected void UpdateBlackboardData()
        {
            if (blackboard == null || controller == null) return;

            blackboard.SetValue("target", controller.CurrentTarget);
            blackboard.SetValue("distanceToTarget",
                controller.CurrentTarget != null ?
                Vector3.Distance(controller.transform.position, controller.CurrentTarget.position) :
                float.MaxValue);

            if (controller.Attributes?.characterAtttibute != null)
            {
                float healthPercent = controller.Attributes.characterAtttibute.currentHealth /
                                    controller.Attributes.characterAtttibute.maxHealth;
                blackboard.SetValue("healthPercent", healthPercent);
            }

            blackboard.SetValue("isStunned", controller.IsStunned);
            blackboard.SetValue("isDodging", controller.IsDodging());
            blackboard.SetValue("currentState", controller.CurrentState);
        }
    }
}