using UnityEngine;
using Z.BehaviourTree;

namespace Z.BehaviourTree.AI
{
    public class CheckInChaseRange : AINodeBase
    {
        [Tooltip("额外追击距离")]
        public float extraRange = 0f;

        protected override State OnUpdate()
        {
            if (controller == null || controller.CurrentTarget == null)
                return State.Failure;

            float distance = Vector3.Distance(
                controller.transform.position, 
                controller.CurrentTarget.position
            );

            return distance <= config.loseTargetDistance + extraRange ? State.Success : State.Failure;
        }
    }
}
