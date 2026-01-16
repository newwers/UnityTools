using System.Collections.Generic;
using UnityEngine;

// Boss AI策略
[CreateAssetMenu(fileName = "Boss AI Strategy", menuName = "AI Strategies/Boss")]
public class BossAIStrategy : EliteAIStrategy
{
    private int currentPhase = 0;
    private bool phaseTransitioning = false;

    public override void Initialize(CharacterBase controller, EnemyConfigData config)
    {
        base.Initialize(controller, config);
        currentPhase = 0;
    }

    public override CharacterState DecideNextState()
    {
        if (phaseTransitioning) return CharacterState.Idle;

        // 检查阶段转换
        CheckPhaseTransition();

        return base.DecideNextState();
    }

    public override AttackActionData SelectAttack()
    {
        // 先检查是否有Boss攻击配置
        if (config.bossAttackActions != null && config.bossAttackActions.Count > 0)
        {
            // Boss根据阶段选择攻击
            var phaseAttacks = GetCurrentPhaseAttacks();
            if (phaseAttacks.Count == 0) return base.SelectAttack();

            // Boss有更智能的攻击选择
            float distance = GetDistanceToTarget();
            var suitableAttacks = new List<AttackActionData>();

            foreach (var attack in phaseAttacks)
            {
                if (attack != null && distance <= GetAttackRange(attack) * 1.2f)
                {
                    suitableAttacks.Add(attack);
                }
            }

            if (suitableAttacks.Count > 0)
            {
                // 根据攻击冷却和优先级选择
                suitableAttacks.Sort((a, b) => b.priority.CompareTo(a.priority));
                return suitableAttacks[0];
            }

            return base.SelectAttack();
        }

        // 如果没有Boss攻击配置，从精英攻击列表中获取
        return base.SelectAttack();
    }

    public override bool ShouldUseSpecialAbility()
    {
        // Boss更频繁使用特殊能力
        if (Time.time - lastSpecialAbilityTime < specialAbilityCooldown * 0.7f)
            return false;

        // 根据阶段调整几率
        float probability = 0.4f + (currentPhase * 0.2f);
        if (Random.value < probability)
        {
            lastSpecialAbilityTime = Time.time;
            return true;
        }

        return false;
    }

    public override bool ShouldDodge()
    {
        if (!controller.CanDash()) return false;

        // Boss敌人有更高的闪避概率，且随阶段提升
        float dodgeProbability = config.dodgeProbability * (1.5f + (currentPhase * 0.3f)); // Boss敌人闪避概率提升50%，每阶段额外提升30%
        if (GameDifficultyManager.Instance != null)
        {
            dodgeProbability *= GameDifficultyManager.Instance.CurrentModifiers.dodgeProbabilityMultiplier;
        }

        // 随机判断是否闪避
        return Random.value < dodgeProbability;
    }

    public override bool ShouldUseRecoverySkill()
    {
        if (!enemyAIController.CanUseRecoverySkill()) return false;

        // Boss敌人有更高的恢复技能使用概率，且随阶段提升
        float recoveryProbability = config.recoverySkillProbability * (1.6f + (currentPhase * 0.4f)); // Boss敌人恢复技能使用概率提升60%，每阶段额外提升40%
        if (GameDifficultyManager.Instance != null)
        {
            recoveryProbability *= GameDifficultyManager.Instance.CurrentModifiers.recoverySkillProbabilityMultiplier;
        }

        // 随机判断是否使用恢复技能
        return Random.value < recoveryProbability;
    }

    private void CheckPhaseTransition()
    {
        if (config.bossPhases == null || config.bossPhases.Count == 0)
            return;

        float healthPercent = controller.PlayerAttributes.characterAtttibute.currentHealth / controller.PlayerAttributes.characterAtttibute.maxHealth;

        for (int i = currentPhase; i < config.bossPhases.Count; i++)
        {
            if (healthPercent <= config.bossPhases[i].healthPercentThreshold)
            {
                if (i > currentPhase)
                {
                    StartPhaseTransition(i);
                }
                break;
            }
        }
    }

    private void StartPhaseTransition(int newPhase)
    {
        phaseTransitioning = true;
        currentPhase = newPhase;

        // 播放阶段转换效果
        if (config.bossPhases[newPhase].phaseTransitionEffect != null)
        {
            //Instantiate(config.bossPhases[newPhase].phaseTransitionEffect,controller.transform.position, Quaternion.identity);
        }

        // 应用新阶段的配置
        var phaseConfig = config.bossPhases[newPhase].phaseConfig;
        if (phaseConfig != null)
        {
            enemyAIController.ApplyAIConfig(phaseConfig);
        }

        // 阶段转换完成
        phaseTransitioning = false;
    }

    private List<AttackActionData> GetCurrentPhaseAttacks()
    {
        //return config.bossAttackActions;
        var attacks = new List<AttackActionData>();

        // 加上基础攻击
        attacks.AddRange(config.attackActions);
        attacks.AddRange(config.eliteAttackActions);
        attacks.AddRange(config.bossAttackActions);

        return attacks;
    }
}