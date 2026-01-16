using UnityEngine;

[CreateAssetMenu(fileName = "Game Difficulty Settings", menuName = "Game/Difficulty Settings")]
public class GameDifficultySettings : ScriptableObject
{
    [Header("难度配置")]
    public DifficultyConfig normalConfig = new DifficultyConfig
    {
        difficultyName = "普通",
        modifiers = new DifficultyModifiers
        {
            healthMultiplier = 1f,
            attackMultiplier = 1f,
            moveSpeedMultiplier = 1f,
            attackSpeedMultiplier = 1f,
            defenseMultiplier = 1f,
            detectRangeMultiplier = 1f,
            aiDecisionSpeedMultiplier = 1f,
            useEnhancedAI = false,
            expMultiplier = 1f,
            lootMultiplier = 1f,
            dodgeProbabilityMultiplier = 1f,
            recoverySkillProbabilityMultiplier = 1f
        }
    };

    public DifficultyConfig hardConfig = new DifficultyConfig
    {
        difficultyName = "困难",
        modifiers = new DifficultyModifiers
        {
            healthMultiplier = 1.5f,
            attackMultiplier = 1.3f,
            moveSpeedMultiplier = 1.2f,
            attackSpeedMultiplier = 0.85f,
            defenseMultiplier = 1.2f,
            detectRangeMultiplier = 1.3f,
            aiDecisionSpeedMultiplier = 1.2f,
            useEnhancedAI = false,
            expMultiplier = 1.5f,
            lootMultiplier = 1.3f,
            dodgeProbabilityMultiplier = 1.5f,
            recoverySkillProbabilityMultiplier = 1.5f
        }
    };

    public DifficultyConfig hellConfig = new DifficultyConfig
    {
        difficultyName = "地狱",
        modifiers = new DifficultyModifiers
        {
            healthMultiplier = 2f,
            attackMultiplier = 1.8f,
            moveSpeedMultiplier = 1.5f,
            attackSpeedMultiplier = 0.7f,
            defenseMultiplier = 1.5f,
            detectRangeMultiplier = 1.5f,
            aiDecisionSpeedMultiplier = 1.5f,
            useEnhancedAI = true,
            expMultiplier = 2f,
            lootMultiplier = 1.8f,
            dodgeProbabilityMultiplier = 2.0f,
            recoverySkillProbabilityMultiplier = 2.0f
        }
    };

    public DifficultyModifiers GetModifiers(GameDifficulty difficulty)
    {
        switch (difficulty)
        {
            case GameDifficulty.Normal:
                return normalConfig.modifiers;
            case GameDifficulty.Hard:
                return hardConfig.modifiers;
            case GameDifficulty.Hell:
                return hellConfig.modifiers;
            default:
                return normalConfig.modifiers;
        }
    }

    public DifficultyConfig GetConfig(GameDifficulty difficulty)
    {
        switch (difficulty)
        {
            case GameDifficulty.Normal:
                return normalConfig;
            case GameDifficulty.Hard:
                return hardConfig;
            case GameDifficulty.Hell:
                return hellConfig;
            default:
                return normalConfig;
        }
    }
}

[System.Serializable]
public class DifficultyConfig
{
    public string difficultyName;
    public DifficultyModifiers modifiers;
}
