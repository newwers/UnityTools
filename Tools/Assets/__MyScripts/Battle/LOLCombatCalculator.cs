using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Z.Battle
{
    public class LOLCombatCalculator 
    {
        /// <summary>
        /// 攻击回调,
        /// 物理伤害
        /// 魔法伤害
        /// </summary>
        public event Action<float,float> OnAttackEvent;
        private LOLCharacterStats attackerStats;
        private LOLCharacterStats targetStats;

        public LOLCombatCalculator(LOLCharacterStats attacker, LOLCharacterStats target)
        {
            attackerStats = attacker;
            targetStats = target;
        }

        public LOLCombatCalculator(LOLCharacterStats attacker)
        {
            attackerStats = attacker;
        }

        public void SetTarget(LOLCharacterStats target)
        {
            targetStats = target;
        }

        public void BattleBegin()
        {
            if (targetStats == null || attackerStats == null)
            {
                Debug.LogError($"没有目标或者攻击者,不进行伤害计算");//是否需要记录攻击者和被攻击者的名称数据
                return;
            }
            //物理伤害计算
            float physicalValue = PhysicalDamageCalculator();

            float magicalValue = MagicalDamageCalculator();

            float beforeHealth = targetStats.health;

            //伤害计算完，如何同步数据到双方?数值引用进行同步吗
            targetStats.health = (int)(targetStats.health - physicalValue - magicalValue);

            Debug.Log($"beforeHealth:{beforeHealth} -> {targetStats.health},物理伤害：{physicalValue},魔法伤害：{magicalValue}");

            //如果有反伤之类，需要重新构建一个伤害计算进行处理,伤害构建用对象池处理
            OnAttackEvent?.Invoke(physicalValue,magicalValue);
        }

        float PhysicalDamageCalculator()
        {
            //
            float value = attackerStats.physicalAttack - targetStats.physicalDefense;//攻击者物理伤害 - 被攻击者的物理防御力

            //是否有数值减伤
            //是否有百分比减伤
            float finalVaue = Mathf.Max(value - targetStats.physicalDamageReduction, 0) * (1 - targetStats.percentagePhysicalDamageReduction - targetStats.percentageAllDamageReduction);

            return finalVaue;
        }

        float MagicalDamageCalculator()
        {
            float value = attackerStats.magicalAttack - targetStats.magicalDefense;//攻击者技能伤害 - 被攻击者的魔法防御力

            //是否有数值减伤
            //是否有百分比减伤
            float finalVaue = Mathf.Max(value - targetStats.magicalDamageReduction, 0) * (1 - targetStats.percentageMagicalDamageReduction - targetStats.percentageAllDamageReduction);

            return finalVaue;
        }

        // 检查目标是否在攻击范围内
        public bool IsTargetInRange(Vector3 attackerPosition, Vector3 targetPosition)
        {
            float distance = Vector3.Distance(attackerPosition, targetPosition);
            return distance <= attackerStats.attackRange;
        }

    }
}
