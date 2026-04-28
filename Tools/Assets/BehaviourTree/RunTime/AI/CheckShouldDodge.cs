using UnityEngine;
using Z.BehaviourTree;

namespace Z.BehaviourTree.AI
{
    public class CheckShouldDodge : AINodeBase
    {
        protected override State OnUpdate()
        {
            if (controller == null) return State.Failure;

            if (!controller.CanDash())
                return State.Failure;

            float dodgeProbability = config.dodgeProbability;
            if (GameDifficultyManager.Instance != null)
            {
                dodgeProbability *= GameDifficultyManager.Instance.CurrentModifiers.dodgeProbabilityMultiplier;
            }

            return Random.value < dodgeProbability ? State.Success : State.Failure;
        }
    }
}
