using UnityEngine;

namespace Z.BehaviourTree.AI
{
    public class CheckShouldRetreat : AINodeBase
    {
        [Tooltip("撤退生命值阈值")]
        public float healthThreshold = 0.2f;

        protected override State OnUpdate()
        {
            if (controller == null || controller.Attributes?.characterAtttibute == null)
                return State.Failure;

            if (config.retreatHealthThreshold <= 0)
                return State.Failure;

            float healthPercent = controller.Attributes.characterAtttibute.currentHealth /
                                controller.Attributes.characterAtttibute.maxHealth;

            return healthPercent <= Mathf.Max(config.retreatHealthThreshold, healthThreshold) ? State.Success : State.Failure;
        }
    }
}
