﻿using UnityEngine;

namespace Z.BehaviourTree.AI
{
    public class ActionPatrol : AINodeBase
    {
        private Vector3 currentPatrolTarget;

        protected override void OnStart()
        {
            base.OnStart();
            if (controller != null && controller.configData != null)
            {
                currentPatrolTarget = controller.configData.patrolArea.GetRandomPointInArea(controller.PatrolCenter);
            }
            controller?.ChangeState(CharacterState.Patrol);
        }

        protected override State OnUpdate()
        {
            if (controller == null || controller.IsDead) return State.Failure;

            float distanceToTarget = Vector3.Distance(controller.transform.position, currentPatrolTarget);

            if (distanceToTarget < 1f)
            {
                return State.Success;
            }

            Vector3 direction = (currentPatrolTarget - controller.transform.position).normalized;
            controller.Move(new Vector2(direction.x, 0));

            if (controller.animator != null)
            {
                controller.animator.SetFloat("MoveX", 1);
                float actualMoveSpeed = new Vector2(controller.rb.linearVelocity.x, 0).magnitude;
                float animMoveSpeedValue = actualMoveSpeed / 1.5f;
                controller.animator.SetFloat("animMoveSpeed", Mathf.Abs(animMoveSpeedValue));
            }

            return State.Running;
        }
    }
}