using System.Collections.Generic;
using UnityEngine;
using Z.BehaviourTree;

namespace Z.BehaviourTree.AI
{
    public class ExecuteAttack : AINodeBase
    {
        [Tooltip("是否随机选择攻击")]
        public bool randomAttack = true;

        [Tooltip("优先使用精英攻击")]
        public bool preferEliteAttacks = false;

        protected override State OnUpdate()
        {
            if (controller == null || !controller.CanAttack())
                return State.Failure;

            AttackActionData attackData = SelectAttack();
            if (attackData == null)
                return State.Failure;

            controller.PerformAttack(attackData);
            return State.Success;
        }

        private AttackActionData SelectAttack()
        {
            if (config == null || config.attackActions == null || config.attackActions.Count == 0)
                return null;

            List<AttackActionData> availableAttacks = new List<AttackActionData>(config.attackActions);

            if ((config.difficulty == EnemyDifficulty.Elite || config.difficulty == EnemyDifficulty.Boss) &&
                config.eliteAttackActions != null && config.eliteAttackActions.Count > 0)
            {
                if (preferEliteAttacks && Random.value < 0.5f)
                {
                    return config.eliteAttackActions[Random.Range(0, config.eliteAttackActions.Count)];
                }
                availableAttacks.AddRange(config.eliteAttackActions);
            }

            if (availableAttacks.Count == 0)
                return null;

            return randomAttack 
                ? availableAttacks[Random.Range(0, availableAttacks.Count)] 
                : availableAttacks[0];
        }
    }
}
