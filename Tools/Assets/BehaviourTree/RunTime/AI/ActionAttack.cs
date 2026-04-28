﻿using UnityEngine;

namespace Z.BehaviourTree.AI
{
    public class ActionAttack : AINodeBase
    {
        protected override void OnStart()
        {
            base.OnStart();
            controller?.ChangeState(CharacterState.Attacking);
        }

        protected override State OnUpdate()
        {
            if (controller == null || controller.IsDead) return State.Failure;
            if (controller.CurrentTarget == null) return State.Failure;

            float distanceToTarget = Vector3.Distance(
                controller.transform.position,
                controller.CurrentTarget.position);

            if (distanceToTarget > controller.GetCurrentAttackRange())
            {
                return State.Failure;
            }

            controller.Move(Vector2.zero);

            Vector3 direction = (controller.CurrentTarget.position - controller.transform.position).normalized;
            if ((direction.x > 0 && !controller.IsFacingRight) ||
                (direction.x < 0 && controller.IsFacingRight))
            {
                controller.Flip();
            }

            if (controller.CanAttack())
            {
                controller.PerformAttack();
            }

            if (controller.animator != null)
            {
                controller.animator.SetFloat("MoveX", 0);
                controller.animator.SetFloat("animMoveSpeed", 1);
            }

            return State.Running;
        }
    }
}