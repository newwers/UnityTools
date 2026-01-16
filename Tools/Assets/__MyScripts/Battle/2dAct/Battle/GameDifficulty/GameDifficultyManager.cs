using UnityEngine;

public class GameDifficultyManager : BaseMonoSingleClass<GameDifficultyManager>
{
    [Header("难度设置")]
    [SerializeField] private GameDifficultySettings difficultySettings;
    
    [SerializeField] private GameDifficulty currentDifficulty = GameDifficulty.Normal;

    private DifficultyModifiers currentModifiers;

    public GameDifficulty CurrentDifficulty => currentDifficulty;
    public DifficultyModifiers CurrentModifiers => currentModifiers;

    protected override void Awake()
    {
        base.Awake();
        
        if (difficultySettings == null)
        {
            LogManager.LogError("[GameDifficultyManager] 难度设置未配置!");
            return;
        }
        
        UpdateModifiers();
    }

    public void SetDifficulty(GameDifficulty difficulty)
    {
        if (currentDifficulty == difficulty) return;
        
        currentDifficulty = difficulty;
        UpdateModifiers();
        
        LogManager.Log($"[GameDifficultyManager] 游戏难度已设置为: {GetDifficultyName()}");
        
        OnDifficultyChanged?.Invoke(currentDifficulty);
    }

    public string GetDifficultyName()
    {
        if (difficultySettings == null) return "未知";
        return difficultySettings.GetConfig(currentDifficulty).difficultyName;
    }

    public void ApplyDifficultyToAttributes(CharacterAttributes attributes)
    {
        if (attributes == null || currentModifiers == null) return;

        attributes.maxHealth = Mathf.RoundToInt(attributes.maxHealth * currentModifiers.healthMultiplier);
        attributes.currentHealth = attributes.maxHealth;
    }

    public float GetAttackCooldownMultiplier()
    {
        return currentModifiers?.attackSpeedMultiplier ?? 1f;
    }

    public float GetDetectRangeMultiplier()
    {
        return currentModifiers?.detectRangeMultiplier ?? 1f;
    }

    public float GetAIDecisionSpeedMultiplier()
    {
        return currentModifiers?.aiDecisionSpeedMultiplier ?? 1f;
    }

    public bool ShouldUseEnhancedAI()
    {
        return currentModifiers?.useEnhancedAI ?? false;
    }

    public float GetExpMultiplier()
    {
        return currentModifiers?.expMultiplier ?? 1f;
    }

    public float GetLootMultiplier()
    {
        return currentModifiers?.lootMultiplier ?? 1f;
    }

    public float GetDodgeProbabilityMultiplier()
    {
        return currentModifiers?.dodgeProbabilityMultiplier ?? 1f;
    }

    public float GetRecoverySkillProbabilityMultiplier()
    {
        return currentModifiers?.recoverySkillProbabilityMultiplier ?? 1f;
    }

    private void UpdateModifiers()
    {
        if (difficultySettings == null) return;
        
        currentModifiers = difficultySettings.GetModifiers(currentDifficulty);
    }

    public delegate void DifficultyChangedEvent(GameDifficulty newDifficulty);
    public event DifficultyChangedEvent OnDifficultyChanged;
}
