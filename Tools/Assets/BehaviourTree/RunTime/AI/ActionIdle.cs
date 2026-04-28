﻿using UnityEngine;

namespace Z.BehaviourTree.AI
{
    public class ActionIdle : AINodeBase
    {
        private float idleTimer;
        private float currentIdleTime;

        protected override void OnStart()
        {
            base.OnStart();
            if (controller != null && controller.configData != null)
            {
                currentIdleTime = Random.Range(
                    controller.configData.idleTimeMin,
                    controller.configData.idleTimeMax);
            }
            idleTimer = 0f;
            controller?.ChangeState(CharacterState.Idle);
        }

        protected override State OnUpdate()
        {
            if (controller == null || controller.IsDead) return State.Failure;

            idleTimer += Time.deltaTime;

            if (idleTimer >= currentIdleTime)
            {
                return State.Success;
            }

            controller.Move(Vector2.zero);

            if (controller.animator != null)
            {
                controller.animator.SetFloat("MoveX", 0);
                controller.animator.SetFloat("animMoveSpeed", 1);
            }

            return State.Running;
        }
    }
}