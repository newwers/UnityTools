using UnityEngine;
using Z.BehaviourTree;

namespace Z.BehaviourTree.AI
{
    public class ActionDodge : AINodeBase
    {
        private Vector2 dodgeDirection;

        protected override void OnStart()
        {
            base.OnStart();
            if (controller != null && controller.CurrentTarget != null)
            {
                dodgeDirection = (controller.transform.position - controller.CurrentTarget.position).normalized;
            }
            controller?.PerformDash(dodgeDirection);
        }

        protected override State OnUpdate()
        {
            if (controller == null || controller.IsDead) return State.Failure;

            if (!controller.IsDodging())
            {
                return State.Success;
            }

            return State.Running;
        }
    }
}