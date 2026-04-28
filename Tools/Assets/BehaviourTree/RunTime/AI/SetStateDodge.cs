using UnityEngine;
using Z.BehaviourTree;

namespace Z.BehaviourTree.AI
{
    public class SetStateDodge : AINodeBase
    {
        [Tooltip("是否远离目标闪避")]
        public bool dodgeAwayFromTarget = true;

        protected override State OnUpdate()
        {
            if (controller == null) return State.Failure;

            Vector2 dodgeDirection = Vector2.left;
            if (controller.CurrentTarget != null)
            {
                Vector3 direction = dodgeAwayFromTarget 
                    ? (controller.transform.position - controller.CurrentTarget.position).normalized
                    : (controller.CurrentTarget.position - controller.transform.position).normalized;
                dodgeDirection = new Vector2(direction.x, 0).normalized;
            }

            controller.PerformDash(dodgeDirection);
            return State.Success;
        }
    }
}
