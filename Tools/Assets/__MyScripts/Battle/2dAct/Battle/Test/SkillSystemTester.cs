using UnityEngine;

public class SkillSystemTester : MonoBehaviour
{
    [Header("测试对象")]
    public CharacterLogic player;
    public CharacterLogic enemy;

    [Header("测试技能")]
    public SkillData testSkill;

    [Header("测试效果")]
    public EffectData stunEffect;
    public EffectData shieldEffect;
    public EffectData damageBoostEffect;

    void Start()
    {
        LogManager.Log("=== 技能系统测试开始 ===");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            TestEnergyConsumption();
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            TestBuffSystem();
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            TestDamageCalculation();
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            TestSkillEffects();
        }

        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            TestShieldAndBlock();
        }
    }

    void TestEnergyConsumption()
    {
        LogManager.Log("--- 测试1: 能量消耗 ---");

        if (player == null || player.PlayerAttributes == null)
        {
            LogManager.LogWarning("玩家未配置！");
            return;
        }

        var stats = player.PlayerAttributes.characterAtttibute;
        float initialEnergy = stats.currentEnergy;

        LogManager.Log($"初始能量: {initialEnergy}");

        float energyCost = 30f;
        if (stats.currentEnergy >= energyCost)
        {
            stats.ChangeEnergy(-energyCost);
            LogManager.Log($"消耗能量: {energyCost}, 剩余: {stats.currentEnergy}");
        }
        else
        {
            LogManager.LogWarning($"能量不足！需要: {energyCost}, 当前: {stats.currentEnergy}");
        }
    }

    void TestBuffSystem()
    {
        LogManager.Log("--- 测试2: Buff 系统 ---");

        if (enemy == null || enemy.buffSystem == null)
        {
            LogManager.LogWarning("敌人未配置！");
            return;
        }

        if (stunEffect != null)
        {
            enemy.buffSystem.ApplyBuff(stunEffect, player.gameObject);
            LogManager.Log($"应用眩晕效果到 {enemy.name}");
        }

        if (shieldEffect != null)
        {
            enemy.buffSystem.ApplyBuff(shieldEffect, player.gameObject);
            LogManager.Log($"应用护盾效果到 {enemy.name}");
        }

        if (damageBoostEffect != null)
        {
            player.buffSystem.ApplyBuff(damageBoostEffect, player.gameObject);
            LogManager.Log($"应用增伤效果到 {player.name}");
        }

        var activeBuffs = enemy.buffSystem.GetActiveBuffs();
        LogManager.Log($"当前 Buff 数量: {activeBuffs.Count}");
        foreach (var buff in activeBuffs)
        {
            LogManager.Log($"  - {buff.data.effectName}: 剩余 {buff.remainingDuration}秒, 层数 {buff.currentStacks}");
        }
    }

    void TestDamageCalculation()
    {
        LogManager.Log("--- 测试3: 伤害计算 ---");

        if (player == null || enemy == null)
        {
            LogManager.LogWarning("玩家或敌人未配置！");
            return;
        }

        var attackerStats = player.PlayerAttributes.characterAtttibute;
        var targetStats = enemy.PlayerAttributes.characterAtttibute;

        var damageInfo = new DamageInfo
        {
            baseDamage = 50f,
            breakPower = 20f,
            staggerDamage = 30f,
            isGuaranteedCrit = true,
            lifeStealPercent = 10f,
            source = player.gameObject
        };

        LogManager.Log($"攻击者力量: {attackerStats.strength}");
        LogManager.Log($"基础伤害: {damageInfo.baseDamage}");
        LogManager.Log($"是否必定暴击: {damageInfo.isGuaranteedCrit}");

        var result = DamageCalculator.CalculateDamage(
            damageInfo,
            attackerStats,
            targetStats,
            player.buffSystem,
            enemy.buffSystem
        );

        LogManager.Log($"最终伤害: {result.finalDamage}");
        LogManager.Log($"是否暴击: {result.isCritical}");
        LogManager.Log($"是否格挡: {result.isBlocked}");
        LogManager.Log($"是否闪避: {result.isMiss}");
        LogManager.Log($"护盾伤害: {result.shieldDamage}");
        LogManager.Log($"生命伤害: {result.healthDamage}");
    }

    void TestSkillEffects()
    {
        LogManager.Log("--- 测试4: 技能效果 ---");

        if (testSkill == null)
        {
            LogManager.LogWarning("测试技能未配置！");
            return;
        }

        LogManager.Log($"技能名称: {testSkill.SkillName}");
        LogManager.Log($"能量消耗: {testSkill.EnergyCost}");
        LogManager.Log($"基础伤害: {testSkill.baseDamage}");
        LogManager.Log($"破防力: {testSkill.breakPower}");
        LogManager.Log($"是否必定暴击: {testSkill.isGuaranteedCrit}");
        LogManager.Log($"生命偷取: {testSkill.lifeStealPercent}%");
        LogManager.Log($"释放前效果数量: {testSkill.effectsOnCast.Count}");
        LogManager.Log($"释放后效果数量: {testSkill.effectsOnComplete.Count}");

        if (enemy != null && enemy.buffSystem != null)
        {
            // 应用释放前效果（通常施加给自己）
            foreach (var effect in testSkill.effectsOnCast)
            {
                if (effect != null)
                {
                    GameObject receiver = (effect.effectTarget == EffectTarget.Attacker) ? player.gameObject : enemy.gameObject;
                    var buffSystem = receiver.GetComponent<BuffSystem>();
                    if (buffSystem != null)
                    {
                        buffSystem.ApplyBuff(effect, player.gameObject);
                        LogManager.Log($"应用释放前效果: {effect.effectName}");
                    }
                }
            }
        }
    }

    void TestShieldAndBlock()
    {
        LogManager.Log("--- 测试5: 护盾与格挡 ---");

        if (enemy == null)
        {
            LogManager.LogWarning("敌人未配置！");
            return;
        }

        var stats = enemy.PlayerAttributes.characterAtttibute;

        LogManager.Log($"当前护盾: {stats.currentShield} / {stats.maxShield}");
        LogManager.Log($"当前格挡值: {stats.currentBlockValue}");
        LogManager.Log($"是否格挡中: {stats.isBlocking}");

        stats.ChangeShield(50f);
        LogManager.Log($"增加护盾后: {stats.currentShield}");

        stats.isBlocking = true;
        stats.currentBlockValue = 100f;
        LogManager.Log($"进入格挡状态，格挡值: {stats.currentBlockValue}");

        var damageInfo = new DamageInfo
        {
            baseDamage = 100f,
            breakPower = 50f,
            staggerDamage = 0f,
            isGuaranteedCrit = false,
            lifeStealPercent = 0f,
            source = player.gameObject
        };

        var result = DamageCalculator.CalculateDamage(
            damageInfo,
            player.PlayerAttributes.characterAtttibute,
            stats,
            player.buffSystem,
            enemy.buffSystem
        );

        LogManager.Log($"攻击结果 - 是否被格挡: {result.isBlocked}");
        LogManager.Log($"护盾伤害: {result.shieldDamage}, 剩余护盾: {stats.currentShield}");
    }

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label("=== 技能系统测试 ===");
        GUILayout.Label("按键说明：");
        GUILayout.Label("1 - 测试能量消耗");
        GUILayout.Label("2 - 测试 Buff 系统");
        GUILayout.Label("3 - 测试伤害计算");
        GUILayout.Label("4 - 测试技能效果");
        GUILayout.Label("5 - 测试护盾与格挡");
        GUILayout.EndArea();
    }
}
