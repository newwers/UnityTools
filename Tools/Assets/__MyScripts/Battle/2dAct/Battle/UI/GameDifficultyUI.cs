using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameDifficultyUI : MonoBehaviour
{
    [Header("UI组件")]
    [SerializeField] private TMP_Dropdown difficultyDropdown;
    [SerializeField] private Button applyButton;
    [SerializeField] private TextMeshProUGUI currentDifficultyText;
    [SerializeField] private TextMeshProUGUI difficultyDescriptionText;

    [Header("难度描述")]
    [SerializeField] private string normalDescription = "普通难度：适合新手玩家，敌人属性和AI为标准水平";
    [SerializeField] private string hardDescription = "困难难度：敌人属性全面提升，具有更强的战斗力";
    [SerializeField] private string hellDescription = "地狱难度：敌人属性大幅提升，并使用增强AI策略";

    private void Start()
    {
        if (difficultyDropdown != null)
        {
            difficultyDropdown.ClearOptions();
            difficultyDropdown.AddOptions(new System.Collections.Generic.List<string>
            {
                "普通",
                "困难",
                "地狱"
            });

            difficultyDropdown.onValueChanged.AddListener(OnDifficultyDropdownChanged);
        }

        if (applyButton != null)
        {
            applyButton.onClick.AddListener(OnApplyButtonClicked);
        }

        UpdateCurrentDifficultyDisplay();
    }

    private void OnEnable()
    {
        if (GameDifficultyManager.Instance != null)
        {
            GameDifficultyManager.Instance.OnDifficultyChanged += OnDifficultyChanged;
        }
    }

    private void OnDisable()
    {
        if (GameDifficultyManager.Instance != null)
        {
            GameDifficultyManager.Instance.OnDifficultyChanged -= OnDifficultyChanged;
        }
    }

    private void OnDifficultyDropdownChanged(int index)
    {
        UpdateDifficultyDescription((GameDifficulty)index);
    }

    private void OnApplyButtonClicked()
    {
        if (GameDifficultyManager.Instance == null || difficultyDropdown == null)
            return;

        GameDifficulty selectedDifficulty = (GameDifficulty)difficultyDropdown.value;
        GameDifficultyManager.Instance.SetDifficulty(selectedDifficulty);
    }

    private void OnDifficultyChanged(GameDifficulty newDifficulty)
    {
        UpdateCurrentDifficultyDisplay();
    }

    private void UpdateCurrentDifficultyDisplay()
    {
        if (GameDifficultyManager.Instance == null)
            return;

        if (currentDifficultyText != null)
        {
            currentDifficultyText.text = $"当前难度: {GameDifficultyManager.Instance.GetDifficultyName()}";
        }

        if (difficultyDropdown != null)
        {
            difficultyDropdown.value = (int)GameDifficultyManager.Instance.CurrentDifficulty;
        }

        UpdateDifficultyDescription(GameDifficultyManager.Instance.CurrentDifficulty);
    }

    private void UpdateDifficultyDescription(GameDifficulty difficulty)
    {
        if (difficultyDescriptionText == null)
            return;

        switch (difficulty)
        {
            case GameDifficulty.Normal:
                difficultyDescriptionText.text = normalDescription;
                break;
            case GameDifficulty.Hard:
                difficultyDescriptionText.text = hardDescription;
                break;
            case GameDifficulty.Hell:
                difficultyDescriptionText.text = hellDescription;
                break;
        }
    }

    public void SetDifficultyNormal()
    {
        if (GameDifficultyManager.Instance != null)
        {
            GameDifficultyManager.Instance.SetDifficulty(GameDifficulty.Normal);
        }
    }

    public void SetDifficultyHard()
    {
        if (GameDifficultyManager.Instance != null)
        {
            GameDifficultyManager.Instance.SetDifficulty(GameDifficulty.Hard);
        }
    }

    public void SetDifficultyHell()
    {
        if (GameDifficultyManager.Instance != null)
        {
            GameDifficultyManager.Instance.SetDifficulty(GameDifficulty.Hell);
        }
    }
}
