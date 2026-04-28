﻿using UnityEngine;

namespace Z.BehaviourTree.AI
{
    public class ActionChase : AINodeBase
    {
        protected override void OnStart()
        {
            base.OnStart();
            controller?.ChangeState(CharacterState.Chase);
        }

        protected override State OnUpdate()
        {
            if (controller == null || controller.IsDead) return State.Failure;
            if (controller.CurrentTarget == null) return State.Failure;

            float distanceToTarget = Vector3.Distance(
                controller.transform.position,
                controller.CurrentTarget.position);

            if (distanceToTarget > controller.configData.loseTargetDistance)
            {
                controller.SetCurrentTarget(null);
                return State.Failure;
            }

            if (distanceToTarget <= controller.GetCurrentAttackRange())
            {
                return State.Success;
            }

            Vector3 direction = (controller.CurrentTarget.position - controller.transform.position).normalized;
            controller.Move(new Vector2(direction.x, 0));

            if (controller.animator != null)
            {
                controller.animator.SetFloat("MoveX", 1);
                controller.animator.SetFloat("animMoveSpeed", Mathf.Abs(controller.configData.chaseSpeed));
            }

            return State.Running;
        }
    }
}