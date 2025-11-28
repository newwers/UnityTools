using UnityEngine;

public class AttributeEffectsTest : MonoBehaviour
{
    [Header("测试目标")]
    [SerializeField] private BuffSystem targetBuffSystem;
    [SerializeField] private CharacterAttributes targetAttributes;

    [Header("测试按键")]
    [SerializeField] private KeyCode healKey = KeyCode.Alpha1;
    [SerializeField] private KeyCode energyKey = KeyCode.Alpha2;
    [SerializeField] private KeyCode strengthKey = KeyCode.Alpha3;
    [SerializeField] private KeyCode speedKey = KeyCode.Alpha4;
    [SerializeField] private KeyCode attackKey = KeyCode.Alpha5;
    [SerializeField] private KeyCode defenseKey = KeyCode.Alpha6;
    [SerializeField] private KeyCode critKey = KeyCode.Alpha7;
    [SerializeField] private KeyCode maxHealthKey = KeyCode.Alpha8;
    [SerializeField] private KeyCode maxEnergyKey = KeyCode.Alpha9;
    [SerializeField] private KeyCode comboKey = KeyCode.Alpha0;

    private void Update()
    {
        if (targetBuffSystem == null || targetAttributes == null)
        {
            return;
        }

        if (Input.GetKeyDown(healKey))
        {
            TestInstantHeal();
        }

        if (Input.GetKeyDown(energyKey))
        {
            TestEnergyEffects();
        }

        if (Input.GetKeyDown(strengthKey))
        {
            TestStrengthBoost();
        }

        if (Input.GetKeyDown(speedKey))
        {
            TestSpeedBoost();
        }

        if (Input.GetKeyDown(attackKey))
        {
            TestAttackBoost();
        }

        if (Input.GetKeyDown(defenseKey))
        {
            TestDefenseBoost();
        }

        if (Input.GetKeyDown(critKey))
        {
            TestCritBoost();
        }

        if (Input.GetKeyDown(maxHealthKey))
        {
            TestMaxHealthBoost();
        }

        if (Input.GetKeyDown(maxEnergyKey))
        {
            TestMaxEnergyBoost();
        }

        if (Input.GetKeyDown(comboKey))
        {
            TestComboBuffs();
        }
    }

    private void TestInstantHeal()
    {
        LogManager.Log("=== 测试立即回血效果 ===");
        LogManager.Log($"回血前生命: {targetAttributes.currentHealth}/{targetAttributes.maxHealth}");
        
        var healEffect = EffectDataExamples.CreateInstantHealEffect(50f);
        targetBuffSystem.ApplyBuff(healEffect);
        
        LogManager.Log($"回血后生命: {targetAttributes.currentHealth}/{targetAttributes.maxHealth}");
    }

    private void TestEnergyEffects()
    {
        LogManager.Log("=== 测试能量效果 ===");
        LogManager.Log($"当前能量: {targetAttributes.currentEnergy}/{targetAttributes.maxEnergy}");
        
        var instantEnergyEffect = EffectDataExamples.CreateInstantEnergyRestoreEffect(30f);
        targetBuffSystem.ApplyBuff(instantEnergyEffect);
        LogManager.Log($"立即回复后能量: {targetAttributes.currentEnergy}/{targetAttributes.maxEnergy}");
        
        var energyRegenEffect = EffectDataExamples.CreateEnergyRegenerationEffect(10f, 5f);
        targetBuffSystem.ApplyBuff(energyRegenEffect);
        LogManager.Log("已添加持续回复能量效果（10秒，每秒5点）");
    }

    private void TestStrengthBoost()
    {
        LogManager.Log("=== 测试力量增强效果 ===");
        LogManager.Log($"增强前力量: {targetAttributes.strength}");
        
        var strengthEffect = EffectDataExamples.CreateStrengthBoostEffect(30f, strengthAmount: 10f);
        targetBuffSystem.ApplyBuff(strengthEffect);
        
        LogManager.Log($"增强后力量: {targetAttributes.strength} (+10固定值)");
        
        var strengthPercentEffect = EffectDataExamples.CreateStrengthBoostEffect(30f, strengthPercent: 20f);
        targetBuffSystem.ApplyBuff(strengthPercentEffect);
        
        LogManager.Log($"再次增强力量: {targetAttributes.strength} (+20%)");
    }

    private void TestSpeedBoost()
    {
        LogManager.Log("=== 测试移速增强效果 ===");
        LogManager.Log($"增强前移速: {targetAttributes.moveSpeed}");
        
        var speedEffect = EffectDataExamples.CreateSpeedBoostEffect(15f, speedPercent: 30f);
        targetBuffSystem.ApplyBuff(speedEffect);
        
        LogManager.Log($"增强后移速: {targetAttributes.moveSpeed} (+30%)");
    }

    private void TestAttackBoost()
    {
        LogManager.Log("=== 测试攻击增强效果 ===");
        
        var attackEffect = EffectDataExamples.CreateAttackBoostEffect(20f, attackPercent: 25f);
        targetBuffSystem.ApplyBuff(attackEffect);
        
        LogManager.Log("已添加攻击增强效果（+25%攻击，持续20秒）");
        LogManager.Log("注意：攻击增强需要在伤害计算时读取");
    }

    private void TestDefenseBoost()
    {
        LogManager.Log("=== 测试防御增强效果 ===");
        LogManager.Log($"增强前防御: {targetAttributes.defense}");
        
        var defenseEffect = EffectDataExamples.CreateDefenseBoostEffect(25f, defenseAmount: 10f);
        targetBuffSystem.ApplyBuff(defenseEffect);
        
        LogManager.Log($"增强后防御: {targetAttributes.defense} (+10固定值)");
    }

