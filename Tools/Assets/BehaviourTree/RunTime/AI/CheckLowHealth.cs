using UnityEngine;

namespace Z.BehaviourTree.AI
{
    public class CheckLowHealth : AINodeBase
    {
        [Tooltip("低血量阈值")]
        [Range(0f, 1f)]
        public float healthThreshold = 0.3f;

        protected override State OnUpdate()
        {
            if (controller == null || controller.Attributes?.characterAtttibute == null)
                return State.Failure;

            float healthPercent = controller.Attributes.characterAtttibute.currentHealth /
                                controller.Attributes.characterAtttibute.maxHealth;

            return healthPercent <= healthThreshold ? State.Success : State.Failure;
        }
    }
}
