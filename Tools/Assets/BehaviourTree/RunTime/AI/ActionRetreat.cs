using UnityEngine;
using Z.BehaviourTree;

namespace Z.BehaviourTree.AI
{
    public class ActionRetreat : AINodeBase
    {
        protected override void OnStart()
        {
            base.OnStart();
            controller?.ChangeState(CharacterState.Retreat);
        }

        protected override State OnUpdate()
        {
            if (controller == null || controller.IsDead) return State.Failure;
            if (controller.CurrentTarget == null) return State.Failure;

            Vector3 retreatDirection = (controller.transform.position - controller.CurrentTarget.position).normalized;
            controller.Move(new Vector2(retreatDirection.x, 0));

            return State.Running;
        }
    }
}