    private void TestCritBoost()
    {
        LogManager.Log("=== 测试暴击增强效果 ===");
        LogManager.Log($"增强前暴击率: {targetAttributes.critRate}%");
        LogManager.Log($"增强前暴击倍率: {targetAttributes.critMultiplier}x");
        
        var critRateEffect = EffectDataExamples.CreateCritRateBoostEffect(12f, 15f);
        targetBuffSystem.ApplyBuff(critRateEffect);
        LogManager.Log($"增强后暴击率: {targetAttributes.critRate}% (+15%)");
        
        var critDamageEffect = EffectDataExamples.CreateCritDamageBoostEffect(12f, 50f);
        targetBuffSystem.ApplyBuff(critDamageEffect);
        LogManager.Log($"增强后暴击倍率: {targetAttributes.critMultiplier}x (+50%)");
    }

    private void TestMaxHealthBoost()
    {
        LogManager.Log("=== 测试生命上限增强效果 ===");
        LogManager.Log($"增强前生命上限: {targetAttributes.maxHealth}");
        LogManager.Log($"当前生命: {targetAttributes.currentHealth}");
        
        var maxHealthEffect = EffectDataExamples.CreateMaxHealthBoostEffect(60f, maxHealthAmount: 100f);
        targetBuffSystem.ApplyBuff(maxHealthEffect);
        
        LogManager.Log($"增强后生命上限: {targetAttributes.maxHealth} (+100固定值)");
        LogManager.Log($"当前生命: {targetAttributes.currentHealth}（不会自动增加）");
    }

    private void TestMaxEnergyBoost()
    {
        LogManager.Log("=== 测试能量上限增强效果 ===");
        LogManager.Log($"增强前能量上限: {targetAttributes.maxEnergy}");
        LogManager.Log($"当前能量: {targetAttributes.currentEnergy}");
        
        var maxEnergyEffect = EffectDataExamples.CreateMaxEnergyBoostEffect(60f, maxEnergyPercent: 20f);
        targetBuffSystem.ApplyBuff(maxEnergyEffect);
        
        LogManager.Log($"增强后能量上限: {targetAttributes.maxEnergy} (+20%)");
        LogManager.Log($"当前能量: {targetAttributes.currentEnergy}（不会自动增加）");
    }

    private void TestComboBuffs()
    {
        LogManager.Log("=== 测试组合Buff效果 ===");
        LogManager.Log("同时添加多个增益效果...");
        
        targetBuffSystem.ApplyBuff(EffectDataExamples.CreateStrengthBoostEffect(30f, strengthPercent: 20f));
        targetBuffSystem.ApplyBuff(EffectDataExamples.CreateSpeedBoostEffect(30f, speedPercent: 30f));
        targetBuffSystem.ApplyBuff(EffectDataExamples.CreateAttackBoostEffect(30f, attackPercent: 25f));
        targetBuffSystem.ApplyBuff(EffectDataExamples.CreateDefenseBoostEffect(30f, defensePercent: 15f));
        targetBuffSystem.ApplyBuff(EffectDataExamples.CreateCritRateBoostEffect(30f, 15f));
        targetBuffSystem.ApplyBuff(EffectDataExamples.CreateMaxHealthBoostEffect(30f, maxHealthPercent: 20f));
        
        LogManager.Log("组合Buff已应用！");
        LogManager.Log($"力量: {targetAttributes.strength}");
        LogManager.Log($"移速: {targetAttributes.moveSpeed}");
        LogManager.Log($"防御: {targetAttributes.defense}");
        LogManager.Log($"暴击率: {targetAttributes.critRate}%");
        LogManager.Log($"生命上限: {targetAttributes.maxHealth}");
    }

    private void OnGUI()
    {
        if (targetBuffSystem == null || targetAttributes == null)
        {
            GUILayout.Label("请在Inspector中设置测试目标");
            return;
        }

        GUILayout.BeginArea(new Rect(10, 10, 400, 500));
        GUILayout.Box("属性效果测试面板", GUILayout.Width(390));
        
        GUILayout.Label($"当前属性状态:");
        GUILayout.Label($"生命: {targetAttributes.currentHealth:F0}/{targetAttributes.maxHealth:F0}");
        GUILayout.Label($"能量: {targetAttributes.currentEnergy:F0}/{targetAttributes.maxEnergy:F0}");
        GUILayout.Label($"力量: {targetAttributes.strength:F1}");
        GUILayout.Label($"移速: {targetAttributes.moveSpeed:F1}");
        GUILayout.Label($"防御: {targetAttributes.defense}");
        GUILayout.Label($"暴击率: {targetAttributes.critRate:F1}%");
        GUILayout.Label($"暴击倍率: {targetAttributes.critMultiplier:F2}x");
        
        GUILayout.Space(10);
        GUILayout.Label("测试按键:");
        GUILayout.Label($"[1] - 立即回血 (50点)");
        GUILayout.Label($"[2] - 能量效果 (立即+持续)");
        GUILayout.Label($"[3] - 增加力量 (+10固定 + +20%)");
        GUILayout.Label($"[4] - 增加移速 (+30%)");
        GUILayout.Label($"[5] - 增加攻击 (+25%)");
        GUILayout.Label($"[6] - 增加防御 (+10固定)");
        GUILayout.Label($"[7] - 增加暴击 (+15%率 +50%伤害)");
        GUILayout.Label($"[8] - 增加生命上限 (+100固定)");
        GUILayout.Label($"[9] - 增加能量上限 (+20%)");
        GUILayout.Label($"[0] - 组合Buff (全属性增强)");
        
        GUILayout.Space(10);
        GUILayout.Label($"当前激活Buff数量: {targetBuffSystem.GetActiveBuffs().Count}");
        
        GUILayout.EndArea();
    }
}
